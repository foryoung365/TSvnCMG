
namespace TSvnCMG
{
    partial class LoadingForm
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
            this.Hint = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Hint
            // 
            this.Hint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.Hint.Enabled = false;
            this.Hint.Font = new System.Drawing.Font("Microsoft YaHei", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Hint.Location = new System.Drawing.Point(0, 0);
            this.Hint.Multiline = true;
            this.Hint.Name = "Hint";
            this.Hint.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.Hint.Size = new System.Drawing.Size(463, 164);
            this.Hint.TabIndex = 0;
            this.Hint.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // LoadingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(463, 164);
            this.Controls.Add(this.Hint);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "LoadingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Generating";
            this.Load += new System.EventHandler(this.LoadingForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Hint;
    }
}