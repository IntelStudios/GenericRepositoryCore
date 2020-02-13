using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.SqlClient;
using GenericRepository.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenericRepository.Helpers;

namespace GenericRepository.Test.DBProgrammability
{
    [TestClass]
    public class DBFunction
    {
        static string dbBaseName = "xeelo-tests-gr-fn";
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

        [TestMethod]
        public async Task Execute_FN2_10()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            int value = -1;

            try
            {
                value = await context.ExecuteScalarFunctionAsync<int>("fnParseInt", new List<SqlParameter> { new SqlParameter("@string", 10) });
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to execute scalar function - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(value == 10, "Returned {0} instead of {1}.", value, 10);
        }

        [TestMethod]
        public async Task Execute_FN2_aa()
        {
            IGRContext context = TestUtils.GetContext(dbName);

            int value = -1;

            try
            {
                value = await context.ExecuteScalarFunctionAsync<int>("fnParseInt", new List<SqlParameter> { new SqlParameter("@string", "aa") });
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to execute scalar function - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(value == 0, "Returned {0} instead of {1}.", value, 0);
        }        
    }
}
