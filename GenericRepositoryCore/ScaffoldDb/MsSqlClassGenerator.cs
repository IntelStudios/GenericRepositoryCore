using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Reflection;

namespace GenericRepositoryCore.ScaffoldDb
{
    public static class MsSqlClassGenerator
    {
        private static string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static string outputFolder = Path.Combine(new DirectoryInfo(baseDirectory).Parent.Parent.Parent.FullName, "Model"); // Project.Model

        public static void CreateClasses(string connectionString, string databaseName, string nameSpace = null, string outputFolder = null) 
        {
            if (!string.IsNullOrEmpty(outputFolder))
            {
                MsSqlClassGenerator.outputFolder = outputFolder;
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string procs = GetQuerry(Assembly.GetExecutingAssembly(), "GenericRepositoryCore.ScaffoldDb.Proc_mssql_classgenerator.sql");
                    string drop = GetQuerry(Assembly.GetExecutingAssembly(), "GenericRepositoryCore.ScaffoldDb.Drop_mssql_generator.sql");

                    //create procedures
                    CreateOrDropProcedure(conn, procs);

                    //get DataTransferObjects
                    List<DbTable> orm = GetDto(conn, databaseName);
                    if (orm.Count != 0)
                    {
                        AddNamespaces(orm, nameSpace);
                        CreateFilesWithClases(orm);
                    }

                    //drop procedures
                    CreateOrDropProcedure(conn, drop);
                }
            }
            catch (Exception ex)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    CreateOrDropProcedure(conn, GetQuerry(Assembly.GetExecutingAssembly(), "GenericRepositoryCore.ScaffoldDb.Drop_mssql_generator.sql"));
                }
                Console.WriteLine(ex.Message);
            }
        }

        private static void AddNamespaces(List<DbTable> orm, string nameSpace = null)
        {
            string assemblyName;
            if (nameSpace is null)
            {
                assemblyName = Assembly.GetEntryAssembly().GetName().Name;
            }else
            {
                assemblyName = nameSpace;
            }

            string ModelNamespaces = "namespace " + assemblyName + ".Model";
            string RepositoriesNamespaces = "namespace " + assemblyName + ".Repositories";
            foreach (var x in orm)
            {
                x.Class = x.Class.Replace("%namespace%", ModelNamespaces);
                x.Repository = x.Repository.Replace("%namespace%", RepositoriesNamespaces);
                x.Repository = x.Repository.Replace("%usingToModel%", "using " + assemblyName + ".Model;");
            }        
        }

        private static void CreateFilesWithClases(List<DbTable> orm)
        {
            DirectoryInfo Model = new DirectoryInfo(outputFolder);
            //DirectoryInfo Repositories = new DirectoryInfo(outputFolder);
            if (!Model.Exists) Model.Create();
            //if (!Repositories.Exists) Repositories.Create();

            foreach (DbTable item in orm)
            {
                File.WriteAllText(Path.Combine(Model.FullName, item.TableName + ".cs"), item.Class);
                //File.WriteAllText(Path.Combine(Repositories.FullName, item.TableName + ".cs"), item.Repository);
            }
        }
        private static List<DbTable> GetDto(SqlConnection conn, string databaseName)
        {
            List<DbTable> dto = new List<DbTable>();
            using (SqlCommand commExecProc = new SqlCommand("PrintDtoForDatabase", conn))
            {
                commExecProc.CommandType = System.Data.CommandType.StoredProcedure;
                commExecProc.Parameters.AddWithValue("@p_DbName", databaseName);

                using (SqlDataReader reader = commExecProc.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int i = -1;
                        DbTable tmp = new DbTable();
                        tmp.TableName = reader.GetString(++i);
                        tmp.Class = reader.IsDBNull(++i) ? "non implemented" : reader.GetString(i);
                        tmp.Repository = reader.IsDBNull(++i) ? "non implemented" : reader.GetString(i);
                        dto.Add(tmp);
                    }
                }
            }
            return dto;
        }

        private static void CreateOrDropProcedure(SqlConnection conn, string procs)
        {
            string[] allObjToQuerry = procs.Split("GO", StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in allObjToQuerry)
            {
                using (SqlCommand commCreateProc = new SqlCommand(item, conn))
                {
                    commCreateProc.ExecuteNonQuery();
                }
            }
        }

        private static string GetQuerry(Assembly assembly, string namespacePath)
        {
            string output = "";
            using (StreamReader sr = new StreamReader(
               assembly.GetManifestResourceStream(
               namespacePath)
               ))
            {
                output = sr.ReadToEnd();
            }
            return output;
        }       
    }
}
