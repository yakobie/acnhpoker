using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace ACNHPoker
{
    public partial class Form1 : Form
    {
        #region variable
        private static Socket s;
        private string version = "ACNH Poker R16 for v1.10.0";
        private inventorySlot selectedButton;
        private Villager[] V = null;
        private Button[] villagerButton = null;
        private int[] HouseList;
        private bool firstload = false;
        private static byte[] header;
        private bool blocker = false;
        private bool firstWarning = false;
        private int selectedSlot = 1;
        private Button selectedVillagerButton = null;
        private DataGridViewRow lastRow;
        private DataGridViewRow recipelastRow;
        private DataGridViewRow flowerlastRow;
        private DataGridViewRow favlastRow;

        private Panel currentPanel;
        private Timer refreshTimer;
        private static DataTable itemSource = null;
        private static DataTable recipeSource = null;
        private DataTable flowerSource = null;
        private DataTable favSource = null;
        private static DataTable variationSource = null;

        //private Boolean offsetFound = false;
        private int maxPage = 1;
        private int currentPage = 1;
        private variation selection = null;
        public map Map = null;
        public MapRegenerator R = null;
        private miniMap MiniMap = null;
        private USBBot bot = null;
        private bool offline = true;
        private bool allowUpdate = true;
        private int counter = 0;

        private Setting setting;
        private Friendship friendship;
        private teleport teleporter;
        private controller Controller;
        public dodo dodoSetup;
        private string IslandName = "";
        private readonly string settingFile = @"ACNHPoker.exe.config";
        private string languageSetting = "eng";

        private const string insectAppearFileName = @"InsectAppearParam.bin";
        private const string fishRiverAppearFileName = @"FishAppearRiverParam.bin";
        private const string fishSeaAppearFileName = @"FishAppearSeaParam.bin";
        private const string CreatureSeaAppearFileName = @"CreatureAppearSeaParam.bin";

        static private byte[] InsectAppearParam = LoadBinaryFile(insectAppearFileName);
        static private byte[] FishRiverAppearParam = LoadBinaryFile(fishRiverAppearFileName);
        static private byte[] FishSeaAppearParam = LoadBinaryFile(fishSeaAppearFileName);
        static private byte[] CreatureSeaAppearParam = LoadBinaryFile(CreatureSeaAppearFileName);

        private int[] insectRate;
        private int[] riverFishRate;
        private int[] seaFishRate;
        private int[] seaCreatureRate;
        private DataGridView currentGridView;

        private static Dictionary<string, string> OverrideDict;

        private bool overrideSetting = false;
        private bool disableValidation = false;
        public bool sound = true;
        private WaveOut waveOut;
        private static Object itemLock = new Object();
        private static Object villagerLock = new Object();

        #endregion

        #region Form Load
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = version;

            this.ipBox.Text = ConfigurationManager.AppSettings["ipAddress"];

            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

            if (config.AppSettings.Settings["override"].Value == "true")
            {
                overrideSetting = true;
                egg.BackColor = Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            }

            if (config.AppSettings.Settings["disableValidation"].Value == "true")
            {
                disableValidation = true;
            }

            if (config.AppSettings.Settings["Sound"].Value == "false")
            {
                sound = false;
            }

            setting = new Setting(this, overrideSetting, disableValidation, sound);
            if (overrideSetting)
                setting.overrideAddresses();

            if (File.Exists(Utilities.itemPath))
            {
                //load the csv
                itemSource = loadItemCSV(Utilities.itemPath);
                itemGridView.DataSource = itemSource;

                //set the ID row invisible
                itemGridView.Columns["id"].Visible = false;
                itemGridView.Columns["iName"].Visible = false;
                itemGridView.Columns["jpn"].Visible = false;
                itemGridView.Columns["tchi"].Visible = false;
                itemGridView.Columns["schi"].Visible = false;
                itemGridView.Columns["kor"].Visible = false;
                itemGridView.Columns["fre"].Visible = false;
                itemGridView.Columns["ger"].Visible = false;
                itemGridView.Columns["spa"].Visible = false;
                itemGridView.Columns["ita"].Visible = false;
                itemGridView.Columns["dut"].Visible = false;
                itemGridView.Columns["rus"].Visible = false;
                itemGridView.Columns["color"].Visible = false;

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
                itemGridView.Columns.Insert(13, imageColumn);
                imageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

                itemGridView.Columns["eng"].Width = 195;
                itemGridView.Columns["jpn"].Width = 195;
                itemGridView.Columns["tchi"].Width = 195;
                itemGridView.Columns["schi"].Width = 195;
                itemGridView.Columns["kor"].Width = 195;
                itemGridView.Columns["fre"].Width = 195;
                itemGridView.Columns["ger"].Width = 195;
                itemGridView.Columns["spa"].Width = 195;
                itemGridView.Columns["ita"].Width = 195;
                itemGridView.Columns["dut"].Width = 195;
                itemGridView.Columns["rus"].Width = 195;
                itemGridView.Columns["Image"].Width = 128;

                itemGridView.Columns["eng"].HeaderText = "Name";
                itemGridView.Columns["jpn"].HeaderText = "Name";
                itemGridView.Columns["tchi"].HeaderText = "Name";
                itemGridView.Columns["schi"].HeaderText = "Name";
                itemGridView.Columns["kor"].HeaderText = "Name";
                itemGridView.Columns["fre"].HeaderText = "Name";
                itemGridView.Columns["ger"].HeaderText = "Name";
                itemGridView.Columns["spa"].HeaderText = "Name";
                itemGridView.Columns["ita"].HeaderText = "Name";
                itemGridView.Columns["dut"].HeaderText = "Name";
                itemGridView.Columns["rus"].HeaderText = "Name";
            }
            else
            {
                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
                myMessageBox.Show("[Warning] Missing items.csv file!", "Missing file!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (File.Exists(Utilities.overridePath))
            {
                OverrideDict = CreateOverride(Utilities.overridePath);
            }

            if (File.Exists(Utilities.recipePath))
            {
                recipeSource = loadItemCSV(Utilities.recipePath);
                recipeGridView.DataSource = recipeSource;

                recipeGridView.Columns["id"].Visible = false;
                recipeGridView.Columns["iName"].Visible = false;
                recipeGridView.Columns["jpn"].Visible = false;
                recipeGridView.Columns["tchi"].Visible = false;
                recipeGridView.Columns["schi"].Visible = false;
                recipeGridView.Columns["kor"].Visible = false;
                recipeGridView.Columns["fre"].Visible = false;
                recipeGridView.Columns["ger"].Visible = false;
                recipeGridView.Columns["spa"].Visible = false;
                recipeGridView.Columns["ita"].Visible = false;
                recipeGridView.Columns["dut"].Visible = false;
                recipeGridView.Columns["rus"].Visible = false;

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

                recipeGridView.Columns.Insert(13, recipeimageColumn);
                recipeimageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

                recipeGridView.Columns["eng"].Width = 195;
                recipeGridView.Columns["jpn"].Width = 195;
                recipeGridView.Columns["tchi"].Width = 195;
                recipeGridView.Columns["schi"].Width = 195;
                recipeGridView.Columns["kor"].Width = 195;
                recipeGridView.Columns["fre"].Width = 195;
                recipeGridView.Columns["ger"].Width = 195;
                recipeGridView.Columns["spa"].Width = 195;
                recipeGridView.Columns["ita"].Width = 195;
                recipeGridView.Columns["dut"].Width = 195;
                recipeGridView.Columns["rus"].Width = 195;
                recipeGridView.Columns["Image"].Width = 128;

                recipeGridView.Columns["eng"].HeaderText = "Name";
                recipeGridView.Columns["jpn"].HeaderText = "Name";
                recipeGridView.Columns["tchi"].HeaderText = "Name";
                recipeGridView.Columns["schi"].HeaderText = "Name";
                recipeGridView.Columns["kor"].HeaderText = "Name";
                recipeGridView.Columns["fre"].HeaderText = "Name";
                recipeGridView.Columns["ger"].HeaderText = "Name";
                recipeGridView.Columns["spa"].HeaderText = "Name";
                recipeGridView.Columns["ita"].HeaderText = "Name";
                recipeGridView.Columns["dut"].HeaderText = "Name";
                recipeGridView.Columns["rus"].HeaderText = "Name";
            }
            else
            {
                recipeModeBtn.Visible = false;
                recipeGridView.Visible = false;
            }

            if (File.Exists(Utilities.flowerPath))
            {
                flowerSource = loadItemCSV(Utilities.flowerPath);
                flowerGridView.DataSource = flowerSource;

                flowerGridView.Columns["id"].Visible = false;
                flowerGridView.Columns["iName"].Visible = false;
                flowerGridView.Columns["jpn"].Visible = false;
                flowerGridView.Columns["tchi"].Visible = false;
                flowerGridView.Columns["schi"].Visible = false;
                flowerGridView.Columns["kor"].Visible = false;
                flowerGridView.Columns["fre"].Visible = false;
                flowerGridView.Columns["ger"].Visible = false;
                flowerGridView.Columns["spa"].Visible = false;
                flowerGridView.Columns["ita"].Visible = false;
                flowerGridView.Columns["dut"].Visible = false;
                flowerGridView.Columns["rus"].Visible = false;
                flowerGridView.Columns["value"].Visible = false;

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

                flowerGridView.Columns.Insert(13, flowerimageColumn);
                flowerimageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

                flowerGridView.Columns["eng"].Width = 195;
                flowerGridView.Columns["jpn"].Width = 195;
                flowerGridView.Columns["tchi"].Width = 195;
                flowerGridView.Columns["schi"].Width = 195;
                flowerGridView.Columns["kor"].Width = 195;
                flowerGridView.Columns["fre"].Width = 195;
                flowerGridView.Columns["ger"].Width = 195;
                flowerGridView.Columns["spa"].Width = 195;
                flowerGridView.Columns["ita"].Width = 195;
                flowerGridView.Columns["dut"].Width = 195;
                flowerGridView.Columns["rus"].Width = 195;
                flowerGridView.Columns["Image"].Width = 128;

                flowerGridView.Columns["eng"].HeaderText = "Name";
                flowerGridView.Columns["jpn"].HeaderText = "Name";
                flowerGridView.Columns["tchi"].HeaderText = "Name";
                flowerGridView.Columns["schi"].HeaderText = "Name";
                flowerGridView.Columns["kor"].HeaderText = "Name";
                flowerGridView.Columns["fre"].HeaderText = "Name";
                flowerGridView.Columns["ger"].HeaderText = "Name";
                flowerGridView.Columns["spa"].HeaderText = "Name";
                flowerGridView.Columns["ita"].HeaderText = "Name";
                flowerGridView.Columns["dut"].HeaderText = "Name";
                flowerGridView.Columns["rus"].HeaderText = "Name";
            }
            else
            {
                flowerModeBtn.Visible = false;
                flowerGridView.Visible = false;
            }

            if (File.Exists(Utilities.variationPath))
            {
                variationSource = loadItemCSV(Utilities.variationPath);
            }
            else
            {
                variationModeButton.Visible = false;
            }

            if (!File.Exists(Utilities.favPath))
            {
                string favheader = "id" + " ; " + "iName" + " ; " + "Name" + " ; " + "value" + " ; ";

                using (StreamWriter sw = File.CreateText(Utilities.favPath))
                {
                    sw.WriteLine(favheader);
                }
            }

            favSource = loadCSVwoKey(Utilities.favPath);
            favGridView.DataSource = favSource;

            favGridView.Columns["id"].Visible = false;
            favGridView.Columns["iName"].Visible = false;
            favGridView.Columns["value"].Visible = false;

            favGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            favGridView.DefaultCellStyle.BackColor = Color.FromArgb(255, 47, 49, 54);
            favGridView.DefaultCellStyle.ForeColor = Color.FromArgb(255, 114, 105, 110);
            favGridView.DefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 57, 60, 67);

            favGridView.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(255, 57, 60, 67);
            favGridView.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            favGridView.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(255, 57, 60, 67);

            favGridView.EnableHeadersVisualStyles = false;

            DataGridViewImageColumn favimageColumn = new DataGridViewImageColumn
            {
                Name = "Image",
                HeaderText = "Image",
                ImageLayout = DataGridViewImageCellLayout.Zoom
            };
            favGridView.Columns.Insert(4, favimageColumn);
            favimageColumn.SortMode = DataGridViewColumnSortMode.NotSortable;

            favGridView.Columns["Name"].Width = 195;
            favGridView.Columns["Image"].Width = 128;


            if (!Directory.Exists(Directory.GetCurrentDirectory() + "\\" + "img"))
            {
                ImageDownloader imageDownloader = new ImageDownloader();
                imageDownloader.ShowDialog();
            }

            currentPanel = itemModePanel;

            LanguageSetup(config.AppSettings.Settings["language"].Value);
            this.KeyPreview = true;
        }
        private Dictionary<string, string> CreateOverride(string path)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();

            if (File.Exists(path))
            {
                string[] lines = File.ReadAllLines(path);

                foreach (string line in lines)
                {
                    string[] parts = line.Split(new[] { " ; " }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 3)
                    {
                        dict.Add(parts[1], parts[2]);
                    }
                    //Debug.Print(parts[0]);
                }
            }

            return dict;
        }

        private void hideAllLanguage()
        {
            if (itemGridView.Columns.Contains("id"))
            {
                itemGridView.Columns["eng"].Visible = false;
                itemGridView.Columns["jpn"].Visible = false;
                itemGridView.Columns["tchi"].Visible = false;
                itemGridView.Columns["schi"].Visible = false;
                itemGridView.Columns["kor"].Visible = false;
                itemGridView.Columns["fre"].Visible = false;
                itemGridView.Columns["ger"].Visible = false;
                itemGridView.Columns["spa"].Visible = false;
                itemGridView.Columns["ita"].Visible = false;
                itemGridView.Columns["dut"].Visible = false;
                itemGridView.Columns["rus"].Visible = false;
            }

            if (recipeGridView.Columns.Contains("id"))
            {
                recipeGridView.Columns["eng"].Visible = false;
                recipeGridView.Columns["jpn"].Visible = false;
                recipeGridView.Columns["tchi"].Visible = false;
                recipeGridView.Columns["schi"].Visible = false;
                recipeGridView.Columns["kor"].Visible = false;
                recipeGridView.Columns["fre"].Visible = false;
                recipeGridView.Columns["ger"].Visible = false;
                recipeGridView.Columns["spa"].Visible = false;
                recipeGridView.Columns["ita"].Visible = false;
                recipeGridView.Columns["dut"].Visible = false;
                recipeGridView.Columns["rus"].Visible = false;
            }

            if (flowerGridView.Columns.Contains("id"))
            {
                flowerGridView.Columns["eng"].Visible = false;
                flowerGridView.Columns["jpn"].Visible = false;
                flowerGridView.Columns["tchi"].Visible = false;
                flowerGridView.Columns["schi"].Visible = false;
                flowerGridView.Columns["kor"].Visible = false;
                flowerGridView.Columns["fre"].Visible = false;
                flowerGridView.Columns["ger"].Visible = false;
                flowerGridView.Columns["spa"].Visible = false;
                flowerGridView.Columns["ita"].Visible = false;
                flowerGridView.Columns["dut"].Visible = false;
                flowerGridView.Columns["rus"].Visible = false;
            }
        }

        #endregion

        #region Side Tab
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
        #endregion

        #region Mode Button
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
        #endregion

        #region Setting
        private void configBtn_Click(object sender, EventArgs e)
        {
            setting.ShowDialog();
        }

        public void toggleOverride()
        {
            if (overrideSetting == true)
            {
                overrideSetting = false;
                egg.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            }
            else
            {
                overrideSetting = true;
                egg.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255)))));
            }
        }

        public void toggleValidation()
        {
            if (disableValidation == true)
                disableValidation = false;
            else
                disableValidation = true;
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

        #endregion

        #region Language
        private void Language_SelectedIndexChanged(object sender, EventArgs e)
        {
            itemSearchBox.Clear();

            Configuration config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);

            switch (Language.SelectedIndex)
            {
                case 0:
                    languageSetting = "eng";
                    config.AppSettings.Settings["language"].Value = "eng";
                    break;
                case 1:
                    languageSetting = "jpn";
                    config.AppSettings.Settings["language"].Value = "jpn";
                    break;
                case 2:
                    languageSetting = "tchi";
                    config.AppSettings.Settings["language"].Value = "tchi";
                    break;
                case 3:
                    languageSetting = "schi";
                    config.AppSettings.Settings["language"].Value = "schi";
                    break;
                case 4:
                    languageSetting = "kor";
                    config.AppSettings.Settings["language"].Value = "kor";
                    break;
                case 5:
                    languageSetting = "fre";
                    config.AppSettings.Settings["language"].Value = "fre";
                    break;
                case 6:
                    languageSetting = "ger";
                    config.AppSettings.Settings["language"].Value = "ger";
                    break;
                case 7:
                    languageSetting = "spa";
                    config.AppSettings.Settings["language"].Value = "spa";
                    break;
                case 8:
                    languageSetting = "ita";
                    config.AppSettings.Settings["language"].Value = "ita";
                    break;
                case 9:
                    languageSetting = "dut";
                    config.AppSettings.Settings["language"].Value = "dut";
                    break;
                case 10:
                    languageSetting = "rus";
                    config.AppSettings.Settings["language"].Value = "rus";
                    break;
                default:
                    languageSetting = "eng";
                    config.AppSettings.Settings["language"].Value = "eng";
                    break;
            }

            config.Save(ConfigurationSaveMode.Minimal);

            if (itemGridView.Columns.Contains(languageSetting))
            {
                hideAllLanguage();
                itemGridView.Columns[languageSetting].Visible = true;
                recipeGridView.Columns[languageSetting].Visible = true;
                flowerGridView.Columns[languageSetting].Visible = true;
            }
        }

        private void LanguageSetup(string configLanguage)
        {
            switch (configLanguage)
            {
                case "eng":
                    Language.SelectedIndex = 0;
                    break;
                case "jpn":
                    Language.SelectedIndex = 1;
                    break;
                case "tchi":
                    Language.SelectedIndex = 2;
                    break;
                case "schi":
                    Language.SelectedIndex = 3;
                    break;
                case "kor":
                    Language.SelectedIndex = 4;
                    break;
                case "fre":
                    Language.SelectedIndex = 5;
                    break;
                case "ger":
                    Language.SelectedIndex = 6;
                    break;
                case "spa":
                    Language.SelectedIndex = 7;
                    break;
                case "ita":
                    Language.SelectedIndex = 8;
                    break;
                case "dut":
                    Language.SelectedIndex = 9;
                    break;
                case "rus":
                    Language.SelectedIndex = 10;
                    break;
                default:
                    Language.SelectedIndex = 0;
                    break;
            }
        }

        #endregion

        #region Fav
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            int index = favGridView.CurrentCell.RowIndex;
            if (favGridView.CurrentCell != null)
            {
                //Debug.Print(index.ToString());

                string id = favGridView.Rows[index].Cells["id"].Value.ToString();
                string iName = favGridView.Rows[index].Cells["iName"].Value.ToString();
                string value = favGridView.Rows[index].Cells["value"].Value.ToString();

                File_DeleteLine(Utilities.favPath, id, iName, value);

                favGridView.Rows.RemoveAt(index);
                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
            }
        }

        private void File_DeleteLine(string Path, string key1, string key2, string key3)
        {
            StringBuilder sb = new StringBuilder();
            string line;
            using (StreamReader sr = new StreamReader(Path))
            {
                while (!sr.EndOfStream)
                {
                    line = sr.ReadLine();
                    if (!line.Contains(key1) || !line.Contains(key2) || !line.Contains(key3))
                    {
                        using (StringWriter sw = new StringWriter(sb))
                        {
                            sw.WriteLine(line);
                        }
                    }
                    else
                    {
                        //MessageBox.Show(line);
                    }
                }
            }
            using (StreamWriter sw = new StreamWriter(Path))
            {
                sw.Write(sb.ToString());
            }
        }
        #endregion

        #region Validation
        private Boolean validation()
        {
            //return true;
            if (disableValidation)
                return false;
            try
            {
                byte[] Bank1 = Utilities.peekAddress(s, bot, Utilities.TownNameddress, 150); //TownNameddress
                byte[] Bank2 = Utilities.peekAddress(s, bot, Utilities.TurnipPurchasePriceAddr, 150); //TurnipPurchasePriceAddr
                byte[] Bank3 = Utilities.peekAddress(s, bot, Utilities.MasterRecyclingBase, 150); //MasterRecyclingBase
                byte[] Bank4 = Utilities.peekAddress(s, bot, Utilities.playerReactionAddress, 150); //reactionAddress
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
                if (count1 > 1 || count2 > 1 || count3 > 1)
                { return true; }
                else
                { return false; }
            }
            catch (Exception e)
            {
                myMessageBox.Show(e.Message.ToString(), "Todo : this is dumb");
                return false;
            }
        }

        #endregion

        private void mapDropperBtn_Click(object sender, EventArgs e)
        {
            itemSearchBox.Clear();

            if (Map == null)
            {
                Map = new map(s, bot, Utilities.itemPath, Utilities.recipePath, Utilities.flowerPath, Utilities.variationPath, Utilities.favPath, this, Utilities.imagePath, OverrideDict, sound);
                Map.Show();
            }
        }

        private void regeneratorBtn_Click(object sender, EventArgs e)
        {
            if (!Utilities.IsConnected(s))
            {
                if (!reconnect())
                    return;
            }

            if (R == null)
            {
                R = new MapRegenerator(s, this, sound);
                //this.Hide();
                R.Show();
            }
        }

        private bool reconnect()
        {
            s.Close();

            s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ipBox.Text), 6000);

            IAsyncResult result = s.BeginConnect(ep, null, null);
            bool conSuceded = result.AsyncWaitHandle.WaitOne(3000, true);


            if (conSuceded == true)
            {
                try
                {
                    s.EndConnect(result);
                    teleporter = new teleport(s);
                    Controller = new controller(s, IslandName);
                    return true;
                }
                catch
                {
                    myMessageBox.Show("Connection Totally Wrecked. Application restarting...", "Why did you leave me?", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Application.Restart();
                    return false;
                }
            }
            else
            {
                myMessageBox.Show("Connection Totally Wrecked. Application restarting...", "Why did you leave me?", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Restart();
                return false;
            }
        }

        #region Form Control
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Log.logEvent("MainForm", "Form Closed");
        }

        private void Form1_LocationChanged(object sender, EventArgs e)
        {
            if (selection != null)
            {
                selection.Location = new System.Drawing.Point(this.Location.X + 7, this.Location.Y + 550);
            }
        }
        #endregion

        private void button11_Click(object sender, EventArgs e)
        {
            int i = 0;

            SaveFileDialog file = new SaveFileDialog()
            {
                Filter = "New Horizons Villager (*.nhv2)|*.nhv2",
                //FileName = V[i].GetInternalName() + ".nhv2",
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

            Thread dumpThread = new Thread(delegate () { dumpVillager2(i, file); });
            dumpThread.Start();
        }

        private void dumpVillager2(int i, SaveFileDialog file)
        {
            //byte[] b1 = Utilities.ReadByteArray(s, Utilities.VillagerBuffer1 + (i * Utilities.VillagerSize), (int)Utilities.VillagerSize, ref counter);
            byte[] b2 = Utilities.ReadByteArray(s, Utilities.VillagerAddress + (i * Utilities.VillagerSize), (int)Utilities.VillagerSize, ref counter);
            //byte[] b3 = Utilities.ReadByteArray(s, Utilities.VillagerBuffer2 + (i * Utilities.VillagerSize), (int)Utilities.VillagerSize, ref counter);

            //File.WriteAllBytes(file.FileName + "1", b1);
            File.WriteAllBytes(file.FileName + "2", b2);
            //File.WriteAllBytes(file.FileName + "3", b3);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            teleport.dump();
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Thread stateThread = new Thread(delegate () { trystate(); });
            stateThread.Start();
        }
        private void trystate()
        {
            do
            {
                Debug.Print(teleport.GetLocationState().ToString());
                Thread.Sleep(2000);
            } while (true);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            controller.talkAndGetDodoCode();
        }

        private void button16_Click(object sender, EventArgs e)
        {
            SaveFileDialog file = new SaveFileDialog()
            {
                Filter = "New Horizons Villager (*.nhv2)|*.nhv2",
                //FileName = V[i].GetInternalName() + ".nhv2",
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

            Thread dumpThread = new Thread(delegate () { dumpHead(file); });
            dumpThread.Start();
        }

        private void dumpHead(SaveFileDialog file)
        {
            for (int i = 0; i < 10; i++)
            {
                //byte[] b = Utilities.ReadByteArray(s, Utilities.VillagerBuffer1 + (i * Utilities.VillagerSize), (int)Utilities.VillagerSize, ref counter);
                //File.WriteAllBytes(file.FileName + i, b);
            }
        }

        private void button17_Click(object sender, EventArgs e)
        {
            string[] namelist = new string[8];

            string saveFolder = @"save\";

            int num = 0;

            using (StreamWriter sw = File.CreateText(saveFolder + "visitor.txt"))
            {
                for (int i = 0; i < 8; i++)
                {
                    if (i == 0)
                        continue;
                    namelist[i] = Utilities.GetVisitorName(s, null, i);
                    if (namelist[i].Equals(String.Empty))
                        sw.WriteLine("[Empty]");
                    else
                    {
                        sw.WriteLine(namelist[i]);
                        num++;
                    }
                }
                if (num >= 7)
                {
                    sw.WriteLine("Num of Visitor : " + num);
                    sw.WriteLine(" [Island Full] ");
                }
            }


        }

        private void button21_Click(object sender, EventArgs e)
        {
            controller.dropItem();
        }

        private void dodoHelperBtn_Click(object sender, EventArgs e)
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
                                    "3. Return to the game and dock your Switch if needed. Try pressing the buttons below to test the virtual controller.\n" +
                                    "4. If the virtual controller does not response, try the \"Detach\" button first, then the \"A\" button.\n" +
                                    "5. If the virtual controller still does not appear, try restart your Switch.\n\n" +
                                    ">> Please try the buttons below to test the virtual controller. <<"
                                    );
            }
        }

        private void button18_Click(object sender, EventArgs e)
        {
            controller.emote(0);
        }
    }
}
