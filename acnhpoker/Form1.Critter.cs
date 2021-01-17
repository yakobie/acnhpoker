using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace ACNHPoker
{
    public partial class Form1 : Form
    {

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

                DataGridViewCellStyle btnStyle = new DataGridViewCellStyle
                {
                    BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218))))),
                    Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                };

                DataGridViewCellStyle selectedbtnStyle = new DataGridViewCellStyle
                {
                    BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255))))),
                    Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                };

                DataGridViewCellStyle FontStyle = new DataGridViewCellStyle
                {
                    Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold),
                    Alignment = DataGridViewContentAlignment.MiddleLeft
                };

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
                    int spawnRate;
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

                    int spawnRate;
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
            DataGridViewCellStyle btnStyle = new DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218))))),
                Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            DataGridViewCellStyle selectedbtnStyle = new DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255))))),
                Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

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
            if (sound)
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

            DataGridViewCellStyle btnStyle = new DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218))))),
                Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            DataGridViewCellStyle selectedbtnStyle = new DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255))))),
                Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

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

            DataGridViewCellStyle btnStyle = new DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218))))),
                Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            DataGridViewCellStyle selectedbtnStyle = new DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255))))),
                Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

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
            if (sound)
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

            DataGridViewCellStyle btnStyle = new DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218))))),
                Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

            DataGridViewCellStyle selectedbtnStyle = new DataGridViewCellStyle
            {
                BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(80)))), ((int)(((byte)(255))))),
                Font = new System.Drawing.Font("Arial", 8F, System.Drawing.FontStyle.Bold),
                Alignment = DataGridViewContentAlignment.MiddleCenter
            };

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
            if (sound)
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
                b = new byte[78]; //[12 * 3 * 2];


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
            //Debug.Print(Encoding.UTF8.GetString(Utilities.transform(b)));
            Utilities.SendSpawnRate(s, bot, b, localIndex, type, ref counter);
            localIndex++;
            if (mode == 1)
            {
                for (int i = 0; i < b.Length; i++)
                    b[i] = source[size * localIndex + 2 + i];
            }
            Utilities.SendSpawnRate(s, bot, b, localIndex, type, ref counter);
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

            if (sound)
                System.Media.SystemSounds.Asterisk.Play();

            Invoke((MethodInvoker)delegate
            {
                enableBtn();
            });
        }
    }
}