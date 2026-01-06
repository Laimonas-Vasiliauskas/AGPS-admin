using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AGPSadmin
{
    public partial class NotificationItem : UserControl
    {
        public NotificationItem()
        {
            InitializeComponent();

            labelProjectName.Text = string.Empty;
            labelPartName.Text = string.Empty;
            labelDone.Text = string.Empty;
            labelMadeBy.Text = string.Empty;
            labelTypeOfWork.Text = string.Empty;
            labelComments.Text = string.Empty;

            this.BorderStyle = BorderStyle.FixedSingle;
            this.Margin = new Padding(4);
            this.Padding = new Padding(6);
        }

        public void SetData(string projectName, string partName, int doneDelta, int totalDone, string madeBy, string typeOfWork, string comments)
        {
            labelProjectName.Text = "Project: " + (projectName ?? string.Empty);
            labelPartName.Text = "Part: " + (partName ?? string.Empty);
            labelDone.Text = $"Done: +{doneDelta} (Total {totalDone})";
            labelMadeBy.Text = "Made by: " + (madeBy ?? string.Empty);
            labelTypeOfWork.Text = "Type: " + (typeOfWork ?? string.Empty);
            labelComments.Text = "Comments: " + (comments ?? string.Empty);
        }
    }
}
