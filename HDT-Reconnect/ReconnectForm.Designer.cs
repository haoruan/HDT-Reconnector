
namespace HDT_Reconnect
{
    partial class ReconnectForm
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
            this.ReconnectButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ReconnectButton
            // 
            this.ReconnectButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReconnectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ReconnectButton.Location = new System.Drawing.Point(0, 0);
            this.ReconnectButton.Margin = new System.Windows.Forms.Padding(0);
            this.ReconnectButton.Name = "ReconnectButton";
            this.ReconnectButton.Size = new System.Drawing.Size(199, 47);
            this.ReconnectButton.TabIndex = 0;
            this.ReconnectButton.Text = "Disconnect";
            this.ReconnectButton.UseVisualStyleBackColor = true;
            this.ReconnectButton.Click += new System.EventHandler(this.ReconnectButton_Click);
            // 
            // ReconnectForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(199, 47);
            this.ControlBox = false;
            this.Controls.Add(this.ReconnectButton);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ReconnectForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Test";
            this.TopMost = true;
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button ReconnectButton;
    }
}