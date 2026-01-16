namespace AGPSadmin
{
    partial class NotificationItem
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelProjectName = new System.Windows.Forms.Label();
            this.labelPartName = new System.Windows.Forms.Label();
            this.labelDone = new System.Windows.Forms.Label();
            this.labelMadeBy = new System.Windows.Forms.Label();
            this.labelTypeOfWork = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelProjectName
            // 
            this.labelProjectName.AutoSize = true;
            this.labelProjectName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelProjectName.Location = new System.Drawing.Point(5, 5);
            this.labelProjectName.Name = "labelProjectName";
            this.labelProjectName.Size = new System.Drawing.Size(50, 16);
            this.labelProjectName.TabIndex = 0;
            this.labelProjectName.Text = "label1";
            // 
            // labelPartName
            // 
            this.labelPartName.AutoSize = true;
            this.labelPartName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelPartName.Location = new System.Drawing.Point(5, 25);
            this.labelPartName.Name = "labelPartName";
            this.labelPartName.Size = new System.Drawing.Size(50, 16);
            this.labelPartName.TabIndex = 1;
            this.labelPartName.Text = "label2";
            // 
            // labelDone
            // 
            this.labelDone.AutoSize = true;
            this.labelDone.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelDone.Location = new System.Drawing.Point(5, 55);
            this.labelDone.Name = "labelDone";
            this.labelDone.Size = new System.Drawing.Size(50, 16);
            this.labelDone.TabIndex = 2;
            this.labelDone.Text = "label3";
            // 
            // labelMadeBy
            // 
            this.labelMadeBy.AutoSize = true;
            this.labelMadeBy.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelMadeBy.Location = new System.Drawing.Point(5, 75);
            this.labelMadeBy.Name = "labelMadeBy";
            this.labelMadeBy.Size = new System.Drawing.Size(50, 16);
            this.labelMadeBy.TabIndex = 3;
            this.labelMadeBy.Text = "label4";
            // 
            // labelTypeOfWork
            // 
            this.labelTypeOfWork.AutoSize = true;
            this.labelTypeOfWork.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelTypeOfWork.Location = new System.Drawing.Point(5, 95);
            this.labelTypeOfWork.Name = "labelTypeOfWork";
            this.labelTypeOfWork.Size = new System.Drawing.Size(50, 16);
            this.labelTypeOfWork.TabIndex = 5;
            this.labelTypeOfWork.Text = "label5";
            // 
            // NotificationItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.InactiveCaption;
            this.Controls.Add(this.labelTypeOfWork);
            this.Controls.Add(this.labelMadeBy);
            this.Controls.Add(this.labelDone);
            this.Controls.Add(this.labelPartName);
            this.Controls.Add(this.labelProjectName);
            this.Name = "NotificationItem";
            this.Size = new System.Drawing.Size(250, 140);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelProjectName;
        private System.Windows.Forms.Label labelPartName;
        private System.Windows.Forms.Label labelDone;
        private System.Windows.Forms.Label labelMadeBy;
        private System.Windows.Forms.Label labelTypeOfWork;
    }
}
