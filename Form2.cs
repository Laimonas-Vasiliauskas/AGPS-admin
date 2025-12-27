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
        }

        private int projectId = 0;
        public void EditProjectWithPart(Project project, Part part)
        {
            this.Text = "Edit Project";

            this.label9.Text = "" + project.id;
            this.textBox1.Text = project.projectname;
            this.textBox2.Text = part.partname;
            this.textBox3.Text = part.madeby;
            this.comboBox1.Text = part.typeofwork;
            this.textBox5.Text = part.comments;

            this.projectId = project.id;
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Project project = new Project();
            Part part = new Part();
            project.id = this.projectId;
            project.projectname = this.textBox1.Text;
            part.partname = this.textBox2.Text;
            part.madeby = this.textBox3.Text;
            part.typeofwork = this.comboBox1.Text;
            part.comments = this.textBox5.Text;
            part.remaining = int.TryParse(this.textBox6.Text, out int remaining) ? remaining : 0;
            part.done = int.TryParse(this.textBox7.Text, out int done) ? done : 0;

            var repo = new ProjectRepository();

            if (this.projectId == 0)
            {
                repo.AddProjectWithPart(project, part);
            }
            else
            {   
                repo.UpdateProjectWithPart(project, part);
            }

            this.DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
