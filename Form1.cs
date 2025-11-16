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
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AdminApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            ReadProjects();
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
            dataTable.Columns.Add("Is Checked");

            var repo = new AGPSadmin.Repositories.ProjectRepository();
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
                row["Is Checked"] = project.isChecked;
                dataTable.Rows.Add(row);
            }

            this.dataGridView1.DataSource = dataTable;
        }
        private void button3_Click(object sender, EventArgs e)
        {
            var value = this.dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
            if(value == null || value.Length == 0)
            {
                return;
            }
            int projectid = int.Parse(value);

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
            var value = this.dataGridView1.SelectedRows[0].Cells[0].Value.ToString();
            if (value == null || value.Length == 0)
            {
                return;
            }
            int projectid = int.Parse(value);

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
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = comboBox1.SelectedItem.ToString();

            ProjectRepository repo = new ProjectRepository();
            DataTable dt = repo.GetProjectTable(selected);

            dataGridView1.DataSource = dt;
        }
        private void LoadProjects()
        {
            ProjectRepository repo = new ProjectRepository();
            var names = repo.GetProjectNames();

            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(names.ToArray());
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadProjects();
        }
    }
}
