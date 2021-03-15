using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ACNHPoker
{
    public partial class dodo : Form
    {
        private static Socket s;
        private Form mainForm;
        public Boolean dodoSetupDone = false;
        private Boolean idleEmote = false;
        private int idleNum = 0;
        private Boolean HoldingL = false;
        private PubSub MyPubSub;
        private TwitchBot MyTwitchBot;
        private static List<DropOrder> DropOrderList = new List<DropOrder>();
        private string TwitchBotUserName;
        private string TwitchBotOauth;
        private string TwitchChannelName;
        private string TwitchChannelAccessToken;
        private string TwitchChannelid;
        private bool wasLoading = false;
        private bool lastOrderIsRecipe = false;
        private static OrderDisplay itemDisplay;

        bool restoreDodo = false;
        bool dropItem = false;

        Thread standaloneThread;
        private bool standaloneRunning = false;

        public dodo(Socket S, Form main, bool standaloneMode = false)
        {
            InitializeComponent();
            mainForm = main;
            s = S;

            if (teleport.allAnchorValid())
            {
                Point Done = new Point(-4200, 0);
                FullPanel.Location = Done;
                dodoSetupDone = true;
            }

            if (File.Exists(Utilities.TwitchSettingPath))
            {
                TwitchBtn.Visible = true;
                itemDisplayBtn.Visible = true;
                dropItemBox.Enabled = true;
            }

            if (standaloneMode)
            {
                dropItemBox.Visible = true;
                restoreDodobox.Visible = true;
                standaloneStart.Visible = true;
            }
        }

        #region Teleport Setup

        private void StartNextBtn_Click(object sender, EventArgs e)
        {
            resetBtn();
            Point page1 = new Point(-600, 0);
            FullPanel.Location = page1;
        }

        private void Anchor0NextBtn_Click(object sender, EventArgs e)
        {
            resetBtn();
            Point page2 = new Point(-1200, 0);
            FullPanel.Location = page2;
        }

        private void Anchor1NextBtn_Click(object sender, EventArgs e)
        {
            resetBtn();
            Point page3 = new Point(-1800, 0);
            FullPanel.Location = page3;
        }

        private void Anchor2NextBtn_Click(object sender, EventArgs e)
        {
            resetBtn();
            Point page4 = new Point(-2400, 0);
            FullPanel.Location = page4;
        }

        private void Anchor3NextBtn_Click(object sender, EventArgs e)
        {
            resetBtn();
            Point page5 = new Point(-3000, 0);
            FullPanel.Location = page5;
        }

        private void Anchor4NextBtn_Click(object sender, EventArgs e)
        {
            resetBtn();
            Point page6 = new Point(-3600, 0);
            FullPanel.Location = page6;
        }

        private void DoneBtn_Click(object sender, EventArgs e)
        {
            if (teleport.allAnchorValid())
            {
                resetBtn();
                Point Done = new Point(-4200, 0);
                FullPanel.Location = Done;
                dodoSetupDone = true;
            }
            else
            {
                myMessageBox.Show("One or more anchors have not been setup correctly.", "Skip leg day?", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Anchor0PreviousBtn_Click(object sender, EventArgs e)
        {
            resetBtn();
            Point Start = new Point(0, 0);
            FullPanel.Location = Start;
        }

        private void Anchor1PreviousBtn_Click(object sender, EventArgs e)
        {
            resetBtn();
            Point page1 = new Point(-600, 0);
            FullPanel.Location = page1;
        }

        private void Anchor2PreviousBtn_Click(object sender, EventArgs e)
        {
            resetBtn();
            Point page2 = new Point(-1200, 0);
            FullPanel.Location = page2;
        }

        private void Anchor3PreviousBtn_Click(object sender, EventArgs e)
        {
            resetBtn();
            Point page3 = new Point(-1800, 0);
            FullPanel.Location = page3;
        }

        private void Anchor4PreviousBtn_Click(object sender, EventArgs e)
        {
            resetBtn();
            Point page4 = new Point(-2400, 0);
            FullPanel.Location = page4;
        }

        private void DonePreviousBtn_Click(object sender, EventArgs e)
        {
            resetBtn();
            Point page5 = new Point(-3000, 0);
            FullPanel.Location = page5;
        }

        private void Anchor0SetBtn_Click(object sender, EventArgs e)
        {
            teleport.SetAnchor(0);
            Anchor0SetBtn.ForeColor = Color.Green;
            Anchor0Line3.Visible = true;
            Anchor0TestBtn.Visible = true;
            Anchor0TestBtn.Enabled = true;
            Anchor0TestBtn.Text = "Test";
            Anchor0TestBtn.ForeColor = Color.White;
        }

        private void Anchor0TestBtn_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.Enabled = false;

            Thread TeleportThread = new Thread(delegate () { TestTeleport(0, btn); });
            TeleportThread.Start();
        }

        private void Anchor1SetBtn_Click(object sender, EventArgs e)
        {
            teleport.SetAnchor(1);
            Anchor1SetBtn.ForeColor = Color.Green;
            Anchor1Line3.Visible = true;
            Anchor1TestBtn.Visible = true;
            Anchor1TestBtn.Enabled = true;
            Anchor1TestBtn.Text = "Test";
            Anchor1TestBtn.ForeColor = Color.White;
        }

        private void Anchor1TestBtn_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.Enabled = false;

            Thread TeleportThread = new Thread(delegate () { TestTeleport(1, btn); });
            TeleportThread.Start();
        }

        private void Anchor2SetBtn_Click(object sender, EventArgs e)
        {
            teleport.SetAnchor(2);
            Anchor2SetBtn.ForeColor = Color.Green;
            Anchor2Line3.Visible = true;
            Anchor2TestBtn.Visible = true;
            Anchor2TestBtn.Enabled = true;
            Anchor2TestBtn.Text = "Test";
            Anchor2TestBtn.ForeColor = Color.White;
        }

        private void Anchor2TestBtn_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.Enabled = false;

            Thread TeleportThread = new Thread(delegate () { TestTeleport(2, btn); });
            TeleportThread.Start();
        }

        private void Anchor3SetBtn_Click(object sender, EventArgs e)
        {
            teleport.SetAnchor(3);
            Anchor3SetBtn.ForeColor = Color.Green;
            Anchor3Line3.Visible = true;
            Anchor3TestBtn.Visible = true;
            Anchor3TestBtn.Enabled = true;
            Anchor3TestBtn.Text = "Test";
            Anchor3TestBtn.ForeColor = Color.White;
        }

        private void Anchor3TestBtn_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.Enabled = false;

            Thread TeleportThread = new Thread(delegate () { TestTeleport(3, btn); });
            TeleportThread.Start();
        }

        private void Anchor4SetBtn_Click(object sender, EventArgs e)
        {
            teleport.SetAnchor(4);
            Anchor4SetBtn.ForeColor = Color.Green;
            Anchor4Line3.Visible = true;
            Anchor4TestBtn.Visible = true;
            Anchor4TestBtn.Enabled = true;
            Anchor4TestBtn.Text = "Test";
            Anchor4TestBtn.ForeColor = Color.White;
        }

        private void Anchor4TestBtn_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.Enabled = false;

            Thread TeleportThread = new Thread(delegate () { TestTeleport(4, btn); });
            TeleportThread.Start();
        }

        private void DoneAnchor0TestBtn_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.Enabled = false;

            Thread TeleportThread = new Thread(delegate () { TestTeleport(0, btn); });
            TeleportThread.Start();
        }

        private void DoneAnchor1TestBtn_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.Enabled = false;

            Thread TeleportThread = new Thread(delegate () { TestTeleport(1, btn); });
            TeleportThread.Start();
        }

        private void DoneAnchor2TestBtn_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.Enabled = false;

            Thread TeleportThread = new Thread(delegate () { TestTeleport(2, btn); });
            TeleportThread.Start();
        }

        private void DoneAnchor3TestBtn_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.Enabled = false;

            Thread TeleportThread = new Thread(delegate () { TestTeleport(3, btn); });
            TeleportThread.Start();
        }

        private void DoneAnchor4TestBtn_Click(object sender, EventArgs e)
        {
            var btn = (Button)sender;
            btn.Enabled = false;

            Thread TeleportThread = new Thread(delegate () { TestTeleport(4, btn); });
            TeleportThread.Start();
        }

        private void TestTeleport(int num, Button btn)
        {
            if (teleport.TeleportToAnchor(num))
            {
                btn.Invoke((MethodInvoker)delegate
                {
                    btn.Enabled = true;
                    btn.Text = "Success";
                    btn.ForeColor = Color.Green;
                });
            }
            else
            {
                btn.Invoke((MethodInvoker)delegate
                {
                    btn.Enabled = true;
                    btn.Text = "Failed";
                    btn.ForeColor = Color.Red;
                });
            }
        }

        private void resetBtn()
        {
            Anchor0SetBtn.ForeColor = Color.White;
            Anchor0TestBtn.Enabled = true;
            Anchor0TestBtn.Text = "Test";
            Anchor0TestBtn.ForeColor = Color.White;
            Anchor1SetBtn.ForeColor = Color.White;
            Anchor1TestBtn.Enabled = true;
            Anchor1TestBtn.Text = "Test";
            Anchor1TestBtn.ForeColor = Color.White;
            Anchor2SetBtn.ForeColor = Color.White;
            Anchor2TestBtn.Enabled = true;
            Anchor2TestBtn.Text = "Test";
            Anchor2TestBtn.ForeColor = Color.White;
            Anchor3SetBtn.ForeColor = Color.White;
            Anchor3TestBtn.Enabled = true;
            Anchor3TestBtn.Text = "Test";
            Anchor3TestBtn.ForeColor = Color.White;
            Anchor4SetBtn.ForeColor = Color.White;
            Anchor4TestBtn.Enabled = true;
            Anchor4TestBtn.Text = "Test";
            Anchor4TestBtn.ForeColor = Color.White;
            DoneAnchor0TestBtn.Text = "Test";
            DoneAnchor0TestBtn.ForeColor = Color.White;
            DoneAnchor1TestBtn.Text = "Test";
            DoneAnchor1TestBtn.ForeColor = Color.White;
            DoneAnchor2TestBtn.Text = "Test";
            DoneAnchor2TestBtn.ForeColor = Color.White;
            DoneAnchor3TestBtn.Text = "Test";
            DoneAnchor3TestBtn.ForeColor = Color.White;
            DoneAnchor4TestBtn.Text = "Test";
            DoneAnchor4TestBtn.ForeColor = Color.White;
        }

        private void DoneFullTestBtn_Click(object sender, EventArgs e)
        {
            DoneAnchor0TestBtn.Enabled = false;
            DoneAnchor1TestBtn.Enabled = false;
            DoneAnchor2TestBtn.Enabled = false;
            DoneAnchor3TestBtn.Enabled = false;
            DoneAnchor4TestBtn.Enabled = false;
            DoneFullTestBtn.Enabled = false;

            Thread TeleportThread = new Thread(delegate () { TestNormalRestore(); });
            TeleportThread.Start();
        }

        private void TestNormalRestore()
        {
            controller.clickDown(); // Hide Weapon
            Thread.Sleep(1000);

            teleport.TeleportToAnchor(2);

            Debug.Print("Teleport to Airport");

            do
            {
                //Debug.Print(teleport.GetOverworldState().ToString());
                Debug.Print("Try Enter Airport");
                controller.EnterAirport();
                Thread.Sleep(2000);
            }
            while (teleport.GetOverworldState() != teleport.OverworldState.OverworldOrInAirport);

            Debug.Print("Inside Airport");
            Thread.Sleep(2000);

            teleport.TeleportToAnchor(3);

            Debug.Print("Get Dodo");
            controller.talkAndGetDodoCode();
            Debug.Print("Finish getting Dodo");

            teleport.TeleportToAnchor(4);

            do
            {
                //Debug.Print(teleport.GetOverworldState().ToString());
                Debug.Print("Try Exit Airport");
                controller.ExitAirport();
                Thread.Sleep(2000);
            }
            while (teleport.GetOverworldState() != teleport.OverworldState.OverworldOrInAirport);

            Debug.Print("Back to Overworld");
            Thread.Sleep(2000);

            teleport.TeleportToAnchor(1);

            controller.emote(0);

            controller.detachController();

            this.Invoke((MethodInvoker)delegate
            {
                DoneFullTestBtn.Text = "Done";
                DoneFullTestBtn.ForeColor = Color.Green;
                DoneAnchor0TestBtn.Enabled = true;
                DoneAnchor1TestBtn.Enabled = true;
                DoneAnchor2TestBtn.Enabled = true;
                DoneAnchor3TestBtn.Enabled = true;
                DoneAnchor4TestBtn.Enabled = true;
                DoneFullTestBtn.Enabled = true;
            });
        }

        #endregion

        private void dodo_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (MyPubSub != null)
            {
                MyPubSub.Dispose();
                MyPubSub = null;
            }
            if (MyTwitchBot != null)
            {
                MyTwitchBot = null;
            }
            if (itemDisplay != null)
            {
                itemDisplay.Close();
                itemDisplay = null;
            }
            if (mainForm is Form1)
            {
                Form1 owner = (Form1)mainForm;
                owner.dodoSetup = null;
            }
        }

        private void BackToSetupBtn_Click(object sender, EventArgs e)
        {
            resetBtn();
            Point Start = new Point(0, 0);
            FullPanel.Location = Start;
        }

        public int CheckOnlineStatus()
        {
            if (teleport.checkOnlineStatus() == 0x1)
            {
                onlineLabel.Invoke((MethodInvoker)delegate
                {
                    onlineLabel.Text = "ONLINE";
                    onlineLabel.ForeColor = Color.Green;
                });
                return 1;
            }
            else
            {
                onlineLabel.Invoke((MethodInvoker)delegate
                {
                    onlineLabel.Text = "OFFLINE";
                    onlineLabel.ForeColor = Color.Red;
                });
                return 0;
            }
        }

        public void DisplayDodo(string dodo)
        {
            dodoCode.Invoke((MethodInvoker)delegate
            {
                dodoCode.Text = dodo;
            });
        }

        public void WriteLog(string line, Boolean time = false)
        {
            dodoLog.Invoke((MethodInvoker)delegate
            {
                if (time)
                {
                    DateTime localDate = DateTime.Now;
                    dodoLog.AppendText(localDate.ToString() + " : " + line + "\n");
                }
                else
                    dodoLog.AppendText(line + "\n");
            });
        }

        public void LockBtn()
        {
            BackToSetupBtn.Invoke((MethodInvoker)delegate
            {
                BackToSetupBtn.Enabled = false;
            });
        }

        private void LockControl()
        {
            this.Invoke((MethodInvoker)delegate
            {
                controllerPanel.Enabled = false;
                functionPanel.Enabled = false;
                AbortBtn.Visible = true;
            });
        }

        private void unLockControl()
        {
            this.Invoke((MethodInvoker)delegate
            {
                controllerPanel.Enabled = true;
                functionPanel.Enabled = true;
                AbortBtn.Visible = false;
            });
        }

        public teleport.OverworldState DodoMonitor()
        {
            if (CheckOnlineStatus() == 1)
            {
                teleport.OverworldState state = teleport.GetOverworldState();
                WriteLog(state.ToString() + " " + idleNum, true);

                if (state == teleport.OverworldState.Loading || state == teleport.OverworldState.UserArriveLeavingOrTitleScreen)
                {
                    idleNum = 0;
                    wasLoading = true;
                }
                else
                {
                    idleNum++;
                    if (idleNum >= 2)
                    {
                        if (wasLoading)
                        {
                            if (Utilities.hasItemInFirstSlot(s))
                            {
                                if (lastOrderIsRecipe)
                                    controller.dropRecipe();
                                else
                                    controller.dropItem();
                            }

                            wasLoading = false;
                        }

                        if (DropOrderList.Count <= 0)
                        {
                            Debug.Print("No item needed to drop");
                        }
                        else
                        {
                            _ = DropItem(DropOrderList.ElementAt(0));
                            if (DropOrderList.Count > 0)
                                state = teleport.OverworldState.ItemDropping;
                        }
                    }

                    if (idleEmote && state == teleport.OverworldState.OverworldOrInAirport)
                    {
                        if (idleNum >= 5 && idleNum % 5 == 0)
                        {
                            Random random = new Random();
                            int v = random.Next(0, 10);
                            controller.emote(v);
                        }
                    }
                }
                return state;
            }
            else
            {
                WriteLog("[Warning] Disconnected.", true);
                WriteLog("Please wait a moment for the restore.", true);
                LockControl();

                int retry = 0;
                do
                {
                    if (retry >= 30)
                    {
                        WriteLog("[Warning] Start Hard Restore", true);
                        HardRestore();
                        break;
                    }
                    if (teleport.GetLocationState() == teleport.LocationState.Announcement)
                    {
                        WriteLog("In Announcement", true);
                        if (!HoldingL)
                        {
                            controller.pressL();
                            HoldingL = true;
                        }
                        retry = 0;
                    }
                    else
                    {
                        WriteLog("Waiting for Overworld", true);
                    }
                    controller.clickA();
                    Thread.Sleep(3000);
                    retry++;
                }
                while (teleport.GetOverworldState() != teleport.OverworldState.OverworldOrInAirport);

                //Thread.Sleep(2000);
                controller.releaseL();
                HoldingL = false;
                WriteLog("[Warning] Start Normal Restore", true);
                WriteLog("Please wait for the bot to finish the sequence.", true);
                NormalRestore();
                unLockControl();
                WriteLog("Restore sequence finished.", true);
                return teleport.OverworldState.OverworldOrInAirport;
            }
        }

        public void NormalRestore()
        {
            do
            {
                if (teleport.GetLocationState() == teleport.LocationState.Announcement)
                {
                    WriteLog("In Announcement", true);
                    if (!HoldingL)
                    {
                        controller.pressL();
                        HoldingL = true;
                    }
                }
                else
                {
                    WriteLog("Confirming Overworld", true);
                }
                controller.clickA();
                Thread.Sleep(3000);
            }
            while (teleport.GetOverworldState() != teleport.OverworldState.OverworldOrInAirport);

            controller.releaseL();
            HoldingL = false;
            Thread.Sleep(2000);
            controller.clickDown(); // Hide Weapon
            Thread.Sleep(1000);

            teleport.TeleportToAnchor(2);

            WriteLog("Teleport to Airport", true);

            do
            {
                //Debug.Print(teleport.GetOverworldState().ToString());
                WriteLog("Try Entering Airport", true);
                controller.EnterAirport();
                Thread.Sleep(2000);
            }
            while (teleport.GetOverworldState() != teleport.OverworldState.OverworldOrInAirport);

            WriteLog("Inside Airport", true);
            Thread.Sleep(2000);

            teleport.TeleportToAnchor(3);

            WriteLog("Try Getting Dodo", true);
            if (skipDialogCheckBox.Checked)
                DisplayDodo(controller.talkAndGetDodoCode());
            else
                DisplayDodo(controller.talkAndGetDodoCodeLegacy());
            WriteLog("Finish Getting Dodo", true);
            CheckOnlineStatus();

            teleport.TeleportToAnchor(4);

            do
            {
                //Debug.Print(teleport.GetOverworldState().ToString());
                WriteLog("Try Exiting Airport", true);
                controller.ExitAirport();
                Thread.Sleep(2000);
            }
            while (teleport.GetOverworldState() != teleport.OverworldState.OverworldOrInAirport);

            WriteLog("Back to Overworld", true);
            Thread.Sleep(2000);

            teleport.TeleportToAnchor(1);

            controller.emote(0);

            //controller.detachController();
        }

        public void HardRestore()
        {
            WriteLog("Capturing the Crash", true);
            controller.clickCAPTURE();
            Thread.Sleep(2000);
            WriteLog("Open Home Menu", true);
            controller.clickHOME();
            Thread.Sleep(5000);

            controller.clickX();
            Thread.Sleep(1000);
            controller.clickA(); //Close Game
            WriteLog("Try Closing the Game", true);
            Thread.Sleep(15000);

            controller.clickA(); //Select Game
            Thread.Sleep(2000);
            controller.clickA(); //Select first user
            WriteLog("Game & User Selected", true);
            Thread.Sleep(10000);

            int retry = 0;
            do
            {
                //Debug.Print(teleport.GetOverworldState().ToString());
                //Debug.Print("Waiting for Overworld");
                if (teleport.GetLocationState() == teleport.LocationState.Announcement)
                {
                    WriteLog("In Announcement", true);
                    if (!HoldingL)
                    {
                        controller.pressL();
                        HoldingL = true;
                    }
                }
                controller.clickA();
                Thread.Sleep(2000);
                retry++;
            }
            while (teleport.GetOverworldState() != teleport.OverworldState.OverworldOrInAirport);

            controller.releaseL();
            HoldingL = false;
            WriteLog("Exiting House", true);
            Thread.Sleep(2000);
        }

        private void CloseGate()
        {
            controller.clickDown(); // Hide Weapon
            Thread.Sleep(1000);

            teleport.TeleportToAnchor(2);

            Debug.Print("Teleport to Airport");

            do
            {
                //Debug.Print(teleport.GetOverworldState().ToString());
                Debug.Print("Try Enter Airport");
                controller.EnterAirport();
                Thread.Sleep(2000);
            }
            while (teleport.GetOverworldState() != teleport.OverworldState.OverworldOrInAirport);

            Debug.Print("Inside Airport");
            Thread.Sleep(2000);

            teleport.TeleportToAnchor(3);

            Debug.Print("Close Gate");
            controller.talkAndCloseGate();
            Debug.Print("Finish Close Gate");
            CheckOnlineStatus();

            teleport.TeleportToAnchor(4);

            do
            {
                //Debug.Print(teleport.GetOverworldState().ToString());
                Debug.Print("Try Exit Airport");
                controller.ExitAirport();
                Thread.Sleep(2000);
            }
            while (teleport.GetOverworldState() != teleport.OverworldState.OverworldOrInAirport);

            Debug.Print("Back to Overworld");
            Thread.Sleep(2000);

        }

        public static void AddItem(string Owner, string Id, string Count, string Name, string Color, Image Image = null)
        {
            DropOrderList.Add(new DropOrder() { owner = Owner, id = Id, count = Count, name = Name, color = Color, image = Image });

            if (itemDisplay != null && Image != null)
                itemDisplay.setItemdisplay(Image);

            Debug.Print($"{Owner} - Item Added : {Name} ({Color})  [{Id}] [{Count}]");
        }

        private async Task DropItem(DropOrder CurrentOrder)
        {
            string flag1;
            if (CurrentOrder.id == "16A2" || CurrentOrder.name.Contains("wrapping paper"))
                flag1 = "00";
            else
                flag1 = "7F";

            string flag2 = "00";

            Utilities.SpawnItem(s, null, 0, flag1 + flag2 + CurrentOrder.id, Utilities.precedingZeros(CurrentOrder.count, 8));
            //Thread.Sleep(500);
            if (CurrentOrder.id == "16A2")
            {
                controller.dropRecipe();
                lastOrderIsRecipe = true;
            }
            else
            {
                controller.dropItem();
                lastOrderIsRecipe = false;
            }
            if (!CurrentOrder.color.Equals(string.Empty))
                await MyTwitchBot.SendMessage($"{CurrentOrder.owner}, your order of \"{CurrentOrder.name}\" ({CurrentOrder.color}) have been dropped.");
            else
                await MyTwitchBot.SendMessage($"{CurrentOrder.owner}, your order of \"{CurrentOrder.name}\" have been dropped.");

            //await MyTwitchBot.SendMessage($"If you can't find your order, people flying in/out might have canceled it. We are very sorry. Feel free to place your order again.");

            DropOrderList.RemoveAt(0);
        }

        private void dodoLog_TextChanged(object sender, EventArgs e)
        {
            dodoLog.SelectionStart = dodoLog.Text.Length;
            dodoLog.ScrollToCaret();
        }

        #region Virtual Controller

        private void LstickMouseUp(object sender, MouseEventArgs e)
        {
            controller.resetLeftStick();
        }

        private void LstickUPBtn_MouseDown(object sender, MouseEventArgs e)
        {
            controller.LstickUp();
        }

        private void LstickRIGHTBtn_MouseDown(object sender, MouseEventArgs e)
        {
            controller.LstickRight();
        }

        private void LstickDOWNBtn_MouseDown(object sender, MouseEventArgs e)
        {
            controller.LstickDown();
        }

        private void LstickLEFTBtn_MouseDown(object sender, MouseEventArgs e)
        {
            controller.LstickLeft();
        }

        private void XBtn_Click(object sender, EventArgs e)
        {
            controller.clickX();
        }

        private void ABtn_Click(object sender, EventArgs e)
        {
            controller.clickA();
        }

        private void BBtn_Click(object sender, EventArgs e)
        {
            controller.clickB();
        }

        private void YBtn_Click(object sender, EventArgs e)
        {
            controller.clickY();
        }

        private void emoteUPBtn_Click(object sender, EventArgs e)
        {
            controller.emote(0);
        }

        private void emoteRIGHTBtn_Click(object sender, EventArgs e)
        {
            controller.emote(2);
        }

        private void emoteDOWNBtn_Click(object sender, EventArgs e)
        {
            controller.emote(4);
        }

        private void emoteLEFTBtn_Click(object sender, EventArgs e)
        {
            controller.emote(6);
        }

        private void emoteTopRightBtn_Click(object sender, EventArgs e)
        {
            controller.emote(1);
        }

        private void emoteBottomRightBtn_Click(object sender, EventArgs e)
        {
            controller.emote(3);
        }

        private void emoteTopLeftBtn_Click(object sender, EventArgs e)
        {
            controller.emote(7);
        }

        private void emoteBottomLeftBtn_Click(object sender, EventArgs e)
        {
            controller.emote(5);
        }

        private void LeftStickBtn_Click(object sender, EventArgs e)
        {
            controller.clickLeftStick();
        }

        private void RightStickBtn_Click(object sender, EventArgs e)
        {
            controller.clickRightStick();
        }

        private void DupBtn_Click(object sender, EventArgs e)
        {
            controller.clickUp();
        }

        private void DrightBtn_Click(object sender, EventArgs e)
        {
            controller.clickRight();
        }

        private void DdownBtn_Click(object sender, EventArgs e)
        {
            controller.clickDown();
        }

        private void DleftBtn_Click(object sender, EventArgs e)
        {
            controller.clickLeft();
        }

        private void LBtn_Click(object sender, EventArgs e)
        {
            controller.clickL();
        }

        private void RBtn_Click(object sender, EventArgs e)
        {
            controller.clickR();
        }

        private void ZLBtn_Click(object sender, EventArgs e)
        {
            controller.clickZL();
        }

        private void ZRBtn_Click(object sender, EventArgs e)
        {
            controller.clickZR();
        }

        private void minusBtn_Click(object sender, EventArgs e)
        {
            controller.clickMINUS();
        }

        private void plusBtn_Click(object sender, EventArgs e)
        {
            controller.clickPLUS();
        }

        private void caputureBtn_Click(object sender, EventArgs e)
        {
            controller.clickCAPTURE();
        }

        private void HomeBtn_Click(object sender, EventArgs e)
        {
            controller.clickHOME();
        }

        private void RstickMouseUp(object sender, MouseEventArgs e)
        {
            controller.resetRightStick();
        }

        private void RstickUPBtn_MouseDown(object sender, MouseEventArgs e)
        {
            controller.RstickUp();
        }

        private void RstickRIGHTBtn_MouseDown(object sender, MouseEventArgs e)
        {
            controller.RstickRight();
        }

        private void RstickDOWNBtn_MouseDown(object sender, MouseEventArgs e)
        {
            controller.RstickDown();
        }

        private void RstickLEFTBtn_MouseDown(object sender, MouseEventArgs e)
        {
            controller.RstickLeft();
        }

        private void DetachBtn_Click(object sender, EventArgs e)
        {
            controller.detachController();
        }

        private void DoneAnchor0Btn_Click(object sender, EventArgs e)
        {
            teleport.TeleportToAnchor(0);
        }

        private void DoneAnchor1Btn_Click(object sender, EventArgs e)
        {
            teleport.TeleportToAnchor(1);
        }

        private void DoneAnchor2Btn_Click(object sender, EventArgs e)
        {
            teleport.TeleportToAnchor(2);
        }

        #endregion

        private void idleEmoteCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (idleEmoteCheckBox.Checked)
                idleEmote = true;
            else
                idleEmote = false;
        }

        private void AbortBtn_Click(object sender, EventArgs e)
        {
            if (mainForm is MapRegenerator)
            {
                MapRegenerator owner = (MapRegenerator)mainForm;
                owner.abort();
            }
            if (standaloneThread != null)
            {
                standaloneThread.Abort();

                standaloneStart.Text = "Start";
                standaloneStart.Tag = "Start";
                standaloneRunning = false;
                standaloneThread = null;
            }
            WriteLog(">> Restore Sequence Aborted! <<");
            unLockControl();
            dodoCode.Text = "";
            onlineLabel.Text = "";
            idleNum = 0;
            wasLoading = false;
            lastOrderIsRecipe = false;
        }

        private void TwitchBtn_Click(object sender, EventArgs e)
        {
            TwitchBotUserName = Utilities.GetJsonSetting(Utilities.TwitchSettingPath, "TwitchBotUserName");
            TwitchBotOauth = Utilities.GetJsonSetting(Utilities.TwitchSettingPath, "TwitchBotOauth");
            TwitchChannelName = Utilities.GetJsonSetting(Utilities.TwitchSettingPath, "TwitchChannelName");
            TwitchChannelAccessToken = Utilities.GetJsonSetting(Utilities.TwitchSettingPath, "TwitchChannelAccessToken");

            if (TwitchBotUserName.Equals(string.Empty) || TwitchBotOauth.Equals(string.Empty) || TwitchChannelName.Equals(string.Empty) || TwitchChannelAccessToken.Equals(string.Empty))
            {
                myMessageBox.Show("Invalid Twitch setting!", "Error Code: 2124-4007", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            TwitchChannelid = Utilities.getChannelId(TwitchChannelName);

            if (TwitchChannelid.Equals(string.Empty))
            {
                myMessageBox.Show("Unable to retrieve your channel ID!", "Error Code: 2181-4008", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            TwitchBtn.Enabled = false;
            itemDisplayBtn.Enabled = true;
            WriteLog("--------------------------------------------------------------------------------------------");
            MyTwitchBot = new TwitchBot(TwitchBotUserName, TwitchBotOauth, TwitchChannelName);
            WriteLog($"Your chat bot name is {TwitchBotUserName}", true);
            WriteLog($"Your channel name is {TwitchChannelName}", true);
            WriteLog($"Check your Twitch chat for the start up message!", true);

            MyPubSub = new PubSub(MyTwitchBot, TwitchChannelid, TwitchChannelAccessToken, this);
            WriteLog($"Your channel ID is {TwitchChannelid}", true);
            WriteLog("--------------------------------------------------------------------------------------------");
            WriteLog($"Create/Edit a \"Custom Rewards\" on Twitch to get the ID!", true);
        }

        private void itemDisplayBtn_Click(object sender, EventArgs e)
        {
            if (itemDisplay == null)
            {
                itemDisplay = new OrderDisplay();
                itemDisplay.Show();
            }
            else
            {
                itemDisplay.Close();
                itemDisplay = null;
            }
        }

        private void standaloneStart_Click(object sender, EventArgs e)
        {
            if (standaloneStart.Tag.ToString().Equals("Start"))
            {
                standaloneStart.Text = "Stop";
                standaloneStart.Tag = "Stop";
                standaloneRunning = true;
                standaloneThread = new Thread(delegate () { standaloneLoop(); });
                standaloneThread.Start();
            }
            else
            {
                standaloneStart.Text = "Start";
                standaloneStart.Tag = "Start";
                standaloneRunning = false;
                standaloneThread = null;
            }
        }

        private void standaloneLoop()
        {
            do
            {
                this.Invoke((MethodInvoker)delegate
                {
                    if (restoreDodobox.Checked)
                        restoreDodo = true;
                    else
                        restoreDodo = false;

                    if (dropItemBox.Checked)
                        dropItem = true;
                    else
                        dropItem = false;
                });

                if (CheckOnlineStatus() == 1 || !restoreDodo)
                {
                    teleport.OverworldState state = teleport.GetOverworldState();
                    WriteLog(state.ToString() + " " + idleNum, true);

                    if (state == teleport.OverworldState.Loading || state == teleport.OverworldState.UserArriveLeavingOrTitleScreen)
                    {
                        idleNum = 0;
                        wasLoading = true;
                    }
                    else
                    {
                        idleNum++;
                        if (idleNum >= 2)
                        {
                            if (dropItem)
                            {
                                if (wasLoading)
                                {
                                    if (Utilities.hasItemInFirstSlot(s))
                                    {
                                        if (lastOrderIsRecipe)
                                            controller.dropRecipe();
                                        else
                                            controller.dropItem();
                                    }

                                    wasLoading = false;
                                }

                                if (DropOrderList.Count <= 0)
                                {
                                    Debug.Print("No item needed to drop");
                                }
                                else
                                {
                                    _ = DropItem(DropOrderList.ElementAt(0));
                                    if (DropOrderList.Count > 0)
                                        state = teleport.OverworldState.ItemDropping;
                                }
                            }
                        }

                        if (idleEmote && state == teleport.OverworldState.OverworldOrInAirport)
                        {
                            if (idleNum >= 10 && idleNum % 10 == 0)
                            {
                                Random random = new Random();
                                int v = random.Next(0, 10);
                                controller.emote(v);
                            }
                        }
                    }
                }
                else
                {
                    if (restoreDodo)
                    {
                        WriteLog("[Warning] Disconnected.", true);
                        WriteLog("Please wait a moment for the restore.", true);
                        LockControl();

                        int retry = 0;
                        do
                        {
                            if (retry >= 30)
                            {
                                WriteLog("[Warning] Start Hard Restore", true);
                                HardRestore();
                                break;
                            }
                            if (teleport.GetLocationState() == teleport.LocationState.Announcement)
                            {
                                WriteLog("In Announcement", true);
                                if (!HoldingL)
                                {
                                    controller.pressL();
                                    HoldingL = true;
                                }
                                retry = 0;
                            }
                            else
                            {
                                WriteLog("Waiting for Overworld", true);
                            }
                            controller.clickA();
                            Thread.Sleep(3000);
                            retry++;
                        }
                        while (teleport.GetOverworldState() != teleport.OverworldState.OverworldOrInAirport);

                        //Thread.Sleep(2000);
                        controller.releaseL();
                        HoldingL = false;
                        WriteLog("[Warning] Start Normal Restore", true);
                        WriteLog("Please wait for the bot to finish the sequence.", true);
                        NormalRestore();
                        unLockControl();
                        WriteLog("Restore sequence finished.", true);

                    }
                }

                Thread.Sleep(2000);

            } while (standaloneRunning);
        }
    }
}
