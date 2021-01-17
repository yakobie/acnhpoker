
namespace ACNHPoker
{
    partial class bulkSpawn
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(bulkSpawn));
            this.downNumber = new System.Windows.Forms.RichTextBox();
            this.yCoordinate = new System.Windows.Forms.RichTextBox();
            this.xCoordinate = new System.Windows.Forms.RichTextBox();
            this.leftBtn = new System.Windows.Forms.RadioButton();
            this.rightBtn = new System.Windows.Forms.RadioButton();
            this.miniMapBox = new System.Windows.Forms.PictureBox();
            this.selectBtn = new System.Windows.Forms.Button();
            this.spawnBtn = new System.Windows.Forms.Button();
            this.wrapSetting = new System.Windows.Forms.ComboBox();
            this.previewBtn = new System.Windows.Forms.Button();
            this.createBtn = new System.Windows.Forms.Button();
            this.ReadBtn = new System.Windows.Forms.Button();
            this.numOfItemBox = new System.Windows.Forms.RichTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.miniMapBox)).BeginInit();
            this.SuspendLayout();
            // 
            // downNumber
            // 
            this.downNumber.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(68)))), ((int)(((byte)(75)))));
            this.downNumber.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.downNumber.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.downNumber.ForeColor = System.Drawing.Color.White;
            this.downNumber.Location = new System.Drawing.Point(480, 173);
            this.downNumber.Margin = new System.Windows.Forms.Padding(4);
            this.downNumber.MaxLength = 3;
            this.downNumber.Multiline = false;
            this.downNumber.Name = "downNumber";
            this.downNumber.Size = new System.Drawing.Size(84, 25);
            this.downNumber.TabIndex = 73;
            this.downNumber.Text = "32";
            // 
            // yCoordinate
            // 
            this.yCoordinate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(68)))), ((int)(((byte)(75)))));
            this.yCoordinate.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.yCoordinate.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.yCoordinate.ForeColor = System.Drawing.Color.White;
            this.yCoordinate.Location = new System.Drawing.Point(573, 371);
            this.yCoordinate.Margin = new System.Windows.Forms.Padding(4);
            this.yCoordinate.MaxLength = 3;
            this.yCoordinate.Multiline = false;
            this.yCoordinate.Name = "yCoordinate";
            this.yCoordinate.Size = new System.Drawing.Size(84, 25);
            this.yCoordinate.TabIndex = 75;
            this.yCoordinate.Text = "";
            // 
            // xCoordinate
            // 
            this.xCoordinate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(68)))), ((int)(((byte)(75)))));
            this.xCoordinate.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.xCoordinate.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.xCoordinate.ForeColor = System.Drawing.Color.White;
            this.xCoordinate.Location = new System.Drawing.Point(471, 371);
            this.xCoordinate.Margin = new System.Windows.Forms.Padding(4);
            this.xCoordinate.MaxLength = 3;
            this.xCoordinate.Multiline = false;
            this.xCoordinate.Name = "xCoordinate";
            this.xCoordinate.Size = new System.Drawing.Size(84, 25);
            this.xCoordinate.TabIndex = 74;
            this.xCoordinate.Text = "";
            // 
            // leftBtn
            // 
            this.leftBtn.AutoSize = true;
            this.leftBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.leftBtn.ForeColor = System.Drawing.Color.White;
            this.leftBtn.Location = new System.Drawing.Point(582, 160);
            this.leftBtn.Margin = new System.Windows.Forms.Padding(4);
            this.leftBtn.Name = "leftBtn";
            this.leftBtn.Size = new System.Drawing.Size(50, 20);
            this.leftBtn.TabIndex = 76;
            this.leftBtn.Text = "Left";
            this.leftBtn.UseVisualStyleBackColor = true;
            // 
            // rightBtn
            // 
            this.rightBtn.AutoSize = true;
            this.rightBtn.Checked = true;
            this.rightBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.rightBtn.ForeColor = System.Drawing.Color.White;
            this.rightBtn.Location = new System.Drawing.Point(582, 188);
            this.rightBtn.Margin = new System.Windows.Forms.Padding(4);
            this.rightBtn.Name = "rightBtn";
            this.rightBtn.Size = new System.Drawing.Size(59, 20);
            this.rightBtn.TabIndex = 77;
            this.rightBtn.TabStop = true;
            this.rightBtn.Text = "Right";
            this.rightBtn.UseVisualStyleBackColor = true;
            // 
            // miniMapBox
            // 
            this.miniMapBox.Location = new System.Drawing.Point(12, 12);
            this.miniMapBox.Name = "miniMapBox";
            this.miniMapBox.Size = new System.Drawing.Size(448, 384);
            this.miniMapBox.TabIndex = 78;
            this.miniMapBox.TabStop = false;
            // 
            // selectBtn
            // 
            this.selectBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.selectBtn.FlatAppearance.BorderSize = 0;
            this.selectBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.selectBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.selectBtn.ForeColor = System.Drawing.Color.White;
            this.selectBtn.Location = new System.Drawing.Point(484, 59);
            this.selectBtn.Margin = new System.Windows.Forms.Padding(4);
            this.selectBtn.Name = "selectBtn";
            this.selectBtn.Size = new System.Drawing.Size(138, 28);
            this.selectBtn.TabIndex = 221;
            this.selectBtn.Text = "Select";
            this.selectBtn.UseVisualStyleBackColor = false;
            this.selectBtn.Click += new System.EventHandler(this.selectBtn_Click);
            // 
            // spawnBtn
            // 
            this.spawnBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.spawnBtn.FlatAppearance.BorderSize = 0;
            this.spawnBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.spawnBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.spawnBtn.ForeColor = System.Drawing.Color.White;
            this.spawnBtn.Location = new System.Drawing.Point(484, 335);
            this.spawnBtn.Margin = new System.Windows.Forms.Padding(4);
            this.spawnBtn.Name = "spawnBtn";
            this.spawnBtn.Size = new System.Drawing.Size(138, 28);
            this.spawnBtn.TabIndex = 222;
            this.spawnBtn.Text = "Spawn";
            this.spawnBtn.UseVisualStyleBackColor = false;
            this.spawnBtn.Click += new System.EventHandler(this.spawnBtn_Click);
            // 
            // wrapSetting
            // 
            this.wrapSetting.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(57)))), ((int)(((byte)(63)))));
            this.wrapSetting.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.wrapSetting.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.wrapSetting.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.wrapSetting.ForeColor = System.Drawing.Color.White;
            this.wrapSetting.FormattingEnabled = true;
            this.wrapSetting.ItemHeight = 16;
            this.wrapSetting.Items.AddRange(new object[] {
            "DIY no",
            "DIY A-Z"});
            this.wrapSetting.Location = new System.Drawing.Point(484, 12);
            this.wrapSetting.Name = "wrapSetting";
            this.wrapSetting.Size = new System.Drawing.Size(138, 24);
            this.wrapSetting.TabIndex = 223;
            // 
            // previewBtn
            // 
            this.previewBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.previewBtn.FlatAppearance.BorderSize = 0;
            this.previewBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.previewBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.previewBtn.ForeColor = System.Drawing.Color.White;
            this.previewBtn.Location = new System.Drawing.Point(484, 287);
            this.previewBtn.Margin = new System.Windows.Forms.Padding(4);
            this.previewBtn.Name = "previewBtn";
            this.previewBtn.Size = new System.Drawing.Size(138, 28);
            this.previewBtn.TabIndex = 224;
            this.previewBtn.Text = "Preview";
            this.previewBtn.UseVisualStyleBackColor = false;
            this.previewBtn.Click += new System.EventHandler(this.previewBtn_Click);
            // 
            // createBtn
            // 
            this.createBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.createBtn.FlatAppearance.BorderSize = 0;
            this.createBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.createBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.createBtn.ForeColor = System.Drawing.Color.White;
            this.createBtn.Location = new System.Drawing.Point(689, 79);
            this.createBtn.Margin = new System.Windows.Forms.Padding(4);
            this.createBtn.Name = "createBtn";
            this.createBtn.Size = new System.Drawing.Size(138, 28);
            this.createBtn.TabIndex = 225;
            this.createBtn.Text = "Create";
            this.createBtn.UseVisualStyleBackColor = false;
            this.createBtn.Click += new System.EventHandler(this.createBtn_Click);
            // 
            // ReadBtn
            // 
            this.ReadBtn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(114)))), ((int)(((byte)(137)))), ((int)(((byte)(218)))));
            this.ReadBtn.FlatAppearance.BorderSize = 0;
            this.ReadBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ReadBtn.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.ReadBtn.ForeColor = System.Drawing.Color.White;
            this.ReadBtn.Location = new System.Drawing.Point(689, 43);
            this.ReadBtn.Margin = new System.Windows.Forms.Padding(4);
            this.ReadBtn.Name = "ReadBtn";
            this.ReadBtn.Size = new System.Drawing.Size(138, 28);
            this.ReadBtn.TabIndex = 226;
            this.ReadBtn.Text = "Read";
            this.ReadBtn.UseVisualStyleBackColor = false;
            this.ReadBtn.Click += new System.EventHandler(this.ReadBtn_Click);
            // 
            // numOfItemBox
            // 
            this.numOfItemBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(68)))), ((int)(((byte)(75)))));
            this.numOfItemBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.numOfItemBox.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.numOfItemBox.ForeColor = System.Drawing.Color.White;
            this.numOfItemBox.Location = new System.Drawing.Point(484, 110);
            this.numOfItemBox.Margin = new System.Windows.Forms.Padding(4);
            this.numOfItemBox.MaxLength = 3;
            this.numOfItemBox.Multiline = false;
            this.numOfItemBox.Name = "numOfItemBox";
            this.numOfItemBox.Size = new System.Drawing.Size(84, 25);
            this.numOfItemBox.TabIndex = 227;
            this.numOfItemBox.Text = "";
            // 
            // bulkSpawn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(54)))), ((int)(((byte)(57)))), ((int)(((byte)(63)))));
            this.ClientSize = new System.Drawing.Size(667, 410);
            this.Controls.Add(this.numOfItemBox);
            this.Controls.Add(this.ReadBtn);
            this.Controls.Add(this.createBtn);
            this.Controls.Add(this.previewBtn);
            this.Controls.Add(this.wrapSetting);
            this.Controls.Add(this.spawnBtn);
            this.Controls.Add(this.selectBtn);
            this.Controls.Add(this.miniMapBox);
            this.Controls.Add(this.rightBtn);
            this.Controls.Add(this.leftBtn);
            this.Controls.Add(this.yCoordinate);
            this.Controls.Add(this.xCoordinate);
            this.Controls.Add(this.downNumber);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "bulkSpawn";
            this.Text = "Bulk Spawn";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.bulkSpawn_FormClosed);
            ((System.ComponentModel.ISupportInitialize)(this.miniMapBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox downNumber;
        private System.Windows.Forms.RichTextBox yCoordinate;
        private System.Windows.Forms.RichTextBox xCoordinate;
        private System.Windows.Forms.RadioButton leftBtn;
        private System.Windows.Forms.RadioButton rightBtn;
        private System.Windows.Forms.PictureBox miniMapBox;
        private System.Windows.Forms.Button selectBtn;
        private System.Windows.Forms.Button spawnBtn;
        private System.Windows.Forms.ComboBox wrapSetting;
        private System.Windows.Forms.Button previewBtn;
        private System.Windows.Forms.Button createBtn;
        private System.Windows.Forms.Button ReadBtn;
        private System.Windows.Forms.RichTextBox numOfItemBox;
    }
}