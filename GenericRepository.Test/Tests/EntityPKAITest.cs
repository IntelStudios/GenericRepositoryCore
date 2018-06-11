using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Test.Models;
using GenericRepository.Test.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GenericRepository.Test
{
    [TestClass]
    public class EntityPKAITest
    {
        static string dbBaseName = "xeelo-tests-gr-insert-pkai";
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
        public void Test_PrimaryKeys_AI()
        {
            TestEntityAIPKRepository grEntities = TestUtils.GetTestEntityAIPKRepository(dbName);

            int entityId = 99;

            TestEntityAIPK entity = new TestEntityAIPK()
            {
                TestEntityAIPKID = entityId,
                TestEntityAIPKName = "Entity " + entityId
            };

            IGRUpdatable<TestEntityAIPK> updatable = null;

            try
            {
                updatable = grEntities.GRInsert(entity);
                updatable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to insert entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(updatable.ExecutionStats.AffectedRows == 1, "Only single entity should be inserted.");

            Assert.IsTrue(entity.TestEntityAIPKID != entityId, "Entity ID was not changed.");

            TestEntityAIPK entityDB = grEntities.GRGet(entity.TestEntityAIPKID);

            Assert.IsTrue(entityDB != null, "Entity was not found.");

            int newEntityId = entity.TestEntityAIPKID;

            // trying to save the same entity
            IGRUpdatable<TestEntityAIPK> updatable2 = null;

            try
            {
                updatable2 = grEntities.GRInsert(entity);
                updatable2.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to save new entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(entity.TestEntityAIPKID != newEntityId, "AIID was not loaded");
        }
    }
}
