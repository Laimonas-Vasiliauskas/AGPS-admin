using AGPSadmin.Models;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace AGPSadmin.Repositories
{
    public class ProjectRepository
    {
        private readonly string connectionString;

        public ProjectRepository()
        {
            string raw = ConfigurationManager.ConnectionStrings["AGPSdb"].ConnectionString;

            string pwd = Environment.GetEnvironmentVariable("AGPSDB_PASSWORD");

            connectionString = raw.Replace("{PWD}", pwd);
        }

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
                            project.remaining = Convert.ToInt32(reader["remaining"]);
                            project.done = Convert.ToInt32(reader["done"]);

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
                                project.remaining = reader.GetInt32(7);
                                project.done = reader.GetInt32(8);
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
                    string sql = "INSERT INTO projects (projectname, partname, madeby, typeofwork, created_at, comments, remaining, done) " +
                                 "VALUES (@projectname, @partname, @madeby, @typeofwork, @created_at, @comments, @remaining, @done)";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@projectname", project.projectname);
                        command.Parameters.AddWithValue("@partname", project.partname);
                        command.Parameters.AddWithValue("@madeby", project.madeby);
                        command.Parameters.AddWithValue("@typeofwork", project.typeofwork);
                        command.Parameters.AddWithValue("@created_at", DateTime.Now);
                        command.Parameters.AddWithValue("@comments", project.comments);
                        command.Parameters.AddWithValue("@remaining", project.remaining);
                        command.Parameters.AddWithValue("@done", project.done);
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
                                 "typeofwork = @typeofwork, comments = @comments, remaining = @remaining, done = @done WHERE id = @id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@projectname", project.projectname);
                        command.Parameters.AddWithValue("@partname", project.partname);
                        command.Parameters.AddWithValue("@madeby", project.madeby);
                        command.Parameters.AddWithValue("@typeofwork", project.typeofwork);
                        command.Parameters.AddWithValue("@comments", project.comments);
                        command.Parameters.AddWithValue("@remaining", project.remaining);
                        command.Parameters.AddWithValue("@done", project.done);
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
        public void ExportToExcel(string filePath, string projectName)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
            SELECT 
                id AS [ID],
                projectname AS [Project Name],
                partname AS [Part Name],
                madeby AS [Made By],
                typeofwork AS [Type Of Work],
                created_at AS [Created At],
                comments AS [Comments],
                remaining AS [Remaining],
                done AS [Done]
            FROM projects
            WHERE projectname = @projectName
            ORDER BY id DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@projectName", projectName);

                    using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                    {
                        da.Fill(dt);
                    }
                }
            }

            if (dt.Rows.Count == 0)
            {
                MessageBox.Show($"No rows found for project '{projectName}'");
                return;
            }

            using (var excel = new OfficeOpenXml.ExcelPackage())
            {
                var ws = excel.Workbook.Worksheets.Add("Project");
                ws.Cells["A1"].LoadFromDataTable(dt, true);
                ws.Cells.AutoFitColumns();
                using (var headerRange = ws.Cells[1, 1, 1, dt.Columns.Count])
                {
                    headerRange.Style.Font.Bold = true;
                }
                ws.View.FreezePanes(2, 1);
                ws.Column(6).Width = 20;
                ws.Column(6).Style.Numberformat.Format = "yyyy-MM-dd HH:mm";
                File.WriteAllBytes(filePath, excel.GetAsByteArray());
            }
        }
        public void ImportExcelToSql(string filePath)
        {
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets[0];
                int rows = ws.Dimension.Rows;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    for (int row = 2; row <= rows; row++) 
                    {
                        string projectName = ws.Cells[row, 2].Text;
                        string partName = ws.Cells[row, 3].Text;
                        string madeBy = ws.Cells[row, 4].Text;
                        string typeOfWork = ws.Cells[row, 5].Text;
                        DateTime createdAt = DateTime.Parse(ws.Cells[row, 6].Text);
                        string comments = ws.Cells[row, 7].Text;
                        string remaining = ws.Cells[row, 8].Text;
                        string done = ws.Cells[row, 9].Text;

                        string sql = @"INSERT INTO projects
                               (projectname, partname, madeby, typeofwork, created_at, comments, remaining, done)
                               VALUES (@p1, @p2, @p3, @p4, @p5, @p6, @p7, @p8)";

                        using (SqlCommand cmd = new SqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@p1", projectName);
                            cmd.Parameters.AddWithValue("@p2", partName);
                            cmd.Parameters.AddWithValue("@p3", madeBy);
                            cmd.Parameters.AddWithValue("@p4", typeOfWork);
                            cmd.Parameters.AddWithValue("@p5", createdAt);
                            cmd.Parameters.AddWithValue("@p6", comments);
                            cmd.Parameters.AddWithValue("@p7", remaining);
                            cmd.Parameters.AddWithValue("@p8", done);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }

    }
}
