using AGPSadmin;
using AGPSadmin.Models;
using AGPSadmin.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static OfficeOpenXml.ExcelErrorValue;

namespace AdminApp
{
    public partial class Form1 : Form
    {
        private ProjectRepository repo = new ProjectRepository();
        private readonly Dictionary<int, int> _lastDoneByPartId = new Dictionary<int, int>();
        private bool _baselineLoaded = false;
        private bool _isRefreshing = false;
        private bool _isUpdating = false;

        public Form1()
        {
            InitializeComponent();
            ReadProjects();
            panel1.BackColor = System.Drawing.Color.LightGreen;
            panel2.BackColor = System.Drawing.Color.LightCoral;
            panel3.BackColor = System.Drawing.Color.Khaki;
        }

        private void ReadProjects()
        {
            if (_isRefreshing) return;
            _isRefreshing = true;

            try
            {
                DataTable dataTable = new DataTable();

                dataTable.Columns.Add("ID");
                dataTable.Columns.Add("PartId");
                dataTable.Columns.Add("Project Name");
                dataTable.Columns.Add("Part Name");
                dataTable.Columns.Add("Made By");
                dataTable.Columns.Add("Type of Work");
                dataTable.Columns.Add("Date");
                dataTable.Columns.Add("Comments");
                dataTable.Columns.Add("Remaining");
                dataTable.Columns.Add("Done");

                var repo = new ProjectRepository();
                var projects = repo.GetProjectsWithParts();

                foreach (var project in projects)
                {
                    if (project.Parts == null || project.Parts.Count == 0)
                    {
                        var row = dataTable.NewRow();
                        row["ID"] = project.id;
                        row["Project Name"] = project.projectname;
                        dataTable.Rows.Add(row);
                    }
                    else
                    {
                        foreach (var part in project.Parts)
                        {
                            var row = dataTable.NewRow();
                            row["ID"] = project.id;
                            row["PartId"] = part.id;
                            row["Project Name"] = project.projectname;
                            row["Part Name"] = part.partname;
                            row["Made By"] = part.madeby;
                            row["Type of Work"] = part.typeofwork;
                            row["Date"] = part.created_at;
                            row["Comments"] = part.comments;
                            row["Remaining"] = part.remaining;
                            row["Done"] = part.done;
                            dataTable.Rows.Add(row);
                        }
                    }
                }

                dataGridView1.DataSource = dataTable;
                ApplyProjectStatusColors();
                DetectDoneChangesAndPopupFromGrid();
            }
            finally
            {
                _isRefreshing = false;
            }
        }
        // Mygtukas EDIT
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
            var project = repo.GetProjectWithParts(projectid);

            Part part = null;
            var partName = row.Cells["Part Name"].Value?.ToString();
            if (!string.IsNullOrWhiteSpace(partName) && project?.Parts != null)
            {
                part = project.Parts.FirstOrDefault(p => p.partname == partName);
            }

            if (project == null) return;

            Form2 form = new Form2();
            form.EditProjectWithPart(project, part);
            if (form.ShowDialog() == DialogResult.OK)
            {
                ReadProjects();
            }
        }

        // Mygtukas DELETE
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

            var value = row.Cells[1].Value?.ToString();

            if (string.IsNullOrWhiteSpace(value))
            {
                MessageBox.Show("Selected project ID is invalid.");
                return;
            }

            if (!int.TryParse(value, out int partid))
            {
                MessageBox.Show("Selected project ID is not a valid number.");
                return;
            }

            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete this part?\n" +
            "If it is the last part, the project will be deleted too.", "Confirm Deletion", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                var repo = new ProjectRepository();
                repo.DeletePartOrProject(partid);
                ReadProjects();
            }
        }

        // Mygtukas ADD
        private void button1_Click(object sender, EventArgs e)
        {
            Form2 form = new Form2();
            if (form.ShowDialog() == DialogResult.OK)
            {
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
            dataGridView1.Columns["PartId"].Visible = false;
            dataGridView1.Columns["Id"].Visible = false;
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

        // Mygtukas EXPORT
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

        // Mygtukas IMPORT
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
            if (dataGridView1.Columns.Contains("Remaining") == false ||
                dataGridView1.Columns.Contains("Done") == false)
                return;

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
        private void button6_Click(object sender, EventArgs e)
        {
            ReadProjects();
            dataGridView1.Columns["PartId"].Visible = false;
        }
        private void DetectDoneChangesAndPopupFromGrid()
        {
            var changes = new List<string>();

            // PIRMAS kartas: tik bazė (be popup)
            if (!_baselineLoaded)
            {
                _lastDoneByPartId.Clear();

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    // projektas be dalių -> PartId bus null/empty
                    if (row.Cells["PartId"].Value == null || row.Cells["PartId"].Value == DBNull.Value)
                        continue;

                    int partId = Convert.ToInt32(row.Cells["PartId"].Value);

                    int done = 0;
                    int.TryParse(row.Cells["Done"].Value?.ToString(), out done);

                    _lastDoneByPartId[partId] = done;
                }

                _baselineLoaded = true;
                return;
            }

            // Toliau: tikrinam pokyčius
            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                if (row.IsNewRow) continue;

                if (row.Cells["PartId"].Value == null || row.Cells["PartId"].Value == DBNull.Value)
                    continue;

                int partId = Convert.ToInt32(row.Cells["PartId"].Value);

                int done = 0;
                int.TryParse(row.Cells["Done"].Value?.ToString(), out done);

                _lastDoneByPartId.TryGetValue(partId, out int lastDone);

                if (done > lastDone)
                {
                    int diff = done - lastDone;

                    string projectName = row.Cells["Project Name"].Value?.ToString() ?? "";
                    string partName = row.Cells["Part Name"].Value?.ToString() ?? "";
                    string typeOfWork = row.Cells["Type of Work"].Value?.ToString() ?? "";
                    string madeBy = row.Cells["Made By"].Value?.ToString() ?? "";

                    changes.Add($" Project: {projectName} \n Part: {partName} \n Done: +{diff} (Total {done}) \n Made By: {madeBy}");

                    _lastDoneByPartId[partId] = done;
                }
                else if (!_lastDoneByPartId.ContainsKey(partId))
                {
                    // nauja part atsirado
                    _lastDoneByPartId[partId] = done;
                }
            }

            if (changes.Count > 0)
            {
                MessageBox.Show(
                    string.Join(Environment.NewLine, changes),
                    "DB Changes",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            ReadProjects();
            dataGridView1.Columns["PartId"].Visible = false;
        }
    }
}
