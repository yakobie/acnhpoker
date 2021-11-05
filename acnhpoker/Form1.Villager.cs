using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace ACNHPoker
{


    public partial class Form1 : Form
    {
        private void villagerBtn_Click(object sender, EventArgs e)
        {
            this.inventoryBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.critterBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.otherBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.villagerBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));

            inventoryLargePanel.Visible = false;
            otherLargePanel.Visible = false;
            critterLargePanel.Visible = false;
            villagerLargePanel.Visible = true;
            closeVariationMenu();

            int player = 0;

            if (V == null && !firstload)
            {
                firstload = true;
                Thread LoadAllVillagerThread = new Thread(delegate () { loadAllVillager(player); });
                LoadAllVillagerThread.Start();
            }
        }

        private void loadAllVillager(int player)
        {
            lock (villagerLock)
            {
                if (bot == null)
                    showVillagerWait(25000, "Acquiring villager data...");
                else
                    showVillagerWait(15000, "Acquiring villager data...");

                if ((s == null || s.Connected == false) & bot == null)
                    return;

                blocker = true;
                selectedVillagerButton = null;

                HouseList = new int[10];

                for (int i = 0; i < 10; i++)
                {
                    byte b = Utilities.GetHouseOwner(s, bot, i, ref counter);
                    if (b == 0xDD)
                    {
                        hideVillagerWait();
                        return;
                    }
                    HouseList[i] = Convert.ToInt32(b);
                }
                Debug.Print(string.Join(" ", HouseList));


                V = new Villager[10];
                villagerButton = new Button[10];

                for (int i = 0; i < 10; i++)
                {
                    byte[] b = Utilities.GetVillager(s, bot, i, (int)(Utilities.VillagerMemoryTinySize), ref counter);
                    V[i] = new Villager(b, i)
                    {
                        HouseIndex = Utilities.FindHouseIndex(i, HouseList)
                    };

                    byte f = Utilities.GetVillagerHouseFlag(s, bot, V[i].HouseIndex, 0x8, ref counter);
                    V[i].MoveInFlag = Convert.ToInt32(f);

                    byte[] move = Utilities.GetMoveout(s, bot, i, (int)0x33, ref counter);
                    V[i].AbandonedHouseFlag = Convert.ToInt32(move[0]);
                    V[i].InvitedFlag = Convert.ToInt32(move[0x14]);
                    V[i].ForceMoveOutFlag = Convert.ToInt32(move[move.Length - 1]);
                    byte[] catchphrase = Utilities.GetCatchphrase(s, bot, i, ref counter);
                    V[i].catchphrase = catchphrase;

                    villagerButton[i] = new Button();
                    villagerButton[i].TextAlign = System.Drawing.ContentAlignment.TopCenter;
                    villagerButton[i].ForeColor = System.Drawing.Color.White;
                    villagerButton[i].Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
                    int friendship = V[i].Friendship[player];
                    villagerButton[i].BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(friendship)))), ((int)(((byte)(friendship / 2)))), ((int)(((byte)(friendship)))));
                    villagerButton[i].FlatAppearance.BorderSize = 0;
                    villagerButton[i].FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                    if (i < 5)
                        villagerButton[i].Location = new System.Drawing.Point((i * 128) + (i * 10) + 50, 54);
                    else
                        villagerButton[i].Location = new System.Drawing.Point(((i - 5) * 128) + ((i - 5) * 10) + 50, 192);
                    villagerButton[i].Name = "villagerBtn" + i.ToString();
                    villagerButton[i].Tag = i;
                    villagerButton[i].Size = new System.Drawing.Size(128, 128);
                    villagerButton[i].UseVisualStyleBackColor = false;
                    Image img;
                    if (V[i].GetRealName() == "ERROR")
                    {
                        string path = Utilities.GetVillagerImage(V[i].GetRealName());
                        if (!path.Equals(string.Empty))
                            img = Image.FromFile(path);
                        else
                            img = new Bitmap(Properties.Resources.Leaf, new Size(110, 110));
                    }
                    else
                    {
                        string path = Utilities.GetVillagerImage(V[i].GetInternalName());
                        if (!path.Equals(string.Empty))
                            img = Image.FromFile(path);
                        else
                            img = new Bitmap(Properties.Resources.Leaf, new Size(110, 110));
                    }

                    villagerButton[i].Text = V[i].GetRealName() + " : " + V[i].GetInternalName();

                    if (V[i].MoveInFlag == 0xC || V[i].MoveInFlag == 0xB)
                    {
                        if (V[i].IsReal())
                        {
                            if (V[i].IsInvited())
                                villagerButton[i].Text += "\n(Tom Nook Invited)";
                            else
                                villagerButton[i].Text += "\n(Moving In)";
                        }
                        else
                            villagerButton[i].Text += "\n(Just Move Out)";
                    }
                    else if (V[i].InvitedFlag == 0x2)
                        villagerButton[i].Text += "\n(Invited by Visitor)";
                    else if (V[i].AbandonedHouseFlag == 0x1 && V[i].ForceMoveOutFlag == 0x0)
                        villagerButton[i].Text += "\n(Floor Sweeping)";
                    else if (V[i].AbandonedHouseFlag == 0x2 && V[i].ForceMoveOutFlag == 0x1)
                        villagerButton[i].Text += "\n(Moving Out 1)";
                    else if (V[i].AbandonedHouseFlag == 0x2 && V[i].ForceMoveOutFlag == 0x0)
                        villagerButton[i].Text += "\n(Moving Out 2)";

                    villagerButton[i].Image = (Image)(new Bitmap(img, new Size(110, 110)));
                    villagerButton[i].ImageAlign = ContentAlignment.BottomCenter;
                    villagerButton[i].MouseDown += new System.Windows.Forms.MouseEventHandler(this.VillagerButton_MouseDown);
                }


                this.Invoke((MethodInvoker)delegate
                {
                    for (int i = 0; i < 10; i++)
                    {
                        this.villagerLargePanel.Controls.Add(villagerButton[i]);
                        villagerButton[i].BringToFront();
                        //overlay.BringToFront();
                    }

                    IndexValue.Text = "";
                    NameValue.Text = "";
                    InternalNameValue.Text = "";
                    PersonalityValue.Text = "";
                    FriendShipValue.Text = "";
                    HouseIndexValue.Text = "";
                    MoveInFlag.Text = "";
                    MoveOutValue.Text = "";
                    ForceMoveOutValue.Text = "";
                    CatchphraseValue.Text = "";
                    FullAddress.Text = "";

                    RefreshVillagerBtn.Enabled = true;
                });

                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();


                blocker = false;

                hideVillagerWait();

            }
        }

        private void RefreshVillagerBtn_Click(object sender, EventArgs e)
        {
            if ((s == null || s.Connected == false) & bot == null)
            {
                return;
            }

            if (villagerButton != null)
            {
                for (int i = 0; i < 10; i++)
                    this.villagerLargePanel.Controls.Remove(villagerButton[i]);
            }

            int player = 0;

            Thread LoadAllVillagerThread = new Thread(delegate () { loadAllVillager(player); });
            LoadAllVillagerThread.Start();
        }

        public void RefreshVillagerUI(bool clear)
        {
            for (int j = 0; j < 10; j++)
            {
                int friendship = V[j].Friendship[0];
                if (villagerButton[j] != selectedVillagerButton)
                    villagerButton[j].BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(friendship)))), ((int)(((byte)(friendship / 2)))), ((int)(((byte)(friendship)))));

                villagerButton[j].Text = V[j].GetRealName() + " : " + V[j].GetInternalName();

                if (V[j].MoveInFlag == 0xC || V[j].MoveInFlag == 0xB)
                {
                    if (V[j].IsReal())
                    {
                        if (V[j].IsInvited())
                            villagerButton[j].Text += "\n(Tom Nook Invited)";
                        else
                            villagerButton[j].Text += "\n(Moving In)";
                    }
                    else
                        villagerButton[j].Text += "\n(Just Move Out)";
                }
                else if (V[j].InvitedFlag == 0x2)
                    villagerButton[j].Text += "\n(Invited by Visitor)";
                else if (V[j].AbandonedHouseFlag == 0x1 && V[j].ForceMoveOutFlag == 0x0)
                    villagerButton[j].Text += "\n(Floor Sweeping)";
                else if (V[j].AbandonedHouseFlag == 0x2 && V[j].ForceMoveOutFlag == 0x1)
                    villagerButton[j].Text += "\n(Moving Out 1)";
                else if (V[j].AbandonedHouseFlag == 0x2 && V[j].ForceMoveOutFlag == 0x0)
                    villagerButton[j].Text += "\n(Moving Out 2)";

                Image img;
                if (V[j].GetRealName() == "ERROR")
                {
                    string path = Utilities.GetVillagerImage(V[j].GetRealName());
                    if (!path.Equals(string.Empty))
                        img = Image.FromFile(path);
                    else
                        img = new Bitmap(Properties.Resources.Leaf, new Size(110, 110));
                }
                else
                {
                    string path = Utilities.GetVillagerImage(V[j].GetInternalName());
                    if (!path.Equals(string.Empty))
                        img = Image.FromFile(path);
                    else
                        img = new Bitmap(Properties.Resources.Leaf, new Size(110, 110));
                }
                villagerButton[j].Image = (Image)(new Bitmap(img, new Size(110, 110)));

            }
            if (!clear)
            {
                if (selectedVillagerButton != null)
                {
                    selectedVillagerButton.BackColor = System.Drawing.Color.LightSeaGreen;

                    int i = Int16.Parse(selectedVillagerButton.Tag.ToString());

                    if (ignoreHeader.Checked)
                    {
                        VillagerControl.Enabled = true;
                    }
                    else
                    {
                        if (blocker == false)
                            VillagerControl.Enabled = true;
                        else
                            VillagerControl.Enabled = false;

                        if (V[i].MoveInFlag == 0xC || V[i].MoveInFlag == 0xB)
                        {
                            if (V[i].IsReal())
                                VillagerControl.Enabled = false;
                        }
                    }

                    IndexValue.Text = V[i].Index.ToString();
                    NameValue.Text = V[i].GetRealName();
                    InternalNameValue.Text = V[i].GetInternalName();
                    PersonalityValue.Text = V[i].GetPersonality();
                    FriendShipValue.Text = V[i].Friendship[0].ToString();
                    HouseIndexValue.Text = V[i].HouseIndex.ToString();
                    MoveInFlag.Text = "0x" + V[i].MoveInFlag.ToString("X");
                    MoveOutValue.Text = "0x" + V[i].AbandonedHouseFlag.ToString("X");
                    ForceMoveOutValue.Text = "0x" + V[i].ForceMoveOutFlag.ToString("X");
                    CatchphraseValue.Text = Encoding.Unicode.GetString(V[i].catchphrase, 0, 44);
                    FullAddress.Text = Utilities.ByteToHexString(V[i].GetHeader());
                    //PlayerName.Text = V[i].GetPlayerName(playerSelectorVillager.SelectedIndex);
                }
            }
            /*
            if (sound)
                System.Media.SystemSounds.Asterisk.Play();
            */
        }

        private void VillagerButton_MouseDown(object sender, MouseEventArgs e)
        {
            selectedVillagerButton = (Button)sender;

            RefreshVillagerUI(false);
        }

        private void DumpVillagerBtn_Click(object sender, EventArgs e)
        {
            if (IndexValue.Text == "")
                return;
            int i = Int16.Parse(IndexValue.Text);

            SaveFileDialog file = new SaveFileDialog()
            {
                Filter = "New Horizons Villager (*.nhv2)|*.nhv2",
                FileName = V[i].GetInternalName() + ".nhv2",
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
            for (int j = 0; j < temp.Length - 1; j++)
                path = path + temp[j] + "\\";

            config.AppSettings.Settings["LastSave"].Value = path;
            config.Save(ConfigurationSaveMode.Minimal);

            Thread dumpThread = new Thread(delegate () { dumpVillager(i, file); });
            dumpThread.Start();
        }

        private void dumpVillager(int i, SaveFileDialog file)
        {
            showVillagerWait((int)Utilities.VillagerSize, "Dumping " + V[i].GetRealName() + " ...");

            blocker = true;

            byte[] VillagerData = Utilities.GetVillager(s, bot, i, (int)Utilities.VillagerSize, ref counter);
            File.WriteAllBytes(file.FileName, VillagerData);

            byte[] CheckData = File.ReadAllBytes(file.FileName);
            byte[] CheckHeader = new byte[52];
            if (header[0] != 0x0 && header[1] != 0x0 && header[2] != 0x0)
            {
                Buffer.BlockCopy(CheckData, 0x4, CheckHeader, 0x0, 52);
            }

            if (!CheckHeader.SequenceEqual(header))
            {
                Debug.Print(Utilities.ByteToHexString(CheckHeader));
                Debug.Print(Utilities.ByteToHexString(header));
                MessageBox.Show("Wait something is wrong here!? \n\n Header Mismatch!", "Warning");
            }
            if (sound)
                System.Media.SystemSounds.Asterisk.Play();

            blocker = false;

            hideVillagerWait();
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                if (counter <= VillagerProgressBar.Maximum)
                    VillagerProgressBar.Value = counter;
                else
                    VillagerProgressBar.Value = VillagerProgressBar.Maximum;
            });
        }

        private void DumpMoveOutBtn_Click(object sender, EventArgs e)
        {
            if (IndexValue.Text == "")
                return;
            int i = Int16.Parse(IndexValue.Text);
            byte[] move = Utilities.GetMoveout(s, bot, i, (int)(0x33), ref counter);
            File.WriteAllBytes(V[i].GetInternalName() + "MOVEOUT.bin", move);
        }

        private void MoveOutBtn_Click(object sender, EventArgs e)
        {
            if (IndexValue.Text == "")
                return;
            int i = Int16.Parse(IndexValue.Text);

            if (checkMultipleMoveOut() && !firstWarning)
            {
                DialogResult dialogResult = myMessageBox.Show("It seems you alreadly have someone moving out." +
    "                                                   \nAre you sure you want to force another moveout?" +
    "                                                   \nNote that multiple moveout on the same day is not recommended.", "Multiple moveout detected!", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dialogResult == DialogResult.No)
                    return;
                if (dialogResult == DialogResult.Yes)
                    firstWarning = true;
            }

            Utilities.SetMoveout(s, bot, i);

            V[i].AbandonedHouseFlag = 2;
            V[i].ForceMoveOutFlag = 1;
            V[i].InvitedFlag = 0;
            RefreshVillagerUI(false);
        }
        private void StayMoveBtn_Click(object sender, EventArgs e)
        {
            if (IndexValue.Text == "")
                return;
            int i = Int16.Parse(IndexValue.Text);

            if (checkMultipleMoveOut() && !firstWarning)
            {
                DialogResult dialogResult = myMessageBox.Show("It seems you alreadly have someone moving out." +
    "                                                   \nAre you sure you want to force another moveout?" +
    "                                                   \nNote that multiple moveout on the same day is not recommended.", "Multiple moveout detected!", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                if (dialogResult == DialogResult.No)
                    return;
                if (dialogResult == DialogResult.Yes)
                    firstWarning = true;
            }

            Utilities.SetMoveout(s, bot, i, "2", "0");

            V[i].AbandonedHouseFlag = 2;
            V[i].ForceMoveOutFlag = 0;
            V[i].InvitedFlag = 0;
            RefreshVillagerUI(false);
        }
        private void CancelMoveOutBtn_Click(object sender, EventArgs e)
        {
            if (IndexValue.Text == "")
                return;
            int i = Int16.Parse(IndexValue.Text);
            Utilities.SetMoveout(s, bot, i, "0", "0");

            V[i].AbandonedHouseFlag = 0;
            V[i].ForceMoveOutFlag = 0;
            V[i].InvitedFlag = 0;
            RefreshVillagerUI(false);
        }

        private void MoveOutAllBtn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                Utilities.SetMoveout(s, bot, i);

                V[i].AbandonedHouseFlag = 2;
                V[i].ForceMoveOutFlag = 1;
                V[i].InvitedFlag = 0;
            }
            RefreshVillagerUI(false);

            if (sound)
                System.Media.SystemSounds.Asterisk.Play();
        }

        private void StayMoveAllBtn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                Utilities.SetMoveout(s, bot, i, "2", "0");

                V[i].AbandonedHouseFlag = 2;
                V[i].ForceMoveOutFlag = 0;
                V[i].InvitedFlag = 0;
            }
            RefreshVillagerUI(false);

            if (sound)
                System.Media.SystemSounds.Asterisk.Play();
        }

        private void CancelMoveOutAllBtn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                Utilities.SetMoveout(s, bot, i, "0", "0");

                V[i].AbandonedHouseFlag = 0;
                V[i].ForceMoveOutFlag = 0;
                V[i].InvitedFlag = 0;
            }
            RefreshVillagerUI(false);

            if (sound)
                System.Media.SystemSounds.Asterisk.Play();
        }

        private void SetFriendshipBtn_Click(object sender, EventArgs e)
        {

            if (IndexValue.Text == "")
                return;
            int i = Int16.Parse(IndexValue.Text);

            Image img;
            string path;
            if (V[i].GetRealName() == "ERROR")
            {
                path = Utilities.GetVillagerImage(V[i].GetRealName());
            }
            else
            {
                path = Utilities.GetVillagerImage(V[i].GetInternalName());
            }

            if (!path.Equals(string.Empty))
                img = Image.FromFile(path);
            else
                img = new Bitmap(Properties.Resources.Leaf, new Size(128, 128));

            friendship = new Friendship(this, i, s, bot, img, V[i].GetRealName(), sound);
            friendship.Show();
            friendship.Location = new System.Drawing.Point(this.Location.X + 30, this.Location.Y + 30);
        }

        public string PassPlayerName(int i, int p)
        {
            return V[i].GetPlayerName(p);
        }

        public void SetFriendship(int i, int p, int value)
        {
            V[i].Friendship[p] = (byte)value;
        }

        private void DumpAllHouseBtn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                byte[] house = Utilities.GetHouse(s, bot, i, ref counter);
                File.WriteAllBytes("House" + i + ".nhvh", house);
            }
        }

        private void DumpAllHouseBufferBtn_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
            {
                byte[] house = Utilities.GetHouse(s, bot, i, ref counter, Utilities.VillagerHouseBufferDiff);
                File.WriteAllBytes("HouseBuffer" + i + ".nhvh", house);
            }
        }

        private void DumpHouseBtn_Click(object sender, EventArgs e)
        {
            if (IndexValue.Text == "")
                return;
            int i = Int16.Parse(IndexValue.Text);

            int j = V[i].HouseIndex;

            if (j < 0 || j > 9)
                return;

            SaveFileDialog file = new SaveFileDialog()
            {
                Filter = "New Horizons Villager House (*.nhvh)|*.nhvh",
                FileName = V[i].GetInternalName() + ".nhvh",
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
            for (int k = 0; k < temp.Length - 1; k++)
                path = path + temp[k] + "\\";

            config.AppSettings.Settings["LastSave"].Value = path;
            config.Save(ConfigurationSaveMode.Minimal);

            Thread dumpThread = new Thread(delegate () { dumpHouse(i, j, file); });
            dumpThread.Start();
        }

        private void dumpHouse(int i, int j, SaveFileDialog file)
        {
            showVillagerWait((int)Utilities.VillagerHouseSize, "Dumping " + V[i].GetRealName() + "'s House ...");

            byte[] house = Utilities.GetHouse(s, bot, j, ref counter);
            File.WriteAllBytes(file.FileName, house);
            if (sound)
                System.Media.SystemSounds.Asterisk.Play();

            hideVillagerWait();
        }

        private void ReadMysVillagerBtn_Click(object sender, EventArgs e)
        {
            byte[] IName = Utilities.GetMysVillagerName(s, bot);
            string StrName = Encoding.ASCII.GetString(Utilities.ByteTrim(IName));
            string RealName = Utilities.GetVillagerRealName(StrName);

            Image img;
            if (RealName == "ERROR")
            {
                string path = Utilities.GetVillagerImage(RealName);
                if (!path.Equals(string.Empty))
                    img = Image.FromFile(path);
                else
                    img = new Bitmap(Properties.Resources.Leaf, new Size(110, 110));
                MysVillagerBtn.Text = "";
            }
            else
            {
                string path = Utilities.GetVillagerImage(StrName);
                if (!path.Equals(string.Empty))
                    img = Image.FromFile(path);
                else
                    img = new Bitmap(Properties.Resources.Leaf, new Size(110, 110));
                MysIName.Text = StrName;
                MysRealName.Text = RealName;
                MysVillagerBtn.Text = RealName + " : " + StrName;
            }
            MysVillagerBtn.Image = (Image)(new Bitmap(img, new Size(110, 110)));
            if (sound)
                System.Media.SystemSounds.Asterisk.Play();
        }

        private void TransformBtn_Click(object sender, EventArgs e)
        {
            if (MysSelector.SelectedIndex < 0)
                return;
            string[] lines = MysSelector.SelectedItem.ToString().Split(new string[] { " " }, StringSplitOptions.None);
            byte[] IName = Encoding.Default.GetBytes(lines[lines.Length - 1]);
            byte[] species = new byte[1];
            species[0] = Utilities.CheckSpecies[(lines[lines.Length - 1]).Substring(0, 3)];

            Utilities.SetMysVillager(s, bot, IName, species, ref counter);

            Image img;
            string path = Utilities.GetVillagerImage(lines[lines.Length - 1]);
            if (!path.Equals(String.Empty))
                img = Image.FromFile(path);
            else
                img = new Bitmap(Properties.Resources.Leaf, new Size(110, 110));
            MysIName.Text = lines[lines.Length - 1];
            MysRealName.Text = lines[0];
            MysVillagerBtn.Text = lines[0] + " : " + lines[lines.Length - 1];
            MysVillagerBtn.Image = (Image)(new Bitmap(img, new Size(110, 110)));
            if (sound)
                System.Media.SystemSounds.Asterisk.Play();
        }

        private void SetCatchphraseBtn_Click(object sender, EventArgs e)
        {
            if (IndexValue.Text == "")
                return;
            int i = Int16.Parse(IndexValue.Text);
            byte[] phrase = new byte[44];
            byte[] temp = Encoding.Unicode.GetBytes(CatchphraseValue.Text);

            for (int j = 0; j < temp.Length; j++)
            {
                phrase[j] = temp[j];
            }

            Utilities.SetCatchphrase(s, bot, i, phrase);

            V[i].catchphrase = phrase;
            RefreshVillagerUI(false);
        }

        private void ResetCatchphraseBtn_Click(object sender, EventArgs e)
        {
            if (IndexValue.Text == "")
                return;
            int i = Int16.Parse(IndexValue.Text);
            byte[] phrase = new byte[44];

            Utilities.SetCatchphrase(s, bot, i, phrase);

            V[i].catchphrase = phrase;
            RefreshVillagerUI(false);
        }

        private void showVillagerWait(int size, string msg = "")
        {
            this.Invoke((MethodInvoker)delegate
            {
                VillagerControl.Enabled = false;
                //overlay.Visible = true;
                //overlay.BringToFront();
                WaitMessagebox.SelectionAlignment = HorizontalAlignment.Center;
                WaitMessagebox.Text = msg;
                counter = 0;
                if (bot == null)
                    VillagerProgressBar.Maximum = size / 500 + 5;
                else
                    VillagerProgressBar.Maximum = size / 300 + 5;
                Debug.Print("Max : " + VillagerProgressBar.Maximum.ToString());
                VillagerProgressBar.Value = counter;
                PleaseWaitPanel.Visible = true;
                ProgressTimer.Start();
            });
        }
        private void hideVillagerWait()
        {
            if (InvokeRequired)
            {
                MethodInvoker method = new MethodInvoker(hideVillagerWait);
                Invoke(method);
                return;
            }
            if (!blocker)
            {
                Debug.Print("Counter : " + counter.ToString());
                PleaseWaitPanel.Visible = false;
                VillagerControl.Enabled = true;
                //overlay.Visible = false;
                if (PleaseWaitPanel.Location.X != 780)
                    PleaseWaitPanel.Location = new Point(780, 330);
                ProgressTimer.Stop();
            }
        }

        private void LoadVillagerBtn_Click(object sender, EventArgs e)
        {

            if (IndexValue.Text == "")
                return;
            int i = Int16.Parse(IndexValue.Text);

            OpenFileDialog file = new OpenFileDialog()
            {
                Filter = "New Horizons Villager (*.nhv2)|*.nhv2|New Horizons Villager (*.nhv)|*.nhv",
                //FileName = V[i].GetInternalName() + ".nhv",
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
            for (int j = 0; j < temp.Length - 1; j++)
                path = path + temp[j] + "\\";

            config.AppSettings.Settings["LastLoad"].Value = path;
            config.Save(ConfigurationSaveMode.Minimal);

            byte[] data = File.ReadAllBytes(file.FileName);

            if (data.Length == Utilities.VillagerOldSize)
            {
                data = ConvertToNew(data);
            }

            if (data.Length != Utilities.VillagerSize)
            {
                MessageBox.Show("Villager file size incorrect!", "Villager file invalid");
                return;
            }

            string msg = "Loading villager...";

            Thread LoadVillagerThread = new Thread(delegate () { loadvillager(i, data, msg); });
            LoadVillagerThread.Start();
        }

        private void loadvillager(int i, byte[] villager, string msg)
        {
            showVillagerWait((int)Utilities.VillagerSize * 2, msg);

            blocker = true;

            byte[] modifiedVillager = villager;
            if (!ignoreHeader.Checked)
            {
                if (header[0] != 0x0 && header[1] != 0x0 && header[2] != 0x0)
                {
                    Buffer.BlockCopy(header, 0x0, modifiedVillager, 0x4, 52);
                }
            }

            V[i].LoadData(modifiedVillager);

            //byte[] move = Utilities.GetMoveout(s, bot, i, (int)0x33, ref counter);
            V[i].AbandonedHouseFlag = Convert.ToInt32(villager[Utilities.VillagerMoveoutOffset]);
            V[i].ForceMoveOutFlag = Convert.ToInt32(villager[Utilities.VillagerForceMoveoutOffset]);

            byte[] phrase = new byte[44];
            //buffer.BlockCopy(Bank01to20, slotOffset, slotBytes, 0x0, 0x4);
            Buffer.BlockCopy(villager, (int)Utilities.VillagerCatchphraseOffset, phrase, 0x0, 44);
            V[i].catchphrase = phrase;

            Utilities.LoadVillager(s, bot, i, modifiedVillager, ref counter);

            this.Invoke((MethodInvoker)delegate
            {
                RefreshVillagerUI(false);
            });

            blocker = false;

            hideVillagerWait();
        }

        private void LoadHouseBtn_Click(object sender, EventArgs e)
        {
            if (IndexValue.Text == "")
                return;
            int i = Int16.Parse(IndexValue.Text);

            int j = V[i].HouseIndex;

            if (j < 0 || j > 9)
                return;

            OpenFileDialog file = new OpenFileDialog()
            {
                Filter = "New Horizons Villager House (*.nhvh)|*.nhvh",
                //FileName = V[i].GetInternalName() + ".nhvh",
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
            for (int k = 0; k < temp.Length - 1; k++)
                path = path + temp[k] + "\\";

            config.AppSettings.Settings["LastLoad"].Value = path;
            config.Save(ConfigurationSaveMode.Minimal);

            byte[] data = File.ReadAllBytes(file.FileName);

            if (data.Length != Utilities.VillagerHouseSize)
            {
                MessageBox.Show("House file size incorrect!", "House file invalid");
                return;
            }

            string msg = "Loading house...";

            Thread LoadHouseThread = new Thread(delegate () { loadhouse(i, j, data, msg); });
            LoadHouseThread.Start();

        }
        private void loadhouse(int i, int j, byte[] house, string msg)
        {
            showVillagerWait((int)Utilities.VillagerHouseSize * 2, msg);

            byte[] modifiedHouse = house;

            byte h = (Byte)i;
            modifiedHouse[Utilities.VillagerHouseOwnerOffset] = h;
            V[i].HouseIndex = j;
            HouseList[j] = i;

            Utilities.LoadHouse(s, bot, j, modifiedHouse, ref counter);

            this.Invoke((MethodInvoker)delegate
            {
                RefreshVillagerUI(false);
            });

            hideVillagerWait();
        }

        private void ReplaceBtn_Click(object sender, EventArgs e)
        {

            if (IndexValue.Text == "")
                return;
            int i = Int16.Parse(IndexValue.Text);

            int j = V[i].HouseIndex;

            if (j > 9)
                return;

            int EmptyHouseNumber;

            if (j < 0)
            {
                EmptyHouseNumber = findEmptyHouse();
                if (EmptyHouseNumber >= 0)
                    j = EmptyHouseNumber;
                else
                    return;
            }

            if (ReplaceSelector.SelectedIndex < 0)
                return;
            string[] lines = ReplaceSelector.SelectedItem.ToString().Split(new string[] { " " }, StringSplitOptions.None);
            //byte[] IName = Encoding.Default.GetBytes(lines[lines.Length - 1]);
            string IName = lines[lines.Length - 1];
            string RealName = lines[0];
            string IVpath = Utilities.villagerPath + IName + ".nhv2";
            string RVpath = Utilities.villagerPath + RealName + ".nhv2";
            byte[] villagerData;
            byte[] houseData;

            if (checkDuplicate(IName))
            {
                DialogResult dialogResult = myMessageBox.Show(RealName + " is currently living on your island!" +
                    "                                                   \nAre you sure you want to continue the replacement?" +
                    "                                                   \nNote that the game will attempt to remove any duplicated villager!", "Villager already exists!", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
                if (dialogResult == DialogResult.No)
                {
                    return;
                }
            }

            if (File.Exists(IVpath))
            {
                Debug.Print("FOUND: " + IVpath);
                villagerData = File.ReadAllBytes(IVpath);
            }
            else if (File.Exists(RVpath))
            {
                Debug.Print("FOUND: " + RVpath);
                villagerData = File.ReadAllBytes(RVpath);
            }
            else
            {
                MessageBox.Show("Villager files \"" + IName + ".nhv2\" " + "/ \"" + RealName + ".nhv2\" " + "not found!", "Villager file not found");
                return;
            }

            string IHpath = Utilities.villagerPath + IName + ".nhvh";
            string RHpath = Utilities.villagerPath + RealName + ".nhvh";
            if (File.Exists(IHpath))
            {
                Debug.Print("FOUND: " + IHpath);
                houseData = File.ReadAllBytes(IHpath);
            }
            else if (File.Exists(RHpath))
            {
                Debug.Print("FOUND: " + RHpath);
                houseData = File.ReadAllBytes(RHpath);
            }
            else
            {
                MessageBox.Show("Villager house files \"" + IName + ".nhvh\" " + "/ \"" + RealName + ".nhvh\" " + "not found!", "House file not found");
                return;
            }

            string msg = "Replacing Villager...";

            Thread LoadBothThread = new Thread(delegate () { loadBoth(i, j, villagerData, houseData, msg); });
            LoadBothThread.Start();
        }

        private void loadBoth(int i, int j, byte[] villager, byte[] house, string msg)
        {
            if (villager.Length != Utilities.VillagerSize)
            {
                MessageBox.Show("Villager file size incorrect!", "Villager file invalid");
                return;
            }
            if (house.Length != Utilities.VillagerHouseSize)
            {
                MessageBox.Show("House file size incorrect!", "House file invalid");
                return;
            }

            showVillagerWait((int)Utilities.VillagerSize * 2 + (int)Utilities.VillagerHouseSize * 2, msg);

            blocker = true;

            byte[] modifiedVillager = villager;
            if (!ignoreHeader.Checked)
            {
                if (header[0] != 0x0 && header[1] != 0x0 && header[2] != 0x0)
                {
                    Buffer.BlockCopy(header, 0x0, modifiedVillager, 0x4, 52);
                }
            }

            V[i].LoadData(modifiedVillager);

            V[i].AbandonedHouseFlag = Convert.ToInt32(villager[Utilities.VillagerMoveoutOffset]);
            V[i].ForceMoveOutFlag = Convert.ToInt32(villager[Utilities.VillagerForceMoveoutOffset]);

            byte[] phrase = new byte[44];
            Buffer.BlockCopy(villager, (int)Utilities.VillagerCatchphraseOffset, phrase, 0x0, 44);
            V[i].catchphrase = phrase;


            byte[] modifiedHouse = house;

            byte h = (Byte)i;
            modifiedHouse[Utilities.VillagerHouseOwnerOffset] = h;
            V[i].HouseIndex = j;
            HouseList[j] = i;

            Utilities.LoadVillager(s, bot, i, modifiedVillager, ref counter);
            Utilities.LoadHouse(s, bot, j, modifiedHouse, ref counter);

            this.Invoke((MethodInvoker)delegate
            {
                RefreshVillagerUI(false);
            });

            blocker = false;

            hideVillagerWait();
        }

        private void LockTimer_Tick(object sender, EventArgs e)
        {
            if (LockBox.Checked)
            {
                try
                {
                    Utilities.pokeAddress(s, bot, Utilities.TextSpeedAddress.ToString("X"), "3");
                }
                catch
                {
                    LockBox.Checked = false;
                }
            }
        }

        private void LockBox_CheckedChanged(object sender, EventArgs e)
        {
            Timer LockTimer = new Timer();
            if (LockBox.Checked)
            {
                LockTimer.Tick += new EventHandler(LockTimer_Tick);
                LockTimer.Interval = 500; // in miliseconds
                LockTimer.Start();
            }
            else
            {
                LockTimer.Stop();
            }
        }

        private void cleanVillagerPage()
        {
            if (villagerButton != null)
            {
                for (int i = 0; i < 10; i++)
                    this.villagerLargePanel.Controls.Remove(villagerButton[i]);
            }

            V = null;
            villagerButton = null;
            firstload = false;
            PleaseWaitPanel.Location = new Point(170, 150);
        }

        private bool checkDuplicate(string iName)
        {
            for (int i = 0; i < 10; i++)
            {
                if (V[i].GetInternalName() == iName)
                    return true;
            }
            return false;
        }

        private bool checkMultipleMoveOut()
        {
            /*
            int num = 0;
            for (int i = 0; i < 10; i++)
            {
                if (V[i].AbandonedHouseFlag == 0x2)
                    num++;
            }
            if (num > 0)
                return true;
            else
            */
            return false;
        }

        private int findEmptyHouse()
        {
            for (int i = 0; i < 10; i++)
            {
                if (HouseList[i] == 255)
                    return i;
            }
            return -1;
        }

        private void VillagerSearch_Click(object sender, EventArgs e)
        {
            VillagerSearch.Text = "";
            VillagerSearch.ForeColor = Color.White;
        }

        private void VillagerSearch_Leave(object sender, EventArgs e)
        {
            if (VillagerSearch.Text == "")
            {
                VillagerSearch.Text = "Search...";
                VillagerSearch.ForeColor = Color.FromArgb(255, 114, 118, 125);
            }
        }

        private void VillagerSearch_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < ReplaceSelector.Items.Count; i++)
            {
                string[] lines = ReplaceSelector.Items[i].ToString().Split(new string[] { " " }, StringSplitOptions.None);
                //byte[] IName = Encoding.Default.GetBytes(lines[lines.Length - 1]);
                //string IName = lines[lines.Length - 1];
                string RealName = lines[0];
                if (VillagerSearch.Text.ToLower() == RealName.ToLower())
                {
                    ReplaceSelector.SelectedIndex = i;
                    break;
                }
            }
        }

        private void IslandSearch_Click(object sender, EventArgs e)
        {
            IslandSearch.Text = "";
            IslandSearch.ForeColor = Color.White;
        }

        private void IslandSearch_Leave(object sender, EventArgs e)
        {
            if (IslandSearch.Text == "")
            {
                IslandSearch.Text = "Search...";
                IslandSearch.ForeColor = Color.FromArgb(255, 114, 118, 125);
            }
        }

        private void IslandSearch_TextChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < MysSelector.Items.Count; i++)
            {
                string[] lines = MysSelector.Items[i].ToString().Split(new string[] { " " }, StringSplitOptions.None);
                //byte[] IName = Encoding.Default.GetBytes(lines[lines.Length - 1]);
                //string IName = lines[lines.Length - 1];
                string RealName = lines[0];
                if (IslandSearch.Text.ToLower() == RealName.ToLower())
                {
                    MysSelector.SelectedIndex = i;
                    break;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string folderPath = @"villager/";
            foreach (string file in Directory.EnumerateFiles(folderPath, "*.nhv"))
            {
                byte[] data = File.ReadAllBytes(file);

                if (data.Length != Utilities.VillagerOldSize)
                {
                    MessageBox.Show("Villager file size incorrect!", "Villager file invalid");
                }
                else
                {
                    byte[] newdata = ConvertToNew(data);
                    File.WriteAllBytes(file + "2", newdata);
                }
            }
        }

        private static byte[] ConvertToNew(byte[] oldVillager)
        {
            byte[] newVillager = new byte[Utilities.VillagerSize];

            Array.Copy(oldVillager, 0, newVillager, 0, 0x2f84);

            for (int i = 0; i < 160; i++)
            {
                var src = 0x2f84 + (0x14C * i);
                var dest = 0x2f84 + (0x158 * i);

                Array.Copy(oldVillager, src, newVillager, dest, 0x14C);
            }

            Array.Copy(oldVillager, 0xff04, newVillager, 0x10684, oldVillager.Length - 0xff04);

            return newVillager;
        }

        /*
        private void loadFriendship(int player)
        {
            showVillagerWait(10, "Acquiring Friendship data...");

            if ((s == null || s.Connected == false) & bot == null)
                return;

            blocker = true;

            for (int i = 0; i < 10; i++)
            {
                //byte[] b = Utilities.GetVillager(s, bot, i, (int)(Utilities.VillagerMemoryTinySize), ref counter);
                byte[] b = Utilities.GetPlayerDataVillager(s, bot, i, player, (int)(Utilities.VillagerMemoryTinySize), ref counter);
                V[i].TempData[player] = b;
                V[i].Friendship[player] = b[70];
            }

            this.Invoke((MethodInvoker)delegate
            {
                RefreshVillagerUI(false);
            });

            blocker = false;

            hideVillagerWait();
        }
        */

        public void loadFriendship(byte[] b, int i, int player)
        {
            V[i].TempData[player] = b;
            V[i].Friendship[player] = b[70];
        }

        private bool checkFriendshipInit(int player)
        {
            for (int i = 0; i < 10; i++)
            {
                if (V[i].Friendship[player] != 0)
                    return true;
            }
            return false;
        }
    }
}