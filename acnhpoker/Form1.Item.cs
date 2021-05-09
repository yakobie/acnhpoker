using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace ACNHPoker
{
    public partial class Form1 : Form
    {
        #region Load File
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

        private DataTable loadCSVwoKey(string filePath)
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

            return dt;
        }

        #endregion

        private void connectBtn_Click(object sender, EventArgs e)
        {
            if (connectBtn.Tag.ToString() == "connect")
            {

                string ipPattern = @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b";

                if (!Regex.IsMatch(ipBox.Text, ipPattern))
                {
                    pictureBox1.BackColor = System.Drawing.Color.Orange;
                    return;
                }

                s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipBox.Text), 6000);

                if (s.Connected == false)
                {
                    //really messed up way to do it but yolo
                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;


                        IAsyncResult result = s.BeginConnect(ep, null, null);
                        bool conSuceded = result.AsyncWaitHandle.WaitOne(3000, true);


                        if (conSuceded == true)
                        {
                            try
                            {
                                s.EndConnect(result);
                            }
                            catch
                            {
                                this.pictureBox1.Invoke((MethodInvoker)delegate
                                {
                                    Log.logEvent("MainForm", "Connection Failed : " + ipBox.Text);
                                    this.pictureBox1.BackColor = Color.Red;
                                });
                                myMessageBox.Show("You have successfully started a connection!\n" +
                                                "Your Switch IP address is correct!\n" +
                                                "However...\n" +
                                                "Sys-botbase is not responding...\n\n\n" +

                                                "First, \n" +
                                                "check that your Switch is running in CFW mode.\n" +
                                                "On your Switch, go to \"System Settings\"->\"System\"\n" +
                                                "Check \"Current version:\" and make sure you have \"AMS\" in it.\n\n" +
                                                "Second, \n" +
                                                "check that you have the latest version of Sys-botbase installed.\n" +
                                                "You can get the latest version at \n        https://github.com/olliz0r/sys-botbase/releases \n" +
                                                "Double check your installation and make sure that the \n \"430000000000000B\" folder is inside the \"atmosphere\\contents\" folder.\n\n"

                                                , "Where are you, my socket 6000?", MessageBoxButtons.OK, MessageBoxIcon.Warning);

                                return;
                            }

                            Invoke((MethodInvoker)delegate
                            {
                                this.pictureBox1.BackColor = System.Drawing.Color.Green;

                                Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

                                if (File.Exists(settingFile))
                                {
                                    config.AppSettings.Settings["ipAddress"].Value = this.ipBox.Text;
                                    config.Save(ConfigurationSaveMode.Minimal);
                                    if (config.AppSettings.Settings["autoRefresh"].Value == "true")
                                    {
                                        this.autoRefreshCheckBox.Checked = true;
                                    }
                                    else
                                    {
                                        this.autoRefreshCheckBox.Checked = false;
                                    }
                                }

                                Log.logEvent("MainForm", "Connection Succeeded : " + ipBox.Text);

                                if (validation())
                                {
                                    string sysbotbaseVersion = Utilities.getVersion(s);
                                    myMessageBox.Show("You have successfully established a connection!\n" +
                                                    "Your Sys-botbase installation and IP address are correct.\n" +
                                                    "However...\n" +
                                                    "Sys-botbase validation failed! Poke request is returning same result.\n\n\n" +

                                                    "First, \n" +
                                                    "check that you have the correct matching version.\n" +
                                                    "You are using \"" + version + "\" right now.\n" +
                                                    "You can find the latest version at : \n   https://github.com/MyShiLingStar/ACNHPoker/releases \n" +
                                                    "Your sys-botbase version is :  " + sysbotbaseVersion + "\n" +
                                                    "You can find the latest version at : \n   https://github.com/olliz0r/sys-botbase/releases \n\n" +
                                                    "Second, \n" +
                                                    "try holding the power button and restart your switch.\n" +
                                                    "Then press and HOLD the \"L\" button while you are selecting the game to boot up.\n" +
                                                    "Keep holding the \"L\" button and release it once you can see the title screen.\n" +
                                                    "Retry the connection.\n\n" +
                                                    "Third, \n" +
                                                    "If you use a cheat file before and have the following folder : \n" +
                                                    "sd: / atmospshere / contents / 01006F8002326000 /\n" +
                                                    "please try to remove or rename it.\n"

                                                    , "Sys-botbase validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    s.Close();
                                    return;
                                }



                                this.refreshBtn.Visible = true;
                                //this.playerSelectionPanel.Visible = true;
                                this.playerSelectorInventory.Visible = true;
                                this.autoRefreshCheckBox.Visible = true;
                                this.saveBtn.Visible = true;
                                this.loadBtn.Visible = true;
                                this.otherBtn.Visible = true;
                                this.critterBtn.Visible = true;
                                this.villagerBtn.Visible = true;
                                this.wrapSetting.SelectedIndex = 0;
                                //this.selectedItem.setHide(true);
                                this.connectBtn.Tag = "disconnect";
                                this.connectBtn.Text = "Disconnect";
                                this.USBconnectBtn.Visible = false;
                                this.configBtn.Visible = false;
                                this.mapDropperBtn.Visible = true;
                                this.regeneratorBtn.Visible = true;
                                this.dodoHelperBtn.Visible = true;
                                offline = false;

                                int CurrentPlayerIndex = updateDropdownBox();

                                playerSelectorInventory.SelectedIndex = CurrentPlayerIndex;
                                playerSelectorOther.SelectedIndex = CurrentPlayerIndex;
                                this.Text = this.Text + UpdateTownID();

                                InitTimer();
                                setEatBtn();
                                if (!disableValidation)
                                {
                                    UpdateTurnipPrices();
                                }
                                readWeatherSeed();

                                currentGridView = insectGridView;

                                LoadGridView(InsectAppearParam, insectGridView, ref insectRate, Utilities.InsectDataSize, Utilities.InsectNumRecords);
                                LoadGridView(FishRiverAppearParam, riverFishGridView, ref riverFishRate, Utilities.FishDataSize, Utilities.FishRiverNumRecords, 1);
                                LoadGridView(FishSeaAppearParam, seaFishGridView, ref seaFishRate, Utilities.FishDataSize, Utilities.FishSeaNumRecords, 1);
                                LoadGridView(CreatureSeaAppearParam, seaCreatureGridView, ref seaCreatureRate, Utilities.SeaCreatureDataSize, Utilities.SeaCreatureNumRecords, 1);

                                teleporter = new teleport(s);
                                Controller = new controller(s, IslandName);
                            });

                        }
                        else
                        {
                            s.Close();
                            this.pictureBox1.Invoke((MethodInvoker)delegate
                            {
                                this.pictureBox1.BackColor = System.Drawing.Color.Red;
                            });
                            myMessageBox.Show("Unable to connect to the Sys-botbase server.\n" +
                                            "Please double check your IP address and Sys-botbase installation.\n\n" +
                                            "You might also need to disable your firewall or antivirus temporary to allow outgoing connection."
                                            , "Unable to establish connection!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        }
                    }).Start();
                }
            }
            else
            {
                s.Close();
                foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                {
                    btn.reset();
                }
                connectBtn.Enabled = true;
                refreshBtn.Visible = false;
                //playerSelectionPanel.Visible = false;
                playerSelectorInventory.Visible = false;
                autoRefreshCheckBox.Visible = false;
                this.USBconnectBtn.Visible = true;

                //this.saveBtn.Visible = false;
                //this.loadBtn.Visible = false;
                inventoryBtn_Click(sender, e);
                otherBtn.Visible = false;
                critterBtn.Visible = false;
                this.villagerBtn.Visible = false;
                this.configBtn.Visible = true;
                cleanVillagerPage();
                this.mapDropperBtn.Visible = false;
                this.regeneratorBtn.Visible = false;
                this.dodoHelperBtn.Visible = false;
                offline = true;

                this.connectBtn.Tag = "connect";
                this.connectBtn.Text = "Connect";

                this.Text = version;
            }
        }

        #region Auto Refresh
        public void InitTimer()
        {
            refreshTimer = new Timer();
            refreshTimer.Tick += new EventHandler(refreshTimer_Tick);
            refreshTimer.Interval = 3000; // in miliseconds
            refreshTimer.Start();
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                if (s != null && s.Connected == true && autoRefreshCheckBox.Checked && allowUpdate)
                    Invoke((MethodInvoker)delegate
                    {
                        UpdateInventory();
                    });
            }
            catch (Exception ex)
            {
                Log.logEvent("MainForm", "RefreshTimer: " + ex.Message.ToString());
                Invoke((MethodInvoker)delegate { this.autoRefreshCheckBox.Checked = false; });
                refreshTimer.Stop();
                myMessageBox.Show("Lost connection to the switch...\nDid the switch go to sleep?", "Disconnected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Update Inventroy
        private bool UpdateInventory()
        {
            allowUpdate = false;

            try
            {
                byte[] Bank01to20 = Utilities.GetInventoryBank(s, bot, 1);
                if (Bank01to20 == null)
                {
                    return true;
                }
                byte[] Bank21to40 = Utilities.GetInventoryBank(s, bot, 21);
                if (Bank21to40 == null)
                {
                    return true;
                }
                //string Bank1 = Utilities.ByteToHexString(Bank01to20);
                //string Bank2 = Utilities.ByteToHexString(Bank21to40);

                //Debug.Print(Bank1);
                //Debug.Print(Bank2);

                foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                {
                    if (btn.Tag == null)
                        continue;

                    if (btn.Tag.ToString() == "")
                        continue;

                    int slotId = int.Parse(btn.Tag.ToString());

                    byte[] slotBytes = new byte[2];
                    byte[] flag1Bytes = new byte[1];
                    byte[] flag2Bytes = new byte[1];
                    byte[] dataBytes = new byte[4];
                    byte[] recipeBytes = new byte[2];

                    int slotOffset;
                    int countOffset;
                    int flag1Offset;
                    int flag2Offset;
                    if (slotId < 21)
                    {
                        slotOffset = ((slotId - 1) * 0x8);
                        flag1Offset = 0x3 + ((slotId - 1) * 0x8);
                        flag2Offset = 0x2 + ((slotId - 1) * 0x8);
                        countOffset = 0x4 + ((slotId - 1) * 0x8);
                    }
                    else
                    {
                        slotOffset = ((slotId - 21) * 0x8);
                        flag1Offset = 0x3 + ((slotId - 21) * 0x8);
                        flag2Offset = 0x2 + ((slotId - 21) * 0x8);
                        countOffset = 0x4 + ((slotId - 21) * 0x8);
                    }

                    if (slotId < 21)
                    {
                        Buffer.BlockCopy(Bank01to20, slotOffset, slotBytes, 0x0, 0x2);
                        Buffer.BlockCopy(Bank01to20, flag1Offset, flag1Bytes, 0x0, 0x1);
                        Buffer.BlockCopy(Bank01to20, flag2Offset, flag2Bytes, 0x0, 0x1);
                        Buffer.BlockCopy(Bank01to20, countOffset, dataBytes, 0x0, 0x4);
                        Buffer.BlockCopy(Bank01to20, countOffset, recipeBytes, 0x0, 0x2);
                    }
                    else
                    {
                        Buffer.BlockCopy(Bank21to40, slotOffset, slotBytes, 0x0, 0x2);
                        Buffer.BlockCopy(Bank21to40, flag1Offset, flag1Bytes, 0x0, 0x1);
                        Buffer.BlockCopy(Bank21to40, flag2Offset, flag2Bytes, 0x0, 0x1);
                        Buffer.BlockCopy(Bank21to40, countOffset, dataBytes, 0x0, 0x4);
                        Buffer.BlockCopy(Bank21to40, countOffset, recipeBytes, 0x0, 0x2);
                    }

                    string itemID = Utilities.flip(Utilities.ByteToHexString(slotBytes));
                    string itemData = Utilities.flip(Utilities.ByteToHexString(dataBytes));
                    string recipeData = Utilities.flip(Utilities.ByteToHexString(recipeBytes));
                    string flag1 = Utilities.ByteToHexString(flag1Bytes);
                    string flag2 = Utilities.ByteToHexString(flag2Bytes);

                    //Debug.Print("Slot : " + slotId.ToString() + " ID : " + itemID + " Data : " + itemData + " recipeData : " + recipeData + " Flag1 : " + flag1 + " Flag2 : " + flag2);

                    if (itemID == "FFFE") //Nothing
                    {
                        btn.setup("", 0xFFFE, 0x0, "", "00", "00");
                        continue;
                    }
                    else if (itemID == "16A2") //Recipe
                    {
                        btn.setup(GetNameFromID(recipeData, recipeSource), 0x16A2, Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(recipeData, recipeSource), "", flag1, flag2);
                        continue;
                    }
                    else if (itemID == "1095") //Delivery
                    {
                        btn.setup(GetNameFromID(recipeData, itemSource), 0x1095, Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(recipeData, itemSource, Convert.ToUInt32("0x" + itemData, 16)), "", flag1, flag2);
                        continue;
                    }
                    else if (itemID == "16A1") //Bottle Message
                    {
                        btn.setup(GetNameFromID(recipeData, recipeSource), 0x16A1, Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(recipeData, recipeSource), "", flag1, flag2);
                        continue;
                    }
                    else if (itemID == "0A13") // Fossil
                    {
                        btn.setup(GetNameFromID(recipeData, itemSource), 0x0A13, Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(recipeData, itemSource), "", flag1, flag2);
                        continue;
                    }
                    else if (itemID == "315A" || itemID == "1618") // Wall-Mounted
                    {
                        btn.setup(GetNameFromID(itemID, itemSource), Convert.ToUInt16("0x" + itemID, 16), Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(itemID, itemSource, Convert.ToUInt32("0x" + itemData, 16)), GetImagePathFromID(recipeData, itemSource), flag1, flag2);
                        continue;
                    }
                    else
                    {
                        btn.setup(GetNameFromID(itemID, itemSource), Convert.ToUInt16("0x" + itemID, 16), Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(itemID, itemSource, Convert.ToUInt32("0x" + itemData, 16)), "", flag1, flag2);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.logEvent("MainForm", "UpdateInventory: " + ex.Message.ToString());
                Invoke((MethodInvoker)delegate { this.autoRefreshCheckBox.Checked = false; });
                refreshTimer.Stop();
                myMessageBox.Show(ex.Message.ToString(), "This seems like a bad idea but it's fine for now.");
                return true;
            }
            allowUpdate = true;
            return false;
        }
        #endregion

        public IEnumerable<T> FindControls<T>(Control control) where T : Control
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => FindControls<T>(ctrl))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == typeof(T)).Cast<T>();
        }

        #region Button Click
        private void customIdBtn_Click(object sender, EventArgs e)
        {
            if (customIdTextbox.Text == "")
            {
                MessageBox.Show("Please enter an ID before sending item");
                return;
            }

            /*
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }
            */

            if (customAmountTxt.Text == "")
            {
                MessageBox.Show("Please enter an amount");
                return;
            }

            if (selectedButton == null)
            {
                MessageBox.Show("Please select a slot");
                return;
            }

            string hexValue = "0";
            if (hexModeBtn.Tag.ToString() == "Normal")
            {
                int decValue = int.Parse(customAmountTxt.Text) - 1;
                if (decValue >= 0)
                    hexValue = decValue.ToString("X");
            }

            try
            {
                if (customIdTextbox.Text == "16A2") //recipe
                {
                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        if (!offline)
                            Utilities.SpawnItem(s, bot, selectedSlot, selectedItem.getFlag1() + selectedItem.getFlag2() + customIdTextbox.Text, Utilities.precedingZeros(hexValue, 8));
                        selectedButton.setup(GetNameFromID(Utilities.turn2bytes(hexValue), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(Utilities.turn2bytes(hexValue), recipeSource), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                    else
                    {
                        if (!offline)
                            Utilities.SpawnItem(s, bot, selectedSlot, selectedItem.getFlag1() + selectedItem.getFlag2() + customIdTextbox.Text, customAmountTxt.Text);
                        selectedButton.setup(GetNameFromID(Utilities.turn2bytes(customAmountTxt.Text), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(Utilities.turn2bytes(customAmountTxt.Text), recipeSource), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                }
                else if (customIdTextbox.Text == "315A" || customIdTextbox.Text == "1618") // Wall-Mounted
                {
                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        if (!offline)
                            Utilities.SpawnItem(s, bot, selectedSlot, selectedItem.getFlag1() + selectedItem.getFlag2() + customIdTextbox.Text, Utilities.precedingZeros(hexValue, 8));
                        selectedButton.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + hexValue, 16)), GetImagePathFromID((Utilities.turn2bytes(hexValue)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                    else
                    {
                        if (!offline)
                            Utilities.SpawnItem(s, bot, selectedSlot, selectedItem.getFlag1() + selectedItem.getFlag2() + customIdTextbox.Text, customAmountTxt.Text);
                        selectedButton.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), GetImagePathFromID((Utilities.turn2bytes(customAmountTxt.Text)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                }
                else
                {
                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        if (!offline)
                            Utilities.SpawnItem(s, bot, selectedSlot, selectedItem.getFlag1() + selectedItem.getFlag2() + customIdTextbox.Text, Utilities.precedingZeros(hexValue, 8));
                        selectedButton.setup(GetNameFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + hexValue, 16)), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                    else
                    {
                        if (!offline)
                            Utilities.SpawnItem(s, bot, selectedSlot, selectedItem.getFlag1() + selectedItem.getFlag2() + customIdTextbox.Text, customAmountTxt.Text);
                        selectedButton.setup(GetNameFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.logEvent("MainForm", "SpawnItem: " + ex.Message.ToString());
                myMessageBox.Show(ex.Message.ToString(), "FIXME: This doesn't account for children of hierarchy... too bad!");
            }

            this.ShowMessage(customIdTextbox.Text);
        }
        private void deleteItemBtn_Click(object sender, EventArgs e)
        {
            /*
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }
            */

            ToolStripItem item = (sender as ToolStripItem);
            if (item != null)
            {
                if (item.Owner is ContextMenuStrip owner)
                {
                    if (!offline)
                    {
                        int slotId = int.Parse(owner.SourceControl.Tag.ToString());
                        try
                        {
                            Utilities.DeleteSlot(s, bot, slotId);
                        }
                        catch (Exception ex)
                        {
                            Log.logEvent("MainForm", "DeleteItemRightClick: " + ex.Message.ToString());
                            myMessageBox.Show(ex.Message.ToString(), "Bizarre vector flip inherited from earlier code, WTF?");
                        }
                    }

                    var btnParent = (inventorySlot)owner.SourceControl;
                    btnParent.reset();
                    btnToolTip.RemoveAll();
                    if (sound)
                        System.Media.SystemSounds.Asterisk.Play();
                }
            }
        }
        private void spawnAllBtn_Click(object sender, EventArgs e)
        {
            if (customIdTextbox.Text == "")
            {
                MessageBox.Show("Please enter an ID before sending item");
                return;
            }

            /*
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }
            */

            if (customAmountTxt.Text == "")
            {
                MessageBox.Show("Please enter an amount");
                return;
            }

            string itemID = selectedItem.getFlag1() + selectedItem.getFlag2() + customIdTextbox.Text;

            if (hexModeBtn.Tag.ToString() == "Normal")
            {
                int decValue = int.Parse(customAmountTxt.Text) - 1;
                string itemAmount;
                if (decValue < 0)
                    itemAmount = "0";
                else
                    itemAmount = decValue.ToString("X");
                Thread spawnAllThread = new Thread(delegate () { spawnAll(itemID, itemAmount); });
                spawnAllThread.Start();
            }
            else
            {
                string itemAmount = customAmountTxt.Text;
                Thread spawnAllThread = new Thread(delegate () { spawnAll(itemID, itemAmount); });
                spawnAllThread.Start();
            }
            this.ShowMessage(customIdTextbox.Text);
        }
        private void spawnAll(string itemID, string itemAmount)
        {
            showWait();

            if (!offline)
            {
                byte[] b = new byte[160];
                byte[] ID = Utilities.stringToByte(Utilities.flip(Utilities.precedingZeros(itemID, 8)));
                byte[] Data = Utilities.stringToByte(Utilities.flip(Utilities.precedingZeros(itemAmount, 8)));

                //Debug.Print(Utilities.precedingZeros(itemID, 8));
                //Debug.Print(Utilities.precedingZeros(itemAmount, 8));

                for (int i = 0; i < b.Length; i += 8)
                {
                    for (int j = 0; j < 4; j++)
                    {
                        b[i + j] = ID[j];
                        b[i + j + 4] = Data[j];
                    }
                }

                //string result = Encoding.ASCII.GetString(Utilities.transform(b));
                //Debug.Print(result);
                try
                {
                    Utilities.OverwriteAll(s, bot, b, b, ref counter);
                }
                catch (Exception ex)
                {
                    Log.logEvent("MainForm", "SpawnAll: " + ex.Message.ToString());
                    myMessageBox.Show(ex.Message.ToString(), "Multithreading badness. This will cause a crash later!");
                }

                foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                {
                    Invoke((MethodInvoker)delegate
                    {
                        if (Utilities.turn2bytes(itemID) == "16A2") //Recipe
                        {
                            btn.setup(GetNameFromID(Utilities.turn2bytes(itemAmount), recipeSource), 0x16A2, Convert.ToUInt32("0x" + itemAmount, 16), GetImagePathFromID(Utilities.turn2bytes(itemAmount), recipeSource), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                        }
                        else
                        {
                            btn.setup(GetNameFromID(Utilities.turn2bytes(itemID), itemSource), Convert.ToUInt16("0x" + Utilities.turn2bytes(itemID), 16), Convert.ToUInt32("0x" + itemAmount, 16), GetImagePathFromID(Utilities.turn2bytes(itemID), itemSource, Convert.ToUInt32("0x" + itemAmount, 16)), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                        }
                    });
                }

                Thread.Sleep(1000);
            }
            else
            {
                foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                {
                    Invoke((MethodInvoker)delegate
                    {
                        if (Utilities.turn2bytes(itemID) == "16A2") //Recipe
                        {
                            btn.setup(GetNameFromID(Utilities.turn2bytes(itemAmount), recipeSource), 0x16A2, Convert.ToUInt32("0x" + itemAmount, 16), GetImagePathFromID(Utilities.turn2bytes(itemAmount), recipeSource), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                        }
                        else
                        {
                            btn.setup(GetNameFromID(Utilities.turn2bytes(itemID), itemSource), Convert.ToUInt16("0x" + Utilities.turn2bytes(itemID), 16), Convert.ToUInt32("0x" + itemAmount, 16), GetImagePathFromID(Utilities.turn2bytes(itemID), itemSource, Convert.ToUInt32("0x" + itemAmount, 16)), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                        }
                    });
                }
            }
            if (sound)
                System.Media.SystemSounds.Asterisk.Play();
            hideWait();
        }
        private void fillRemainBtn_Click(object sender, EventArgs e)
        {
            if (customIdTextbox.Text == "")
            {
                MessageBox.Show("Please enter an ID before sending item");
                return;
            }

            /*
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }
            */

            if (customAmountTxt.Text == "")
            {
                MessageBox.Show("Please enter an amount");
                return;
            }

            string itemID = customIdTextbox.Text;

            if (hexModeBtn.Tag.ToString() == "Normal")
            {
                int decValue = int.Parse(customAmountTxt.Text) - 1;
                string itemAmount = decValue.ToString("X");
                Thread fillRemainThread = new Thread(delegate () { fillRemain(itemID, itemAmount); });
                fillRemainThread.Start();
            }
            else
            {
                string itemAmount = customAmountTxt.Text;
                Thread fillRemainThread = new Thread(delegate () { fillRemain(itemID, itemAmount); });
                fillRemainThread.Start();
            }
            this.ShowMessage(customIdTextbox.Text);
        }
        private void fillRemain(string itemID, string itemAmount)
        {
            lock (itemLock)
            {
                showWait();

                if (!offline)
                {
                    try
                    {
                        byte[] Bank01to20 = Utilities.GetInventoryBank(s, bot, 1);
                        byte[] Bank21to40 = Utilities.GetInventoryBank(s, bot, 21);

                        foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                        {
                            int slot = int.Parse(btn.Tag.ToString());
                            byte[] slotBytes = new byte[2];

                            int slotOffset;
                            if (slot < 21)
                            {
                                slotOffset = ((slot - 1) * 0x8);
                            }
                            else
                            {
                                slotOffset = ((slot - 21) * 0x8);
                            }

                            if (slot < 21)
                            {
                                Buffer.BlockCopy(Bank01to20, slotOffset, slotBytes, 0x0, 0x2);
                            }
                            else
                            {
                                Buffer.BlockCopy(Bank21to40, slotOffset, slotBytes, 0x0, 0x2);
                            }

                            string slotID = Utilities.flip(Utilities.ByteToHexString(slotBytes));

                            if (slotID == "FFFE")
                            {
                                Utilities.SpawnItem(s, bot, slot, selectedItem.getFlag1() + selectedItem.getFlag2() + itemID, itemAmount);
                                Invoke((MethodInvoker)delegate
                                {
                                    if (itemID == "16A2") //Recipe
                                    {
                                        btn.setup(GetNameFromID(Utilities.turn2bytes(itemAmount), recipeSource), 0x16A2, Convert.ToUInt32("0x" + itemAmount, 16), GetImagePathFromID(Utilities.turn2bytes(itemAmount), recipeSource), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                                    }
                                    else
                                    {
                                        btn.setup(GetNameFromID(itemID, itemSource), Convert.ToUInt16("0x" + itemID, 16), Convert.ToUInt32("0x" + itemAmount, 16), GetImagePathFromID(itemID, itemSource, Convert.ToUInt32("0x" + itemAmount, 16)), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                                    }
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.logEvent("MainForm", "FillRemain: " + ex.Message.ToString());
                        myMessageBox.Show(ex.Message.ToString(), "This code didn't port easily. WTF does it do?");
                    }

                    Thread.Sleep(3000);
                }
                else
                {
                    foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                    {
                        if (btn.fillItemID() == "FFFE")
                        {
                            Invoke((MethodInvoker)delegate
                            {
                                if (itemID == "16A2") //Recipe
                                {
                                    btn.setup(GetNameFromID(Utilities.turn2bytes(itemAmount), recipeSource), 0x16A2, Convert.ToUInt32("0x" + itemAmount, 16), GetImagePathFromID(Utilities.turn2bytes(itemAmount), recipeSource), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                                }
                                else
                                {
                                    btn.setup(GetNameFromID(itemID, itemSource), Convert.ToUInt16("0x" + itemID, 16), Convert.ToUInt32("0x" + itemAmount, 16), GetImagePathFromID(itemID, itemSource, Convert.ToUInt32("0x" + itemAmount, 16)), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                                }
                            });
                        }
                    }
                }
                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
                hideWait();
            }
        }
        private void refreshBtn_Click(object sender, EventArgs e)
        {
            UpdateInventory();
            if (sound)
                System.Media.SystemSounds.Asterisk.Play();
        }
        private void clearBtn_Click(object sender, EventArgs e)
        {
            /*
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }
            */

            Thread clearThread = new Thread(clearInventory);
            clearThread.Start();
        }
        private void clearInventory()
        {
            showWait();

            try
            {
                if (!offline)
                {
                    byte[] b = new byte[160];

                    //Debug.Print(Utilities.precedingZeros(itemID, 8));
                    //Debug.Print(Utilities.precedingZeros(itemAmount, 8));

                    for (int i = 0; i < b.Length; i += 8)
                    {
                        b[i] = 0xFE;
                        b[i + 1] = 0xFF;
                        for (int j = 0; j < 6; j++)
                        {
                            b[i + 2 + j] = 0x00;
                        }
                    }

                    Utilities.OverwriteAll(s, bot, b, b, ref counter);
                    //string result = Encoding.ASCII.GetString(Utilities.transform(b));
                    //Debug.Print(result);

                    foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            btn.reset();
                        });
                    }
                    Invoke((MethodInvoker)delegate
                    {
                        btnToolTip.RemoveAll();
                    });
                    Thread.Sleep(1000);
                }
                else
                {
                    foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            btn.reset();
                        });
                    }
                    Invoke((MethodInvoker)delegate
                    {
                        btnToolTip.RemoveAll();
                    });
                }
            }
            catch (Exception ex)
            {
                Log.logEvent("MainForm", "ClearInventory: " + ex.Message.ToString());
                myMessageBox.Show(ex.Message.ToString(), "This is catastrophically bad, don't do this. Someone needs to fix this.");
            }
            if (sound)
                System.Media.SystemSounds.Asterisk.Play();
            hideWait();
        }

        private void spawnRecipeBtn_Click(object sender, EventArgs e)
        {
            if (recipeNum.Text == "")
            {
                MessageBox.Show("Please enter a recipe ID before sending item");
                return;
            }

            /*
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }
            */

            if (selectedButton == null)
            {
                MessageBox.Show("Please select a slot");
                return;
            }

            if (!offline)
                Utilities.SpawnRecipe(s, bot, selectedSlot, "16A2", Utilities.turn2bytes(recipeNum.Text));

            this.ShowMessage(Utilities.turn2bytes(recipeNum.Text));

            selectedButton.setup(GetNameFromID(Utilities.turn2bytes(recipeNum.Text), recipeSource), 0x16A2, Convert.ToUInt32("0x" + recipeNum.Text, 16), GetImagePathFromID(Utilities.turn2bytes(recipeNum.Text), recipeSource));
        }

        private void deleteBtn_Click(object sender, KeyEventArgs e)
        {
            if (selectedButton == null)
            {
                MessageBox.Show("Please select a slot");
                return;
            }

            if (!offline)
            {
                try
                {
                    Utilities.DeleteSlot(s, bot, int.Parse(selectedButton.Tag.ToString()));
                }
                catch (Exception ex)
                {
                    Log.logEvent("MainForm", "DeleteItemKeyBoard: " + ex.Message.ToString());
                    myMessageBox.Show(ex.Message.ToString(), "Because nobody could *ever* possible attempt to parse bad data.");
                }
            }
            selectedButton.reset();
            btnToolTip.RemoveAll();

            //UpdateInventory();
            if (sound)
                System.Media.SystemSounds.Asterisk.Play();
        }

        private void spawnFlowerBtn_Click(object sender, EventArgs e)
        {
            if (flowerID.Text == "")
            {
                MessageBox.Show("Please select a flower");
                return;
            }

            /*
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }
            */

            if (selectedButton == null)
            {
                MessageBox.Show("Please select a slot");
                return;
            }

            if (!offline)
                Utilities.SpawnFlower(s, bot, selectedSlot, flowerID.Text, flowerValue.Text);

            this.ShowMessage(flowerID.Text);

            selectedButton.setup(GetNameFromID(flowerID.Text, itemSource), Convert.ToUInt16("0x" + flowerID.Text, 16), Convert.ToUInt32("0x" + flowerValue.Text, 16), GetImagePathFromID(flowerID.Text, itemSource));

        }

        private void inventory_MouseDown(object sender, MouseEventArgs e)
        {
            var button = (inventorySlot)sender;

            foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
            {
                if (btn.Tag == null)
                    continue;
                //btn.FlatAppearance.BorderSize = 0;
                btn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            }

            button.FlatStyle = FlatStyle.Flat;
            //button.FlatAppearance.BorderColor = System.Drawing.Color.LightSeaGreen;
            button.BackColor = System.Drawing.Color.LightSeaGreen;
            selectedButton = button;
            selectedSlot = int.Parse(button.Tag.ToString());

            if (Control.ModifierKeys == Keys.Control)
            {
                if (currentPanel == itemModePanel)
                {
                    customIdBtn_Click(sender, e);
                }
                else if (currentPanel == recipeModePanel)
                {
                    spawnRecipeBtn_Click(sender, e);
                }
                else if (currentPanel == flowerModePanel)
                {
                    spawnFlowerBtn_Click(sender, e);
                }

                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
            }
            else if (Control.ModifierKeys == Keys.Alt)
            {
                deleteBtn_Click(sender, null);
            }
        }
        #endregion

        #region Key Press Check
        private void customIdTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
            {
                e.Handled = true;
            }
            if (c >= 'a' && c <= 'f') e.KeyChar = char.ToUpper(c);
        }

        private void customAmountTxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (hexModeBtn.Tag.ToString() == "Normal")
            {
                if (!((c >= '0' && c <= '9')))
                {
                    e.Handled = true;
                }
            }
            else
            {
                if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
                {
                    e.Handled = true;
                }
                if (c >= 'a' && c <= 'f') e.KeyChar = char.ToUpper(c);
            }
        }

        private void HexTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
            {
                e.Handled = true;
            }
            if (c >= 'a' && c <= 'f') e.KeyChar = char.ToUpper(c);
        }

        private void DecTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (!((c >= '0' && c <= '9')))
            {
                e.Handled = true;
            }
        }

        #endregion

        #region Search Box
        private void itemSearchBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (itemGridView.DataSource != null)
                    (itemGridView.DataSource as DataTable).DefaultView.RowFilter = string.Format(languageSetting + " LIKE '%{0}%'", EscapeLikeValue(itemSearchBox.Text));
                if (recipeGridView.DataSource != null)
                    (recipeGridView.DataSource as DataTable).DefaultView.RowFilter = string.Format(languageSetting + " LIKE '%{0}%'", EscapeLikeValue(itemSearchBox.Text));
                if (flowerGridView.DataSource != null)
                    (flowerGridView.DataSource as DataTable).DefaultView.RowFilter = string.Format(languageSetting + " LIKE '%{0}%'", EscapeLikeValue(itemSearchBox.Text));
                if (favGridView.DataSource != null)
                    (favGridView.DataSource as DataTable).DefaultView.RowFilter = string.Format("Name" + " LIKE '%{0}%'", EscapeLikeValue(itemSearchBox.Text));
            }
            catch
            {
                itemSearchBox.Clear();
            }
        }

        public static string EscapeLikeValue(string valueWithoutWildcards)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < valueWithoutWildcards.Length; i++)
            {
                char c = valueWithoutWildcards[i];
                if (c == '*' || c == '%' || c == '[' || c == ']')
                    sb.Append("[").Append(c).Append("]");
                else if (c == '\'')
                    sb.Append("''");
                else
                    sb.Append(c);
            }
            return sb.ToString();
        }

        private void itemSearchBox_Click(object sender, EventArgs e)
        {
            if (itemSearchBox.Text == "Search")
            {
                itemSearchBox.Text = "";
                itemSearchBox.ForeColor = Color.White;
            }
        }
        #endregion

        #region GridView Add Image / Click
        private void itemGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < this.itemGridView.Rows.Count)
            {
                if (e.ColumnIndex == 13)
                {
                    //itemGridView.Rows[e.RowIndex].Height = 128;
                    //itemGridView.Columns[13].Width = 128;
                    string path;
                    string imageName = itemGridView.Rows[e.RowIndex].Cells["iName"].Value.ToString();

                    if (OverrideDict.ContainsKey(imageName))
                    {
                        path = Utilities.imagePath + OverrideDict[imageName] + ".png";
                        if (File.Exists(path))
                        {
                            Image img = Image.FromFile(path);
                            //e.CellStyle.BackColor = Color.Green;
                            e.Value = img;

                            return;
                        }
                    }

                    path = Utilities.imagePath + imageName + ".png";
                    if (File.Exists(path))
                    {
                        Image img = Image.FromFile(path);
                        e.Value = img;
                    }
                    else
                    {
                        path = Utilities.imagePath + imageName + "_Remake_0_0.png";
                        if (File.Exists(path))
                        {
                            Image img = Image.FromFile(path);
                            e.CellStyle.BackColor = Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(77)))), ((int)(((byte)(162)))));
                            e.Value = img;
                        }
                        else
                        {
                            path = Utilities.imagePath + removeNumber(imageName) + ".png";
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

        private void itemGridView_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
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
                    lastRow = itemGridView.Rows[e.RowIndex];
                    itemGridView.Rows[e.RowIndex].Height = 128;
                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        if (customAmountTxt.Text == "" || customAmountTxt.Text == "0")
                        {
                            customAmountTxt.Text = "1";
                        }
                    }
                    else
                    {
                        hexMode_Click(sender, e);
                        customAmountTxt.Text = "1";
                    }

                    string id = itemGridView.Rows[e.RowIndex].Cells["id"].Value.ToString();
                    string name = itemGridView.Rows[e.RowIndex].Cells[languageSetting].Value.ToString();

                    customIdTextbox.Text = id;

                    selectedItem.setup(name, Convert.ToUInt16("0x" + id, 16), 0x0, GetImagePathFromID(id, itemSource), true, "");
                    if (selection != null)
                    {
                        selection.receiveID(Utilities.precedingZeros(selectedItem.fillItemID(), 4), languageSetting);
                    }
                    updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
                }
            }
            else if (me.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (lastRow != null)
                {
                    lastRow.Height = 22;
                }

                if (e.RowIndex > -1)
                {
                    lastRow = itemGridView.Rows[e.RowIndex];
                    itemGridView.Rows[e.RowIndex].Height = 128;

                    string name = selectedItem.displayItemName();
                    string id = selectedItem.displayItemID();
                    string path = selectedItem.getPath();

                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        hexMode_Click(sender, e);
                    }
                    else
                    {

                    }

                    if (customIdTextbox.Text != "")
                    {
                        if (customIdTextbox.Text == "315A" || customIdTextbox.Text == "1618") // Wall-Mounted
                        {
                            customAmountTxt.Text = Utilities.precedingZeros("00" + itemGridView.Rows[e.RowIndex].Cells["id"].Value.ToString(), 8);
                            selectedItem.setup(name, Convert.ToUInt16(id, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), path, true, GetImagePathFromID(itemGridView.Rows[e.RowIndex].Cells["id"].Value.ToString(), itemSource));
                        }
                        else
                        {
                            customAmountTxt.Text = Utilities.precedingZeros(itemGridView.Rows[e.RowIndex].Cells["id"].Value.ToString(), 8);
                            selectedItem.setup(name, Convert.ToUInt16(id, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), path, true, GetNameFromID(itemGridView.Rows[e.RowIndex].Cells["id"].Value.ToString(), itemSource));
                        }

                        if (selection != null)
                        {
                            selection.receiveID(Utilities.turn2bytes(selectedItem.fillItemData()), languageSetting);
                        }

                        updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
                    }

                }
            }
        }

        private void recipeGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < this.recipeGridView.Rows.Count)
            {
                if (e.ColumnIndex == 13)
                {
                    string imageName = recipeGridView.Rows[e.RowIndex].Cells["iName"].Value.ToString();
                    string path;

                    if (OverrideDict.ContainsKey(imageName))
                    {
                        path = Utilities.imagePath + OverrideDict[imageName] + ".png";
                        if (File.Exists(path))
                        {
                            Image img = Image.FromFile(path);
                            //e.CellStyle.BackColor = Color.Green;
                            e.Value = img;

                            return;
                        }
                    }

                    path = Utilities.imagePath + imageName + ".png";
                    if (File.Exists(path))
                    {
                        Image img = Image.FromFile(path);
                        e.Value = img;
                    }
                    else
                    {
                        path = Utilities.imagePath + imageName + "_Remake_0_0.png";
                        if (File.Exists(path))
                        {
                            Image img = Image.FromFile(path);
                            e.CellStyle.BackColor = Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(77)))), ((int)(((byte)(162)))));
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

        private void recipeGridView_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (recipelastRow != null)
            {
                recipelastRow.Height = 22;
            }

            if (e.RowIndex > -1)
            {
                recipelastRow = recipeGridView.Rows[e.RowIndex];
                recipeGridView.Rows[e.RowIndex].Height = 128;
                recipeNum.Text = recipeGridView.Rows[e.RowIndex].Cells["id"].Value.ToString();

                selectedItem.setup(recipeGridView.Rows[e.RowIndex].Cells[languageSetting].Value.ToString(), 0x16A2, Convert.ToUInt32("0x" + recipeGridView.Rows[e.RowIndex].Cells["id"].Value.ToString(), 16), GetImagePathFromID(recipeGridView.Rows[e.RowIndex].Cells["id"].Value.ToString(), recipeSource), true);
                updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
            }
        }

        private void flowerGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < this.flowerGridView.Rows.Count)
            {
                if (e.ColumnIndex == 13)
                {
                    string imageName = flowerGridView.Rows[e.RowIndex].Cells["iName"].Value.ToString();

                    if (OverrideDict.ContainsKey(imageName))
                    {
                        string path = Utilities.imagePath + OverrideDict[imageName] + ".png";
                        if (File.Exists(path))
                        {
                            Image img = Image.FromFile(path);
                            //e.CellStyle.BackColor = Color.Green;
                            e.Value = img;

                            return;
                        }
                    }
                    else
                    {
                        e.CellStyle.BackColor = Color.Red;
                    }
                }
            }
        }

        private void flowerGridView_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (flowerlastRow != null)
            {
                flowerlastRow.Height = 22;
            }
            if (e.RowIndex > -1)
            {
                flowerlastRow = flowerGridView.Rows[e.RowIndex];
                flowerGridView.Rows[e.RowIndex].Height = 128;
                flowerID.Text = flowerGridView.Rows[e.RowIndex].Cells["id"].Value.ToString();
                flowerValue.Text = flowerGridView.Rows[e.RowIndex].Cells["value"].Value.ToString();

                selectedItem.setup(flowerGridView.Rows[e.RowIndex].Cells[languageSetting].Value.ToString(), Convert.ToUInt16("0x" + flowerGridView.Rows[e.RowIndex].Cells["id"].Value.ToString(), 16), Convert.ToUInt32("0x" + flowerGridView.Rows[e.RowIndex].Cells["value"].Value.ToString(), 16), GetImagePathFromID(flowerGridView.Rows[e.RowIndex].Cells["id"].Value.ToString(), itemSource), true);
                updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
            }
        }

        private void favGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < this.favGridView.Rows.Count)
            {
                if (e.ColumnIndex == 4)
                {
                    string imageName = favGridView.Rows[e.RowIndex].Cells["iName"].Value.ToString();
                    string path;

                    if (OverrideDict.ContainsKey(imageName))
                    {
                        path = Utilities.imagePath + OverrideDict[imageName] + ".png";
                        if (File.Exists(path))
                        {
                            Image img = Image.FromFile(path);
                            //e.CellStyle.BackColor = Color.Green;
                            e.Value = img;

                            return;
                        }
                    }

                    path = Utilities.imagePath + imageName + ".png";
                    if (File.Exists(path))
                    {
                        Image img = Image.FromFile(path);
                        e.Value = img;
                    }
                    else
                    {
                        path = Utilities.imagePath + imageName + "_Remake_0_0.png";
                        if (File.Exists(path))
                        {
                            Image img = Image.FromFile(path);
                            e.CellStyle.BackColor = Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(77)))), ((int)(((byte)(162)))));
                            e.Value = img;
                        }
                        else
                        {
                            path = Utilities.imagePath + removeNumber(imageName) + ".png";
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

        private void favGridView_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == System.Windows.Forms.MouseButtons.Left)
            {
                if (favlastRow != null)
                {
                    favlastRow.Height = 22;
                }
                if (e.RowIndex > -1)
                {
                    favlastRow = favGridView.Rows[e.RowIndex];
                    favGridView.Rows[e.RowIndex].Height = 128;
                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        hexMode_Click(sender, e);
                    }

                    string id = favGridView.Rows[e.RowIndex].Cells["id"].Value.ToString();
                    string iName = favGridView.Rows[e.RowIndex].Cells["iName"].Value.ToString();
                    string name = favGridView.Rows[e.RowIndex].Cells["Name"].Value.ToString();
                    string data = favGridView.Rows[e.RowIndex].Cells["value"].Value.ToString();

                    customIdTextbox.Text = Utilities.precedingZeros(id, 4);
                    customAmountTxt.Text = Utilities.precedingZeros(data, 8);

                    selectedItem.setup(name, Convert.ToUInt16("0x" + id, 16), Convert.ToUInt32("0x" + data, 16), GetImagePathFromID(id, itemSource, Convert.ToUInt32("0x" + data, 16)), true, "");
                    if (selection != null)
                    {
                        selection.receiveID(Utilities.precedingZeros(selectedItem.fillItemID(), 4), languageSetting);
                    }
                    updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
                }
            }
        }

        #endregion

        #region Progessbar
        private void showWait()
        {
            if (InvokeRequired)
            {
                MethodInvoker method = new MethodInvoker(showWait);
                Invoke(method);
                return;
            }
            waitMsg.Visible = true;
            pacman2.Visible = true;

            itemModePanel.Visible = false;
            recipeModePanel.Visible = false;
            flowerModePanel.Visible = false;

            itemModeBtn.Visible = false;
            recipeModeBtn.Visible = false;
            flowerModeBtn.Visible = false;

            selectedItem.Visible = false;
            selectedItemName.Visible = false;
        }

        private void hideWait()
        {
            if (InvokeRequired)
            {
                MethodInvoker method = new MethodInvoker(hideWait);
                Invoke(method);
                return;
            }

            waitMsg.Visible = false;
            pacman2.Visible = false;
            currentPanel.Visible = true;

            itemModeBtn.Visible = true;
            recipeModeBtn.Visible = true;
            flowerModeBtn.Visible = true;

            selectedItem.Visible = true;
            selectedItemName.Visible = true;
        }
        #endregion

        #region Inventory Selector
        private void inventorySelector_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Debug.Print(inventorySelector.SelectedIndex.ToString());
            if (playerSelectorInventory.SelectedIndex < 0)
                return;

            switch (playerSelectorInventory.SelectedIndex)
            {
                case 0:
                    Utilities.setAddress(1);
                    maxPage = 1;
                    currentPage = 1;
                    hidePagination();
                    UpdateInventory();
                    break;
                case 1:
                    Utilities.setAddress(2);
                    maxPage = 1;
                    currentPage = 1;
                    hidePagination();
                    UpdateInventory();
                    break;
                case 2:
                    Utilities.setAddress(3);
                    maxPage = 1;
                    currentPage = 1;
                    hidePagination();
                    UpdateInventory();
                    break;
                case 3:
                    Utilities.setAddress(4);
                    maxPage = 1;
                    currentPage = 1;
                    hidePagination();
                    UpdateInventory();
                    break;
                case 4:
                    Utilities.setAddress(5);
                    maxPage = 1;
                    currentPage = 1;
                    hidePagination();
                    UpdateInventory();
                    break;
                case 5:
                    Utilities.setAddress(6);
                    maxPage = 1;
                    currentPage = 1;
                    hidePagination();
                    UpdateInventory();
                    break;
                case 6:
                    Utilities.setAddress(7);
                    maxPage = 1;
                    currentPage = 1;
                    hidePagination();
                    UpdateInventory();
                    break;
                case 7:
                    Utilities.setAddress(8);
                    maxPage = 1;
                    currentPage = 1;
                    hidePagination();
                    UpdateInventory();
                    break;
                case 8: // House
                    Utilities.setAddress(11);
                    maxPage = 60;
                    currentPage = 1;
                    showPagination();
                    UpdateInventory();
                    break;
                case 9:
                    Utilities.setAddress(12);
                    maxPage = 60;
                    currentPage = 1;
                    showPagination();
                    UpdateInventory();
                    break;
                case 10:
                    Utilities.setAddress(13);
                    maxPage = 60;
                    currentPage = 1;
                    showPagination();
                    UpdateInventory();
                    break;
                case 11:
                    Utilities.setAddress(14);
                    maxPage = 60;
                    currentPage = 1;
                    showPagination();
                    UpdateInventory();
                    break;
                case 12:
                    Utilities.setAddress(15);
                    maxPage = 60;
                    currentPage = 1;
                    showPagination();
                    UpdateInventory();
                    break;
                case 13:
                    Utilities.setAddress(16);
                    maxPage = 60;
                    currentPage = 1;
                    showPagination();
                    UpdateInventory();
                    break;
                case 14:
                    Utilities.setAddress(17);
                    maxPage = 60;
                    currentPage = 1;
                    showPagination();
                    UpdateInventory();
                    break;
                case 15:
                    Utilities.setAddress(18);
                    maxPage = 60;
                    currentPage = 1;
                    showPagination();
                    UpdateInventory();
                    break;
                case 16: // Re
                    Utilities.setAddress(9);
                    maxPage = 2;
                    currentPage = 1;
                    showPagination();
                    UpdateInventory();
                    break;
            }
            if (sound)
                System.Media.SystemSounds.Asterisk.Play();
        }

        #endregion

        #region Mode Button
        private void itemModeBtn_Click(object sender, EventArgs e)
        {
            this.itemModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            this.recipeModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.flowerModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.favModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            this.recipeModePanel.Visible = false;
            this.itemModePanel.Visible = true;
            this.flowerModePanel.Visible = false;

            this.itemGridView.Visible = true;
            this.recipeGridView.Visible = false;
            this.flowerGridView.Visible = false;
            this.favGridView.Visible = false;

            this.variationModeButton.Visible = true;


            string hexValue = "0";
            if (hexModeBtn.Tag.ToString() == "Normal")
            {
                if (customAmountTxt.Text == "")
                    customAmountTxt.Text = "1";
                int decValue = int.Parse(customAmountTxt.Text) - 1;
                if (decValue >= 0)
                    hexValue = decValue.ToString("X");
            }
            else
            {
                if (customAmountTxt.Text == "")
                    customAmountTxt.Text = "00000000";
            }



            if (customIdTextbox.Text != "")
            {
                if (customIdTextbox.Text == "16A2") //recipe
                {
                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        selectedItem.setup(GetNameFromID(Utilities.turn2bytes(hexValue), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(Utilities.turn2bytes(hexValue), recipeSource), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                    else
                    {
                        selectedItem.setup(GetNameFromID(Utilities.turn2bytes(customAmountTxt.Text), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(Utilities.turn2bytes(customAmountTxt.Text), recipeSource), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                }
                else if (customIdTextbox.Text == "315A" || customIdTextbox.Text == "1618") // Wall-Mounted
                {
                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + hexValue, 16)), true, GetImagePathFromID((Utilities.turn2bytes(hexValue)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                    else
                    {
                        selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), true, GetImagePathFromID((Utilities.turn2bytes(customAmountTxt.Text)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                }
                else
                {
                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        selectedItem.setup(GetNameFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + hexValue, 16)), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                    else
                    {
                        selectedItem.setup(GetNameFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                    }

                    if (selection != null)
                    {
                        selection.receiveID(customIdTextbox.Text, languageSetting);
                    }
                }
                updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
            }
            else
            {
                selectedItem.reset();
                selectedItemName.Text = "";
            }

            currentPanel = itemModePanel;

            if (itemSearchBox.Text != "Search")
            {
                itemSearchBox.Clear();
            }
        }

        private void recipeModeBtn_Click(object sender, EventArgs e)
        {
            this.itemModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.recipeModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            this.flowerModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.favModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            this.itemModePanel.Visible = false;
            this.recipeModePanel.Visible = true;
            this.flowerModePanel.Visible = false;
            this.favGridView.Visible = false;

            this.itemGridView.Visible = false;
            this.recipeGridView.Visible = true;
            this.flowerGridView.Visible = false;

            this.variationModeButton.Visible = false;
            closeVariationMenu();

            if (recipeNum.Text != "")
            {
                //Debug.Print(GetNameFromID(recipeNum.Text, recipeSource));
                selectedItem.setup(GetNameFromID(Utilities.turn2bytes(recipeNum.Text), recipeSource), 0x16A2, Convert.ToUInt32("0x" + recipeNum.Text, 16), GetImagePathFromID(Utilities.turn2bytes(recipeNum.Text), recipeSource), true);
                updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
            }
            else
            {
                selectedItem.reset();
                selectedItemName.Text = "";
            }

            currentPanel = recipeModePanel;

            if (itemSearchBox.Text != "Search")
            {
                itemSearchBox.Clear();
            }
        }

        private void flowerModeBtn_Click(object sender, EventArgs e)
        {
            this.itemModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.recipeModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.flowerModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            this.favModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            this.itemModePanel.Visible = false;
            this.recipeModePanel.Visible = false;
            this.flowerModePanel.Visible = true;
            this.favGridView.Visible = false;

            this.itemGridView.Visible = false;
            this.recipeGridView.Visible = false;
            this.flowerGridView.Visible = true;

            this.variationModeButton.Visible = false;
            closeVariationMenu();

            if (flowerID.Text != "")
            {
                selectedItem.setup(GetNameFromID(flowerID.Text, itemSource), Convert.ToUInt16("0x" + flowerID.Text, 16), Convert.ToUInt32("0x" + flowerValue.Text, 16), GetImagePathFromID(flowerID.Text, itemSource), true);
                updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
            }
            else
            {
                selectedItem.reset();
                selectedItemName.Text = "";
            }

            currentPanel = flowerModePanel;

            if (itemSearchBox.Text != "Search")
            {
                itemSearchBox.Clear();
            }
        }

        private void favModeBtn_Click(object sender, EventArgs e)
        {
            this.itemModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.recipeModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.flowerModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.favModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));

            this.recipeModePanel.Visible = false;
            this.itemModePanel.Visible = true;
            this.flowerModePanel.Visible = false;

            this.itemGridView.Visible = false;
            this.recipeGridView.Visible = false;
            this.flowerGridView.Visible = false;
            this.favGridView.Visible = true;

            this.variationModeButton.Visible = true;


            string hexValue = "0";
            if (hexModeBtn.Tag.ToString() == "Normal")
            {
                if (customAmountTxt.Text == "")
                    customAmountTxt.Text = "1";
                int decValue = int.Parse(customAmountTxt.Text) - 1;
                if (decValue >= 0)
                    hexValue = decValue.ToString("X");
            }
            else
            {
                if (customAmountTxt.Text == "")
                    customAmountTxt.Text = "00000000";
            }



            if (customIdTextbox.Text != "")
            {
                if (customIdTextbox.Text == "16A2") //recipe
                {
                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        selectedItem.setup(GetNameFromID(Utilities.turn2bytes(hexValue), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(Utilities.turn2bytes(hexValue), recipeSource), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                    else
                    {
                        selectedItem.setup(GetNameFromID(Utilities.turn2bytes(customAmountTxt.Text), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(Utilities.turn2bytes(customAmountTxt.Text), recipeSource), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                }
                else if (customIdTextbox.Text == "315A" || customIdTextbox.Text == "1618") // Wall-Mounted
                {
                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + hexValue, 16)), true, GetImagePathFromID((Utilities.turn2bytes(hexValue)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                    else
                    {
                        selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), true, GetImagePathFromID((Utilities.turn2bytes(customAmountTxt.Text)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                }
                else
                {
                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        selectedItem.setup(GetNameFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + hexValue, 16)), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                    else
                    {
                        selectedItem.setup(GetNameFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                    }

                    if (selection != null)
                    {
                        selection.receiveID(customIdTextbox.Text, languageSetting);
                    }
                }
                updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
            }
            else
            {
                selectedItem.reset();
                selectedItemName.Text = "";
            }

            currentPanel = itemModePanel;

            if (itemSearchBox.Text != "Search")
            {
                itemSearchBox.Clear();
            }
        }
        #endregion

        #region Keyboard
        public void KeyboardKeyDown(object sender, KeyEventArgs e)
        {
            //Debug.Print(e.KeyCode.ToString());
            if (e.KeyCode.ToString() == "F12" && teleporter != null)
            {
                if (dodoSetup == null)
                {
                    dodoSetup = new dodo(s, this, true)
                    {
                        ControlBox = true,
                        ShowInTaskbar = true
                    };
                    dodoSetup.Show();
                    dodoSetup.WriteLog("[You have started dodo helper in standalone mode.]\n\n" +
                                        "1. Disconnect all controller by selecting \"Controllers\" > \"Change Grip/Order\"\n" +
                                        "2. Leave only the Joy-Con docked on your Switch.\n" +
                                        "3. Return to the game and dock your Switch if needed. Try pressing the buttons below to test the controller.\n" +
                                        "4. If the virtual controller does not response, try the \"Detach\" button on the right, then the \"A\" button.\n" +
                                        "5. If the virtual controller still does not appear, try restart your Switch.\n\n" +
                                        ">> Please try the buttons below to test the virtual controller. <<"
                                        );
                }
            }
            else if (e.KeyCode.ToString() == "F2" || e.KeyCode.ToString() == "Insert")
            {
                if (selectedButton == null & (s != null || bot != null))
                {
                    int firstSlot = findEmpty();
                    if (firstSlot > 0)
                    {
                        selectedSlot = firstSlot;
                        updateSlot(firstSlot);
                    }
                }
                if (currentPanel == itemModePanel)
                {
                    customIdBtn_Click(sender, e);
                }
                else if (currentPanel == recipeModePanel)
                {
                    spawnRecipeBtn_Click(sender, e);
                }
                else if (currentPanel == flowerModePanel)
                {
                    spawnFlowerBtn_Click(sender, e);
                }
                int nextSlot = findEmpty();
                if (nextSlot > 0)
                {
                    selectedSlot = nextSlot;
                    updateSlot(nextSlot);
                }
                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
            }
            else if (e.KeyCode.ToString() == "F1")
            {
                deleteBtn_Click(sender, e);
                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
            }
            else if (e.KeyCode.ToString() == "F3")
            {
                keyboardCopy(sender, e);
                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
            }
            else if (e.KeyCode.ToString() == "End")
            {
                if (itemGridView.CurrentRow == null)
                    return;
                if (currentPanel == itemModePanel)
                {
                    if (itemGridView.Rows.Count <= 0)
                    {
                        return;
                    }

                    if (itemGridView.Rows.Count == 1)
                    {
                        lastRow = itemGridView.Rows[itemGridView.CurrentRow.Index];
                        itemGridView.Rows[itemGridView.CurrentRow.Index].Height = 160;

                        if (hexModeBtn.Tag.ToString() == "Normal")
                        {
                            if (customAmountTxt.Text == "" || customAmountTxt.Text == "0")
                            {
                                customAmountTxt.Text = "1";
                            }
                        }
                        else
                        {
                            hexMode_Click(sender, e);
                            customAmountTxt.Text = "1";
                        }
                        customIdTextbox.Text = itemGridView.Rows[itemGridView.CurrentRow.Index].Cells["id"].Value.ToString();

                        selectedItem.setup(itemGridView.Rows[itemGridView.CurrentRow.Index].Cells[languageSetting].Value.ToString(), Convert.ToUInt16("0x" + itemGridView.Rows[itemGridView.CurrentRow.Index].Cells["id"].Value.ToString(), 16), 0x0, GetImagePathFromID(itemGridView.Rows[itemGridView.CurrentRow.Index].Cells["id"].Value.ToString(), itemSource), true);
                        if (selection != null)
                        {
                            selection.receiveID(Utilities.precedingZeros(selectedItem.fillItemID(), 4), languageSetting);
                        }
                        updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
                    }
                    else if (itemGridView.CurrentRow.Index + 1 < itemGridView.Rows.Count)
                    {
                        if (lastRow != null)
                        {
                            lastRow.Height = 22;
                        }
                        lastRow = itemGridView.Rows[itemGridView.CurrentRow.Index + 1];
                        itemGridView.Rows[itemGridView.CurrentRow.Index + 1].Height = 160;

                        if (hexModeBtn.Tag.ToString() == "Normal")
                        {
                            if (customAmountTxt.Text == "" || customAmountTxt.Text == "0")
                            {
                                customAmountTxt.Text = "1";
                            }
                        }
                        else
                        {
                            hexMode_Click(sender, e);
                            customAmountTxt.Text = "1";
                        }
                        customIdTextbox.Text = itemGridView.Rows[itemGridView.CurrentRow.Index + 1].Cells["id"].Value.ToString();

                        selectedItem.setup(itemGridView.Rows[itemGridView.CurrentRow.Index + 1].Cells[languageSetting].Value.ToString(), Convert.ToUInt16("0x" + itemGridView.Rows[itemGridView.CurrentRow.Index + 1].Cells["id"].Value.ToString(), 16), 0x0, GetImagePathFromID(itemGridView.Rows[itemGridView.CurrentRow.Index + 1].Cells["id"].Value.ToString(), itemSource), true);
                        if (selection != null)
                        {
                            selection.receiveID(Utilities.precedingZeros(selectedItem.fillItemID(), 4), languageSetting);
                        }
                        updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());

                        itemGridView.CurrentCell = itemGridView.Rows[itemGridView.CurrentRow.Index + 1].Cells[languageSetting];

                        //Debug.Print(itemGridView.CurrentRow.Index.ToString());
                    }
                }
                else if (currentPanel == recipeModePanel)
                {
                    if (recipeGridView.Rows.Count <= 0)
                    {
                        return;
                    }

                    if (recipeGridView.Rows.Count == 1)
                    {
                        recipelastRow = recipeGridView.Rows[recipeGridView.CurrentRow.Index];
                        recipeGridView.Rows[recipeGridView.CurrentRow.Index].Height = 160;

                        recipeNum.Text = recipeGridView.Rows[recipeGridView.CurrentRow.Index].Cells["id"].Value.ToString();

                        selectedItem.setup(recipeGridView.Rows[recipeGridView.CurrentRow.Index].Cells[languageSetting].Value.ToString(), 0x16A2, Convert.ToUInt32("0x" + recipeGridView.Rows[recipeGridView.CurrentRow.Index].Cells["id"].Value.ToString(), 16), GetImagePathFromID(recipeGridView.Rows[recipeGridView.CurrentRow.Index].Cells["id"].Value.ToString(), recipeSource), true);
                        updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
                    }
                    else if (recipeGridView.CurrentRow.Index + 1 < recipeGridView.Rows.Count)
                    {
                        if (recipelastRow != null)
                        {
                            recipelastRow.Height = 22;
                        }

                        recipelastRow = recipeGridView.Rows[recipeGridView.CurrentRow.Index + 1];
                        recipeGridView.Rows[recipeGridView.CurrentRow.Index + 1].Height = 160;

                        recipeNum.Text = recipeGridView.Rows[recipeGridView.CurrentRow.Index + 1].Cells["id"].Value.ToString();

                        selectedItem.setup(recipeGridView.Rows[recipeGridView.CurrentRow.Index + 1].Cells[languageSetting].Value.ToString(), 0x16A2, Convert.ToUInt32("0x" + recipeGridView.Rows[recipeGridView.CurrentRow.Index + 1].Cells["id"].Value.ToString(), 16), GetImagePathFromID(recipeGridView.Rows[recipeGridView.CurrentRow.Index + 1].Cells["id"].Value.ToString(), recipeSource), true);
                        updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());

                        recipeGridView.CurrentCell = recipeGridView.Rows[recipeGridView.CurrentRow.Index + 1].Cells[languageSetting];
                    }
                }
                else if (currentPanel == flowerModePanel)
                {

                }
            }
            else if (e.KeyCode.ToString() == "Home")
            {
                if (itemGridView.CurrentRow == null)
                    return;
                if (currentPanel == itemModePanel)
                {
                    if (itemGridView.Rows.Count <= 0)
                    {
                        return;
                    }

                    if (itemGridView.Rows.Count == 1)
                    {
                        lastRow = itemGridView.Rows[itemGridView.CurrentRow.Index];
                        itemGridView.Rows[itemGridView.CurrentRow.Index].Height = 160;

                        if (hexModeBtn.Tag.ToString() == "Normal")
                        {
                            if (customAmountTxt.Text == "" || customAmountTxt.Text == "0")
                            {
                                customAmountTxt.Text = "1";
                            }
                        }
                        else
                        {
                            hexMode_Click(sender, e);
                            customAmountTxt.Text = "1";
                        }
                        customIdTextbox.Text = itemGridView.Rows[itemGridView.CurrentRow.Index].Cells["id"].Value.ToString();

                        selectedItem.setup(itemGridView.Rows[itemGridView.CurrentRow.Index].Cells[languageSetting].Value.ToString(), Convert.ToUInt16("0x" + itemGridView.Rows[itemGridView.CurrentRow.Index].Cells["id"].Value.ToString(), 16), 0x0, GetImagePathFromID(itemGridView.Rows[itemGridView.CurrentRow.Index].Cells["id"].Value.ToString(), itemSource), true);
                        if (selection != null)
                        {
                            selection.receiveID(Utilities.precedingZeros(selectedItem.fillItemID(), 4), languageSetting);
                        }
                        updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
                    }
                    else if (itemGridView.CurrentRow.Index > 0)
                    {
                        if (lastRow != null)
                        {
                            lastRow.Height = 22;
                        }

                        lastRow = itemGridView.Rows[itemGridView.CurrentRow.Index - 1];
                        itemGridView.Rows[itemGridView.CurrentRow.Index - 1].Height = 160;

                        if (hexModeBtn.Tag.ToString() == "Normal")
                        {
                            if (customAmountTxt.Text == "" || customAmountTxt.Text == "0")
                            {
                                customAmountTxt.Text = "1";
                            }
                        }
                        else
                        {
                            hexMode_Click(sender, e);
                            customAmountTxt.Text = "1";
                        }
                        customIdTextbox.Text = itemGridView.Rows[itemGridView.CurrentRow.Index - 1].Cells["id"].Value.ToString();

                        selectedItem.setup(itemGridView.Rows[itemGridView.CurrentRow.Index - 1].Cells[languageSetting].Value.ToString(), Convert.ToUInt16("0x" + itemGridView.Rows[itemGridView.CurrentRow.Index - 1].Cells["id"].Value.ToString(), 16), 0x0, GetImagePathFromID(itemGridView.Rows[itemGridView.CurrentRow.Index - 1].Cells["id"].Value.ToString(), itemSource), true);
                        if (selection != null)
                        {
                            selection.receiveID(Utilities.precedingZeros(selectedItem.fillItemID(), 4), languageSetting);
                        }
                        updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());

                        itemGridView.CurrentCell = itemGridView.Rows[itemGridView.CurrentRow.Index - 1].Cells[languageSetting];
                    }
                }
                else if (currentPanel == recipeModePanel)
                {
                    if (recipeGridView.Rows.Count <= 0)
                    {
                        return;
                    }

                    if (recipeGridView.Rows.Count == 1)
                    {
                        recipelastRow = recipeGridView.Rows[recipeGridView.CurrentRow.Index];
                        recipeGridView.Rows[recipeGridView.CurrentRow.Index].Height = 160;

                        recipeNum.Text = recipeGridView.Rows[recipeGridView.CurrentRow.Index].Cells["id"].Value.ToString();

                        selectedItem.setup(recipeGridView.Rows[recipeGridView.CurrentRow.Index].Cells[languageSetting].Value.ToString(), 0x16A2, Convert.ToUInt32("0x" + recipeGridView.Rows[recipeGridView.CurrentRow.Index].Cells["id"].Value.ToString(), 16), GetImagePathFromID(recipeGridView.Rows[recipeGridView.CurrentRow.Index].Cells["id"].Value.ToString(), recipeSource), true);
                        updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
                    }
                    else if (recipeGridView.CurrentRow.Index > 0)
                    {
                        if (recipelastRow != null)
                        {
                            recipelastRow.Height = 22;
                        }

                        recipelastRow = recipeGridView.Rows[recipeGridView.CurrentRow.Index - 1];
                        recipeGridView.Rows[recipeGridView.CurrentRow.Index - 1].Height = 160;

                        recipeNum.Text = recipeGridView.Rows[recipeGridView.CurrentRow.Index - 1].Cells["id"].Value.ToString();

                        selectedItem.setup(recipeGridView.Rows[recipeGridView.CurrentRow.Index - 1].Cells[languageSetting].Value.ToString(), 0x16A2, Convert.ToUInt32("0x" + recipeGridView.Rows[recipeGridView.CurrentRow.Index - 1].Cells["id"].Value.ToString(), 16), GetImagePathFromID(recipeGridView.Rows[recipeGridView.CurrentRow.Index - 1].Cells["id"].Value.ToString(), recipeSource), true);
                        updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());

                        recipeGridView.CurrentCell = recipeGridView.Rows[recipeGridView.CurrentRow.Index - 1].Cells[languageSetting];
                    }
                }
                else if (currentPanel == flowerModePanel)
                {

                }
            }
            else if (Control.ModifierKeys == Keys.Control)
            {
                string code = e.KeyCode.ToString();
                string pattern = @"(D)([0-9])";
                Regex regex = new Regex(pattern);
                Match match = regex.Match(code);
                if (match.Success)
                {
                    Debug.Print("Start Teleport");
                    int num = int.Parse(code.Replace("D", ""));
                    if (num - 1 < 0)
                        teleport.TeleportTo(9);
                    else
                        teleport.TeleportTo(num - 1);
                }
            }
        }

        private void keyboardCopy(object sender, KeyEventArgs e)
        {
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            itemModeBtn_Click(sender, e);
            if (hexModeBtn.Tag.ToString() == "Normal")
            {
                hexMode_Click(sender, e);
            }
            selectedItem.setup(selectedButton);
            if (selection != null)
            {
                selection.receiveID(Utilities.precedingZeros(selectedItem.fillItemID(), 4), languageSetting);
            }
            updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
            customAmountTxt.Text = Utilities.precedingZeros(selectedItem.fillItemData(), 8);
            customIdTextbox.Text = Utilities.precedingZeros(selectedItem.fillItemID(), 4);
        }

        private void customIdTextbox_KeyUp(object sender, KeyEventArgs e)
        {
            if (customIdTextbox.Text == "" | customAmountTxt.Text == "")
                return;

            string hexValue = "0";
            if (hexModeBtn.Tag.ToString() == "Normal")
            {
                int decValue = int.Parse(customAmountTxt.Text) - 1;
                if (decValue >= 0)
                    hexValue = decValue.ToString("X");
            }


            if (customIdTextbox.Text == "16A2") //recipe
            {
                if (hexModeBtn.Tag.ToString() == "Normal")
                {
                    selectedItem.setup(GetNameFromID(Utilities.turn2bytes(hexValue), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(Utilities.turn2bytes(hexValue), recipeSource), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else
                {
                    selectedItem.setup(GetNameFromID(Utilities.turn2bytes(customAmountTxt.Text), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(Utilities.turn2bytes(customAmountTxt.Text), recipeSource), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
            }
            else if (customIdTextbox.Text == "315A" || customIdTextbox.Text == "1618") // Wall-Mounted
            {
                if (hexModeBtn.Tag.ToString() == "Normal")
                {
                    selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + hexValue, 16)), true, GetImagePathFromID((Utilities.turn2bytes(hexValue)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else
                {
                    selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), true, GetImagePathFromID((Utilities.turn2bytes(customAmountTxt.Text)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                }
            }
            else
            {
                if (hexModeBtn.Tag.ToString() == "Normal")
                {
                    selectedItem.setup(GetNameFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + hexValue, 16)), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else
                {
                    selectedItem.setup(GetNameFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }

                if (selection != null)
                {
                    selection.receiveID(customIdTextbox.Text, languageSetting);
                }
            }
            updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
        }

        private void customAmountTxt_KeyUp(object sender, KeyEventArgs e)
        {
            if (customIdTextbox.Text == "" | customAmountTxt.Text == "")
                return;

            string hexValue = "0";
            if (hexModeBtn.Tag.ToString() == "Normal")
            {
                int decValue = int.Parse(customAmountTxt.Text) - 1;
                if (decValue >= 0)
                    hexValue = decValue.ToString("X");
            }

            if (customIdTextbox.Text == "16A2") //recipe
            {
                if (hexModeBtn.Tag.ToString() == "Normal")
                {
                    selectedItem.setup(GetNameFromID(Utilities.turn2bytes(hexValue), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(Utilities.turn2bytes(hexValue), recipeSource), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else
                {
                    selectedItem.setup(GetNameFromID(Utilities.turn2bytes(customAmountTxt.Text), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(Utilities.turn2bytes(customAmountTxt.Text), recipeSource), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
            }
            else if (customIdTextbox.Text == "315A" || customIdTextbox.Text == "1618") // Wall-Mounted
            {
                if (hexModeBtn.Tag.ToString() == "Normal")
                {
                    selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + hexValue, 16)), true, GetImagePathFromID((Utilities.turn2bytes(hexValue)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else
                {
                    selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), true, GetImagePathFromID((Utilities.turn2bytes(customAmountTxt.Text)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                }
            }
            else
            {
                if (ItemAttr.hasGenetics(Convert.ToUInt16("0x" + customIdTextbox.Text, 16)))
                {
                    string value = customAmountTxt.Text;
                    int length = value.Length;
                    string firstByte;
                    string secondByte;
                    if (length < 2)
                    {
                        firstByte = "0";
                        secondByte = value;
                    }
                    else
                    {
                        firstByte = value.Substring(length - 2, 1);
                        secondByte = value.Substring(length - 1, 1);
                    }

                    setGeneComboBox(firstByte, secondByte);
                    genePanel.Visible = true;
                }

                if (hexModeBtn.Tag.ToString() == "Normal")
                {
                    selectedItem.setup(GetNameFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + hexValue, 16)), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else
                {
                    selectedItem.setup(GetNameFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }

                if (selection != null)
                {
                    selection.receiveID(customIdTextbox.Text, languageSetting);
                }
            }
            updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
        }

        private void customIdTextbox_TextChanged(object sender, EventArgs e)
        {
            if (((RichTextBox)sender).Modified)
            {
                if (customIdTextbox.Text != "")
                {
                    if (ItemAttr.hasGenetics(Convert.ToUInt16("0x" + customIdTextbox.Text, 16)))
                    {
                        if (hexModeBtn.Tag.ToString() == "Normal")
                        {
                            hexMode_Click(sender, e);
                        }

                        string value = customAmountTxt.Text;
                        int length = value.Length;
                        string firstByte;
                        string secondByte;
                        if (length < 2)
                        {
                            firstByte = "0";
                            secondByte = value;
                        }
                        else
                        {
                            firstByte = value.Substring(length - 2, 1);
                            secondByte = value.Substring(length - 1, 1);
                        }

                        setGeneComboBox(firstByte, secondByte);
                        genePanel.Visible = true;
                    }
                    else
                    {
                        genePanel.Visible = false;
                    }

                    if (customIdTextbox.Text == "315A" || customIdTextbox.Text == "1618")
                    {
                        WallMountMsg.Visible = true;
                    }
                    else
                    {
                        WallMountMsg.Visible = false;
                    }
                }
                else
                {
                    genePanel.Visible = false;
                    WallMountMsg.Visible = false;
                }
            }
            else
            {
                genePanel.Visible = false;
                WallMountMsg.Visible = false;
            }
        }

        #endregion

        private void ShowMessage(string itemID)
        {
            int rowIndex = -1;

            if (currentPanel == itemModePanel)
            {
                DataGridViewRow row = itemGridView.Rows
                .Cast<DataGridViewRow>()
                .Where(r => r.Cells["id"].Value.ToString().Equals(itemID.ToLower()))
                .FirstOrDefault();

                if (row == null)
                {
                    row = itemGridView.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => r.Cells["id"].Value.ToString().Equals(itemID))
                    .FirstOrDefault();

                    if (row == null)
                    {
                        return;
                    }
                }
                rowIndex = row.Index;
                msgLabel.Text = "Spawn " + itemGridView.Rows[rowIndex].Cells[languageSetting].Value.ToString();
            }
            else if (currentPanel == recipeModePanel)
            {
                DataGridViewRow row = recipeGridView.Rows
                .Cast<DataGridViewRow>()
                .Where(r => r.Cells["id"].Value.ToString().Equals(itemID.ToLower()))
                .FirstOrDefault();

                if (row == null)
                {
                    row = recipeGridView.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => r.Cells["id"].Value.ToString().Equals(itemID))
                    .FirstOrDefault();

                    if (row == null)
                    {
                        return;
                    }
                }
                rowIndex = row.Index;
                msgLabel.Text = "Spawn " + recipeGridView.Rows[rowIndex].Cells[languageSetting].Value.ToString() + " recipe";
            }
            else if (currentPanel == flowerModePanel)
            {
                DataGridViewRow row = flowerGridView.Rows
                .Cast<DataGridViewRow>()
                .Where(r => r.Cells["id"].Value.ToString().Equals(itemID.ToLower()))
                .FirstOrDefault();

                if (row == null)
                {
                    row = flowerGridView.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => r.Cells["id"].Value.ToString().Equals(itemID))
                    .FirstOrDefault();

                    if (row == null)
                    {
                        return;
                    }
                }
                rowIndex = row.Index;
                msgLabel.Text = "Spawn " + flowerGridView.Rows[rowIndex].Cells[languageSetting].Value.ToString() + " (Sparkling)";
            }
            /*
            var time = new System.Windows.Forms.Timer();
            time.Interval = 3000;
            time.Tick += (s, e) =>
            {
                msgLabel.Text = "";
                time.Stop();
            };
            time.Start();
            */
        }

        #region Pagination
        private void nextBtn_Click(object sender, EventArgs e)
        {
            if (currentPage < maxPage)
            {
                if (playerSelectorInventory.SelectedIndex == 16)
                {
                    Utilities.gotoRecyclingPage((uint)(currentPage + 1));
                }
                else if (playerSelectorInventory.SelectedIndex > 7)
                {
                    Utilities.gotoHousePage((uint)(currentPage + 1), playerSelectorInventory.SelectedIndex - 7);
                }
                currentPage++;
                setPageLabel();
                UpdateInventory();
            }
            else
            {
                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
            }
        }

        private void backBtn_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                if (playerSelectorInventory.SelectedIndex == 16)
                {
                    Utilities.gotoRecyclingPage((uint)(currentPage - 1));
                }
                else if (playerSelectorInventory.SelectedIndex > 7)
                {
                    Utilities.gotoHousePage((uint)(currentPage - 1), playerSelectorInventory.SelectedIndex - 7);
                }
                currentPage--;
                setPageLabel();
                UpdateInventory();
            }
            else
            {
                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
            }
        }

        private void fastNextBtn_Click(object sender, EventArgs e)
        {
            if (currentPage == maxPage)
            {
                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
                return;
            }
            if (currentPage + 10 < maxPage)
            {
                if (playerSelectorInventory.SelectedIndex == 16)
                {
                    Utilities.gotoRecyclingPage((uint)(currentPage + 1));
                }
                else if (playerSelectorInventory.SelectedIndex > 7)
                {
                    Utilities.gotoHousePage((uint)(currentPage + 10), playerSelectorInventory.SelectedIndex - 7);
                }
                currentPage += 10;
            }
            else
            {
                if (playerSelectorInventory.SelectedIndex == 16)
                {
                    Utilities.gotoRecyclingPage((uint)maxPage);
                }
                else if (playerSelectorInventory.SelectedIndex > 7)
                {
                    Utilities.gotoHousePage((uint)maxPage, playerSelectorInventory.SelectedIndex - 7);
                }
                currentPage = maxPage;
                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
            }
            setPageLabel();
            UpdateInventory();
        }

        private void fastBackBtn_Click(object sender, EventArgs e)
        {
            if (currentPage == 1)
            {
                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
                return;
            }
            if (currentPage - 10 > 1)
            {
                if (playerSelectorInventory.SelectedIndex == 16)
                {
                    Utilities.gotoRecyclingPage((uint)(currentPage - 10));
                }
                else if (playerSelectorInventory.SelectedIndex > 7)
                {
                    Utilities.gotoHousePage((uint)(currentPage - 10), playerSelectorInventory.SelectedIndex - 7);
                }
                currentPage -= 10;
            }
            else
            {
                if (playerSelectorInventory.SelectedIndex == 16)
                {
                    Utilities.gotoRecyclingPage(1);
                }
                else if (playerSelectorInventory.SelectedIndex > 7)
                {
                    Utilities.gotoHousePage(1, playerSelectorInventory.SelectedIndex - 7);
                }
                currentPage = 1;
                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
            }
            setPageLabel();
            UpdateInventory();
        }

        private void setPageLabel()
        {
            pageLabel.Text = "Page " + currentPage;
        }

        private void showPagination()
        {
            setPageLabel();
            paginationPanel.Visible = true;
        }

        private void hidePagination()
        {
            setPageLabel();
            paginationPanel.Visible = false;
        }

        #endregion

        #region Turnip
        private void setTurnipBtn_Click(object sender, EventArgs e)
        {
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }
            if (turnipBuyPrice.Text == "" ||
                turnipSell1AM.Text == "" || turnipSell1PM.Text == "" ||
                turnipSell2AM.Text == "" || turnipSell2PM.Text == "" ||
                turnipSell3AM.Text == "" || turnipSell3PM.Text == "" ||
                turnipSell4AM.Text == "" || turnipSell4PM.Text == "" ||
                turnipSell5AM.Text == "" || turnipSell5PM.Text == "" ||
                turnipSell6AM.Text == "" || turnipSell6PM.Text == "")
            {
                MessageBox.Show("Turnip prices cannot be empty");
                return;
            }

            DialogResult dialogResult = myMessageBox.Show("Are you sure you want to set the turnip prices?\n[Warning] All original prices will be overwritten!", "Set turnip prices", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (dialogResult == DialogResult.Yes)
            {
                UInt32[] prices = new UInt32[13] {
                Convert.ToUInt32(turnipSell1AM.Text, 10), Convert.ToUInt32(turnipSell1PM.Text, 10),
                Convert.ToUInt32(turnipSell2AM.Text, 10), Convert.ToUInt32(turnipSell2PM.Text, 10),
                Convert.ToUInt32(turnipSell3AM.Text, 10), Convert.ToUInt32(turnipSell3PM.Text, 10),
                Convert.ToUInt32(turnipSell4AM.Text, 10), Convert.ToUInt32(turnipSell4PM.Text, 10),
                Convert.ToUInt32(turnipSell5AM.Text, 10), Convert.ToUInt32(turnipSell5PM.Text, 10),
                Convert.ToUInt32(turnipSell6AM.Text, 10), Convert.ToUInt32(turnipSell6PM.Text, 10),
                Convert.ToUInt32(turnipBuyPrice.Text, 10)};

                try
                {
                    Utilities.ChangeTurnipPrices(s, bot, prices);
                    UpdateTurnipPrices();
                }
                catch (Exception ex)
                {
                    Log.logEvent("MainForm", "SetTurnip: " + ex.Message.ToString());
                    myMessageBox.Show(ex.Message.ToString(), "This is a terrible way of doing this!");
                }

                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
            }
        }

        private void setMaxBtn_Click(object sender, EventArgs e)
        {
            string max = "999999999";
            string min = "1";
            DialogResult dialogResult = myMessageBox.Show("Are you sure you want to set all the turnip prices to MAX?\n[Warning] All original prices will be overwritten!", "Set all turnip prices", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            if (dialogResult == DialogResult.Yes)
            {
                UInt32[] prices = new UInt32[13] {
                Convert.ToUInt32(max, 10), Convert.ToUInt32(max, 10),
                Convert.ToUInt32(max, 10), Convert.ToUInt32(max, 10),
                Convert.ToUInt32(max, 10), Convert.ToUInt32(max, 10),
                Convert.ToUInt32(max, 10), Convert.ToUInt32(max, 10),
                Convert.ToUInt32(max, 10), Convert.ToUInt32(max, 10),
                Convert.ToUInt32(max, 10), Convert.ToUInt32(max, 10),
                Convert.ToUInt32(min, 10)};

                try
                {
                    Utilities.ChangeTurnipPrices(s, bot, prices);
                    UpdateTurnipPrices();
                }
                catch (Exception ex)
                {
                    Log.logEvent("MainForm", "SetAllTurnip: " + ex.Message.ToString());
                    myMessageBox.Show(ex.Message.ToString(), "This is a terrible way of doing this!");
                }

                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
            }
        }
        #endregion

        #region Tooltip
        private void inventory_MouseHover(object sender, EventArgs e)
        {
            var button = (inventorySlot)sender;
            if (!button.isEmpty())
            {
                /*
                if (button.getContainItemName() != "")
                {
                    btnToolTip.SetToolTip(button, button.displayItemName() + "\n\nID : " + button.displayItemID() + "\nCount : " + button.displayItemData() + "\nFlag : 0x" + button.getFlag1() + button.getFlag2() + "\nContain Item : " + button.getContainItemName());
                }
                else
                {*/
                btnToolTip.SetToolTip(button, button.displayItemName() + "\n\nID : " + button.displayItemID() + "\nCount : " + button.displayItemData() + "\nFlag : 0x" + button.getFlag1() + button.getFlag2());
                //}
            }
        }
        #endregion

        #region Variation
        private void variationModeButton_Click(object sender, EventArgs e)
        {
            if (selection == null)
            {
                openVariationMenu();
            }
            else
            {
                closeVariationMenu();
            }
        }

        private void openVariationMenu()
        {
            selection = new variation();
            selection.Show();
            selection.Location = new System.Drawing.Point(this.Location.X + 7, this.Location.Y + 550);
            string id = Utilities.precedingZeros(selectedItem.fillItemID(), 4);
            if (id == "315A" || id == "1618")
            {
                selection.receiveID(Utilities.turn2bytes(selectedItem.fillItemData()), languageSetting);
            }
            else
            {
                selection.receiveID(Utilities.precedingZeros(selectedItem.fillItemID(), 4), languageSetting);
            }
            selection.mainform = this;
            variationModeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
        }

        private void closeVariationMenu()
        {
            if (selection != null)
            {
                selection.Dispose();
                selection = null;
                variationModeButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            }
        }

        public void ReceiveVariation(inventorySlot select, int type = 0)
        {
            if (type == 0) //Left click
            {
                selectedItem.setup(select);
                if (hexModeBtn.Tag.ToString() == "Normal")
                {
                    hexMode_Click(null, null);
                }
                updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
                customAmountTxt.Text = Utilities.precedingZeros(selectedItem.fillItemData(), 8);
                customIdTextbox.Text = Utilities.precedingZeros(selectedItem.fillItemID(), 4);
            }
            else if (type == 1) // Right click
            {
                if (customIdTextbox.Text == "315A" || customIdTextbox.Text == "1618")
                {
                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        hexMode_Click(null, null);
                    }

                    string count = translateVariationValue(select.fillItemData()) + Utilities.precedingZeros(select.fillItemID(), 4);
                    selectedItem.setup(GetNameFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + count, 16), GetImagePathFromID(Utilities.turn2bytes(customIdTextbox.Text), itemSource), true, select.getPath(), selectedItem.getFlag1(), selectedItem.getFlag2());
                    customAmountTxt.Text = count;
                }
            }
        }

        private string translateVariationValue(string input)
        {
            int hexValue = Convert.ToUInt16("0x" + input, 16);
            int firstHalf = 0;
            int secondHalf = 0;
            string output;

            if (hexValue <= 0x7)
            {
                return Utilities.precedingZeros(input, 4);
            }
            else if (hexValue <= 0x27)
            {
                firstHalf = (0x20 / 4);
                secondHalf = (hexValue - 0x20);
            }
            else if (hexValue <= 0x47)
            {
                firstHalf = (0x40 / 4);
                secondHalf = (hexValue - 0x40);
            }
            else if (hexValue <= 0x67)
            {
                firstHalf = (0x60 / 4);
                secondHalf = (hexValue - 0x60);
            }
            else if (hexValue <= 0x87)
            {
                firstHalf = (0x80 / 4);
                secondHalf = (hexValue - 0x80);
            }
            else if (hexValue <= 0xA7)
            {
                firstHalf = (0xA0 / 4);
                secondHalf = (hexValue - 0xA0);
            }
            else if (hexValue <= 0xC7)
            {
                firstHalf = (0xC0 / 4);
                secondHalf = (hexValue - 0xC0);
            }
            else if (hexValue <= 0xE7)
            {
                firstHalf = (0xE0 / 4);
                secondHalf = (hexValue - 0xE0);
            }

            output = Utilities.precedingZeros((firstHalf + secondHalf).ToString("X"), 4);
            return output;
        }

        #endregion

        #region Gene
        private void setGeneComboBox(string firstByte, string secondByte)
        {
            switch (firstByte)
            {
                case "0":
                    flowerGeneW.SelectedIndex = 0;
                    flowerGeneS.SelectedIndex = 0;
                    break;
                case "1":
                    flowerGeneW.SelectedIndex = 1;
                    flowerGeneS.SelectedIndex = 0;
                    break;
                case "3":
                    flowerGeneW.SelectedIndex = 2;
                    flowerGeneS.SelectedIndex = 0;
                    break;
                case "4":
                    flowerGeneW.SelectedIndex = 0;
                    flowerGeneS.SelectedIndex = 1;
                    break;
                case "5":
                    flowerGeneW.SelectedIndex = 1;
                    flowerGeneS.SelectedIndex = 1;
                    break;
                case "7":
                    flowerGeneW.SelectedIndex = 2;
                    flowerGeneS.SelectedIndex = 1;
                    break;
                case "C":
                    flowerGeneW.SelectedIndex = 0;
                    flowerGeneS.SelectedIndex = 2;
                    break;
                case "D":
                    flowerGeneW.SelectedIndex = 1;
                    flowerGeneS.SelectedIndex = 2;
                    break;
                case "F":
                    flowerGeneW.SelectedIndex = 2;
                    flowerGeneS.SelectedIndex = 2;
                    break;
                default:
                    flowerGeneW.SelectedIndex = -1;
                    flowerGeneS.SelectedIndex = -1;
                    break;
            }
            switch (secondByte)
            {
                case "0":
                    flowerGeneR.SelectedIndex = 0;
                    flowerGeneY.SelectedIndex = 0;
                    break;
                case "1":
                    flowerGeneR.SelectedIndex = 1;
                    flowerGeneY.SelectedIndex = 0;
                    break;
                case "3":
                    flowerGeneR.SelectedIndex = 2;
                    flowerGeneY.SelectedIndex = 0;
                    break;
                case "4":
                    flowerGeneR.SelectedIndex = 0;
                    flowerGeneY.SelectedIndex = 1;
                    break;
                case "5":
                    flowerGeneR.SelectedIndex = 1;
                    flowerGeneY.SelectedIndex = 1;
                    break;
                case "7":
                    flowerGeneR.SelectedIndex = 2;
                    flowerGeneY.SelectedIndex = 1;
                    break;
                case "C":
                    flowerGeneR.SelectedIndex = 0;
                    flowerGeneY.SelectedIndex = 2;
                    break;
                case "D":
                    flowerGeneR.SelectedIndex = 1;
                    flowerGeneY.SelectedIndex = 2;
                    break;
                case "F":
                    flowerGeneR.SelectedIndex = 2;
                    flowerGeneY.SelectedIndex = 2;
                    break;
                default:
                    flowerGeneR.SelectedIndex = -1;
                    flowerGeneY.SelectedIndex = -1;
                    break;
            }
        }

        private void SelectionChangeCommitted(object sender, EventArgs e)
        {
            int R = flowerGeneR.SelectedIndex;
            int Y = flowerGeneY.SelectedIndex;
            int W = flowerGeneW.SelectedIndex;
            int S = flowerGeneS.SelectedIndex;

            setGeneTextBox(R, Y, W, S);
        }

        private void setGeneTextBox(int R, int Y, int W, int S)
        {
            string firstByte;
            switch (W)
            {
                case 0:
                    switch (S)
                    {
                        case 0:
                            firstByte = "0";
                            break;
                        case 1:
                            firstByte = "4";
                            break;
                        case 2:
                            firstByte = "C";
                            break;
                        default:
                            firstByte = "8";
                            break;
                    }
                    break;
                case 1:
                    switch (S)
                    {
                        case 0:
                            firstByte = "1";
                            break;
                        case 1:
                            firstByte = "5";
                            break;
                        case 2:
                            firstByte = "D";
                            break;
                        default:
                            firstByte = "9";
                            break;
                    }
                    break;
                case 2:
                    switch (S)
                    {
                        case 0:
                            firstByte = "3";
                            break;
                        case 1:
                            firstByte = "7";
                            break;
                        case 2:
                            firstByte = "F";
                            break;
                        default:
                            firstByte = "B";
                            break;
                    }
                    break;
                default:
                    switch (S)
                    {
                        case 0:
                            firstByte = "2";
                            break;
                        case 1:
                            firstByte = "6";
                            break;
                        case 2:
                            firstByte = "E";
                            break;
                        default:
                            firstByte = "A";
                            break;
                    }
                    break;
            }
            string secondByte;
            switch (R)
            {
                case 0:
                    switch (Y)
                    {
                        case 0:
                            secondByte = "0";
                            break;
                        case 1:
                            secondByte = "4";
                            break;
                        case 2:
                            secondByte = "C";
                            break;
                        default:
                            secondByte = "8";
                            break;
                    }
                    break;
                case 1:
                    switch (Y)
                    {
                        case 0:
                            secondByte = "1";
                            break;
                        case 1:
                            secondByte = "5";
                            break;
                        case 2:
                            secondByte = "D";
                            break;
                        default:
                            secondByte = "9";
                            break;
                    }
                    break;
                case 2:
                    switch (Y)
                    {
                        case 0:
                            secondByte = "3";
                            break;
                        case 1:
                            secondByte = "7";
                            break;
                        case 2:
                            secondByte = "F";
                            break;
                        default:
                            secondByte = "B";
                            break;
                    }
                    break;
                default:
                    switch (Y)
                    {
                        case 0:
                            secondByte = "2";
                            break;
                        case 1:
                            secondByte = "6";
                            break;
                        case 2:
                            secondByte = "E";
                            break;
                        default:
                            secondByte = "A";
                            break;
                    }
                    break;
            }
            //Debug.Print(firstByte + secondByte);
            if (hexModeBtn.Tag.ToString() == "Normal")
            {
                hexMode_Click(null, null);
            }
            customAmountTxt.Text = Utilities.precedingZeros(firstByte + secondByte, 8);
            selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), true);
            updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
        }

        #endregion

        #region Right Click
        private void wrapAllItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }
            */
            if (wrapSetting.SelectedIndex < 0)
                wrapSetting.SelectedIndex = 0;

            string[] flagBuffer = wrapSetting.SelectedItem.ToString().Split(' ');
            byte flagByte = Utilities.stringToByte(flagBuffer[flagBuffer.Length - 1])[0];
            Thread wrapAllThread = new Thread(delegate () { wrapAll(flagByte); });
            wrapAllThread.Start();
        }

        private void wrapAll(byte flagByte)
        {
            showWait();

            string flag = "00";
            if (RetainNameCheckBox.Checked)
            {
                flag = Utilities.precedingZeros((flagByte + 0x40).ToString("X"), 2);
            }
            else
            {
                flag = Utilities.precedingZeros((flagByte).ToString("X"), 2);
            }

            if (!offline)
            {
                byte[] Bank01to20 = Utilities.GetInventoryBank(s, bot, 1);
                byte[] Bank21to40 = Utilities.GetInventoryBank(s, bot, 21);

                foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                {
                    int slot = int.Parse(btn.Tag.ToString());
                    byte[] slotBytes = new byte[2];

                    int slotOffset;
                    if (slot < 21)
                    {
                        slotOffset = ((slot - 1) * 0x8);
                    }
                    else
                    {
                        slotOffset = ((slot - 21) * 0x8);
                    }

                    if (slot < 21)
                    {
                        Buffer.BlockCopy(Bank01to20, slotOffset, slotBytes, 0x0, 0x2);
                    }
                    else
                    {
                        Buffer.BlockCopy(Bank21to40, slotOffset, slotBytes, 0x0, 0x2);
                    }

                    string slotID = Utilities.flip(Utilities.ByteToHexString(slotBytes));

                    if (slotID != "FFFE")
                    {
                        Utilities.setFlag1(s, bot, slot, flag);
                        Invoke((MethodInvoker)delegate
                        {
                            btn.setFlag1(flag);
                            btn.refresh(false);
                        });
                    }
                }
            }
            else
            {
                foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                {
                    if (btn.fillItemID() != "FFFE")
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            btn.setFlag1(flag);
                            btn.refresh(false);
                        });
                    }
                }
            }
            if (sound)
                System.Media.SystemSounds.Asterisk.Play();

            hideWait();
        }

        private void unwrapAllItemToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }
            */
            Thread unwrapAllThread = new Thread(delegate () { unwrapAll(); });
            unwrapAllThread.Start();
        }

        private void unwrapAll()
        {
            showWait();

            if (!offline)
            {
                byte[] Bank01to20 = Utilities.GetInventoryBank(s, bot, 1);
                byte[] Bank21to40 = Utilities.GetInventoryBank(s, bot, 21);

                foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                {
                    int slot = int.Parse(btn.Tag.ToString());
                    byte[] slotBytes = new byte[2];

                    int slotOffset;
                    if (slot < 21)
                    {
                        slotOffset = ((slot - 1) * 0x8);
                    }
                    else
                    {
                        slotOffset = ((slot - 21) * 0x8);
                    }

                    if (slot < 21)
                    {
                        Buffer.BlockCopy(Bank01to20, slotOffset, slotBytes, 0x0, 0x2);
                    }
                    else
                    {
                        Buffer.BlockCopy(Bank21to40, slotOffset, slotBytes, 0x0, 0x2);
                    }

                    string slotID = Utilities.flip(Utilities.ByteToHexString(slotBytes));

                    if (slotID != "FFFE")
                    {
                        Utilities.setFlag1(s, bot, slot, "00");
                        Invoke((MethodInvoker)delegate
                        {
                            btn.setFlag1("00");
                            btn.refresh(false);
                        });
                    }
                }
            }
            else
            {
                foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                {
                    if (btn.fillItemID() != "FFFE")
                    {
                        Invoke((MethodInvoker)delegate
                        {
                            btn.setFlag1("00");
                            btn.refresh(false);
                        });
                    }
                }
            }
            if (sound)
                System.Media.SystemSounds.Asterisk.Play();

            hideWait();
        }
        private void addToFavoriteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripItem item = (sender as ToolStripItem);
            if (item != null)
            {
                if (item.Owner is ContextMenuStrip owner)
                {
                    var btn = (inventorySlot)owner.SourceControl;
                    if (btn.fillItemID() == "FFFE")
                    {
                        return;
                    }
                    else
                    {
                        DataTable dt = (DataTable)favGridView.DataSource;
                        DataRow dr = dt.NewRow();
                        dr["id"] = Utilities.turn2bytes(btn.fillItemID());
                        dr["iName"] = btn.getiName();
                        dr["Name"] = btn.displayItemName();
                        dr["value"] = Utilities.precedingZeros(btn.fillItemData(), 8);

                        dt.Rows.Add(dr);
                        favGridView.DataSource = dt;

                        string line = dr["id"] + " ; " + dr["iName"] + " ; " + dr["Name"] + " ; " + dr["value"] + " ; ";

                        if (!File.Exists(Utilities.favPath))
                        {
                            string favheader = "id" + " ; " + "iName" + " ; " + "Name" + " ; " + "value" + " ; ";

                            using (StreamWriter sw = File.CreateText(Utilities.favPath))
                            {
                                sw.WriteLine(favheader);
                            }
                        }

                        using (StreamWriter sw = File.AppendText(Utilities.favPath))
                        {
                            sw.WriteLine(line);
                        }

                    }
                    if (sound)
                        System.Media.SystemSounds.Asterisk.Play();
                }
            }
        }

        #endregion

        #region Inventory Name
        private string[] getInventoryName()
        {
            string[] namelist = new string[8];
            Debug.Print("Peek 8 Name:");
            byte[] tempHeader = null;
            Boolean headerFound = false;

            for (int i = 0; i < 8; i++)
            {
                byte[] b = Utilities.peekAddress(s, bot, (Utilities.player1SlotBase + (i * Utilities.playerOffset)) + Utilities.InventoryNameOffset, 0x34);
                namelist[i] = Encoding.Unicode.GetString(b, 32, 20);
                namelist[i] = namelist[i].Replace("\0", string.Empty);
                if (namelist[i].Equals(string.Empty) && !headerFound)
                {
                    header = tempHeader;
                    headerFound = true;
                }
                tempHeader = b;
            }
            return namelist;
        }

        public static byte[] getHeader()
        {
            return header;
        }

        private int updateDropdownBox()
        {
            string[] namelist = getInventoryName();
            int currentPlayer = 0;
            for (int i = 7; i >= 0; i--)
            {
                if (namelist[i] != string.Empty)
                {
                    playerSelectorInventory.Items.RemoveAt(i);
                    playerSelectorInventory.Items.Insert(i, namelist[i]);
                    playerSelectorInventory.Items.RemoveAt(i + 8);
                    playerSelectorInventory.Items.Insert(i + 8, namelist[i] + "'s House");

                    playerSelectorOther.Items.RemoveAt(i);
                    playerSelectorOther.Items.Insert(i, namelist[i]);
                    if (i > currentPlayer)
                        currentPlayer = i;
                }
            }
            return currentPlayer;
        }
        #endregion

        private static string removeNumber(string filename)
        {
            char[] MyChar = { '0', '1', '2', '3', '4' };
            return filename.Trim(MyChar);
        }
    }
}
