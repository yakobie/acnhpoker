using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;

namespace ACNHPoker
{
    public partial class map : Form
    {
        private static Socket s;
        private USBBot bot;
        private DataTable source;
        private DataTable recipeSource;
        private DataTable flowerSource;
        private DataTable variationSource;
        private DataTable favSource;
        private DataTable fieldSource;
        private floorSlot selectedButton = null;
        private floorSlot[] floorSlots;
        private Form1 main;
        private int counter = 0;

        private DataGridViewRow lastRow = null;
        private string imagePath;

        private const string csvFolder = @"csv\";
        private const string fieldFile = @"field.csv";
        private const string fieldPath = csvFolder + fieldFile;

        private Dictionary<string, string> OverrideDict;

        private int anchorX = -1;
        private int anchorY = -1;

        private DataTable currentDataTable;

        public map(Socket S, USBBot Bot, string itemPath, string recipePath, string flowerPath, string variationPath, string favPath, Form1 Main, string ImagePath, Dictionary<string, string> overrideDict)
        {
            try
            {
                s = S;
                bot = Bot;
                if (File.Exists(itemPath))
                    source = loadItemCSV(itemPath);
                if (File.Exists(recipePath))
                    recipeSource = loadItemCSV(recipePath);
                if (File.Exists(flowerPath))
                    flowerSource = loadItemCSV(flowerPath);
                if (File.Exists(variationPath))
                    variationSource = loadItemCSV(variationPath);
                if (File.Exists(favPath))
                    favSource = loadItemCSV(favPath);
                if (File.Exists(fieldPath))
                    fieldSource = loadItemCSV(fieldPath);
                main = Main;
                imagePath = ImagePath;
                OverrideDict = overrideDict;

                floorSlots = new floorSlot[49];


                InitializeComponent();

                foreach (floorSlot btn in BtnPanel.Controls.OfType<floorSlot>())
                {
                    int i = int.Parse(btn.Tag.ToString());
                    floorSlots[i] = btn;
                }

                if (source != null)
                {
                    fieldGridView.DataSource = source;

                    //set the ID row invisible
                    fieldGridView.Columns["id"].Visible = false;
                    fieldGridView.Columns["iName"].Visible = false;
                    fieldGridView.Columns["jpn"].Visible = false;
                    fieldGridView.Columns["tchi"].Visible = false;
                    fieldGridView.Columns["schi"].Visible = false;
                    fieldGridView.Columns["kor"].Visible = false;
                    fieldGridView.Columns["fre"].Visible = false;
                    fieldGridView.Columns["ger"].Visible = false;
                    fieldGridView.Columns["spa"].Visible = false;
                    fieldGridView.Columns["ita"].Visible = false;
                    fieldGridView.Columns["dut"].Visible = false;
                    fieldGridView.Columns["rus"].Visible = false;
                    fieldGridView.Columns["color"].Visible = false;

                    //select the full row and change color cause windows blue sux
                    fieldGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                    fieldGridView.DefaultCellStyle.BackColor = Color.FromArgb(255, 47, 49, 54);
                    fieldGridView.DefaultCellStyle.ForeColor = Color.FromArgb(255, 114, 105, 110);
                    fieldGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 57, 60, 67);

                    fieldGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 57, 60, 67);
                    fieldGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                    fieldGridView.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 57, 60, 67);

                    fieldGridView.EnableHeadersVisualStyles = false;

                    //create the image column
                    DataGridViewImageColumn imageColumn = new DataGridViewImageColumn
                    {
                        Name = "Image",
                        HeaderText = "Image",
                        ImageLayout = DataGridViewImageCellLayout.Zoom
                    };
                    fieldGridView.Columns.Insert(13, imageColumn);
                    imageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

                    fieldGridView.Columns["eng"].Width = 195;
                    fieldGridView.Columns["jpn"].Width = 195;
                    fieldGridView.Columns["tchi"].Width = 195;
                    fieldGridView.Columns["schi"].Width = 195;
                    fieldGridView.Columns["kor"].Width = 195;
                    fieldGridView.Columns["fre"].Width = 195;
                    fieldGridView.Columns["ger"].Width = 195;
                    fieldGridView.Columns["spa"].Width = 195;
                    fieldGridView.Columns["ita"].Width = 195;
                    fieldGridView.Columns["dut"].Width = 195;
                    fieldGridView.Columns["rus"].Width = 195;
                    fieldGridView.Columns["Image"].Width = 128;

                    fieldGridView.Columns["eng"].HeaderText = "Name";
                    fieldGridView.Columns["jpn"].HeaderText = "Name";
                    fieldGridView.Columns["tchi"].HeaderText = "Name";
                    fieldGridView.Columns["schi"].HeaderText = "Name";
                    fieldGridView.Columns["kor"].HeaderText = "Name";
                    fieldGridView.Columns["fre"].HeaderText = "Name";
                    fieldGridView.Columns["ger"].HeaderText = "Name";
                    fieldGridView.Columns["spa"].HeaderText = "Name";
                    fieldGridView.Columns["ita"].HeaderText = "Name";
                    fieldGridView.Columns["dut"].HeaderText = "Name";
                    fieldGridView.Columns["rus"].HeaderText = "Name";

                    currentDataTable = source;
                }

                this.BringToFront();
                this.Focus();
                this.KeyPreview = true;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void castAnchorBtn_Click(object sender, EventArgs e)
        {
            btnToolTip.RemoveAll();

            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            BtnPanel.Enabled = true;

            byte[] b = Utilities.peekAddress(s, bot, Utilities.coordinate, 8);
            int x = BitConverter.ToInt32(b, 0);
            int y = BitConverter.ToInt32(b, 4);
            xCoordinate.Text = x.ToString("X");
            yCoordinate.Text = y.ToString("X");

            anchorX = x;
            anchorY = y;

            UInt32 address = (UInt32)(Utilities.mapHead + (0xC00 * (x - 3)) + (0x10 * (y - 3)));
            displayAnchor(address);

            moveRightBtn.Enabled = true;
            moveLeftBtn.Enabled = true;
            moveUpBtn.Enabled = true;
            moveDownBtn.Enabled = true;
            moveUpRightBtn.Enabled = true;
            moveUpLeftBtn.Enabled = true;
            moveDownRightBtn.Enabled = true;
            moveDownLeftBtn.Enabled = true;
            moveRight7Btn.Enabled = true;
            moveLeft7Btn.Enabled = true;
            moveUp7Btn.Enabled = true;
            moveDown7Btn.Enabled = true;
        }

        private void moveAnchor(int x, int y)
        {
            btnToolTip.RemoveAll();

            xCoordinate.Text = x.ToString("X");
            yCoordinate.Text = y.ToString("X");

            anchorX = x;
            anchorY = y;

            UInt32 address = (UInt32)(Utilities.mapHead + (0xC00 * (x - 3)) + (0x10 * (y - 3)));
            displayAnchor(address);

            if (selectedButton != null)
                selectedButton.BackColor = System.Drawing.Color.LightSeaGreen;
        }

        private void displayAnchor(UInt32 address)
        {
            byte[] b = Utilities.ReadByteArray8(s, address, 0x4E70);

            byte[] colume1Left = new byte[0x70];
            byte[] colume1Right = new byte[0x70];
            byte[] colume2Left = new byte[0x70];
            byte[] colume2Right = new byte[0x70];
            byte[] colume3Left = new byte[0x70];
            byte[] colume3Right = new byte[0x70];
            byte[] colume4Left = new byte[0x70];
            byte[] colume4Right = new byte[0x70];
            byte[] colume5Left = new byte[0x70];
            byte[] colume5Right = new byte[0x70];
            byte[] colume6Left = new byte[0x70];
            byte[] colume6Right = new byte[0x70];
            byte[] colume7Left = new byte[0x70];
            byte[] colume7Right = new byte[0x70];

            Buffer.BlockCopy(b, 0x0, colume1Left, 0x0, 0x70);
            Buffer.BlockCopy(b, 0x600, colume1Right, 0x0, 0x70);
            Buffer.BlockCopy(b, 0xC00, colume2Left, 0x0, 0x70);
            Buffer.BlockCopy(b, 0x1200, colume2Right, 0x0, 0x70);
            Buffer.BlockCopy(b, 0x1800, colume3Left, 0x0, 0x70);
            Buffer.BlockCopy(b, 0x1E00, colume3Right, 0x0, 0x70);
            Buffer.BlockCopy(b, 0x2400, colume4Left, 0x0, 0x70);
            Buffer.BlockCopy(b, 0x2a00, colume4Right, 0x0, 0x70);
            Buffer.BlockCopy(b, 0x3000, colume5Left, 0x0, 0x70);
            Buffer.BlockCopy(b, 0x3600, colume5Right, 0x0, 0x70);
            Buffer.BlockCopy(b, 0x3C00, colume6Left, 0x0, 0x70);
            Buffer.BlockCopy(b, 0x4200, colume6Right, 0x0, 0x70);
            Buffer.BlockCopy(b, 0x4800, colume7Left, 0x0, 0x70);
            Buffer.BlockCopy(b, 0x4E00, colume7Right, 0x0, 0x70);

            BtnSetup(colume1Left, colume1Right, (anchorX - 3), (anchorY - 3), floor1, floor2, floor3, floor4, floor5, floor6, floor7, 0, false);
            BtnSetup(colume2Left, colume2Right, (anchorX - 2), (anchorY - 3), floor8, floor9, floor10, floor11, floor12, floor13, floor14, 1, false);
            BtnSetup(colume3Left, colume3Right, (anchorX - 1), (anchorY - 3), floor15, floor16, floor17, floor18, floor19, floor20, floor21, 2, false);
            BtnSetup(colume4Left, colume4Right, (anchorX - 0), (anchorY - 3), floor22, floor23, floor24, floor25, floor26, floor27, floor28, 3, true);
            BtnSetup(colume5Left, colume5Right, (anchorX + 1), (anchorY - 3), floor29, floor30, floor31, floor32, floor33, floor34, floor35, 4, false);
            BtnSetup(colume6Left, colume6Right, (anchorX + 2), (anchorY - 3), floor36, floor37, floor38, floor39, floor40, floor41, floor42, 5, false);
            BtnSetup(colume7Left, colume7Right, (anchorX + 3), (anchorY - 3), floor43, floor44, floor45, floor46, floor47, floor48, floor49, 6, false);

            if (selectedButton != null)
            {
                selectedButton.BackColor = System.Drawing.Color.LightSeaGreen;
            }
        }

        private void BtnSetup(byte[] b, byte[] b2, int x, int y, floorSlot slot1, floorSlot slot2, floorSlot slot3, floorSlot slot4, floorSlot slot5, floorSlot slot6, floorSlot slot7, int colume, Boolean anchor = false)
        {
            byte[] idBytes = new byte[2];
            byte[] flag1Bytes = new byte[1];
            byte[] flag2Bytes = new byte[1];
            byte[] dataBytes = new byte[4];

            byte[] part2IdBytes = new byte[4];
            byte[] part2DataBytes = new byte[4];
            byte[] part3IdBytes = new byte[4];
            byte[] part3DataBytes = new byte[4];
            byte[] part4IdBytes = new byte[4];
            byte[] part4DataBytes = new byte[4];

            byte[] idFull = new byte[4];

            floorSlot currentBtn = null;

            for (int i = 0; i < 7; i++)
            {
                Buffer.BlockCopy(b, (i * 0x10) + 0x0, idBytes, 0x0, 0x2);
                Buffer.BlockCopy(b, (i * 0x10) + 0x2, flag2Bytes, 0x0, 0x1);
                Buffer.BlockCopy(b, (i * 0x10) + 0x3, flag1Bytes, 0x0, 0x1);
                Buffer.BlockCopy(b, (i * 0x10) + 0x4, dataBytes, 0x0, 0x4);

                Buffer.BlockCopy(b, (i * 0x10) + 0x8, part2IdBytes, 0x0, 0x4);
                Buffer.BlockCopy(b, (i * 0x10) + 0xC, part2DataBytes, 0x0, 0x4);
                Buffer.BlockCopy(b2, (i * 0x10) + 0x0, part3IdBytes, 0x0, 0x4);
                Buffer.BlockCopy(b2, (i * 0x10) + 0x4, part3DataBytes, 0x0, 0x4);
                Buffer.BlockCopy(b2, (i * 0x10) + 0x8, part4IdBytes, 0x0, 0x4);
                Buffer.BlockCopy(b2, (i * 0x10) + 0xC, part4DataBytes, 0x0, 0x4);

                string itemID = Utilities.flip(Utilities.ByteToHexString(idBytes));
                string flag2 = Utilities.ByteToHexString(flag2Bytes);
                string flag1 = Utilities.ByteToHexString(flag1Bytes);
                string itemData = Utilities.flip(Utilities.ByteToHexString(dataBytes));

                string part2Id = Utilities.flip(Utilities.ByteToHexString(part2IdBytes));
                string part2Data = Utilities.flip(Utilities.ByteToHexString(part2DataBytes));
                string part3Id = Utilities.flip(Utilities.ByteToHexString(part3IdBytes));
                string part3Data = Utilities.flip(Utilities.ByteToHexString(part3DataBytes));
                string part4Id = Utilities.flip(Utilities.ByteToHexString(part4IdBytes));
                string part4Data = Utilities.flip(Utilities.ByteToHexString(part4DataBytes));

                if (i == 0)
                {
                    currentBtn = slot1;
                    setBtn(slot1, itemID, itemData, part2Id, part2Data, part3Id, part3Data, part4Id, part4Data, flag1, flag2);
                }
                else if (i == 1)
                {
                    currentBtn = slot2;
                    setBtn(slot2, itemID, itemData, part2Id, part2Data, part3Id, part3Data, part4Id, part4Data, flag1, flag2);
                }
                else if (i == 2)
                {
                    currentBtn = slot3;
                    setBtn(slot3, itemID, itemData, part2Id, part2Data, part3Id, part3Data, part4Id, part4Data, flag1, flag2);
                }
                else if (i == 3)
                {
                    currentBtn = slot4;
                    setBtn(slot4, itemID, itemData, part2Id, part2Data, part3Id, part3Data, part4Id, part4Data, flag1, flag2);
                    if (anchor)
                    {
                        //slot4.BackColor = Color.Red;
                    }
                }
                else if (i == 4)
                {
                    currentBtn = slot5;
                    setBtn(slot5, itemID, itemData, part2Id, part2Data, part3Id, part3Data, part4Id, part4Data, flag1, flag2);
                }
                else if (i == 5)
                {
                    currentBtn = slot6;
                    setBtn(slot6, itemID, itemData, part2Id, part2Data, part3Id, part3Data, part4Id, part4Data, flag1, flag2);
                }
                else if (i == 6)
                {
                    currentBtn = slot7;
                    setBtn(slot7, itemID, itemData, part2Id, part2Data, part3Id, part3Data, part4Id, part4Data, flag1, flag2);
                }

                currentBtn.mapX = x;
                currentBtn.mapY = y + i;
            }
        }

        private void setBtn(floorSlot btn, string itemID, string itemData, string part2Id, string part2Data, string part3Id, string part3Data, string part4Id, string part4Data, string flag1, string flag2)
        {
            string Name = GetNameFromID(itemID, source);
            UInt16 ID = Convert.ToUInt16("0x" + itemID, 16);
            UInt32 Data = Convert.ToUInt32("0x" + itemData, 16);
            UInt32 IntP2Id = Convert.ToUInt32("0x" + part2Id, 16);
            UInt32 IntP2Data = Convert.ToUInt32("0x" + part2Data, 16);
            UInt32 IntP3Id = Convert.ToUInt32("0x" + part3Id, 16);
            UInt32 IntP3Data = Convert.ToUInt32("0x" + part3Data, 16);
            UInt32 IntP4Id = Convert.ToUInt32("0x" + part4Id, 16);
            UInt32 IntP4Data = Convert.ToUInt32("0x" + part4Data, 16);

            string P1Id = itemID;
            string P2Id = Utilities.turn2bytes(part2Id);
            string P3Id = Utilities.turn2bytes(part3Id);
            string P4Id = Utilities.turn2bytes(part4Id);

            string P1Data = Utilities.turn2bytes(itemData);
            string P2Data = Utilities.turn2bytes(part2Data);
            string P3Data = Utilities.turn2bytes(part3Data);
            string P4Data = Utilities.turn2bytes(part4Data);

            string Path1;
            string Path2;
            string Path3;
            string Path4;

            if (P1Id == "FFFD")
                Path1 = GetImagePathFromID(P1Data, source);
            else if (P1Id == "16A2")
            {
                Path1 = GetImagePathFromID(P1Data, recipeSource, Data);
                Name = GetNameFromID(P1Data, recipeSource);
            }
            else
                Path1 = GetImagePathFromID(itemID, source, Data);

            if (P2Id == "FFFD")
                Path2 = GetImagePathFromID(P2Data, source);
            else
                Path2 = GetImagePathFromID(P2Id, source, IntP2Data);

            if (P3Id == "FFFD")
                Path3 = GetImagePathFromID(P3Data, source);
            else
                Path3 = GetImagePathFromID(P3Id, source, IntP3Data);

            if (P4Id == "FFFD")
                Path4 = GetImagePathFromID(P4Data, source);
            else
                Path4 = GetImagePathFromID(P4Id, source, IntP4Data);

            btn.setup(Name, ID, Data, IntP2Id, IntP2Data, IntP3Id, IntP3Data, IntP4Id, IntP4Data, Path1, Path2, Path3, Path4, "", flag1, flag2);
        }

        private UInt32 getAddress(int x, int y)
        {
            return (UInt32)(Utilities.mapHead + (0xC00 * x) + (0x10 * (y)));
        }

        private void moveRightBtn_Click(object sender, EventArgs e)
        {
            int newX = anchorX + 1;
            int newY = anchorY;

            if (newX < 0x27 || newY < 0x1B  || newX > 0x90 || newY > 0x74)
            {
                System.Media.SystemSounds.Asterisk.Play();
                return;
            }

            moveAnchor(newX, newY);
        }

        private void moveLeftBtn_Click(object sender, EventArgs e)
        {
            int newX = anchorX - 1;
            int newY = anchorY;

            if (newX < 0x27 || newY < 0x1B  || newX > 0x90 || newY > 0x74)
            {
                System.Media.SystemSounds.Asterisk.Play();
                return;
            }

            moveAnchor(newX, newY);
        }

        private void moveDownBtn_Click(object sender, EventArgs e)
        {
            int newX = anchorX;
            int newY = anchorY + 1;

            if (newX < 0x27 || newY < 0x1B  || newX > 0x90 || newY > 0x74)
            {
                System.Media.SystemSounds.Asterisk.Play();
                return;
            }

            moveAnchor(newX, newY);
        }

        private void moveUpBtn_Click(object sender, EventArgs e)
        {
            int newX = anchorX;
            int newY = anchorY - 1;

            if (newX < 0x27 || newY < 0x1B  || newX > 0x90 || newY > 0x74)
            {
                System.Media.SystemSounds.Asterisk.Play();
                return;
            }

            moveAnchor(newX, newY);
        }

        private void moveUpRightBtn_Click(object sender, EventArgs e)
        {
            int newX = anchorX + 1;
            int newY = anchorY - 1;

            if (newX < 0x27 || newY < 0x1B  || newX > 0x90 || newY > 0x74)
            {
                System.Media.SystemSounds.Asterisk.Play();
                return;
            }

            moveAnchor(newX, newY);
        }

        private void moveDownRightBtn_Click(object sender, EventArgs e)
        {
            int newX = anchorX + 1;
            int newY = anchorY + 1;

            if (newX < 0x27 || newY < 0x1B  || newX > 0x90 || newY > 0x74)
            {
                System.Media.SystemSounds.Asterisk.Play();
                return;
            }

            moveAnchor(newX, newY);
        }

        private void moveDownLeftBtn_Click(object sender, EventArgs e)
        {
            int newX = anchorX - 1;
            int newY = anchorY + 1;

            if (newX < 0x27 || newY < 0x1B  || newX > 0x90 || newY > 0x74)
            {
                System.Media.SystemSounds.Asterisk.Play();
                return;
            }

            moveAnchor(newX, newY);
        }

        private void moveUpLeftBtn_Click(object sender, EventArgs e)
        {
            int newX = anchorX - 1;
            int newY = anchorY - 1;

            if (newX < 0x27 || newY < 0x1B  || newX > 0x90 || newY > 0x74)
            {
                System.Media.SystemSounds.Asterisk.Play();
                return;
            }

            moveAnchor(newX, newY);
        }

        private void moveUp7Btn_Click(object sender, EventArgs e)
        {
            if (anchorY <= 0x1B)
            {
                System.Media.SystemSounds.Asterisk.Play();
                return;
            }

            int newX = anchorX;
            int newY = anchorY - 7;

            if (newY < 0x1B)
            {
                newY = 0x1B;
                System.Media.SystemSounds.Asterisk.Play();
            }

            moveAnchor(newX, newY);
        }

        private void moveRight7Btn_Click(object sender, EventArgs e)
        {
            if (anchorX >= 0x90)
            {
                System.Media.SystemSounds.Asterisk.Play();
                return;
            }

            int newX = anchorX + 7;
            int newY = anchorY;

            if (newX > 0x90)
            {
                newX = 0x90;
                System.Media.SystemSounds.Asterisk.Play();
            }

            moveAnchor(newX, newY);
        }

        private void moveDown7Btn_Click(object sender, EventArgs e)
        {
            if (anchorY >= 0x74)
            {
                System.Media.SystemSounds.Asterisk.Play();
                return;
            }

            int newX = anchorX;
            int newY = anchorY + 7;

            if (newY > 0x74)
            {
                newY = 0x74;
                System.Media.SystemSounds.Asterisk.Play();
            }

            moveAnchor(newX, newY);
        }

        private void moveLeft7Btn_Click(object sender, EventArgs e)
        {
            if (anchorY <= 0x27)
            {
                System.Media.SystemSounds.Asterisk.Play();
                return;
            }

            int newX = anchorX - 7;
            int newY = anchorY;

            if (newX < 0x27)
            {
                newX = 0x27;
                System.Media.SystemSounds.Asterisk.Play();
            }

            moveAnchor(newX, newY);
        }

        private void floor_MouseHover(object sender, EventArgs e)
        {
            var button = (floorSlot)sender;
            //if (!button.isEmpty())
            //{
                /*
                if (button.getContainItemName() != "")
                {
                    btnToolTip.SetToolTip(button, button.displayItemName() + "\n\nID : " + button.displayItemID() + "\nCount : " + button.displayItemData() + "\nFlag : 0x" + button.getFlag1() + button.getFlag2() + "\nContain Item : " + button.getContainItemName());
                }
                else
                {
                */
                btnToolTip.SetToolTip(button, 
                                        button.itemName + 
                                        "\n\n" + "" +
                                        "ID : " + Utilities.precedingZeros(button.itemID.ToString("X"), 4) + "\n" +
                                        "Count : " + Utilities.precedingZeros(button.itemData.ToString("X"), 8) + "\n" +
                                        "Flag1 : 0x" + button.flag1 + "\n" +
                                        "Flag2 : 0x" + button.flag2 + "\n" +
                                        "Coordinate : " + button.mapX + " " + button.mapY + "\n\n" +
                                        "Part2 : " + button.part2.ToString("X") + " " + Utilities.precedingZeros(button.part2Data.ToString("X"), 8) + "\n" +
                                        "Part3 : " + button.part3.ToString("X") + " " + Utilities.precedingZeros(button.part3Data.ToString("X"), 8) + "\n" +
                                        "Part4 : " + button.part4.ToString("X") + " " + Utilities.precedingZeros(button.part4Data.ToString("X"), 8) + "\n" +
                                        "Locked : " + button.locked.ToString()
                                        );
                //}
            //}
        }

        private string removeNumber(string filename)
        {
            char[] MyChar = { '0', '1', '2', '3', '4' };
            return filename.Trim(MyChar);
        }

        public string GetImagePathFromID(string itemID, DataTable source, UInt32 data = 0)
        {
            if (source == null)
            {
                return "";
            }

            if (fieldSource != null)
            {
                string path;

                DataRow FieldRow = fieldSource.Rows.Find(itemID);
                if (FieldRow != null)
                {
                    string imageName = FieldRow[1].ToString();

                    if (OverrideDict.ContainsKey(imageName))
                    {
                        path = imagePath + OverrideDict[imageName] + ".png";
                        if (File.Exists(path))
                        {
                            return path;
                        }
                    }
                }

            }

            DataRow row = source.Rows.Find(itemID);
            DataRow VarRow = null;
            if (variationSource != null)
                VarRow = variationSource.Rows.Find(itemID);

            if (row == null)
            {
                return ""; //row not found
            }
            else
            {

                string path;
                if (VarRow != null & source != recipeSource)
                {
                    path = imagePath + VarRow["iName"] + ".png";
                    if (File.Exists(path))
                    {
                        return path;
                    }
                    string main = (data & 0xF).ToString();
                    string sub = (((data & 0xFF) - (data & 0xF)) / 0x20).ToString();
                    //Debug.Print("data " + data.ToString("X") + " Main " + main + " Sub " + sub);
                    path = imagePath + VarRow["iName"] + "_Remake_" + main + "_" + sub + ".png";
                    if (File.Exists(path))
                    {
                        return path;
                    }

                }

                string imageName = row[1].ToString();

                if (OverrideDict.ContainsKey(imageName))
                {
                    path = imagePath + OverrideDict[imageName] + ".png";
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }

                path = imagePath + imageName + ".png";
                if (File.Exists(path))
                {
                    return path;
                }
                else
                {
                    path = imagePath + imageName + "_Remake_0_0.png";
                    if (File.Exists(path))
                    {
                        return path;
                    }
                    else
                    {
                        path = imagePath + removeNumber(imageName) + ".png";
                        if (File.Exists(path))
                        {
                            return path;
                        }
                        else
                        {
                            return "";
                        }
                    }
                }
            }
        }

        public string GetNameFromID(string itemID, DataTable table)
        {
            if (fieldSource != null)
            {
                DataRow FieldRow = fieldSource.Rows.Find(itemID);
                if (FieldRow != null)
                {
                    return (string)FieldRow["name"];
                }
            }

            if (table == null)
            {
                return "";
            }

            DataRow row = table.Rows.Find(itemID);

            if (row == null)
            {
                return ""; //row not found
            }
            else
            {
                //row found set the index and find the name
                return (string)row["eng"];
            }
        }

        private void fieldGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            AddImage(fieldGridView, e);
        }

        private void AddImage(DataGridView Grid, DataGridViewCellFormattingEventArgs e)
        {
            if (Grid.Columns["Image"] == null)
                return;
            if (e.RowIndex >= 0 && e.RowIndex < Grid.Rows.Count)
            {
                if (e.ColumnIndex == Grid.Columns["Image"].Index)
                {
                    string path;
                    string imageName = Grid.Rows[e.RowIndex].Cells["iName"].Value.ToString();

                    if (OverrideDict.ContainsKey(imageName))
                    {
                        path = imagePath + OverrideDict[imageName] + ".png";
                        if (File.Exists(path))
                        {
                            Image img = Image.FromFile(path);
                            //e.CellStyle.BackColor = Color.Green;
                            e.Value = img;

                            return;
                        }
                    }

                    path = imagePath + imageName + ".png";
                    if (File.Exists(path))
                    {
                        Image img = Image.FromFile(path);
                        e.Value = img;
                    }
                    else
                    {
                        path = imagePath + imageName + "_Remake_0_0.png";
                        if (File.Exists(path))
                        {
                            Image img = Image.FromFile(path);
                            e.CellStyle.BackColor = Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(77)))), ((int)(((byte)(162)))));
                            e.Value = img;
                        }
                        else
                        {
                            path = imagePath + removeNumber(imageName) + ".png";
                            if (File.Exists(path))
                            {
                                Image img = Image.FromFile(path);
                                e.Value = img;
                            }
                            else
                            {
                                e.CellStyle.BackColor = Color.Red;
                            }
                        }
                    }
                }
            }
        }

        private void fieldGridView_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (lastRow != null)
                {
                    lastRow.Height = 22;
                }

                if (e.RowIndex > -1)
                {
                    lastRow = fieldGridView.Rows[e.RowIndex];
                    fieldGridView.Rows[e.RowIndex].Height = 128;

                    if (currentDataTable == source)
                    {
                        string id = fieldGridView.Rows[e.RowIndex].Cells["id"].Value.ToString();
                        string name = fieldGridView.Rows[e.RowIndex].Cells["eng"].Value.ToString();

                        IdTextbox.Text = id;
                        HexTextbox.Text = "00000000";

                        selectedItem.setup(name, Convert.ToUInt16("0x" + id, 16), 0x0, GetImagePathFromID(id, source), true, "");
                    }
                    else if (currentDataTable == recipeSource)
                    {
                        string id = "16A2"; // Recipe;
                        string name = fieldGridView.Rows[e.RowIndex].Cells["eng"].Value.ToString();
                        string hexValue = fieldGridView.Rows[e.RowIndex].Cells["id"].Value.ToString();

                        IdTextbox.Text = id;
                        HexTextbox.Text = Utilities.precedingZeros(hexValue, 8);

                        selectedItem.setup(name, Convert.ToUInt16("0x" + id, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(hexValue, recipeSource), true, "");
                    }
                    else if (currentDataTable == flowerSource)
                    {
                        string id = fieldGridView.Rows[e.RowIndex].Cells["id"].Value.ToString();
                        string name = fieldGridView.Rows[e.RowIndex].Cells["eng"].Value.ToString();
                        string hexValue = fieldGridView.Rows[e.RowIndex].Cells["value"].Value.ToString();

                        IdTextbox.Text = id;
                        HexTextbox.Text = Utilities.precedingZeros(hexValue, 8);

                        selectedItem.setup(name, Convert.ToUInt16("0x" + id, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(id, source), true, "");

                    }
                    else if(currentDataTable == favSource)
                    {
                        string id = fieldGridView.Rows[e.RowIndex].Cells["id"].Value.ToString();
                        string name = fieldGridView.Rows[e.RowIndex].Cells["Name"].Value.ToString();
                        string hexValue = fieldGridView.Rows[e.RowIndex].Cells["value"].Value.ToString();

                        IdTextbox.Text = id;
                        HexTextbox.Text = Utilities.precedingZeros(hexValue, 8);

                        selectedItem.setup(name, Convert.ToUInt16("0x" + id, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(id, source), true, "");
                    }
                    else if (currentDataTable == fieldSource)
                    {
                        string id = fieldGridView.Rows[e.RowIndex].Cells["id"].Value.ToString();
                        string name = fieldGridView.Rows[e.RowIndex].Cells["Name"].Value.ToString();
                        string hexValue = fieldGridView.Rows[e.RowIndex].Cells["value"].Value.ToString();

                        IdTextbox.Text = id;
                        HexTextbox.Text = Utilities.precedingZeros(hexValue, 8);

                        selectedItem.setup(name, Convert.ToUInt16("0x" + id, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(id, fieldSource), true, "");
                    }
                    /*
                    if (selection != null)
                    {
                        selection.receiveID(Utilities.precedingZeros(selectedItem.fillItemID(), 4), languageSetting);
                    }
                    */
                    //updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());

                }
            }
        }

        private void itemModeBtn_Click(object sender, EventArgs e)
        {
            itemModeBtn.BackColor = Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            recipeModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            flowerModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            favModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            fieldModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            fieldGridView.Columns.Remove("Image");

            if (source != null)
            {
                fieldGridView.DataSource = source;

                //set the ID row invisible
                fieldGridView.Columns["id"].Visible = false;
                fieldGridView.Columns["iName"].Visible = false;
                fieldGridView.Columns["jpn"].Visible = false;
                fieldGridView.Columns["tchi"].Visible = false;
                fieldGridView.Columns["schi"].Visible = false;
                fieldGridView.Columns["kor"].Visible = false;
                fieldGridView.Columns["fre"].Visible = false;
                fieldGridView.Columns["ger"].Visible = false;
                fieldGridView.Columns["spa"].Visible = false;
                fieldGridView.Columns["ita"].Visible = false;
                fieldGridView.Columns["dut"].Visible = false;
                fieldGridView.Columns["rus"].Visible = false;
                fieldGridView.Columns["color"].Visible = false;

                DataGridViewImageColumn imageColumn = new DataGridViewImageColumn
                {
                    Name = "Image",
                    HeaderText = "Image",
                    ImageLayout = DataGridViewImageCellLayout.Zoom
                };
                fieldGridView.Columns.Insert(13, imageColumn);
                imageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

                fieldGridView.Columns["eng"].Width = 195;
                fieldGridView.Columns["jpn"].Width = 195;
                fieldGridView.Columns["tchi"].Width = 195;
                fieldGridView.Columns["schi"].Width = 195;
                fieldGridView.Columns["kor"].Width = 195;
                fieldGridView.Columns["fre"].Width = 195;
                fieldGridView.Columns["ger"].Width = 195;
                fieldGridView.Columns["spa"].Width = 195;
                fieldGridView.Columns["ita"].Width = 195;
                fieldGridView.Columns["dut"].Width = 195;
                fieldGridView.Columns["rus"].Width = 195;
                fieldGridView.Columns["Image"].Width = 128;

                fieldGridView.Columns["eng"].HeaderText = "Name";
                fieldGridView.Columns["jpn"].HeaderText = "Name";
                fieldGridView.Columns["tchi"].HeaderText = "Name";
                fieldGridView.Columns["schi"].HeaderText = "Name";
                fieldGridView.Columns["kor"].HeaderText = "Name";
                fieldGridView.Columns["fre"].HeaderText = "Name";
                fieldGridView.Columns["ger"].HeaderText = "Name";
                fieldGridView.Columns["spa"].HeaderText = "Name";
                fieldGridView.Columns["ita"].HeaderText = "Name";
                fieldGridView.Columns["dut"].HeaderText = "Name";
                fieldGridView.Columns["rus"].HeaderText = "Name";

                currentDataTable = source;
            }

            FlagTextbox.Text = "20";

            if (itemSearchBox.Text != "Search")
            {
                itemSearchBox.Clear();
            }
        }

        private void recipeModeBtn_Click(object sender, EventArgs e)
        {
            itemModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            recipeModeBtn.BackColor = Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            flowerModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            favModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            fieldModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            fieldGridView.Columns.Remove("Image");

            if (recipeSource != null)
            {
                fieldGridView.DataSource = recipeSource;

                fieldGridView.Columns["id"].Visible = false;
                fieldGridView.Columns["iName"].Visible = false;
                fieldGridView.Columns["jpn"].Visible = false;
                fieldGridView.Columns["tchi"].Visible = false;
                fieldGridView.Columns["schi"].Visible = false;
                fieldGridView.Columns["kor"].Visible = false;
                fieldGridView.Columns["fre"].Visible = false;
                fieldGridView.Columns["ger"].Visible = false;
                fieldGridView.Columns["spa"].Visible = false;
                fieldGridView.Columns["ita"].Visible = false;
                fieldGridView.Columns["dut"].Visible = false;
                fieldGridView.Columns["rus"].Visible = false;

                DataGridViewImageColumn imageColumn = new DataGridViewImageColumn
                {
                    Name = "Image",
                    HeaderText = "Image",
                    ImageLayout = DataGridViewImageCellLayout.Zoom
                };
                fieldGridView.Columns.Insert(13, imageColumn);
                imageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

                fieldGridView.Columns["eng"].Width = 195;
                fieldGridView.Columns["jpn"].Width = 195;
                fieldGridView.Columns["tchi"].Width = 195;
                fieldGridView.Columns["schi"].Width = 195;
                fieldGridView.Columns["kor"].Width = 195;
                fieldGridView.Columns["fre"].Width = 195;
                fieldGridView.Columns["ger"].Width = 195;
                fieldGridView.Columns["spa"].Width = 195;
                fieldGridView.Columns["ita"].Width = 195;
                fieldGridView.Columns["dut"].Width = 195;
                fieldGridView.Columns["rus"].Width = 195;
                fieldGridView.Columns["Image"].Width = 128;

                fieldGridView.Columns["eng"].HeaderText = "Name";
                fieldGridView.Columns["jpn"].HeaderText = "Name";
                fieldGridView.Columns["tchi"].HeaderText = "Name";
                fieldGridView.Columns["schi"].HeaderText = "Name";
                fieldGridView.Columns["kor"].HeaderText = "Name";
                fieldGridView.Columns["fre"].HeaderText = "Name";
                fieldGridView.Columns["ger"].HeaderText = "Name";
                fieldGridView.Columns["spa"].HeaderText = "Name";
                fieldGridView.Columns["ita"].HeaderText = "Name";
                fieldGridView.Columns["dut"].HeaderText = "Name";
                fieldGridView.Columns["rus"].HeaderText = "Name";

                currentDataTable = recipeSource;
            }

            FlagTextbox.Text = "00";

            if (itemSearchBox.Text != "Search")
            {
                itemSearchBox.Clear();
            }
        }

        private void flowerModeBtn_Click(object sender, EventArgs e)
        {
            itemModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            recipeModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            flowerModeBtn.BackColor = Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            favModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            fieldModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            fieldGridView.Columns.Remove("Image");

            if (flowerSource != null)
            {
                fieldGridView.DataSource = flowerSource;

                fieldGridView.Columns["id"].Visible = false;
                fieldGridView.Columns["iName"].Visible = false;
                fieldGridView.Columns["jpn"].Visible = false;
                fieldGridView.Columns["tchi"].Visible = false;
                fieldGridView.Columns["schi"].Visible = false;
                fieldGridView.Columns["kor"].Visible = false;
                fieldGridView.Columns["fre"].Visible = false;
                fieldGridView.Columns["ger"].Visible = false;
                fieldGridView.Columns["spa"].Visible = false;
                fieldGridView.Columns["ita"].Visible = false;
                fieldGridView.Columns["dut"].Visible = false;
                fieldGridView.Columns["rus"].Visible = false;
                fieldGridView.Columns["value"].Visible = false;

                DataGridViewImageColumn imageColumn = new DataGridViewImageColumn
                {
                    Name = "Image",
                    HeaderText = "Image",
                    ImageLayout = DataGridViewImageCellLayout.Zoom
                };
                fieldGridView.Columns.Insert(13, imageColumn);
                imageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

                fieldGridView.Columns["eng"].Width = 195;
                fieldGridView.Columns["jpn"].Width = 195;
                fieldGridView.Columns["tchi"].Width = 195;
                fieldGridView.Columns["schi"].Width = 195;
                fieldGridView.Columns["kor"].Width = 195;
                fieldGridView.Columns["fre"].Width = 195;
                fieldGridView.Columns["ger"].Width = 195;
                fieldGridView.Columns["spa"].Width = 195;
                fieldGridView.Columns["ita"].Width = 195;
                fieldGridView.Columns["dut"].Width = 195;
                fieldGridView.Columns["rus"].Width = 195;
                fieldGridView.Columns["Image"].Width = 128;

                fieldGridView.Columns["eng"].HeaderText = "Name";
                fieldGridView.Columns["jpn"].HeaderText = "Name";
                fieldGridView.Columns["tchi"].HeaderText = "Name";
                fieldGridView.Columns["schi"].HeaderText = "Name";
                fieldGridView.Columns["kor"].HeaderText = "Name";
                fieldGridView.Columns["fre"].HeaderText = "Name";
                fieldGridView.Columns["ger"].HeaderText = "Name";
                fieldGridView.Columns["spa"].HeaderText = "Name";
                fieldGridView.Columns["ita"].HeaderText = "Name";
                fieldGridView.Columns["dut"].HeaderText = "Name";
                fieldGridView.Columns["rus"].HeaderText = "Name";

                currentDataTable = flowerSource;
            }

            FlagTextbox.Text = "20";

            if (itemSearchBox.Text != "Search")
            {
                itemSearchBox.Clear();
            }
        }

        private void favModeBtn_Click(object sender, EventArgs e)
        {
            itemModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            recipeModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            flowerModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            favModeBtn.BackColor = Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            fieldModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            fieldGridView.Columns.Remove("Image");

            if (favSource != null)
            {
                fieldGridView.DataSource = favSource;

                fieldGridView.Columns["id"].Visible = false;
                fieldGridView.Columns["iName"].Visible = false;
                fieldGridView.Columns["Name"].Visible = true;
                fieldGridView.Columns["value"].Visible = false;

                DataGridViewImageColumn imageColumn = new DataGridViewImageColumn
                {
                    Name = "Image",
                    HeaderText = "Image",
                    ImageLayout = DataGridViewImageCellLayout.Zoom
                };
                fieldGridView.Columns.Insert(4, imageColumn);
                imageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

                fieldGridView.Columns["Name"].Width = 195;
                fieldGridView.Columns["Image"].Width = 128;

                currentDataTable = favSource;
            }

            FlagTextbox.Text = "20";

            if (itemSearchBox.Text != "Search")
            {
                itemSearchBox.Clear();
            }
        }

        private void fieldModeBtn_Click(object sender, EventArgs e)
        {
            itemModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            recipeModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            flowerModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            favModeBtn.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            fieldModeBtn.BackColor = Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));

            fieldGridView.Columns.Remove("Image");

            if (favSource != null)
            {
                fieldGridView.DataSource = fieldSource;

                fieldGridView.Columns["id"].Visible = false;
                fieldGridView.Columns["iName"].Visible = false;
                fieldGridView.Columns["name"].Visible = true;
                fieldGridView.Columns["value"].Visible = false;

                DataGridViewImageColumn imageColumn = new DataGridViewImageColumn
                {
                    Name = "Image",
                    HeaderText = "Image",
                    ImageLayout = DataGridViewImageCellLayout.Zoom
                };
                fieldGridView.Columns.Insert(3, imageColumn);
                imageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

                fieldGridView.Columns["name"].Width = 195;
                fieldGridView.Columns["Image"].Width = 128;

                fieldGridView.Columns["name"].HeaderText = "Name";

                currentDataTable = fieldSource;
            }

            FlagTextbox.Text = "00";

            if (itemSearchBox.Text != "Search")
            {
                itemSearchBox.Clear();
            }
        }

        private void floor_MouseDown(object sender, MouseEventArgs e)
        {
            var button = (floorSlot)sender;

            foreach (floorSlot btn in BtnPanel.Controls.OfType<floorSlot>())
            {
                if (btn.locked)
                    btn.BackColor = Color.Gray;
                else
                    btn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            }

            button.FlatStyle = FlatStyle.Flat;
            button.BackColor = System.Drawing.Color.LightSeaGreen;
            selectedButton = button;
        }

        private void selectedItem_Click(object sender, EventArgs e)
        {
            if (selectedButton == null)
            {
                MessageBox.Show("Please select a slot!");
                return;
            }
            if (IdTextbox.Text == "" || HexTextbox.Text == "" || FlagTextbox.Text == "")
            {
                return;
            }

            string address1 = getAddress(selectedButton.mapX, selectedButton.mapY).ToString("X");
            string address2 = (getAddress(selectedButton.mapX, selectedButton.mapY) + 0x600).ToString("X");
            string address3 = (getAddress(selectedButton.mapX, selectedButton.mapY) + Utilities.mapOffset).ToString("X");
            string address4 = ((getAddress(selectedButton.mapX, selectedButton.mapY) + 0x600) + Utilities.mapOffset).ToString("X");
            string itemID = Utilities.precedingZeros(IdTextbox.Text, 4);
            string count = Utilities.precedingZeros(HexTextbox.Text, 8);
            string flag2 = Utilities.precedingZeros(FlagTextbox.Text, 2);

            Utilities.dropItem(s, bot, address1, address2, address3, address4, itemID, count, "00", flag2);
            setBtn(selectedButton, itemID, count, "0000FFFD", "0100" + itemID, "0000FFFD", "0001" + itemID, "0000FFFD", "0101" + itemID, "00", flag2);
            selectedButton.BackColor = System.Drawing.Color.LightSeaGreen;
            System.Media.SystemSounds.Asterisk.Play();

        }

        private DataTable loadItemCSV(string filePath)
        {
            var dt = new DataTable();

            File.ReadLines(filePath).Take(1)
                .SelectMany(x => x.Split(new[] { " ; " }, StringSplitOptions.RemoveEmptyEntries))
                .ToList()
                .ForEach(x => dt.Columns.Add(x.Trim()));

            File.ReadLines(filePath).Skip(1)
                .Select(x => x.Split(new[] { " ; " }, StringSplitOptions.RemoveEmptyEntries))
                .ToList()
                .ForEach(line => dt.Rows.Add(line));

            if (dt.Columns.Contains("id"))
                dt.PrimaryKey = new DataColumn[1] { dt.Columns["id"] };

            return dt;
        }

        private void itemSearchBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (fieldGridView.DataSource != null)
                {
                    if (currentDataTable == source)
                    {
                        (fieldGridView.DataSource as DataTable).DefaultView.RowFilter = string.Format("eng" + " LIKE '%{0}%'", itemSearchBox.Text);
                    }
                    else if (currentDataTable == recipeSource)
                    {
                        (fieldGridView.DataSource as DataTable).DefaultView.RowFilter = string.Format("eng" + " LIKE '%{0}%'", itemSearchBox.Text);
                    }
                    else if (currentDataTable == flowerSource)
                    {
                        (fieldGridView.DataSource as DataTable).DefaultView.RowFilter = string.Format("eng" + " LIKE '%{0}%'", itemSearchBox.Text);
                    }
                    else if (currentDataTable == favSource)
                    {
                        (fieldGridView.DataSource as DataTable).DefaultView.RowFilter = string.Format("name" + " LIKE '%{0}%'", itemSearchBox.Text);
                    }
                    else if (currentDataTable == fieldSource)
                    {
                        (fieldGridView.DataSource as DataTable).DefaultView.RowFilter = string.Format("name" + " LIKE '%{0}%'", itemSearchBox.Text);
                    }
                }
            }
            catch
            {
                itemSearchBox.Clear();
            }
        }

        private void itemSearchBox_Click(object sender, EventArgs e)
        {
            if (itemSearchBox.Text == "Search")
            {
                itemSearchBox.Text = "";
                itemSearchBox.ForeColor = Color.White;
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem item = (sender as ToolStripItem);
            if (item != null)
            {
                if (item.Owner is ContextMenuStrip owner)
                {
                    var btn = (floorSlot)owner.SourceControl;
                    string address1 = getAddress(btn.mapX, btn.mapY).ToString("X");
                    string address2 = (getAddress(btn.mapX, btn.mapY) + 0x600).ToString("X");
                    string address3 = (getAddress(btn.mapX, btn.mapY) + Utilities.mapOffset).ToString("X");
                    string address4 = (getAddress(btn.mapX, btn.mapY) + 0x600 + +Utilities.mapOffset).ToString("X");

                    Utilities.deleteFloorItem(s, bot, address1, address2, address3, address4);

                    btn.reset();
                    btnToolTip.RemoveAll();
                    System.Media.SystemSounds.Asterisk.Play();
                }
            }
        }

        private void copyItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem item = (sender as ToolStripItem);
            if (item != null)
            {
                if (item.Owner is ContextMenuStrip owner)
                {
                    var btn = (floorSlot)owner.SourceControl;
                    string id = Utilities.precedingZeros(btn.itemID.ToString("X"), 4);
                    string name = btn.Name;
                    string hexValue = Utilities.precedingZeros(btn.itemData.ToString("X"), 8);
                    string flag1 = btn.flag1;
                    string flag2 = btn.flag2;

                    IdTextbox.Text = id;
                    HexTextbox.Text = hexValue;
                    FlagTextbox.Text = flag2;

                    if (id == "16A2")
                        selectedItem.setup(name, Convert.ToUInt16("0x" + id, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(Utilities.turn2bytes(hexValue), recipeSource), true, "", flag1, flag2);
                    else
                        selectedItem.setup(name, Convert.ToUInt16("0x" + id, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(id, source), true, "", flag1, flag2);
                    System.Media.SystemSounds.Asterisk.Play();
                }
            }
        }

        private void Hex_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
            {
                e.Handled = true;
            }
            if (c >= 'a' && c <= 'f') e.KeyChar = char.ToUpper(c);
        }

        private void refreshBtn_Click(object sender, EventArgs e)
        {
            if (anchorX < 0 || anchorY < 0)
                return;

            int x = anchorX;
            int y = anchorY;

            UInt32 address = (UInt32)(Utilities.mapHead + (0xC00 * (x - 3)) + (0x10 * (y - 3)));
            displayAnchor(address);
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void fillRemainBtn_Click(object sender, EventArgs e)
        {
            if (anchorX < 0 || anchorY < 0)
                return;

            if (IdTextbox.Text == "" || HexTextbox.Text == "" || FlagTextbox.Text == "")
            {
                return;
            }

            refreshBtn_Click(sender, e);

            string itemID = Utilities.precedingZeros(IdTextbox.Text, 4);
            string count = Utilities.precedingZeros(HexTextbox.Text, 8);
            string flag2 = Utilities.precedingZeros(FlagTextbox.Text, 2);

            Thread fillRemainThread = new Thread(delegate () { fillRemain(itemID, count, flag2); });
            fillRemainThread.Start();

            System.Media.SystemSounds.Asterisk.Play();


        }

        private void fillRemain(string itemID, string count, string flag2)
        {
            for (int i = 0; i < 49; i++)
            {
                if (floorSlots[i].isEmpty())
                {
                    string address1 = getAddress(floorSlots[i].mapX, floorSlots[i].mapY).ToString("X"); //Top left
                    string address2 = (getAddress(floorSlots[i].mapX, floorSlots[i].mapY) + 0x600).ToString("X");
                    string address3 = (getAddress(floorSlots[i].mapX, floorSlots[i].mapY) + Utilities.mapOffset).ToString("X"); //Top left
                    string address4 = ((getAddress(floorSlots[i].mapX, floorSlots[i].mapY) + 0x600) + Utilities.mapOffset).ToString("X");

                    Utilities.dropItem(s, bot, address1, address2, address3, address4, itemID, count, "00", flag2);
                    setBtn(floorSlots[i], itemID, count, "0000FFFD", "0100" + itemID, "0000FFFD", "0001" + itemID, "0000FFFD", "0101" + itemID, "00", flag2);
                }
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (anchorX < 0 || anchorY < 0)
                    return;
                SaveFileDialog file = new SaveFileDialog()
                {
                    Filter = "New Horizons Grid (*.nhg)|*.nhg",
                    //FileName = "items.nhi",
                };

                string savepath = Directory.GetCurrentDirectory() + @"\save";
                //Debug.Print(savepath);
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

                string Bank = "";

                UInt32[] address = new UInt32[7];

                address[0] = (UInt32)(Utilities.mapHead + (0xC00 * (anchorX - 3)) + (0x10 * (anchorY - 3)));
                address[1] = (UInt32)(Utilities.mapHead + (0xC00 * (anchorX - 2)) + (0x10 * (anchorY - 3)));
                address[2] = (UInt32)(Utilities.mapHead + (0xC00 * (anchorX - 1)) + (0x10 * (anchorY - 3)));
                address[3] = (UInt32)(Utilities.mapHead + (0xC00 * (anchorX - 0)) + (0x10 * (anchorY - 3)));
                address[4] = (UInt32)(Utilities.mapHead + (0xC00 * (anchorX + 1)) + (0x10 * (anchorY - 3)));
                address[5] = (UInt32)(Utilities.mapHead + (0xC00 * (anchorX + 2)) + (0x10 * (anchorY - 3)));
                address[6] = (UInt32)(Utilities.mapHead + (0xC00 * (anchorX + 3)) + (0x10 * (anchorY - 3)));
                /*if (!offline)
                {*/
                for (int i = 0; i < 7; i++)
                {
                    byte[] b = Utilities.peekAddress(s, bot, address[i], 112);
                    byte[] b2 = Utilities.peekAddress(s, bot, address[i] + 0x600, 112);
                    Bank += Utilities.ByteToHexString(b) + Utilities.ByteToHexString(b2);
                }


                byte[] save = new byte[1568];

                for (int i = 0; i < Bank.Length / 2 - 1; i++)
                {
                    string data = String.Concat(Bank[(i * 2)].ToString(), Bank[((i * 2) + 1)].ToString());
                    //Debug.Print(i.ToString() + " " + data);
                    save[i] = Convert.ToByte(data, 16);
                }

                File.WriteAllBytes(file.FileName, save);
                System.Media.SystemSounds.Asterisk.Play();
                /*}
                
                else
                {
                    inventorySlot[] SlotPointer = new inventorySlot[40];
                    foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                    {
                        int slotId = int.Parse(btn.Tag.ToString());
                        SlotPointer[slotId - 1] = btn;
                    }
                    for (int i = 0; i < SlotPointer.Length; i++)
                    {
                        string first = Utilities.flip(Utilities.precedingZeros(SlotPointer[i].getFlag1() + SlotPointer[i].getFlag2() + Utilities.precedingZeros(SlotPointer[i].fillItemID(), 4), 8));
                        string second = Utilities.flip(Utilities.precedingZeros(SlotPointer[i].fillItemData(), 8));
                        //Debug.Print(first + " " + second + " " + SlotPointer[i].getFlag1() + " " + SlotPointer[i].getFlag2() + " " + SlotPointer[i].fillItemID());
                        Bank = Bank + first + second;
                    }

                    byte[] save = new byte[320];

                    for (int i = 0; i < Bank.Length / 2 - 1; i++)
                    {
                        string data = String.Concat(Bank[(i * 2)].ToString(), Bank[((i * 2) + 1)].ToString());
                        //Debug.Print(i.ToString() + " " + data);
                        save[i] = Convert.ToByte(data, 16);
                    }

                    File.WriteAllBytes(file.FileName, save);
                }
                //Debug.Print(Bank);

                System.Media.SystemSounds.Asterisk.Play();
                */
            }
            catch
            {
                if (s != null)
                {
                    s.Close();
                }
                return;
            }
        }

        private void loadBtn_Click(object sender, EventArgs e)
        {
            try
            {
                if (anchorX < 0 || anchorY < 0)
                    return;
                OpenFileDialog file = new OpenFileDialog()
                {
                    Filter = "New Horizons Grid (*.nhg)|*.nhg|All files (*.*)|*.*",
                    FileName = "Grid.nhg",
                };

                string savepath = Directory.GetCurrentDirectory() + @"\save";
                //Debug.Print(savepath);
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

                byte[] data = File.ReadAllBytes(file.FileName);

                btnToolTip.RemoveAll();
                /*
                string bank = "";
                for (int i = 0; i < data.Length; i++)
                {
                    bank += Utilities.precedingZeros(((UInt16)data[i]).ToString("X"), 2);
                }
                */
                //Debug.Print(bank);
                Thread LoadThread = new Thread(delegate () { loadFloor(data); });
                LoadThread.Start();
            }
            catch
            {
                if (s != null)
                {
                    s.Close();
                }
                return;
            }
        }

        private void loadFloor(byte[] data)
        {
            byte[][] b = new byte[14][];

            for (int i = 0; i < 14; i++)
            {
                b[i] = new byte[112];
                for (int j = 0; j < 112; j++)
                {
                    b[i][j] = data[j + 112 * i];
                }
            }

            UInt32 address1 = (UInt32)(Utilities.mapHead + (0xC00 * (anchorX - 3)) + (0x10 * (anchorY - 3)));
            UInt32 address2 = (UInt32)(Utilities.mapHead + (0xC00 * (anchorX - 2)) + (0x10 * (anchorY - 3)));
            UInt32 address3 = (UInt32)(Utilities.mapHead + (0xC00 * (anchorX - 1)) + (0x10 * (anchorY - 3)));
            UInt32 address4 = (UInt32)(Utilities.mapHead + (0xC00 * (anchorX - 0)) + (0x10 * (anchorY - 3)));
            UInt32 address5 = (UInt32)(Utilities.mapHead + (0xC00 * (anchorX + 1)) + (0x10 * (anchorY - 3)));
            UInt32 address6 = (UInt32)(Utilities.mapHead + (0xC00 * (anchorX + 2)) + (0x10 * (anchorY - 3)));
            UInt32 address7 = (UInt32)(Utilities.mapHead + (0xC00 * (anchorX + 3)) + (0x10 * (anchorY - 3)));

            Utilities.dropColume(s, bot, address1, address1 + 0x600, b[0], b[1], ref counter);
            Utilities.dropColume(s, bot, address2, address2 + 0x600, b[2], b[3], ref counter);
            Utilities.dropColume(s, bot, address3, address3 + 0x600, b[4], b[5], ref counter);
            Utilities.dropColume(s, bot, address4, address4 + 0x600, b[6], b[7], ref counter);
            Utilities.dropColume(s, bot, address5, address5 + 0x600, b[8], b[9], ref counter);
            Utilities.dropColume(s, bot, address6, address6 + 0x600, b[10], b[11], ref counter);
            Utilities.dropColume(s, bot, address7, address7 + 0x600, b[12], b[13], ref counter);

            Utilities.dropColume(s, bot, address1 + Utilities.mapOffset, address1 + 0x600 + Utilities.mapOffset, b[0], b[1], ref counter);
            Utilities.dropColume(s, bot, address2 + Utilities.mapOffset, address2 + 0x600 + Utilities.mapOffset, b[2], b[3], ref counter);
            Utilities.dropColume(s, bot, address3 + Utilities.mapOffset, address3 + 0x600 + Utilities.mapOffset, b[4], b[5], ref counter);
            Utilities.dropColume(s, bot, address4 + Utilities.mapOffset, address4 + 0x600 + Utilities.mapOffset, b[6], b[7], ref counter);
            Utilities.dropColume(s, bot, address5 + Utilities.mapOffset, address5 + 0x600 + Utilities.mapOffset, b[8], b[9], ref counter);
            Utilities.dropColume(s, bot, address6 + Utilities.mapOffset, address6 + 0x600 + Utilities.mapOffset, b[10], b[11], ref counter);
            Utilities.dropColume(s, bot, address7 + Utilities.mapOffset, address7 + 0x600 + Utilities.mapOffset, b[12], b[13], ref counter);

            BtnSetup(b[0], b[1], anchorX - 3, anchorY - 3, floor1, floor2, floor3, floor4, floor5, floor6, floor7, 0, false);
            BtnSetup(b[2], b[3], anchorX - 2, anchorY - 3, floor8, floor9, floor10, floor11, floor12, floor13, floor14, 0, false);
            BtnSetup(b[4], b[5], anchorX - 1, anchorY - 3, floor15, floor16, floor17, floor18, floor19, floor20, floor21, 0, false);
            BtnSetup(b[6], b[7], anchorX - 0, anchorY - 3, floor22, floor23, floor24, floor25, floor26, floor27, floor28, 0, false);
            BtnSetup(b[8], b[9], anchorX + 1, anchorY - 3, floor29, floor30, floor31, floor32, floor33, floor34, floor35, 0, false);
            BtnSetup(b[10], b[11], anchorX + 2, anchorY - 3, floor36, floor37, floor38, floor39, floor40, floor41, floor42, 0, false);
            BtnSetup(b[12], b[13], anchorX + 3, anchorY - 3, floor43, floor44, floor45, floor46, floor47, floor48, floor49, 0, false);

            System.Media.SystemSounds.Asterisk.Play();
        }

        private void deleteItem(floorSlot btn)
        {
            string address1 = getAddress(btn.mapX, btn.mapY).ToString("X");
            string address2 = (getAddress(btn.mapX, btn.mapY) + 0x600).ToString("X");
            string address3 = (getAddress(btn.mapX, btn.mapY) + Utilities.mapOffset).ToString("X");
            string address4 = (getAddress(btn.mapX, btn.mapY) + 0x600 + +Utilities.mapOffset).ToString("X");

            Utilities.deleteFloorItem(s, bot, address1, address2, address3, address4);

            btn.reset();
            btnToolTip.RemoveAll();
        }

        private void copyItem(floorSlot btn)
        {
            string id = Utilities.precedingZeros(btn.itemID.ToString("X"), 4);
            string name = btn.Name;
            string hexValue = Utilities.precedingZeros(btn.itemData.ToString("X"), 8);
            string flag1 = btn.flag1;
            string flag2 = btn.flag2;

            IdTextbox.Text = id;
            HexTextbox.Text = hexValue;
            FlagTextbox.Text = flag2;

            if (id == "16A2")
                selectedItem.setup(name, Convert.ToUInt16("0x" + id, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(Utilities.turn2bytes(hexValue), recipeSource), true, "", flag1, flag2);
            else
                selectedItem.setup(name, Convert.ToUInt16("0x" + id, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(id, source), true, "", flag1, flag2);
        }

        private void KeyboardKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.ToString() == "F2" || e.KeyCode.ToString() == "Insert")
            {
                if (selectedButton != null & (s != null || bot != null))
                {
                    selectedItem_Click(sender, e);
                }
                System.Media.SystemSounds.Asterisk.Play();
            }
            else if (e.KeyCode.ToString() == "F1") // Delete
            {
                if (selectedButton != null & (s != null || bot != null))
                {
                    deleteItem(selectedButton);
                }
                System.Media.SystemSounds.Asterisk.Play();
            }
            else if (e.KeyCode.ToString() == "F3") // Copy
            {
                if (selectedButton != null & (s != null || bot != null))
                {
                    copyItem(selectedButton);
                }
                System.Media.SystemSounds.Asterisk.Play();
            }
            else if (e.KeyCode.ToString() == "End")
            {
                if (fieldGridView.Rows.Count <= 0)
                {
                    return;
                }
                else if (fieldGridView.Rows.Count == 1)
                {
                    lastRow = fieldGridView.Rows[fieldGridView.CurrentRow.Index];
                    fieldGridView.Rows[fieldGridView.CurrentRow.Index].Height = 160;

                    KeyPressSetup(fieldGridView.CurrentRow.Index);
                }
                else if (fieldGridView.CurrentRow.Index + 1 < fieldGridView.Rows.Count)
                {
                    if (lastRow != null)
                    {
                        lastRow.Height = 22;
                    }
                    lastRow = fieldGridView.Rows[fieldGridView.CurrentRow.Index + 1];
                    fieldGridView.Rows[fieldGridView.CurrentRow.Index + 1].Height = 160;

                    KeyPressSetup(fieldGridView.CurrentRow.Index + 1);
                    fieldGridView.CurrentCell = fieldGridView.Rows[fieldGridView.CurrentRow.Index + 1].Cells[fieldGridView.CurrentCell.ColumnIndex];
                }

            }
            else if (e.KeyCode.ToString() == "Home")
            {
                if (fieldGridView.Rows.Count <= 0)
                {
                    return;
                }
                else if (fieldGridView.Rows.Count == 1)
                {
                    lastRow = fieldGridView.Rows[fieldGridView.CurrentRow.Index];
                    fieldGridView.Rows[fieldGridView.CurrentRow.Index].Height = 160;

                    KeyPressSetup(fieldGridView.CurrentRow.Index);
                }
                else if (fieldGridView.CurrentRow.Index > 0)
                {
                    if (lastRow != null)
                    {
                        lastRow.Height = 22;
                    }

                    lastRow = fieldGridView.Rows[fieldGridView.CurrentRow.Index - 1];
                    fieldGridView.Rows[fieldGridView.CurrentRow.Index - 1].Height = 160;

                    KeyPressSetup(fieldGridView.CurrentRow.Index - 1);
                    fieldGridView.CurrentCell = fieldGridView.Rows[fieldGridView.CurrentRow.Index - 1].Cells[fieldGridView.CurrentCell.ColumnIndex];
                }
            }
        }

        private void KeyPressSetup(int index)
        {
            if (currentDataTable == source)
            {
                string id = fieldGridView.Rows[index].Cells["id"].Value.ToString();
                string name = fieldGridView.Rows[index].Cells["eng"].Value.ToString();

                IdTextbox.Text = id;
                HexTextbox.Text = "00000000";

                selectedItem.setup(name, Convert.ToUInt16("0x" + id, 16), 0x0, GetImagePathFromID(id, source), true, "");
            }
            else if (currentDataTable == recipeSource)
            {
                string id = "16A2"; // Recipe;
                string name = fieldGridView.Rows[index].Cells["eng"].Value.ToString();
                string hexValue = fieldGridView.Rows[index].Cells["id"].Value.ToString();

                IdTextbox.Text = id;
                HexTextbox.Text = Utilities.precedingZeros(hexValue, 8);

                selectedItem.setup(name, Convert.ToUInt16("0x" + id, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(hexValue, recipeSource), true, "");
            }
            else if (currentDataTable == flowerSource)
            {
                string id = fieldGridView.Rows[index].Cells["id"].Value.ToString();
                string name = fieldGridView.Rows[index].Cells["eng"].Value.ToString();
                string hexValue = fieldGridView.Rows[index].Cells["value"].Value.ToString();

                IdTextbox.Text = id;
                HexTextbox.Text = Utilities.precedingZeros(hexValue, 8);

                selectedItem.setup(name, Convert.ToUInt16("0x" + id, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(id, source), true, "");

            }
            else if (currentDataTable == favSource)
            {
                string id = fieldGridView.Rows[index].Cells["id"].Value.ToString();
                string name = fieldGridView.Rows[index].Cells["Name"].Value.ToString();
                string hexValue = fieldGridView.Rows[index].Cells["value"].Value.ToString();

                IdTextbox.Text = id;
                HexTextbox.Text = Utilities.precedingZeros(hexValue, 8);

                selectedItem.setup(name, Convert.ToUInt16("0x" + id, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(id, source), true, "");
            }
            else if (currentDataTable == fieldSource)
            {
                string id = fieldGridView.Rows[index].Cells["id"].Value.ToString();
                string name = fieldGridView.Rows[index].Cells["Name"].Value.ToString();
                string hexValue = fieldGridView.Rows[index].Cells["value"].Value.ToString();

                IdTextbox.Text = id;
                HexTextbox.Text = Utilities.precedingZeros(hexValue, 8);

                selectedItem.setup(name, Convert.ToUInt16("0x" + id, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(id, fieldSource), true, "");
            }
        }

        private void map_FormClosed(object sender, FormClosedEventArgs e)
        {
            main.Map = null;
        }
    }
}
