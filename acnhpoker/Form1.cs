using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using static acnhpoker.Utilities;

namespace acnhpoker
{
    public partial class Form1 : Form
    {
        Socket s;
        Utilities utilities = new Utilities();

        private Button selectedButton;
        public int selectedSlot = 1;
        public DataGridViewRow lastRow;

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


            if(s.Connected == false)
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

                        this.connectBtn.Invoke((MethodInvoker)delegate
                        {
                            this.connectBtn.Enabled = false;
                        });
                        this.pictureBox1.Invoke((MethodInvoker)delegate
                        {
                            this.pictureBox1.BackColor = System.Drawing.Color.Green;
                        });
                        this.ipBox.Invoke((MethodInvoker)delegate
                        {
                            this.ipBox.ReadOnly = true;
                        });

                        Invoke((MethodInvoker)delegate { updateInventory(); });

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

        private string getImagePathFromID(string itemID)
        {
            int rowIndex = -1;

            DataGridViewRow row = itemGridView.Rows
                .Cast<DataGridViewRow>()
                .Where(r => r.Cells["ID"].Value.ToString().Equals(itemID.ToLower()))
                .FirstOrDefault();

            if(row == null)
            {
                return ""; //row not found
            } 
            else
            {
                //row found set the index and find the file
                rowIndex = row.Index;

                string path = @"img\" + itemGridView.Rows[rowIndex].Cells[1].Value.ToString() + @"\" + itemGridView.Rows[rowIndex].Cells[0].Value.ToString() + ".png";
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

        private void updateInventory()
        {
            byte[] inventoryBytesBank1 = utilities.GetInventoryBank1(s);
            byte[] inventoryBytesBank2 = utilities.GetInventoryBank2(s);

            foreach (Button btn in this.pnlBank1.Controls.OfType<Button>())
            {
                if (btn.Tag == null)
                    continue;

                if (btn.Tag.ToString() == "")
                    continue;

                int slotId = int.Parse(btn.Tag.ToString());

                if(slotId > 20)
                {
                    continue;
                }

                byte[] slotBytes = new byte[4];
                byte[] amountBytes = new byte[2];

                int slotOffset = ((slotId - 1) * 0x10);
                int countOffset = 0x8 + ((slotId - 1) * 0x10);

                Buffer.BlockCopy(inventoryBytesBank1, slotOffset, slotBytes, 0x0, 0x4);
                Buffer.BlockCopy(inventoryBytesBank1, countOffset, amountBytes, 0x0, 0x2);
                string itemID = utilities.UnflipItemId(Encoding.ASCII.GetString(slotBytes));
                
                //wow i want to gouge my eyeballs out
                string itemAmountStr = (Convert.ToInt32(Encoding.ASCII.GetString(amountBytes), 16) + 1).ToString();

                btn.Text = "";

                if (itemID == "FFFE")
                    continue;
               
                string itemPath = getImagePathFromID(itemID);

                if (itemPath == "")
                {
                    btn.Image = (Image)(new Bitmap(Properties.Resources.ACLeaf.ToBitmap(), new Size(64, 64)));
                    if(itemAmountStr != "1" || itemAmountStr != "0")
                    {
                        btn.Text = itemAmountStr;
                    }
                }
                else
                {
                    Image img = Image.FromFile(itemPath);
                    btn.Image = (Image)(new Bitmap(img, new Size(64, 64)));
                    if (itemAmountStr != "1")
                    {
                        btn.Text = itemAmountStr;
                    }
                }

            }

            foreach (Button btn in this.pnlBank2.Controls.OfType<Button>())
            {
                if (btn.Tag == null)
                    continue;

                if (btn.Tag.ToString() == "")
                    continue;

                int slotId = int.Parse(btn.Tag.ToString());
                if (slotId <= 20)
                {
                    continue;
                }

                slotId = slotId - 20;

                byte[] slotBytes = new byte[4];
                byte[] amountBytes = new byte[2];

                int slotOffset = ((slotId - 1) * 0x10);
                int countOffset = 0x8 + ((slotId - 1) * 0x10);

                Buffer.BlockCopy(inventoryBytesBank2, slotOffset, slotBytes, 0x0, 0x4);
                Buffer.BlockCopy(inventoryBytesBank2, countOffset, amountBytes, 0x0, 0x2);
                string itemID = utilities.UnflipItemId(Encoding.ASCII.GetString(slotBytes));

                //wow i want to gouge my eyeballs out
                string itemAmountStr = (Convert.ToInt32(Encoding.ASCII.GetString(amountBytes), 16) + 1).ToString();

                btn.Text = "";

                if (itemID == "FFFE")
                    continue;

                string itemPath = getImagePathFromID(itemID);

                if (itemPath == "")
                {
                    btn.Image = (Image)(new Bitmap(Properties.Resources.ACLeaf.ToBitmap(), new Size(64, 64)));
                    if (itemAmountStr != "1" || itemAmountStr != "0")
                    {
                        btn.Text = itemAmountStr;
                    }
                }
                else
                {
                    Image img = Image.FromFile(itemPath);
                    btn.Image = (Image)(new Bitmap(img, new Size(64, 64)));
                    if (itemAmountStr != "1")
                    {
                        btn.Text = itemAmountStr;
                    }
                }

            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            //probably a way to do this in the form builder but w/e
            this.Icon = Properties.Resources.ACLeaf;

            //load the csv
            itemGridView.DataSource = loadItemCSV("items.csv");

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
            DataGridViewImageColumn imageColumn = new DataGridViewImageColumn();
            imageColumn.Name = "Image";
            imageColumn.HeaderText = "Image";
            imageColumn.ImageLayout = DataGridViewImageCellLayout.Zoom;
            itemGridView.Columns.Insert(3, imageColumn);
            imageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

            foreach (DataGridViewColumn c in itemGridView.Columns)
            {
                c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                c.HeaderCell.Style.Font = new Font("Arial", 9, FontStyle.Bold);
            }

        }

        private void slotBtn_Click(object sender, EventArgs e)
        {
            var button = (Button)sender;

            foreach (var b in this.Controls.OfType<Button>())
            {
                
                b.FlatAppearance.BorderSize = 0;
            }

            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = System.Drawing.Color.LightSeaGreen;
            button.FlatAppearance.BorderSize = 1;
            selectedButton = button;
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

            utilities.SpawnItem(s, selectedSlot, customIdTextbox.Text, int.Parse(customAmountTxt.Text));

            string itemPath = getImagePathFromID(customIdTextbox.Text);

            if (itemPath == "")
            {
                selectedButton.Image = (Image)(new Bitmap(Properties.Resources.ACLeaf.ToBitmap(), new Size(64, 64)));
            }
            else
            {
                Image img = Image.FromFile(itemPath);
                selectedButton.Image = (Image)(new Bitmap(img, new Size(64, 64)));
            }

            if (customAmountTxt.Text == "1")
                selectedButton.Text = "";
            else
                selectedButton.Text = customAmountTxt.Text;

        }

        private void customIdTextbox_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (!((c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f')))
            {
                e.Handled = true;
            }

            if(customIdTextbox.Text.Length >= 4)
            {
                e.Handled = true;
            }
        }

        private void customAmountTxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;
            if (!((c >= '0' && c <= '9')))
            {
                e.Handled = true;
            }

            if (customAmountTxt.Text.Length >= 2)
            {
                e.Handled = true;
            }
        }

        private void itemSearchBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                (itemGridView.DataSource as DataTable).DefaultView.RowFilter = string.Format("Name LIKE '%{0}%'", itemSearchBox.Text);
            }
            catch
            {
                itemSearchBox.Clear();
            }
        }

        private void itemGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < this.itemGridView.Rows.Count - 1)

            {
                if (e.ColumnIndex == 3)
                {
                    string path = @"img\" + itemGridView.Rows[e.RowIndex].Cells[1].Value.ToString() + @"\" + itemGridView.Rows[e.RowIndex].Cells[0].Value.ToString() + ".png";
                    if(File.Exists(path))
                    {
                        Image img = Image.FromFile(path);
                        e.Value = img;
                    }

                }
            }

        }

        private void itemSearchBox_Click(object sender, EventArgs e)
        {
            if(itemSearchBox.Text == "Search")
            {
                itemSearchBox.Text = "";
                itemSearchBox.ForeColor = Color.White;
            }


        }

        private void itemGridView_CellMouseUp(object sender, DataGridViewCellMouseEventArgs e)
        {
            if(lastRow != null)
            {
                lastRow.Height = 22;
            }
            if (e.RowIndex > -1)
            {
                lastRow = itemGridView.Rows[e.RowIndex];
                itemGridView.Rows[e.RowIndex].Height = 160;
                customIdTextbox.Text = itemGridView.Rows[e.RowIndex].Cells[2].Value.ToString();
                if(customAmountTxt.Text == "" || customAmountTxt.Text == "0")
                {
                    customAmountTxt.Text = "1";
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
    }
}
