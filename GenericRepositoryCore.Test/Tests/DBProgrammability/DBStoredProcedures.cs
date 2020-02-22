using GenericRepository.Exceptions;
using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Models;
using GenericRepository.Test.Models;
using Ionic.Zip;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Test.DBProgrammability
{
    [TestClass]
    public class DBStoredProcedures
    {
        #region Init and cleanup
        static string dbBaseName = "xeelo-tests-gr-sp";
        static string dbName = null;

        [ClassInitialize]
        public static void TestInit(TestContext context)
        {
            dbName = TestUtils.InitializeDatabase(dbBaseName);
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            TestUtils.DeleteDatabase(dbName);
        }
        #endregion

        [TestMethod]
        public async Task Get_ListFromSP5_Primitive_All()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            List<int> ret = null;

            try
            {
                ret = await context.GetValuesFromSPAsync<int>("spGetTestEntityAutoPropertyIDs");
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get list from spGetTestEntityAutoPropertyIDs - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(ret.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", ret.Count, TestUtils.TestCollectionSize);

            foreach (var item in ret)
            {
                Assert.IsTrue(item > -1, "Entity was loaded incorrectly!");
            }

            Assert.IsTrue(ret.Distinct().Count() == ret.Count, "Entities are not unique!");
        }

        [TestMethod]
        public async Task Get_ListFromSP5_Primitive_All_ColumnName()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            List<int> ret = null;

            try
            {
                ret = await context.GetValuesFromSPAsync<int>("spGetTestEntityAutoPropertyIDs", "id");
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get list from spGetTestEntityAutoPropertyIDs - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(ret.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", ret.Count, TestUtils.TestCollectionSize);

            foreach (var item in ret)
            {
                Assert.IsTrue(item > -1, "Entity was loaded incorrectly!");
            }

            Assert.IsTrue(ret.Distinct().Count() == ret.Count, "Entities are not unique!");
        }

        [TestMethod]
        public async Task Get_ListFromSP1_All()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            List<TestEntitySP1> testEntities = null;

            try
            {
                testEntities = await context.GetEntitiesFromSPAsync<TestEntitySP1>("spGetTestEntityAutoProperties");
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get list from sp1 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(testEntities.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", testEntities.Count, TestUtils.TestCollectionSize);

            foreach (var item in testEntities)
            {
                Assert.IsTrue(item.id > 0, "Entity was not loaded!");

                Assert.IsTrue(item.name == string.Format(TestUtils.NameFormatString, item.id),
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Name = '{1}' instead of Name = '{2}'.",
                    item.id,
                    string.Format(TestUtils.NameFormatString, item.id),
                    item.name
                    );

                Assert.IsTrue(item.order == (item.id + 49) % 100 + 1,
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Order = '{1}' instead of Order = '{2}'.",
                    item.id,
                    (item.id + 49) % 100 + 1,
                    item.order
                    );
            }

            List<int> allIds = testEntities.Select(e => e.id).ToList();
            List<string> allNames = testEntities.Select(e => e.name).ToList();

            Assert.IsTrue(allIds.Distinct().Count() == testEntities.Count, "Entity IDs are not unique!");
            Assert.IsTrue(allNames.Distinct().Count() == testEntities.Count, "Entity names are not unique!");
        }

        [TestMethod]
        public async Task Get_ListFromSP1_ID1()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            List<TestEntitySP1> testEntities = null;

            try
            {
                testEntities = await context.GetEntitiesFromSPAsync<TestEntitySP1>("spGetTestEntityAutoProperties", new List<SqlParameter> { new SqlParameter("@TestEntityAutoPropertiesID", 1) });
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get list (ID: 1) from sp1 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(testEntities.Count == 1, "Returned {0} entities instead of {1}.", testEntities.Count, 1);

            foreach (var item in testEntities)
            {
                Assert.IsTrue(item.id > 0, "Entity was not loaded!");

                Assert.IsTrue(item.name == string.Format(TestUtils.NameFormatString, item.id),
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Name = '{1}' instead of Name = '{2}'.",
                    item.id,
                    string.Format(TestUtils.NameFormatString, item.id),
                    item.name
                    );

                Assert.IsTrue(item.order == (item.id + 49) % 100 + 1,
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Order = '{1}' instead of Order = '{2}'.",
                    item.id,
                    (item.id + 49) % 100 + 1,
                    item.order
                    );
            }

            List<int> allIds = testEntities.Select(e => e.id).ToList();
            List<string> allNames = testEntities.Select(e => e.name).ToList();

            Assert.IsTrue(allIds.Distinct().Count() == testEntities.Count, "Entity IDs are not unique!");
            Assert.IsTrue(allNames.Distinct().Count() == testEntities.Count, "Entity names are not unique!");
        }

        [TestMethod]
        public async Task Get_ItemFromSP1_ID1_TEST()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            try
            {
                var testEntity = await context.GetEntityFromSPAsync<TestEntitySP1>(
                    "spGetTestEntityAutoProperties"); //, new List<SqlParameter> { new SqlParameter("@TestEntityAutoPropertiesID", 1) });
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get item (ID: 1) from spGetTestEntityAutoProperties - {0}.", GRStringHelpers.GetExceptionString(exc));
            }
        }

        [TestMethod]
        public async Task Get_ItemFromSP1_ID1()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            TestEntitySP1 testEntity = null;

            try
            {
                testEntity = await context.GetEntityFromSPAsync<TestEntitySP1>("spGetTestEntityAutoProperties", new List<SqlParameter> { new SqlParameter("@TestEntityAutoPropertiesID", 1) });
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get item (ID: 1) from spGetTestEntityAutoProperties - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(testEntity.id > 0, "Entity was not loaded!");

            Assert.IsTrue(testEntity.name == string.Format(TestUtils.NameFormatString, testEntity.id),
                "Entity name was loaded incorrectly! Entity should have Name = '{0}' instead of Name = '{1}'.",
                string.Format(TestUtils.NameFormatString, testEntity.id),
                testEntity.name
                );

            Assert.IsTrue(testEntity.order == (testEntity.id + 49) % 100 + 1,
                "Entity name was loaded incorrectly! Entity should have Order = '{0}' instead of Order = '{1}'.",
                (testEntity.id + 49) % 100 + 1,
                testEntity.order
                );
        }

        [TestMethod]
        public async Task Get_ItemFromSP1()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            TestEntitySP1 testEntity = null;

            try
            {
                testEntity = await context.GetEntityFromSPAsync<TestEntitySP1>("spGetTestEntityAutoProperties");
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get item from spGetTestEntityAutoProperties - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(testEntity.id > 0, "Entity was not loaded!");

            Assert.IsTrue(testEntity.name == string.Format(TestUtils.NameFormatString, testEntity.id),
                "Entity name was loaded incorrectly! Entity should have Name = '{0}' instead of Name = '{1}'.",
                string.Format(TestUtils.NameFormatString, testEntity.id),
                testEntity.name
                );

            Assert.IsTrue(testEntity.order == (testEntity.id + 49) % 100 + 1,
                "Entity name was loaded incorrectly! Entity should have Order = '{0}' instead of Order = '{1}'.",
                (testEntity.id + 49) % 100 + 1,
                testEntity.order
                );
        }

        [TestMethod]
        public void Get_DatasetSP1_ALL()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            DataSet ds = null;

            try
            {
                ds = context.GetDataSetFromSP("spGetTestEntityAutoProperties", null);
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get dataset from sp1 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(ds.Tables.Count == 1, "Returned {0} tables instead of {1}.", ds.Tables.Count, 1);

            Assert.IsTrue(ds.Tables[0].Rows.Count == TestUtils.TestCollectionSize, "Returned {0} rows instead of {1}.", ds.Tables[0].Rows.Count, TestUtils.TestCollectionSize);

            foreach (DataRow item in ds.Tables[0].Rows)
            {
                Assert.IsTrue((int)item["id"] > 0, "Entity was not loaded!");

                Assert.IsTrue(item["name"].ToString() == string.Format(TestUtils.NameFormatString, (int)item["id"]),
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Name = '{1}' instead of Name = '{2}'.",
                    (int)item["id"],
                    string.Format(TestUtils.NameFormatString, (int)item["id"]),
                    item["name"].ToString()
                    );

                Assert.IsTrue((int)item["order"] == ((int)item["id"] + 49) % 100 + 1,
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Order = '{1}' instead of Order = '{2}'.",
                    (int)item["id"],
                    ((int)item["id"] + 49) % 100 + 1,
                    (int)item["order"]
                    );
            }
        }

        [TestMethod]
        public void Get_DatasetSP1_ID1()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            DataSet ds = null;

            try
            {
                ds = context.GetDataSetFromSP("spGetTestEntityAutoProperties", new List<SqlParameter> { new SqlParameter("@TestEntityAutoPropertiesID", 1) });
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get dataset from sp1 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(ds.Tables.Count == 1, "Returned {0} tables instead of {1}.", ds.Tables.Count, 1);

            Assert.IsTrue(ds.Tables[0].Rows.Count == 1, "Returned {0} rows instead of {1}.", ds.Tables[0].Rows.Count, 1);

            foreach (DataRow item in ds.Tables[0].Rows)
            {
                Assert.IsTrue((int)item["id"] > 0, "Entity was not loaded!");

                Assert.IsTrue(item["name"].ToString() == string.Format(TestUtils.NameFormatString, (int)item["id"]),
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Name = '{1}' instead of Name = '{2}'.",
                    (int)item["id"],
                    string.Format(TestUtils.NameFormatString, (int)item["id"]),
                    item["name"].ToString()
                    );

                Assert.IsTrue((int)item["order"] == ((int)item["id"] + 49) % 100 + 1,
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Order = '{1}' instead of Order = '{2}'.",
                    (int)item["id"],
                    ((int)item["id"] + 49) % 100 + 1,
                    (int)item["order"]
                    );
            }
        }

        [TestMethod]
        public async Task Get_DatatableWithOutParsSP2_ALL()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            DataTable dt = null;
            List<SqlParameter> returnParams = new List<SqlParameter>();

            try
            {
                dt = await context.GetDataTableFromSPAsync("spGetTestEntityAutoPropertiesWithReturningCount",
                    new List<SqlParameter> {
                        new SqlParameter("@TestEntityAutoPropertiesID", DBNull.Value),
                        new SqlParameter("@EntryCount", SqlDbType.Int) { Direction = ParameterDirection.Output } 
                        },
                    returnParams);
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get datatable from sp2 and return entry count - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(dt.Rows.Count == TestUtils.TestCollectionSize, "Returned {0} rows instead of {1}.", dt.Rows.Count, TestUtils.TestCollectionSize);

            Assert.IsTrue(returnParams.Count == 1, "Returned {0} output params instead of {1}.", returnParams.Count, 1);

            Assert.IsTrue((int)returnParams[0].Value == TestUtils.TestCollectionSize, "Returned param @EntryCount value is {0} instead of {1}.",  (int)returnParams[0].Value, TestUtils.TestCollectionSize);

            foreach (DataRow item in dt.Rows)
            {
                Assert.IsTrue((int)item["id"] > 0, "Entity was not loaded!");

                Assert.IsTrue(item["name"].ToString() == string.Format(TestUtils.NameFormatString, (int)item["id"]),
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Name = '{1}' instead of Name = '{2}'.",
                    (int)item["id"],
                    string.Format(TestUtils.NameFormatString, (int)item["id"]),
                    item["name"].ToString()
                    );

                Assert.IsTrue((int)item["order"] == ((int)item["id"] + 49) % 100 + 1,
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Order = '{1}' instead of Order = '{2}'.",
                    (int)item["id"],
                    ((int)item["id"] + 49) % 100 + 1,
                    (int)item["order"]
                    );
            }
        }

        [TestMethod]
        public async Task Get_DatatableWithOutParsSP2_ID1()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            DataTable dt = null;
            List<SqlParameter> returnParams = new List<SqlParameter>();

            try
            {
                dt = await context.GetDataTableFromSPAsync("spGetTestEntityAutoPropertiesWithReturningCount",
                    new List<SqlParameter> {
                        new SqlParameter("@TestEntityAutoPropertiesID", 1),
                        new SqlParameter("@EntryCount", SqlDbType.Int) { Direction = ParameterDirection.Output }
                        },
                    returnParams);
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get datatable from sp2 and return entry count - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(dt.Rows.Count == 1, "Returned {0} rows instead of {1}.", dt.Rows.Count, 1);

            Assert.IsTrue(returnParams.Count == 1, "Returned {0} output params instead of {1}.", returnParams.Count, 1);

            Assert.IsTrue((int)returnParams[0].Value == 1, "Returned param @EntryCount value is {0} instead of {1}.", (int)returnParams[0].Value, 1);

            foreach (DataRow item in dt.Rows)
            {
                Assert.IsTrue((int)item["id"] > 0, "Entity was not loaded!");

                Assert.IsTrue(item["name"].ToString() == string.Format(TestUtils.NameFormatString, (int)item["id"]),
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Name = '{1}' instead of Name = '{2}'.",
                    (int)item["id"],
                    string.Format(TestUtils.NameFormatString, (int)item["id"]),
                    item["name"].ToString()
                    );

                Assert.IsTrue((int)item["order"] == ((int)item["id"] + 49) % 100 + 1,
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Order = '{1}' instead of Order = '{2}'.",
                    (int)item["id"],
                    ((int)item["id"] + 49) % 100 + 1,
                    (int)item["order"]
                    );
            }
        }

        [TestMethod]
        public async Task SP_WithOutParams()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            List<SqlParameter> returnParams = null;

            try
            {
                returnParams = await context.ExecuteSPWithOutParamsAsync("spGetTestEntityAutoPropertiesWithReturningCount",
                    new List<SqlParameter> {
                        new SqlParameter("@TestEntityAutoPropertiesID", DBNull.Value),
                        new SqlParameter("@EntryCount", SqlDbType.Int) { Direction = ParameterDirection.Output }
                        });
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to execute sp2 and return entry count - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(returnParams.Count == 1, "Returned {0} output params instead of {1}.", returnParams.Count, 1);

            Assert.IsTrue((int)returnParams[0].Value == TestUtils.TestCollectionSize, "Returned param @EntryCount value is {0} instead of {1}.", (int)returnParams[0].Value, TestUtils.TestCollectionSize);
        }

        [TestMethod]
        public async Task ExecuteSP2_ID1()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            List<SqlParameter> returnParams = null;

            try
            {
                returnParams = await context.ExecuteSPWithOutParamsAsync("spGetTestEntityAutoPropertiesWithReturningCount",
                    new List<SqlParameter> {
                        new SqlParameter("@TestEntityAutoPropertiesID", 1),
                        new SqlParameter("@EntryCount", SqlDbType.Int) { Direction = ParameterDirection.Output }
                        });
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to execute sp2 and return entry count - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(returnParams.Count == 1, "Returned {0} output params instead of {1}.", returnParams.Count, 1);

            Assert.IsTrue((int)returnParams[0].Value == 1, "Returned param @EntryCount value is {0} instead of {1}.", (int)returnParams[0].Value, 1);            
        }

        [TestMethod]
        public async Task Get_MemoryStreamFromSP3()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            MemoryStream ms = null;
            string data = null;

            try
            {
                using (Stream stream = await context.GetMemoryStreamFromSPAsync("spGetStream", null, CommandBehavior.SequentialAccess))
                {
                    ms = new MemoryStream();

                    stream.CopyTo(ms);
                }

                data = Encoding.UTF8.GetString(ms.ToArray());
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get memory stream from sp3 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(ms.Length == TestUtils.TestSPVarbinaryData.Length, "Returned stream length equals {0} instead of {1}.", ms.Length, TestUtils.TestSPVarbinaryData.Length);

            Assert.IsTrue(data == TestUtils.TestSPVarbinaryData, "Returned data equals {0} instead of {1}.", data, TestUtils.TestSPVarbinaryData);

            ms.Close();
        }

        [TestMethod]
        public async Task Get_ZipStreamFromSP3()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            ZipFile file = null;
            MemoryStream fileStream = null;
            string data = null;

            try
            {
                using (Stream stream = await context.GetZipStreamFromSPAsync("spGetStream", null, CommandBehavior.SequentialAccess))
                {
                    file = ZipFile.Read(stream);

                    if (file.Entries.Count == 1)
                    {
                        fileStream = new MemoryStream();
                        file[0].Extract(fileStream);

                        data = Encoding.UTF8.GetString(fileStream.ToArray());
                    }
                }
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get zip stream from sp3 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(file.Entries.Count == 1, "Returned stream contains {0} files instead of {1}.", file.Entries.Count, 1);

            Assert.IsTrue(file[0].FileName == "default", "Returned filename equals {0} instead of {1}.", file[0].FileName, "default");

            Assert.IsTrue(fileStream.Length == TestUtils.TestSPVarbinaryData.Length, "Returned stream length equals {0} instead of {1}.", fileStream.Length, TestUtils.TestSPVarbinaryData.Length);

            Assert.IsTrue(data == TestUtils.TestSPVarbinaryData, "Returned data equals {0} instead of {1}.", data, TestUtils.TestSPVarbinaryData);

            fileStream.Close();
        }        

        [TestMethod]
        public async Task Get_ListFromSP4_Primitive_Error()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            List<int> res;

            try
            {
                res = await context.GetValuesFromSPAsync<int>("spRaiseError");

                Assert.Fail("No exception was thrown");
            }
            catch (Exception exc)
            {
                Assert.IsTrue(exc.GetType() == typeof(GRQueryExecutionFailedException), "Exception should be type of 'GRQueryExecutionFailedException' insteed of '{0}", exc.GetType());
                Assert.IsTrue(exc.InnerException != null, "InnerException must be assigned");
                Assert.IsTrue(exc.InnerException.GetType() == typeof(SqlException), "InnerException should be type of 'SqlException' insteed of '{0}", exc.InnerException.GetType());
                Assert.IsTrue(((SqlException)exc.InnerException).Message == "Error occured", "InnerException should have Message = 'Error occured' insteed of Message = '{0}'", ((SqlException)exc.InnerException).Message);
                Assert.IsTrue(((SqlException)exc.InnerException).Number == 50000, "InnerException should have Number = 50000 insteed of Number = {0}", ((SqlException)exc.InnerException).Number);
                Assert.IsTrue(((SqlException)exc.InnerException).Procedure == "spRaiseError", "InnerException should have Procedure = 'spRaiseError' insteed of Procedure = '{0}'", ((SqlException)exc.InnerException).Procedure);
            }
        }

        [TestMethod]
        public async Task Get_ListFromSP4_Structure_Error()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            List<TestEntitySP4> res;

            try
            {
                res = await context.GetEntitiesFromSPAsync<TestEntitySP4>("spRaiseError");

                Assert.Fail("No exception was thrown");
            }
            catch (Exception exc)
            {
                Assert.IsTrue(exc.GetType() == typeof(GRQueryExecutionFailedException), "Exception should be type of 'GRQueryExecutionFailedException' insteed of '{0}", exc.GetType());
                Assert.IsTrue(exc.InnerException != null, "InnerException must be assigned");
                Assert.IsTrue(exc.InnerException.GetType() == typeof(SqlException), "InnerException should be type of 'SqlException' insteed of '{0}", exc.InnerException.GetType());
                Assert.IsTrue(((SqlException)exc.InnerException).Message == "Error occured", "InnerException should have Message = 'Error occured' insteed of Message = '{0}'", ((SqlException)exc.InnerException).Message);
                Assert.IsTrue(((SqlException)exc.InnerException).Number == 50000, "InnerException should have Number = 50000 insteed of Number = {0}", ((SqlException)exc.InnerException).Number);
                Assert.IsTrue(((SqlException)exc.InnerException).Procedure == "spRaiseError", "InnerException should have Procedure = 'spRaiseError' insteed of Procedure = '{0}'", ((SqlException)exc.InnerException).Procedure);
            }
        }

        [TestMethod]
        public async Task Get_ItemFromSP4_Primitive_Error()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            int res;

            try
            {
                res = await context.GetValueFromSPAsync<int>("spRaiseError");

                Assert.Fail("No exception was thrown");
            }
            catch (Exception exc)
            {
                Assert.IsTrue(exc.GetType() == typeof(GRQueryExecutionFailedException), "Exception should be type of 'GRQueryExecutionFailedException' insteed of '{0}", exc.GetType());
                Assert.IsTrue(exc.InnerException != null, "InnerException must be assigned");
                Assert.IsTrue(exc.InnerException.GetType() == typeof(SqlException), "InnerException should be type of 'SqlException' insteed of '{0}", exc.InnerException.GetType());
                Assert.IsTrue(((SqlException)exc.InnerException).Message == "Error occured", "InnerException should have Message = 'Error occured' insteed of Message = '{0}'", ((SqlException)exc.InnerException).Message);
                Assert.IsTrue(((SqlException)exc.InnerException).Number == 50000, "InnerException should have Number = 50000 insteed of Number = {0}", ((SqlException)exc.InnerException).Number);
                Assert.IsTrue(((SqlException)exc.InnerException).Procedure == "spRaiseError", "InnerException should have Procedure = 'spRaiseError' insteed of Procedure = '{0}'", ((SqlException)exc.InnerException).Procedure);
            }
        }

        [TestMethod]
        public async Task Get_ItemFromSP4_Structure_Error()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            TestEntitySP4 res;

            try
            {
                res = await context.GetEntityFromSPAsync<TestEntitySP4>("spRaiseError");

                Assert.Fail("No exception was thrown");
            }
            catch (Exception exc)
            {
                Assert.IsTrue(exc.GetType() == typeof(GRQueryExecutionFailedException), "Exception should be type of 'GRQueryExecutionFailedException' insteed of '{0}", exc.GetType());
                Assert.IsTrue(exc.InnerException != null, "InnerException must be assigned");
                Assert.IsTrue(exc.InnerException.GetType() == typeof(SqlException), "InnerException should be type of 'SqlException' insteed of '{0}", exc.InnerException.GetType());
                Assert.IsTrue(((SqlException)exc.InnerException).Message == "Error occured", "InnerException should have Message = 'Error occured' insteed of Message = '{0}'", ((SqlException)exc.InnerException).Message);
                Assert.IsTrue(((SqlException)exc.InnerException).Number == 50000, "InnerException should have Number = 50000 insteed of Number = {0}", ((SqlException)exc.InnerException).Number);
                Assert.IsTrue(((SqlException)exc.InnerException).Procedure == "spRaiseError", "InnerException should have Procedure = 'spRaiseError' insteed of Procedure = '{0}'", ((SqlException)exc.InnerException).Procedure);
            }
        }

        [TestMethod]
        public void Get_DatasetFromSP4_Error()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            DataSet res;

            try
            {
                res = context.GetDataSetFromSP("spRaiseError", null);

                Assert.Fail("No exception was thrown");
            }
            catch (Exception exc)
            {
                Assert.IsTrue(exc.GetType() == typeof(GRQueryExecutionFailedException), "Exception should be type of 'GRQueryExecutionFailedException' insteed of '{0}", exc.GetType());
                Assert.IsTrue(exc.InnerException != null, "InnerException must be assigned");
                Assert.IsTrue(exc.InnerException.GetType() == typeof(SqlException), "InnerException should be type of 'SqlException' insteed of '{0}", exc.InnerException.GetType());
                Assert.IsTrue(((SqlException)exc.InnerException).Message == "Error occured", "InnerException should have Message = 'Error occured' insteed of Message = '{0}'", ((SqlException)exc.InnerException).Message);
                Assert.IsTrue(((SqlException)exc.InnerException).Number == 50000, "InnerException should have Number = 50000 insteed of Number = {0}", ((SqlException)exc.InnerException).Number);
                Assert.IsTrue(((SqlException)exc.InnerException).Procedure == "spRaiseError", "InnerException should have Procedure = 'spRaiseError' insteed of Procedure = '{0}'", ((SqlException)exc.InnerException).Procedure);
            }
        }

        [TestMethod]
        public async Task Get_DatatableFromSP4_Error()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            DataTable res;

            try
            {
                res = await context.GetDataTableFromSPAsync("spRaiseError", null);

                Assert.Fail("No exception was thrown");
            }
            catch (Exception exc)
            {
                Assert.IsTrue(exc.GetType() == typeof(GRQueryExecutionFailedException), "Exception should be type of 'GRQueryExecutionFailedException' insteed of '{0}", exc.GetType());
                Assert.IsTrue(exc.InnerException != null, "InnerException must be assigned");
                Assert.IsTrue(exc.InnerException.GetType() == typeof(SqlException), "InnerException should be type of 'SqlException' insteed of '{0}", exc.InnerException.GetType());
                Assert.IsTrue(((SqlException)exc.InnerException).Message == "Error occured", "InnerException should have Message = 'Error occured' insteed of Message = '{0}'", ((SqlException)exc.InnerException).Message);
                Assert.IsTrue(((SqlException)exc.InnerException).Number == 50000, "InnerException should have Number = 50000 insteed of Number = {0}", ((SqlException)exc.InnerException).Number);
                Assert.IsTrue(((SqlException)exc.InnerException).Procedure == "spRaiseError", "InnerException should have Procedure = 'spRaiseError' insteed of Procedure = '{0}'", ((SqlException)exc.InnerException).Procedure);
            }
        }

        [TestMethod]
        public async Task Get_ExecuteSP4_Error()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            List<SqlParameter> res;

            try
            {
                res = await context.ExecuteSPWithOutParamsAsync("spRaiseError");

                Assert.Fail("No exception was thrown");
            }
            catch (Exception exc)
            {
                Assert.IsTrue(exc.GetType() == typeof(GRQueryExecutionFailedException), "Exception should be type of 'GRQueryExecutionFailedException' insteed of '{0}", exc.GetType());
                Assert.IsTrue(exc.InnerException != null, "InnerException must be assigned");
                Assert.IsTrue(exc.InnerException.GetType() == typeof(SqlException), "InnerException should be type of 'SqlException' insteed of '{0}", exc.InnerException.GetType());
                Assert.IsTrue(((SqlException)exc.InnerException).Message == "Error occured", "InnerException should have Message = 'Error occured' insteed of Message = '{0}'", ((SqlException)exc.InnerException).Message);
                Assert.IsTrue(((SqlException)exc.InnerException).Number == 50000, "InnerException should have Number = 50000 insteed of Number = {0}", ((SqlException)exc.InnerException).Number);
                Assert.IsTrue(((SqlException)exc.InnerException).Procedure == "spRaiseError", "InnerException should have Procedure = 'spRaiseError' insteed of Procedure = '{0}'", ((SqlException)exc.InnerException).Procedure);
            }
        }

        [TestMethod]
        public async Task Get_MemoryStreamFromSP4_Error()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            MemoryStream ms = null;

            try
            {
                using (Stream stream = await context.GetMemoryStreamFromSPAsync("spRaiseError", null, CommandBehavior.SequentialAccess))
                {
                    ms = new MemoryStream();

                    stream.CopyTo(ms);
                }

                Assert.Fail("No exception was thrown");
            }
            catch (Exception exc)
            {
                Assert.IsTrue(exc.GetType() == typeof(GRQueryExecutionFailedException), "Exception should be type of 'GRQueryExecutionFailedException' insteed of '{0}", exc.GetType());
                Assert.IsTrue(exc.InnerException != null, "InnerException must be assigned");
                Assert.IsTrue(exc.InnerException.GetType() == typeof(SqlException), "InnerException should be type of 'SqlException' insteed of '{0}", exc.InnerException.GetType());
                Assert.IsTrue(((SqlException)exc.InnerException).Message == "Error occured", "InnerException should have Message = 'Error occured' insteed of Message = '{0}'", ((SqlException)exc.InnerException).Message);
                Assert.IsTrue(((SqlException)exc.InnerException).Number == 50000, "InnerException should have Number = 50000 insteed of Number = {0}", ((SqlException)exc.InnerException).Number);
                Assert.IsTrue(((SqlException)exc.InnerException).Procedure == "spRaiseError", "InnerException should have Procedure = 'spRaiseError' insteed of Procedure = '{0}'", ((SqlException)exc.InnerException).Procedure);
            }
        }

        [TestMethod]
        public async Task Get_ZipStreamFromSP4_Error()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            ZipFile file = null;
            MemoryStream fileStream = null;

            try
            {
                using (Stream stream = await context.GetZipStreamFromSPAsync("spRaiseError", null, CommandBehavior.SequentialAccess))
                {
                    file = ZipFile.Read(stream);

                    if (file.Entries.Count == 1)
                    {
                        fileStream = new MemoryStream();
                        file[0].Extract(fileStream);
                    }
                }

                Assert.Fail("No exception was thrown");
            }
            catch (Exception exc)
            {
                Assert.IsTrue(exc.GetType() == typeof(GRQueryExecutionFailedException), "Exception should be type of 'GRQueryExecutionFailedException' insteed of '{0}", exc.GetType());
                Assert.IsTrue(exc.InnerException != null, "InnerException must be assigned");
                Assert.IsTrue(exc.InnerException.GetType() == typeof(SqlException), "InnerException should be type of 'SqlException' insteed of '{0}", exc.InnerException.GetType());
                Assert.IsTrue(((SqlException)exc.InnerException).Message == "Error occured", "InnerException should have Message = 'Error occured' insteed of Message = '{0}'", ((SqlException)exc.InnerException).Message);
                Assert.IsTrue(((SqlException)exc.InnerException).Number == 50000, "InnerException should have Number = 50000 insteed of Number = {0}", ((SqlException)exc.InnerException).Number);
                Assert.IsTrue(((SqlException)exc.InnerException).Procedure == "spRaiseError", "InnerException should have Procedure = 'spRaiseError' insteed of Procedure = '{0}'", ((SqlException)exc.InnerException).Procedure);
            }
        }

        [TestMethod]
        public async Task Get_ListFromSP5_Prefixed()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            List<TestEntityAutoProperties> testEntities = null;

            try
            {
                testEntities = await context.GetEntitiesFromSPAsync<TestEntityAutoProperties>("spGetTestEntityAutoPropertiesPrefixed", "prefix");
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get list from sp5 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(testEntities.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", testEntities.Count, TestUtils.TestCollectionSize);

            foreach (var item in testEntities)
            {
                Assert.IsTrue(item.TestEntityAutoPropertiesID > 0, "Entity was not loaded!");

                Assert.IsTrue(item.Name == string.Format(TestUtils.NameFormatString, item.TestEntityAutoPropertiesID),
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Name = '{1}' instead of Name = '{2}'.",
                    item.TestEntityAutoPropertiesID,
                    string.Format(TestUtils.NameFormatString, item.TestEntityAutoPropertiesID),
                    item.Name
                    );

                Assert.IsTrue(item.TestEntityAutoPropertiesOrder == (item.TestEntityAutoPropertiesID + 49) % 100 + 1,
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Order = '{1}' instead of Order = '{2}'.",
                    item.TestEntityAutoPropertiesID,
                    (item.TestEntityAutoPropertiesID + 49) % 100 + 1,
                    item.TestEntityAutoPropertiesOrder
                    );
            }

            List<int> allIds = testEntities.Select(e => e.TestEntityAutoPropertiesID).ToList();
            List<string> allNames = testEntities.Select(e => e.Name).ToList();

            Assert.IsTrue(allIds.Distinct().Count() == testEntities.Count, "Entity IDs are not unique!");
            Assert.IsTrue(allNames.Distinct().Count() == testEntities.Count, "Entity names are not unique!");
        }

        [TestMethod]
        public async Task Get_ListFromSP5_PropCollection()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            GRTable testEntities = null;

            GRPropertyCollection propCol = new GRPropertyCollection();
            propCol.AddType<TestEntityAutoProperties>("prefix");

            try
            {
                testEntities = await context.GetEntitiesFromSPAsync("spGetTestEntityAutoPropertiesPrefixed", null, propCol);
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get list from sp5 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(testEntities.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", testEntities.Count, TestUtils.TestCollectionSize);

            foreach (var row in testEntities)
            {
                TestEntityAutoProperties item = row.Get<TestEntityAutoProperties>("prefix");

                Assert.IsTrue(item.TestEntityAutoPropertiesID > 0, "Entity was not loaded!");

                Assert.IsTrue(item.Name == string.Format(TestUtils.NameFormatString, item.TestEntityAutoPropertiesID),
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Name = '{1}' instead of Name = '{2}'.",
                    item.TestEntityAutoPropertiesID,
                    string.Format(TestUtils.NameFormatString, item.TestEntityAutoPropertiesID),
                    item.Name
                    );

                Assert.IsTrue(item.TestEntityAutoPropertiesOrder == (item.TestEntityAutoPropertiesID + 49) % 100 + 1,
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Order = '{1}' instead of Order = '{2}'.",
                    item.TestEntityAutoPropertiesID,
                    (item.TestEntityAutoPropertiesID + 49) % 100 + 1,
                    item.TestEntityAutoPropertiesOrder
                    );
            }
        }

        [TestMethod]
        public async Task Get_ListFromSP5_PropCollection_ExcludeProperty()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            GRTable testEntities = null;

            GRPropertyCollection propCol = new GRPropertyCollection();
            propCol.AddType<TestEntityAutoProperties>("prefix");
            propCol.RemoveProperty<TestEntityAutoProperties>("prefix", p => p.Name);

            try
            {
                testEntities = await context.GetEntitiesFromSPAsync("spGetTestEntityAutoPropertiesPrefixed", null, propCol);
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get list from sp5 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(testEntities.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", testEntities.Count, TestUtils.TestCollectionSize);

            foreach (var row in testEntities)
            {
                TestEntityAutoProperties item = row.Get<TestEntityAutoProperties>("prefix");

                Assert.IsTrue(item.TestEntityAutoPropertiesID > 0, "Entity was not loaded!");

                Assert.IsTrue(item.Name == null, "Entity name was loaded but it shouldn't.");

                Assert.IsTrue(item.TestEntityAutoPropertiesOrder == (item.TestEntityAutoPropertiesID + 49) % 100 + 1,
                    "Entity name was loaded incorrectly! Entity with ID = {0} should have Order = '{1}' instead of Order = '{2}'.",
                    item.TestEntityAutoPropertiesID,
                    (item.TestEntityAutoPropertiesID + 49) % 100 + 1,
                    item.TestEntityAutoPropertiesOrder
                    );
            }
        }

        [TestMethod]
        public async Task Get_ValueFromSP()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            int result = -1;

            try
            {
                result = await context.GetValueFromSPAsync<int>("spGetTestEntityAutoPropertiesPrefixed");
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get value from sp5 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(result == 1, $"Parsed value {result} does not correspond to expected value 1.");
        }

        [TestMethod]
        public async Task Get_ValuesFromSP_FirstColumn()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            List<int> result = null;

            try
            {
                result = await context.GetValuesFromSPAsync<int>("spGetTestEntityAutoPropertiesPrefixed");
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get list from sp5 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(result.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", result.Count, TestUtils.TestCollectionSize);

            for (int i = 0; i < result.Count; i++)
            {
                Assert.IsTrue(result[i] == i + 1, $"Parsed value {result[i]} does not correspond to expected value {i + 1}.");
            }
        }

        [TestMethod]
        public async Task Get_ValuesFromSP_NamedColumn()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            List<int> result = null;

            try
            {
                result = await context.GetValuesFromSPAsync<int>("spGetTestEntityAutoPropertiesPrefixed", "prefix_TestEntityAutoPropertiesOrder");
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get list from sp5 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(result.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", result.Count, TestUtils.TestCollectionSize);

            for (int i = 0; i < result.Count; i++)
            {
                int res = result[i];
                int exp = (50 + i) % 100 + 1;
                Assert.IsTrue(res == exp, $"Parsed value {exp} on row {i+1} does not correspond to expected value {res}.");
            }
        }

        [TestMethod]
        public async Task Test_ExecuteSPWithJsonOutParamsAsync()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            var result = await context.GetEntitiesFromSPWithSingleJsonOutParamAsync<TestEntityAutoProperties>("spGetJsonTestEntityAutoPropertiesPrefixed", new List<SqlParameter>
            {
                new SqlParameter("@input", "adssadasd"){ Direction = ParameterDirection.Input },
                new SqlParameter("@jsonOutput", SqlDbType.NVarChar, -1){ Direction = ParameterDirection.Output }
            });

            Assert.IsTrue(result.Count == 100);
        }
    }
}
