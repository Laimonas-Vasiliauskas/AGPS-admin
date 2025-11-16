using AGPSadmin.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace AGPSadmin.Repositories
{
    public class ProjectRepository
    {
        private readonly string connectionString = "Data Source=localhost\\SQLEXPRESS;Initial Catalog=AGPSdb;Integrated Security=True;";

        public List<Project> GetProjects()
        {
            var projects = new List<Project>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * From projects ORDER BY id DESC";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Project project = new Project();

                            project.id = Convert.ToInt32(reader["id"]);
                            project.projectname = Convert.ToString(reader["projectname"]);
                            project.partname = Convert.ToString(reader["partname"]);
                            project.madeby = Convert.ToString(reader["madeby"]);
                            project.typeofwork = Convert.ToString(reader["typeofwork"]);
                            project.created_at = Convert.ToString(reader["created_at"]);
                            project.comments = Convert.ToString(reader["comments"]);
                            project.isChecked = Convert.ToString(reader["isChecked"]);

                            projects.Add(project);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while retrieving projects: " + ex.Message);
            }

            return projects;
        }

        public Project GetProject(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT * FROM projects WHERE id = @id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Project project = new Project();
                                project.id = reader.GetInt32(0);
                                project.projectname = reader.GetString(1);
                                project.partname = reader.GetString(2);
                                project.madeby = reader.GetString(3);
                                project.typeofwork = reader.GetString(4);
                                project.created_at = reader.GetDateTime(5).ToString();
                                project.comments = reader.GetString(6);
                                project.isChecked = reader.GetString(7);
                                return project;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while retrieving project: " + ex.Message);
            }

            return null;
        }

        public void AddProject(Project project)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "INSERT INTO projects (projectname, partname, madeby, typeofwork, created_at, comments, isChecked) " +
                                 "VALUES (@projectname, @partname, @madeby, @typeofwork, @created_at, @comments, @isChecked)";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@projectname", project.projectname);
                        command.Parameters.AddWithValue("@partname", project.partname);
                        command.Parameters.AddWithValue("@madeby", project.madeby);
                        command.Parameters.AddWithValue("@typeofwork", project.typeofwork);
                        command.Parameters.AddWithValue("@created_at", DateTime.Now);
                        command.Parameters.AddWithValue("@comments", project.comments);
                        command.Parameters.AddWithValue("@isChecked", project.isChecked);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while adding project: " + ex.Message);
            }
        }

        public void UpdateProject(Project project)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "UPDATE projects SET projectname = @projectname, partname = @partname, madeby = @madeby, " +
                                 "typeofwork = @typeofwork, comments = @comments, isChecked = @isChecked WHERE id = @id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@projectname", project.projectname);
                        command.Parameters.AddWithValue("@partname", project.partname);
                        command.Parameters.AddWithValue("@madeby", project.madeby);
                        command.Parameters.AddWithValue("@typeofwork", project.typeofwork);
                        command.Parameters.AddWithValue("@comments", project.comments);
                        command.Parameters.AddWithValue("@isChecked", project.isChecked);
                        command.Parameters.AddWithValue("@id", project.id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while updating project: " + ex.Message);
            }
        }

        public void DeleteProject(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "DELETE FROM projects WHERE id = @id";
                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@id", id);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while deleting project: " + ex.Message);
            }
        }

        public List<string> GetProjectNames(string projectName)
        {
            var result = new List<string>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT projectname FROM projects WHERE (@projectname IS NULL OR @projectname = '') OR projectname LIKE '%' + @projectname + '%';";

                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@projectname", projectName);

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            result.Add(reader["projectname"].ToString());
                    }
                }
            }

            return result;
        }

        public DataTable GetProjectTable(string projectName)
        {
            DataTable table = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sql = "SELECT * FROM projects WHERE (@projectname IS NULL OR @projectname = '') OR projectname = @projectname";

                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@projectname", projectName);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(table);
                    }
                }
            }

            return table;
        }
    }
}
