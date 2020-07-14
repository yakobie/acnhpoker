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
namespace acnhpoker
{
    public partial class Form1 : Form
    {
        Socket s;
        readonly Utilities utilities = new Utilities();

        readonly private string version = "ACNH Poker V1.3.1 MyShiLingStar R9.2";
        private inventorySlot selectedButton;
        public int selectedSlot = 1;
        public DataGridViewRow lastRow;
        public DataGridViewRow recipelastRow;
        public DataGridViewRow flowerlastRow;
        private Panel currentPanel;
        private System.Windows.Forms.Timer refreshTimer;
        private DataTable itemSource;
        private DataTable recipeSource;
        private DataTable variationSource;
        private Boolean offsetFound = false;
        private int maxPage = 1;
        private int currentPage = 1;
        private variation selection = null;
        private USBBot bot = null;
        private string settingFile = @"acnhpoker.exe.config";

        const string insectAppearFileName = @"InsectAppearParam.bin";
        const string fishRiverAppearFileName = @"FishAppearRiverParam.bin";
        const string fishSeaAppearFileName = @"FishAppearSeaParam.bin";
        const string CreatureSeaAppearFileName = @"CreatureAppearSeaParam.bin";

        static private byte[] InsectAppearParam = LoadBinaryFile(insectAppearFileName);
        static private byte[] FishRiverAppearParam = LoadBinaryFile(fishRiverAppearFileName);
        static private byte[] FishSeaAppearParam = LoadBinaryFile(fishSeaAppearFileName);
        static private byte[] CreatureSeaAppearParam = LoadBinaryFile(CreatureSeaAppearFileName);

        private int[] insectRate;
        private int[] riverFishRate;
        private int[] seaFishRate;
        private int[] seaCreatureRate;
        private DataGridView currentGridView;

        const string itemPath = @"items.csv";
        const string recipePath = @"recipe.csv";
        const string flowerPath = @"flowers.csv";
        const string variationPath = @"variation.csv";

        public Form1()
        {
            InitializeComponent();
        }

        private DataTable loadItemCSV(string filePath)
        {
            var dt = new DataTable();

            File.ReadLines(filePath).Take(1)
                .SelectMany(x => x.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                .ToList()
                .ForEach(x => dt.Columns.Add(x.Trim()));

            File.ReadLines(filePath).Skip(1)
                .Select(x => x.Split(','))
                .ToList()
                .ForEach(line => dt.Rows.Add(line));

            if (dt.Columns.Contains("ID"))
                dt.PrimaryKey = new DataColumn[1] { dt.Columns["ID"] };

            return dt;
        }
        static private byte[] LoadBinaryFile(string file)
        {
            if (File.Exists(file))
            {
                return File.ReadAllBytes(file);
            }
            else return null;
        }

        private void connectBtn_Click(object sender, EventArgs e)
        {
            if (connectBtn.Tag.ToString() == "connect")
            {

                string ipPattern = @"\b(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\b";

                if (!Regex.IsMatch(ipBox.Text, ipPattern))
                {
                    pictureBox1.BackColor = System.Drawing.Color.Red;
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
                                    this.pictureBox1.BackColor = System.Drawing.Color.Red;
                                });
                                return;
                            }

                            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

                            this.pictureBox1.Invoke((MethodInvoker)delegate
                            {
                                this.pictureBox1.BackColor = System.Drawing.Color.Green;
                            });
                            this.ipBox.Invoke((MethodInvoker)delegate
                            {
                                this.ipBox.ReadOnly = true;
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
                            });
                            Invoke((MethodInvoker)delegate
                            {
                                if (validation())
                                {
                                    MessageBox.Show("Sys-botbase validation failed!\nPoke request is returning same result.\n" +
                                                    "Please remove or rename the cheat folder located at \n\n" +
                                                    "sd:/atmospshere/contents/01006f8002326000/\n\n" +
                                                    "You may also setup a \"override_config.ini\" to switch between cheat file usage and ACNHPoker.", "Sys-botbase validation");

                                    s.Close();
                                    return;
                                }

                                this.refreshBtn.Visible = true;
                                this.playerSelectionPanel.Visible = true;
                                this.autoRefreshCheckBox.Visible = true;
                                this.saveBtn.Visible = true;
                                this.loadBtn.Visible = true;
                                this.otherBtn.Visible = true;
                                this.critterBtn.Visible = true;
                                this.wrapSetting.SelectedIndex = 0;
                                //this.selectedItem.setHide(true);
                                this.connectBtn.Tag = "disconnect";
                                this.connectBtn.Text = "Disconnect";
                                this.USBconnectBtn.Visible = false;

                                UpdateInventory();
                                UpdateTownID();
                                InitTimer();
                                setEatBtn();
                                UpdateTurnipPrices();
                                loadReaction();
                                readWeatherSeed();

                                currentGridView = insectGridView;

                                LoadGridView(InsectAppearParam, insectGridView, ref insectRate, Utilities.InsectDataSize, Utilities.InsectNumRecords);
                                LoadGridView(FishRiverAppearParam, riverFishGridView, ref riverFishRate, Utilities.FishDataSize, Utilities.FishRiverNumRecords, 1);
                                LoadGridView(FishSeaAppearParam, seaFishGridView, ref seaFishRate, Utilities.FishDataSize, Utilities.FishSeaNumRecords, 1);
                                LoadGridView(CreatureSeaAppearParam, seaCreatureGridView, ref seaCreatureRate, Utilities.SeaCreatureDataSize, Utilities.SeaCreatureNumRecords, 1);
                                /*
                                int firstSlot = findEmpty();
                                if (firstSlot > 0)
                                {
                                    selectedSlot = firstSlot;
                                    updateSlot(firstSlot);
                                }
                                */
                            });

                        }

                        else
                        {
                            s.Close();
                            this.pictureBox1.Invoke((MethodInvoker)delegate
                            {
                                this.pictureBox1.BackColor = System.Drawing.Color.Red;

                            });
                            MessageBox.Show("Could not connect to the botbase server, Go to https://github.com/KingLycosa/acnhpoker for help.");
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
                playerSelectionPanel.Visible = false;
                autoRefreshCheckBox.Visible = false;
                this.USBconnectBtn.Visible = true;

                this.saveBtn.Visible = false;
                this.loadBtn.Visible = false;
                inventoryBtn_Click(sender, e);
                otherBtn.Visible = false;
                critterBtn.Visible = false;

                this.connectBtn.Tag = "connect";
                this.connectBtn.Text = "Connect";

                this.Text = version;
            }
        }

        public void InitTimer()
        {
            refreshTimer = new System.Windows.Forms.Timer();
            refreshTimer.Tick += new EventHandler(refreshTimer_Tick);
            refreshTimer.Interval = 3000; // in miliseconds
            refreshTimer.Start();
        }

        private void refreshTimer_Tick(object sender, EventArgs e)
        {
            if (s != null && s.Connected == true && autoRefreshCheckBox.Checked)
                Invoke((MethodInvoker)delegate { UpdateInventory(); });
        }

        private int UpdateInventory()
        {
            try
            {
                byte[] Bank01to20 = utilities.GetInventoryBank(s, bot, 1);
                if (Bank01to20 == null)
                {
                    return 0;
                }
                byte[] Bank21to40 = utilities.GetInventoryBank(s, bot, 21);
                if (Bank21to40 == null)
                {
                    return 0;
                }
                string Bank1 = Encoding.UTF8.GetString(Bank01to20);
                string Bank2 = Encoding.UTF8.GetString(Bank21to40);

                //Debug.Print(Bank1);
                //Debug.Print(Bank2);

                foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                {
                    if (btn.Tag == null)
                        continue;

                    if (btn.Tag.ToString() == "")
                        continue;

                    int slotId = int.Parse(btn.Tag.ToString());

                    byte[] slotBytes = new byte[4];
                    byte[] flag1Bytes = new byte[2];
                    byte[] flag2Bytes = new byte[2];
                    byte[] dataBytes = new byte[8];
                    byte[] recipeBytes = new byte[4];

                    int slotOffset = 0;
                    int countOffset = 0;
                    int flag1Offset = 0;
                    int flag2Offset = 0;
                    if (slotId < 21)
                    {
                        slotOffset = ((slotId - 1) * 0x10);
                        flag1Offset = 0x6 + ((slotId - 1) * 0x10);
                        flag2Offset = 0x4 + ((slotId - 1) * 0x10);
                        countOffset = 0x8 + ((slotId - 1) * 0x10);
                    }
                    else
                    {
                        slotOffset = ((slotId - 21) * 0x10);
                        flag1Offset = 0x6 + ((slotId - 21) * 0x10);
                        flag2Offset = 0x4 + ((slotId - 21) * 0x10);
                        countOffset = 0x8 + ((slotId - 21) * 0x10);
                    }

                    if (slotId < 21)
                    {
                        Buffer.BlockCopy(Bank01to20, slotOffset, slotBytes, 0x0, 0x4);
                        Buffer.BlockCopy(Bank01to20, flag1Offset, flag1Bytes, 0x0, 0x2);
                        Buffer.BlockCopy(Bank01to20, flag2Offset, flag2Bytes, 0x0, 0x2);
                        Buffer.BlockCopy(Bank01to20, countOffset, dataBytes, 0x0, 0x8);
                        Buffer.BlockCopy(Bank01to20, countOffset, recipeBytes, 0x0, 0x4);
                    }
                    else
                    {
                        Buffer.BlockCopy(Bank21to40, slotOffset, slotBytes, 0x0, 0x4);
                        Buffer.BlockCopy(Bank21to40, flag1Offset, flag1Bytes, 0x0, 0x2);
                        Buffer.BlockCopy(Bank21to40, flag2Offset, flag2Bytes, 0x0, 0x2);
                        Buffer.BlockCopy(Bank21to40, countOffset, dataBytes, 0x0, 0x8);
                        Buffer.BlockCopy(Bank21to40, countOffset, recipeBytes, 0x0, 0x4);
                    }

                    string itemID = utilities.flip(Encoding.ASCII.GetString(slotBytes));
                    string itemData = utilities.flip(Encoding.ASCII.GetString(dataBytes));
                    string recipeData = utilities.flip(Encoding.ASCII.GetString(recipeBytes));
                    string flag1 = Encoding.ASCII.GetString(flag1Bytes);
                    string flag2 = Encoding.ASCII.GetString(flag2Bytes);

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
                    else if (itemID == "114A") // Money Tree
                    {
                        btn.setup(GetNameFromID(itemID, itemSource), Convert.ToUInt16("0x" + itemID, 16), Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(itemID, itemSource, Convert.ToUInt32("0x" + itemData, 16)), GetNameFromID(recipeData, itemSource), flag1, flag2);
                        continue;
                    }
                    else
                    {
                        btn.setup(GetNameFromID(itemID, itemSource), Convert.ToUInt16("0x" + itemID, 16), Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(itemID, itemSource, Convert.ToUInt32("0x" + itemData, 16)), "", flag1, flag2);
                        continue;
                    }
                }
                return 1;
            }
            catch
            {
                if (s != null)
                {
                    s.Close();
                }
                return 0;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = version;

            this.ipBox.Text = ConfigurationManager.AppSettings["ipAddress"];

            if (File.Exists(itemPath))
            {
                //load the csv
                itemSource = loadItemCSV(itemPath);
                itemGridView.DataSource = loadItemCSV(itemPath);

                //set the ID row invisible
                itemGridView.Columns["ID"].Visible = false;

                //change the width of the first two columns
                itemGridView.Columns[0].Width = 150;
                itemGridView.Columns[1].Width = 65;

                //select the full row and change color cause windows blue sux
                itemGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                itemGridView.DefaultCellStyle.BackColor = Color.FromArgb(255, 47, 49, 54);
                itemGridView.DefaultCellStyle.ForeColor = Color.FromArgb(255, 114, 105, 110);
                itemGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 57, 60, 67);

                itemGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 57, 60, 67);
                itemGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                itemGridView.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 57, 60, 67);

                itemGridView.EnableHeadersVisualStyles = false;

                //create the image column
                DataGridViewImageColumn imageColumn = new DataGridViewImageColumn
                {
                    Name = "Image",
                    HeaderText = "Image",
                    ImageLayout = DataGridViewImageCellLayout.Zoom
                };
                itemGridView.Columns.Insert(3, imageColumn);
                imageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

                foreach (DataGridViewColumn c in itemGridView.Columns)
                {
                    c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    c.HeaderCell.Style.Font = new Font("Arial", 9, FontStyle.Bold);
                }
            }
            else
            {
                System.Media.SystemSounds.Asterisk.Play();
                MessageBox.Show("[Warning] Missing item.csv files !");
            }


            if (File.Exists(recipePath))
            {
                recipeSource = loadItemCSV(recipePath);
                recipeGridView.DataSource = loadItemCSV(recipePath);
                recipeGridView.Columns["ID"].Visible = false;
                recipeGridView.Columns[0].Width = 150;
                recipeGridView.Columns[1].Width = 65;

                recipeGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                recipeGridView.DefaultCellStyle.BackColor = Color.FromArgb(255, 47, 49, 54);
                recipeGridView.DefaultCellStyle.ForeColor = Color.FromArgb(255, 114, 105, 110);
                recipeGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 57, 60, 67);

                recipeGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 57, 60, 67);
                recipeGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                recipeGridView.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 57, 60, 67);

                recipeGridView.EnableHeadersVisualStyles = false;

                DataGridViewImageColumn recipeimageColumn = new DataGridViewImageColumn
                {
                    Name = "Image",
                    HeaderText = "Image",
                    ImageLayout = DataGridViewImageCellLayout.Zoom
                };
                recipeGridView.Columns.Insert(3, recipeimageColumn);
                recipeimageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

                foreach (DataGridViewColumn c in recipeGridView.Columns)
                {
                    c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    c.HeaderCell.Style.Font = new Font("Arial", 9, FontStyle.Bold);
                }
            }
            else
            {
                recipeModeBtn.Visible = false;
                recipeGridView.Visible = false;
            }

            if (File.Exists(flowerPath))
            {
                flowerGridView.DataSource = loadItemCSV(flowerPath);
                flowerGridView.Columns["ID"].Visible = false;
                flowerGridView.Columns["Value"].Visible = false;
                flowerGridView.Columns[0].Width = 150;
                flowerGridView.Columns[1].Width = 65;

                flowerGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                flowerGridView.DefaultCellStyle.BackColor = Color.FromArgb(255, 47, 49, 54);
                flowerGridView.DefaultCellStyle.ForeColor = Color.FromArgb(255, 114, 105, 110);
                flowerGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 57, 60, 67);

                flowerGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 57, 60, 67);
                flowerGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                flowerGridView.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 57, 60, 67);

                flowerGridView.EnableHeadersVisualStyles = false;

                DataGridViewImageColumn flowerimageColumn = new DataGridViewImageColumn
                {
                    Name = "Image",
                    HeaderText = "Image",
                    ImageLayout = DataGridViewImageCellLayout.Zoom
                };
                flowerGridView.Columns.Insert(3, flowerimageColumn);
                flowerimageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

                foreach (DataGridViewColumn c in flowerGridView.Columns)
                {
                    c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    c.HeaderCell.Style.Font = new Font("Arial", 9, FontStyle.Bold);
                }
            }
            else
            {
                flowerModeBtn.Visible = false;
                flowerGridView.Visible = false;
            }

            if (File.Exists(variationPath))
            {
                variationSource = loadItemCSV(variationPath);
            }
            else
            {
                variationModeButton.Visible = false;
            }

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\" + "img"))
            {
                ImageDownloader imageDownloader = new ImageDownloader();
                imageDownloader.ShowDialog();
            }

            currentPanel = itemModePanel;

            this.KeyPreview = true;
        }

        public IEnumerable<T> FindControls<T>(Control control) where T : Control
        {
            var controls = control.Controls.Cast<Control>();

            return controls.SelectMany(ctrl => FindControls<T>(ctrl))
                                      .Concat(controls)
                                      .Where(c => c.GetType() == typeof(T)).Cast<T>();
        }

        private void customIdBtn_Click(object sender, EventArgs e)
        {
            if (customIdTextbox.Text == "")
            {
                MessageBox.Show("Please enter an ID before sending item");
                return;
            }

            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

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

            if (customIdTextbox.Text == "16A2") //recipe
            {
                if (hexModeBtn.Tag.ToString() == "Normal")
                {
                    utilities.SpawnItem(s, bot, selectedSlot, selectedItem.getFlag1() + selectedItem.getFlag2() + customIdTextbox.Text, utilities.precedingZeros(hexValue, 8));
                    selectedButton.setup(GetNameFromID(utilities.turn2bytes(hexValue), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(utilities.turn2bytes(hexValue), recipeSource), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else
                {
                    utilities.SpawnItem(s, bot, selectedSlot, selectedItem.getFlag1() + selectedItem.getFlag2() + customIdTextbox.Text, customAmountTxt.Text);
                    selectedButton.setup(GetNameFromID(utilities.turn2bytes(customAmountTxt.Text), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(utilities.turn2bytes(customAmountTxt.Text), recipeSource), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
            }
            else if (customIdTextbox.Text == "114A") //money tree
            {
                if (hexModeBtn.Tag.ToString() == "Normal")
                {
                    utilities.SpawnItem(s, bot, selectedSlot, selectedItem.getFlag1() + selectedItem.getFlag2() + customIdTextbox.Text, utilities.precedingZeros(hexValue, 8));
                    selectedButton.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + hexValue, 16)), GetNameFromID((utilities.turn2bytes(hexValue)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else
                {
                    utilities.SpawnItem(s, bot, selectedSlot, selectedItem.getFlag1() + selectedItem.getFlag2() + customIdTextbox.Text, customAmountTxt.Text);
                    selectedButton.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), GetNameFromID((utilities.turn2bytes(customAmountTxt.Text)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                }
            }
            else
            {
                if (hexModeBtn.Tag.ToString() == "Normal")
                {
                    utilities.SpawnItem(s, bot, selectedSlot, selectedItem.getFlag1() + selectedItem.getFlag2() + customIdTextbox.Text, utilities.precedingZeros(hexValue, 8));
                    selectedButton.setup(GetNameFromID(utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + hexValue, 16)), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else
                {
                    utilities.SpawnItem(s, bot, selectedSlot, selectedItem.getFlag1() + selectedItem.getFlag2() + customIdTextbox.Text, customAmountTxt.Text);
                    selectedButton.setup(GetNameFromID(utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
            }

            /*
            if (hexModeBtn.Tag.ToString() == "Normal")
            {
                int decValue = int.Parse(customAmountTxt.Text) - 1;
                string hexValue;
                if (decValue < 0)
                    hexValue = "0";
                else
                    hexValue = decValue.ToString("X");

                utilities.SpawnItem(s, bot, selectedSlot, selectedItem.getFlag1() + selectedItem.getFlag2() + customIdTextbox.Text, utilities.precedingZeros(hexValue, 8));

                selectedButton.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + hexValue, 16)));
            }
            else
            {
                utilities.SpawnItem(s, bot, selectedSlot, selectedItem.getFlag1() + selectedItem.getFlag2() + customIdTextbox.Text, customAmountTxt.Text);

                if (customIdTextbox.Text == "16A2")
                {
                    selectedButton.setup(GetNameFromID(utilities.turn2bytes(customAmountTxt.Text), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(utilities.turn2bytes(customAmountTxt.Text), recipeSource), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else if (customIdTextbox.Text == "114A")
                {
                    selectedButton.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), GetNameFromID((utilities.turn2bytes(customAmountTxt.Text)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else
                {
                    selectedButton.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
            }
            */

            this.ShowMessage(customIdTextbox.Text);
        }

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

        private void itemSearchBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                (itemGridView.DataSource as DataTable).DefaultView.RowFilter = string.Format("Name LIKE '%{0}%'", itemSearchBox.Text);
                (recipeGridView.DataSource as DataTable).DefaultView.RowFilter = string.Format("Name LIKE '%{0}%'", itemSearchBox.Text);
                (flowerGridView.DataSource as DataTable).DefaultView.RowFilter = string.Format("Name LIKE '%{0}%'", itemSearchBox.Text);
            }
            catch
            {
                itemSearchBox.Clear();
            }
        }

        private void itemGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < this.itemGridView.Rows.Count)

            {
                if (e.ColumnIndex == 3)
                {
                    string path = @"img\" + itemGridView.Rows[e.RowIndex].Cells[1].Value.ToString() + @"\" + itemGridView.Rows[e.RowIndex].Cells[0].Value.ToString() + ".png";
                    if (File.Exists(path))
                    {
                        Image img = Image.FromFile(path);
                        e.Value = img;
                    }

                }
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
                    itemGridView.Rows[e.RowIndex].Height = 160;
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
                    customIdTextbox.Text = itemGridView.Rows[e.RowIndex].Cells[2].Value.ToString();

                    //Debug.Print(itemGridView.Rows[e.RowIndex].Cells[2].Value.ToString());
                    selectedItem.setup(itemGridView.Rows[e.RowIndex].Cells[0].Value.ToString(), Convert.ToUInt16("0x" + itemGridView.Rows[e.RowIndex].Cells[2].Value.ToString(), 16), 0x0, GetImagePathFromID(itemGridView.Rows[e.RowIndex].Cells[2].Value.ToString(), itemSource), true, "");
                    if (selection != null)
                    {
                        selection.receiveID(utilities.precedingZeros(selectedItem.fillItemID(), 4));
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
                    itemGridView.Rows[e.RowIndex].Height = 160;

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

                    if (customIdTextbox.Text == "114A")
                    {
                        customAmountTxt.Text = utilities.precedingZeros("20" + itemGridView.Rows[e.RowIndex].Cells[2].Value.ToString(), 8);
                    }
                    else
                    {
                        customAmountTxt.Text = utilities.precedingZeros(itemGridView.Rows[e.RowIndex].Cells[2].Value.ToString(), 8);
                    }

                    if (customIdTextbox.Text != "")
                    {
                        //Debug.Print(itemGridView.Rows[e.RowIndex].Cells[2].Value.ToString());
                        selectedItem.setup(name, Convert.ToUInt16(id, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), path, true, GetNameFromID(itemGridView.Rows[e.RowIndex].Cells[2].Value.ToString(), itemSource));
                        //Debug.Print(itemGridView.Rows[e.RowIndex].Cells[2].Value.ToString());

                        updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
                    }

                }
            }
        }

        private void slotBtn_Paint(object sender, PaintEventArgs e)
        {
            var b = sender as Button;
            var rect = e.ClipRectangle;
            rect.Inflate(-3, -2);
            var flags = TextFormatFlags.WordBreak;

            flags |= TextFormatFlags.Top | TextFormatFlags.Left;

            TextRenderer.DrawText(e.Graphics, b.Text, b.Font, rect, Color.White, Color.Black, flags);
        }


        private void deleteItemBtn_Click(object sender, EventArgs e)
        {
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            ToolStripItem item = (sender as ToolStripItem);
            if (item != null)
            {
                ContextMenuStrip owner = item.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    int slotId = int.Parse(owner.SourceControl.Tag.ToString());
                    utilities.DeleteSlot(s, bot, slotId);

                    var btnParent = (inventorySlot)owner.SourceControl;
                    btnParent.reset();
                    btnToolTip.RemoveAll();
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

            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

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

            byte[] b = new byte[160];
            byte[] ID = utilities.stringToByte(utilities.flip(utilities.precedingZeros(itemID, 8)));
            byte[] Data = utilities.stringToByte(utilities.flip(utilities.precedingZeros(itemAmount, 8)));

            //Debug.Print(utilities.precedingZeros(itemID, 8));
            //Debug.Print(utilities.precedingZeros(itemAmount, 8));

            for (int i = 0; i < b.Length; i += 8)
            {
                for (int j = 0; j < 4; j++)
                {
                    b[i + j] = ID[j];
                    b[i + j + 4] = Data[j];
                }
            }

            //string result = Encoding.ASCII.GetString(utilities.transform(b));
            //Debug.Print(result);

            utilities.OverwriteAll(s, bot, b, b);

            foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
            {
                Invoke((MethodInvoker)delegate
                {
                    btn.setup(GetNameFromID(utilities.turn2bytes(itemID), itemSource), Convert.ToUInt16("0x" + utilities.turn2bytes(itemID), 16), Convert.ToUInt32("0x" + itemAmount, 16), GetImagePathFromID(utilities.turn2bytes(itemID), itemSource, Convert.ToUInt32("0x" + itemAmount, 16)), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                });
            }

            Thread.Sleep(1000);
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

            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

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
            showWait();

            byte[] Bank01to20 = utilities.GetInventoryBank(s, bot, 1);
            byte[] Bank21to40 = utilities.GetInventoryBank(s, bot, 21);

            foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
            {
                int slot = int.Parse(btn.Tag.ToString());
                byte[] slotBytes = new byte[4];

                int slotOffset;
                if (slot < 21)
                {
                    slotOffset = ((slot - 1) * 0x10);
                }
                else
                {
                    slotOffset = ((slot - 21) * 0x10);
                }

                if (slot < 21)
                {
                    Buffer.BlockCopy(Bank01to20, slotOffset, slotBytes, 0x0, 0x4);
                }
                else
                {
                    Buffer.BlockCopy(Bank21to40, slotOffset, slotBytes, 0x0, 0x4);
                }

                string slotID = utilities.flip(Encoding.ASCII.GetString(slotBytes));

                if (slotID == "FFFE")
                {
                    utilities.SpawnItem(s, bot, slot, selectedItem.getFlag1() + selectedItem.getFlag2() + itemID, itemAmount);
                    Invoke((MethodInvoker)delegate
                    {
                        btn.setup(GetNameFromID(itemID, itemSource), Convert.ToUInt16("0x" + itemID, 16), Convert.ToUInt32("0x" + itemAmount, 16), GetImagePathFromID(itemID, itemSource, Convert.ToUInt32("0x" + itemAmount, 16)), "", selectedItem.getFlag1(), selectedItem.getFlag2());
                    });
                }
            }

            Thread.Sleep(3000);
            System.Media.SystemSounds.Asterisk.Play();
            hideWait();
        }

        private void refreshBtn_Click(object sender, EventArgs e)
        {
            UpdateInventory();
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void variationsBtn_Click(object sender, EventArgs e)
        {
            if (customIdTextbox.Text == "")
            {
                MessageBox.Show("Please enter an ID before sending item");
                return;
            }

            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            if (customAmountTxt.Text == "")
            {
                MessageBox.Show("Please enter an amount");
                return;
            }

            int slot = 21;

            for (int variation = 0; variation <= 9; variation++)
            {
                utilities.SpawnItem(s, bot, slot, customIdTextbox.Text, "0x" + utilities.precedingZeros(variation.ToString("X"), 2));

                slot++;
            }

            UpdateInventory();
            System.Media.SystemSounds.Asterisk.Play();

            this.ShowMessage(customIdTextbox.Text);
        }

        private void clearBtn_Click(object sender, EventArgs e)
        {
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            Thread clearThread = new Thread(clearInventory);
            clearThread.Start();
        }

        private void clearInventory()
        {
            showWait();

            byte[] b = new byte[160];

            //Debug.Print(utilities.precedingZeros(itemID, 8));
            //Debug.Print(utilities.precedingZeros(itemAmount, 8));

            for (int i = 0; i < b.Length; i += 8)
            {
                b[i] = 0xFE;
                b[i + 1] = 0xFF;
                for (int j = 0; j < 6; j++)
                {
                    b[i + 2 + j] = 0x00;
                }
            }

            utilities.OverwriteAll(s, bot, b, b);
            //string result = Encoding.ASCII.GetString(utilities.transform(b));
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
            System.Media.SystemSounds.Asterisk.Play();
            hideWait();
        }

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

        private void Player1Btn_CheckedChanged(object sender, EventArgs e)
        {
            utilities.setAddress(1);
            maxPage = 1;
            currentPage = 1;
            hidePagination();
            UpdateInventory();
        }

        private void Player2Btn_CheckedChanged(object sender, EventArgs e)
        {
            utilities.setAddress(2);
            maxPage = 1;
            currentPage = 1;
            hidePagination();
            UpdateInventory();
        }

        private void Player3Btn_CheckedChanged(object sender, EventArgs e)
        {
            utilities.setAddress(3);
            maxPage = 1;
            currentPage = 1;
            hidePagination();
            UpdateInventory();
        }

        private void Player4Btn_CheckedChanged(object sender, EventArgs e)
        {
            utilities.setAddress(4);
            maxPage = 1;
            currentPage = 1;
            hidePagination();
            UpdateInventory();
        }

        private void recyclingBtn_CheckedChanged(object sender, EventArgs e)
        {
            utilities.setAddress(9);
            maxPage = 2;
            currentPage = 1;
            showPagination();
            UpdateInventory();
        }

        private void houseBtn_CheckedChanged(object sender, EventArgs e)
        {
            utilities.setAddress(10);
            maxPage = 40;
            currentPage = 1;
            showPagination();
            UpdateInventory();
        }

        private void itemModeBtn_Click(object sender, EventArgs e)
        {
            this.itemModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            this.recipeModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.flowerModeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            this.recipeModePanel.Visible = false;
            this.itemModePanel.Visible = true;
            this.flowerModePanel.Visible = false;

            this.itemGridView.Visible = true;
            this.recipeGridView.Visible = false;
            this.flowerGridView.Visible = false;

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
                        selectedItem.setup(GetNameFromID(utilities.turn2bytes(hexValue), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(utilities.turn2bytes(hexValue), recipeSource), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                    else
                    {
                        selectedItem.setup(GetNameFromID(utilities.turn2bytes(customAmountTxt.Text), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(utilities.turn2bytes(customAmountTxt.Text), recipeSource), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                }
                else if (customIdTextbox.Text == "114A") //money tree
                {
                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + hexValue, 16)), true, GetNameFromID((utilities.turn2bytes(hexValue)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                    else
                    {
                        selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), true, GetNameFromID((utilities.turn2bytes(customAmountTxt.Text)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                }
                else
                {
                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        selectedItem.setup(GetNameFromID(utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + hexValue, 16)), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                    }
                    else
                    {
                        selectedItem.setup(GetNameFromID(utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                    }

                    if (selection != null)
                    {
                        selection.receiveID(customIdTextbox.Text);
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

            this.itemModePanel.Visible = false;
            this.recipeModePanel.Visible = true;
            this.flowerModePanel.Visible = false;

            this.itemGridView.Visible = false;
            this.recipeGridView.Visible = true;
            this.flowerGridView.Visible = false;

            this.variationModeButton.Visible = false;
            closeVariationMenu();

            if (recipeNum.Text != "")
            {
                //Debug.Print(GetNameFromID(recipeNum.Text, recipeSource));
                selectedItem.setup(GetNameFromID(utilities.turn2bytes(recipeNum.Text), recipeSource), 0x16A2, Convert.ToUInt32("0x" + recipeNum.Text, 16), GetImagePathFromID(utilities.turn2bytes(recipeNum.Text), recipeSource), true);
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

            this.itemModePanel.Visible = false;
            this.recipeModePanel.Visible = false;
            this.flowerModePanel.Visible = true;

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

        private void recipeGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < this.recipeGridView.Rows.Count)
            {
                if (e.ColumnIndex == 3)
                {
                    string path = @"img\" + recipeGridView.Rows[e.RowIndex].Cells[1].Value.ToString() + @"\" + recipeGridView.Rows[e.RowIndex].Cells[0].Value.ToString() + ".png";
                    //Debug.Print(path+"\r\n");
                    if (File.Exists(path))
                    {
                        Image img = Image.FromFile(path);
                        e.Value = img;
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
                recipeGridView.Rows[e.RowIndex].Height = 160;
                recipeNum.Text = recipeGridView.Rows[e.RowIndex].Cells[2].Value.ToString();

                selectedItem.setup(recipeGridView.Rows[e.RowIndex].Cells[0].Value.ToString(), 0x16A2, Convert.ToUInt32("0x" + recipeGridView.Rows[e.RowIndex].Cells[2].Value.ToString(), 16), GetImagePathFromID(recipeGridView.Rows[e.RowIndex].Cells[2].Value.ToString(), recipeSource), true);
                updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
            }
        }

        private void flowerGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < this.flowerGridView.Rows.Count)
            {
                if (e.ColumnIndex == 3)
                {
                    string path = @"img\" + flowerGridView.Rows[e.RowIndex].Cells[1].Value.ToString() + @"\" + flowerGridView.Rows[e.RowIndex].Cells[0].Value.ToString() + ".png";
                    //Debug.Print(path+"\r\n");
                    if (File.Exists(path))
                    {
                        Image img = Image.FromFile(path);
                        e.Value = img;
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
                flowerGridView.Rows[e.RowIndex].Height = 160;
                flowerID.Text = flowerGridView.Rows[e.RowIndex].Cells[2].Value.ToString();
                flowerValue.Text = flowerGridView.Rows[e.RowIndex].Cells[4].Value.ToString();

                selectedItem.setup(flowerGridView.Rows[e.RowIndex].Cells[0].Value.ToString(), Convert.ToUInt16("0x" + flowerGridView.Rows[e.RowIndex].Cells[2].Value.ToString(), 16), Convert.ToUInt32("0x" + flowerGridView.Rows[e.RowIndex].Cells[4].Value.ToString(), 16), GetImagePathFromID(flowerGridView.Rows[e.RowIndex].Cells[2].Value.ToString(), itemSource), true);
                updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
            }
        }

        private void spawnRecipeBtn_Click(object sender, EventArgs e)
        {
            if (recipeNum.Text == "")
            {
                MessageBox.Show("Please enter a recipe ID before sending item");
                return;
            }

            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            if (selectedButton == null)
            {
                MessageBox.Show("Please select a slot");
                return;
            }

            utilities.SpawnRecipe(s, bot, selectedSlot, "16A2", utilities.turn2bytes(recipeNum.Text));

            this.ShowMessage(utilities.turn2bytes(recipeNum.Text));

            selectedButton.setup(GetNameFromID(utilities.turn2bytes(recipeNum.Text), recipeSource), 0x16A2, Convert.ToUInt32("0x" + recipeNum.Text, 16), GetImagePathFromID(utilities.turn2bytes(recipeNum.Text), recipeSource));
        }

        public void KeyboardKeyDown(object sender, KeyEventArgs e)
        {
            //Debug.Print(e.KeyCode.ToString());
            if (e.KeyCode.ToString() == "F2" ^ e.KeyCode.ToString() == "Insert")
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

                if (s != null || bot != null)
                {
                    int nextSlot = findEmpty();
                    if (nextSlot > 0)
                    {
                        selectedSlot = nextSlot;
                        updateSlot(nextSlot);
                    }
                }
            }
            else if (e.KeyCode.ToString() == "F1")
            {
                deleteBtn_Click(sender, e);
            }
            else if (e.KeyCode.ToString() == "F3")
            {
                keyboardCopy(sender, e);
            }
            else if (e.KeyCode.ToString() == "End")
            {
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
                        customIdTextbox.Text = itemGridView.Rows[itemGridView.CurrentRow.Index].Cells[2].Value.ToString();

                        selectedItem.setup(itemGridView.Rows[itemGridView.CurrentRow.Index].Cells[0].Value.ToString(), Convert.ToUInt16("0x" + itemGridView.Rows[itemGridView.CurrentRow.Index].Cells[2].Value.ToString(), 16), 0x0, GetImagePathFromID(itemGridView.Rows[itemGridView.CurrentRow.Index].Cells[2].Value.ToString(), itemSource), true);
                        if (selection != null)
                        {
                            selection.receiveID(utilities.precedingZeros(selectedItem.fillItemID(), 4));
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
                        customIdTextbox.Text = itemGridView.Rows[itemGridView.CurrentRow.Index + 1].Cells[2].Value.ToString();

                        selectedItem.setup(itemGridView.Rows[itemGridView.CurrentRow.Index + 1].Cells[0].Value.ToString(), Convert.ToUInt16("0x" + itemGridView.Rows[itemGridView.CurrentRow.Index + 1].Cells[2].Value.ToString(), 16), 0x0, GetImagePathFromID(itemGridView.Rows[itemGridView.CurrentRow.Index + 1].Cells[2].Value.ToString(), itemSource), true);
                        if (selection != null)
                        {
                            selection.receiveID(utilities.precedingZeros(selectedItem.fillItemID(), 4));
                        }
                        updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());

                        itemGridView.CurrentCell = itemGridView.Rows[itemGridView.CurrentRow.Index + 1].Cells[0];

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

                        recipeNum.Text = recipeGridView.Rows[recipeGridView.CurrentRow.Index].Cells[2].Value.ToString();

                        selectedItem.setup(recipeGridView.Rows[recipeGridView.CurrentRow.Index].Cells[0].Value.ToString(), 0x16A2, Convert.ToUInt32("0x" + recipeGridView.Rows[recipeGridView.CurrentRow.Index].Cells[2].Value.ToString(), 16), GetImagePathFromID(recipeGridView.Rows[recipeGridView.CurrentRow.Index].Cells[2].Value.ToString(), recipeSource), true);
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

                        recipeNum.Text = recipeGridView.Rows[recipeGridView.CurrentRow.Index + 1].Cells[2].Value.ToString();

                        selectedItem.setup(recipeGridView.Rows[recipeGridView.CurrentRow.Index + 1].Cells[0].Value.ToString(), 0x16A2, Convert.ToUInt32("0x" + recipeGridView.Rows[recipeGridView.CurrentRow.Index + 1].Cells[2].Value.ToString(), 16), GetImagePathFromID(recipeGridView.Rows[recipeGridView.CurrentRow.Index + 1].Cells[2].Value.ToString(), recipeSource), true);
                        updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());

                        recipeGridView.CurrentCell = recipeGridView.Rows[recipeGridView.CurrentRow.Index + 1].Cells[0];
                    }
                }
                else if (currentPanel == flowerModePanel)
                {

                }
            }
            else if (e.KeyCode.ToString() == "Home")
            {
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
                        customIdTextbox.Text = itemGridView.Rows[itemGridView.CurrentRow.Index].Cells[2].Value.ToString();

                        selectedItem.setup(itemGridView.Rows[itemGridView.CurrentRow.Index].Cells[0].Value.ToString(), Convert.ToUInt16("0x" + itemGridView.Rows[itemGridView.CurrentRow.Index].Cells[2].Value.ToString(), 16), 0x0, GetImagePathFromID(itemGridView.Rows[itemGridView.CurrentRow.Index].Cells[2].Value.ToString(), itemSource), true);
                        if (selection != null)
                        {
                            selection.receiveID(utilities.precedingZeros(selectedItem.fillItemID(), 4));
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
                        customIdTextbox.Text = itemGridView.Rows[itemGridView.CurrentRow.Index - 1].Cells[2].Value.ToString();

                        selectedItem.setup(itemGridView.Rows[itemGridView.CurrentRow.Index - 1].Cells[0].Value.ToString(), Convert.ToUInt16("0x" + itemGridView.Rows[itemGridView.CurrentRow.Index - 1].Cells[2].Value.ToString(), 16), 0x0, GetImagePathFromID(itemGridView.Rows[itemGridView.CurrentRow.Index - 1].Cells[2].Value.ToString(), itemSource), true);
                        if (selection != null)
                        {
                            selection.receiveID(utilities.precedingZeros(selectedItem.fillItemID(), 4));
                        }
                        updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());

                        itemGridView.CurrentCell = itemGridView.Rows[itemGridView.CurrentRow.Index - 1].Cells[0];
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

                        recipeNum.Text = recipeGridView.Rows[recipeGridView.CurrentRow.Index].Cells[2].Value.ToString();

                        selectedItem.setup(recipeGridView.Rows[recipeGridView.CurrentRow.Index].Cells[0].Value.ToString(), 0x16A2, Convert.ToUInt32("0x" + recipeGridView.Rows[recipeGridView.CurrentRow.Index].Cells[2].Value.ToString(), 16), GetImagePathFromID(recipeGridView.Rows[recipeGridView.CurrentRow.Index].Cells[2].Value.ToString(), recipeSource), true);
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

                        recipeNum.Text = recipeGridView.Rows[recipeGridView.CurrentRow.Index - 1].Cells[2].Value.ToString();

                        selectedItem.setup(recipeGridView.Rows[recipeGridView.CurrentRow.Index - 1].Cells[0].Value.ToString(), 0x16A2, Convert.ToUInt32("0x" + recipeGridView.Rows[recipeGridView.CurrentRow.Index - 1].Cells[2].Value.ToString(), 16), GetImagePathFromID(recipeGridView.Rows[recipeGridView.CurrentRow.Index - 1].Cells[2].Value.ToString(), recipeSource), true);
                        updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());

                        recipeGridView.CurrentCell = recipeGridView.Rows[recipeGridView.CurrentRow.Index - 1].Cells[0];
                    }
                }
                else if (currentPanel == flowerModePanel)
                {

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
                selection.receiveID(utilities.precedingZeros(selectedItem.fillItemID(), 4));
            }
            updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
            customAmountTxt.Text = utilities.precedingZeros(selectedItem.fillItemData(), 8);
            customIdTextbox.Text = utilities.precedingZeros(selectedItem.fillItemID(), 4);
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void deleteBtn_Click(object sender, KeyEventArgs e)
        {
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            if (selectedButton == null)
            {
                MessageBox.Show("Please select a slot");
                return;
            }

            utilities.DeleteSlot(s, bot, int.Parse(selectedButton.Tag.ToString()));
            selectedButton.reset();
            btnToolTip.RemoveAll();

            //UpdateInventory();
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void ShowMessage(string itemID)
        {
            int rowIndex = -1;

            if (currentPanel == itemModePanel)
            {
                DataGridViewRow row = itemGridView.Rows
                .Cast<DataGridViewRow>()
                .Where(r => r.Cells["ID"].Value.ToString().Equals(itemID.ToLower()))
                .FirstOrDefault();

                if (row == null)
                {
                    row = itemGridView.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => r.Cells["ID"].Value.ToString().Equals(itemID))
                    .FirstOrDefault();

                    if (row == null)
                    {
                        return;
                    }
                }
                rowIndex = row.Index;
                msgLabel.Text = "Spawn " + itemGridView.Rows[rowIndex].Cells[0].Value.ToString();
            }
            else if (currentPanel == recipeModePanel)
            {
                DataGridViewRow row = recipeGridView.Rows
                .Cast<DataGridViewRow>()
                .Where(r => r.Cells["ID"].Value.ToString().Equals(itemID.ToLower()))
                .FirstOrDefault();

                if (row == null)
                {
                    row = recipeGridView.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => r.Cells["ID"].Value.ToString().Equals(itemID))
                    .FirstOrDefault();

                    if (row == null)
                    {
                        return;
                    }
                }
                rowIndex = row.Index;
                msgLabel.Text = "Spawn " + recipeGridView.Rows[rowIndex].Cells[0].Value.ToString() + " recipe";
            }
            else if (currentPanel == flowerModePanel)
            {
                DataGridViewRow row = flowerGridView.Rows
                .Cast<DataGridViewRow>()
                .Where(r => r.Cells["ID"].Value.ToString().Equals(itemID.ToLower()))
                .FirstOrDefault();

                if (row == null)
                {
                    row = flowerGridView.Rows
                    .Cast<DataGridViewRow>()
                    .Where(r => r.Cells["ID"].Value.ToString().Equals(itemID))
                    .FirstOrDefault();

                    if (row == null)
                    {
                        return;
                    }
                }
                rowIndex = row.Index;
                msgLabel.Text = "Spawn " + flowerGridView.Rows[rowIndex].Cells[0].Value.ToString() + " (Sparkling)";
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

        private void PeekBtn_Click(object sender, EventArgs e)
        {
            byte[] AddressBank = utilities.peekAddress(s, bot, "0x" + debugAddress.Text, 160);

            byte[] firstBytes = new byte[8];
            byte[] secondBytes = new byte[8];
            byte[] thirdBytes = new byte[8];
            byte[] fourthBytes = new byte[8];
            byte[] FullBytes = new byte[32];

            Buffer.BlockCopy(AddressBank, 0, firstBytes, 0x0, 0x8);
            Buffer.BlockCopy(AddressBank, 0x8, secondBytes, 0x0, 0x8);
            Buffer.BlockCopy(AddressBank, 0x10, thirdBytes, 0x0, 0x8);
            Buffer.BlockCopy(AddressBank, 0x18, fourthBytes, 0x0, 0x8);
            Buffer.BlockCopy(AddressBank, 0, FullBytes, 0x0, 0x20);

            string firstResult = Encoding.ASCII.GetString(firstBytes);
            string secondResult = Encoding.ASCII.GetString(secondBytes);
            string thirdResult = Encoding.ASCII.GetString(thirdBytes);
            string fourthResult = Encoding.ASCII.GetString(fourthBytes);
            string FullResult = Encoding.ASCII.GetString(FullBytes);

            Result1.Text = utilities.flip(firstResult);
            Result2.Text = utilities.flip(secondResult);
            Result3.Text = utilities.flip(thirdResult);
            Result4.Text = utilities.flip(fourthResult);
            FullAddress.Text = FullResult;
        }

        private void PeekMainBtn_Click(object sender, EventArgs e)
        {
            byte[] Bank = utilities.peekMainAddress(s, bot, "00E95FC0", 4);
            Debug.Print(Encoding.UTF8.GetString(Bank));
        }

        private void PokeMainBtn_Click(object sender, EventArgs e)
        {

        }

        private void spawnAllBtn3_Click(object sender, EventArgs e)
        {

        }

        private void spawnFlowerBtn_Click(object sender, EventArgs e)
        {
            if (flowerID.Text == "")
            {
                MessageBox.Show("Please select a flower");
                return;
            }

            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            if (selectedButton == null)
            {
                MessageBox.Show("Please select a slot");
                return;
            }

            utilities.SpawnFlower(s, bot, selectedSlot, flowerID.Text, flowerValue.Text);

            this.ShowMessage(flowerID.Text);

            selectedButton.setup(GetNameFromID(flowerID.Text, itemSource), Convert.ToUInt16("0x" + flowerID.Text, 16), Convert.ToUInt32("0x" + flowerValue.Text, 16), GetImagePathFromID(flowerID.Text, itemSource));

        }

        private void ChaseBtn_Click(object sender, EventArgs e)
        {
            UInt32 startAddress = 0xAB000000;
            UInt32 endAddress = 0xAD000000;

            UInt32 diff = ((endAddress - startAddress) / 10);

            //Debug.Print(diff.ToString("X") + " " + (startAddress + diff).ToString("X"));


            //Debug.Print(Encoding.Default.GetString(utilities.peekAddress(s, "0x" + (startAddress).ToString("X"))));


            //Debug.Print(Encoding.Default.GetString(output));

            //Debug.Print("Searching...");

            Thread SearchThread1 = new Thread(delegate () { SearchAddress(startAddress, startAddress + diff); });
            SearchThread1.Start();

            Thread SearchThread2 = new Thread(delegate () { SearchAddress(startAddress + diff * 1, startAddress + diff * 2); });
            SearchThread2.Start();

            Thread SearchThread3 = new Thread(delegate () { SearchAddress(startAddress + diff * 2, startAddress + diff * 3); });
            SearchThread3.Start();

            Thread SearchThread4 = new Thread(delegate () { SearchAddress(startAddress + diff * 3, startAddress + diff * 4); });
            SearchThread4.Start();

            Thread SearchThread5 = new Thread(delegate () { SearchAddress(startAddress + diff * 4, startAddress + diff * 5); });
            SearchThread5.Start();

            Thread SearchThread6 = new Thread(delegate () { SearchAddress(startAddress + diff * 5, startAddress + diff * 6); });
            SearchThread6.Start();

            Thread SearchThread7 = new Thread(delegate () { SearchAddress(startAddress + diff * 6, startAddress + diff * 7); });
            SearchThread7.Start();

            Thread SearchThread8 = new Thread(delegate () { SearchAddress(startAddress + diff * 7, startAddress + diff * 8); });
            SearchThread8.Start();

            Thread SearchThread9 = new Thread(delegate () { SearchAddress(startAddress + diff * 8, startAddress + diff * 9); });
            SearchThread9.Start();

            Thread SearchThread10 = new Thread(delegate () { SearchAddress(startAddress + diff * 9, endAddress); });
            SearchThread10.Start();
        }

        private void SearchAddress(UInt32 startAddress, UInt32 endAddress)
        {
            Debug.Print("Thread Start " + startAddress.ToString("X") + " " + endAddress.ToString("X"));

            byte[] result = Encoding.UTF8.GetBytes("400A000001000000C409000001000000");

            BoyerMoore boi = new BoyerMoore(result);

            for (UInt32 i = 0x0; startAddress + i <= endAddress; i += 500)
            {
                if (offsetFound)
                {
                    return;
                }

                byte[] b = utilities.peekAddress(s, bot, "0x" + (startAddress + i).ToString("X"), 160);
                Debug.Print(Encoding.UTF8.GetString(b));
                int NUM = boi.Search(b);

                if (NUM >= 0)
                {
                    Debug.Print(">> 0x" + (startAddress + i + NUM / 2).ToString("X") + " << DONE : 0x" + (NUM / 2).ToString("X"));
                    offsetFound = true;
                    return;
                }
            }

        }

        private void UpdateTownID()
        {
            byte[] townID = Utilities.GetTownID(s, bot);
            /*TownName.Text = String.Format("{1:X2} {2:X2} {3:X2} {4:X2} | {0}",
                Encoding.Unicode.GetString(townID, 4, 0x18),
                townID[3], townID[2], townID[1], townID[0]);*/
            //TownName.Text = Encoding.Unicode.GetString(townID, 4, 0x18);
            this.Text = this.Text + "  |  Island Name : " + Encoding.Unicode.GetString(townID, 4, 0x18);
        }

        private void readWeatherSeed()
        {
            byte[] b = utilities.transform(Utilities.GetWeatherSeed(s, bot));
            string result = Encoding.ASCII.GetString(b);
            UInt32 decValue = Convert.ToUInt32(utilities.flip(result), 16);
            UInt32 Seed = decValue - 2147483648;
            SeedTextbox.Text = Seed.ToString();
        }

        private void autoRefreshCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

            if (this.autoRefreshCheckBox.Checked)
            {
                config.AppSettings.Settings["autoRefresh"].Value = "true";
            }
            else
            {
                config.AppSettings.Settings["autoRefresh"].Value = "false";
            }

            config.Save(ConfigurationSaveMode.Minimal);
        }

        private void debugBtn_Click(object sender, EventArgs e)
        {
            currentGridView = insectGridView;
            LoadGridView(InsectAppearParam, insectGridView, ref insectRate, Utilities.InsectDataSize, Utilities.InsectNumRecords);
            LoadGridView(FishRiverAppearParam, riverFishGridView, ref riverFishRate, Utilities.FishDataSize, Utilities.FishRiverNumRecords, 1);
            LoadGridView(FishSeaAppearParam, seaFishGridView, ref seaFishRate, Utilities.FishDataSize, Utilities.FishSeaNumRecords, 1);
            LoadGridView(CreatureSeaAppearParam, seaCreatureGridView, ref seaCreatureRate, Utilities.SeaCreatureDataSize, Utilities.SeaCreatureNumRecords, 1);
        }

        private void PokeBtn_Click(object sender, EventArgs e)
        {
            utilities.pokeAddress(s, bot, "0x" + debugAddress.Text, debugAmount.Text);
        }

        private void eatBtn_Click(object sender, EventArgs e)
        {
            utilities.setStamina(s, bot, "0A");
            System.Media.SystemSounds.Asterisk.Play();
            setEatBtn();
        }

        private void poopBtn_Click(object sender, EventArgs e)
        {
            utilities.setStamina(s, bot, "00");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void setEatBtn()
        {
            eatBtn.Visible = true;
            Random rnd = new Random();
            int dice = rnd.Next(1, 8);

            switch (dice)
            {
                case 1:
                    eatBtn.Text = "Eat 10 Apples";
                    break;
                case 2:
                    eatBtn.Text = "Eat 10 Oranges";
                    break;
                case 3:
                    eatBtn.Text = "Eat 10 Cherries";
                    break;
                case 4:
                    eatBtn.Text = "Eat 10 Pears";
                    break;
                case 5:
                    eatBtn.Text = "Eat 10 Peaches";
                    break;
                case 6:
                    eatBtn.Text = "Eat 10 Coconuts";
                    break;
                default:
                    eatBtn.Text = "Eat 10 Turnips";
                    break;
            }
        }

        private void selectedItem_Click(object sender, EventArgs e)
        {
            if (selectedButton == null)
            {
                if (s != null || bot != null)
                {
                    int firstSlot = findEmpty();
                    if (firstSlot > 0)
                    {
                        selectedSlot = firstSlot;
                        updateSlot(firstSlot);
                    }
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
            if (s != null || bot != null)
            {
                int nextSlot = findEmpty();
                if (nextSlot > 0)
                {
                    selectedSlot = nextSlot;
                    updateSlot(nextSlot);
                }
            }
        }

        private void updateSlot(int select)
        {
            foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
            {
                if (btn.Tag == null)
                    continue;
                btn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
                if (int.Parse(btn.Tag.ToString()) == select)
                {
                    btn.FlatStyle = FlatStyle.Flat;
                    selectedButton = btn;
                    btn.BackColor = System.Drawing.Color.LightSeaGreen;
                }
            }
        }

        public void updateSelectedItemInfo(string Name, string ID, string Data)
        {
            selectedItemName.Text = Name;
            selectedID.Text = ID;
            selectedData.Text = Data;
            selectedFlag1.Text = selectedItem.getFlag1();
            selectedFlag2.Text = selectedItem.getFlag2();
        }

        public string GetNameFromID(string itemID, DataTable source)
        {
            if (source == null)
            {
                return "";
            }

            DataRow row = source.Rows.Find(itemID);

            if (row == null)
            {
                return ""; //row not found
            }
            else
            {
                //row found set the index and find the name
                return (string)row[0];
            }

        }

        public string GetImagePathFromID(string itemID, DataTable source, UInt32 data = 0)
        {
            if (source == null)
            {
                return "";
            }

            DataRow row = source.Rows.Find(itemID);
            DataRow VarRow = variationSource.Rows.Find(itemID);

            if (row == null)
            {
                return ""; //row not found
            }
            else
            {

                string path;
                if (VarRow != null & source != recipeSource)
                {
                    path = @"img\variation\" + VarRow[1] + @"\" + VarRow[3] + ".png";
                    if (File.Exists(path))
                    {
                        return path;
                    }
                    string main = (data & 0xF).ToString();
                    string sub = (((data & 0xFF) - (data & 0xF)) / 0x20).ToString();
                    //Debug.Print("data " + data.ToString("X") + " Main " + main + " Sub " + sub);
                    path = @"img\variation\" + VarRow[1] + @"\" + VarRow[3] + "_" + main + "_" + sub + ".png";
                    if (File.Exists(path))
                    {
                        return path;
                    }

                }

                path = @"img\" + row[1] + @"\" + row[0] + ".png";
                if (File.Exists(path))
                {
                    return path; //file found
                }
                else
                {
                    return ""; //file not found
                }
            }
        }

        private void inventory_Click(object sender, EventArgs e)
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
        }

        private void copyItemBtn_Click(object sender, EventArgs e)
        {
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            ToolStripItem item = (sender as ToolStripItem);
            if (item != null)
            {
                ContextMenuStrip owner = item.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    itemModeBtn_Click(sender, e);
                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        hexMode_Click(sender, e);
                    }
                    var btn = (inventorySlot)owner.SourceControl;
                    selectedItem.setup(btn);
                    if (selection != null)
                    {
                        selection.receiveID(utilities.precedingZeros(selectedItem.fillItemID(), 4));
                    }
                    updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
                    if (selectedItem.fillItemID() == "FFFE")
                    {
                        hexMode_Click(sender, e);
                        customAmountTxt.Text = "";
                        customIdTextbox.Text = "";
                    }
                    else
                    {
                        customAmountTxt.Text = utilities.precedingZeros(selectedItem.fillItemData(), 8);
                        customIdTextbox.Text = utilities.precedingZeros(selectedItem.fillItemID(), 4);
                    }
                    System.Media.SystemSounds.Asterisk.Play();
                }
            }
        }

        private void wrapItemBtn_Click(object sender, EventArgs e)
        {
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            ToolStripItem item = (sender as ToolStripItem);
            if (item != null)
            {
                ContextMenuStrip owner = item.Owner as ContextMenuStrip;
                if (owner != null)
                {
                    string flag = "00";
                    switch (wrapSetting.SelectedIndex)
                    {
                        case 0:
                            flag = "01";
                            break;

                        case 1:
                            flag = "05";
                            break;

                        case 2:
                            flag = "09";
                            break;

                        case 3:
                            flag = "0D";
                            break;

                        case 4:
                            flag = "11";
                            break;

                        case 5:
                            flag = "15";
                            break;

                        case 6:
                            flag = "19";
                            break;

                        case 7:
                            flag = "1D";
                            break;

                        case 8:
                            flag = "21";
                            break;

                        case 9:
                            flag = "25";
                            break;

                        case 10:
                            flag = "29";
                            break;

                        case 11:
                            flag = "2D";
                            break;

                        case 12:
                            flag = "31";
                            break;

                        case 13:
                            flag = "35";
                            break;

                        case 14:
                            flag = "39";
                            break;

                        case 15:
                            flag = "3D";
                            break;

                        case 16:
                            flag = "02";
                            break;

                        case 17:
                            flag = "43";
                            break;
                    }
                    int slotId = int.Parse(owner.SourceControl.Tag.ToString());
                    utilities.setFlag1(s, bot, slotId, flag);

                    var btnParent = (inventorySlot)owner.SourceControl;
                    btnParent.setFlag1(flag);
                    btnParent.refresh(false);
                    System.Media.SystemSounds.Asterisk.Play();
                }
            }
        }

        private void hexMode_Click(object sender, EventArgs e)
        {
            if (hexModeBtn.Tag.ToString() == "Normal")
            {
                AmountLabel.Text = "Hex Value";
                hexModeBtn.Tag = "Hex";
                hexModeBtn.Text = "Normal Mode";
                if (customAmountTxt.Text != "")
                {
                    int decValue = int.Parse(customAmountTxt.Text) - 1;
                    string hexValue;
                    if (decValue < 0)
                        hexValue = "0";
                    else
                        hexValue = decValue.ToString("X");
                    customAmountTxt.Text = utilities.precedingZeros(hexValue, 8);
                }
            }
            else
            {
                AmountLabel.Text = "Amount";
                hexModeBtn.Tag = "Normal";
                hexModeBtn.Text = "Hex Mode";
                if (customAmountTxt.Text != "")
                {
                    string hexValue = customAmountTxt.Text;
                    int decValue = Convert.ToInt32(hexValue, 16) + 1;
                    customAmountTxt.Text = decValue.ToString();
                }
            }
        }

        private void saveBtn_Click(object sender, EventArgs e)
        {
            /*
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to save your inventory?\n[Warning] Your previous save will be overwritten!", "Load inventory", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {*/
            try
            {
                SaveFileDialog file = new SaveFileDialog()
                {
                    Filter = "New Horizons Inventory (*.nhi)|*.nhi",
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

                byte[] Bank01to20 = utilities.GetInventoryBank(s, bot, 1);
                byte[] Bank21to40 = utilities.GetInventoryBank(s, bot, 21);
                string Bank = Encoding.ASCII.GetString(Bank01to20).Substring(0, 320) + Encoding.ASCII.GetString(Bank21to40).Substring(0, 320);
                //Debug.Print(Bank);

                byte[] save = new byte[320];

                for (int i = 0; i < Bank.Length / 2 - 1; i++)
                {

                    string data = String.Concat(Bank[(i * 2)].ToString(), Bank[((i * 2) + 1)].ToString());
                    //Debug.Print(i.ToString() + " " + data);
                    save[i] = Convert.ToByte(data, 16);
                }

                File.WriteAllBytes(file.FileName, save);
                System.Media.SystemSounds.Asterisk.Play();
                /*
                Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
                config.AppSettings.Settings["save01"].Value = Bank1.Substring(0, 320);
                config.AppSettings.Settings["save21"].Value = Bank2.Substring(0, 320);
                config.Save(ConfigurationSaveMode.Minimal);
                MessageBox.Show("Inventory Saved!");
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
            //}
        }

        private void loadBtn_Click(object sender, EventArgs e)
        {
            /*DialogResult dialogResult = MessageBox.Show("Are you sure you want to load your inventory?\n[Warning] All of your inventory slots will be overwritten!", "Load inventory", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {*/
            try
            {
                /*
                Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
                string Bank1 = config.AppSettings.Settings["save01"].Value;
                string Bank2 = config.AppSettings.Settings["save21"].Value;
                byte[] Bank01to20 = Encoding.ASCII.GetBytes(Bank1);
                byte[] Bank21to40 = Encoding.ASCII.GetBytes(Bank2);

                Thread LoadThread = new Thread(delegate () { loadInventory(Bank01to20, Bank21to40); });
                LoadThread.Start();
                */

                OpenFileDialog file = new OpenFileDialog()
                {
                    Filter = "New Horizons Inventory (*.nhi)|*.nhi|All files (*.*)|*.*",
                    FileName = "items.nhi",
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

                /*
                string bank = "";
                for (int i = 0; i < data.Length; i++)
                {
                    bank += utilities.precedingZeros(((UInt16)data[i]).ToString("X"), 2);
                }
                */
                //Debug.Print(bank);
                Thread LoadThread = new Thread(delegate () { loadInventory(data); });
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

            //}
        }

        private void loadInventory(byte[] data)
        {
            showWait();

            byte[] b1 = new byte[160];
            byte[] b2 = new byte[160];

            for (int i = 0; i < b1.Length; i++)
            {
                b1[i] = data[i];
                b2[i] = data[i + 160];
            }

            utilities.OverwriteAll(s, bot, b1, b2);
            /*
            foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
            {
                if (btn.Tag == null)
                    continue;

                if (btn.Tag.ToString() == "")
                    continue;

                int slotId = int.Parse(btn.Tag.ToString());

                int slotOffset = 0;
                int countOffset = 0;

                slotOffset = ((slotId - 1) * 16);
                countOffset = ((slotId - 1) * 16) + 8;

                string itemID = "";
                string itemData = "";

                string slotBytes = bank.Substring(slotOffset, 8);
                string dataBytes = bank.Substring(countOffset, 8);


                itemID = utilities.flip(slotBytes);
                itemData = utilities.flip(dataBytes);

                utilities.SpawnItem(s, bot, slotId, itemID, itemData);

            }
            */

            Invoke((MethodInvoker)delegate
            {
                UpdateInventory();
            });


            hideWait();
            System.Media.SystemSounds.Asterisk.Play();
        }

        private int findEmpty()
        {
            byte[] Bank01to20 = utilities.GetInventoryBank(s, bot, 1);
            byte[] Bank21to40 = utilities.GetInventoryBank(s, bot, 21);
            //string Bank1 = Encoding.ASCII.GetString(Bank01to20);
            //string Bank2 = Encoding.ASCII.GetString(Bank21to40);
            //Debug.Print(Bank1);
            //Debug.Print(Bank2);

            for (int slot = 1; slot <= 40; slot++)
            {
                byte[] slotBytes = new byte[4];

                int slotOffset;
                if (slot < 21)
                {
                    slotOffset = ((slot - 1) * 0x10);
                }
                else
                {
                    slotOffset = ((slot - 21) * 0x10);
                }

                if (slot < 21)
                {
                    Buffer.BlockCopy(Bank01to20, slotOffset, slotBytes, 0x0, 0x4);
                }
                else
                {
                    Buffer.BlockCopy(Bank21to40, slotOffset, slotBytes, 0x0, 0x4);
                }

                string itemID = utilities.flip(Encoding.ASCII.GetString(slotBytes));

                if (itemID == "FFFE")
                {
                    return slot;
                }
            }
            return -1;
        }

        private void inventoryBtn_Paint(object sender, PaintEventArgs e)
        {
            Font font = new Font("Arial", 10, FontStyle.Bold);
            Brush brush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
            e.Graphics.TranslateTransform(21, 7);
            e.Graphics.RotateTransform(90);
            e.Graphics.DrawString("Inventory", font, brush, 0, 0);
        }

        private void critterBtn_Paint(object sender, PaintEventArgs e)
        {
            Font font = new Font("Arial", 10, FontStyle.Bold);
            Brush brush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
            e.Graphics.TranslateTransform(21, 14);
            e.Graphics.RotateTransform(90);
            e.Graphics.DrawString("Critter", font, brush, 0, 0);
        }

        private void otherBtn_Paint(object sender, PaintEventArgs e)
        {
            Font font = new Font("Arial", 10, FontStyle.Bold);
            Brush brush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
            e.Graphics.TranslateTransform(21, 14);
            e.Graphics.RotateTransform(90);
            e.Graphics.DrawString("Other", font, brush, 0, 0);
        }

        private void inventoryBtn_Click(object sender, EventArgs e)
        {
            this.inventoryBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            this.critterBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.otherBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            inventoryLargePanel.Visible = true;
            otherLargePanel.Visible = false;
            critterLargePanel.Visible = false;
        }

        private void critterBtn_Click(object sender, EventArgs e)
        {
            this.inventoryBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.critterBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            this.otherBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            inventoryLargePanel.Visible = false;
            otherLargePanel.Visible = false;
            critterLargePanel.Visible = true;
            closeVariationMenu();
        }

        private void otherBtn_Click(object sender, EventArgs e)
        {
            this.inventoryBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.critterBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.otherBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));

            inventoryLargePanel.Visible = false;
            otherLargePanel.Visible = true;
            critterLargePanel.Visible = false;
            closeVariationMenu();
        }
        private void UpdateTurnipPrices()
        {
            UInt32[] turnipPrices = utilities.GetTurnipPrices(s, bot);
            turnipBuyPrice.Clear();
            turnipBuyPrice.SelectionAlignment = HorizontalAlignment.Center;
            turnipBuyPrice.Text = String.Format("{0}", turnipPrices[12]);
            int buyPrice = int.Parse(String.Format("{0}", turnipPrices[12]));

            turnipSell1AM.Clear();
            turnipSell1AM.Text = String.Format("{0}", turnipPrices[0]);
            int MondayAM = int.Parse(String.Format("{0}", turnipPrices[0]));
            setTurnipColor(buyPrice, MondayAM, turnipSell1AM);

            turnipSell1PM.Clear();
            turnipSell1PM.Text = String.Format("{0}", turnipPrices[1]);
            int MondayPM = int.Parse(String.Format("{0}", turnipPrices[1]));
            setTurnipColor(buyPrice, MondayPM, turnipSell1PM);

            turnipSell2AM.Clear();
            turnipSell2AM.Text = String.Format("{0}", turnipPrices[2]);
            int TuesdayAM = int.Parse(String.Format("{0}", turnipPrices[2]));
            setTurnipColor(buyPrice, TuesdayAM, turnipSell2AM);

            turnipSell2PM.Clear();
            turnipSell2PM.Text = String.Format("{0}", turnipPrices[3]);
            int TuesdayPM = int.Parse(String.Format("{0}", turnipPrices[3]));
            setTurnipColor(buyPrice, TuesdayPM, turnipSell2PM);

            turnipSell3AM.Clear();
            turnipSell3AM.Text = String.Format("{0}", turnipPrices[4]);
            int WednesdayAM = int.Parse(String.Format("{0}", turnipPrices[4]));
            setTurnipColor(buyPrice, WednesdayAM, turnipSell3AM);

            turnipSell3PM.Clear();
            turnipSell3PM.Text = String.Format("{0}", turnipPrices[5]);
            int WednesdayPM = int.Parse(String.Format("{0}", turnipPrices[5]));
            setTurnipColor(buyPrice, WednesdayPM, turnipSell3PM);

            turnipSell4AM.Clear();
            turnipSell4AM.Text = String.Format("{0}", turnipPrices[6]);
            int ThursdayAM = int.Parse(String.Format("{0}", turnipPrices[6]));
            setTurnipColor(buyPrice, ThursdayAM, turnipSell4AM);

            turnipSell4PM.Clear();
            turnipSell4PM.Text = String.Format("{0}", turnipPrices[7]);
            int ThursdayPM = int.Parse(String.Format("{0}", turnipPrices[7]));
            setTurnipColor(buyPrice, ThursdayPM, turnipSell4PM);

            turnipSell5AM.Clear();
            turnipSell5AM.Text = String.Format("{0}", turnipPrices[8]);
            int FridayAM = int.Parse(String.Format("{0}", turnipPrices[8]));
            setTurnipColor(buyPrice, FridayAM, turnipSell5AM);

            turnipSell5PM.Clear();
            turnipSell5PM.Text = String.Format("{0}", turnipPrices[9]);
            int FridayPM = int.Parse(String.Format("{0}", turnipPrices[9]));
            setTurnipColor(buyPrice, FridayPM, turnipSell5PM);

            turnipSell6AM.Clear();
            turnipSell6AM.Text = String.Format("{0}", turnipPrices[10]);
            int SaturdayAM = int.Parse(String.Format("{0}", turnipPrices[10]));
            setTurnipColor(buyPrice, SaturdayAM, turnipSell6AM);

            turnipSell6PM.Clear();
            turnipSell6PM.Text = String.Format("{0}", turnipPrices[11]);
            int SaturdayPM = int.Parse(String.Format("{0}", turnipPrices[11]));
            setTurnipColor(buyPrice, SaturdayPM, turnipSell6PM);

            int[] price = { MondayAM, MondayPM, TuesdayAM, TuesdayPM, WednesdayAM, WednesdayPM, ThursdayAM, ThursdayPM, FridayAM, FridayPM, SaturdayAM, SaturdayPM };
            int highest = findHighest(price);
            setStar(highest, MondayAM, MondayPM, TuesdayAM, TuesdayPM, WednesdayAM, WednesdayPM, ThursdayAM, ThursdayPM, FridayAM, FridayPM, SaturdayAM, SaturdayPM);
        }

        private void setTurnipColor(int buyPrice, int comparePrice, RichTextBox target)
        {
            target.SelectionAlignment = HorizontalAlignment.Center;

            if (comparePrice > buyPrice)
            {
                target.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            }
            else if (comparePrice < buyPrice)
            {
                target.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(255)))), ((int)(((byte)(0)))));
            }
        }

        private int findHighest(int[] price)
        {
            int highest = -1;
            for (int i = 0; i < price.Length; i++)
            {
                if (price[i] > highest)
                {
                    highest = price[i];
                }
            }
            return highest;
        }

        private void setStar(int highest, int MondayAM, int MondayPM, int TuesdayAM, int TuesdayPM, int WednesdayAM, int WednesdayPM, int ThursdayAM, int ThursdayPM, int FridayAM, int FridayPM, int SaturdayAM, int SaturdayPM)
        {
            if (MondayAM >= highest) { mondayAMStar.Visible = true; } else { mondayAMStar.Visible = false; };
            if (MondayPM >= highest) { mondayPMStar.Visible = true; } else { mondayPMStar.Visible = false; };

            if (TuesdayAM >= highest) { tuesdayAMStar.Visible = true; } else { tuesdayAMStar.Visible = false; };
            if (TuesdayPM >= highest) { tuesdayPMStar.Visible = true; } else { tuesdayPMStar.Visible = false; };

            if (WednesdayAM >= highest) { wednesdayAMStar.Visible = true; } else { wednesdayAMStar.Visible = false; };
            if (WednesdayPM >= highest) { wednesdayPMStar.Visible = true; } else { wednesdayPMStar.Visible = false; };

            if (ThursdayAM >= highest) { thursdayAMStar.Visible = true; } else { thursdayAMStar.Visible = false; };
            if (ThursdayPM >= highest) { thursdayPMStar.Visible = true; } else { thursdayPMStar.Visible = false; };

            if (FridayAM >= highest) { fridayAMStar.Visible = true; } else { fridayAMStar.Visible = false; };
            if (FridayPM >= highest) { fridayPMStar.Visible = true; } else { fridayPMStar.Visible = false; };

            if (SaturdayAM >= highest) { saturdayAMStar.Visible = true; } else { saturdayAMStar.Visible = false; };
            if (SaturdayPM >= highest) { saturdayPMStar.Visible = true; } else { saturdayPMStar.Visible = false; };
        }

        private void nextBtn_Click(object sender, EventArgs e)
        {
            if (currentPage < maxPage)
            {
                if (recyclingBtn.Checked)
                {
                    utilities.gotoRecyclingPage((uint)(currentPage + 1));
                }
                else if (houseBtn.Checked)
                {
                    utilities.gotoHousePage((uint)(currentPage + 1));
                }
                currentPage++;
                setPageLabel();
                UpdateInventory();
            }
            else
            {
                System.Media.SystemSounds.Asterisk.Play();
            }
        }

        private void backBtn_Click(object sender, EventArgs e)
        {
            if (currentPage > 1)
            {
                if (recyclingBtn.Checked)
                {
                    utilities.gotoRecyclingPage((uint)(currentPage - 1));
                }
                else if (houseBtn.Checked)
                {
                    utilities.gotoHousePage((uint)(currentPage - 1));
                }
                currentPage--;
                setPageLabel();
                UpdateInventory();
            }
            else
            {
                System.Media.SystemSounds.Asterisk.Play();
            }
        }

        private void fastNextBtn_Click(object sender, EventArgs e)
        {
            if (currentPage == maxPage)
            {
                System.Media.SystemSounds.Asterisk.Play();
                return;
            }
            if (currentPage + 10 < maxPage)
            {
                if (recyclingBtn.Checked)
                {
                    utilities.gotoRecyclingPage((uint)(currentPage + 1));
                }
                else if (houseBtn.Checked)
                {
                    utilities.gotoHousePage((uint)(currentPage + 1));
                }
                currentPage += 10;
            }
            else
            {
                if (recyclingBtn.Checked)
                {
                    utilities.gotoRecyclingPage((uint)maxPage);
                }
                else if (houseBtn.Checked)
                {
                    utilities.gotoHousePage((uint)maxPage);
                }
                currentPage = maxPage;
                System.Media.SystemSounds.Asterisk.Play();
            }
            setPageLabel();
            UpdateInventory();
        }

        private void fastBackBtn_Click(object sender, EventArgs e)
        {
            if (currentPage == 1)
            {
                System.Media.SystemSounds.Asterisk.Play();
                return;
            }
            if (currentPage - 10 > 1)
            {
                if (recyclingBtn.Checked)
                {
                    utilities.gotoRecyclingPage((uint)(currentPage - 10));
                }
                else if (houseBtn.Checked)
                {
                    utilities.gotoHousePage((uint)(currentPage - 10));
                }
                currentPage -= 10;
            }
            else
            {
                if (recyclingBtn.Checked)
                {
                    utilities.gotoRecyclingPage(1);
                }
                else if (houseBtn.Checked)
                {
                    utilities.gotoHousePage(1);
                }
                currentPage = 1;
                System.Media.SystemSounds.Asterisk.Play();
            }
            setPageLabel();
            UpdateInventory();
        }

        private void setPageLabel()
        {
            pageLabel.Text = "Page " + currentPage;
        }

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
            System.Media.SystemSounds.Asterisk.Play();

            DialogResult dialogResult = MessageBox.Show("Are you sure you want to set the turnip prices?\n[Warning] All original prices will be overwritten!", "Set turnip prices", MessageBoxButtons.YesNo);
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
                utilities.ChangeTurnipPrices(s, bot, prices);
                UpdateTurnipPrices();
                System.Media.SystemSounds.Asterisk.Play();
            }
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

        private void loadReaction()
        {
            byte[] reactionBank = utilities.getReaction(s, bot);
            //Debug.Print(Encoding.ASCII.GetString(reactionBank));

            byte[] reaction1 = new byte[2];
            byte[] reaction2 = new byte[2];
            byte[] reaction3 = new byte[2];
            byte[] reaction4 = new byte[2];
            byte[] reaction5 = new byte[2];
            byte[] reaction6 = new byte[2];
            byte[] reaction7 = new byte[2];
            byte[] reaction8 = new byte[2];

            Buffer.BlockCopy(reactionBank, 0, reaction1, 0x0, 0x2);
            Buffer.BlockCopy(reactionBank, 2, reaction2, 0x0, 0x2);
            Buffer.BlockCopy(reactionBank, 4, reaction3, 0x0, 0x2);
            Buffer.BlockCopy(reactionBank, 6, reaction4, 0x0, 0x2);
            Buffer.BlockCopy(reactionBank, 8, reaction5, 0x0, 0x2);
            Buffer.BlockCopy(reactionBank, 10, reaction6, 0x0, 0x2);
            Buffer.BlockCopy(reactionBank, 12, reaction7, 0x0, 0x2);
            Buffer.BlockCopy(reactionBank, 14, reaction8, 0x0, 0x2);

            setReactionBox(Encoding.ASCII.GetString(reaction1), reactionSlot1);
            setReactionBox(Encoding.ASCII.GetString(reaction2), reactionSlot2);
            setReactionBox(Encoding.ASCII.GetString(reaction3), reactionSlot3);
            setReactionBox(Encoding.ASCII.GetString(reaction4), reactionSlot4);
            setReactionBox(Encoding.ASCII.GetString(reaction5), reactionSlot5);
            setReactionBox(Encoding.ASCII.GetString(reaction6), reactionSlot6);
            setReactionBox(Encoding.ASCII.GetString(reaction7), reactionSlot7);
            setReactionBox(Encoding.ASCII.GetString(reaction8), reactionSlot8);
            //Debug.Print(Encoding.ASCII.GetString(reaction1) + " | " + Encoding.ASCII.GetString(reaction2) + " | " + Encoding.ASCII.GetString(reaction3) + " | " + Encoding.ASCII.GetString(reaction4));
            //Debug.Print(Encoding.ASCII.GetString(reaction5) + " | " + Encoding.ASCII.GetString(reaction6) + " | " + Encoding.ASCII.GetString(reaction7) + " | " + Encoding.ASCII.GetString(reaction8));
        }

        private void setReactionBox(string reaction, ComboBox box)
        {
            if (reaction == "00")
            {
                return;
            }
            string hexValue = reaction;
            int decValue = Convert.ToInt32(hexValue, 16) - 1;
            box.SelectedIndex = decValue;
        }

        private void setReactionBtn_Click(object sender, EventArgs e)
        {
            System.Media.SystemSounds.Asterisk.Play();
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to change your reaction wheel?\n[Warning] Your previous reaction wheel will be overwritten!", "Change Reaction Wheel", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                string reaction1 = (utilities.precedingZeros((reactionSlot1.SelectedIndex + 1).ToString("X"), 2) + utilities.precedingZeros((reactionSlot2.SelectedIndex + 1).ToString("X"), 2) + utilities.precedingZeros((reactionSlot3.SelectedIndex + 1).ToString("X"), 2) + utilities.precedingZeros((reactionSlot4.SelectedIndex + 1).ToString("X"), 2));
                string reaction2 = (utilities.precedingZeros((reactionSlot5.SelectedIndex + 1).ToString("X"), 2) + utilities.precedingZeros((reactionSlot6.SelectedIndex + 1).ToString("X"), 2) + utilities.precedingZeros((reactionSlot7.SelectedIndex + 1).ToString("X"), 2) + utilities.precedingZeros((reactionSlot8.SelectedIndex + 1).ToString("X"), 2));
                utilities.setReaction(s, bot, reaction1, reaction2);
                System.Media.SystemSounds.Asterisk.Play();
            }
        }

        private void inventory_MouseHover(object sender, EventArgs e)
        {
            var button = (inventorySlot)sender;
            if (!button.isEmpty())
            {
                if (button.getContainItemName() != "")
                {
                    btnToolTip.SetToolTip(button, button.displayItemName() + "\n\nID : " + button.displayItemID() + "\nCount : " + button.displayItemData() + "\nFlag : 0x" + button.getFlag1() + button.getFlag2() + "\nContain Item : " + button.getContainItemName());
                }
                else
                {
                    btnToolTip.SetToolTip(button, button.displayItemName() + "\n\nID : " + button.displayItemID() + "\nCount : " + button.displayItemData() + "\nFlag : 0x" + button.getFlag1() + button.getFlag2());
                }
            }
        }

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
            selection.receiveID(utilities.precedingZeros(selectedItem.fillItemID(), 4));
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

        private void Form1_LocationChanged(object sender, EventArgs e)
        {
            if (selection != null)
            {
                selection.Location = new System.Drawing.Point(this.Location.X + 7, this.Location.Y + 550);
            }
        }

        private void USBconnectBtn_Click(object sender, EventArgs e)
        {
            if (USBconnectBtn.Tag.ToString() == "connect")
            {
                bot = new USBBot();
                if (bot.Connect())
                {
                    if (UpdateInventory() == 0)
                        return;
                    UpdateTownID();
                    //InitTimer();
                    setEatBtn();
                    UpdateTurnipPrices();
                    loadReaction();
                    readWeatherSeed();
                    this.connectBtn.Visible = false;
                    this.refreshBtn.Visible = true;
                    this.playerSelectionPanel.Visible = true;
                    //this.autoRefreshCheckBox.Visible = true;
                    this.autoRefreshCheckBox.Checked = false;
                    this.saveBtn.Visible = true;
                    this.loadBtn.Visible = true;
                    this.otherBtn.Visible = true;
                    this.critterBtn.Visible = true;
                    this.wrapSetting.SelectedIndex = 0;
                    //this.selectedItem.setHide(true);
                    this.ipBox.Visible = false;
                    this.pictureBox1.Visible = false;
                    this.pokeMainCheatPanel.Visible = false;
                    this.Text += "  |  [Connected via USB]";

                    currentGridView = insectGridView;

                    LoadGridView(InsectAppearParam, insectGridView, ref insectRate, Utilities.InsectDataSize, Utilities.InsectNumRecords);
                    LoadGridView(FishRiverAppearParam, riverFishGridView, ref riverFishRate, Utilities.FishDataSize, Utilities.FishRiverNumRecords, 1);
                    LoadGridView(FishSeaAppearParam, seaFishGridView, ref seaFishRate, Utilities.FishDataSize, Utilities.FishSeaNumRecords, 1);
                    LoadGridView(CreatureSeaAppearParam, seaCreatureGridView, ref seaCreatureRate, Utilities.SeaCreatureDataSize, Utilities.SeaCreatureNumRecords, 1);

                    USBconnectBtn.Text = "Disconnect";
                    USBconnectBtn.Tag = "Disconnect";
                }
                else
                {
                    bot = null;
                }
            }
            else
            {
                bot.Disconnect();

                foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
                {
                    btn.reset();
                }

                this.connectBtn.Visible = true;
                this.connectBtn.Enabled = true;
                this.refreshBtn.Visible = false;
                this.playerSelectionPanel.Visible = false;
                this.autoRefreshCheckBox.Visible = false;

                this.saveBtn.Visible = false;
                this.loadBtn.Visible = false;
                inventoryBtn_Click(sender, e);
                this.otherBtn.Visible = false;
                this.critterBtn.Visible = false;
                this.ipBox.Visible = true;
                this.pictureBox1.Visible = true;

                this.USBconnectBtn.Text = "USB";
                this.USBconnectBtn.Tag = "connect";
                this.Text = version;
            }
        }

        public void ReceiveVariation(inventorySlot select)
        {
            //selectedItem.setHide(true);
            selectedItem.setup(select);
            if (hexModeBtn.Tag.ToString() == "Normal")
            {
                hexMode_Click(null, null);
            }
            updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
            customAmountTxt.Text = utilities.precedingZeros(selectedItem.fillItemData(), 8);
            customIdTextbox.Text = utilities.precedingZeros(selectedItem.fillItemID(), 4);
        }

        private void speedEnableBtn_Click(object sender, EventArgs e)
        {
            speedEnableBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            speedDisableBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            utilities.pokeMainAddress(s, bot, Utilities.wSpeedAddress.ToString("X"), "1E221001");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void speedDisableBtn_Click(object sender, EventArgs e)
        {
            speedDisableBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            speedEnableBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            utilities.pokeMainAddress(s, bot, Utilities.wSpeedAddress.ToString("X"), "BD4FAE61");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void disableCollisionBtn_Click(object sender, EventArgs e)
        {
            disableCollisionBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            enableCollisionBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            utilities.pokeMainAddress(s, bot, Utilities.CollisionAddress.ToString("X"), "12800014");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void enableCollisionBtn_Click(object sender, EventArgs e)
        {
            enableCollisionBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            disableCollisionBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            utilities.pokeMainAddress(s, bot, Utilities.CollisionAddress.ToString("X"), "B953B814");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void freezeTimeBtn_Click(object sender, EventArgs e)
        {
            freezeTimeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            unfreezeTimeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            utilities.pokeMainAddress(s, bot, Utilities.freezeTimeAddress.ToString("X"), "D503201F");
            readtime();
            timePanel.Visible = true;

            System.Media.SystemSounds.Asterisk.Play();
        }

        private void unfreezeTimeBtn_Click(object sender, EventArgs e)
        {
            unfreezeTimeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            freezeTimeBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            utilities.pokeMainAddress(s, bot, Utilities.freezeTimeAddress.ToString("X"), "F9203260");
            timePanel.Visible = false;
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void animationSpdx2_Click(object sender, EventArgs e)
        {
            animationSpdx2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            animationSpdx0_1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            animationSpdx50.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            animationSpdx1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            utilities.pokeMainAddress(s, bot, Utilities.aSpeedAddress.ToString("X"), "40000000");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void animationSpdx0_1_Click(object sender, EventArgs e)
        {
            animationSpdx0_1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            animationSpdx2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            animationSpdx50.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            animationSpdx1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            utilities.pokeMainAddress(s, bot, Utilities.aSpeedAddress.ToString("X"), "3DCCCCCD");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void animationSpdx50_Click(object sender, EventArgs e)
        {
            animationSpdx50.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            animationSpdx2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            animationSpdx0_1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            animationSpdx1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            utilities.pokeMainAddress(s, bot, Utilities.aSpeedAddress.ToString("X"), "42480000");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void animationSpdx1_Click(object sender, EventArgs e)
        {
            animationSpdx1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            animationSpdx2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            animationSpdx0_1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            animationSpdx50.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            utilities.pokeMainAddress(s, bot, Utilities.aSpeedAddress.ToString("X"), "3F800000");
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void readtime()
        {
            byte[] b = utilities.peekAddress(s, bot, "0x" + Utilities.readTimeAddress.ToString("X"), 6);
            string time = Encoding.UTF8.GetString(b);
            Debug.Print(time);

            yearTextbox.Clear();
            yearTextbox.SelectionAlignment = HorizontalAlignment.Center;
            yearTextbox.Text = Convert.ToInt32(utilities.flip(time.Substring(0, 4)), 16).ToString();

            monthTextbox.Clear();
            monthTextbox.SelectionAlignment = HorizontalAlignment.Center;
            monthTextbox.Text = Convert.ToInt32((time.Substring(4, 2)), 16).ToString();

            dayTextbox.Clear();
            dayTextbox.SelectionAlignment = HorizontalAlignment.Center;
            dayTextbox.Text = Convert.ToInt32((time.Substring(6, 2)), 16).ToString();

            hourTextbox.Clear();
            hourTextbox.SelectionAlignment = HorizontalAlignment.Center;
            hourTextbox.Text = Convert.ToInt32((time.Substring(8, 2)), 16).ToString();

            minTextbox.Clear();
            minTextbox.SelectionAlignment = HorizontalAlignment.Center;
            minTextbox.Text = Convert.ToInt32((time.Substring(10, 2)), 16).ToString();
        }

        private void settimeBtn_Click(object sender, EventArgs e)
        {
            int decYear = int.Parse(yearTextbox.Text);
            if (decYear > 2060)
            {
                decYear = 2060;
            }
            else if (decYear < 1970)
            {
                decYear = 1970;
            }
            yearTextbox.Text = decYear.ToString();
            string hexYear = decYear.ToString("X");

            int decMonth = int.Parse(monthTextbox.Text);
            if (decMonth > 12)
            {
                decMonth = 12;
            }
            else if (decMonth < 0)
            {
                decMonth = 1;
            }
            monthTextbox.Text = decMonth.ToString();
            string hexMonth = decMonth.ToString("X");

            int decDay = int.Parse(dayTextbox.Text);
            if (decDay > 31)
            {
                decDay = 31;
            }
            else if (decDay < 0)
            {
                decDay = 1;
            }
            dayTextbox.Text = decDay.ToString();
            string hexDay = decDay.ToString("X");

            int decHour = int.Parse(hourTextbox.Text);
            if (decHour > 23)
            {
                decHour = 23;
            }
            else if (decHour < 0)
            {
                decHour = 0;
            }
            hourTextbox.Text = decHour.ToString();
            string hexHour = decHour.ToString("X");


            int decMin = int.Parse(minTextbox.Text);
            if (decMin > 59)
            {
                decMin = 59;
            }
            else if (decMin < 0)
            {
                decMin = 0;
            }
            minTextbox.Text = decMin.ToString();
            string hexMin = decMin.ToString("X");

            utilities.pokeAddress(s, bot, "0x" + Utilities.readTimeAddress.ToString("X"), utilities.flip(utilities.precedingZeros(hexYear, 4)));

            utilities.pokeAddress(s, bot, "0x" + (Utilities.readTimeAddress + 0x2).ToString("X"), utilities.precedingZeros(hexMonth, 2) + utilities.precedingZeros(hexDay, 2) + utilities.precedingZeros(hexHour, 2) + utilities.precedingZeros(hexMin, 2));

            System.Media.SystemSounds.Asterisk.Play();
        }

        private void minus1HourBtn_Click(object sender, EventArgs e)
        {
            int decHour = int.Parse(hourTextbox.Text) - 1;
            if (decHour < 0)
            {
                decHour = 23;
                string hexHour = decHour.ToString("X");

                int decDay = int.Parse(dayTextbox.Text) - 1;
                string hexDay = decDay.ToString("X");

                utilities.pokeAddress(s, bot, "0x" + (Utilities.readTimeAddress + 0x3).ToString("X"), utilities.precedingZeros(hexDay, 2) + utilities.precedingZeros(hexHour, 2));
            }
            else
            {
                string hexHour = decHour.ToString("X");
                utilities.pokeAddress(s, bot, "0x" + (Utilities.readTimeAddress + 0x4).ToString("X"), utilities.precedingZeros(hexHour, 2));
            }
            readtime();
            System.Media.SystemSounds.Asterisk.Play();
        }

        private void add1HourBtn_Click(object sender, EventArgs e)
        {
            int decHour = int.Parse(hourTextbox.Text) + 1;
            if (decHour >= 24)
            {
                decHour = 0;
                string hexHour = decHour.ToString("X");

                int decDay = int.Parse(dayTextbox.Text) + 1;
                string hexDay = decDay.ToString("X");

                utilities.pokeAddress(s, bot, "0x" + (Utilities.readTimeAddress + 0x3).ToString("X"), utilities.precedingZeros(hexDay, 2) + utilities.precedingZeros(hexHour, 2));
            }
            else
            {
                string hexHour = decHour.ToString("X");
                utilities.pokeAddress(s, bot, "0x" + (Utilities.readTimeAddress + 0x4).ToString("X"), utilities.precedingZeros(hexHour, 2));
            }
            readtime();
            System.Media.SystemSounds.Asterisk.Play();
        }

        private Boolean validation()
        {
            byte[] Bank1 = utilities.peekAddress(s, bot, "0x" + Utilities.TownNameddress.ToString("X"), 150); //TownNameddress
            byte[] Bank2 = utilities.peekAddress(s, bot, "0x" + Utilities.TurnipPurchasePriceAddr.ToString("X"), 150); //TurnipPurchasePriceAddr
            byte[] Bank3 = utilities.peekAddress(s, bot, "0x" + Utilities.MasterRecyclingBase.ToString("X"), 150); //MasterRecyclingBase
            byte[] Bank4 = utilities.peekAddress(s, bot, "0x" + Utilities.reactionAddress.ToString("X"), 150); //reactionAddress
            byte[] Bank5 = utilities.peekAddress(s, bot, "0x" + Utilities.staminaAddress.ToString("X"), 150); //staminaAddress

            string result1 = Encoding.UTF8.GetString(Bank1);
            string result2 = Encoding.UTF8.GetString(Bank2);
            string result3 = Encoding.UTF8.GetString(Bank3);
            string result4 = Encoding.UTF8.GetString(Bank4);
            string result5 = Encoding.UTF8.GetString(Bank5);

            Debug.Print(result1);
            Debug.Print(result2);
            Debug.Print(result3);
            Debug.Print(result4);
            Debug.Print(result5);

            int count1 = 0;
            if (result1 == result2)
            { count1++; }
            if (result1 == result3)
            { count1++; }
            if (result1 == result4)
            { count1++; }
            if (result1 == result5)
            { count1++; }

            int count2 = 0;
            if (result2 == result3)
            { count2++; }
            if (result2 == result4)
            { count2++; }
            if (result2 == result5)
            { count2++; }

            int count3 = 0;
            if (result3 == result4)
            { count3++; }
            if (result3 == result5)
            { count3++; }
            Debug.Print(count1.ToString() + " " + count2.ToString() + " " + count3.ToString());
            if (count1 > 1 ^ count2 > 1 ^ count3 > 1)
            { return true; }
            else
            { return false; }
        }

        //=====================================================================================================================================================================
        private void LoadGridView(byte[] source, DataGridView grid, ref int[] rate, int size, int num, int mode = 0)
        {
            if (source != null)
            {
                grid.DataSource = null;
                grid.Rows.Clear();
                grid.Columns.Clear();

                DataTable dt = new DataTable();

                grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                grid.DefaultCellStyle.BackColor = Color.FromArgb(255, 47, 49, 54);
                grid.DefaultCellStyle.ForeColor = Color.White;
                //grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(0, 57, 60, 67);


                grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 57, 60, 67);
                grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 57, 60, 67);
                grid.EnableHeadersVisualStyles = false;

                DataGridViewCellStyle btnStyle = new DataGridViewCellStyle();
                btnStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
                btnStyle.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
                btnStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                DataGridViewCellStyle selectedbtnStyle = new DataGridViewCellStyle();
                selectedbtnStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
                selectedbtnStyle.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
                selectedbtnStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                DataGridViewCellStyle FontStyle = new DataGridViewCellStyle();
                FontStyle.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
                FontStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

                dt.Columns.Add("Index", typeof(int));
                dt.Columns.Add("Name", typeof(string));
                dt.Columns.Add("ID", typeof(string));
                dt.Columns.Add(" ", typeof(int));


                UInt16 Id;
                string Name;

                for (int i = 0; i < num / 2; i++)
                {
                    Id = (UInt16)(source[i * size * 2]
                             + (source[i * size * 2 + 1] << 8));
                    Name = GetNameFromID(String.Format("{0:X4}", Id), itemSource);
                    int spawnRate = 0;
                    if (grid == insectGridView)
                    {
                        spawnRate = getSpawnRate(source, i * size * 2 + 2, size - 12);
                    }
                    else if (grid == seaCreatureGridView)
                    {
                        spawnRate = getSpawnRate(source, i * size * 2 + 2, size - 10);
                    }
                    else
                    {
                        spawnRate = getSpawnRate(source, i * size * 2 + 2, size - 8);
                    }
                    dt.Rows.Add(new object[] { i, Name, String.Format("{0:X4}", Id), spawnRate });
                }
                grid.DataSource = dt;


                DataGridViewImageColumn imageColumn = new DataGridViewImageColumn
                {
                    Name = "Image",
                    HeaderText = "Image",
                    ImageLayout = DataGridViewImageCellLayout.Zoom,
                    Resizable = DataGridViewTriState.False,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                };
                grid.Columns.Insert(0, imageColumn);

                // Index
                grid.Columns[1].DefaultCellStyle = FontStyle;
                grid.Columns[1].Width = 50;
                //grid.Columns[1].Visible = false;
                grid.Columns[1].Resizable = DataGridViewTriState.False;

                // Name
                grid.Columns[2].DefaultCellStyle = FontStyle;
                grid.Columns[2].Width = 250;
                grid.Columns[2].Resizable = DataGridViewTriState.False;

                // ID
                grid.Columns[3].DefaultCellStyle = FontStyle;
                //grid.Columns[3].Visible = false;
                grid.Columns[3].Width = 60;
                grid.Columns[3].Resizable = DataGridViewTriState.False;

                // Rate
                grid.Columns[4].DefaultCellStyle = FontStyle;
                grid.Columns[4].Width = 50;
                grid.Columns[4].Visible = false;
                //grid.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                grid.Columns[4].Resizable = DataGridViewTriState.False;

                DataGridViewProgressColumn barColumn = new DataGridViewProgressColumn
                {
                    Name = "Bar",
                    HeaderText = "",
                    DefaultCellStyle = FontStyle,
                    Width = 320,
                    Resizable = DataGridViewTriState.False,
                };
                grid.Columns.Add(barColumn);

                DataGridViewButtonColumn minColumn = new DataGridViewButtonColumn
                {
                    Name = "Min",
                    HeaderText = "",
                    FlatStyle = FlatStyle.Popup,
                    DefaultCellStyle = btnStyle,
                    Width = 100,
                    Text = "Disable Spawn",
                    UseColumnTextForButtonValue = true,
                    Resizable = DataGridViewTriState.False,
                };
                grid.Columns.Add(minColumn);

                DataGridViewTextBoxColumn separator1 = new DataGridViewTextBoxColumn
                {
                    Width = 10,
                    Resizable = DataGridViewTriState.False,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                };
                grid.Columns.Add(separator1);

                DataGridViewButtonColumn defaultColumn = new DataGridViewButtonColumn
                {
                    Name = "Default",
                    HeaderText = "",
                    FlatStyle = FlatStyle.Popup,
                    DefaultCellStyle = selectedbtnStyle,
                    Width = 100,
                    Text = "Default",
                    UseColumnTextForButtonValue = true,
                    Resizable = DataGridViewTriState.False,
                };
                grid.Columns.Add(defaultColumn);

                DataGridViewTextBoxColumn separator2 = new DataGridViewTextBoxColumn
                {
                    Width = 10,
                    Resizable = DataGridViewTriState.False,
                    SortMode = DataGridViewColumnSortMode.NotSortable,
                };
                grid.Columns.Add(separator2);

                DataGridViewButtonColumn maxColumn = new DataGridViewButtonColumn
                {
                    Name = "Max",
                    HeaderText = "",
                    FlatStyle = FlatStyle.Popup,
                    DefaultCellStyle = btnStyle,
                    Width = 100,
                    Text = "Max Spawn",
                    UseColumnTextForButtonValue = true,
                    Resizable = DataGridViewTriState.False,
                };
                grid.Columns.Add(maxColumn);

                rate = new int[num / 2];

                for (int i = 0; i < num / 2; i++)
                {
                    Id = (UInt16)(source[i * size * 2]
                            + (source[i * size * 2 + 1] << 8));

                    int spawnRate = 0;
                    if (grid == insectGridView)
                    {
                        spawnRate = getSpawnRate(source, i * size * 2 + 2, size - 12);
                    }
                    else if (grid == seaCreatureGridView)
                    {
                        spawnRate = getSpawnRate(source, i * size * 2 + 2, size - 10);
                    }
                    else
                    {
                        spawnRate = getSpawnRate(source, i * size * 2 + 2, size - 8);
                    }

                    rate[i] = spawnRate;

                    DataGridViewProgressCell pc = new DataGridViewProgressCell
                    {
                        setValue = spawnRate,
                        remark = getRemark(String.Format("{0:X4}", Id)),
                        mode = mode,
                    };
                    grid.Rows[i].Cells[5] = pc;
                }

                grid.ColumnHeadersVisible = false;
                grid.ClearSelection();
            }
        }

        private int getSpawnRate(byte[] source, int index, int size)
        {
            int max = 0;
            for (int i = 0; i < size; i++)
            {
                if (source[index + i] > max)
                    max = source[index + i];
            }
            return max;
        }

        private string getRemark(string ID)
        {
            switch (ID)
            {
                case ("0272"): //Common butterfly
                    return ("Except on rainy days");
                case ("0271"): //Yellow butterfly
                    return ("Except on rainy days");
                case ("0247"): //Tiger butterfly
                    return ("Except on rainy days");
                case ("0262"): //Peacock butterfly
                    return ("Catch 20 or more bugs to spawn\nExcept on rainy days");
                case ("0D95"): //Common bluebottle
                    return ("Except on rainy days");
                case ("0D96"): //Paper kite butterfly
                    return ("Except on rainy days");
                case ("0D97"): //Great purple emperor
                    return ("Catch 50 or more bugs to spawn\nExcept on rainy days");
                case ("027C"): //Monarch butterfly
                    return ("Except on rainy days");
                case ("0273"): //Emperor butterfly
                    return ("Catch 20 or more bugs to spawn\nExcept on rainy days");
                case ("026C"): //Agrias butterfly
                    return ("Catch 20 or more bugs to spawn\nExcept on rainy days");
                case ("0248"): //Rajah brooke's birdwing
                    return ("Catch 20 or more bugs to spawn\nExcept on rainy days");
                case ("024A"): //Queen Alexandra's birdwing
                    return ("Catch 50 or more bugs to spawn\nExcept on rainy days");
                case ("0250"): //Moth
                    return ("Except on rainy days");
                case ("028C"): //Atlas moth
                    return ("Catch 20 or more bugs to spawn");
                case ("0D9C"): //Madagascan sunset moth
                    return ("Catch 20 or more bugs to spawn\nExcept on rainy days");

                case ("0284"): //Long locust
                    return ("");
                case ("0288"): //Migratory Locust
                    return ("Catch 20 or more bugs to spawn");
                case ("025D"): //Rice grasshopper
                    return ("");
                case ("0265"): //Grasshopper
                    return ("Except on rainy days");

                case ("0269"): //Cricket
                    return ("Except on rainy days");
                case ("0282"): //Bell cricket
                    return ("Except on rainy days");

                case ("025F"): //Mantis
                    return ("Except on rainy days");
                case ("0256"): //Orchid mantis
                    return ("Catch 20 or more bugs to spawn\nExcept on rainy days");

                case ("026F"): //Honeybee
                    return ("Except on rainy days");
                case ("0283"): //Wasp
                    return ("✶ Spawn rate seems to have no effect\nSpawn when nest falls from tree");

                case ("0246"): //Brown cicada
                    return ("");
                case ("026D"): //Robust cicada
                    return ("");
                case ("026A"): //Giant cicada
                    return ("Catch 20 or more bugs to spawn");
                case ("0289"): //Walker cicada
                    return ("");
                case ("0259"): //Evening cicada
                    return ("");

                case ("0281"): //Cicada shell
                    return ("Catch 50 or more bugs to spawn");

                case ("0249"): //Red dragonfly
                    return ("Except on rainy days");
                case ("0253"): //Darner dragonfly
                    return ("Except on rainy days");
                case ("027B"): //Banded dragonfly
                    return ("Catch 50 or more bugs to spawn\nExcept on rainy days");
                case ("14DB"): //Damselfly
                    return ("Except on rainy days");

                case ("025B"): //Firefly
                    return ("Except on rainy days");

                case ("027A"): //Mole cricket
                    return ("");

                case ("024B"): //Pondskater
                    return ("");
                case ("0252"): //Diving beetle
                    return ("");
                case ("1425"): //Giant water bug
                    return ("Catch 50 or more bugs to spawn");

                case ("0260"): //Stinkbug
                    return ("Except on rainy days");
                case ("0D9B"): //Man-faced stink bug
                    return ("Catch 20 or more bugs to spawn\nExcept on rainy days");

                case ("0287"): //Ladybug
                    return ("Except on rainy days");

                case ("0257"): //Tiger beetle
                    return ("Catch 20 or more bugs to spawn\nExcept on rainy days");
                case ("0285"): //Jewel beetle
                    return ("Catch 20 or more bugs to spawn\nExcept on rainy days");
                case ("028A"): //Violin beetle
                    return ("Except on rainy days");
                case ("0261"): //Citrus long-horned beetle
                    return ("Except on rainy days");
                case ("0D9F"): //Rosalia batesi beetle
                    return ("Catch 20 or more bugs to spawn\nExcept on rainy days");
                case ("0D9D"): //Blue weevil beetle
                    return ("");
                case ("025C"): //Dung beetle
                    return ("Spawn when there is snowball on the ground");
                case ("0266"): //Earth-boring dung beetle
                    return ("");
                case ("027F"): //Scarab beetle
                    return ("Catch 50 or more bugs to spawn");
                case ("0D98"): //Drone beetle
                    return ("");
                case ("0254"): //Goliath beetle
                    return ("Catch 100 or more bugs to spawn");

                case ("0278"): //Saw stag
                    return ("");
                case ("0270"): //Miyama stag
                    return ("");
                case ("027D"): //Giant stag
                    return ("Catch 50 or more bugs to spawn");
                case ("0277"): //Rainbow stag
                    return ("Catch 50 or more bugs to spawn");
                case ("025A"): //Cyclommatus stag
                    return ("Catch 100 or more bugs to spawn");
                case ("027E"): //Golden stag
                    return ("Catch 100 or more bugs to spawn");
                case ("0D9A"): //Giraffe stag
                    return ("Catch 100 or more bugs to spawn");

                case ("0264"): //Horned dynastid
                    return ("");
                case ("0267"): //Horned atlas
                    return ("Catch 100 or more bugs to spawn");
                case ("028D"): //Horned elephant
                    return ("Catch 100 or more bugs to spawn");
                case ("0258"): //Horned hercules
                    return ("Catch 100 or more bugs to spawn");

                case ("0276"): //Walking stick
                    return ("Catch 20 or more bugs to spawn");
                case ("0268"): //Walking leaf
                    return ("Catch 20 or more bugs to spawn");
                case ("026E"): //Bagworm
                    return ("✶ Spawn rate seems to have no effect\nSpawn when shaking tree");
                case ("024C"): //Ant
                    return ("✶ Spawn rate seems to have no effect\nSpawn when there is rotten turnip");
                case ("028B"): //Hermit crab
                    return ("Spawn on beach");
                case ("024F"): //Wharf roach
                    return ("Spawn on rocky formations at beach");
                case ("0255"): //Fly
                    return ("✶ Spawn rate seems to have no effect\nSpawn when there is trash item");
                case ("025E"): //Mosquito
                    return ("Except on rainy days");
                case ("0279"): //Flea
                    return ("Spawn on villagers");
                case ("0263"): //Snail
                    return ("Rainy days only");
                case ("024E"): //Pill bug
                    return ("Spawn underneath rocks");
                case ("0274"): //Centipede
                    return ("Spawn underneath rocks");
                case ("026B"): //Spider
                    return ("✶ Spawn rate seems to have no effect\nSpawn when shaking tree");

                case ("0286"): //Tarantula
                    return ("");
                case ("0280"): //Scorpion
                    return ("");

                case ("0DD3"): //Snowflake
                    return ("Spawn when in season/time");
                case ("16E3"): //Cherry-blossom petal
                    return ("Spawn when in season/time");
                case ("1CCE"): //Maple leaf
                    return ("Spawn when in season/time");

                case ("08AC"): //Koi
                    return ("Catch 20 or more fishes to spawn");
                case ("1486"): //Ranchu Goldfish
                    return ("Catch 20 or more fishes to spawn");
                case ("08B0"): //Soft-shelled Turtle
                    return ("Catch 20 or more fishes to spawn");
                case ("08B7"): //Giant Snakehead
                    return ("Catch 50 or more fishes to spawn");
                case ("08BB"): //Pike
                    return ("Catch 20 or more fishes to spawn");
                case ("08BF"): //Char
                    return ("Catch 20 or more fishes to spawn");
                case ("1061"): //Golden Trout
                    return ("Catch 100 or more fishes to spawn");
                case ("08C1"): //Stringfish
                    return ("Catch 100 or more fishes to spawn");
                case ("08C3"): //King Salmon
                    return ("Catch 20 or more fishes to spawn");
                case ("08C4"): //Mitten Crab
                    return ("Catch 20 or more fishes to spawn");
                case ("08C6"): //Nibble Fish
                    return ("Catch 20 or more fishes to spawn");
                case ("08C7"): //Angelfish
                    return ("Catch 20 or more fishes to spawn");
                case ("105F"): //Betta
                    return ("Catch 20 or more fishes to spawn");
                case ("08C9"): //Piranha
                    return ("Catch 20 or more fishes to spawn");
                case ("08CA"): //Arowana
                    return ("Catch 50 or more fishes to spawn");
                case ("08CB"): //Dorado
                    return ("Catch 100 or more fishes to spawn");
                case ("08CC"): //Gar
                    return ("Catch 50 or more fishes to spawn");
                case ("08CD"): //Arapaima
                    return ("Catch 50 or more fishes to spawn");
                case ("08CE"): //Saddled Bichir
                    return ("Catch 20 or more fishes to spawn");
                case ("105D"): //Sturgeon
                    return ("Catch 20 or more fishes to spawn");


                case ("08D4"): //Napoleonfish
                    return ("Catch 50 or more fishes to spawn");
                case ("08D6"): //Blowfish
                    return ("Catch 20 or more fishes to spawn");
                case ("08D9"): //Barred Knifejaw
                    return ("Catch 20 or more fishes to spawn");
                case ("08DF"): //Moray Eel
                    return ("Catch 20 or more fishes to spawn");
                case ("08E2"): //Tuna
                    return ("Catch 50 or more fishes to spawn");
                case ("08E3"): //Blue Marlin
                    return ("Catch 50 or more fishes to spawn");
                case ("08E4"): //Giant Trevally
                    return ("Catch 20 or more fishes to spawn");
                case ("106A"): //Mahi-mahi
                    return ("Catch 50 or more fishes to spawn");
                case ("08E6"): //Ocean Sunfish
                    return ("Catch 20 or more fishes to spawn");
                case ("08E5"): //Ray
                    return ("Catch 20 or more fishes to spawn");
                case ("08E9"): //Saw Shark
                    return ("Catch 50 or more fishes to spawn");
                case ("08E7"): //Hammerhead Shark
                    return ("Catch 20 or more fishes to spawn");
                case ("08E8"): //Great White Shark
                    return ("Catch 50 or more fishes to spawn");
                case ("08EA"): //Whale Shark
                    return ("Catch 50 or more fishes to spawn");
                case ("106B"): //Suckerfish
                    return ("Catch 20 or more fishes to spawn");
                case ("08E1"): //Football Fish
                    return ("Catch 20 or more fishes to spawn");
                case ("08EB"): //Oarfish
                    return ("Catch 50 or more fishes to spawn");
                case ("106C"): //Barreleye
                    return ("Catch 100 or more fishes to spawn");
                case ("08EC"): //Coelacanth
                    return ("Catch 100 or more fishes to spawn\nRainy days only");

            }
            return "";
        }

        private void GridView_SelectionChanged(object sender, EventArgs e)
        {
            currentGridView.ClearSelection();
        }

        private void GridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
            {
                if ((s == null || s.Connected == false) & bot == null)
                {
                    MessageBox.Show("Please connect to the switch first");
                    return;
                }

                if (senderGrid == insectGridView)
                    CellContentClick(senderGrid, e.RowIndex, e.ColumnIndex, ref insectRate);
                else if (senderGrid == riverFishGridView)
                    CellContentClick(senderGrid, e.RowIndex, e.ColumnIndex, ref riverFishRate);
                else if (senderGrid == seaFishGridView)
                    CellContentClick(senderGrid, e.RowIndex, e.ColumnIndex, ref seaFishRate);
                else if (senderGrid == seaCreatureGridView)
                    CellContentClick(senderGrid, e.RowIndex, e.ColumnIndex, ref seaCreatureRate);
            }
        }

        private void CellContentClick(DataGridView grid, int row, int col, ref int[] rate)
        {
            DataGridViewCellStyle btnStyle = new DataGridViewCellStyle();
            btnStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            btnStyle.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            btnStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            DataGridViewCellStyle selectedbtnStyle = new DataGridViewCellStyle();
            selectedbtnStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            selectedbtnStyle.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            selectedbtnStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            var Minbtn = (DataGridViewButtonCell)grid.Rows[row].Cells[6];
            var Defbtn = (DataGridViewButtonCell)grid.Rows[row].Cells[8];
            var Maxbtn = (DataGridViewButtonCell)grid.Rows[row].Cells[10];

            var cell = (DataGridViewProgressCell)grid.Rows[row].Cells[5];
            var index = (int)grid.Rows[row].Cells[1].Value;
            if (col == 6)
            {
                rate[index] = 0;
                cell.setValue = 0;

                if (grid == insectGridView)
                    setSpawnRate(InsectAppearParam, Utilities.InsectDataSize, (int)grid.Rows[row].Cells[1].Value, 0, 0);
                else if (grid == riverFishGridView)
                    setSpawnRate(FishRiverAppearParam, Utilities.FishDataSize, (int)grid.Rows[row].Cells[1].Value, 0, 1);
                else if (grid == seaFishGridView)
                    setSpawnRate(FishSeaAppearParam, Utilities.FishDataSize, (int)grid.Rows[row].Cells[1].Value, 0, 2);
                else if (grid == seaCreatureGridView)
                    setSpawnRate(CreatureSeaAppearParam, Utilities.SeaCreatureDataSize, (int)grid.Rows[row].Cells[1].Value, 0, 3);

                Minbtn.Style = selectedbtnStyle;
                Defbtn.Style = btnStyle;
                Maxbtn.Style = btnStyle;
            }
            else if (col == 8)
            {
                if (grid.Rows[row].Cells[4].Value != null)
                {
                    rate[index] = (int)grid.Rows[row].Cells[4].Value;
                    cell.setValue = (int)grid.Rows[row].Cells[4].Value;
                    cell.remark = getRemark(String.Format("{0:X4}", grid.Rows[row].Cells[3].Value.ToString()));

                    if (grid == insectGridView)
                        setSpawnRate(InsectAppearParam, Utilities.InsectDataSize, (int)grid.Rows[row].Cells[1].Value, 1, 0);
                    else if (grid == riverFishGridView)
                        setSpawnRate(FishRiverAppearParam, Utilities.FishDataSize, (int)grid.Rows[row].Cells[1].Value, 1, 1);
                    else if (grid == seaFishGridView)
                        setSpawnRate(FishSeaAppearParam, Utilities.FishDataSize, (int)grid.Rows[row].Cells[1].Value, 1, 2);
                    else if (grid == seaCreatureGridView)
                        setSpawnRate(CreatureSeaAppearParam, Utilities.SeaCreatureDataSize, (int)grid.Rows[row].Cells[1].Value, 1, 3);

                    Minbtn.Style = btnStyle;
                    Defbtn.Style = selectedbtnStyle;
                    Maxbtn.Style = btnStyle;
                }
            }
            else if (col == 10)
            {
                rate[index] = 255;
                cell.setValue = 255;

                if (grid == insectGridView)
                    setSpawnRate(InsectAppearParam, Utilities.InsectDataSize, (int)grid.Rows[row].Cells[1].Value, 2, 0);
                else if (grid == riverFishGridView)
                    setSpawnRate(FishRiverAppearParam, Utilities.FishDataSize, (int)grid.Rows[row].Cells[1].Value, 2, 1);
                else if (grid == seaFishGridView)
                    setSpawnRate(FishSeaAppearParam, Utilities.FishDataSize, (int)grid.Rows[row].Cells[1].Value, 2, 2);
                else if (grid == seaCreatureGridView)
                    setSpawnRate(CreatureSeaAppearParam, Utilities.SeaCreatureDataSize, (int)grid.Rows[row].Cells[1].Value, 2, 3);

                Minbtn.Style = btnStyle;
                Defbtn.Style = btnStyle;
                Maxbtn.Style = selectedbtnStyle;
            }
            grid.InvalidateCell(cell);
            System.Media.SystemSounds.Asterisk.Play();
        }


        private void critterSearchBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (currentGridView.DataSource == null)
                    return;
                if (currentGridView == insectGridView)
                    SearchBox_TextChanged(currentGridView, ref insectRate);
                else if (currentGridView == riverFishGridView)
                    SearchBox_TextChanged(currentGridView, ref riverFishRate, 1);
                else if (currentGridView == seaFishGridView)
                    SearchBox_TextChanged(currentGridView, ref seaFishRate, 1);
                else if (currentGridView == seaCreatureGridView)
                    SearchBox_TextChanged(currentGridView, ref seaCreatureRate, 1);
            }
            catch
            {
                critterSearchBox.Clear();
            }
        }

        private void SearchBox_TextChanged(DataGridView grid, ref int[] rate, int mode = 0)
        {
            (grid.DataSource as DataTable).DefaultView.RowFilter = string.Format("Name LIKE '%{0}%'", critterSearchBox.Text);

            DataGridViewCellStyle btnStyle = new DataGridViewCellStyle();
            btnStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            btnStyle.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            btnStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            DataGridViewCellStyle selectedbtnStyle = new DataGridViewCellStyle();
            selectedbtnStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            selectedbtnStyle.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            selectedbtnStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            for (int i = 0; i < grid.Rows.Count; i++)
            {
                var cell = (DataGridViewProgressCell)grid.Rows[i].Cells[5];
                var Minbtn = (DataGridViewButtonCell)grid.Rows[i].Cells[6];
                var Defbtn = (DataGridViewButtonCell)grid.Rows[i].Cells[8];
                var Maxbtn = (DataGridViewButtonCell)grid.Rows[i].Cells[10];
                int spawnrate = rate[(int)grid.Rows[i].Cells[1].Value];

                cell.setValue = spawnrate;
                cell.remark = getRemark(String.Format("{0:X4}", grid.Rows[i].Cells[3].Value.ToString()));
                cell.mode = mode;
                if (spawnrate <= 0)
                {
                    Minbtn.Style = selectedbtnStyle;
                    Defbtn.Style = btnStyle;
                    Maxbtn.Style = btnStyle;
                }
                else if (spawnrate >= 255)
                {
                    Minbtn.Style = btnStyle;
                    Defbtn.Style = btnStyle;
                    Maxbtn.Style = selectedbtnStyle;
                }
            }
        }

        private void critterSearchBox_Click(object sender, EventArgs e)
        {
            if (critterSearchBox.Text == "Search")
                critterSearchBox.Clear();
        }

        private void GridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var senderGrid = (DataGridView)sender;

            if (e.RowIndex >= 0 && e.RowIndex < senderGrid.Rows.Count)
            {
                int row = e.RowIndex;
                if (e.ColumnIndex == 0)
                {
                    CellFormatting(senderGrid, row, e);
                }
            }
        }

        private void CellFormatting(DataGridView grid, int row, DataGridViewCellFormattingEventArgs e)
        {
            if (grid.Rows[row].Cells.Count <= 3)
                return;
            if (grid.Rows[row].Cells[3].Value == null)
                return;
            string Id = grid.Rows[row].Cells[3].Value.ToString();
            //Debug.Print(Id);
            string imagePath = GetImagePathFromID(Id, itemSource);
            if (imagePath != "")
            {
                Image image = Image.FromFile(imagePath);
                e.Value = image;
            }
        }

        private void disableAllBtn_Click(object sender, EventArgs e)
        {
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            if (critterSearchBox.Text != "Search")
            {
                critterSearchBox.Clear();
            }

            if (currentGridView == insectGridView)
                disableAll(currentGridView, ref insectRate);
            else if (currentGridView == riverFishGridView)
                disableAll(currentGridView, ref riverFishRate, 1);
            else if (currentGridView == seaFishGridView)
                disableAll(currentGridView, ref seaFishRate, 1);
            else if (currentGridView == seaCreatureGridView)
                disableAll(currentGridView, ref seaCreatureRate, 1);
        }

        private void disableAll(DataGridView grid, ref int[] rate, int mode = 0)
        {
            string temp = null;
            if (critterSearchBox.Text != "Search")
            {
                temp = critterSearchBox.Text;
                critterSearchBox.Clear();
            }
            //critterSearchBox.Clear();

            for (int i = 0; i < rate.Length; i++)
            {
                rate[i] = 0;
            }

            disableBtn();

            Thread disableThread = new Thread(delegate () { disableAll(grid, mode, temp); });
            disableThread.Start();
        }

        private void disableAll(DataGridView grid, int mode, string temp)
        {

            DataGridViewCellStyle btnStyle = new DataGridViewCellStyle();
            btnStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            btnStyle.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            btnStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            DataGridViewCellStyle selectedbtnStyle = new DataGridViewCellStyle();
            selectedbtnStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            selectedbtnStyle.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            selectedbtnStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            for (int i = 0; i < grid.Rows.Count; i++)
            {
                var cell = (DataGridViewProgressCell)grid.Rows[i].Cells[5];
                var Minbtn = (DataGridViewButtonCell)grid.Rows[i].Cells[6];
                var Defbtn = (DataGridViewButtonCell)grid.Rows[i].Cells[8];
                var Maxbtn = (DataGridViewButtonCell)grid.Rows[i].Cells[10];

                if (grid == insectGridView)
                    setSpawnRate(InsectAppearParam, Utilities.InsectDataSize, (int)grid.Rows[i].Cells[1].Value, 0, 0);
                else if (grid == riverFishGridView)
                    setSpawnRate(FishRiverAppearParam, Utilities.FishDataSize, (int)grid.Rows[i].Cells[1].Value, 0, 1);
                else if (grid == seaFishGridView)
                    setSpawnRate(FishSeaAppearParam, Utilities.FishDataSize, (int)grid.Rows[i].Cells[1].Value, 0, 2);
                else if (grid == seaCreatureGridView)
                    setSpawnRate(CreatureSeaAppearParam, Utilities.SeaCreatureDataSize, (int)grid.Rows[i].Cells[1].Value, 0, 3);

                cell.setValue = 0;
                cell.remark = getRemark(String.Format("{0:X4}", grid.Rows[i].Cells[3].Value.ToString()));
                cell.mode = mode;
                Minbtn.Style = selectedbtnStyle;
                Defbtn.Style = btnStyle;
                Maxbtn.Style = btnStyle;

                grid.InvalidateCell(cell);
            }

            if (temp != null)
                critterSearchBox.Text = temp;
            System.Media.SystemSounds.Asterisk.Play();

            Invoke((MethodInvoker)delegate
            {
                enableBtn();
            });
        }

        private void resetAllBtn_Click(object sender, EventArgs e)
        {
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            if (critterSearchBox.Text != "Search")
            {
                critterSearchBox.Clear();
            }

            if (currentGridView == insectGridView)
                resetAll(currentGridView, ref insectRate);
            else if (currentGridView == riverFishGridView)
                resetAll(currentGridView, ref riverFishRate, 1);
            else if (currentGridView == seaFishGridView)
                resetAll(currentGridView, ref seaFishRate, 1);
            else if (currentGridView == seaCreatureGridView)
                resetAll(currentGridView, ref seaCreatureRate, 1);
        }

        private void resetAll(DataGridView grid, ref int[] rate, int mode = 0)
        {
            string temp = null;
            if (critterSearchBox.Text != "Search")
            {
                temp = critterSearchBox.Text;
                critterSearchBox.Clear();
            }
            //critterSearchBox.Clear();

            for (int i = 0; i < rate.Length; i++)
            {
                rate[i] = (int)grid.Rows[i].Cells[4].Value;
            }

            disableBtn();

            Thread resetThread = new Thread(delegate () { resetAll(grid, mode, temp); });
            resetThread.Start();
        }

        private void resetAll(DataGridView grid, int mode, string temp)
        {

            DataGridViewCellStyle btnStyle = new DataGridViewCellStyle();
            btnStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            btnStyle.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            btnStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            DataGridViewCellStyle selectedbtnStyle = new DataGridViewCellStyle();
            selectedbtnStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            selectedbtnStyle.Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold);
            selectedbtnStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            for (int i = 0; i < grid.Rows.Count; i++)
            {
                var cell = (DataGridViewProgressCell)grid.Rows[i].Cells[5];
                var Minbtn = (DataGridViewButtonCell)grid.Rows[i].Cells[6];
                var Defbtn = (DataGridViewButtonCell)grid.Rows[i].Cells[8];
                var Maxbtn = (DataGridViewButtonCell)grid.Rows[i].Cells[10];

                if (grid == insectGridView)
                    setSpawnRate(InsectAppearParam, Utilities.InsectDataSize, (int)grid.Rows[i].Cells[1].Value, 1, 0);
                else if (grid == riverFishGridView)
                    setSpawnRate(FishRiverAppearParam, Utilities.FishDataSize, (int)grid.Rows[i].Cells[1].Value, 1, 1);
                else if (grid == seaFishGridView)
                    setSpawnRate(FishSeaAppearParam, Utilities.FishDataSize, (int)grid.Rows[i].Cells[1].Value, 1, 2);
                else if (grid == seaCreatureGridView)
                    setSpawnRate(CreatureSeaAppearParam, Utilities.SeaCreatureDataSize, (int)grid.Rows[i].Cells[1].Value, 1, 3);

                cell.setValue = (int)grid.Rows[i].Cells[4].Value;
                cell.remark = getRemark(String.Format("{0:X4}", grid.Rows[i].Cells[3].Value.ToString()));
                cell.mode = mode;
                Minbtn.Style = btnStyle;
                Defbtn.Style = selectedbtnStyle;
                Maxbtn.Style = btnStyle;

                grid.InvalidateCell(cell);
            }
            if (temp != null)
                critterSearchBox.Text = temp;
            System.Media.SystemSounds.Asterisk.Play();

            Invoke((MethodInvoker)delegate
            {
                enableBtn();
            });
        }

        private void disableBtn()
        {
            pleaseWaitLabel.Visible = true;
            pacman.Visible = true;

            disableAllBtn.Visible = false;
            resetAllBtn.Visible = false;
            readDataBtn.Visible = false;
            currentGridView.Enabled = false;
            critterSearchBox.Enabled = false;

            insectBtn.Enabled = false;
            riverFishBtn.Enabled = false;
            seaFishBtn.Enabled = false;
            seaCreatureBtn.Enabled = false;
        }

        private void enableBtn()
        {
            pleaseWaitLabel.Visible = false;
            pacman.Visible = false;

            disableAllBtn.Visible = true;
            resetAllBtn.Visible = true;
            readDataBtn.Visible = true;
            currentGridView.Enabled = true;
            critterSearchBox.Enabled = true;

            insectBtn.Enabled = true;
            riverFishBtn.Enabled = true;
            seaFishBtn.Enabled = true;
            seaCreatureBtn.Enabled = true;
        }

        private void insectBtn_Click(object sender, EventArgs e)
        {
            if (critterSearchBox.Text != "Search")
            {
                critterSearchBox.Clear();
            }

            currentGridView = insectGridView;

            insectBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            riverFishBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            seaFishBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            seaCreatureBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            insectGridView.Visible = true;
            riverFishGridView.Visible = false;
            seaFishGridView.Visible = false;
            seaCreatureGridView.Visible = false;

            insectGridView.ClearSelection();
        }

        private void riverFishBtn_Click(object sender, EventArgs e)
        {
            if (critterSearchBox.Text != "Search")
            {
                critterSearchBox.Clear();
            }

            currentGridView = riverFishGridView;

            insectBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            riverFishBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            seaFishBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            seaCreatureBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));


            insectGridView.Visible = false;
            riverFishGridView.Visible = true;
            seaFishGridView.Visible = false;
            seaCreatureGridView.Visible = false;

            riverFishGridView.ClearSelection();
        }

        private void seaFishBtn_Click(object sender, EventArgs e)
        {

            if (critterSearchBox.Text != "Search")
            {
                critterSearchBox.Clear();
            }

            currentGridView = seaFishGridView;

            insectBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            riverFishBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            seaFishBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            seaCreatureBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));


            insectGridView.Visible = false;
            riverFishGridView.Visible = false;
            seaFishGridView.Visible = true;
            seaCreatureGridView.Visible = false;

            seaFishGridView.ClearSelection();
        }

        private void seaCreatureBtn_Click(object sender, EventArgs e)
        {
            if (critterSearchBox.Text != "Search")
            {
                critterSearchBox.Clear();
            }

            currentGridView = seaCreatureGridView;

            insectBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            riverFishBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            seaFishBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            seaCreatureBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));


            insectGridView.Visible = false;
            riverFishGridView.Visible = false;
            seaFishGridView.Visible = false;
            seaCreatureGridView.Visible = true;

            seaCreatureGridView.ClearSelection();
        }

        private void setSpawnRate(byte[] source, int size, int index, int mode, int type)
        {

            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            if (source == null)
            {
                MessageBox.Show("Missing critter data. Please load data first.");
                return;
            }
            int localIndex = index * 2;
            byte[] b;
            if (source == InsectAppearParam)
                b = new byte[12 * 6 * 2];
            else
                b = new byte[12 * 3 * 2];


            if (mode == 0) // min
            {
                for (int i = 0; i < b.Length; i++)
                    b[i] = 0;
            }
            else if (mode == 1) // default
            {
                for (int i = 0; i < b.Length; i++)
                    b[i] = source[size * localIndex + 2 + i];
            }
            else if (mode == 2) // max
                for (int i = 0; i < b.Length; i += 2)
                {
                    b[i] = 0xFF;
                    b[i + 1] = 0;
                }
            //Debug.Print(Encoding.UTF8.GetString(utilities.transform(b)));
            Utilities.SendSpawnRate(s, bot, b, localIndex, type);
            localIndex++;
            if (mode == 1)
            {
                for (int i = 0; i < b.Length; i++)
                    b[i] = source[size * localIndex + 2 + i];
            }
            Utilities.SendSpawnRate(s, bot, b, localIndex, type);
        }

        private void readDataBtn_Click(object sender, EventArgs e)
        {
            if ((s == null || s.Connected == false) & bot == null)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            disableBtn();

            Thread readThread = new Thread(delegate () { readData(); });
            readThread.Start();

        }

        private void readData()
        {
            if (currentGridView == insectGridView)
            {
                InsectAppearParam = Utilities.GetCritterData(s, bot, 0);
                File.WriteAllBytes(insectAppearFileName, InsectAppearParam);

                Invoke((MethodInvoker)delegate
                {
                    insectGridView.DataSource = null;
                    insectGridView.Rows.Clear();
                    insectGridView.Columns.Clear();
                    LoadGridView(InsectAppearParam, insectGridView, ref insectRate, Utilities.InsectDataSize, Utilities.InsectNumRecords);
                });
            }
            else if (currentGridView == riverFishGridView)
            {
                FishRiverAppearParam = Utilities.GetCritterData(s, bot, 1);
                File.WriteAllBytes(fishRiverAppearFileName, FishRiverAppearParam);

                Invoke((MethodInvoker)delegate
                {
                    riverFishGridView.DataSource = null;
                    riverFishGridView.Rows.Clear();
                    riverFishGridView.Columns.Clear();
                    LoadGridView(FishRiverAppearParam, riverFishGridView, ref riverFishRate, Utilities.FishDataSize, Utilities.FishRiverNumRecords, 1);
                });
            }
            else if (currentGridView == seaFishGridView)
            {
                FishSeaAppearParam = Utilities.GetCritterData(s, bot, 2);
                File.WriteAllBytes(fishSeaAppearFileName, FishSeaAppearParam);

                Invoke((MethodInvoker)delegate
                {
                    seaFishGridView.DataSource = null;
                    seaFishGridView.Rows.Clear();
                    seaFishGridView.Columns.Clear();
                    LoadGridView(FishSeaAppearParam, seaFishGridView, ref seaFishRate, Utilities.FishDataSize, Utilities.FishSeaNumRecords, 1);
                });
            }
            else if (currentGridView == seaCreatureGridView)
            {
                CreatureSeaAppearParam = Utilities.GetCritterData(s, bot, 3);
                File.WriteAllBytes(CreatureSeaAppearFileName, CreatureSeaAppearParam);

                Invoke((MethodInvoker)delegate
                {
                    seaCreatureGridView.DataSource = null;
                    seaCreatureGridView.Rows.Clear();
                    seaCreatureGridView.Columns.Clear();
                    LoadGridView(CreatureSeaAppearParam, seaCreatureGridView, ref seaCreatureRate, Utilities.SeaCreatureDataSize, Utilities.SeaCreatureNumRecords, 1);
                });
            }

            System.Media.SystemSounds.Asterisk.Play();

            Invoke((MethodInvoker)delegate
            {
                enableBtn();
            });
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Result1.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.masterAddress).ToString("X");

            Result2.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.TownNameddress).ToString("X");

            Result3.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.reactionAddress).ToString("X");

            Result4.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.weatherSeed).ToString("X");

            Result5.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.MasterRecyclingBase).ToString("X");

            Result6.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.MasterHouseBase).ToString("X");

            Result7.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.InsectAppearPointer).ToString("X");

            Result8.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.FishRiverAppearPointer).ToString("X");

            Result9.Text = (Convert.ToUInt64(debugAmount.Text, 16) + Utilities.FishSeaAppearPointer).ToString("X");
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
                    selectedItem.setup(GetNameFromID(utilities.turn2bytes(hexValue), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(utilities.turn2bytes(hexValue), recipeSource), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else
                {
                    selectedItem.setup(GetNameFromID(utilities.turn2bytes(customAmountTxt.Text), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(utilities.turn2bytes(customAmountTxt.Text), recipeSource), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
            }
            else if (customIdTextbox.Text == "114A") //money tree
            {
                if (hexModeBtn.Tag.ToString() == "Normal")
                {
                    selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + hexValue, 16)), true, GetNameFromID((utilities.turn2bytes(hexValue)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else
                {
                    selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), true, GetNameFromID((utilities.turn2bytes(customAmountTxt.Text)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                }
            }
            else
            {
                if (hexModeBtn.Tag.ToString() == "Normal")
                {
                    selectedItem.setup(GetNameFromID(utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + hexValue, 16)), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else
                {
                    selectedItem.setup(GetNameFromID(utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }

                if (selection != null)
                {
                    selection.receiveID(customIdTextbox.Text);
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
                    selectedItem.setup(GetNameFromID(utilities.turn2bytes(hexValue), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(utilities.turn2bytes(hexValue), recipeSource), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else
                {
                    selectedItem.setup(GetNameFromID(utilities.turn2bytes(customAmountTxt.Text), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(utilities.turn2bytes(customAmountTxt.Text), recipeSource), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
            }
            else if (customIdTextbox.Text == "114A") //money tree
            {
                if (hexModeBtn.Tag.ToString() == "Normal")
                {
                    selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + hexValue, 16)), true, GetNameFromID((utilities.turn2bytes(hexValue)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else
                {
                    selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), true, GetNameFromID((utilities.turn2bytes(customAmountTxt.Text)), itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
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
                    selectedItem.setup(GetNameFromID(utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + hexValue, 16)), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else
                {
                    selectedItem.setup(GetNameFromID(utilities.turn2bytes(customIdTextbox.Text), itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(utilities.turn2bytes(customIdTextbox.Text), itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), true, "", selectedItem.getFlag1(), selectedItem.getFlag2());
                }

                if (selection != null)
                {
                    selection.receiveID(customIdTextbox.Text);
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
                }
                else
                {
                    genePanel.Visible = false;
                }
            }
            else
            {
                genePanel.Visible = false;
            }
        }

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
            customAmountTxt.Text = utilities.precedingZeros(firstByte + secondByte, 8);
            selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(customIdTextbox.Text, itemSource, Convert.ToUInt32("0x" + customAmountTxt.Text, 16)), true);
            updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
        }

    }
}
