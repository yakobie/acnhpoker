using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
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
                        s.EndConnect(result);
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
            (itemGridView.DataSource as DataTable).DefaultView.RowFilter = string.Format("Name LIKE '%{0}%'", itemSearchBox.Text);
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


    }
}
