using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Test.Models;
using GenericRepository.Test.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace GenericRepository.Test.CRUD
{
    [TestClass]
    public class PrimaryKeys
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
        public void PrimaryKey_AutoIncrement_Insert_Get()
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
                updatable = grEntities.GREnqueueInsert(entity);
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
                updatable2 = grEntities.GREnqueueInsert(entity);
                updatable2.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to save new entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(entity.TestEntityAIPKID != newEntityId, "AIID was not loaded");
        }

        [TestMethod]
        public void PrimaryKey_Insert_Get()
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
                updatable = grEntities.GREnqueueInsert(entity);
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
                updatable2 = grEntities.GREnqueueInsert(entity);
                updatable2.GRExecute();
                Assert.Fail("Duplicated entity was inserted - {0}.");
            }
            catch
            {
            }
        }

        [TestMethod]
        public void PrimaryKeyMulti_Insert_Get()
        {
            TestEntityPKsRepository grEntities = TestUtils.GetTestEntityPKsRepository(dbName);

            int entityId = 99;

            TestEntityPKs entity = new TestEntityPKs()
            {
                TestEntityPKsID = entityId,
                TestEntityPKsID2 = entityId,
                TestEntityPKName = "Entity " + entityId
            };

            IGRUpdatable<TestEntityPKs> updatable = null;

            try
            {
                updatable = grEntities.GREnqueueInsert(entity);
                updatable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to insert entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(updatable.ExecutionStats.AffectedRows == 1, "Only single entity should be inserted.");

            Assert.IsTrue(entity.TestEntityPKsID == entityId, "Entity ID was changed.");

            TestEntityPKs entityDB = grEntities.GRGet<TestEntityPKs>(entity);

            Assert.IsTrue(entityDB != null, "Entity was not found.");

            // trying to save the same entity
            IGRUpdatable<TestEntityPKs> updatable2 = null;

            try
            {
                updatable2 = grEntities.GREnqueueInsert(entity);
                updatable2.GRExecute();
                Assert.Fail("Duplicated entity was inserted - {0}.");
            }
            catch
            {
            }
        }
    }
}
