using System;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace ACNHPoker
{
    public partial class Form1 : Form
    {
        [DllImport("winmm.dll")]
        private static extern long mciSendString(string strCommand, StringBuilder strReturn, int iReturnLength, IntPtr hwndCallback);
        Socket s;

        readonly private string version = "ACNH Poker R10 for v1.4.2";
        private inventorySlot selectedButton;
        private Villager[] V = null;
        private Button[] villagerButton = null;
        private int[] HouseList;
        private bool firstload = false;
        private byte[] header;
        private bool blocker = false;
        private bool firstWarning = false;
        private int selectedSlot = 1;
        private Button selectedVillagerButton = null;
        private DataGridViewRow lastRow;
        private DataGridViewRow recipelastRow;
        private DataGridViewRow flowerlastRow;
        private Panel currentPanel;
        private Timer refreshTimer;
        private DataTable itemSource;
        private DataTable recipeSource;
        private DataTable variationSource;
        //private Boolean offsetFound = false;
        private int maxPage = 1;
        private int currentPage = 1;
        private variation selection = null;
        private USBBot bot = null;
        private Boolean offline = true;
        private int counter = 0;
        private readonly string settingFile = @"ACNHPoker.exe.config";

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
        const string villagerPath = @"villager/";

        public Form1()
        {
            InitializeComponent();
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
        private void villagerBtn_Paint(object sender, PaintEventArgs e)
        {
            Font font = new Font("Arial", 10, FontStyle.Bold);
            Brush brush = new System.Drawing.SolidBrush(System.Drawing.Color.White);
            e.Graphics.TranslateTransform(21, 14);
            e.Graphics.RotateTransform(90);
            e.Graphics.DrawString("Villager", font, brush, 0, 0);
        }

        private void inventoryBtn_Click(object sender, EventArgs e)
        {
            this.inventoryBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            this.critterBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.otherBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.villagerBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            inventoryLargePanel.Visible = true;
            otherLargePanel.Visible = false;
            critterLargePanel.Visible = false;
            villagerLargePanel.Visible = false;
        }

        private void critterBtn_Click(object sender, EventArgs e)
        {
            this.inventoryBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.critterBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            this.otherBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.villagerBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            inventoryLargePanel.Visible = false;
            otherLargePanel.Visible = false;
            critterLargePanel.Visible = true;
            villagerLargePanel.Visible = false;
            closeVariationMenu();
        }

        private void otherBtn_Click(object sender, EventArgs e)
        {
            this.inventoryBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.critterBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.otherBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            this.villagerBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));

            inventoryLargePanel.Visible = false;
            otherLargePanel.Visible = true;
            critterLargePanel.Visible = false;
            villagerLargePanel.Visible = false;
            closeVariationMenu();
        }

        private void Form1_LocationChanged(object sender, EventArgs e)
        {
            if (selection != null)
            {
                selection.Location = new System.Drawing.Point(this.Location.X + 7, this.Location.Y + 550);
            }
        }

        private Boolean validation()
        {

            byte[] Bank1 = Utilities.peekAddress(s, bot, Utilities.TownNameddress, 150); //TownNameddress
            byte[] Bank2 = Utilities.peekAddress(s, bot, Utilities.TurnipPurchasePriceAddr, 150); //TurnipPurchasePriceAddr
            byte[] Bank3 = Utilities.peekAddress(s, bot, Utilities.MasterRecyclingBase, 150); //MasterRecyclingBase
            byte[] Bank4 = Utilities.peekAddress(s, bot, Utilities.reactionAddress, 150); //reactionAddress
            byte[] Bank5 = Utilities.peekAddress(s, bot, Utilities.staminaAddress, 150); //staminaAddress

            string result1 = Utilities.ByteToHexString(Bank1);
            string result2 = Utilities.ByteToHexString(Bank2);
            string result3 = Utilities.ByteToHexString(Bank3);
            string result4 = Utilities.ByteToHexString(Bank4);
            string result5 = Utilities.ByteToHexString(Bank5);

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
            Debug.Print("Count : " + count1.ToString() + " " + count2.ToString() + " " + count3.ToString());
            if (count1 > 1 ^ count2 > 1 ^ count3 > 1)
            { return true; }
            else
            { return false; }
        }
    }
}
