namespace PackRatUI {
    partial class ProgressWin {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
                }
            base.Dispose(disposing);
            }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.lFileName = new System.Windows.Forms.Label();
            this.lFileProgress = new System.Windows.Forms.Label();
            this.lTotalCountCurrent = new System.Windows.Forms.Label();
            this.lTotalProgress = new System.Windows.Forms.Label();
            this.lCancel = new System.Windows.Forms.Button();
            this.lTotalCountTotal = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lFileName
            // 
            this.lFileName.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lFileName.Location = new System.Drawing.Point(12, 32);
            this.lFileName.Name = "lFileName";
            this.lFileName.Size = new System.Drawing.Size(440, 23);
            this.lFileName.TabIndex = 0;
            this.lFileName.Text = "SomeFileName.dat";
            this.lFileName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lFileName.UseCompatibleTextRendering = true;
            // 
            // lFileProgress
            // 
            this.lFileProgress.BackColor = System.Drawing.Color.DodgerBlue;
            this.lFileProgress.Location = new System.Drawing.Point(12, 9);
            this.lFileProgress.Name = "lFileProgress";
            this.lFileProgress.Size = new System.Drawing.Size(440, 23);
            this.lFileProgress.TabIndex = 1;
            // 
            // lTotalCountCurrent
            // 
            this.lTotalCountCurrent.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lTotalCountCurrent.Location = new System.Drawing.Point(317, 78);
            this.lTotalCountCurrent.Name = "lTotalCountCurrent";
            this.lTotalCountCurrent.Size = new System.Drawing.Size(67, 23);
            this.lTotalCountCurrent.TabIndex = 2;
            this.lTotalCountCurrent.Text = "1,859 ";
            this.lTotalCountCurrent.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lTotalCountCurrent.UseCompatibleTextRendering = true;
            // 
            // lTotalProgress
            // 
            this.lTotalProgress.BackColor = System.Drawing.Color.DodgerBlue;
            this.lTotalProgress.Location = new System.Drawing.Point(12, 55);
            this.lTotalProgress.Name = "lTotalProgress";
            this.lTotalProgress.Size = new System.Drawing.Size(440, 23);
            this.lTotalProgress.TabIndex = 3;
            // 
            // lCancel
            // 
            this.lCancel.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lCancel.Location = new System.Drawing.Point(12, 90);
            this.lCancel.Name = "lCancel";
            this.lCancel.Size = new System.Drawing.Size(60, 23);
            this.lCancel.TabIndex = 4;
            this.lCancel.Text = "Abort";
            this.lCancel.UseCompatibleTextRendering = true;
            this.lCancel.UseVisualStyleBackColor = true;
            // 
            // lTotalCountTotal
            // 
            this.lTotalCountTotal.Font = new System.Drawing.Font("Consolas", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lTotalCountTotal.Location = new System.Drawing.Point(385, 78);
            this.lTotalCountTotal.Name = "lTotalCountTotal";
            this.lTotalCountTotal.Size = new System.Drawing.Size(67, 23);
            this.lTotalCountTotal.TabIndex = 2;
            this.lTotalCountTotal.Text = "/ 3,091";
            this.lTotalCountTotal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lTotalCountTotal.UseCompatibleTextRendering = true;
            // 
            // ProgressWin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 125);
            this.ControlBox = false;
            this.Controls.Add(this.lCancel);
            this.Controls.Add(this.lTotalProgress);
            this.Controls.Add(this.lTotalCountTotal);
            this.Controls.Add(this.lTotalCountCurrent);
            this.Controls.Add(this.lFileProgress);
            this.Controls.Add(this.lFileName);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ProgressWin";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "   Progress ..";
            this.ResumeLayout(false);

            }

        #endregion
        private System.Windows.Forms.Label lFileName;
        private System.Windows.Forms.Label lFileProgress;
        private System.Windows.Forms.Label lTotalCountCurrent;
        private System.Windows.Forms.Label lTotalProgress;
        private System.Windows.Forms.Button lCancel;
        private System.Windows.Forms.Label lTotalCountTotal;
        }
    }