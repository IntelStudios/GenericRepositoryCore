using GenericRepositoryCore.ScaffoldDb;
using System;

namespace testORM
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing class and repository generation");

            string databaseName = "xeelo-tests-gr-autoproperties-2020-02-14-08-30-36-1651";
            MsSqlClassGenerator.CreateClasses($"Server=(localdb)\\mssqllocaldb;database={databaseName};Trusted_Connection=True;", databaseName);

            Console.WriteLine("Generation completed");
        }
    }
}
