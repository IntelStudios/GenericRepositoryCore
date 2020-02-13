using GenericRepositoryCore.ScaffoldDb;
using System;

namespace ScaffoldTest
{
    class Program
    {
        static void Main(string[] args)
        {
            string conection = @"Server=.\SQLExpress;AttachDbFilename=C:\Program Files\Microsoft SQL Server\MSSQL14.SQLEXPRESS\MSSQL\DATA\MyEfIdentity.mdf;Database=MyEfIdentity; Trusted_Connection=Yes;";
            
            
            MsSqlClassGenerator.CreateClasses(conection, "MyEfIdentity");


        }
    }
}
