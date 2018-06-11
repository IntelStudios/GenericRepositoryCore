using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Models;
using GenericRepository.Test.Models;
using GenericRepository.Test.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace GenericRepository.Test
{
    [TestClass]
    public class EntityDeleteTest
    {
        static string dbBaseName = "xeelo-tests-gr-delete";
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
        public async Task Delete_Entitity()
        {
            var repoWithContext = TestUtils.GetRepositories(dbName);

            TestEntityAutoPropertiesRepository grEntities = repoWithContext.TestEntityAutoPropertiesRepository;
            IGRDeletable<TestEntityJoining> entity2Deletable = new GRDeletable<TestEntityJoining>(repoWithContext.Context, null, null);

            // bulk delete from Entity2Table
            try
            {
                entity2Deletable.GRWhere(e => e.TestEntityJoiningID > 50);
                entity2Deletable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to delete entites with ID > 50 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(entity2Deletable.ExecutionStats.AffectedRows == 50, "Wrong count of lines ({0}) were affected.", entity2Deletable.ExecutionStats.AffectedRows);

            GRQueriable<TestEntityJoining> allE2EntitiesQueriable = new GRQueriable<TestEntityJoining>(repoWithContext.Context);

            var allEntities2 = allE2EntitiesQueriable.GRToList();

            foreach (var e2 in allEntities2)
            {
                Assert.IsTrue(e2.TestEntityJoiningID <= 50, "Wrong entity (ID = {0}) was left in database.", e2.TestEntityJoiningID);
            }

            Assert.IsTrue(allEntities2.Count == 50, "Wrong count ({0}) of entities left in database.", allEntities2.Count);

            // deleting single entity 1
            TestEntityAutoProperties entity = grEntities.GRGet(1);

            IGRDeletable<TestEntityAutoProperties> entity1Deletable = null;

            try
            {
                entity1Deletable = grEntities.GRDelete(entity);
                entity1Deletable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to delete entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            TestEntityAutoProperties updatedEntity = grEntities.GRGet(1);

            Assert.IsTrue(entity1Deletable.ExecutionStats.AffectedRows == 1, "Multiple lines ({0}) were affected.", entity1Deletable.ExecutionStats.AffectedRows);

            TestEntityAutoProperties deletedEntity = grEntities.GRGet(1);

            Assert.IsTrue(deletedEntity == null, "Entity was not deleted.");

            var allEntities = await grEntities.GRAll().GRToListAsync();

            Assert.IsTrue(allEntities.Count == 99, "Wrong count ({0}) of entities left in database.", allEntities.Count);
        }
    }
}
