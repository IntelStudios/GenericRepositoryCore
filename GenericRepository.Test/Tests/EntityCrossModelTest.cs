using GenericRepository.Helpers;
using GenericRepository.Test.Models;
using GenericRepository.Test.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace GenericRepository.Test.Tests
{
    [TestClass]
    public class EntityCrossModelTest
    {
        static string dbBaseName = "xeelo-tests-gr-join-types";
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
        public void Get_Entitity_Cross_Model()
        {
            TestEntityAutoPropertiesRepository grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);
            TestEntityJoining joiningEntity = grEntities.CrossGRGetAsync(1).GetAwaiter().GetResult();

            Assert.IsTrue(joiningEntity.TestEntityJoiningID == 1, "Returned entity with ID {0} instead of {1}.", joiningEntity.TestEntityJoiningID, 1);
        }

        [TestMethod]
        public void Update_Entitity_Cross_Model()
        {
            TestEntityAutoPropertiesRepository grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);

            TestEntityJoining joiningEntity = grEntities.CrossGRGetAsync(2).GetAwaiter().GetResult();

            joiningEntity.Description = "Changed description";

            grEntities.GRUpdate<TestEntityJoining>(joiningEntity).GRExecuteAsync().GetAwaiter().GetResult();

            TestEntityJoining updatedEntity = grEntities.CrossGRGetAsync(2).GetAwaiter().GetResult();

            Assert.IsTrue(updatedEntity.Description == joiningEntity.Description, "Entity description was not updated.");
        }

        [TestMethod]
        public void Insert_Entitity_Cross_Model()
        {
            TestEntityAutoPropertiesRepository grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);

            TestEntityJoining joiningEntity = new TestEntityJoining()
            {
                Description = "New entity",
                TestEntityAutoPropertiesID = 1
            };

            grEntities.GRInsert<TestEntityJoining>(joiningEntity).GRExecuteAsync().GetAwaiter().GetResult();

            Assert.IsTrue(joiningEntity.TestEntityJoiningID > 0, "Pripary key was not assigned.");
        }

        [TestMethod]
        public void Delete_Entitity_Cross_Model()
        {
            TestEntityAutoPropertiesRepository grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);

            TestEntityJoining joiningEntity = new TestEntityJoining()
            {
                Description = "New entity",
                TestEntityAutoPropertiesID = 1
            };

            grEntities.GRInsert<TestEntityJoining>(joiningEntity).GRExecuteAsync().GetAwaiter().GetResult();

            Assert.IsTrue(joiningEntity.TestEntityJoiningID > 0, "Pripary key was not assigned.");

            int id = joiningEntity.TestEntityJoiningID;

            grEntities.GRDelete<TestEntityJoining>(joiningEntity).GRExecuteAsync().GetAwaiter().GetResult();

            TestEntityJoining deletedEntity = grEntities.CrossGRGetAsync(id).GetAwaiter().GetResult();

            Assert.IsTrue(deletedEntity == null, "Entity was not deleted.");
        }

        [TestMethod]
        public void Get_Entities_Cross_Model_GT_50()
        {
            TestEntityAutoPropertiesRepository grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);

            List<TestEntityJoining> testEntities = null;

            try
            {
                testEntities = grEntities.CrossWhere().GetAwaiter().GetResult();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entitites ID > 50 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            foreach (var item in testEntities)
            {
                Assert.IsTrue(item.TestEntityJoiningID > 50, "Wrong entity ID = {0} was returned!", item.TestEntityJoiningID);
            }
        }
    }
}
