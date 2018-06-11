using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Test.Models;
using GenericRepository.Test.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GenericRepository.Test
{
    [TestClass]
    public class EntityPKTest
    {
        static string dbBaseName = "xeelo-tests-gr-insert-pk";
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
        public void Test_PrimaryKeys()
        {
            TestEntityPKRepository grEntities = TestUtils.GetTestEntityPKRepository(dbName);

            int entityId = 99;

            TestEntityPK entity = new TestEntityPK()
            {
                TestEntityPKID = entityId,
                TestEntityPKName = "Entity " + entityId
            };

            IGRUpdatable<TestEntityPK> updatable = null;

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

            Assert.IsTrue(entity.TestEntityPKID == entityId, "Entity ID was changed.");

            TestEntityPK entityDB = grEntities.GRGet(entityId);

            Assert.IsTrue(entityDB != null, "Entity was not found.");

            // trying to save the same entity
            IGRUpdatable<TestEntityPK> updatable2 = null;

            try
            {
                updatable2 = grEntities.GRInsert(entity);
                updatable2.GRExecute();
                Assert.Fail("Duplicated entity was inserted - {0}.");
            }
            catch (Exception exc)
            {
            }
        }
    }
}
