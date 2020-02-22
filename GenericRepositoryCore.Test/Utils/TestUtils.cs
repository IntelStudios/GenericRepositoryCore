using GenericRepository.Contexts;
using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using GenericRepository.Test.Loggers;
using GenericRepository.Test.Models;
using GenericRepository.Test.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data.SqlClient;
using System.IO;

namespace GenericRepository.Test
{
    public class TestUtils
    {
        public const string conStringEnvVariableName = "XEELO_TESTS_CS";
        public const int TestCollectionSize = 100;
        public const string TestSPVarbinaryData = "1234567890";
        public const string NameFormatString = "Name {0}";
        public static string ConnectionString = Environment.GetEnvironmentVariable(conStringEnvVariableName, EnvironmentVariableTarget.Machine);
        public static DateTime DefaultCreatedDate = new DateTime(2018, 1, 1);

        public static string InitializeDatabase(string dbBaseName)
        {
            if (string.IsNullOrEmpty(ConnectionString))
            {
                Assert.Fail("Tests require defined connection string in environment variable '{0}'.", conStringEnvVariableName);
            }

            string dbName = dbBaseName + "-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-ffff");

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                connection.Open();

                CreateDatabase(dbName, connection);

                InitializeTableAutoProperties(connection);
                InitializeTableJoining(connection);
                InitializeTableJoiningTypes(connection);
                InitializeTablePrimaryKeys(connection);
                InitializeTableBinary(connection);
                InitializeTableStoredProcedures(connection);
                InitializeTablePrimitive(connection);
                InitializeTableMultiID(connection);

                CreateStoredProcedures(connection);
                CreateFunctions(connection);
            }

            return dbName;
        }

        private static void InitializeTableMultiID(SqlConnection connection)
        {
            string cmdStr = @"CREATE TABLE [TestEntityMultiID] (
                                                      [TestEntityMulti1ID]  [int] IDENTITY(1, 1) NOT NULL,
                                                      [TestEntityMulti2ID] [int] NULL
                                                        CONSTRAINT PK_TestEntityMultiID PRIMARY KEY (TestEntityMulti1ID) );";

            SqlCommand cmd = new SqlCommand(cmdStr, connection);

            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create table - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            // populate with test data
            try
            {
                for (int i = 0; i < 10; i++)
                {
                    string insStr = string.Format($"INSERT INTO [TestEntityMultiID] (TestEntityMulti2ID) VALUES ({i + 101})");
                    new SqlCommand(insStr, connection).ExecuteNonQuery();

                }
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not insert data into table - {0}.", GRStringHelpers.GetExceptionString(exc));
            }
        }

        private static void InitializeTablePrimitive(SqlConnection connection)
        {
            string crTestEntityPrimitiveNull = @"CREATE TABLE [TestEntityPrimitiveNullTable] (
                                                      [TestEntityPrimitiveNullID]  [int] IDENTITY(1, 1) NOT NULL,
                                                      [TestEntityPrimitiveNullInt] [int] NULL,
                                                      [TestEntityPrimitiveNullBool] [bit] NULL,
                                                      [TestEntityPrimitiveNullDate] [datetime] NULL,
                                                      [TestEntityPrimitiveNullString] [nvarchar](max) NULL
                                                        CONSTRAINT PK_TestEntityPrimitiveNullTable PRIMARY KEY (TestEntityPrimitiveNullID) );";

            SqlCommand crTestEntityPrimitiveNullCommand = new SqlCommand(crTestEntityPrimitiveNull, connection);

            try
            {
                crTestEntityPrimitiveNullCommand.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create table - {0}.", GRStringHelpers.GetExceptionString(exc));
            }
        }

        private static void InitializeTableBinary(SqlConnection connection)
        {
            string crTestEntityBinary = @"CREATE TABLE [TestEntityBinaryTable] (
                                                [TestEntityBinaryID][int] IDENTITY(1, 1) NOT NULL,
                                                [TestEntityBinaryData] varbinary(max) NULL
                                                    CONSTRAINT PK_TestEntityBinaryID PRIMARY KEY (TestEntityBinaryID) );";

            SqlCommand crTestEntityBinaryCommand = new SqlCommand(crTestEntityBinary, connection);

            try
            {
                crTestEntityBinaryCommand.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create table - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            // populate with test data
            try
            {
                string insertTestEntityBinary = string.Format(@"INSERT INTO [TestEntityBinaryTable] (TestEntityBinaryData) VALUES (convert(varbinary, '{0}'))", TestSPVarbinaryData);
                new SqlCommand(insertTestEntityBinary, connection).ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not insert data into table - {0}.", GRStringHelpers.GetExceptionString(exc));
            }
        }

        private static void InitializeTableStoredProcedures(SqlConnection connection)
        {
            // create test table 3
            string crE3Table = @"CREATE TABLE [TestEntity3Table] (
                                                [TestEntity3ID][int] IDENTITY(1, 1) NOT NULL,
                                                [TestEntity3Binary] varbinary(max) NOT NULL
                                                    CONSTRAINT PR_TestEntity3ID PRIMARY KEY (TestEntity3ID) );";

            SqlCommand crE3TableCommand = new SqlCommand(crE3Table, connection);

            try
            {
                crE3TableCommand.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create table - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            // populate with test data
            try
            {
                string insertE3 = string.Format(@"INSERT INTO [TestEntity3Table] (TestEntity3Binary) VALUES (convert(varbinary, '{0}'))", TestSPVarbinaryData);
                new SqlCommand(insertE3, connection).ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not insert data into table - {0}.", GRStringHelpers.GetExceptionString(exc));
            }
        }

        private static void CreateStoredProcedures(SqlConnection connection)
        {
            // create test procedure sp1
            string crSP1 = @"CREATE PROCEDURE [dbo].[spGetTestEntityAutoProperties]
                                 (
                                     @TestEntityAutoPropertiesID int = null
                                 )
                                 AS
                                 select 
	                                  t.TestEntityAutoPropertiesID as [id]
	                                 ,t.TestEntityAutoPropertiesName as [name]
	                                 ,t.TestEntityAutoPropertiesOrder as [order]                                     
                                 from [dbo].[TestEntityAutoPropertiesTable] as t
                                 where (@TestEntityAutoPropertiesID IS NULL OR (t.TestEntityAutoPropertiesID = @TestEntityAutoPropertiesID))";

            SqlCommand crSP1Command = new SqlCommand(crSP1, connection);

            try
            {
                crSP1Command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create stored procedure - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            // create test procedure sp2
            string crSP2 = @"CREATE PROCEDURE [dbo].[spGetTestEntityAutoPropertiesWithReturningCount]
                                 (
                                     @TestEntityAutoPropertiesID int = null
                                    ,@EntryCount int OUTPUT
                                 )
                                 AS
                                 begin
                                     select @EntryCount = count(t.[TestEntityAutoPropertiesID])
                                     from [dbo].[TestEntityAutoPropertiesTable] as t
                                     where (@TestEntityAutoPropertiesID IS NULL OR (t.TestEntityAutoPropertiesID = @TestEntityAutoPropertiesID))

                                     select 
	                                      t.TestEntityAutoPropertiesID as [id]
	                                     ,t.TestEntityAutoPropertiesName as [name]
	                                     ,t.TestEntityAutoPropertiesOrder as [order]                                     
                                     from [dbo].[TestEntityAutoPropertiesTable] as t
                                     where (@TestEntityAutoPropertiesID IS NULL OR (t.TestEntityAutoPropertiesID = @TestEntityAutoPropertiesID))
                                 end";

            SqlCommand crSP2Command = new SqlCommand(crSP2, connection);

            try
            {
                crSP2Command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create stored procedure - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            // create test procedure sp3
            string crSP3 = @"CREATE PROCEDURE [dbo].[spGetStream]
                                 AS
                                 begin
                                     select 
	                                      convert(varbinary(max), t.TestEntity3Binary, 1) as [stream]
                                     from [dbo].[TestEntity3Table] as t
                                 end";

            SqlCommand crSP3Command = new SqlCommand(crSP3, connection);

            try
            {
                crSP3Command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create stored procedure - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            // create test procedure sp4
            string crSP4 = @"CREATE PROCEDURE [dbo].[spRaiseError]
                                 AS
                                 DECLARE @ErrorMessage NVARCHAR(4000)
                                 DECLARE @ErrorSeverity INT
                                 DECLARE @ErrorState INT

                                 begin try
                                     THROW 50000, 'Error occured', 1;
                                 end try
                                 begin catch
                                     set @ErrorMessage = ERROR_MESSAGE()
                                     set @ErrorSeverity = ERROR_SEVERITY()
                                     set @ErrorState = ERROR_STATE()

                                     RAISERROR (@ErrorMessage, 
                                                @ErrorSeverity, 
                                                @ErrorState)
                                 end catch";

            SqlCommand crSP4Command = new SqlCommand(crSP4, connection);

            try
            {
                crSP4Command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create stored procedure - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            // create test procedure sp5
            string crSP5 = @"CREATE PROCEDURE [dbo].[spGetTestEntityAutoPropertyIDs]
                                 AS
                                 select 
	                                  t.TestEntityAutoPropertiesID as [id]
                                 from [dbo].[TestEntityAutoPropertiesTable] as t";

            SqlCommand crSP5Command = new SqlCommand(crSP5, connection);

            try
            {
                crSP5Command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create stored procedure - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            // create test procedure sp6
            string crSP6 = @"CREATE PROCEDURE [dbo].[spGetTestEntityAutoPropertiesPrefixed]
                                AS
                                select 
	                                t.TestEntityAutoPropertiesID as prefix_TestEntityAutoPropertiesID,
			                        t.TestEntityAutoPropertiesName as prefix_TestEntityAutoPropertiesName,
			                        t.TestEntityAutoPropertiesOrder as prefix_TestEntityAutoPropertiesOrder,
			                        t.ModifiedDate as prefix_ModifiedDate,
			                        t.ModifiedBy as prefix_ModifiedBy,
			                        t.CreatedDate as prefix_CreatedDate,
			                        t.CreatedBy as prefix_CreatedBy,
			                        t.IsActive as prefix_IsActive
                                from [dbo].[TestEntityAutoPropertiesTable] as t";

            SqlCommand crSP6Command = new SqlCommand(crSP6, connection);

            try
            {
                crSP6Command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create stored procedure - {0}.", GRStringHelpers.GetExceptionString(exc));
            }
            
            // create test procedure sp7
            string crSP8 = @"CREATE PROCEDURE [dbo].[spGetJsonTestEntityAutoPropertiesPrefixed] @jsonOutput NVARCHAR(MAX) OUTPUT
                                AS
                                BEGIN
                                    SET @jsonOutput = (select * from [dbo].[TestEntityAutoPropertiesTable] for json path)
                                END";

            SqlCommand crSP8Command = new SqlCommand(crSP8, connection);

            try
            {
                crSP8Command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create stored procedure - {0}.", GRStringHelpers.GetExceptionString(exc));
            }
        }

        private static void CreateFunctions(SqlConnection connection)
        {
            // create test function 1
            string crFN1 = @"CREATE FUNCTION [dbo].[fnParseNumber]
                                (	
	                                @string varchar(max)
                                )
                                returns numeric(28,10)
                                -- with encryption
                                as
                                begin

	                                declare @incorrectcharloc int
	                                declare @returnnumber numeric(28,10)

	                                set @string = isnull(@string, '')
	                                set @string = replace(@string, ',', '.')

	                                if len(@string) between 1 and 28 and @string != '0'
	                                begin

		                                -- check if number is positive or negative
		                                declare @multiply int
		                                if @string like '-%'
			                                set @multiply = -1
		                                else
			                                set @multiply = 1
	
		                                -- keep only one decimal separator
		                                set @incorrectcharloc = patindex('%.%.%', @string)
		                                while @incorrectcharloc > 0
		                                begin
			                                set @string = stuff(@string, @incorrectcharloc, 1, '')
			                                set @incorrectcharloc = patindex('%.%.%', @string)
		                                end

		                                -- remove all non numeric characters
		                                set @incorrectcharloc = patindex('%[^0-9.]%', @string)
		                                while @incorrectcharloc > 0
		                                begin
			                                set @string = stuff(@string, @incorrectcharloc, 1, '')
			                                set @incorrectcharloc = patindex('%[^0-9.]%', @string)
		                                end

		                                -- if string is empty or only decimal remain return 0 otherwise concert to number
		                                if(@string = '' or @string = '.')
			                                set @returnnumber = 0
		                                else
			                                set @returnnumber = convert(numeric(28,10), @string) 

		                                -- put back negative / positive
		                                if @multiply = -1
			                                set @returnnumber = @returnnumber * -1

	                                end
	                                else
		                                set @returnnumber = 0

	                                return @returnnumber

                                end";

            SqlCommand crFN1Command = new SqlCommand(crFN1, connection);

            try
            {
                crFN1Command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create function - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            // create test function 2
            string crFN2 = @"CREATE FUNCTION [dbo].[fnParseInt]
                                 (
	                                 @string varchar(max)
                                 )
                                 returns int
                                 -- with encryption
                                 as
                                 begin
 	                                 return isnull(try_cast(dbo.fnParseNumber(@string) as int), 0)
                                 end";

            SqlCommand crFN2Command = new SqlCommand(crFN2, connection);

            try
            {
                crFN2Command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create function - {0}.", GRStringHelpers.GetExceptionString(exc));
            }
        }

        private static void InitializeTablePrimaryKeys(SqlConnection connection)
        {
            // primary keys test
            string crPKTable = "CREATE TABLE [dbo].[TestEntityPK]([TestEntityPKID] [int] NOT NULL, [TestEntityPKName] [nvarchar](max) NULL, CONSTRAINT PK_TestEntityPK PRIMARY KEY (TestEntityPKID))";
            SqlCommand crPKTableCommand = new SqlCommand(crPKTable, connection);

            try
            {
                crPKTableCommand.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create table - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            string crAIPKTable = "CREATE TABLE [dbo].[TestEntityAIPK]([TestEntityAIPKID] [int] IDENTITY(1,1) NOT NULL, [TestEntityAIPKName] [nvarchar](max) NULL)";
            SqlCommand crAIPKTableCommand = new SqlCommand(crAIPKTable, connection);

            try
            {
                crAIPKTableCommand.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create table - {0}.", GRStringHelpers.GetExceptionString(exc));
            }
        }

        private static void InitializeTableJoining(SqlConnection connection)
        {
            string crTestEntityJoining = @"CREATE TABLE [TestEntityJoining] (
                                                [TestEntityJoiningID][int] IDENTITY(1, 1) NOT NULL,
                                                [TestEntityAutoPropertiesID][int] NOT NULL,
                                                [TestEntityJoiningDescription] varchar(max) NOT NULL)";


            SqlCommand TestEntityJoiningCommand = new SqlCommand(crTestEntityJoining, connection);

            try
            {
                TestEntityJoiningCommand.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create table - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            string crFk = @"ALTER TABLE [dbo].[TestEntityJoining]  WITH CHECK ADD CONSTRAINT [FK_TestEntityJoining_TestEntityAutoPropertiesTable] FOREIGN KEY([TestEntityAutoPropertiesID]) REFERENCES [dbo].[TestEntityAutoPropertiesTable] ([TestEntityAutoPropertiesID])";
            SqlCommand crFkCommand = new SqlCommand(crFk, connection);

            try
            {
                crFkCommand.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not foreign key for tables - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            for (int i = 0; i < TestCollectionSize; i++)
            {
                string insertTestEntityJoining = string.Format(@"INSERT INTO [TestEntityJoining] (TestEntityAutoPropertiesID, TestEntityJoiningDescription) VALUES ({0}, '{1}')", 100 - i, string.Format("Description {0}", i + 1));
                new SqlCommand(insertTestEntityJoining, connection).ExecuteNonQuery();
            }
        }

        private static void InitializeTableJoiningTypes(SqlConnection connection)
        {
            string cr1 = @"CREATE TABLE [TestEntityJoiningType1] (
                                        [TestEntityJoiningType1ID][int] NOT NULL,
                                        [TestEntityJoiningType2ID][int] NOT NULL,
                                        [TestEntityJoiningType1Name] varchar(max) NOT NULL)";

            string cr2 = @"CREATE TABLE [TestEntityJoiningType2] (
                                        [TestEntityJoiningType2ID][int] NOT NULL,
                                        [TestEntityJoiningType1ID][int] NOT NULL,
                                        [TestEntityJoiningType2Name] varchar(max) NOT NULL)";

            string cr3 = @"CREATE TABLE [TestEntityJoiningType3] (
                                        [TestEntityJoiningType3ID][int] IDENTITY(1, 1) NOT NULL,
                                        [TestEntityJoiningType3Name] varchar(max) NOT NULL
                                        CONSTRAINT PK_TestEntityJoiningType3 PRIMARY KEY (TestEntityJoiningType3ID));";

            string cr4 = @"CREATE TABLE [TestEntityJoiningType4] (
                                        [TestEntityJoiningType4ID][int] IDENTITY(1, 1) NOT NULL,
                                        [TestEntityJoiningType3ID][int] NOT NULL,
                                        [TestEntityJoiningType4Value][int] NULL
                                        CONSTRAINT PK_TestEntityJoiningType4 PRIMARY KEY (TestEntityJoiningType4ID));";

            SqlCommand cr1Command = new SqlCommand(cr1, connection);
            SqlCommand cr2Command = new SqlCommand(cr2, connection);
            SqlCommand cr3Command = new SqlCommand(cr3, connection);
            SqlCommand cr4Command = new SqlCommand(cr4, connection);

            try
            {
                cr1Command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create table - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            try
            {
                cr2Command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create table - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            try
            {
                cr3Command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create table - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            try
            {
                cr4Command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create table - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            for (int i = 0; i < 10; i++)
            {
                string insertTestEntityJoining = string.Format(@"INSERT INTO [TestEntityJoiningType1] (TestEntityJoiningType1ID, TestEntityJoiningType2ID, TestEntityJoiningType1Name) VALUES ({0}, {1}, '{2}')", i + 1, (i + 1) * 2, string.Format("Name1 {0}", i + 1));
                new SqlCommand(insertTestEntityJoining, connection).ExecuteNonQuery();
            }

            for (int i = 0; i < 10; i++)
            {
                string insertTestEntityJoining = string.Format(@"INSERT INTO [TestEntityJoiningType2] (TestEntityJoiningType2ID, TestEntityJoiningType1ID, TestEntityJoiningType2Name) VALUES ({0}, {1}, '{2}')", i + 1, (i + 1) * 3, string.Format("Name2 {0}", i + 1));
                new SqlCommand(insertTestEntityJoining, connection).ExecuteNonQuery();
            }

            for (int i = 0; i < 10; i++)
            {
                string insCmdText = string.Format(
                    @"INSERT INTO [TestEntityJoiningType3] (TestEntityJoiningType3Name) 
                            VALUES ('{0}')", string.Format("Name {0}", i + 1));

                new SqlCommand(insCmdText, connection).ExecuteNonQuery();
            }

            string insCmdTextValues = string.Format(
                    @"  INSERT INTO [TestEntityJoiningType4] (TestEntityJoiningType3ID, TestEntityJoiningType4Value) 
                            VALUES (1, NULL);
                        INSERT INTO [TestEntityJoiningType4] (TestEntityJoiningType3ID, TestEntityJoiningType4Value) 
                            VALUES (2, 1);
                    ");

            new SqlCommand(insCmdTextValues, connection).ExecuteNonQuery();
        }

        private static void InitializeTableAutoProperties(SqlConnection connection)
        {
            string crTestEntityAutoProperties = @"CREATE TABLE [TestEntityAutoPropertiesTable] (
                                                [TestEntityAutoPropertiesID][int] IDENTITY(1, 1) NOT NULL,
                                                [TestEntityAutoPropertiesName] varchar(max) NOT NULL,
                                                [TestEntityAutoPropertiesOrder] INT NOT NULL,
                                                [ModifiedDate] DATETIME NOT NULL,
                                                [ModifiedBy] INT NOT NULL,
                                                [CreatedDate] DATETIME NOT NULL,
                                                [CreatedBy] INT NOT NULL,
                                                [IsActive] BIT NOT NULL
                                                    CONSTRAINT PK_TestEntityAutoProperties PRIMARY KEY (TestEntityAutoPropertiesID) );";

            SqlCommand crTestEntityAutoPropertiesCommand = new SqlCommand(crTestEntityAutoProperties, connection);

            try
            {
                crTestEntityAutoPropertiesCommand.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create table - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            // populate with test data
            for (int i = 0; i < TestCollectionSize; i++)
            {
                string insertTestEntityAutoProperties = string.Format(@"INSERT INTO [TestEntityAutoPropertiesTable] 
                                        (TestEntityAutoPropertiesName, TestEntityAutoPropertiesOrder, ModifiedDate, ModifiedBy, CreatedDate, CreatedBy, IsActive) 
                                        VALUES 
                                        ('" + NameFormatString + "', {1}, '{2}', {3}, '{4}', {5}, {6})",
                                    i + 1,
                                    (i + 50) % 100 + 1,
                                    DefaultCreatedDate,
                                    -1,
                                    DefaultCreatedDate,
                                    -1,
                                    1
                                    );

                new SqlCommand(insertTestEntityAutoProperties, connection).ExecuteNonQuery();
            }
        }

        private static void CreateDatabase(string dbName, SqlConnection connection)
        {
            string commandString = string.Format("CREATE DATABASE [{0}]", dbName);

            SqlCommand command = new SqlCommand(commandString, connection);

            try
            {
                command.ExecuteNonQuery();
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not create database - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            // use newly create database
            new SqlCommand("USE [" + dbName + "]", connection).ExecuteNonQuery();
        }

        public static void DeleteDatabase(string dbName)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(TestUtils.ConnectionString))
                {
                    connection.Open();
                    new SqlCommand(string.Format(@"USE [master];", dbName), connection).ExecuteNonQuery();
                    new SqlCommand(string.Format(@"ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;", dbName), connection).ExecuteNonQuery();
                    new SqlCommand(string.Format(@"DROP DATABASE [{0}];", dbName), connection).ExecuteNonQuery();
                }
            }
            catch (Exception exc)
            {
                Assert.Fail("Could not drop database - {0}.", GRStringHelpers.GetExceptionString(exc));
            }
        }

        public static TestEntityJoiningType1Repository GetTestEntityJoiningType1Repository(string dbName)
        {
            string repoConnectionString = string.Format("{0};initial catalog={1};", TestUtils.ConnectionString, dbName);

            IGRContext repoContext = new GRMSSQLContext(repoConnectionString);

            repoContext.RegisterLogger(new TraceLogger(), Enums.GRContextLogLevel.Debug | Enums.GRContextLogLevel.Error | Enums.GRContextLogLevel.Warning);

            TestEntityJoiningType1Repository grEntities = new TestEntityJoiningType1Repository(repoContext);

            return grEntities;
        }
        public static TestEntityJoiningType2Repository GetTestEntityJoiningType2Repository(string dbName)
        {
            string repoConnectionString = string.Format("{0};initial catalog={1};", TestUtils.ConnectionString, dbName);

            IGRContext repoContext = new GRMSSQLContext(repoConnectionString);

            repoContext.RegisterLogger(new TraceLogger(), Enums.GRContextLogLevel.Debug | Enums.GRContextLogLevel.Error | Enums.GRContextLogLevel.Warning);

            TestEntityJoiningType2Repository grEntities = new TestEntityJoiningType2Repository(repoContext);

            return grEntities;
        }
        public static TestEntityJoiningType3Repository GetTestEntityJoiningType3Repository(string dbName)
        {
            string repoConnectionString = string.Format("{0};initial catalog={1};", TestUtils.ConnectionString, dbName);

            IGRContext repoContext = new GRMSSQLContext(repoConnectionString);

            repoContext.RegisterLogger(new TraceLogger(), Enums.GRContextLogLevel.Debug | Enums.GRContextLogLevel.Error | Enums.GRContextLogLevel.Warning);

            TestEntityJoiningType3Repository grEntities = new TestEntityJoiningType3Repository(repoContext);

            return grEntities;
        }
        public static TestEntityJoiningType4Repository GetTestEntityJoiningType4Repository(string dbName)
        {
            string repoConnectionString = string.Format("{0};initial catalog={1};", TestUtils.ConnectionString, dbName);

            IGRContext repoContext = new GRMSSQLContext(repoConnectionString);

            repoContext.RegisterLogger(new TraceLogger(), Enums.GRContextLogLevel.Debug | Enums.GRContextLogLevel.Error | Enums.GRContextLogLevel.Warning);

            TestEntityJoiningType4Repository grEntities = new TestEntityJoiningType4Repository(repoContext);

            return grEntities;
        }
        public static TestEntityAutoPropertiesRepository GetTestEntityAutoPropertiesRepository(string dbName)
        {
            string repoConnectionString = string.Format("{0};initial catalog={1};", TestUtils.ConnectionString, dbName);

            IGRContext repoContext = new GRMSSQLContext(repoConnectionString);

            repoContext.RegisterLogger(new TraceLogger(), Enums.GRContextLogLevel.Debug | Enums.GRContextLogLevel.Error | Enums.GRContextLogLevel.Warning);

            TestEntityAutoPropertiesRepository grEntities = new TestEntityAutoPropertiesRepository(repoContext);

            return grEntities;
        }

        public static IGRRepository GetNontGenericRepository(string dbName)
        {
            string repoConnectionString = string.Format("{0};initial catalog={1};", TestUtils.ConnectionString, dbName);

            IGRContext repoContext = new GRMSSQLContext(repoConnectionString);
            repoContext.RegisterLogger(new TraceLogger(), Enums.GRContextLogLevel.Debug | Enums.GRContextLogLevel.Error | Enums.GRContextLogLevel.Warning);

            GRRepository repo = new GRRepository(repoContext);
            return repo;
        }

        public static TestEntityBinaryStreamRepository GetTestEntityBinaryStreamRepository(string dbName)
        {
            string repoConnectionString = string.Format("{0};initial catalog={1};", TestUtils.ConnectionString, dbName);

            IGRContext repoContext = new GRMSSQLContext(repoConnectionString);

            repoContext.RegisterLogger(new TraceLogger(), Enums.GRContextLogLevel.Debug | Enums.GRContextLogLevel.Error | Enums.GRContextLogLevel.Warning);

            TestEntityBinaryStreamRepository grEntities = new TestEntityBinaryStreamRepository(repoContext);

            return grEntities;
        }

        public static TestEntityBinaryArrayRepository GetTestEntityBinaryArrayRepository(string dbName)
        {
            string repoConnectionString = string.Format("{0};initial catalog={1};", TestUtils.ConnectionString, dbName);

            IGRContext repoContext = new GRMSSQLContext(repoConnectionString);

            repoContext.RegisterLogger(new TraceLogger(), Enums.GRContextLogLevel.Debug | Enums.GRContextLogLevel.Error | Enums.GRContextLogLevel.Warning);

            TestEntityBinaryArrayRepository grEntities = new TestEntityBinaryArrayRepository(repoContext);

            return grEntities;
        }

        public static TestEntityPKRepository GetTestEntityPKRepository(string dbName)
        {
            string repoConnectionString = string.Format("{0};initial catalog={1};", TestUtils.ConnectionString, dbName);

            IGRContext repoContext = new GRMSSQLContext(repoConnectionString);

            repoContext.RegisterLogger(new TraceLogger(), Enums.GRContextLogLevel.Debug | Enums.GRContextLogLevel.Error | Enums.GRContextLogLevel.Warning);

            TestEntityPKRepository grEntities = new TestEntityPKRepository(repoContext);

            return grEntities;
        }

        public static TestEntityAIPKRepository GetTestEntityAIPKRepository(string dbName)
        {
            string repoConnectionString = string.Format("{0};initial catalog={1};", TestUtils.ConnectionString, dbName);

            IGRContext repoContext = new GRMSSQLContext(repoConnectionString);

            repoContext.RegisterLogger(new TraceLogger(), Enums.GRContextLogLevel.Debug | Enums.GRContextLogLevel.Error | Enums.GRContextLogLevel.Warning);

            TestEntityAIPKRepository grEntities = new TestEntityAIPKRepository(repoContext);

            return grEntities;
        }

        public static TestEntityPrimitiveNullRepository GetTestEntityPrimitiveNullRepository(string dbName)
        {
            string repoConnectionString = string.Format("{0};initial catalog={1};", TestUtils.ConnectionString, dbName);

            IGRContext repoContext = new GRMSSQLContext(repoConnectionString);

            repoContext.RegisterLogger(new TraceLogger(), Enums.GRContextLogLevel.Debug | Enums.GRContextLogLevel.Error | Enums.GRContextLogLevel.Warning);

            TestEntityPrimitiveNullRepository grEntities = new TestEntityPrimitiveNullRepository(repoContext);

            return grEntities;
        }

        public static RepositoryCollection GetRepositories(string dbName)
        {
            string repoConnectionString = string.Format("{0};initial catalog={1};", TestUtils.ConnectionString, dbName);

            IGRContext repoContext = new GRMSSQLContext(repoConnectionString);

            repoContext.RegisterLogger(new TraceLogger(), Enums.GRContextLogLevel.Debug | Enums.GRContextLogLevel.Error | Enums.GRContextLogLevel.Warning);

            TestEntityAutoPropertiesRepository grEntityAutoPropertiesRepository = new TestEntityAutoPropertiesRepository(repoContext);
            TestEntityBinaryStreamRepository grEntitiesEntityBinaryStreamRepository = new TestEntityBinaryStreamRepository(repoContext);
            TestEntityBinaryArrayRepository grEntityBinaryArrayRepository = new TestEntityBinaryArrayRepository(repoContext);
            TestEntityAIPKRepository grEntityAIPKRepository = new TestEntityAIPKRepository(repoContext);
            TestEntityPKRepository grEntityPKRepository = new TestEntityPKRepository(repoContext);
            TestEntityPrimitiveNullRepository grEntityPrimitiveNullRepository = new TestEntityPrimitiveNullRepository(repoContext);

            return new RepositoryCollection()
            {
                Context = repoContext,
                TestEntityAutoPropertiesRepository = grEntityAutoPropertiesRepository,
                TestEntityBinaryStreamRepository = grEntitiesEntityBinaryStreamRepository,
                TestEntityBinaryArrayRepository = grEntityBinaryArrayRepository,
                TestEntityAIPKRepository = grEntityAIPKRepository,
                TestEntityPKRepository = grEntityPKRepository,
                TestEntityPrimitiveNullRepository = grEntityPrimitiveNullRepository
            };
        }

        public static IGRContext GetContext(string dbName)
        {
            string repoConnectionString = string.Format("{0};initial catalog={1};", TestUtils.ConnectionString, dbName);

            IGRContext context = new GRMSSQLContext(repoConnectionString);
            context.RegisterLogger(new TraceLogger(), Enums.GRContextLogLevel.Debug | Enums.GRContextLogLevel.Error | Enums.GRContextLogLevel.Warning);

            return context;
        }

        public static byte[] ConvertToByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        public static TestEntityMultiID_1Repository GetTestEntityMultiID_1Repository(string dbName)
        {
            string repoConnectionString = string.Format("{0};initial catalog={1};", TestUtils.ConnectionString, dbName);

            IGRContext repoContext = new GRMSSQLContext(repoConnectionString);

            repoContext.RegisterLogger(new TraceLogger(), Enums.GRContextLogLevel.Debug | Enums.GRContextLogLevel.Error | Enums.GRContextLogLevel.Warning);

            TestEntityMultiID_1Repository grEntities = new TestEntityMultiID_1Repository(repoContext);

            return grEntities;
        }

        public static TestEntityMultiID_2Repository GetTestEntityMultiID_2Repository(string dbName)
        {
            string repoConnectionString = string.Format("{0};initial catalog={1};", TestUtils.ConnectionString, dbName);

            IGRContext repoContext = new GRMSSQLContext(repoConnectionString);

            repoContext.RegisterLogger(new TraceLogger(), Enums.GRContextLogLevel.Debug | Enums.GRContextLogLevel.Error | Enums.GRContextLogLevel.Warning);

            TestEntityMultiID_2Repository grEntities = new TestEntityMultiID_2Repository(repoContext);

            return grEntities;
        }

        public static TestEntityMultiID_3Repository GetTestEntityMultiID_3Repository(string dbName)
        {
            string repoConnectionString = string.Format("{0};initial catalog={1};", TestUtils.ConnectionString, dbName);

            IGRContext repoContext = new GRMSSQLContext(repoConnectionString);

            repoContext.RegisterLogger(new TraceLogger(), Enums.GRContextLogLevel.Debug | Enums.GRContextLogLevel.Error | Enums.GRContextLogLevel.Warning);

            TestEntityMultiID_3Repository grEntities = new TestEntityMultiID_3Repository(repoContext);

            return grEntities;
        }

    }
}
