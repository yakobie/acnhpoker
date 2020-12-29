
namespace ACNHPoker
{
    partial class MapRegenerator
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MapRegenerator));
            this.PleaseWaitPanel = new System.Windows.Forms.Panel();
            this.WaitMessagebox = new System.Windows.Forms.RichTextBox();
            this.MapProgressBar = new System.Windows.Forms.ProgressBar();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.label29 = new System.Windows.Forms.Label();
            this.loadMapBtn = new System.Windows.Forms.Button();
            this.saveMapBtn = new System.Windows.Forms.Button();
            this.ProgressTimer = new System.Windows.Forms.Timer(this.components);
            this.trayIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.startRegen = new System.Windows.Forms.Button();
            this.hideBtn = new System.Windows.Forms.Button();
            this.backBtn = new System.Windows.Forms.Button();
            this.timeLabel = new System.Windows.Forms.Label();
            this.startRegen2 = new System.Windows.Forms.Button();
            this.FinMsg = new System.Windows.Forms.RichTextBox();
            this.delay = new System.Windows.Forms.RichTextBox();
            this.ms = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.debugBtn = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.visitorNameBox = new System.Windows.Forms.RichTextBox();
            this.PleaseWaitPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.SuspendLayout();
            // 
            // PleaseWaitPanel
            // 
            this.PleaseWaitPanel.Controls.Add(this.WaitMessagebox);
            this.PleaseWaitPanel.Controls.Add(this.MapProgressBar);
            this.PleaseWaitPanel.Controls.Add(this.pictureBox2);
            this.PleaseWaitPanel.Controls.Add(this.label29);
            this.PleaseWaitPanel.Location = new System.Drawing.Point(2, 180);
            this.PleaseWaitPanel.Name = "PleaseWaitPanel";
            this.PleaseWaitPanel.Size = new System.Drawing.Size(230, 60);
            this.PleaseWaitPanel.TabIndex = 218;
            this.PleaseWaitPanel.Visible = false;
            // 
            // WaitMessagebox
            // 
            this.WaitMessagebox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(57)))), ((int)(((byte)(63)))));
            this.WaitMessagebox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.WaitMessagebox.Cursor = System.Windows.Forms.Cursors.Default;
            this.WaitMessagebox.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.WaitMessagebox.ForeColor = System.Drawing.Color.White;
            this.WaitMessagebox.Location = new System.Drawing.Point(4, 32);
            this.WaitMessagebox.Multiline = false;
            this.WaitMessagebox.Name = "WaitMessagebox";
            this.WaitMessagebox.ReadOnly = true;
            this.WaitMessagebox.Size = new System.Drawing.Size(220, 27);
            this.WaitMessagebox.TabIndex = 215;
            this.WaitMessagebox.Text = "";
            // 
            // MapProgressBar
            // 
            this.MapProgressBar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(57)))), ((int)(((byte)(63)))));
            this.MapProgressBar.ForeColor = System.Drawing.Color.LawnGreen;
            this.MapProgressBar.Location = new System.Drawing.Point(4, 28);
            this.MapProgressBar.Maximum = 260;
            this.MapProgressBar.Name = "MapProgressBar";
            this.MapProgressBar.Size = new System.Drawing.Size(220, 3);
            this.MapProgressBar.TabIndex = 215;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::ACNHPoker.Properties.Resources.loading;
            this.pictureBox2.Location = new System.Drawing.Point(44, 2);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(24, 24);
            this.pictureBox2.TabIndex = 216;
            this.pictureBox2.TabStop = false;
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.label29.ForeColor = System.Drawing.Color.White;
            this.label29.Location = new System.Drawing.Point(70, 7);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(100, 16);
            this.label29.TabIndex = 215;
            this.label29.Text = "Please Wait...";
            this.label29.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // loadMapBtn
            // 
            this.loadMapBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.loadMapBtn.FlatAppearance.BorderSize = 0;
            this.loadMapBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.loadMapBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.loadMapBtn.ForeColor = System.Drawing.Color.White;
            this.loadMapBtn.Location = new System.Drawing.Point(13, 46);
            this.loadMapBtn.Margin = new System.Windows.Forms.Padding(4);
            this.loadMapBtn.Name = "loadMapBtn";
            this.loadMapBtn.Size = new System.Drawing.Size(208, 25);
            this.loadMapBtn.TabIndex = 217;
            this.loadMapBtn.Text = "Load Map Template";
            this.loadMapBtn.UseVisualStyleBackColor = false;
            this.loadMapBtn.Click += new System.EventHandler(this.loadMapBtn_Click);
            // 
            // saveMapBtn
            // 
            this.saveMapBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.saveMapBtn.FlatAppearance.BorderSize = 0;
            this.saveMapBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.saveMapBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.saveMapBtn.ForeColor = System.Drawing.Color.White;
            this.saveMapBtn.Location = new System.Drawing.Point(13, 13);
            this.saveMapBtn.Margin = new System.Windows.Forms.Padding(4);
            this.saveMapBtn.Name = "saveMapBtn";
            this.saveMapBtn.Size = new System.Drawing.Size(208, 25);
            this.saveMapBtn.TabIndex = 216;
            this.saveMapBtn.Text = "Create Map Template";
            this.saveMapBtn.UseVisualStyleBackColor = false;
            this.saveMapBtn.Click += new System.EventHandler(this.saveMapBtn_Click);
            // 
            // ProgressTimer
            // 
            this.ProgressTimer.Interval = 500;
            this.ProgressTimer.Tick += new System.EventHandler(this.ProgressTimer_Tick);
            // 
            // trayIcon
            // 
            this.trayIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("trayIcon.Icon")));
            this.trayIcon.Text = "ACNHPoker : Map Regenerator";
            this.trayIcon.Visible = true;
            this.trayIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.trayIcon_MouseDoubleClick);
            // 
            // startRegen
            // 
            this.startRegen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.startRegen.FlatAppearance.BorderSize = 0;
            this.startRegen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.startRegen.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.startRegen.ForeColor = System.Drawing.Color.White;
            this.startRegen.Location = new System.Drawing.Point(13, 79);
            this.startRegen.Margin = new System.Windows.Forms.Padding(4);
            this.startRegen.Name = "startRegen";
            this.startRegen.Size = new System.Drawing.Size(208, 25);
            this.startRegen.TabIndex = 219;
            this.startRegen.Tag = "Start";
            this.startRegen.Text = "Cast Regen";
            this.startRegen.UseVisualStyleBackColor = false;
            this.startRegen.Click += new System.EventHandler(this.startRegen_Click);
            // 
            // hideBtn
            // 
            this.hideBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.hideBtn.FlatAppearance.BorderSize = 0;
            this.hideBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.hideBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.hideBtn.ForeColor = System.Drawing.Color.White;
            this.hideBtn.Location = new System.Drawing.Point(121, 248);
            this.hideBtn.Margin = new System.Windows.Forms.Padding(4);
            this.hideBtn.Name = "hideBtn";
            this.hideBtn.Size = new System.Drawing.Size(100, 25);
            this.hideBtn.TabIndex = 220;
            this.hideBtn.Text = "Hide";
            this.hideBtn.UseVisualStyleBackColor = false;
            this.hideBtn.Click += new System.EventHandler(this.hideBtn_Click);
            // 
            // backBtn
            // 
            this.backBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.backBtn.FlatAppearance.BorderSize = 0;
            this.backBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.backBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.backBtn.ForeColor = System.Drawing.Color.White;
            this.backBtn.Location = new System.Drawing.Point(13, 248);
            this.backBtn.Margin = new System.Windows.Forms.Padding(4);
            this.backBtn.Name = "backBtn";
            this.backBtn.Size = new System.Drawing.Size(100, 25);
            this.backBtn.TabIndex = 221;
            this.backBtn.Text = "Back";
            this.backBtn.UseVisualStyleBackColor = false;
            this.backBtn.Click += new System.EventHandler(this.backBtn_Click);
            // 
            // timeLabel
            // 
            this.timeLabel.AutoSize = true;
            this.timeLabel.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.timeLabel.ForeColor = System.Drawing.Color.White;
            this.timeLabel.Location = new System.Drawing.Point(86, 232);
            this.timeLabel.Name = "timeLabel";
            this.timeLabel.Size = new System.Drawing.Size(0, 16);
            this.timeLabel.TabIndex = 222;
            this.timeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // startRegen2
            // 
            this.startRegen2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.startRegen2.FlatAppearance.BorderSize = 0;
            this.startRegen2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.startRegen2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.startRegen2.ForeColor = System.Drawing.Color.White;
            this.startRegen2.Location = new System.Drawing.Point(13, 112);
            this.startRegen2.Margin = new System.Windows.Forms.Padding(4);
            this.startRegen2.Name = "startRegen2";
            this.startRegen2.Size = new System.Drawing.Size(208, 25);
            this.startRegen2.TabIndex = 223;
            this.startRegen2.Tag = "Start";
            this.startRegen2.Text = "Cast Moogle Regenja";
            this.startRegen2.UseVisualStyleBackColor = false;
            this.startRegen2.Click += new System.EventHandler(this.startRegen2_Click);
            // 
            // FinMsg
            // 
            this.FinMsg.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(57)))), ((int)(((byte)(63)))));
            this.FinMsg.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.FinMsg.Cursor = System.Windows.Forms.Cursors.Default;
            this.FinMsg.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold);
            this.FinMsg.ForeColor = System.Drawing.Color.White;
            this.FinMsg.Location = new System.Drawing.Point(6, 205);
            this.FinMsg.Multiline = false;
            this.FinMsg.Name = "FinMsg";
            this.FinMsg.ReadOnly = true;
            this.FinMsg.Size = new System.Drawing.Size(220, 27);
            this.FinMsg.TabIndex = 217;
            this.FinMsg.Text = "";
            this.FinMsg.Visible = false;
            // 
            // delay
            // 
            this.delay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(68)))), ((int)(((byte)(75)))));
            this.delay.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.delay.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.delay.ForeColor = System.Drawing.Color.White;
            this.delay.Location = new System.Drawing.Point(127, 142);
            this.delay.MaxLength = 8;
            this.delay.Multiline = false;
            this.delay.Name = "delay";
            this.delay.Size = new System.Drawing.Size(72, 20);
            this.delay.TabIndex = 224;
            this.delay.Text = "50";
            this.delay.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.delay_KeyPress);
            // 
            // ms
            // 
            this.ms.AutoSize = true;
            this.ms.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.ms.ForeColor = System.Drawing.Color.White;
            this.ms.Location = new System.Drawing.Point(198, 143);
            this.ms.Name = "ms";
            this.ms.Size = new System.Drawing.Size(28, 16);
            this.ms.TabIndex = 217;
            this.ms.Text = "ms";
            this.ms.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(10, 143);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 16);
            this.label1.TabIndex = 225;
            this.label1.Text = "Refresh Delay :";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // debugBtn
            // 
            this.debugBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.debugBtn.FlatAppearance.BorderSize = 0;
            this.debugBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.debugBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.debugBtn.ForeColor = System.Drawing.Color.White;
            this.debugBtn.Location = new System.Drawing.Point(13, 313);
            this.debugBtn.Margin = new System.Windows.Forms.Padding(4);
            this.debugBtn.Name = "debugBtn";
            this.debugBtn.Size = new System.Drawing.Size(100, 25);
            this.debugBtn.TabIndex = 226;
            this.debugBtn.Text = "Debug";
            this.debugBtn.UseVisualStyleBackColor = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(30, 165);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 16);
            this.label2.TabIndex = 227;
            this.label2.Text = "Last Visitor :";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // visitorNameBox
            // 
            this.visitorNameBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(68)))), ((int)(((byte)(75)))));
            this.visitorNameBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.visitorNameBox.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.visitorNameBox.ForeColor = System.Drawing.Color.White;
            this.visitorNameBox.Location = new System.Drawing.Point(127, 164);
            this.visitorNameBox.MaxLength = 8;
            this.visitorNameBox.Multiline = false;
            this.visitorNameBox.Name = "visitorNameBox";
            this.visitorNameBox.Size = new System.Drawing.Size(94, 20);
            this.visitorNameBox.TabIndex = 228;
            this.visitorNameBox.Text = "";
            // 
            // MapRegenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(57)))), ((int)(((byte)(63)))));
            this.ClientSize = new System.Drawing.Size(234, 281);
            this.Controls.Add(this.visitorNameBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.debugBtn);
            this.Controls.Add(this.timeLabel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ms);
            this.Controls.Add(this.delay);
            this.Controls.Add(this.startRegen2);
            this.Controls.Add(this.backBtn);
            this.Controls.Add(this.hideBtn);
            this.Controls.Add(this.startRegen);
            this.Controls.Add(this.PleaseWaitPanel);
            this.Controls.Add(this.loadMapBtn);
            this.Controls.Add(this.saveMapBtn);
            this.Controls.Add(this.FinMsg);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(250, 300);
            this.Name = "MapRegenerator";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Map Regenerator";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MapRegenerator_FormClosed);
            this.PleaseWaitPanel.ResumeLayout(false);
            this.PleaseWaitPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel PleaseWaitPanel;
        private System.Windows.Forms.RichTextBox WaitMessagebox;
        private System.Windows.Forms.ProgressBar MapProgressBar;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Button loadMapBtn;
        private System.Windows.Forms.Button saveMapBtn;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Timer ProgressTimer;
        private System.Windows.Forms.NotifyIcon trayIcon;
        private System.Windows.Forms.Button startRegen;
        private System.Windows.Forms.Button hideBtn;
        private System.Windows.Forms.Button backBtn;
        private System.Windows.Forms.Label timeLabel;
        private System.Windows.Forms.Button startRegen2;
        private System.Windows.Forms.RichTextBox FinMsg;
        private System.Windows.Forms.RichTextBox delay;
        private System.Windows.Forms.Label ms;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button debugBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.RichTextBox visitorNameBox;
    }
}