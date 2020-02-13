using GenericRepositoryCore.ScaffoldDb;
using System;

namespace testORM
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            MsSqlClassGenerator.CreateClasses("Server=DESKTOP-E8EIH4G\\SQLEXPRESS;Database=AsarkHome;Trusted_Connection=True;", "AsarkHome");
        }
    }
}
