using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ACNHPoker
{
    public partial class MapRegenerator : Form
    {
        private static Socket s;
        private Form1 main;
        private int counter = 0;
        private Boolean loop = false;
        private Thread RegenThread = null;
        private static Object mapLock = new Object();
        private int delayTime = 50;
        private int pauseTime = 70;
        private bool sound;

        private const string saveFolder = @"save\";
        private const string logFile = @"VisitorLog.csv";
        private string logPath = saveFolder + logFile;

        private miniMap MiniMap = null;
        private int anchorX = -1;
        private int anchorY = -1;
        private byte[] tempData;
        private string tempFilename;

        public MapRegenerator(Socket S, Form1 Main, bool Sound)
        {
            s = S;
            main = Main;
            sound = Sound;

            InitializeComponent();
            FinMsg.SelectionAlignment = HorizontalAlignment.Center;
            logName.Text = logFile;
            Random random = new Random();
            int v = random.Next(8192);
            Debug.Print(v.ToString());
            if (v == 6969)
            {
                this.Icon = ACNHPoker.Properties.Resources.k;
                this.trayIcon.Icon = this.Icon = ACNHPoker.Properties.Resources.k;
            }
            else if (v <= 4096)
            {
                this.Icon = ACNHPoker.Properties.Resources.f;
                this.trayIcon.Icon = this.Icon = ACNHPoker.Properties.Resources.f;
            }
        }

        private void saveMapBtn_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog file = new SaveFileDialog()
                {
                    Filter = "New Horizons Fasil (*.nhf)|*.nhf",
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
                for (int i = 0; i < temp.Length - 1; i++)
                    path = path + temp[i] + "\\";

                config.AppSettings.Settings["LastSave"].Value = path;
                config.Save(ConfigurationSaveMode.Minimal);

                UInt32 address = Utilities.mapZero;

                Thread LoadThread = new Thread(delegate () { saveMapFloor(address, file); });
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
        }

        private void saveMapFloor(UInt32 address, SaveFileDialog file)
        {
            showMapWait(42, "Saving...");

            byte[] save = Utilities.ReadByteArray8(s, address, 0x54000, ref counter);

            File.WriteAllBytes(file.FileName, save);

            if (sound)
                System.Media.SystemSounds.Asterisk.Play();

            hideMapWait();

            this.Invoke((MethodInvoker)delegate
            {
                FinMsg.Visible = true;
                FinMsg.Text = "Template Saved!";
            });
        }

        private void loadMapBtn_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog file = new OpenFileDialog()
                {
                    Filter = "New Horizons Fasil (*.nhf)|*.nhf|All files (*.*)|*.*",
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
                for (int i = 0; i < temp.Length - 1; i++)
                    path = path + temp[i] + "\\";

                config.AppSettings.Settings["LastLoad"].Value = path;
                config.Save(ConfigurationSaveMode.Minimal);

                byte[] data = File.ReadAllBytes(file.FileName);

                UInt32 address = Utilities.mapZero;

                byte[][] b = new byte[42][];

                for (int i = 0; i < 42; i++)
                {
                    b[i] = new byte[0x2000];
                    Buffer.BlockCopy(data, i * 0x2000, b[i], 0x0, 0x2000);
                }

                Thread LoadThread = new Thread(delegate () { loadMapFloor(b, address); });
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
        }

        private void loadMapFloor(byte[][] b, UInt32 address)
        {
            showMapWait(42, "Loading...");

            for (int i = 0; i < 42; i++)
            {
                Utilities.SendByteArray8(s, address + (i * 0x2000), b[i], 0x2000, ref counter);
                Thread.Sleep(250);
            }

            if (sound)
                System.Media.SystemSounds.Asterisk.Play();

            hideMapWait();

            this.Invoke((MethodInvoker)delegate
            {
                FinMsg.Visible = true;
                FinMsg.Text = "Template Loaded!";
            });
        }

        private void startRegen_Click(object sender, EventArgs e)
        {
            FinMsg.Text = "";
            delayTime = int.Parse(delay.Text);

            if (startRegen.Tag.ToString().Equals("Start"))
            {
                //updateVisitorName();

                OpenFileDialog file = new OpenFileDialog()
                {
                    Filter = "New Horizons Fasil (*.nhf)|*.nhf|All files (*.*)|*.*",
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
                for (int i = 0; i < temp.Length - 1; i++)
                    path = path + temp[i] + "\\";

                config.AppSettings.Settings["LastLoad"].Value = path;
                config.Save(ConfigurationSaveMode.Minimal);

                loop = true;
                startRegen.Tag = "Stop";
                startRegen.BackColor = Color.Orange;
                startRegen.Text = "Stop Regen";
                saveMapBtn.Enabled = false;
                loadMapBtn.Enabled = false;
                backBtn.Enabled = false;
                startRegen2.Enabled = false;


                byte[] data = File.ReadAllBytes(file.FileName);

                UInt32 address = Utilities.mapZero;

                byte[][] b = new byte[42][];

                for (int i = 0; i < 42; i++)
                {
                    b[i] = new byte[0x2000];
                    Buffer.BlockCopy(data, i * 0x2000, b[i], 0x0, 0x2000);
                }

                string[] name = file.FileName.Split('\\');

                RegenThread = new Thread(delegate () { regenMapFloor(b, address, name[name.Length - 1]); });
                RegenThread.Start();
            }
            else
            {
                loop = false;
                startRegen.Tag = "Start";
                startRegen.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
                startRegen.Text = "Cast Regen";
                saveMapBtn.Enabled = true;
                loadMapBtn.Enabled = true;
                backBtn.Enabled = true;
                startRegen2.Enabled = true;
            }
        }

        private void regenMapFloor(byte[][] b, UInt32 address, string name)
        {
            string regenMsg = "Regenerating... " + name;

            showMapWait(42, regenMsg);

            byte[] c = new byte[0x2000];

            int writeCount = 0;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            string newVisitor;
            TimeSpan ts;

            Utilities.sendBlankName(s);

            do
            {
                counter = 0;
                writeCount = 0;

                newVisitor = getVisitorName();

                if (!newVisitor.Equals(string.Empty))
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        visitorNameBox.Text = newVisitor;
                        WaitMessagebox.Text = "Paused. " + newVisitor + " arriving!";
                        CreateLog(newVisitor);
                        PauseTimeLabel.Visible = true;
                        PauseTimer.Start();

                    });

                    Thread.Sleep(70000);
                    Utilities.sendBlankName(s);

                    this.Invoke((MethodInvoker)delegate
                    {
                        PauseTimeLabel.Visible = false;
                        PauseTimer.Stop();
                        pauseTime = 70;
                        PauseTimeLabel.Text = pauseTime.ToString();
                        WaitMessagebox.Text = regenMsg;
                    });
                }

                for (int i = 0; i < 42; i++)
                {
                    lock (mapLock)
                    {
                        c = Utilities.ReadByteArray8(s, address + (i * 0x2000), 0x2000, ref counter);

                        if (c != null)
                        {
                            if (SafeEquals(b[i], c))
                            {
                                //Debug.Print("Same " + i);
                                Thread.Sleep(delayTime);
                            }
                            else
                            {
                                Debug.Print("Replace " + i);
                                Utilities.SendByteArray8(s, address + (i * 0x2000), b[i], 0x2000, ref writeCount);
                                Thread.Sleep(500);
                            }
                        }
                        else
                        {
                            Debug.Print("Null " + i);
                            Thread.Sleep(10000);
                        }
                    }
                    if (!loop)
                        break;
                }

                this.Invoke((MethodInvoker)delegate
                {
                    ts = stopWatch.Elapsed;
                    timeLabel.Text = Utilities.precedingZeros(ts.Hours.ToString(), 2) + ":" + Utilities.precedingZeros(ts.Minutes.ToString(), 2) + ":" + Utilities.precedingZeros(ts.Seconds.ToString(), 2);
                });

            } while (loop);

            stopWatch.Stop();

            hideMapWait();

            if (sound)
                System.Media.SystemSounds.Asterisk.Play();
        }

        private static bool SafeEquals(byte[] strA, byte[] strB)
        {
            int length = strA.Length;
            if (length != strB.Length)
            {
                return false;
            }
            for (int i = 0; i < length; i++)
            {
                if (strA[i] != strB[i]) return false;
            }
            return true;
        }

        private void showMapWait(int size, string msg = "")
        {
            this.Invoke((MethodInvoker)delegate
            {
                WaitMessagebox.SelectionAlignment = HorizontalAlignment.Center;
                WaitMessagebox.Text = msg;
                counter = 0;
                MapProgressBar.Maximum = size + 5;
                MapProgressBar.Value = counter;
                PleaseWaitPanel.Visible = true;
                ProgressTimer.Start();
            });
        }

        private void hideMapWait()
        {
            this.Invoke((MethodInvoker)delegate
            {
                PleaseWaitPanel.Visible = false;
                ProgressTimer.Stop();
            });
        }

        private void ProgressTimer_Tick(object sender, EventArgs e)
        {
            Invoke((MethodInvoker)delegate
            {
                if (counter <= MapProgressBar.Maximum)
                    MapProgressBar.Value = counter;
                else
                    MapProgressBar.Value = MapProgressBar.Maximum;
            });
        }

        private void MapRegenerator_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (RegenThread != null)
            {
                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
                RegenThread.Abort();
            }
            main.R = null;
            main.Show();
        }

        private void trayIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            Show();
            this.ShowInTaskbar = true;
            this.WindowState = FormWindowState.Normal;
        }

        private void hideBtn_Click(object sender, EventArgs e)
        {
            this.ShowInTaskbar = false;
            Hide();
        }

        private void backBtn_Click(object sender, EventArgs e)
        {
            if (RegenThread != null)
            {
                if (sound)
                    System.Media.SystemSounds.Asterisk.Play();
                RegenThread.Abort();
            }
            this.Close();
        }

        private void startRegen2_Click(object sender, EventArgs e)
        {
            FinMsg.Text = "";
            delayTime = int.Parse(delay.Text);

            if (startRegen2.Tag.ToString().Equals("Start"))
            {
                OpenFileDialog file = new OpenFileDialog()
                {
                    Filter = "New Horizons Fasil (*.nhf)|*.nhf|All files (*.*)|*.*",
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
                for (int i = 0; i < temp.Length - 1; i++)
                    path = path + temp[i] + "\\";

                config.AppSettings.Settings["LastLoad"].Value = path;
                config.Save(ConfigurationSaveMode.Minimal);

                byte[] data = File.ReadAllBytes(file.FileName);

                UInt32 address = Utilities.mapZero;

                DialogResult dialogResult = MessageBox.Show("Would you like to limit the area of \"ignoring empty tiles\"?" + "\n\n" +
                                                            "This would allow you to pick a 7 x 7 area which the regenerator would only ignore."
                                                            , "Choose an area to ignore?", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    mapPanel.Visible = true;
                    logPanel.Visible = false;

                    tempData = data;
                    string[] name = file.FileName.Split('\\');
                    tempFilename = name[name.Length - 1];

                    this.Width = 485;
                    if (MiniMap == null)
                    {
                        byte[] Acre = Utilities.getAcre(s, null);

                        if (MiniMap == null)
                            MiniMap = new miniMap(data, Acre);

                        miniMapBox.BackgroundImage = MiniMap.combineMap(MiniMap.drawBackground(), MiniMap.drawItemMap());
                    }
                    else
                        return;

                    byte[] Coordinate = Utilities.getCoordinate(s, null);
                    int x = BitConverter.ToInt32(Coordinate, 0);
                    int y = BitConverter.ToInt32(Coordinate, 4);

                    anchorX = x - 0x24;
                    anchorY = y - 0x18;

                    if (anchorX < 3 || anchorY < 3 || anchorX > 108 || anchorY > 92)
                    {
                        anchorX = 3;
                        anchorY = 3;
                    }
                    xCoordinate.Text = anchorX.ToString();
                    yCoordinate.Text = anchorY.ToString();
                    miniMapBox.Image = MiniMap.drawSelectSquare(anchorX * 2, anchorY * 2);
                }
                else
                {
                    loop = true;
                    startRegen2.Tag = "Stop";
                    startRegen2.BackColor = Color.Orange;
                    startRegen2.Text = "Stop Moogle Regenja";
                    saveMapBtn.Enabled = false;
                    loadMapBtn.Enabled = false;
                    backBtn.Enabled = false;
                    startRegen.Enabled = false;


                    byte[][] b = new byte[56][];
                    bool[][] isEmpty = new bool[56][];


                    for (int i = 0; i < 56; i++)
                    {
                        b[i] = new byte[0x1800];
                        Buffer.BlockCopy(data, i * 0x1800, b[i], 0x0, 0x1800);

                        isEmpty[i] = new bool[0x1800];
                        buildEmptyTable(b[i], ref isEmpty[i]);
                    }

                    string[] name = file.FileName.Split('\\');

                    RegenThread = new Thread(delegate () { regenMapFloor2(b, address, isEmpty, name[name.Length - 1]); });
                    RegenThread.Start();
                }
            }
            else
            {
                WaitMessagebox.Text = "Stopping Regen...";
                loop = false;
                startRegen2.Tag = "Start";
                startRegen2.BackColor = Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
                startRegen2.Text = "Cast Moogle Regenja";
                saveMapBtn.Enabled = true;
                loadMapBtn.Enabled = true;
                backBtn.Enabled = true;
                startRegen.Enabled = true;
            }
        }

        private void startBtn_Click(object sender, EventArgs e)
        {
            mapPanel.Visible = false;
            MiniMap = null;
            this.Width = 250;

            UInt32 address = Utilities.mapZero;

            loop = true;
            startRegen2.Tag = "Stop";
            startRegen2.BackColor = Color.Orange;
            startRegen2.Text = "Stop Moogle Regenja";
            saveMapBtn.Enabled = false;
            loadMapBtn.Enabled = false;
            backBtn.Enabled = false;
            startRegen.Enabled = false;


            byte[][] b = new byte[56][];
            bool[][] isEmpty = new bool[56][];


            for (int i = 0; i < 56; i++)
            {
                b[i] = new byte[0x1800];
                Buffer.BlockCopy(tempData, i * 0x1800, b[i], 0x0, 0x1800);

                isEmpty[i] = new bool[0x1800];
                buildEmptyTable(b[i], ref isEmpty[i], i * 2, i * 2 + 1);
            }

            RegenThread = new Thread(delegate () { regenMapFloor2(b, address, isEmpty, tempFilename); });
            RegenThread.Start();
        }

        private void regenMapFloor2(byte[][] b, UInt32 address, bool[][] isEmpty, string name)
        {
            string regenMsg = "Regenerating... " + name;
            showMapWait(56, regenMsg);

            byte[][] u = new byte[56][];

            for (int i = 0; i < 56; i++)
            {
                u[i] = new byte[0x1800];
                Buffer.BlockCopy(b[i], 0, u[i], 0x0, 0x1800);
            }



            byte[] c = new byte[0x2000];

            int writeCount;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            string newVisitor;
            TimeSpan ts;

            Utilities.sendBlankName(s);

            do
            {
                counter = 0;
                writeCount = 0;

                newVisitor = getVisitorName();

                if (!newVisitor.Equals(string.Empty))
                {
                    this.Invoke((MethodInvoker)delegate
                    {
                        visitorNameBox.Text = newVisitor;
                        WaitMessagebox.Text = "Paused. " + newVisitor + " arriving!";
                        CreateLog(newVisitor);
                        PauseTimeLabel.Visible = true;
                        PauseTimer.Start();
                        
                    });

                    Thread.Sleep(70000);
                    Utilities.sendBlankName(s);

                    this.Invoke((MethodInvoker)delegate
                    {
                        PauseTimeLabel.Visible = false;
                        PauseTimer.Stop();
                        pauseTime = 70;
                        PauseTimeLabel.Text = pauseTime.ToString();
                        WaitMessagebox.Text = regenMsg;
                    });
                }

                for (int i = 0; i < 56; i++)
                {
                    lock (mapLock)
                    {
                        c = Utilities.ReadByteArray8(s, address + (i * 0x1800), 0x1800, ref counter);

                        if (c != null)
                        {
                            if (Difference(b[i], ref u[i], isEmpty[i], c))
                            {
                                //Debug.Print("Same " + i);
                                Thread.Sleep(delayTime);
                            }
                            else
                            {
                                Debug.Print("Replace " + i);
                                Utilities.SendByteArray8(s, address + (i * 0x1800), u[i], 0x1800, ref writeCount);
                                Thread.Sleep(500);
                            }
                        }
                        else
                        {
                            Debug.Print("Null " + i);
                            Thread.Sleep(10000);
                        }
                    }
                    if (!loop)
                        break;
                }

                this.Invoke((MethodInvoker)delegate
                {
                    ts = stopWatch.Elapsed;
                    timeLabel.Text = Utilities.precedingZeros(ts.Hours.ToString(),2) + ":" + Utilities.precedingZeros(ts.Minutes.ToString(), 2) + ":" + Utilities.precedingZeros(ts.Seconds.ToString(), 2);
                });

            } while (loop);

            stopWatch.Stop();

            hideMapWait();

            if (sound)
                System.Media.SystemSounds.Asterisk.Play();
        }

        private void buildEmptyTable(byte[] org, ref bool[] table)
        {
            byte[] Part1 = new byte[0xC00];
            byte[] Part2 = new byte[0xC00];

            Buffer.BlockCopy(org, 0, Part1, 0, 0xC00);
            Buffer.BlockCopy(org, 0xc00, Part2, 0, 0xC00);

            byte[] blockLeft = new byte[16];
            byte[] blockRight = new byte[16];

            for (int i = 0; i < 96; i++)
            {
                Buffer.BlockCopy(Part1, i * 16, blockLeft, 0, 16);
                Buffer.BlockCopy(Part1, (i + 96) * 16, blockRight, 0, 16);

                if((Utilities.ByteToHexString(blockLeft)).Equals("FEFF000000000000FEFF000000000000") && (Utilities.ByteToHexString(blockRight)).Equals("FEFF000000000000FEFF000000000000"))
                {
                    for (int j = 0; j < 16; j++)
                    {
                        table[i * 16 + j] = true;
                        table[i * 16 + 96 * 16 + j] = true;
                    }
                }
                else
                {
                    for (int j = 0; j < 16; j++)
                    {
                        table[i * 16 + j] = false;
                        table[i * 16 + 96 * 16 + j] = false;
                    }
                }
            }

            for (int i = 0; i < 96; i++)
            {
                Buffer.BlockCopy(Part2, i * 16, blockLeft, 0, 16);
                Buffer.BlockCopy(Part2, (i + 96) * 16, blockRight, 0, 16);

                if ((Utilities.ByteToHexString(blockLeft)).Equals("FEFF000000000000FEFF000000000000") && (Utilities.ByteToHexString(blockRight)).Equals("FEFF000000000000FEFF000000000000"))
                {
                    for (int j = 0; j < 16; j++)
                    {
                        table[i * 16 + j + 1536 * 2] = true;
                        table[i * 16 + 96 * 16 + j + 1536 * 2] = true;
                    }
                }
                else
                {
                    for (int j = 0; j < 16; j++)
                    {
                        table[i * 16 + j + 1536 * 2] = false;
                        table[i * 16 + 96 * 16 + j + 1536 * 2] = false;
                    }
                }
            }
        }

        private void buildEmptyTable(byte[] org, ref bool[] table, int Part1x, int Part2x)
        {
            byte[] Part1 = new byte[0xC00];
            byte[] Part2 = new byte[0xC00];

            Buffer.BlockCopy(org, 0, Part1, 0, 0xC00);
            Buffer.BlockCopy(org, 0xc00, Part2, 0, 0xC00);

            byte[] blockLeft = new byte[16];
            byte[] blockRight = new byte[16];

            for (int i = 0; i < 96; i++)
            {
                Buffer.BlockCopy(Part1, i * 16, blockLeft, 0, 16);
                Buffer.BlockCopy(Part1, (i + 96) * 16, blockRight, 0, 16);

                if ((Math.Abs(anchorX - Part1x) <= 3) && (Math.Abs(anchorY - i) <= 3) && (Utilities.ByteToHexString(blockLeft)).Equals("FEFF000000000000FEFF000000000000") && (Utilities.ByteToHexString(blockRight)).Equals("FEFF000000000000FEFF000000000000"))
                {
                    for (int j = 0; j < 16; j++)
                    {
                        table[i * 16 + j] = true;
                        table[i * 16 + 96 * 16 + j] = true;
                    }
                }
                else
                {
                    for (int j = 0; j < 16; j++)
                    {
                        table[i * 16 + j] = false;
                        table[i * 16 + 96 * 16 + j] = false;
                    }
                }
            }

            for (int i = 0; i < 96; i++)
            {
                Buffer.BlockCopy(Part2, i * 16, blockLeft, 0, 16);
                Buffer.BlockCopy(Part2, (i + 96) * 16, blockRight, 0, 16);

                if ((Math.Abs(anchorX - Part2x) <= 3) && (Math.Abs(anchorY - i) <= 3) && (Utilities.ByteToHexString(blockLeft)).Equals("FEFF000000000000FEFF000000000000") && (Utilities.ByteToHexString(blockRight)).Equals("FEFF000000000000FEFF000000000000"))
                {
                    for (int j = 0; j < 16; j++)
                    {
                        table[i * 16 + j + 1536 * 2] = true;
                        table[i * 16 + 96 * 16 + j + 1536 * 2] = true;
                    }
                }
                else
                {
                    for (int j = 0; j < 16; j++)
                    {
                        table[i * 16 + j + 1536 * 2] = false;
                        table[i * 16 + 96 * 16 + j + 1536 * 2] = false;
                    }
                }
            }
        }

        private static bool Difference(byte[] org, ref byte[] upd, bool[] isEmpty, byte[] cur)
        {
            bool output = true;
            bool output2 = true;
            bool pass = true;

            for (int i = 0; i < cur.Length; i++)
            {
                if (cur[i] != org[i])
                {
                    if (isEmpty[i])
                    {
                        if (output)
                        {
                            Debug.Print("Empty Space Changed");
                            output = false;
                        }
                        upd[i] = cur[i];
                    }
                    else
                    {
                        pass = false;
                    }
                }
                else
                {
                    if (upd[i] != cur[i])
                    {
                        if (output2)
                        {
                            Debug.Print("Back to Normal");
                            output2 = false;
                        }
                        upd[i] = cur[i];
                    }
                }
            }

            return pass;
        }

        private void delay_KeyPress(object sender, KeyPressEventArgs e)
        {
                char c = e.KeyChar;
                if (!((c >= '0' && c <= '9')))
                {
                    e.Handled = true;
                }
        }

        private string getVisitorName()
        {
            byte[] b = Utilities.getVisitorName(s);
            string tempName = Encoding.Unicode.GetString(b, 0, 20);
            return tempName.Replace("\0", string.Empty);
        }

        private void PauseTimer_Tick(object sender, EventArgs e)
        {
            this.Invoke((MethodInvoker)delegate
            {
                pauseTime--;
                PauseTimeLabel.Text = pauseTime.ToString();
            });
        }

        private void logBtn_Click(object sender, EventArgs e)
        {
            if (logGridView.DataSource == null)
            {
                if (!File.Exists(logPath))
                {
                    string logheader = "Timestamp" + "," + "Name";

                    using (StreamWriter sw = File.CreateText(logPath))
                    {
                        sw.WriteLine(logheader);
                    }
                }
                logGridView.DataSource = loadCSV(logPath);
                logGridView.Columns["Timestamp"].Width = 195;
                logGridView.Columns["Name"].Width = 128;
                logGridView.Sort(logGridView.Columns[0], ListSortDirection.Descending);
                logPanel.Visible = true;
            }
            if (this.Width < 610)
            {
                this.Width = 610;
                logPanel.Visible = true;
            }
            else
            {
                this.Width = 250;
                logPanel.Visible = false;
            }
        }

        private DataTable loadCSV(string filePath)
        {
            var dt = new DataTable();

            File.ReadLines(filePath).Take(1)
                .SelectMany(x => x.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                .ToList()
                .ForEach(x => dt.Columns.Add(x.Trim()));

            dt.Columns["Timestamp"].DataType = typeof(DateTime);

            File.ReadLines(filePath).Skip(1)
                .Select(x => x.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries))
                .ToList()
                .ForEach(line => dt.Rows.Add(line));

            return dt;
        }

        private void CreateLog(string newVisitor)
        {
            if (!File.Exists(logPath))
            {
                string logheader = "Timestamp" + "," + "Name";

                using (StreamWriter sw = File.CreateText(logPath))
                {
                    sw.WriteLine(logheader);
                }
            }

            DateTime localDate = DateTime.Now;
            string newLog = localDate.ToString() + "," + newVisitor;

            using (StreamWriter sw = File.AppendText(logPath))
            {
                sw.WriteLine(newLog);
            }

            if (logGridView.DataSource != null)
            {
                logGridView.DataSource = loadCSV(logPath);
                logGridView.Sort(logGridView.Columns[0], ListSortDirection.Descending);
            }
        }

        private void newLogBtn_Click(object sender, EventArgs e)
        {
            SaveFileDialog file = new SaveFileDialog()
            {
                Filter = "Comma-Separated Values file (*.csv)|*.csv",
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
            for (int i = 0; i < temp.Length - 1; i++)
                path = path + temp[i] + "\\";

            config.AppSettings.Settings["LastSave"].Value = path;
            config.Save(ConfigurationSaveMode.Minimal);

            string logheader = "Timestamp" + "," + "Name";

            using (StreamWriter sw = File.CreateText(file.FileName))
            {
                sw.WriteLine(logheader);
            }

            string[] s = file.FileName.Split('\\');

            logName.Text = s[s.Length - 1];
            logPath = file.FileName;

            if (logGridView.DataSource != null)
            {
                logGridView.DataSource = loadCSV(logPath);
                logGridView.Sort(logGridView.Columns[0], ListSortDirection.Descending);
            }

            if (sound)
                System.Media.SystemSounds.Asterisk.Play();
        }

        private void selectLogBtn_Click(object sender, EventArgs e)
        {
            OpenFileDialog file = new OpenFileDialog()
            {
                Filter = "Comma-Separated Values file (*.csv)|*.csv",
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
            for (int i = 0; i < temp.Length - 1; i++)
                path = path + temp[i] + "\\";

            config.AppSettings.Settings["LastLoad"].Value = path;
            config.Save(ConfigurationSaveMode.Minimal);

            string[] s = file.FileName.Split('\\');

            logName.Text = s[s.Length - 1];
            logPath = file.FileName;

            if (logGridView.DataSource != null)
            {
                logGridView.DataSource = loadCSV(logPath);
                logGridView.Sort(logGridView.Columns[0], ListSortDirection.Descending);
            }
        }

        private void miniMapBox_MouseDown(object sender, MouseEventArgs e)
        {
            Debug.Print(e.X.ToString() + " " + e.Y.ToString());

            int x;
            int y;

            if (e.X / 2 < 3)
                x = 3;
            else if (e.X / 2 > 108)
                x = 108;
            else
                x = e.X / 2;

            if (e.Y / 2 < 3)
                y = 3;
            else if (e.Y / 2 > 92)
                y = 92;
            else
                y = e.Y / 2;

            anchorX = x;
            anchorY = y;

            xCoordinate.Text = x.ToString();
            yCoordinate.Text = y.ToString();

            miniMapBox.Image = MiniMap.drawSelectSquare(anchorX * 2, anchorY * 2);
        }

        private void miniMapBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                int x;
                int y;

                if (e.X / 2 < 3)
                    x = 3;
                else if (e.X / 2 > 108)
                    x = 108;
                else
                    x = e.X / 2;

                if (e.Y / 2 < 3)
                    y = 3;
                else if (e.Y / 2 > 92)
                    y = 92;
                else
                    y = e.Y / 2;

                anchorX = x;
                anchorY = y;

                xCoordinate.Text = x.ToString();
                yCoordinate.Text = y.ToString();

                miniMapBox.Image = MiniMap.drawSelectSquare(anchorX * 2, anchorY * 2);
            }
        }
    }
}
