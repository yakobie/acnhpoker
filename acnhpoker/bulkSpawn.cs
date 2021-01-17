using System;
using System.Configuration;
using System.IO;
using System.Net.Sockets;
using System.Windows.Forms;

namespace ACNHPoker
{
    public partial class bulkSpawn : Form
    {
        private Socket s;
        private USBBot bot;
        private miniMap MiniMap = null;
        private map main;
        private int counter = 0;
        private int anchorX = -1;
        private int anchorY = -1;
        private bool sound;
        private byte[] Layer1 = null;
        private byte[] Layer2 = null;
        private byte[] Acre = null;

        private byte[] save = null;

        private byte[][] item = null;
        private int rowNum;
        private byte[][] SpawnArea = null;
        public bulkSpawn(Socket S, USBBot Bot, byte[] layer1, byte[] layer2, byte[] acre, int x, int y, map Map, bool Sound)
        {
            s = S;
            bot = Bot;
            Layer1 = layer1;
            Layer2 = layer2;
            Acre = acre;
            anchorX = x;
            anchorY = y;
            main = Map;
            sound = Sound;
            MiniMap = new miniMap(Layer1, acre, 4);
            InitializeComponent();
            xCoordinate.Text = x.ToString();
            yCoordinate.Text = y.ToString();
            miniMapBox.BackgroundImage = MiniMap.combineMap(MiniMap.drawBackground(), MiniMap.drawItemMap());
        }

        private void ReadBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog()
            {
                Filter = "New Horizons Inventory(*.nhi) | *.nhi|All files (*.*)|*.*",
            };

            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

            string savepath;

            if (config.AppSettings.Settings["LastLoad"].Value.Equals(string.Empty))
                savepath = Directory.GetCurrentDirectory() + @"\save";
            else
                savepath = config.AppSettings.Settings["LastLoad"].Value;

            if (Directory.Exists(savepath))
            {
                file.InitialDirectory = savepath;
            }
            else
            {
                file.InitialDirectory = @"C:\";
            }

            if (file.ShowDialog() != DialogResult.OK)
                return;

            string[] temp = file.FileName.Split('\\');
            string path = "";
            for (int i = 0; i < temp.Length - 1; i++)
                path = path + temp[i] + "\\";

            config.AppSettings.Settings["LastLoad"].Value = path;
            config.Save(ConfigurationSaveMode.Minimal);

            if (save == null)
                save = File.ReadAllBytes(file.FileName);
            else
            {
                byte[] read = File.ReadAllBytes(file.FileName);
                save = Utilities.add(save, read);
            }
        }

        private void createBtn_Click(object sender, EventArgs e)
        {
            if (save == null)
                return;
            else
            {
                SaveFileDialog file = new SaveFileDialog()
                {
                    Filter = "New Horizons Bulk Spawn (*.nhbs)|*.nhbs",
                };

                Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

                string savepath;

                if (config.AppSettings.Settings["LastSave"].Value.Equals(string.Empty))
                    savepath = Directory.GetCurrentDirectory() + @"\save";
                else
                    savepath = config.AppSettings.Settings["LastSave"].Value;

                if (Directory.Exists(savepath))
                {
                    file.InitialDirectory = savepath;
                }
                else
                {
                    file.InitialDirectory = @"C:\";
                }

                if (file.ShowDialog() != DialogResult.OK)
                    return;

                string[] temp = file.FileName.Split('\\');
                string path = "";
                for (int i = 0; i < temp.Length - 1; i++)
                    path = path + temp[i] + "\\";

                config.AppSettings.Settings["LastSave"].Value = path;
                config.Save(ConfigurationSaveMode.Minimal);


                string dataStr = Utilities.ByteToHexString(save).Replace("FEFF000000000000",string.Empty);
                byte[] final = Utilities.stringToByte(dataStr);

                File.WriteAllBytes(file.FileName, final);
                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
            }
        }

        private void selectBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog()
            {
                Filter = "New Horizons Bulk Spawn (*.nhbs)|*.nhbs",
            };

            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

            string savepath;

            if (config.AppSettings.Settings["LastLoad"].Value.Equals(string.Empty))
                savepath = Directory.GetCurrentDirectory() + @"\save";
            else
                savepath = config.AppSettings.Settings["LastLoad"].Value;

            if (Directory.Exists(savepath))
            {
                file.InitialDirectory = savepath;
            }
            else
            {
                file.InitialDirectory = @"C:\";
            }

            if (file.ShowDialog() != DialogResult.OK)
                return;

            string[] temp = file.FileName.Split('\\');
            string path = "";
            for (int i = 0; i < temp.Length - 1; i++)
                path = path + temp[i] + "\\";

            config.AppSettings.Settings["LastLoad"].Value = path;
            config.Save(ConfigurationSaveMode.Minimal);

            byte[] data = File.ReadAllBytes(file.FileName);

            item = processNHBS(data);
            numOfItemBox.Text = item.Length.ToString();
        }

        private byte[][] processNHBS(byte[] data)
        {
            byte[] tempItem = new byte[8];
            bool[] isItem = new bool[data.Length/8];
            int numOfitem = 0;

            for (int i = 0; i < data.Length / 8; i++)
            {
                Buffer.BlockCopy(data, 0x8 * i, tempItem, 0, 8);
                if (!Utilities.ByteToHexString(tempItem).Equals("FEFF000000000000"))
                {
                    isItem[i] = true;
                    numOfitem++;
                }
            }

            byte[][] item = new byte[numOfitem][];
            int itemNum = 0;
            for (int j = 0; j < data.Length / 8; j++)
            {
                if (isItem[j])
                {
                    item[itemNum] = new byte[8];
                    Buffer.BlockCopy(data, 0x8 * j, item[itemNum], 0, 8);
                    itemNum++;
                }
            }

            return item;
        }

        private void previewBtn_Click(object sender, EventArgs e)
        {
            miniMapBox.Image = null;
            if (downNumber.Text.Equals(string.Empty) || item == null)
                return;
            else
            {
                rowNum = int.Parse(downNumber.Text);

                SpawnArea = buildSpawnArea(rowNum);

                bool right;
                if (leftBtn.Checked)
                    right = false;
                else
                    right = true;

                miniMapBox.Image = MiniMap.drawPreview(rowNum, SpawnArea.Length/2, anchorX, anchorY, right);
            }
        }

        private byte[][] buildSpawnArea(int t)
        {
            int itemNum = 0;
            byte[] emptyLeft = Utilities.stringToByte("FEFF000000000000FEFF000000000000");
            byte[] emptyRight = Utilities.stringToByte("FEFF000000000000FEFF000000000000");


            int numberOfColumn = (item.Length / t + 1);
            int sizeOfRow = 16;

            byte[][] b = new byte[numberOfColumn * 2][];

            for (int i = 0; i < numberOfColumn * 2; i++)
            {
                b[i] = new byte[t * sizeOfRow];
            }

            for (int i = 0; i < numberOfColumn; i++)
            {
                for (int j = 0; j < t; j++)
                {
                    if (itemNum < item.Length)
                    {
                        transformToFloorItem(ref b[i * 2], ref b[i * 2 + 1], j, item[itemNum]);
                        itemNum++;
                    }
                    else
                    {
                        Buffer.BlockCopy(emptyLeft, 0, b[i * 2], 0x10 * j, 16);
                        Buffer.BlockCopy(emptyRight, 0, b[i * 2 + 1], 0x10 * j, 16);
                    }
                }
            }


            return b;
        }

        private void transformToFloorItem(ref byte[] b1, ref byte[] b2, int slot, byte[] item)
        {
            byte[] slotBytes = new byte[2];
            byte[] flag1Bytes = new byte[1];
            byte[] flag2Bytes = new byte[1];
            byte[] dataBytes = new byte[4];

            Buffer.BlockCopy(item, 0x0, slotBytes, 0, 2);
            Buffer.BlockCopy(item, 0x3, flag1Bytes, 0, 1);
            Buffer.BlockCopy(item, 0x2, flag2Bytes, 0, 1);
            Buffer.BlockCopy(item, 0x4, dataBytes, 0, 4);

            string itemID = Utilities.flip(Utilities.ByteToHexString(slotBytes));
            string itemData = Utilities.flip(Utilities.ByteToHexString(dataBytes));
            string flag1 = Utilities.ByteToHexString(flag1Bytes);
            string flag2 = "20";

            byte[] dropItemLeft = Utilities.stringToByte(Utilities.buildDropStringLeft(itemID, itemData, flag1, flag2));
            byte[] dropItemRight = Utilities.stringToByte(Utilities.buildDropStringRight(itemID));

            Buffer.BlockCopy(dropItemLeft, 0, b1, slot * 0x10, 16);
            Buffer.BlockCopy(dropItemRight, 0, b2, slot * 0x10, 16);

        }

        private void bulkSpawn_FormClosed(object sender, FormClosedEventArgs e)
        {
            main.bulk = null;
        }

        private void spawnBtn_Click(object sender, EventArgs e)
        {
            if (leftBtn.Checked)
            {

            }
            else
            {
                for (int i = 0; i < SpawnArea.Length / 2; i++)
                {
                    UInt32 address = (UInt32)(Utilities.mapZero + (0xC00 * (anchorX + i)) + (0x10 * (anchorY)));

                    Utilities.dropColume(s, bot, address, address + 0x600, SpawnArea[i * 2], SpawnArea[i * 2 + 1], ref counter);
                    Utilities.dropColume(s, bot, address + Utilities.mapOffset, address + 0x600 + Utilities.mapOffset, SpawnArea[i * 2], SpawnArea[i * 2 + 1], ref counter);
                }

                main.updataData(anchorX, anchorY, SpawnArea, false);
            }
        }
    }
}
