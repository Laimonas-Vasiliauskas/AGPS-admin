using AdminApp;
using AGPSadmin.Models;
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
    public partial class Form3 : Form
    {
        private readonly AGPSadmin.Repositories.ProjectRepository _repo = new AGPSadmin.Repositories.ProjectRepository();
        private readonly Dictionary<int, int> _lastDoneByPartId = new Dictionary<int, int>();

        public Form3()
        {
            InitializeComponent();

            this.Load += Form3_Load;
            this.FormClosing += Form3_FormClosing;

        }

        private void Form3_Load(object sender, EventArgs e)
        {
            InitializeBaseline();
            this.timer1.Tick += timer1_Tick;
            timer1.Start();
        }

        private void Form3_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { timer1.Stop(); } catch { }
            
        }

        private void InitializeBaseline()
        {
            _lastDoneByPartId.Clear();

            var projects = _repo.GetProjectsWithParts();
            foreach (var project in projects)
            {
                if (project.Parts == null) continue;
                foreach (var part in project.Parts)
                {
                    _lastDoneByPartId[part.id] = part.done;
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                PollForChanges();
            }
            catch
            {
                // ignore 
            }
        }

        private void PollForChanges()
        {
            var projects = _repo.GetProjectsWithParts();
            var notifications = new List<Tuple<string, string, int, int, string, string, string>>();

            foreach (var project in projects)
            {
                if (project.Parts == null) continue;

                foreach (var part in project.Parts)
                {
                    int last = 0;
                    _lastDoneByPartId.TryGetValue(part.id, out last);

                    if (part.done > last)
                    {
                        int delta = part.done - last;
                        notifications.Add(Tuple.Create(project.projectname ?? string.Empty,
                                                      part.partname ?? string.Empty,
                                                      delta,
                                                      part.done,
                                                      part.madeby ?? string.Empty,
                                                      part.typeofwork ?? string.Empty,
                                                      part.comments ?? string.Empty));

                        _lastDoneByPartId[part.id] = part.done;
                    }
                    else if (!_lastDoneByPartId.ContainsKey(part.id))
                    {
                        _lastDoneByPartId[part.id] = part.done;
                    }
                }
            }

            if (notifications.Count > 0)
            {
                // Atvaizduoja notifications
                foreach (var n in notifications)
                {
                    var item = new NotificationItem();
                    item.SetData(n.Item1, n.Item2, n.Item3, n.Item4, n.Item5, n.Item6, n.Item7);
                    // Pridėda į viršu
                    flowLayoutPanel1.Controls.Add(item);
                    item.BringToFront();
                }
            }
        }

        public void AddNotifications(List<NotificationModel> notifications)
        {
            if (notifications == null || notifications.Count == 0)
                return;

            if (this.InvokeRequired)
            {
                this.Invoke(new Action<List<NotificationModel>>(AddNotifications), notifications);
                return;
            }

            // Pridėda kiekviena notification į panel
            foreach (var nm in notifications)
            {
                var item = new NotificationItem();
                item.SetData(nm.ProjectName, nm.PartName, nm.Delta, nm.TotalDone, nm.MadeBy, nm.TypeOfWork, nm.Comments);
                flowLayoutPanel1.Controls.Add(item);
                item.BringToFront();
            }
        }

        private void Form3_FormClosed(object sender, FormClosedEventArgs e)
        {
            
        }
    }
}
