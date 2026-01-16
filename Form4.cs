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
    public partial class Form4 : Form
    {
        public Form4()
        {
            InitializeComponent();

            this.DialogResult = DialogResult.Cancel;

            // Prijungti įvykius, kad pasikeitus projektui būtų užpildytos dalys
        }

        // Užkrauna projektų vardus, kad pridėti dalių

        private int projectId = 0;
        private int partId = 0; // keep track of the edited part id

        // Laukelių priskirimas prie DB reikšmių
        public void EditProjectWithPart(Project project, Part part)
        {
            this.Text = "Edit Project";

            this.comboBox2.Text = project.projectname;
            this.comboBox3.Text = part.partname;
            // remember ids so Save updates existing record
            this.projectId = project.id;
            this.partId = part?.id ?? 0;

            // disable editing of project and part names - only certain fields should be editable
            this.comboBox2.Enabled = false;
            this.comboBox3.Enabled = false;
            
            // make sure only allowed fields are editable: Made By (textBox3), Type of Work (comboBox1), Comments (textBox5), Remaining (textBox6), Done (textBox7)
            this.comboBox1.Enabled = true;
            this.textBox3.ReadOnly = false;
            this.textBox5.ReadOnly = false;
            this.textBox6.ReadOnly = false;
            this.textBox7.ReadOnly = false;

            this.textBox3.Text = part.madeby;
            this.comboBox1.Text = part.typeofwork;
            this.textBox5.Text = part.comments;
            this.textBox6.Text = Convert.ToString(part.remaining);
            this.textBox7.Text = Convert.ToString(part.done);
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

            // ensure we update the existing part rather than inserting a new one
            part.id = this.partId;
            part.project_id = this.projectId;
            
            var repo = new ProjectRepository();
            
           // Redagoja pasirinkta projekta
           repo.UpdateProjectWithPart(project, part);

           // taip pat atnaujinti 'remaining' visoms to pacio partname dalims projekte
           try
           {
               repo.UpdateRemainingForPartNameInProject(this.projectId, part.partname, part.remaining);
           }
           catch
           {
               // ignore errors updating remaining across project
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
