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
            this.labelComments = new System.Windows.Forms.Label();
            this.labelTypeOfWork = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelProjectName
            // 
            this.labelProjectName.AutoSize = true;
            this.labelProjectName.Location = new System.Drawing.Point(0, 11);
            this.labelProjectName.Name = "labelProjectName";
            this.labelProjectName.Size = new System.Drawing.Size(35, 13);
            this.labelProjectName.TabIndex = 0;
            this.labelProjectName.Text = "label1";
            // 
            // labelPartName
            // 
            this.labelPartName.AutoSize = true;
            this.labelPartName.Location = new System.Drawing.Point(0, 24);
            this.labelPartName.Name = "labelPartName";
            this.labelPartName.Size = new System.Drawing.Size(35, 13);
            this.labelPartName.TabIndex = 1;
            this.labelPartName.Text = "label2";
            // 
            // labelDone
            // 
            this.labelDone.AutoSize = true;
            this.labelDone.Location = new System.Drawing.Point(0, 47);
            this.labelDone.Name = "labelDone";
            this.labelDone.Size = new System.Drawing.Size(35, 13);
            this.labelDone.TabIndex = 2;
            this.labelDone.Text = "label3";
            // 
            // labelMadeBy
            // 
            this.labelMadeBy.AutoSize = true;
            this.labelMadeBy.Location = new System.Drawing.Point(0, 60);
            this.labelMadeBy.Name = "labelMadeBy";
            this.labelMadeBy.Size = new System.Drawing.Size(35, 13);
            this.labelMadeBy.TabIndex = 3;
            this.labelMadeBy.Text = "label4";
            // 
            // labelComments
            // 
            this.labelComments.AutoSize = true;
            this.labelComments.Location = new System.Drawing.Point(0, 97);
            this.labelComments.Name = "labelComments";
            this.labelComments.Size = new System.Drawing.Size(35, 13);
            this.labelComments.TabIndex = 4;
            this.labelComments.Text = "label5";
            // 
            // labelTypeOfWork
            // 
            this.labelTypeOfWork.AutoSize = true;
            this.labelTypeOfWork.Location = new System.Drawing.Point(3, 73);
            this.labelTypeOfWork.Name = "labelTypeOfWork";
            this.labelTypeOfWork.Size = new System.Drawing.Size(35, 13);
            this.labelTypeOfWork.TabIndex = 5;
            this.labelTypeOfWork.Text = "label5";
            // 
            // NotificationItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.labelTypeOfWork);
            this.Controls.Add(this.labelComments);
            this.Controls.Add(this.labelMadeBy);
            this.Controls.Add(this.labelDone);
            this.Controls.Add(this.labelPartName);
            this.Controls.Add(this.labelProjectName);
            this.Name = "NotificationItem";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelProjectName;
        private System.Windows.Forms.Label labelPartName;
        private System.Windows.Forms.Label labelDone;
        private System.Windows.Forms.Label labelMadeBy;
        private System.Windows.Forms.Label labelComments;
        private System.Windows.Forms.Label labelTypeOfWork;
    }
}
