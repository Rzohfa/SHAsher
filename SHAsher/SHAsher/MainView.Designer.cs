namespace SHAsher
{
    partial class MainView
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
            this.hashBtn = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dllChooseCB = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.threadCounter = new System.Windows.Forms.NumericUpDown();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.inFileBrowseBtn = new System.Windows.Forms.Button();
            this.inFileBrowseTxt = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.outFileBrowseBtn = new System.Windows.Forms.Button();
            this.outFileBrowseTxt = new System.Windows.Forms.TextBox();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.statusLabel = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.threadCounter)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // hashBtn
            // 
            this.hashBtn.Location = new System.Drawing.Point(336, 144);
            this.hashBtn.Name = "hashBtn";
            this.hashBtn.Size = new System.Drawing.Size(75, 23);
            this.hashBtn.TabIndex = 3;
            this.hashBtn.Text = "Hash!";
            this.hashBtn.UseVisualStyleBackColor = true;
            this.hashBtn.Click += new System.EventHandler(this.hashBtn_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dllChooseCB);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.threadCounter);
            this.groupBox1.Location = new System.Drawing.Point(12, 99);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(208, 75);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Properties";
            // 
            // dllChooseCB
            // 
            this.dllChooseCB.FormattingEnabled = true;
            this.dllChooseCB.Items.AddRange(new object[] {
            "C++",
            "Assembler"});
            this.dllChooseCB.Location = new System.Drawing.Point(100, 47);
            this.dllChooseCB.Name = "dllChooseCB";
            this.dllChooseCB.Size = new System.Drawing.Size(102, 21);
            this.dllChooseCB.TabIndex = 3;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(82, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "C++/Assembler:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 1;
            this.label3.Text = "Liczba wątków";
            // 
            // threadCounter
            // 
            this.threadCounter.Location = new System.Drawing.Point(100, 21);
            this.threadCounter.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.threadCounter.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.threadCounter.Name = "threadCounter";
            this.threadCounter.Size = new System.Drawing.Size(47, 20);
            this.threadCounter.TabIndex = 0;
            this.threadCounter.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.inFileBrowseBtn);
            this.groupBox2.Controls.Add(this.inFileBrowseTxt);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.outFileBrowseBtn);
            this.groupBox2.Controls.Add(this.outFileBrowseTxt);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(405, 81);
            this.groupBox2.TabIndex = 5;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Input/Output";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 11;
            this.label2.Text = "Input file";
            // 
            // inFileBrowseBtn
            // 
            this.inFileBrowseBtn.Location = new System.Drawing.Point(324, 18);
            this.inFileBrowseBtn.Name = "inFileBrowseBtn";
            this.inFileBrowseBtn.Size = new System.Drawing.Size(75, 21);
            this.inFileBrowseBtn.TabIndex = 10;
            this.inFileBrowseBtn.Text = "Browse...";
            this.inFileBrowseBtn.UseVisualStyleBackColor = true;
            this.inFileBrowseBtn.Click += new System.EventHandler(this.InFileBrowseBtn_Click);
            // 
            // inFileBrowseTxt
            // 
            this.inFileBrowseTxt.Location = new System.Drawing.Point(67, 19);
            this.inFileBrowseTxt.Name = "inFileBrowseTxt";
            this.inFileBrowseTxt.Size = new System.Drawing.Size(251, 20);
            this.inFileBrowseTxt.TabIndex = 9;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Output file";
            // 
            // outFileBrowseBtn
            // 
            this.outFileBrowseBtn.Location = new System.Drawing.Point(324, 49);
            this.outFileBrowseBtn.Name = "outFileBrowseBtn";
            this.outFileBrowseBtn.Size = new System.Drawing.Size(75, 21);
            this.outFileBrowseBtn.TabIndex = 7;
            this.outFileBrowseBtn.Text = "Browse...";
            this.outFileBrowseBtn.UseVisualStyleBackColor = true;
            this.outFileBrowseBtn.Click += new System.EventHandler(this.OutFileBrowseBtn_Click);
            // 
            // outFileBrowseTxt
            // 
            this.outFileBrowseTxt.Location = new System.Drawing.Point(67, 50);
            this.outFileBrowseTxt.Name = "outFileBrowseTxt";
            this.outFileBrowseTxt.Size = new System.Drawing.Size(251, 20);
            this.outFileBrowseTxt.TabIndex = 6;
            // 
            // statusLabel
            // 
            this.statusLabel.AutoSize = true;
            this.statusLabel.Location = new System.Drawing.Point(227, 100);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(74, 13);
            this.statusLabel.TabIndex = 6;
            this.statusLabel.Text = "Status: Ready";
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(429, 186);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.hashBtn);
            this.Name = "MainView";
            this.Text = "SHAsher";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.threadCounter)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button hashBtn;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.NumericUpDown threadCounter;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button inFileBrowseBtn;
        private System.Windows.Forms.TextBox inFileBrowseTxt;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button outFileBrowseBtn;
        private System.Windows.Forms.TextBox outFileBrowseTxt;
        private System.Windows.Forms.ComboBox dllChooseCB;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label statusLabel;
    }
}

