using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO.Compression;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace acnhpoker
{
    public partial class variation : Form
    {
        readonly private string variationPath = @"variation.csv";
        readonly private string imgPath = @"img\variation\";
        private DataTable itemSource;
        private DataGridViewRow lastRow;
        private inventorySlot[,] selection;
        private int lengthX = 0;
        private int lengthY = 0;

        public Form1 mainform = null;

        public variation()
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

        private string removeSpace(string input)
        {
            return input.Replace(" ", String.Empty);
        }

        private Boolean hasVar(string path)
        {
            if (path.IndexOf("_0_0") > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        protected override Point ScrollToControl(Control activeControl)
        {
            return this.AutoScrollPosition;
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
                string path = imgPath + row[1] + @"\" + row[3] + ".png";
                if (File.Exists(path))
                {
                    return path;
                }

                path = imgPath + row[1] + @"\" + row[3] + "_0_0" + ".png";
                if (File.Exists(path))
                {
                    return path;
                }
                return "";
            }

        }

        private void Variation_Load(object sender, EventArgs e)
        {
            if (File.Exists(variationPath))
            {
                itemSource = loadItemCSV(variationPath);

                furnitureGridView.DataSource = loadItemCSV(variationPath);
                //furnitureGridView.Columns["ID"].Visible = false;
                //furnitureGridView.Columns["iName"].Visible = false;
                furnitureGridView.Columns[0].Width = 150;
                furnitureGridView.Columns[1].Width = 110;

                furnitureGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                furnitureGridView.DefaultCellStyle.BackColor = Color.FromArgb(255, 47, 49, 54);
                furnitureGridView.DefaultCellStyle.ForeColor = Color.FromArgb(255, 114, 105, 110);
                furnitureGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 57, 60, 67);

                furnitureGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 57, 60, 67);
                furnitureGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
                furnitureGridView.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 57, 60, 67);

                furnitureGridView.EnableHeadersVisualStyles = false;

                furnitureGridView.Font = new Font("Arial", 10);

                DataGridViewImageColumn furnitureImageColumn = new DataGridViewImageColumn
                {
                    Name = "Image",
                    HeaderText = "Image",
                    ImageLayout = DataGridViewImageCellLayout.Zoom
                };
                furnitureGridView.Columns.Insert(3, furnitureImageColumn);
                furnitureImageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

                foreach (DataGridViewColumn c in furnitureGridView.Columns)
                {
                    c.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    c.HeaderCell.Style.Font = new Font("Arial", 9, FontStyle.Bold);
                }

            }
        }

        private void furnitureGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < this.furnitureGridView.Rows.Count)
            {
                if (e.ColumnIndex == 3)
                {
                    string path = imgPath + furnitureGridView.Rows[e.RowIndex].Cells[1].Value.ToString() + @"\" + removeSpace(furnitureGridView.Rows[e.RowIndex].Cells[4].Value.ToString()) + ".png";
                    if (File.Exists(path))
                    {
                        Image img = Image.FromFile(path);
                        e.Value = img;
                        return;
                    }

                    path = imgPath + furnitureGridView.Rows[e.RowIndex].Cells[1].Value.ToString() + @"\" + removeSpace(furnitureGridView.Rows[e.RowIndex].Cells[4].Value.ToString()) + "_0_0" + ".png";
                    if (File.Exists(path))
                    {
                        Image img = Image.FromFile(path);
                        e.Value = img;
                        return;
                    }

                }
            }
        }

        private void furnitureGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (lastRow != null)
            {
                lastRow.Height = 22;
            }

            if (e.RowIndex > -1)
            {
                lastRow = furnitureGridView.Rows[e.RowIndex];
                furnitureGridView.Rows[e.RowIndex].Height = 160;
                /*
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
                */
                //Debug.Print(itemGridView.Rows[e.RowIndex].Cells[2].Value.ToString());

                string name = furnitureGridView.Rows[e.RowIndex].Cells[0].Value.ToString();
                string idString = furnitureGridView.Rows[e.RowIndex].Cells[2].Value.ToString();
                UInt16 id = Convert.ToUInt16("0x" + idString, 16);
                UInt16 data = 0x0;
                string category = furnitureGridView.Rows[e.RowIndex].Cells[1].Value.ToString();
                string iName = furnitureGridView.Rows[e.RowIndex].Cells[4].Value.ToString();
                string path = GetImagePathFromID(idString, itemSource);

                selectedItem.setup(name, id, data, path, true);
                //updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
                if (hasVar(path))
                {
                    //Debug.Print(findMaxVariation(category, iName).ToString());
                    //Debug.Print(findMaxSubVariation(category, iName).ToString());
                    showVariation(name, id, findMaxVariation(category, iName), findMaxSubVariation(category, iName), category, iName);
                }
            }
        }

        private void showVariation(string name, UInt16 id, int main, int sub, string category, string iName)
        {
            selection = new inventorySlot[main + 1, sub + 1];

            for (int j = 0; j <= main; j++)
            {
                for (int k = 0; k <= sub; k++)
                {
                    string path = imgPath + category + @"\" + iName + "_" + j.ToString() + "_" + k.ToString() + ".png";

                    selection[j, k] = new inventorySlot();
                    selection[j, k].BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
                    selection[j, k].FlatAppearance.BorderSize = 0;
                    selection[j, k].FlatStyle = System.Windows.Forms.FlatStyle.Flat;
                    if (sub > main)
                    {
                        selection[j, k].Location = new System.Drawing.Point(5 + (k * 82), 5 + (j * 82));
                    }
                    else
                    {
                        selection[j, k].Location = new System.Drawing.Point(5 + (j * 82), 5 + (k * 82));
                    }
                    selection[j, k].Margin = new System.Windows.Forms.Padding(0);
                    selection[j, k].Size = new System.Drawing.Size(80, 80);
                    selection[j,k].Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
                    //selection[j, k].setHide(true);
                    selection[j, k].setup(name, id, (uint)(j + (0x20 * k)), path, true);
                    selection[j, k].MouseClick += new System.Windows.Forms.MouseEventHandler(this.variation_MouseClick);
                    this.Controls.Add(selection[j, k]);

                    //            this.selectedItem.MouseClick += new System.Windows.Forms.MouseEventHandler(this.selectedItem_MouseClick);
                }
            }
            //Debug.Print(selection.GetLength(0).ToString());
            //Debug.Print(selection.GetLength(1).ToString());
            this.lengthX = selection.GetLength(0);
            this.lengthY = selection.GetLength(1);
        }

        private void variation_MouseClick(object sender, MouseEventArgs e)
        {
            var button = (inventorySlot)sender;

            foreach (inventorySlot btn in this.Controls.OfType<inventorySlot>())
            {
                btn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            }

            button.BackColor = System.Drawing.Color.LightSeaGreen;
            if (mainform == null)
            {
                return;
            }
            else
            {
                mainform.ReceiveVariation((inventorySlot)sender);
                mainform.Focus();

            }
            
        }

        private void removeVariation()
        {
            for (int j = 0; j < this.lengthX; j++)
            {
                for (int k = 0; k < this.lengthY; k++)
                {
                    this.Controls.Remove(selection[j, k]);
                }
            }
        }

        /*
            this.selectedItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.selectedItem.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.selectedItem.FlatAppearance.BorderSize = 0;
            this.selectedItem.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.selectedItem.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.selectedItem.ForeColor = System.Drawing.Color.White;
            this.selectedItem.Location = new System.Drawing.Point(9, 9);
            this.selectedItem.Margin = new System.Windows.Forms.Padding(0);
            this.selectedItem.Name = "selectedItem";
            this.selectedItem.Size = new System.Drawing.Size(128, 128);
            this.selectedItem.TextAlign = System.Drawing.ContentAlignment.TopLeft;
            this.selectedItem.UseVisualStyleBackColor = false;
         */

        private int findMaxVariation(string category, string name)
        {
            for (int i = 9; i >= 0; i--)
            {
                string path = imgPath + category + @"\" + name + "_" + i.ToString() + "_0" + ".png";
                if (File.Exists(path))
                {
                    return i;
                }
            }
            return -1;
        }
        private int findMaxSubVariation(string category, string name)
        {
            for (int i = 9; i >= 0; i--)
            {
                string path = imgPath + category + @"\" + name + "_0_" + i.ToString() + ".png";
                if (File.Exists(path))
                {
                    return i;
                }
            }
            return -1;
        }

        public void receiveID(string id)
        {
            removeVariation();
            this.itemIDLabel.Text = id;
            DataRow row = GetRowFromID(id);
            if (row != null)
            {
                this.infoLabel.Text = "";
                string name = row[0].ToString();
                string idString = row[2].ToString();
                UInt16 itemID = Convert.ToUInt16("0x" + row[2].ToString(), 16);
                //UInt16 data = 0x0;
                string category = row[1].ToString();
                string iName = row[3].ToString();
                string path = GetImagePathFromID(idString, itemSource);

                //updateSelectedItemInfo(selectedItem.displayItemName(), selectedItem.displayItemID(), selectedItem.displayItemData());
                if (hasVar(path))
                {
                    //Debug.Print(row[0].ToString() + " " + row[1].ToString() + " " + row[2].ToString() + " " + row[3].ToString() + " ");
                    showVariation(name, itemID, findMaxVariation(category, iName), findMaxSubVariation(category, iName), category, iName);
                }
                else
                {
                    this.infoLabel.Text = "No variation found.";
                }
            }
            else
            {
                this.infoLabel.Text = "No variation record found.";
            }
        }

        public DataRow GetRowFromID(string id)
        {
            DataRow row = itemSource.Rows.Find(id);

            return row;
        }
    }
}
