using AGPSadmin.Models;
using AGPSadmin.Repositories;
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
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();

            this.DialogResult = DialogResult.Cancel;

            LoadProjects();

            // Prijungti įvykius, kad pasikeitus projektui būtų užpildytos dalys
            this.comboBox2.SelectedIndexChanged += ComboBox2_SelectedIndexChanged;
            this.comboBox2.TextChanged += ComboBox2_TextChanged;
        }

        // Užkrauna projektų vardus, kad pridėti dalių
        private void LoadProjects()
        {
            try
            {
                var repo = new ProjectRepository();
                var names = repo.GetProjectNames("" );

                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(names.ToArray());
                comboBox2.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                comboBox2.AutoCompleteSource = AutoCompleteSource.ListItems;
                
            }
            catch
            {
                
            }
        }

        private void ComboBox2_TextChanged(object sender, EventArgs e)
        {
            LoadPartsForProject(comboBox2.Text);
        }

        private void ComboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            string projectName = comboBox2.SelectedItem?.ToString() ?? comboBox2.Text;
            LoadPartsForProject(projectName);
        }

        private void LoadPartsForProject(string projectName)
        {
            try
            {
                comboBox3.Items.Clear();

                if (string.IsNullOrWhiteSpace(projectName))
                    return;

                var repo = new ProjectRepository();
                int id = repo.GetProjectIdByName(projectName);
                if (id == 0)
                    return;

                var project = repo.GetProjectWithParts(id);
                if (project?.Parts == null)
                    return;

                var partNames = project.Parts.Select(p => p.partname ?? string.Empty).Distinct().ToArray();
                comboBox3.Items.AddRange(partNames);
            }
            catch
            {
                // ignore 
            }
        }

        private int projectId = 0;

        // Mygtukas SAVE
        private void button1_Click(object sender, EventArgs e)
        {
            Project project = new Project();
            Part part = new Part();
            project.id = this.projectId;
            project.projectname = this.comboBox2.Text;
            part.partname = this.comboBox3.Text;
            part.madeby = this.textBox3.Text;
            part.typeofwork = this.comboBox1.Text;
            part.comments = this.textBox5.Text;
            part.remaining = int.TryParse(this.textBox6.Text, out int remaining) ? remaining : 0;
            part.done = int.TryParse(this.textBox7.Text, out int done) ? done : 0;

            var repo = new ProjectRepository();

            if (this.projectId == 0)
            {
                // Jeigu projektas su tokiu vardu egzistuoja, pridėdam naują dalį
                int existingId = repo.GetProjectIdByName(project.projectname);
                if (existingId > 0)
                {
                    // Tikrina ar tokia dalis egzistuoja, prie pasirinkto projekto 
                    var existingProject = repo.GetProjectWithParts(existingId);
                    if (existingProject?.Parts != null && existingProject.Parts.Any(p => string.Equals((p.partname ?? "").Trim(), (part.partname ?? "").Trim(), StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("A part with that name already exists for the selected project.", "Duplicate Part", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    repo.AddPartToProject(existingId, part);
                }
   
                else
                {
                    // Egzistuojantis projektas nepasirinktas, kuria naują
                    repo.AddProjectWithPart(project, part);
                }
            }

            this.DialogResult = DialogResult.OK;
            
        }

        // Mygtukas CANCEL
        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

    }
}
