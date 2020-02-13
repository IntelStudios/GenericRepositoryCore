using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenericRepository.Test.Models;
using GenericRepository.Interfaces;
using System.Collections.Generic;
using System.Linq;
using GenericRepository.Models;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using GenericRepository.Helpers;

namespace GenericRepository.Test.Autoproperties
{
    [TestClass]
    public class Autoproperties
    {
        static string dbBaseName = "xeelo-tests-gr-autoproperties";
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
        public async Task Get_Entities_TestEntityMultiID_1()
        {
            IGRRepository<TestEntityMultiID_1> grEntities = TestUtils.GetTestEntityMultiID_1Repository(dbName);

            List<TestEntityMultiID_1> testEntities = null;

            try
            {
                testEntities = await grEntities.GRAll().GRToListAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get TestEntityMultiID_1 entitites. {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            int expectedCount = 10;

            Assert.IsTrue(testEntities.Count == expectedCount, "Returned {0} entities instead of {1}.", testEntities.Count, expectedCount);

            foreach (var item in testEntities)
            {
                Assert.IsTrue(item.ID == item.TestEntityMulti1ID, "Wrong entity ID = {0} was returned!", item.ID);
                Assert.IsTrue(item.ID + 100 == item.TestEntityMulti2ID, "Wrong entity ID = {0} was returned!", item.ID);
            }
        }

        [TestMethod]
        public async Task Get_Entities_TestEntityMultiID_2()
        {
            IGRRepository<TestEntityMultiID_2> grEntities = TestUtils.GetTestEntityMultiID_2Repository(dbName);

            List<TestEntityMultiID_2> testEntities = null;

            try
            {
                testEntities = await grEntities.GRAll().GRToListAsync();
                Assert.Fail("Entitites should not be loaded.");
            }
            catch (Exception)
            {
                
            }
        }

        [TestMethod]
        public async Task Get_Entities_TestEntityMultiID_3()
        {
            IGRRepository<TestEntityMultiID_3> grEntities = TestUtils.GetTestEntityMultiID_3Repository(dbName);

            List<TestEntityMultiID_3> testEntities = null;

            try
            {
                testEntities = await grEntities.GRAll().GRToListAsync();
                Assert.Fail("Entitites should not be loaded.");
            }
            catch (Exception)
            {

            }
        }
    }
}
