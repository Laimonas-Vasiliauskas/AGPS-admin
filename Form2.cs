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
        public void EditProject(Project project)
        {
            this.Text = "Edit Project";
            this.label1.Text = "Edit Project";

            this.label1.Text = "" + project.id;
            this.textBox1.Text = project.projectname;
            this.textBox2.Text = project.partname;
            this.textBox3.Text = project.madeby;
            this.textBox4.Text = project.typeofwork;
            this.textBox5.Text = project.comments;
            this.textBox6.Text = project.isChecked;

            this.projectId = project.id;
        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            Project project = new Project();
            project.id = this.projectId;
            project.projectname = this.textBox1.Text;
            project.partname = this.textBox2.Text;
            project.madeby = this.textBox3.Text;
            project.typeofwork = this.textBox4.Text;
            project.comments = this.textBox5.Text;
            project.isChecked = this.textBox6.Text;

            var repo = new ProjectRepository();

            if (this.projectId == 0)
            {
                repo.AddProject(project);
            }
            else
            {   
                repo.UpdateProject(project);
            }

            this.DialogResult = DialogResult.OK;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
