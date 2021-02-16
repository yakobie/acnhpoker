using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ACNHPoker
{
    public partial class dodo : Form
    {
        public Boolean dodoSetupDone = false;
        public dodo()
        {
            InitializeComponent();

            if (teleport.allAnchorValid())
            {
                Point Done = new Point(-4200, 0);
                FullPanel.Location = Done;
                dodoSetupDone = true;
            }
        }

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
            Boolean noBestFriend;

            DoneAnchor0TestBtn.Enabled = false;
            DoneAnchor1TestBtn.Enabled = false;
            DoneAnchor2TestBtn.Enabled = false;
            DoneAnchor3TestBtn.Enabled = false;
            DoneAnchor4TestBtn.Enabled = false;
            DoneFullTestBtn.Enabled = false;

            if (TestNoBestFriendCheckbox.Checked)
                noBestFriend = true;
            else
                noBestFriend = false;
            Thread TeleportThread = new Thread(delegate () { TestNormalRestore(noBestFriend); });
            TeleportThread.Start();
        }

        private void TestNormalRestore(Boolean noBestFriend)
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
            if (noBestFriend)
                controller.talkAndGetDodoCode(true);
            else
                controller.talkAndGetDodoCode(false);
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

            controller.emoteUP();

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

        private void dodo_FormClosed(object sender, FormClosedEventArgs e)
        {

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

        public void WriteLog(string line)
        {
            dodoLog.Invoke((MethodInvoker)delegate
            {
                dodoLog.AppendText(line + "\n");
            });
        }

        public void LockBtn()
        {
            BackToSetupBtn.Invoke((MethodInvoker)delegate
            {
                BackToSetupBtn.Enabled = false;
                NoBestFriendCheckbox.Enabled = false;
            });
        }

        public void NormalRestore()
        {
            do
            {
                //Debug.Print(teleport.GetOverworldState().ToString());
                Debug.Print("Confirm Overworld");
                Thread.Sleep(2000);
            }
            while (teleport.GetOverworldState() != teleport.OverworldState.OverworldOrInAirport);

            Thread.Sleep(5000);
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
            if (NoBestFriendCheckbox.Checked)
                DisplayDodo(controller.talkAndGetDodoCode(true));
            else
                DisplayDodo(controller.talkAndGetDodoCode(false));
            Debug.Print("Finish getting Dodo");
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

            teleport.TeleportToAnchor(1);

            controller.emoteUP();

            //controller.detachController();
        }

        public void HardRestore()
        {
            controller.clickCAPTURE();
            Thread.Sleep(2000);
            controller.clickHOME();
            Thread.Sleep(5000);

            controller.clickX();
            Thread.Sleep(1000);
            controller.clickA(); //Close Game
            Thread.Sleep(15000);

            controller.clickA(); //Select Game
            Thread.Sleep(2000);
            controller.clickA(); //Select first user
            Thread.Sleep(10000);

            int retry = 0;
            do
            {
                //Debug.Print(teleport.GetOverworldState().ToString());
                //Debug.Print("Waiting for Overworld");
                controller.clickA();
                Thread.Sleep(2000);
                retry++;
            }
            while (teleport.GetOverworldState() != teleport.OverworldState.OverworldOrInAirport);

            Debug.Print("Exiting House");
            Thread.Sleep(5000);
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

        private void dodoLog_TextChanged(object sender, EventArgs e)
        {
            dodoLog.SelectionStart = dodoLog.Text.Length;
            dodoLog.ScrollToCaret();
        }

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
            controller.emoteUP();
        }

        private void emoteRIGHTBtn_Click(object sender, EventArgs e)
        {
            controller.emoteRIGHT();
        }

        private void emoteDOWNBtn_Click(object sender, EventArgs e)
        {
            controller.emoteDOWN();
        }

        private void emoteLEFTBtn_Click(object sender, EventArgs e)
        {
            controller.emoteLEFT();
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
    }
}
