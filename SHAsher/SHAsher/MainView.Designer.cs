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
            this.inputTxt = new System.Windows.Forms.TextBox();
            this.outputTxt = new System.Windows.Forms.TextBox();
            this.hashBtn = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // inputTxt
            // 
            this.inputTxt.Location = new System.Drawing.Point(12, 12);
            this.inputTxt.Name = "inputTxt";
            this.inputTxt.Size = new System.Drawing.Size(776, 20);
            this.inputTxt.TabIndex = 0;
            // 
            // outputTxt
            // 
            this.outputTxt.Location = new System.Drawing.Point(12, 181);
            this.outputTxt.Name = "outputTxt";
            this.outputTxt.Size = new System.Drawing.Size(776, 20);
            this.outputTxt.TabIndex = 1;
            // 
            // hashBtn
            // 
            this.hashBtn.Location = new System.Drawing.Point(712, 59);
            this.hashBtn.Name = "hashBtn";
            this.hashBtn.Size = new System.Drawing.Size(75, 23);
            this.hashBtn.TabIndex = 3;
            this.hashBtn.Text = "Hash!";
            this.hashBtn.UseVisualStyleBackColor = true;
            this.hashBtn.Click += new System.EventHandler(this.hashBtn_Click);
            // 
            // MainView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.hashBtn);
            this.Controls.Add(this.outputTxt);
            this.Controls.Add(this.inputTxt);
            this.Name = "MainView";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox inputTxt;
        private System.Windows.Forms.TextBox outputTxt;
        private System.Windows.Forms.Button hashBtn;
    }
}

