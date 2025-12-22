using AGPSadmin;
using AGPSadmin.Models;
using AGPSadmin.Repositories;
using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

namespace AdminApp
{
    public partial class Form1 : Form
    {
        private readonly Timer _doneWatchTimer = new Timer();
        private readonly Dictionary<int, int> _lastDoneById = new Dictionary<int, int>();
        private bool _baselineLoaded = false;
        private ProjectRepository repo = new ProjectRepository();

        private bool _isUpdating;
        public Form1()
        {
            InitializeComponent();
            ReadProjects();
            StartDoneWatcher();
            panel1.BackColor = System.Drawing.Color.LightGreen;
            panel2.BackColor = System.Drawing.Color.LightCoral;
            panel3.BackColor = System.Drawing.Color.Khaki;
        }

        private void ReadProjects()
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("ID");
            dataTable.Columns.Add("Project Name");
            dataTable.Columns.Add("Part Name");
            dataTable.Columns.Add("Made By");
            dataTable.Columns.Add("Type of Work");
            dataTable.Columns.Add("Date");
            dataTable.Columns.Add("Comments");
            dataTable.Columns.Add("Remaining");
            dataTable.Columns.Add("Done");

            var repo = new ProjectRepository();
            var projects = repo.GetProjects();

            foreach (var project in projects)
            {
                var row = dataTable.NewRow();
                row["ID"] = project.id;
                row["Project Name"] = project.projectname;
                row["Part Name"] = project.partname;
                row["Made By"] = project.madeby;
                row["Type of Work"] = project.typeofwork;
                row["Date"] = project.created_at;
                row["Comments"] = project.comments;
                row["Remaining"] = project.remaining;
                row["Done"] = project.done;
                dataTable.Rows.Add(row);
            }

            this.dataGridView1.DataSource = dataTable;
            ApplyProjectStatusColors();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (this.dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a project to edit.");
                return;
            }

            DataGridViewRow row = dataGridView1.SelectedRows[0];
            if (row.IsNewRow)
            {
                MessageBox.Show("Cannot edit an empty row.");
                return;
            }

            var value = row.Cells[0].Value?.ToString();

            if (string.IsNullOrWhiteSpace(value))
            {
                MessageBox.Show("Selected project ID is invalid.");
                return;
            }

            if (!int.TryParse(value, out int projectid))
            {
                MessageBox.Show("Selected project ID is not a valid number.");
                return;
            }

            var repo = new ProjectRepository();
            var project = repo.GetProject(projectid);

            if (project == null) return;

            Form2 form = new Form2();
            form.EditProject(project);
            if (form.ShowDialog() == DialogResult.OK)
            {
                ReadProjects();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (this.dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a project to delete.");
                return;
            }

            DataGridViewRow row = dataGridView1.SelectedRows[0];
            if (row.IsNewRow)
            {
                MessageBox.Show("Cannot delete an empty row.");
                return;
            }

            var value = row.Cells[0].Value?.ToString();

            if (string.IsNullOrWhiteSpace(value))
            {
                MessageBox.Show("Selected project ID is invalid.");
                return;
            }

            if (!int.TryParse(value, out int projectid))
            {
                MessageBox.Show("Selected project ID is not a valid number.");
                return;
            }

            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this project?", "Confirm Deletion", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                var repo = new ProjectRepository();
                repo.DeleteProject(projectid);
                ReadProjects();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Form2 form = new Form2();
            if (form.ShowDialog() == DialogResult.OK)
            {
                LoadProjects();
                ReadProjects();
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = comboBox1.SelectedItem.ToString();

            ProjectRepository repo = new ProjectRepository();
            DataTable dt = repo.GetProjectTable(selected);

            dataGridView1.DataSource = dt;
            ApplyProjectStatusColors();

        }

        private void LoadProjects()
        {
            ProjectRepository repo = new ProjectRepository();
            var names = repo.GetProjectNames("");

            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(names.ToArray());
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadProjects();
            ApplyProjectStatusColors();
            dataGridView1.DataBindingComplete += dataGridView1_DataBindingComplete;
        }

        private void comboBox1_TextUpdate(object sender, EventArgs e)
        {
            ApplyProjectStatusColors();

            ProjectRepository repo = new ProjectRepository();
            if (string.IsNullOrEmpty(comboBox1.Text))
            {
                DataTable dt = repo.GetProjectTable("");

                dataGridView1.DataSource = dt;
            }
            else
            {
                if (_isUpdating)
                    return;

                _isUpdating = true;

                string text = comboBox1.Text;
                int caret = comboBox1.SelectionStart;


                var names = repo.GetProjectNames(text);

                comboBox1.BeginUpdate();

                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(names.ToArray());

                comboBox1.DroppedDown = true;

                comboBox1.EndUpdate();

                comboBox1.Text = text;
                comboBox1.SelectionStart = caret;
                comboBox1.SelectionLength = 0;

                _isUpdating = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string projectName = comboBox1.Text?.Trim();

            if (string.IsNullOrWhiteSpace(projectName))
            {
                MessageBox.Show("Please enter or select a project name in Search before exporting.",
                    "No project selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Title = "Export project to Excel";
                sfd.Filter = "Excel files (*.xlsx)|*.xlsx";
                sfd.FileName = projectName + ".xlsx";

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    string filePath = sfd.FileName;
                    repo.ExportToExcel(filePath, projectName);

                    MessageBox.Show("File exported successfully!", "Success",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Excel Files|*.xlsx";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                repo.ImportExcelToSql(dialog.FileName);
                MessageBox.Show("Excel import completed!");
            }
        }
        private void ApplyProjectStatusColors()
        {
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;

                int remaining = Convert.ToInt32(row.Cells["Remaining"].Value ?? 0);
                int done = Convert.ToInt32(row.Cells["Done"].Value ?? 0);

                if (remaining == 0)
                    row.DefaultCellStyle.BackColor = Color.LightGreen;   
                else if (done == 0)
                    row.DefaultCellStyle.BackColor = Color.LightCoral;   
                else
                    row.DefaultCellStyle.BackColor = Color.Khaki;        

                row.DefaultCellStyle.SelectionBackColor = row.DefaultCellStyle.BackColor;
            }
        }
        private void dataGridView1_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            ApplyProjectStatusColors();
        }
        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void StartDoneWatcher()
        {
            _doneWatchTimer.Interval = 100;
            _doneWatchTimer.Tick += DoneWatchTimer_Tick;
            _doneWatchTimer.Start();
        }

        private void DoneWatchTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                var projects = repo.GetProjects();

                if (!_baselineLoaded)
                {
                    _lastDoneById.Clear();
                    foreach (var p in projects)
                        _lastDoneById[p.id] = p.done;

                    _baselineLoaded = true;
                    return;
                }

                foreach (var p in projects)
                {
                    if (_lastDoneById.TryGetValue(p.id, out int oldDone))
                    {
                        if (p.done > oldDone)
                        {
                            int delta = p.done - oldDone;
                            _lastDoneById[p.id] = p.done;

                            MessageBox.Show(
                                $"Ready to check: +{delta}\n" +
                                $"Project: {p.projectname}\n" +
                                $"Part name: {p.partname}\n" +
                                $"Made by: {p.madeby}\n" +
                                $"Type of work: {p.typeofwork}\n" +
                                $"Done totall: {p.done}",
                                "Update",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            );

                            ReadProjects();
                        }
                        else
                        {
                            _lastDoneById[p.id] = p.done;
                        }
                    }
                    else
                    {
                        _lastDoneById[p.id] = p.done;
                    }
                }
            }
            catch
            {
            }
        }
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _doneWatchTimer.Stop();
            base.OnFormClosing(e);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            LoadProjects();
        }
    }
}
