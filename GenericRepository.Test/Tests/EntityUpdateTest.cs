using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Test.Models;
using GenericRepository.Test.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace GenericRepository.Test
{
    [TestClass]
    public class EntityUpdateTest
    {
        static string dbBaseName = "xeelo-tests-gr-update";
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
        public void Update_Entitity_Direct()
        {
            TestEntityAutoPropertiesRepository grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);

            TestEntityAutoProperties entity = grEntities.GRGet(1);

            IGRUpdatable<TestEntityAutoProperties> updatable = null;

            try
            {
                updatable = grEntities.GRUpdate(entity);
                updatable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to update entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            TestEntityAutoProperties updatedEntity = grEntities.GRGet(1);

            Assert.IsTrue(updatable.ExecutionStats.AffectedRows == 1, "Multiple lines ({0}) were affected.", updatable.ExecutionStats.AffectedRows);
            
            Assert.IsTrue(entity.Name == updatedEntity.Name, "Entity name was not saved.");
            Assert.IsTrue(entity.TestEntityAutoPropertiesOrder == updatedEntity.TestEntityAutoPropertiesOrder, "Entity order was not saved.");

            Assert.IsTrue(grEntities.ServerTime == updatedEntity.ModifiedDate, "Entity ModifiedDate was not updated.");
            Assert.IsTrue(grEntities.UserID == updatedEntity.ModifiedBy, "Entity ModifiedBy was not updated.");

            Assert.IsTrue(TestUtils.DefaultCreatedDate == updatedEntity.CreatedDate, "Entity CreatedDate was updated.");
            Assert.IsTrue(updatedEntity.CreatedBy == -1, "Entity CreatedBy was updated.");
        }

        [TestMethod]
        public void Update_Entitity_Include_IsActive()
        {
            TestEntityAutoPropertiesRepository grEntities = TestUtils.GetTestEntityAutoPropertiesRepository(dbName);

            TestEntityAutoProperties entityToUpdate = grEntities.GRGet(1);
            TestEntityAutoProperties entityUnchanged = grEntities.GRGet(1);

            DateTime fakeDate = new DateTime(2020, 1, 2);
            int fakeUser = 666;

            entityToUpdate.ModifiedDate = fakeDate;
            entityToUpdate.CreatedDate = fakeDate;
            entityToUpdate.ModifiedBy = fakeUser;
            entityToUpdate.CreatedBy = fakeUser;

            entityToUpdate.IsActive = false;
            entityToUpdate.Name = "Changed";
            entityToUpdate.TestEntityAutoPropertiesOrder = -999;

            IGRUpdatable<TestEntityAutoProperties> updatable = null;

            try
            {
                updatable = grEntities.GRUpdate(entityToUpdate).GRInclude(e => e.IsActive);
                updatable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to update entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            TestEntityAutoProperties updatedEntity = grEntities.GRGet(1);

            Assert.IsTrue(updatable.ExecutionStats.AffectedRows == 1, "Multiple lines ({0}) were affected.", updatable.ExecutionStats.AffectedRows);

            Assert.IsTrue(updatedEntity.IsActive == false, "Entity IsActive was not saved.");
            

            Assert.IsTrue(updatedEntity.Name == entityUnchanged.Name, "Entity name was saved.");
            Assert.IsTrue(updatedEntity.TestEntityAutoPropertiesOrder == entityUnchanged.TestEntityAutoPropertiesOrder, "Entity order was saved.");

            Assert.IsTrue(grEntities.ServerTime == updatedEntity.ModifiedDate, "Entity ModifiedDate was not updated.");
            Assert.IsTrue(grEntities.UserID == updatedEntity.ModifiedBy, "Entity ModifiedBy was not updated.");

            Assert.IsTrue(TestUtils.DefaultCreatedDate == updatedEntity.CreatedDate, "Entity CreatedDate was updated.");
            Assert.IsTrue(updatedEntity.CreatedBy == -1, "Entity CreatedBy was updated.");
        }

        [TestMethod]
        public void Update_Entitity_Binary_Array_To_Null()
        {
            string teststring = "hello";
            byte[] testbytes = Encoding.UTF8.GetBytes(teststring);

            TestEntityBinaryArrayRepository grentities = TestUtils.GetTestEntityBinaryArrayRepository(dbName);

            IGRUpdatable<TestEntityBinaryArray> updatable = null;

            TestEntityBinaryArray entity = new TestEntityBinaryArray()
            {
                TestEntityBinaryData = testbytes
            };

            try
            {
                updatable = grentities.GRInsert(entity);
                updatable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("unable to update entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            TestEntityBinaryArray updatedentity = grentities.GRGet(entity.TestEntityBinaryID);

            Assert.IsTrue(updatedentity.TestEntityBinaryData.SequenceEqual(entity.TestEntityBinaryData), "Data was corrupted.", updatable.ExecutionStats.AffectedRows);

            // setting to null
            entity.TestEntityBinaryData = null;
            try
            {
                updatable = grentities.GRUpdate(entity);
                updatable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("unable to update entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            updatedentity = grentities.GRGet(entity.TestEntityBinaryID);

            Assert.IsTrue(updatedentity.TestEntityBinaryData == null, "data was not wet to null.", updatable.ExecutionStats.AffectedRows);

            // setting back
            entity.TestEntityBinaryData = testbytes;
            try
            {
                updatable = grentities.GRUpdate(entity);
                updatable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("unable to update entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            updatedentity = grentities.GRGet(entity.TestEntityBinaryID);

            Assert.IsTrue(updatedentity.TestEntityBinaryData.SequenceEqual(testbytes), "data was corrupted.", updatable.ExecutionStats.AffectedRows);
        }

        [TestMethod]
        public void Update_Entitity_Binary_Stream_To_Null()
        {
            string teststring = "hello";
            byte[] testbytes = Encoding.UTF8.GetBytes(teststring);
            Stream stream = new MemoryStream(testbytes);

            TestEntityBinaryStreamRepository grentities = TestUtils.GetTestEntityBinaryStreamRepository(dbName);

            IGRUpdatable<TestEntityBinaryStream> updatable = null;

            TestEntityBinaryStream entity = new TestEntityBinaryStream()
            {
                TestEntityBinaryData = stream
            };

            try
            {
                updatable = grentities.GRInsert(entity);
                updatable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("unable to update entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            TestEntityBinaryStream updatedentity = grentities.GRGet(entity.TestEntityBinaryID);
            byte[] updatedbytes = TestUtils.ConvertToByteArray(updatedentity.TestEntityBinaryData);

            Assert.IsTrue(updatedbytes.SequenceEqual(testbytes), "data was corrupted.", updatable.ExecutionStats.AffectedRows);

            // setting to null
            entity.TestEntityBinaryData = null;
            try
            {
                updatable = grentities.GRUpdate(entity);
                updatable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("unable to update entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            updatedentity = grentities.GRGet(entity.TestEntityBinaryID);

            Assert.IsTrue(updatedentity.TestEntityBinaryData == null, "data was not set to null.", updatable.ExecutionStats.AffectedRows);

            // setting back
            entity.TestEntityBinaryData = new MemoryStream(testbytes);
            try
            {
                updatable = grentities.GRUpdate(entity);
                updatable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("unable to update entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            updatedentity = grentities.GRGet(entity.TestEntityBinaryID);

            updatedbytes = TestUtils.ConvertToByteArray(updatedentity.TestEntityBinaryData);

            Assert.IsTrue(updatedbytes.SequenceEqual(testbytes), "Data was corrupted.", updatable.ExecutionStats.AffectedRows);
        }

        [TestMethod]
        public void Update_Entitity_Direct_Primitive_To_Null()
        {
            int testint = 1;
            bool testbool = true;
            DateTime testdate = DateTime.Now;
            testdate = DateTime.ParseExact(testdate.ToString("yyyy-MM-dd HH:mm:ss"), "yyyy-MM-dd HH:mm:ss", null);
            string teststring = "hello";

            TestEntityPrimitiveNullRepository grentities = TestUtils.GetTestEntityPrimitiveNullRepository(dbName);

            IGRUpdatable<TestEntityPrimitiveNull> updatable = null;

            TestEntityPrimitiveNull entity = new TestEntityPrimitiveNull()
            {
                TestEntityPrimitiveNullInt = testint,
                TestEntityPrimitiveNullBool = testbool,
                TestEntityPrimitiveNullDate = testdate,
                TestEntityPrimitiveNullString = teststring
            };

            try
            {
                updatable = grentities.GRInsert(entity);
                updatable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("unable to update entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            TestEntityPrimitiveNull updatedentity = grentities.GRGet(entity.TestEntityPrimitiveNullID);

            Assert.IsTrue(updatedentity.TestEntityPrimitiveNullInt == entity.TestEntityPrimitiveNullInt, "Data int was corrupted.", updatable.ExecutionStats.AffectedRows);
            Assert.IsTrue(updatedentity.TestEntityPrimitiveNullBool == entity.TestEntityPrimitiveNullBool, "Data bool was corrupted.", updatable.ExecutionStats.AffectedRows);
            Assert.IsTrue(DateTime.Equals(updatedentity.TestEntityPrimitiveNullDate, entity.TestEntityPrimitiveNullDate), "Data date was corrupted.", updatable.ExecutionStats.AffectedRows);
            Assert.IsTrue(updatedentity.TestEntityPrimitiveNullString == entity.TestEntityPrimitiveNullString, "Data string was corrupted.", updatable.ExecutionStats.AffectedRows);

            // setting to null
            entity.TestEntityPrimitiveNullInt = null;
            entity.TestEntityPrimitiveNullBool = null;
            entity.TestEntityPrimitiveNullDate = null;
            entity.TestEntityPrimitiveNullString = null;

            try
            {
                updatable = grentities.GRUpdate(entity);
                updatable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("unable to update entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            updatedentity = grentities.GRGet(entity.TestEntityPrimitiveNullID);

            Assert.IsTrue(updatedentity.TestEntityPrimitiveNullInt == null, "data was not wet to null.", updatable.ExecutionStats.AffectedRows);
            Assert.IsTrue(updatedentity.TestEntityPrimitiveNullBool == null, "data was not wet to null.", updatable.ExecutionStats.AffectedRows);
            Assert.IsTrue(updatedentity.TestEntityPrimitiveNullDate == null, "data was not wet to null.", updatable.ExecutionStats.AffectedRows);
            Assert.IsTrue(updatedentity.TestEntityPrimitiveNullString == null, "data was not wet to null.", updatable.ExecutionStats.AffectedRows);

            // setting back
            entity.TestEntityPrimitiveNullInt = testint;
            entity.TestEntityPrimitiveNullBool = testbool;
            entity.TestEntityPrimitiveNullDate = testdate;
            entity.TestEntityPrimitiveNullString = teststring;

            try
            {
                updatable = grentities.GRUpdate(entity);
                updatable.GRExecute();
            }
            catch (Exception exc)
            {
                Assert.Fail("unable to update entity - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            updatedentity = grentities.GRGet(entity.TestEntityPrimitiveNullID);

            Assert.IsTrue(updatedentity.TestEntityPrimitiveNullInt == entity.TestEntityPrimitiveNullInt, "Data int was corrupted.", updatable.ExecutionStats.AffectedRows);
            Assert.IsTrue(updatedentity.TestEntityPrimitiveNullBool == entity.TestEntityPrimitiveNullBool, "Data bool was corrupted.", updatable.ExecutionStats.AffectedRows);
            Assert.IsTrue(DateTime.Equals(updatedentity.TestEntityPrimitiveNullDate, entity.TestEntityPrimitiveNullDate), "Data date was corrupted.", updatable.ExecutionStats.AffectedRows);
            Assert.IsTrue(updatedentity.TestEntityPrimitiveNullString == entity.TestEntityPrimitiveNullString, "Data string was corrupted.", updatable.ExecutionStats.AffectedRows);
        }
    }
}
