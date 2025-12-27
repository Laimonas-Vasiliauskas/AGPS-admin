using AGPSadmin.Models;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace AGPSadmin.Repositories
{
    public class ProjectRepository
    {
        private readonly string connectionString;

        public ProjectRepository()
        {
            string raw = ConfigurationManager.ConnectionStrings["AGPStestDB"].ConnectionString;

            string pwd = Environment.GetEnvironmentVariable("AGPSDB_PASSWORD");

            connectionString = raw.Replace("{PWD}", pwd);
        }

        public List<Project> GetProjectsWithParts()
        {
            var projects = new Dictionary<int, Project>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
                SELECT 
                    p.id AS ProjectId,
                    p.projectname,
                    pa.id AS PartId,
                    pa.project_id,
                    pa.partname,
                    pa.madeby,
                    pa.typeofwork,
                    pa.created_at,
                    pa.comments,
                    pa.remaining,
                    pa.done
                FROM projects p
                LEFT JOIN parts pa ON pa.project_id = p.id
                ORDER BY p.id DESC";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int projectId = reader.GetInt32(reader.GetOrdinal("ProjectId"));

                            // create project once
                            if (!projects.TryGetValue(projectId, out Project project))
                            {
                                project = new Project
                                {
                                    id = projectId,
                                    projectname = reader.GetString(reader.GetOrdinal("projectname"))
                                };
                                projects.Add(projectId, project);
                            }

                            // add part if exists
                            if (!reader.IsDBNull(reader.GetOrdinal("PartId")))
                            {
                                project.Parts.Add(new Part
                                {
                                    id = reader.GetInt32(reader.GetOrdinal("PartId")),
                                    project_id = projectId,
                                    partname = reader.GetString(reader.GetOrdinal("partname")),
                                    madeby = reader.GetString(reader.GetOrdinal("madeby")),
                                    typeofwork = reader.GetString(reader.GetOrdinal("typeofwork")),
                                    created_at = reader.GetDateTime(reader.GetOrdinal("created_at")),
                                    comments = reader.GetString(reader.GetOrdinal("comments")),
                                    remaining = reader.GetInt32(reader.GetOrdinal("remaining")),
                                    done = reader.GetInt32(reader.GetOrdinal("done"))
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while retrieving projects with parts: " + ex.Message);
            }

            return projects.Values.ToList();
        }


        public Project GetProjectWithParts(int id)
        {
            Project project = null;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
                SELECT 
                    p.id AS ProjectId,
                    p.projectname,
                    pa.id AS PartId,
                    pa.project_id,
                    pa.partname,
                    pa.madeby,
                    pa.typeofwork,
                    pa.created_at,
                    pa.comments,
                    pa.remaining,
                    pa.done
                FROM projects p
                LEFT JOIN parts pa ON pa.project_id = p.id
                WHERE p.id = @id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.Add("@id", SqlDbType.Int).Value = id;

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // create project once
                                if (project == null)
                                {
                                    project = new Project
                                    {
                                        id = reader.GetInt32(reader.GetOrdinal("ProjectId")),
                                        projectname = reader.GetString(reader.GetOrdinal("projectname"))
                                    };
                                }

                                // add part if exists
                                if (!reader.IsDBNull(reader.GetOrdinal("PartId")))
                                {
                                    project.Parts.Add(new Part
                                    {
                                        id = reader.GetInt32(reader.GetOrdinal("PartId")),
                                        project_id = project.id,
                                        partname = reader.GetString(reader.GetOrdinal("partname")),
                                        madeby = reader.GetString(reader.GetOrdinal("madeby")),
                                        typeofwork = reader.GetString(reader.GetOrdinal("typeofwork")),
                                        created_at = reader.GetDateTime(reader.GetOrdinal("created_at")),
                                        comments = reader.GetString(reader.GetOrdinal("comments")),
                                        remaining = reader.GetInt32(reader.GetOrdinal("remaining")),
                                        done = reader.GetInt32(reader.GetOrdinal("done"))
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while retrieving project with parts: " + ex.Message);
            }

            return project;
        }

        public void AddProjectWithPart(Project project, Part part)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1️⃣ Insert Project
                        string insertProjectSql =
                            "INSERT INTO projects (projectname) " +
                            "OUTPUT INSERTED.id " +
                            "VALUES (@projectname)";

                        int projectId;
                        using (SqlCommand cmd = new SqlCommand(insertProjectSql, connection, transaction))
                        {
                            cmd.Parameters.Add("@projectname", SqlDbType.NVarChar)
                                          .Value = project.projectname;

                            projectId = (int)cmd.ExecuteScalar();
                        }

                        // 2️⃣ Insert Part
                        string insertPartSql =
                            @"INSERT INTO parts 
                      (project_id, partname, madeby, typeofwork, created_at, comments, remaining, done)
                      VALUES
                      (@project_id, @partname, @madeby, @typeofwork, @created_at, @comments, @remaining, @done)";

                        using (SqlCommand cmd = new SqlCommand(insertPartSql, connection, transaction))
                        {
                            cmd.Parameters.Add("@project_id", SqlDbType.Int).Value = projectId;
                            cmd.Parameters.Add("@partname", SqlDbType.NVarChar).Value = part.partname;
                            cmd.Parameters.Add("@madeby", SqlDbType.NVarChar).Value = part.madeby;
                            cmd.Parameters.Add("@typeofwork", SqlDbType.NVarChar).Value = part.typeofwork;
                            cmd.Parameters.Add("@created_at", SqlDbType.DateTime).Value = DateTime.Now;
                            cmd.Parameters.Add("@comments", SqlDbType.NVarChar).Value = part.comments ?? "";
                            cmd.Parameters.Add("@remaining", SqlDbType.Int).Value = part.remaining;
                            cmd.Parameters.Add("@done", SqlDbType.Int).Value = part.done;

                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public void UpdateProjectWithPart(Project project, Part part)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // 1️⃣ Update Project
                        string updateProjectSql =
                            "UPDATE projects SET projectname = @projectname WHERE id = @id";

                        using (SqlCommand cmd = new SqlCommand(updateProjectSql, connection, transaction))
                        {
                            cmd.Parameters.Add("@projectname", SqlDbType.NVarChar)
                                          .Value = project.projectname;
                            cmd.Parameters.Add("@id", SqlDbType.Int)
                                          .Value = project.id;
                            cmd.ExecuteNonQuery();
                        }

                        // 2️⃣ Update Part
                        string updatePartSql =
                            @"UPDATE parts SET
                        partname = @partname,
                        madeby = @madeby,
                        typeofwork = @typeofwork,
                        comments = @comments,
                        remaining = @remaining,
                        done = @done
                      WHERE id = @partId";

                        using (SqlCommand cmd = new SqlCommand(updatePartSql, connection, transaction))
                        {
                            cmd.Parameters.Add("@partname", SqlDbType.NVarChar).Value = part.partname;
                            cmd.Parameters.Add("@madeby", SqlDbType.NVarChar).Value = part.madeby;
                            cmd.Parameters.Add("@typeofwork", SqlDbType.NVarChar).Value = part.typeofwork;
                            cmd.Parameters.Add("@comments", SqlDbType.NVarChar).Value = part.comments ?? "";
                            cmd.Parameters.Add("@remaining", SqlDbType.Int).Value = part.remaining;
                            cmd.Parameters.Add("@done", SqlDbType.Int).Value = part.done;
                            cmd.Parameters.Add("@partId", SqlDbType.Int).Value = part.id;

                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
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
                string sql = "SELECT DISTINCT projectname FROM projects WHERE (@projectname IS NULL OR @projectname = '') OR projectname LIKE '%' + @projectname + '%';";

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
        public void AddWorkAndUpdateRemainingForAll(int rowId, string projectName, string partName, int doneDelta)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlTransaction tx = connection.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd1 = new SqlCommand(@"UPDATE projects SET done = done + @delta WHERE id = @id;", connection, tx))
                        {
                            cmd1.Parameters.AddWithValue("@delta", doneDelta);
                            cmd1.Parameters.AddWithValue("@id", rowId);
                            cmd1.ExecuteNonQuery();
                        }
                        using (SqlCommand cmd2 = new SqlCommand(@"UPDATE projects SET remaining = CASE WHEN remaining - @delta < 0 THEN 0 ELSE remaining - @delta END WHERE projectname = @projectname AND partname = @partname;", connection, tx))
                        {
                            cmd2.Parameters.AddWithValue("@delta", doneDelta);
                            cmd2.Parameters.AddWithValue("@projectname", projectName);
                            cmd2.Parameters.AddWithValue("@partname", partName);
                            cmd2.ExecuteNonQuery();
                        }
                        tx.Commit();
                    }
                    catch
                    {
                        tx.Rollback();
                        throw;
                    }
                }
            }
        }

    }
}
