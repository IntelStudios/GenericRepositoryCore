using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Test.Models;
using GenericRepository.Test.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GenericRepository.Test
{
    [TestClass]
    public class EntityInsertTest
    {
        static string dbBaseName = "xeelo-tests-gr-insert";
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
        public void Insert_Entitity_Direct()
        {
            TestEntityAutoPropertiesRepository grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);

            TestEntityAutoProperties entity = new TestEntityAutoProperties();
            entity.Name = "NEW NAME";
            entity.TestEntityAutoPropertiesDescription = "NEW DESCRIPTION";
            entity.TestEntityAutoPropertiesOrder = 999;

            IGRUpdatable<TestEntityAutoProperties> updatable = null;

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

            Assert.IsTrue(entity.TestEntityAutoPropertiesID > 0, "Entity does not have assigned ID.");

            TestEntityAutoProperties entityDB = grEntities.GRGet(entity.TestEntityAutoPropertiesID);

            Assert.IsTrue(entityDB != null, "Entity was not found.");

            Assert.IsTrue(entity.Name == entityDB.Name, "Entity name was not saved.");
            Assert.IsTrue(entity.TestEntityAutoPropertiesOrder == entityDB.TestEntityAutoPropertiesOrder, "Entity order was not saved.");

            Assert.IsTrue(grEntities.ServerTime == entityDB.ModifiedDate, "Entity ModifiedDate was not updated.");
            Assert.IsTrue(grEntities.UserID == entityDB.ModifiedBy, "Entity ModifiedBy was not updated.");

            Assert.IsTrue(grEntities.ServerTime == entityDB.CreatedDate, "Entity CreatedDate was not updated.");
            Assert.IsTrue(grEntities.UserID == entityDB.CreatedBy, "Entity CreatedBy was not updated.");
        }

        [TestMethod]
        public void Insert_Entitity_Queue()
        {
            RepositoryCollection repoWithContext = TestUtils.GetRepositories(dbName);

            List<IGRUpdatable<TestEntityAutoProperties>> updatables = new List<IGRUpdatable<TestEntityAutoProperties>>();

            for (int i = 0; i < 10; i++)
            {
                TestEntityAutoProperties entity = new TestEntityAutoProperties();
                entity.Name = "NEW NAME " + (i + 1);
                entity.TestEntityAutoPropertiesDescription = "NEW DESCRIPTION" + (i + 1);
                entity.TestEntityAutoPropertiesOrder = 1000 + (i + 1);

                updatables.Add(repoWithContext.TestEntityAutoPropertiesRepository.GRInsert(entity));
            }

            try
            {
                repoWithContext.Context.SaveChanges();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to save queue of new entities - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            foreach (var item in updatables)
            {
                Assert.IsTrue(item.Entity.TestEntityAutoPropertiesID > 0, "Entity does not have assigned ID.");

                TestEntityAutoProperties entityDB = repoWithContext.TestEntityAutoPropertiesRepository.GRGet(item.Entity.TestEntityAutoPropertiesID);

                Assert.IsTrue(entityDB != null, "Entity was not found.");

                Assert.IsTrue(item.Entity.Name == entityDB.Name, "Entity name was not saved.");
                Assert.IsTrue(item.Entity.TestEntityAutoPropertiesOrder == entityDB.TestEntityAutoPropertiesOrder, "Entity order was not saved.");

                Assert.IsTrue(((TestEntityAutoPropertiesRepository)repoWithContext.TestEntityAutoPropertiesRepository).ServerTime == entityDB.ModifiedDate, "Entity ModifiedDate was not updated.");
                Assert.IsTrue(((TestEntityAutoPropertiesRepository)repoWithContext.TestEntityAutoPropertiesRepository).UserID == entityDB.ModifiedBy, "Entity ModifiedBy was not updated.");

                Assert.IsTrue(((TestEntityAutoPropertiesRepository)repoWithContext.TestEntityAutoPropertiesRepository).ServerTime == entityDB.CreatedDate, "Entity CreatedDate was not updated.");
                Assert.IsTrue(((TestEntityAutoPropertiesRepository)repoWithContext.TestEntityAutoPropertiesRepository).UserID == entityDB.CreatedBy, "Entity CreatedBy was not updated.");

            }
        }

        [TestMethod]
        public void Insert_Entitity_Binary_Stream()
        {
            string testString = "Hello";
            byte[] testBytes = Encoding.UTF8.GetBytes(testString);
            Stream stream = new MemoryStream(testBytes);

            TestEntityBinaryStreamRepository grEntities = TestUtils.GetTestEntityBinaryStreamRepository(dbName);

            TestEntityBinaryStream entity = new TestEntityBinaryStream();
            entity.TestEntityBinaryData = stream;

            IGRUpdatable<TestEntityBinaryStream> updatable = null;

            try
            {
                updatable = grEntities.GRInsert(entity);
                updatable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to insert entity with binary content - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(updatable.ExecutionStats.AffectedRows == 1, "Only single entity should be inserted.");

            Assert.IsTrue(entity.TestEntityBinaryID > 0, "Entity does not have assigned ID.");

            TestEntityBinaryStream entityDB = grEntities.GRGet(entity.TestEntityBinaryID);

            Assert.IsTrue(entityDB != null, "Entity was not found.");

            byte[] bytes = TestUtils.ConvertToByteArray(entityDB.TestEntityBinaryData);

            Assert.IsTrue(Encoding.UTF8.GetString(bytes).Equals(testString), "Entity data was corrupted.");
        }

        [TestMethod]
        public void Insert_Entitity_Binary_Stream_Null()
        {
            TestEntityBinaryStreamRepository grEntities = TestUtils.GetTestEntityBinaryStreamRepository(dbName);

            TestEntityBinaryStream entity = new TestEntityBinaryStream();

            IGRUpdatable<TestEntityBinaryStream> updatable = null;

            try
            {
                updatable = grEntities.GRInsert(entity);
                updatable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to insert entity with binary content - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(updatable.ExecutionStats.AffectedRows == 1, "Only single entity should be inserted.");

            Assert.IsTrue(entity.TestEntityBinaryID > 0, "Entity does not have assigned ID.");

            TestEntityBinaryStream entityDB = grEntities.GRGet(entity.TestEntityBinaryID);

            Assert.IsTrue(entityDB != null, "Entity was not found.");

            Assert.IsTrue(entityDB.TestEntityBinaryData == null, "Entity data was corrupted.");
        }

        [TestMethod]
        public void Insert_Entitity_Binary_Array()
        {
            string testString = "Hello";
            byte[] testBytes = Encoding.UTF8.GetBytes(testString);

            TestEntityBinaryArrayRepository grEntities = TestUtils.GetTestEntityBinaryArrayRepository(dbName);

            TestEntityBinaryArray entity = new TestEntityBinaryArray();
            entity.TestEntityBinaryData = testBytes;

            IGRUpdatable<TestEntityBinaryArray> updatable = null;

            try
            {
                updatable = grEntities.GRInsert(entity);
                updatable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to insert entity with binary content - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(updatable.ExecutionStats.AffectedRows == 1, "Only single entity should be inserted.");

            Assert.IsTrue(entity.TestEntityBinaryID > 0, "Entity does not have assigned ID.");

            TestEntityBinaryArray entityDB = grEntities.GRGet(entity.TestEntityBinaryID);

            Assert.IsTrue(entityDB != null, "Entity was not found.");

            Assert.IsTrue(Encoding.UTF8.GetString(entityDB.TestEntityBinaryData).Equals(testString), "Entity data was corrupted.");
        }

        [TestMethod]
        public void Insert_Entitity_Binary_Array_Null()
        {
            TestEntityBinaryArrayRepository grEntities = TestUtils.GetTestEntityBinaryArrayRepository(dbName);

            TestEntityBinaryArray entity = new TestEntityBinaryArray();

            IGRUpdatable<TestEntityBinaryArray> updatable = null;

            try
            {
                updatable = grEntities.GRInsert(entity);
                updatable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to insert entity with binary content - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(updatable.ExecutionStats.AffectedRows == 1, "Only single entity should be inserted.");

            Assert.IsTrue(entity.TestEntityBinaryID > 0, "Entity does not have assigned ID.");

            TestEntityBinaryArray entityDB = grEntities.GRGet(entity.TestEntityBinaryID);

            Assert.IsTrue(entityDB != null, "Entity was not found.");

            Assert.IsTrue(entityDB.TestEntityBinaryData == null, "Entity data was corrupted.");
        }

        [TestMethod]
        public void Insert_Entitity_Direct_Primitive_Null()
        {
            TestEntityPrimitiveNullRepository grEntities = TestUtils.GetTestEntityPrimitiveNullRepository(dbName);

            TestEntityPrimitiveNull entity = new TestEntityPrimitiveNull();
            entity.TestEntityPrimitiveNullInt = null;
            entity.TestEntityPrimitiveNullBool = null;
            entity.TestEntityPrimitiveNullDate = null;
            entity.TestEntityPrimitiveNullString = null;

            IGRUpdatable<TestEntityPrimitiveNull> updatable = null;

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

            Assert.IsTrue(entity.TestEntityPrimitiveNullID > 0, "Entity does not have assigned ID.");

            TestEntityPrimitiveNull entityDB = grEntities.GRGet(entity.TestEntityPrimitiveNullID);

            Assert.IsTrue(entityDB != null, "Entity was not found.");

            Assert.IsTrue(entity.TestEntityPrimitiveNullInt == entityDB.TestEntityPrimitiveNullInt, "Entity int was not saved.");
            Assert.IsTrue(entityDB.TestEntityPrimitiveNullInt == null, "Entity int is not null.");
            Assert.IsTrue(entity.TestEntityPrimitiveNullBool == entityDB.TestEntityPrimitiveNullBool, "Entity bool was not saved.");
            Assert.IsTrue(entityDB.TestEntityPrimitiveNullBool == null, "Entity bool is not null.");
            Assert.IsTrue(entity.TestEntityPrimitiveNullDate == entityDB.TestEntityPrimitiveNullDate, "Entity date was not saved.");
            Assert.IsTrue(entityDB.TestEntityPrimitiveNullDate == null, "Entity date is not null.");
            Assert.IsTrue(entity.TestEntityPrimitiveNullString == entityDB.TestEntityPrimitiveNullString, "Entity string was not saved.");
            Assert.IsTrue(entityDB.TestEntityPrimitiveNullString == null, "Entity string is not null.");

        }
    }
}
