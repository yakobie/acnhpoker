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
        Utilities utilities = new Utilities();

        private inventorySlot selectedButton;
        public int selectedSlot = 1;
        public DataGridViewRow lastRow;
        public DataGridViewRow recipelastRow;
        public DataGridViewRow flowerlastRow;
        private Panel currentPanel;
        private System.Windows.Forms.Timer refreshTimer;
        private DataTable itemSource;
        private DataTable recipeSource;
        Boolean offsetFound = false;
        int maxPage = 1;
        int currentPage = 1;

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

        private void connectBtn_Click(object sender, EventArgs e)
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

                        this.connectBtn.Invoke((MethodInvoker)delegate
                        {
                            this.connectBtn.Enabled = false;
                            this.DisconnectBtn.Visible = true;
                        });
                        this.pictureBox1.Invoke((MethodInvoker)delegate
                        {
                            this.pictureBox1.BackColor = System.Drawing.Color.Green;
                        });
                        this.ipBox.Invoke((MethodInvoker)delegate
                        {

                            this.ipBox.ReadOnly = true;
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
                        });

                        this.refreshBtn.Invoke((MethodInvoker)delegate
                        {
                            this.refreshBtn.Visible = true;
                            this.playerSelectionPanel.Visible = true;
                            this.autoRefreshCheckBox.Visible = true;
                            this.saveBtn.Visible = true;
                            this.loadBtn.Visible = true;
                            this.poopBtn.Visible = true;
                            this.otherBtn.Visible = true;
                            this.wrapSetting.SelectedIndex = 0;
                        });

                        Invoke((MethodInvoker)delegate { 
                            UpdateInventory();
                            UpdateTownID();
                            InitTimer();
                            setEatBtn();
                            UpdateTurnipPrices();
                            loadReaction();

                            int firstSlot = findEmpty();
                            if (firstSlot > 0)
                            {
                                selectedSlot = firstSlot;
                                updateSlot(firstSlot);
                            }

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

        private void UpdateInventory()
        {
            try
            {
                byte[] Bank01to20 = utilities.GetInventoryBank(s, 1);
                byte[] Bank21to40 = utilities.GetInventoryBank(s, 21);
                string Bank1 = Encoding.ASCII.GetString(Bank01to20);
                string Bank2 = Encoding.ASCII.GetString(Bank21to40);
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

                    string itemID = utilities.Unflip4bytes(Encoding.ASCII.GetString(slotBytes));
                    string itemData = utilities.Unflipbytes(Encoding.ASCII.GetString(dataBytes));
                    string recipeData = utilities.Unflip4bytes(Encoding.ASCII.GetString(recipeBytes));
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
                        btn.setup(GetNameFromID(recipeData, recipeSource), 0x16A2, Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(recipeData, recipeSource), flag1, flag2);
                        continue;
                    }
                    else if (itemID == "1095") //Delivery
                    {
                        btn.setup(GetNameFromID(recipeData, itemSource), 0x1095, Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(recipeData, itemSource), flag1, flag2);
                        continue;
                    }
                    else if (itemID == "16A1") //Bottle Message
                    {
                        btn.setup(GetNameFromID(recipeData, recipeSource), 0x16A1, Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(recipeData, recipeSource), flag1, flag2);
                        continue;
                    }
                    else if (itemID == "0A13") // Fossil
                    {
                        btn.setup(GetNameFromID(recipeData, itemSource), 0x0A13, Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(recipeData, itemSource), flag1, flag2);
                        continue;
                    }
                    else
                    {
                        btn.setup(GetNameFromID(itemID, itemSource), Convert.ToUInt16("0x" + itemID, 16), Convert.ToUInt32("0x" + itemData, 16), GetImagePathFromID(itemID,itemSource), flag1, flag2);
                        continue;
                    }
                }
            }
            catch
            {
                s.Close();
                return;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //probably a way to do this in the form builder but w/e
            this.Icon = Properties.Resources.ACLeaf;

            //Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            this.ipBox.Text = ConfigurationManager.AppSettings["ipAddress"];

            //load the csv
            itemSource = loadItemCSV("items.csv");
            recipeSource = loadItemCSV("recipe.csv");

            itemGridView.DataSource = loadItemCSV("items.csv");
            recipeGridView.DataSource = loadItemCSV("recipe.csv");

            string flowerPath = @"flowers.csv";

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

                DataGridViewImageColumn flowerimageColumn = new DataGridViewImageColumn();
                flowerimageColumn.Name = "Image";
                flowerimageColumn.HeaderText = "Image";
                flowerimageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
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

            //set the ID row invisible
            itemGridView.Columns["ID"].Visible = false;
            recipeGridView.Columns["ID"].Visible = false;

            //change the width of the first two columns
            itemGridView.Columns[0].Width = 150;
            itemGridView.Columns[1].Width = 65;
            recipeGridView.Columns[0].Width = 150;
            recipeGridView.Columns[1].Width = 65;

            //select the full row and change color cause windows blue sux
            itemGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            itemGridView.DefaultCellStyle.BackColor = Color.FromArgb(255, 47, 49, 54);
            itemGridView.DefaultCellStyle.ForeColor = Color.FromArgb(255, 114, 105, 110);
            itemGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 57, 60, 67);

            itemGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 57, 60, 67);
            itemGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            itemGridView.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 57, 60, 67);

            itemGridView.EnableHeadersVisualStyles = false;

            recipeGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            recipeGridView.DefaultCellStyle.BackColor = Color.FromArgb(255, 47, 49, 54);
            recipeGridView.DefaultCellStyle.ForeColor = Color.FromArgb(255, 114, 105, 110);
            recipeGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 57, 60, 67);

            recipeGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 57, 60, 67);
            recipeGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            recipeGridView.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 57, 60, 67);

            recipeGridView.EnableHeadersVisualStyles = false;

            //create the image column
            DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
            imageColumn.Name = "Image";
            imageColumn.HeaderText = "Image";
            imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            itemGridView.Columns.Insert(3, imageColumn);
            imageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;


            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\" + "img"))
            {
                ImageDownloader imageDownloader = new ImageDownloader();
                imageDownloader.ShowDialog();
            }


            foreach (DataGridViewColumn c in itemGridView.Columns)
            {
                c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                c.HeaderCell.Style.Font = new Font("Arial", 9, FontStyle.Bold);
            }


            DataGridViewImageColumn recipeimageColumn = new DataGridViewImageColumn();
            recipeimageColumn.Name = "Image";
            recipeimageColumn.HeaderText = "Image";
            recipeimageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            recipeGridView.Columns.Insert(3, recipeimageColumn);
            recipeimageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

            foreach (DataGridViewColumn c in recipeGridView.Columns)
            {
                c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                c.HeaderCell.Style.Font = new Font("Arial", 9, FontStyle.Bold);
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

        private void slotBtn_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;

            var c = FindControls<Button>(this);
            foreach (var b in c)
            {
                if (b.Tag == null)
                    continue;
                b.FlatAppearance.BorderSize = 0;
            }

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = System.Drawing.Color.LightSeaGreen;
            button.FlatAppearance.BorderSize = 3;
            selectedSlot = int.Parse(button.Tag.ToString());

        }

        private void customIdBtn_Click(object sender, EventArgs e)
        {
            if (customIdTextbox.Text == "")
            {
                MessageBox.Show("Please enter an ID before sending item");
                return;
            }

            if (s == null || s.Connected == false)
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

            if (hexModeBtn.Tag.ToString() == "Normal")
            {
                int decValue = int.Parse(customAmountTxt.Text) - 1;
                string hexValue = decValue.ToString("X");

                utilities.SpawnItem(s, selectedSlot, customIdTextbox.Text, utilities.precedingZeros(hexValue, 8));

                selectedButton.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + hexValue, 16), GetImagePathFromID(customIdTextbox.Text, itemSource));
            }
            else
            {
                utilities.SpawnItem(s, selectedSlot, selectedItem.getFlag1() + selectedItem.getFlag2() + customIdTextbox.Text, customAmountTxt.Text);

                if (customIdTextbox.Text == "16A2")
                {
                    selectedButton.setup(GetNameFromID(utilities.removeZeros(customAmountTxt.Text), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(utilities.removeZeros(customAmountTxt.Text), recipeSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                }
                else
                {
                    selectedButton.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(customIdTextbox.Text, itemSource), selectedItem.getFlag1(), selectedItem.getFlag2());
                }
            }

            this.ShowMessage(customIdTextbox.Text);
        }

        private void customIdTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
            {
                e.Handled = true;
            }
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
            if (lastRow != null)
            {
                lastRow.Height = 22;
            }

            if (e.RowIndex > -1)
            {
                lastRow = itemGridView.Rows[e.RowIndex];
                itemGridView.Rows[e.RowIndex].Height = 160;
                customIdTextbox.Text = itemGridView.Rows[e.RowIndex].Cells[2].Value.ToString();
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

                //Debug.Print(itemGridView.Rows[e.RowIndex].Cells[2].Value.ToString());
                selectedItem.setup(itemGridView.Rows[e.RowIndex].Cells[0].Value.ToString(), Convert.ToUInt16("0x" + itemGridView.Rows[e.RowIndex].Cells[2].Value.ToString(), 16), 0x0, GetImagePathFromID(itemGridView.Rows[e.RowIndex].Cells[2].Value.ToString(),itemSource));
                updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
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
            if (s == null || s.Connected == false)
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
                    utilities.DeleteSlot(s, slotId);

                    var btnParent = (inventorySlot)owner.SourceControl;
                    btnParent.reset();
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

            if (s == null || s.Connected == false)
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
                int itemAmount = int.Parse(customAmountTxt.Text);
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

        private void spawnAll(string itemID, int itemAmount)
        {
            showWait();

            foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
            {
                utilities.SpawnItem(s, int.Parse(btn.Tag.ToString()), itemID, itemAmount);
                Invoke((MethodInvoker)delegate
                {
                    btn.setup(GetNameFromID(itemID, itemSource), Convert.ToUInt16("0x" + itemID, 16), Convert.ToUInt32("0x" + (itemAmount - 1), 16), GetImagePathFromID(itemID, itemSource));
                });
            }

            Thread.Sleep(3000);

            hideWait();
        }

        private void spawnAll(string itemID, string itemAmount)
        {
            showWait();

            foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
            {
                utilities.SpawnItem(s, int.Parse(btn.Tag.ToString()), itemID, itemAmount);
                Invoke((MethodInvoker)delegate
                {
                    btn.setup(GetNameFromID(itemID, itemSource), Convert.ToUInt16("0x" + itemID, 16), Convert.ToUInt32("0x" + itemAmount, 16), GetImagePathFromID(itemID, itemSource));
                });
            }

            Thread.Sleep(3000);

            hideWait();
        }

        private void refreshBtn_Click(object sender, EventArgs e)
        {
            UpdateInventory();
        }

        private void variationsBtn_Click(object sender, EventArgs e)
        {
            if (customIdTextbox.Text == "")
            {
                MessageBox.Show("Please enter an ID before sending item");
                return;
            }

            if (s == null || s.Connected == false)
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

            for (int variation = 1; variation <= 10; variation++)
            {
                utilities.SpawnItem(s, slot, customIdTextbox.Text, variation);

                slot++;
            }

            UpdateInventory();

            this.ShowMessage(customIdTextbox.Text);
        }

        private void clearBtn_Click(object sender, EventArgs e)
        {
            if (s == null || s.Connected == false)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            Thread clearThread = new Thread(clearInventory);
            clearThread.Start();

            foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
            {
                btn.reset();
            }
        }

        private void clearInventory()
        {

            showWait();

            for (int slot = 1; slot <= 40; slot++)
            {
                utilities.DeleteSlot(s, slot);
            }

            Thread.Sleep(3000);

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


            if (customIdTextbox.Text != "")
            {
                if (customIdTextbox.Text == "16A2")
                {
                    selectedItem.setup(GetNameFromID(utilities.removeZeros(customAmountTxt.Text), recipeSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(utilities.removeZeros(customAmountTxt.Text), recipeSource));
                }
                else
                {
                    if (hexModeBtn.Tag.ToString() == "Normal")
                    {
                        selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + (int.Parse(customAmountTxt.Text) - 1), 16), GetImagePathFromID(customIdTextbox.Text, itemSource));
                    }
                    else
                    {
                        selectedItem.setup(GetNameFromID(customIdTextbox.Text, itemSource), Convert.ToUInt16("0x" + customIdTextbox.Text, 16), Convert.ToUInt32("0x" + customAmountTxt.Text, 16), GetImagePathFromID(customIdTextbox.Text, itemSource));
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

            if (recipeNum.Text != "")
            {
                //Debug.Print(GetNameFromID(recipeNum.Text, recipeSource));
                selectedItem.setup(GetNameFromID(recipeNum.Text,recipeSource), 0x16A2, Convert.ToUInt32("0x" + recipeNum.Text, 16), GetImagePathFromID(recipeNum.Text, recipeSource));
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

            if (flowerID.Text != "")
            {
                selectedItem.setup(GetNameFromID(flowerID.Text, itemSource), Convert.ToUInt16("0x" + flowerID.Text, 16), Convert.ToUInt32("0x" + flowerValue.Text, 16), GetImagePathFromID(flowerID.Text, itemSource));
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

                selectedItem.setup(recipeGridView.Rows[e.RowIndex].Cells[0].Value.ToString(), 0x16A2, Convert.ToUInt32("0x" + recipeGridView.Rows[e.RowIndex].Cells[2].Value.ToString(), 16), GetImagePathFromID(recipeGridView.Rows[e.RowIndex].Cells[2].Value.ToString(),recipeSource));
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

                selectedItem.setup(flowerGridView.Rows[e.RowIndex].Cells[0].Value.ToString(), Convert.ToUInt16("0x" + flowerGridView.Rows[e.RowIndex].Cells[2].Value.ToString(), 16), Convert.ToUInt32("0x" + flowerGridView.Rows[e.RowIndex].Cells[4].Value.ToString(), 16), GetImagePathFromID(flowerGridView.Rows[e.RowIndex].Cells[2].Value.ToString(), itemSource));
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

            if (s == null || s.Connected == false)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            if (selectedButton == null)
            {
                MessageBox.Show("Please select a slot");
                return;
            }

            utilities.SpawnRecipe(s, selectedSlot, "16A2", recipeNum.Text);

            this.ShowMessage(recipeNum.Text);

            selectedButton.setup(GetNameFromID(recipeNum.Text, recipeSource), 0x16A2, Convert.ToUInt32("0x" + recipeNum.Text, 16), GetImagePathFromID(recipeNum.Text, recipeSource));
        }

        private void KeyboardKeyDown(object sender, KeyEventArgs e)
        {
            //Debug.Print(e.KeyCode.ToString());
            if (e.KeyCode.ToString() == "F2" ^ e.KeyCode.ToString() == "Insert")
            {
                if (selectedButton == null)
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
            }
            else if (e.KeyCode.ToString() == "F1")
            {
                deleteBtn_Click(sender, e);
            }
            else if (e.KeyCode.ToString() == "F3")
            {
                keyboardCopy(sender,e);
            }
        }

        private void keyboardCopy(object sender, KeyEventArgs e)
        {
            itemModeBtn_Click(sender, e);
            if (hexModeBtn.Tag.ToString() == "Normal")
            {
                hexMode_Click(sender, e);
            }
            selectedItem.setup(selectedButton);
            updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
            customIdTextbox.Text = utilities.precedingZeros(selectedItem.fillItemID(), 4);
            customAmountTxt.Text = utilities.precedingZeros(selectedItem.fillItemData(), 8);
        }

        private void deleteBtn_Click(object sender, KeyEventArgs e)
        {
            if (s == null || s.Connected == false)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            if (selectedButton == null)
            {
                MessageBox.Show("Please select a slot");
                return;
            }

            utilities.DeleteSlot(s, int.Parse(selectedButton.Tag.ToString()));

            UpdateInventory();

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
            byte[] AddressBank = utilities.peekAddress(s, debugAddress.Text);

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

            Result1.Text = utilities.Unflipbytes(firstResult);
            Result2.Text = utilities.Unflipbytes(secondResult);
            Result3.Text = utilities.Unflipbytes(thirdResult);
            Result4.Text = utilities.Unflipbytes(fourthResult);
            FullAddress.Text = FullResult;
        }

        private void PeekMainBtn_Click(object sender, EventArgs e)
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

            if (s == null || s.Connected == false)
            {
                MessageBox.Show("Please connect to the switch first");
                return;
            }

            if (selectedButton == null)
            {
                MessageBox.Show("Please select a slot");
                return;
            }

            utilities.SpawnFlower(s, selectedSlot, flowerID.Text, flowerValue.Text);

            this.ShowMessage(flowerID.Text);

            selectedButton.setup(GetNameFromID(flowerID.Text, itemSource), Convert.ToUInt16("0x" + flowerID.Text, 16), Convert.ToUInt32("0x" + flowerValue.Text, 16), GetImagePathFromID(flowerID.Text, itemSource));

        }

        private void ChaseBtn_Click(object sender, EventArgs e)
        {
            UInt32 startAddress = 0xAC470000;
            UInt32 endAddress = 0xAC500000;

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

            Thread SearchThread10 = new Thread(delegate () { SearchAddress(startAddress+ diff * 9, endAddress); });
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

                byte[] b = utilities.peekAddress(s, "0x" + (startAddress + i).ToString("X"));
                int NUM = boi.Search(b);

                if (NUM >= 0)
                {
                    Debug.Print(">> 0x"+(startAddress+i+NUM/2).ToString("X") + " << DONE : 0x" + (NUM/2).ToString("X"));
                    offsetFound = true;
                    return;
                }
            }

        }

        private void DisconnectBtn_Click(object sender, EventArgs e)
        {
            s.Close();
            foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
            {
                btn.reset();
            }
            DisconnectBtn.Visible = false;
            connectBtn.Enabled = true;
            refreshBtn.Visible = false;
            playerSelectionPanel.Visible = false;
            autoRefreshCheckBox.Visible = false;
            eatBtn.Visible = false;
            poopBtn.Visible = false;

            this.saveBtn.Visible = false;
            this.loadBtn.Visible = false;
            inventoryBtn_Click(sender, e);
            otherBtn.Visible = false;
        }

        private void UpdateTownID()
        {
            byte[] townID = Utilities.GetTownID(s);
            /*TownName.Text = String.Format("{1:X2} {2:X2} {3:X2} {4:X2} | {0}",
                Encoding.Unicode.GetString(townID, 4, 0x18),
                townID[3], townID[2], townID[1], townID[0]);*/
            //TownName.Text = Encoding.Unicode.GetString(townID, 4, 0x18);
            this.Text = this.Text + "  |  Island Name : " + Encoding.Unicode.GetString(townID, 4, 0x18);
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
            utilities.FollowPointer(s, debugAddress.Text, 0);
        }

        private void PokeBtn_Click(object sender, EventArgs e)
        {
            utilities.pokeAddress(s, debugAddress.Text, debugAmount.Text);
        }

        private void PokeMainBtn_Click(object sender, EventArgs e)
        {

        }

        private void eatBtn_Click(object sender, EventArgs e)
        {
            utilities.setStamina(s, "10");
            setEatBtn();
        }

        private void poopBtn_Click(object sender, EventArgs e)
        {
            utilities.setStamina(s, "0");
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
        }

        public string GetNameFromID(string itemID, DataTable source)
        {
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

        public string GetImagePathFromID(string itemID, DataTable source)
        {
            DataRow row = source.Rows.Find(itemID);

            if (row == null)
            {
                return ""; //row not found
            }
            else
            {
                //row found set the index and find the file
                string path = @"img\" + row[1] + @"\" + row[0] + ".png";
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
            if (s == null || s.Connected == false)
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
                    updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
                    if ( selectedItem.fillItemID() == "FFFE")
                    {
                        hexMode_Click(sender, e);
                        customIdTextbox.Text = "";
                        customAmountTxt.Text = "";
                    }
                    else
                    {
                        customIdTextbox.Text = utilities.precedingZeros(selectedItem.fillItemID(), 4);
                        customAmountTxt.Text = utilities.precedingZeros(selectedItem.fillItemData(), 8);
                    }
                }
            }
        }

        private void wrapItemBtn_Click(object sender, EventArgs e)
        {
            if (s == null || s.Connected == false)
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
                    switch(wrapSetting.SelectedIndex)
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
                    utilities.setFlag1(s, slotId, flag);

                    var btnParent = (inventorySlot)owner.SourceControl;
                    btnParent.setFlag1(flag);
                    btnParent.refresh();
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
                    string hexValue = decValue.ToString("X");
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
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to save your inventory?\n[Warning] Your previous save will be overwritten!", "Load inventory", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    byte[] Bank01to20 = utilities.GetInventoryBank(s, 1);
                    byte[] Bank21to40 = utilities.GetInventoryBank(s, 21);
                    string Bank1 = Encoding.ASCII.GetString(Bank01to20);
                    string Bank2 = Encoding.ASCII.GetString(Bank21to40);

                    Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
                    config.AppSettings.Settings["save01"].Value = Bank1.Substring(0, 320);
                    config.AppSettings.Settings["save21"].Value = Bank2.Substring(0, 320);
                    config.Save(ConfigurationSaveMode.Minimal);
                    MessageBox.Show("Inventory Saved!");
                }
                catch
                {
                    s.Close();
                    return;
                }
            }
        }

        private void loadBtn_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to load your inventory?\n[Warning] All of your inventory slots will be overwritten!", "Load inventory", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                try
                {
                    Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
                    string Bank1 = config.AppSettings.Settings["save01"].Value;
                    string Bank2 = config.AppSettings.Settings["save21"].Value;
                    byte[] Bank01to20 = Encoding.ASCII.GetBytes(Bank1);
                    byte[] Bank21to40 = Encoding.ASCII.GetBytes(Bank2);

                    Thread LoadThread = new Thread(delegate () { loadInventory(Bank01to20, Bank21to40); });
                    LoadThread.Start();

                }
                catch
                {
                    s.Close();
                    return;
                }

            }
        }

        private void loadInventory(byte[] bank01to20, byte[] bank21to40)
        {
            showWait();

            foreach (inventorySlot btn in this.inventoryPanel.Controls.OfType<inventorySlot>())
            {
                if (btn.Tag == null)
                    continue;

                if (btn.Tag.ToString() == "")
                    continue;

                int slotId = int.Parse(btn.Tag.ToString());

                byte[] slotBytes = new byte[4];
                byte[] dataBytes = new byte[8];
                byte[] recipeBytes = new byte[4];

                int slotOffset = 0;
                int countOffset = 0;
                if (slotId < 21)
                {
                    slotOffset = ((slotId - 1) * 0x10);
                    countOffset = 0x8 + ((slotId - 1) * 0x10);
                }
                else
                {
                    slotOffset = ((slotId - 21) * 0x10);
                    countOffset = 0x8 + ((slotId - 21) * 0x10);
                }

                string itemID = "";
                string itemData = "";
                string recipeData = "";

                if (slotId < 21)
                {
                    Buffer.BlockCopy(bank01to20, slotOffset, slotBytes, 0x0, 0x4);
                    Buffer.BlockCopy(bank01to20, countOffset, dataBytes, 0x0, 0x8);
                    Buffer.BlockCopy(bank01to20, countOffset, recipeBytes, 0x0, 0x4);
                }
                else
                {
                    Buffer.BlockCopy(bank21to40, slotOffset, slotBytes, 0x0, 0x4);
                    Buffer.BlockCopy(bank21to40, countOffset, dataBytes, 0x0, 0x8);
                    Buffer.BlockCopy(bank21to40, countOffset, recipeBytes, 0x0, 0x4);
                }

                itemID = utilities.Unflip4bytes(Encoding.ASCII.GetString(slotBytes));
                itemData = utilities.Unflipbytes(Encoding.ASCII.GetString(dataBytes));
                recipeData = utilities.Unflip4bytes(Encoding.ASCII.GetString(recipeBytes));

                //Debug.Print("Slot : " + slotId.ToString() + " ID : " + itemID + " Data : " + itemData);

                utilities.SpawnItem(s, slotId, itemID, itemData);

                Invoke((MethodInvoker)delegate {
                    UpdateInventory();
                });

            }

            hideWait();

            MessageBox.Show("Inventory Loaded!");
        }

        private int findEmpty()
        {
            byte[] Bank01to20 = utilities.GetInventoryBank(s, 1);
            byte[] Bank21to40 = utilities.GetInventoryBank(s, 21);
            string Bank1 = Encoding.ASCII.GetString(Bank01to20);
            string Bank2 = Encoding.ASCII.GetString(Bank21to40);
            //Debug.Print(Bank1);
            //Debug.Print(Bank2);

            for (int slot = 1; slot <= 40; slot++)
            {
                byte[] slotBytes = new byte[4];

                int slotOffset = 0;
                if (slot < 21)
                {
                    slotOffset = ((slot - 1) * 0x10);
                }
                else
                {
                    slotOffset = ((slot - 21) * 0x10);
                }

                string itemID = "";

                if (slot < 21)
                {
                    Buffer.BlockCopy(Bank01to20, slotOffset, slotBytes, 0x0, 0x4);
                }
                else
                {
                    Buffer.BlockCopy(Bank21to40, slotOffset, slotBytes, 0x0, 0x4);
                }

                itemID = utilities.Unflip4bytes(Encoding.ASCII.GetString(slotBytes));

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
        }

        private void critterBtn_Click(object sender, EventArgs e)
        {
            this.inventoryBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.critterBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            this.otherBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            inventoryLargePanel.Visible = false;
            otherLargePanel.Visible = false;
        }

        private void otherBtn_Click(object sender, EventArgs e)
        {
            this.inventoryBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.critterBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.otherBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));

            inventoryLargePanel.Visible = false;
            otherLargePanel.Visible = true;
        }
        private void UpdateTurnipPrices()
        {
            UInt32[] turnipPrices = Utilities.GetTurnipPrices(s);
            turnipBuyPrice.Text = String.Format("{0}", turnipPrices[12]);
            int buyPrice = int.Parse(String.Format("{0}", turnipPrices[12]));

            turnipSell1AM.Text = String.Format("{0}", turnipPrices[0]);
            int MondayAM = int.Parse(String.Format("{0}", turnipPrices[0]));
            setTurnipColor(buyPrice, MondayAM, turnipSell1AM);

            turnipSell1PM.Text = String.Format("{0}", turnipPrices[1]);
            int MondayPM = int.Parse(String.Format("{0}", turnipPrices[1]));
            setTurnipColor(buyPrice, MondayPM, turnipSell1PM);

            turnipSell2AM.Text = String.Format("{0}", turnipPrices[2]);
            int TuesdayAM = int.Parse(String.Format("{0}", turnipPrices[2]));
            setTurnipColor(buyPrice, TuesdayAM, turnipSell2AM);

            turnipSell2PM.Text = String.Format("{0}", turnipPrices[3]);
            int TuesdayPM = int.Parse(String.Format("{0}", turnipPrices[3]));
            setTurnipColor(buyPrice, TuesdayPM, turnipSell2PM);

            turnipSell3AM.Text = String.Format("{0}", turnipPrices[4]);
            int WednesdayAM = int.Parse(String.Format("{0}", turnipPrices[4]));
            setTurnipColor(buyPrice, WednesdayAM, turnipSell3AM);

            turnipSell3PM.Text = String.Format("{0}", turnipPrices[5]);
            int WednesdayPM = int.Parse(String.Format("{0}", turnipPrices[5]));
            setTurnipColor(buyPrice, WednesdayPM, turnipSell3PM);

            turnipSell4AM.Text = String.Format("{0}", turnipPrices[6]);
            int ThursdayAM = int.Parse(String.Format("{0}", turnipPrices[6]));
            setTurnipColor(buyPrice, ThursdayAM, turnipSell4AM);

            turnipSell4PM.Text = String.Format("{0}", turnipPrices[7]);
            int ThursdayPM = int.Parse(String.Format("{0}", turnipPrices[7]));
            setTurnipColor(buyPrice, ThursdayPM, turnipSell4PM);

            turnipSell5AM.Text = String.Format("{0}", turnipPrices[8]);
            int FridayAM = int.Parse(String.Format("{0}", turnipPrices[8]));
            setTurnipColor(buyPrice, FridayAM, turnipSell5AM);

            turnipSell5PM.Text = String.Format("{0}", turnipPrices[9]);
            int FridayPM = int.Parse(String.Format("{0}", turnipPrices[9]));
            setTurnipColor(buyPrice, FridayPM, turnipSell5PM);

            turnipSell6AM.Text = String.Format("{0}", turnipPrices[10]);
            int SaturdayAM = int.Parse(String.Format("{0}", turnipPrices[10]));
            setTurnipColor(buyPrice, SaturdayAM, turnipSell6AM);

            turnipSell6PM.Text = String.Format("{0}", turnipPrices[11]);
            int SaturdayPM = int.Parse(String.Format("{0}", turnipPrices[11]));
            setTurnipColor(buyPrice, SaturdayPM, turnipSell6PM);

            int[] price = { MondayAM, MondayPM, TuesdayAM, TuesdayPM, WednesdayAM, WednesdayPM, ThursdayAM, ThursdayPM, FridayAM, FridayPM, SaturdayAM, SaturdayPM };
            int highest = findHighest(price);
            setStar(highest, MondayAM, MondayPM, TuesdayAM, TuesdayPM, WednesdayAM, WednesdayPM, ThursdayAM, ThursdayPM, FridayAM, FridayPM, SaturdayAM, SaturdayPM);
        }

        private void setTurnipColor(int buyPrice, int comparePrice, RichTextBox target)
        {
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
        }

        private void fastNextBtn_Click(object sender, EventArgs e)
        {
            if (currentPage == maxPage)
            {
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
                currentPage = currentPage + 10;
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
            }
            setPageLabel();
            UpdateInventory();
        }

        private void fastBackBtn_Click(object sender, EventArgs e)
        {
            if (currentPage == 1)
            {
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
                currentPage = currentPage - 10;
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
            if (s == null || s.Connected == false)
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
                Utilities.ChangeTurnipPrices(s, prices);
                UpdateTurnipPrices();
                MessageBox.Show("Turnip prices updated!");
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
            byte[] reactionBank = Utilities.getReaction(s);
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

        private void freezeTimeBtn_Click(object sender, EventArgs e)
        {
            //utilities.pokeMainAddress(s, "0x002299B0", "D503201F");
            /*
            utilities.pokeAddress(s, "0x0B991378", "07E4");
            utilities.pokeAddress(s, "0x0B99137A", "04");
            utilities.pokeAddress(s, "0x0B99137B", "14");
            utilities.pokeAddress(s, "0x0B99137C", "04");
            utilities.pokeAddress(s, "0x0B99137D", "14");
            */
        }

        private void unfreezeTimeBtn_Click(object sender, EventArgs e)
        {
            //utilities.pokeMainAddress(s, "0x002299B0", "F9203260");
        }

        private void setReactionBtn_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to change your reaction wheel?\n[Warning] Your previous reaction wheel will be overwritten!", "Change Reaction Wheel", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                string reaction1 = (utilities.precedingZeros((reactionSlot1.SelectedIndex + 1).ToString("X"), 2) + utilities.precedingZeros((reactionSlot2.SelectedIndex + 1).ToString("X"), 2) + utilities.precedingZeros((reactionSlot3.SelectedIndex + 1).ToString("X"), 2) + utilities.precedingZeros((reactionSlot4.SelectedIndex + 1).ToString("X"), 2));
                string reaction2 = (utilities.precedingZeros((reactionSlot5.SelectedIndex + 1).ToString("X"), 2) + utilities.precedingZeros((reactionSlot6.SelectedIndex + 1).ToString("X"), 2) + utilities.precedingZeros((reactionSlot7.SelectedIndex + 1).ToString("X"), 2) + utilities.precedingZeros((reactionSlot8.SelectedIndex + 1).ToString("X"), 2));
                Utilities.setReaction(s, reaction1, reaction2);
                MessageBox.Show("Reaction Wheel updated!");
            }
        }
    }
}
