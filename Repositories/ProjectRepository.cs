using AGPSadmin.Models;
using OfficeOpenXml;
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

        // Prisijungimas prie DB
        public ProjectRepository()
        {
            string raw = ConfigurationManager.ConnectionStrings["AGPStestDB"].ConnectionString;

            string pwd = Environment.GetEnvironmentVariable("AGPSDB_PASSWORD");

            connectionString = raw.Replace("{PWD}", pwd);
        }

        // Metodas gauti duomenis iš DB
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

                            // Pridėti dalį, jeigu egzistuoja
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

        // Metodas grąžina viena projekto pagal jo ID su visomis jo dalimis
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

        // Metodas įrašo nauja projekta į DB
        public void AddProjectWithPart(Project project, Part part)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Įrašo projekta
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

                        // Įrašo dalį
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

        // Metodas pridėda prie projekto papildomų dalių
        public void AddPartToProject(int project_id, Part part)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        string insertPartSql =
                            @"INSERT INTO parts 
                      (project_id, partname, madeby, typeofwork, created_at, comments, remaining, done)
                      VALUES
                      (@project_id, @partname, @madeby, @typeofwork, @created_at, @comments, @remaining, @done)";

                        using (SqlCommand cmd = new SqlCommand(insertPartSql, connection, transaction))
                        {
                            cmd.Parameters.Add("@project_id", SqlDbType.Int).Value = project_id;
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
        // Metodas atnaujina projektą
        public void UpdateProjectWithPart(Project project, Part part)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Atnaujina projektą
                        string updateProjectSql =
                            "UPDATE projects SET projectname = @projectname WHERE id = @id";

                        using (SqlCommand cmd = new SqlCommand(updateProjectSql, connection, transaction))
                        {
                            cmd.Parameters.Add("@projectname", System.Data.SqlDbType.NVarChar).Value = project.projectname ?? string.Empty;
                            cmd.Parameters.Add("@id", System.Data.SqlDbType.Int).Value = project.id;
                            cmd.ExecuteNonQuery();
                        }

                        // Jeigu nėra dalių, neturi ką daryti 
                        if (part == null)
                        {
                            transaction.Commit();
                            return;
                        }

                        // 2️⃣ If part.id == 0 -> insert new part, otherwise update existing part
                        if (part.id == 0)
                        {
                            string insertPartSql =
                                @"INSERT INTO parts
                                (project_id, partname, madeby, typeofwork, created_at, comments, remaining, done)
                                VALUES
                                (@project_id, @partname, @madeby, @typeofwork, @created_at, @comments, @remaining, @done)";

                            using (SqlCommand cmd = new SqlCommand(insertPartSql, connection, transaction))
                            {
                                cmd.Parameters.AddWithValue("@project_id", project.id);
                                cmd.Parameters.AddWithValue("@partname", (object)part.partname ?? string.Empty);
                                cmd.Parameters.AddWithValue("@madeby", (object)part.madeby ?? string.Empty);
                                cmd.Parameters.AddWithValue("@typeofwork", (object)part.typeofwork ?? string.Empty);
                                cmd.Parameters.AddWithValue("@created_at", part.created_at == default(DateTime) ? DateTime.Now : part.created_at);
                                cmd.Parameters.AddWithValue("@comments", (object)part.comments ?? string.Empty);
                                cmd.Parameters.AddWithValue("@remaining", part.remaining);
                                cmd.Parameters.AddWithValue("@done", part.done);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        else
                        {
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
                                cmd.Parameters.AddWithValue("@partname", (object)part.partname ?? string.Empty);
                                cmd.Parameters.AddWithValue("@madeby", (object)part.madeby ?? string.Empty);
                                cmd.Parameters.AddWithValue("@typeofwork", (object)part.typeofwork ?? string.Empty);
                                cmd.Parameters.AddWithValue("@comments", (object)part.comments ?? string.Empty);
                                cmd.Parameters.AddWithValue("@remaining", part.remaining);
                                cmd.Parameters.AddWithValue("@done", part.done);
                                cmd.Parameters.AddWithValue("@partId", part.id);

                                cmd.ExecuteNonQuery();
                            }
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
        // Metodas ištrina projekto dalis, po to projektą
        public void DeletePartOrProject(int partId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlTransaction tx = connection.BeginTransaction())
                {
                    try
                    {
                        int projectId;

                        // 1️⃣ Gauti project_id pagal part.id
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT project_id FROM parts WHERE id = @partId",
                            connection, tx))
                        {
                            cmd.Parameters.AddWithValue("@partId", partId);
                            var result = cmd.ExecuteScalar();

                            if (result == null)
                                return; // tokios dalies nėra

                            projectId = (int)result;
                        }

                        // 2️⃣ Ištrinti dalį
                        using (SqlCommand cmd = new SqlCommand(
                            "DELETE FROM parts WHERE id = @partId",
                            connection, tx))
                        {
                            cmd.Parameters.AddWithValue("@partId", partId);
                            cmd.ExecuteNonQuery();
                        }

                        // 3️⃣ Patikrinti ar liko dalių projekte
                        int remainingParts;
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT COUNT(*) FROM parts WHERE project_id = @projectId",
                            connection, tx))
                        {
                            cmd.Parameters.AddWithValue("@projectId", projectId);
                            remainingParts = (int)cmd.ExecuteScalar();
                        }

                        // 4️⃣ Jei dalių nebeliko – trinti projektą
                        if (remainingParts == 0)
                        {
                            using (SqlCommand cmd = new SqlCommand(
                                "DELETE FROM projects WHERE id = @projectId",
                                connection, tx))
                            {
                                cmd.Parameters.AddWithValue("@projectId", projectId);
                                cmd.ExecuteNonQuery();
                            }
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
        // Metodas grąžina unikalius projektų pavadinimus
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

        // Metodas grąžina DataTable
        public DataTable GetProjectTable(string projectName)
        {
            var table = new DataTable();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = @"
            SELECT 
                p.id AS [ID],
                p.projectname AS [Project Name],
                pa.id AS [PartId],
                pa.partname AS [Part Name],
                pa.madeby AS [Made By],
                pa.typeofwork AS [Type of Work],
                pa.created_at AS [Date],
                pa.comments AS [Comments],
                pa.remaining AS [Remaining],
                pa.done AS [Done]
            FROM projects p
            LEFT JOIN parts pa ON pa.project_id = p.id
            WHERE (@projectname IS NULL OR @projectname = '') OR p.projectname = @projectname
            ORDER BY p.id DESC";
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

        // Metodas eksportuoja vieno projekto duomenis į Excel
        public void ExportToExcel(string filePath, string projectName)
        {
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string sql = @"
            SELECT 
                p.id AS [ID],
                p.projectname AS [Project Name],
                pa.partname AS [Part Name],
                pa.madeby AS [Made By],
                pa.typeofwork AS [Type Of Work],
                pa.created_at AS [Created At],
                pa.comments AS [Comments],
                pa.remaining AS [Remaining],
                pa.done AS [Done]
            FROM projects p
            LEFT JOIN parts pa ON pa.project_id = p.id
            WHERE projectname = @projectname
            ORDER BY id DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@projectname", projectName);

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

        // Metodas importuoja duomenis iš Excel į DB
        public void ImportExcelToSql(string filePath)
        {
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets[0];
                int rows = ws.Dimension?.Rows ?? 0;

                if (rows < 2)
                    return;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    for (int row = 2; row <= rows; row++) 
                    {
                        string projectName = ws.Cells[row, 2].Text?.Trim();
                        if (string.IsNullOrEmpty(projectName))
                            continue; // skip rows without a project name

                        string partName = ws.Cells[row, 3].Text?.Trim();
                        string madeBy = ws.Cells[row, 4].Text?.Trim();
                        string typeOfWork = ws.Cells[row, 5].Text?.Trim();

                        DateTime createdAt;
                        if (!DateTime.TryParse(ws.Cells[row, 6].Text, out createdAt))
                            createdAt = DateTime.Now;

                        string comments = ws.Cells[row, 7].Text?.Trim();

                        int remaining = 0;
                        int.TryParse(ws.Cells[row, 8].Text, out remaining);

                        int done = 0;
                        int.TryParse(ws.Cells[row, 9].Text, out done);

                        // Ensure project exists (create if not) and get project id
                        int projectId = 0;
                        using (SqlCommand cmdFind = new SqlCommand("SELECT id FROM projects WHERE projectname = @name", conn))
                        {
                            cmdFind.Parameters.AddWithValue("@name", projectName);
                            var scalar = cmdFind.ExecuteScalar();
                            if (scalar != null && scalar != DBNull.Value)
                                projectId = Convert.ToInt32(scalar);
                        }

                        if (projectId == 0)
                        {
                            using (SqlCommand cmdInsertProject = new SqlCommand("INSERT INTO projects (projectname) OUTPUT INSERTED.id VALUES (@name)", conn))
                            {
                                cmdInsertProject.Parameters.AddWithValue("@name", projectName);
                                projectId = (int)cmdInsertProject.ExecuteScalar();
                            }
                        }

                        // Insert part linked to project
                        string insertPartSql = @"INSERT INTO parts
                               (project_id, partname, madeby, typeofwork, created_at, comments, remaining, done)
                               VALUES (@project_id, @partname, @madeby, @typeofwork, @created_at, @comments, @remaining, @done)";

                        using (SqlCommand cmd = new SqlCommand(insertPartSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@project_id", projectId);
                            cmd.Parameters.AddWithValue("@partname", (object)partName ?? "");
                            cmd.Parameters.AddWithValue("@madeby", (object)madeBy ?? "");
                            cmd.Parameters.AddWithValue("@typeofwork", (object)typeOfWork ?? "");
                            cmd.Parameters.AddWithValue("@created_at", createdAt);
                            cmd.Parameters.AddWithValue("@comments", (object)comments ?? "");
                            cmd.Parameters.AddWithValue("@remaining", remaining);
                            cmd.Parameters.AddWithValue("@done", done);

                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
        // Metodas gauna projekto ID pagal projekto vardą
        public int GetProjectIdByName(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                return 0;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = "SELECT TOP 1 id FROM projects WHERE projectname = @projectname";
                using (SqlCommand cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.AddWithValue("@projectname", projectName);
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        return Convert.ToInt32(result);
                }
            }

            return 0;
        }
         // Metodas atnaujina 'remaining' visoms projekto dalims su tuo paciu partname
         public void UpdateRemainingForPartNameInProject(int projectId, string partName, int newRemaining)
         {
             if (string.IsNullOrWhiteSpace(partName))
                 return;

             using (SqlConnection connection = new SqlConnection(connectionString))
             {
                 connection.Open();
                 string sql = @"UPDATE parts SET remaining = @remaining 
                                WHERE project_id = @projectId 
                                  AND LOWER(LTRIM(RTRIM(ISNULL(partname, '')))) = LOWER(LTRIM(RTRIM(@partName)))";
                 using (SqlCommand cmd = new SqlCommand(sql, connection))
                 {
                     cmd.Parameters.Add(new SqlParameter("@remaining", SqlDbType.Int) { Value = newRemaining });
                     cmd.Parameters.Add(new SqlParameter("@projectId", SqlDbType.Int) { Value = projectId });
                     cmd.Parameters.Add(new SqlParameter("@partName", SqlDbType.NVarChar, 200) { Value = partName.Trim() });
                     cmd.ExecuteNonQuery();
                 }
             }
         }
     }
 }
