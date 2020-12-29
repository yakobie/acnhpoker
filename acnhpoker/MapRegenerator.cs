using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        private int delayTime = 100;
        private string lastVisitor = "";

        public MapRegenerator(Socket S, Form1 Main)
        {
            s = S;
            main = Main;
            InitializeComponent();
            FinMsg.SelectionAlignment = HorizontalAlignment.Center;
            lastVisitor = getVisitorName();
            visitorNameBox.Text = lastVisitor;
        }

        private void saveMapBtn_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog file = new SaveFileDialog()
                {
                    Filter = "New Horizons Fasil (*.nhf)|*.nhf",
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

                UInt32 address = Utilities.mapHead + (0xC00 * (0x24)) + (0x10 * (0x18));

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
                    FileName = "Map.nhf",
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

                UInt32 address = (UInt32)(Utilities.mapHead + (0xC00 * (0x24)) + (0x10 * (0x18)));

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
            }

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
                OpenFileDialog file = new OpenFileDialog()
                {
                    Filter = "New Horizons Fasil (*.nhf)|*.nhf|All files (*.*)|*.*",
                    FileName = "Map.nhf",
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

                loop = true;
                startRegen.Tag = "Stop";
                startRegen.BackColor = Color.Orange;
                startRegen.Text = "Stop Regen";
                saveMapBtn.Enabled = false;
                loadMapBtn.Enabled = false;
                backBtn.Enabled = false;
                startRegen2.Enabled = false;


                byte[] data = File.ReadAllBytes(file.FileName);

                UInt32 address = (UInt32)(Utilities.mapHead + (0xC00 * (0x24)) + (0x10 * (0x18)));

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

            do
            {
                counter = 0;

                newVisitor = getVisitorName();

                if (!lastVisitor.Equals(newVisitor))
                {
                    lastVisitor = newVisitor;
                    this.Invoke((MethodInvoker)delegate
                    {
                        visitorNameBox.Text = newVisitor;
                        WaitMessagebox.Text = "Paused. " + newVisitor + " arriving!";
                    });
                    Thread.Sleep(70000);
                    this.Invoke((MethodInvoker)delegate
                    {
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
                //System.Media.SystemSounds.Asterisk.Play();
                this.Invoke((MethodInvoker)delegate
                {
                    ts = stopWatch.Elapsed;
                    timeLabel.Text = Utilities.precedingZeros(ts.Hours.ToString(), 2) + ":" + Utilities.precedingZeros(ts.Minutes.ToString(), 2) + ":" + Utilities.precedingZeros(ts.Seconds.ToString(), 2);
                });
            } while (loop);

            stopWatch.Stop();

            hideMapWait();

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
                    FileName = "Map.nhf",
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

                loop = true;
                startRegen2.Tag = "Stop";
                startRegen2.BackColor = Color.Orange;
                startRegen2.Text = "Stop Moogle Regenja";
                saveMapBtn.Enabled = false;
                loadMapBtn.Enabled = false;
                backBtn.Enabled = false;
                startRegen.Enabled = false;

                byte[] data = File.ReadAllBytes(file.FileName);

                UInt32 address = (UInt32)(Utilities.mapHead + (0xC00 * (0x24)) + (0x10 * (0x18)));

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

            int writeCount = 0;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            string newVisitor;
            TimeSpan ts;

            do
            {
                counter = 0;

                newVisitor = getVisitorName();

                if (!lastVisitor.Equals(newVisitor))
                {
                    lastVisitor = newVisitor;
                    this.Invoke((MethodInvoker)delegate
                    {
                        visitorNameBox.Text = newVisitor;
                        WaitMessagebox.Text = "Paused. " + newVisitor + " arriving!";
                    });
                    Thread.Sleep(70000);
                    this.Invoke((MethodInvoker)delegate
                    {
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
                    //Debug.Print("---------- " + i);
                    if (!loop)
                        break;
                }
                //System.Media.SystemSounds.Asterisk.Play();
                this.Invoke((MethodInvoker)delegate
                {
                    ts = stopWatch.Elapsed;
                    timeLabel.Text = Utilities.precedingZeros(ts.Hours.ToString(),2) + ":" + Utilities.precedingZeros(ts.Minutes.ToString(), 2) + ":" + Utilities.precedingZeros(ts.Seconds.ToString(), 2);
                });
            } while (loop);

            stopWatch.Stop();

            hideMapWait();

            System.Media.SystemSounds.Asterisk.Play();
        }

        private static void buildEmptyTable(byte[] org, ref bool[] table)
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
        private static bool Difference(byte[] org, ref byte[] upd, bool[] isEmpty, byte[] cur)
        {
            bool pass = true;

            for (int i = 0; i < cur.Length; i++)
            {
                if (cur[i] != org[i])
                {
                    if (isEmpty[i])
                    {
                        //Debug.Print("Empty Space Changed");
                        upd[i] = cur[i];
                    }
                    else
                    {
                        pass = false;
                    }
                }
                else
                {

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
    }
}
