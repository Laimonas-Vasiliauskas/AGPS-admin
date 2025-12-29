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
        }

        // Užkrauna projektų vardus, kad pridėti dalių
        private void LoadProjects()
        {
            try
            {
                var repo = new ProjectRepository();
                var names = repo.GetProjectNames("");

                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(names.ToArray());
                comboBox2.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                comboBox2.AutoCompleteSource = AutoCompleteSource.ListItems;
            }
            catch
            {
                
            }
        }

        private int projectId = 0;

        // Laukelių priskirimas prie DB reikšmių
        public void EditProjectWithPart(Project project, Part part)
        {
            this.Text = "Edit Project";

            this.label9.Text = "" + project.id;
            this.comboBox2.Text = project.projectname;
            this.comboBox3.Text = part.partname;
            this.textBox3.Text = part.madeby;
            this.comboBox1.Text = part.typeofwork;
            this.textBox5.Text = part.comments;
            this.textBox6.Text = Convert.ToString(part.remaining);
            this.textBox7.Text = Convert.ToString(part.done);

            this.projectId = project.id;
        }

        public void AddProject(Project project)
        {
            this.Text = "Add Project";

            this.label9.Text = "" + project.id;
            this.comboBox2.Text = project.projectname;
        }

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
                    repo.AddPartToProject(existingId, part);
                }
   
                else
                {
                    // Egzistuojantis projektas nepasirinktas, kuria naują
                    repo.AddProjectWithPart(project, part);
                }
            }
            else
            {
                // Redagoja pasirinkta projektą
                repo.UpdateProjectWithPart(project, part);
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
