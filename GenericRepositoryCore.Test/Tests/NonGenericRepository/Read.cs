using GenericRepository.Exceptions;
using GenericRepository.Helpers;
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using GenericRepository.Test.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Test.NonGenericRepository.CRUD
{
    [TestClass]
    public class Read
    {
        static string dbBaseName = "xeelo-tests-gr-ng-read";
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
        public async Task Get_Entities_All()
        {
            IGRRepository repo = TestUtils.GetNontGenericRepository(dbName);

            List<TestEntityAutoProperties> testEntities = null;

            try
            {
                testEntities = await repo.GRAll<TestEntityAutoProperties>().GRToListAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get all entitites - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(testEntities.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", testEntities.Count, TestUtils.TestCollectionSize);

            foreach (var item in testEntities)
            {
                Assert.IsTrue(item.TestEntityAutoPropertiesID > 0, "Entity was not loaded!");

                Assert.IsTrue(item.TestEntityAutoPropertiesDescription == null, "Entity description was loaded, it should be ignored!");

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

                Assert.IsTrue(item.CreatedDate == TestUtils.DefaultCreatedDate,
                    "Entity name was loaded incorrectly! Entity CreatedDate should be '{0}' instead of '{1}'.",
                    TestUtils.DefaultCreatedDate, item.CreatedDate);

                Assert.IsTrue(item.ModifiedDate == TestUtils.DefaultCreatedDate,
                    "Entity name was loaded incorrectly! Entity ModifiedDate should be '{0}' instead of '{1}'.",
                    TestUtils.DefaultCreatedDate, item.ModifiedDate);

                Assert.IsTrue(item.ModifiedBy == -1,
                    "Entity name was loaded incorrectly! Entity ModifiedBy should be '{0}' instead of '{1}'.",
                    -1, item.ModifiedBy);

                Assert.IsTrue(item.CreatedBy == -1,
                    "Entity name was loaded incorrectly! Entity CreatedBy should be '{0}' instead of '{1}'.",
                    -1, item.CreatedBy);

                Assert.IsTrue(item.IsActive,
                    "Entity name was loaded incorrectly! Entity IsActive should be TRUE instead of FALSE.");
            }

            List<int> allIds = testEntities.Select(e => e.TestEntityAutoPropertiesID).ToList();
            List<string> allNames = testEntities.Select(e => e.Name).ToList();

            Assert.IsTrue(allIds.Distinct().Count() == testEntities.Count, "Entity IDs are not unique!");
            Assert.IsTrue(allNames.Distinct().Count() == testEntities.Count, "Entity names are not unique!");
        }

        [TestMethod]
        public async Task Get_Entities_GRAll()
        {
            IGRRepository repo = TestUtils.GetNontGenericRepository(dbName);

            List<TestEntityAutoProperties> testEntities = null;

            try
            {
                testEntities = await repo.GRAll<TestEntityAutoProperties>().GRToListAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get all entitites - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(testEntities.Count == TestUtils.TestCollectionSize, "Returned {0} entities instead of {1}.", testEntities.Count, TestUtils.TestCollectionSize);

            foreach (var item in testEntities)
            {
                Assert.IsTrue(item.TestEntityAutoPropertiesID > 0, "Entity was not loaded!");

                Assert.IsTrue(item.TestEntityAutoPropertiesDescription == null, "Entity description was loaded, it should be ignored!");

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

                Assert.IsTrue(item.CreatedDate == TestUtils.DefaultCreatedDate,
                    "Entity name was loaded incorrectly! Entity CreatedDate should be '{0}' instead of '{1}'.",
                    TestUtils.DefaultCreatedDate, item.CreatedDate);

                Assert.IsTrue(item.ModifiedDate == TestUtils.DefaultCreatedDate,
                    "Entity name was loaded incorrectly! Entity ModifiedDate should be '{0}' instead of '{1}'.",
                    TestUtils.DefaultCreatedDate, item.ModifiedDate);

                Assert.IsTrue(item.ModifiedBy == -1,
                    "Entity name was loaded incorrectly! Entity ModifiedBy should be '{0}' instead of '{1}'.",
                    -1, item.ModifiedBy);

                Assert.IsTrue(item.CreatedBy == -1,
                    "Entity name was loaded incorrectly! Entity CreatedBy should be '{0}' instead of '{1}'.",
                    -1, item.CreatedBy);

                Assert.IsTrue(item.IsActive,
                    "Entity name was loaded incorrectly! Entity IsActive should be TRUE instead of FALSE.");
            }

            List<int> allIds = testEntities.Select(e => e.TestEntityAutoPropertiesID).ToList();
            List<string> allNames = testEntities.Select(e => e.Name).ToList();

            Assert.IsTrue(allIds.Distinct().Count() == testEntities.Count, "Entity IDs are not unique!");
            Assert.IsTrue(allNames.Distinct().Count() == testEntities.Count, "Entity names are not unique!");
        }

        [TestMethod]
        public async Task Get_Entities_GRExcludeAll()
        {
            IGRRepository repo = TestUtils.GetNontGenericRepository(dbName);

            List<TestEntityAutoProperties> testEntities;

            try
            {
                testEntities = await repo.GRAll<TestEntityAutoProperties>().GRExcludeAll().GRToListAsync();
                Assert.Fail("Entities without resulting columns should not be get.");
            }
            catch (GRInvalidOperationException)
            {
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get all excluded entitites - {0}.", GRStringHelpers.GetExceptionString(exc));
            }
        }

        [TestMethod]
        public async Task Get_Entities_Id_GT_50()
        {
            IGRRepository repo = TestUtils.GetNontGenericRepository(dbName);

            List<TestEntityAutoProperties> testEntities = null;

            try
            {
                testEntities = await repo.GRWhere<TestEntityAutoProperties>(e => e.TestEntityAutoPropertiesID > 50).GRToListAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entitites ID > 50 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            int expectedCount = 50;

            Assert.IsTrue(testEntities.Count == expectedCount, "Returned {0} entities instead of {1}.", testEntities.Count, expectedCount);

            foreach (var item in testEntities)
            {
                Assert.IsTrue(item.TestEntityAutoPropertiesID > 50, "Wrong entity ID = {0} was returned!", item.TestEntityAutoPropertiesID);
            }
        }

        [TestMethod]
        public void Get_Entities_Id_GT_50_Count()
        {
            IGRRepository repo = TestUtils.GetNontGenericRepository(dbName);

            int count = 0;

            try
            {
                count = repo.GRWhere<TestEntityAutoProperties>(e => e.TestEntityAutoPropertiesID > 50).GRCountAsync().GetAwaiter().GetResult();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get count of entitites ID > 50 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            int expectedCount = 50;

            Assert.IsTrue(count == expectedCount, "Returned count of {0} entities instead of {1}.", count, expectedCount);
        }

        [TestMethod]
        public void Get_Entities_All_Count()
        {
            IGRRepository repo = TestUtils.GetNontGenericRepository(dbName);

            int count = 0;

            try
            {
                count = repo.GRCount<TestEntityAutoProperties>();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get count of all entitites - {0}.", GRStringHelpers.GetExceptionString(exc));
            }


            Assert.IsTrue(count == TestUtils.TestCollectionSize, "Returned count of {0} entities instead of {1}.", count, TestUtils.TestCollectionSize);
        }

        [TestMethod]
        public async Task Get_Entities_Id_GT_10_LT_15()
        {
            IGRRepository repo = TestUtils.GetNontGenericRepository(dbName);

            List<TestEntityAutoProperties> testEntities = null;

            try
            {
                testEntities = await repo.GRWhere<TestEntityAutoProperties>(e => e.TestEntityAutoPropertiesID > 10 && e.TestEntityAutoPropertiesID < 15).GRToListAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entitites ID > 10 && ID < 15 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            foreach (var item in testEntities)
            {
                Assert.IsTrue(item.TestEntityAutoPropertiesID <= 10 || item.TestEntityAutoPropertiesID >= 10, "Wrong entity with ID = {0} was returned!", item.TestEntityAutoPropertiesID);
            }
        }

        [TestMethod]
        public async Task Get_Entities_Id_EQ_10()
        {
            IGRRepository repo = TestUtils.GetNontGenericRepository(dbName);

            List<TestEntityAutoProperties> testEntities = null;

            try
            {
                testEntities = await repo.GRWhere<TestEntityAutoProperties>(e => e.TestEntityAutoPropertiesID == 10).GRToListAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entitites ID == 10 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(testEntities.Count == 1, "Exactly single instance should be returned instead of {0} entities.", testEntities.Count);

            Assert.IsTrue(testEntities.Single().TestEntityAutoPropertiesID == 10, "Entity with ID = 10 should be returned instead of entitity with ID = {0}", testEntities.Single().TestEntityAutoPropertiesID);

            Assert.IsTrue(testEntities.Single().ID == 10, "Autoproperty [GRIDProperty] was not applied correctly.");
        }

        [TestMethod]
        public void Get_Entities_Id_GT_10_FirstOrDefault()
        {
            IGRRepository repo = TestUtils.GetNontGenericRepository(dbName);

            TestEntityAutoProperties testEntity = null;

            try
            {
                testEntity = repo.GRWhere<TestEntityAutoProperties>(e => e.TestEntityAutoPropertiesID > 10).GRFirstOrDefaultAsync().GetAwaiter().GetResult();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get first entitity ID > 10 - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(testEntity.TestEntityAutoPropertiesID > 10, "Entity with ID > 10 should be returned instead of entitity with ID = {0}", testEntity.TestEntityAutoPropertiesID);
        }

        [TestMethod]
        public void Get_Entities_Name_Include()
        {
            IGRRepository repo = TestUtils.GetNontGenericRepository(dbName);
            TestEntityAutoProperties testEntity = null;

            try
            {
                testEntity = repo
                    .GRWhere<TestEntityAutoProperties>(e => e.TestEntityAutoPropertiesID == 10)
                    .GRInclude(e => e.Name)
                    .GRFirstOrDefaultAsync()
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entity with included property - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(testEntity.Name != string.Empty, "Entity name should not be loaded");
            Assert.IsTrue(testEntity.TestEntityAutoPropertiesID == 0, "Entity ID should not be loaded");
            Assert.IsTrue(testEntity.TestEntityAutoPropertiesOrder == 0, "Entity order should not be loaded");

            Assert.IsTrue(testEntity.CreatedDate == new DateTime(), "Entity CreatedDate should not be loaded");
            Assert.IsTrue(testEntity.ModifiedDate == new DateTime(), "Entity ModifiedDate should not be loaded");
            Assert.IsTrue(testEntity.CreatedBy == 0, "Entity CreatedBy should not be loaded");
            Assert.IsTrue(testEntity.ModifiedBy == 0, "Entity ModifiedBy should not be loaded");
            Assert.IsTrue(!testEntity.IsActive, "Entity IsActive should not be loaded");
        }

        [TestMethod]
        public void Get_Entities_Name_Excluded()
        {
            IGRRepository repo = TestUtils.GetNontGenericRepository(dbName);
            TestEntityAutoProperties testEntity = null;

            try
            {
                testEntity = repo
                    .GRWhere<TestEntityAutoProperties>(e => e.TestEntityAutoPropertiesID == 10)
                    .GRExclude(e => e.Name)
                    .GRFirstOrDefaultAsync()
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entity with excluded property - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(string.IsNullOrEmpty(testEntity.Name), "Entity name should not be loaded");
            Assert.IsTrue(testEntity.TestEntityAutoPropertiesID != 0, "Entity ID should be loaded");
            Assert.IsTrue(testEntity.TestEntityAutoPropertiesOrder != 0, "Entity order should be loaded");
            Assert.IsTrue(string.IsNullOrEmpty(testEntity.TestEntityAutoPropertiesDescription), "Entity description (GR ignored) should not be loaded");

            Assert.IsTrue(testEntity.CreatedDate == TestUtils.DefaultCreatedDate, "Entity CreatedDate should be loaded");
            Assert.IsTrue(testEntity.ModifiedDate == TestUtils.DefaultCreatedDate, "Entity ModifiedDate should be loaded");
            Assert.IsTrue(testEntity.CreatedBy == -1, "Entity CreatedBy should be loaded");
            Assert.IsTrue(testEntity.ModifiedBy == -1, "Entity ModifiedBy should be loaded");
            Assert.IsTrue(testEntity.IsActive, "Entity IsActive should be loaded");
        }

        [TestMethod]
        public void Get_Entities_Binary_Stream()
        {
            IGRRepository repo = TestUtils.GetNontGenericRepository(dbName);
            TestEntityBinaryStream entity = null;

            try
            {
                entity = repo.GRGet<TestEntityBinaryStream>(1);
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entity with binary data - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(entity.TestEntityBinaryData != null, "Entity data stream was not loaded.");

            byte[] data = ConvertToByteArray(entity.TestEntityBinaryData);

            string value = Encoding.UTF8.GetString(data);

            Assert.IsTrue(value == TestUtils.TestSPVarbinaryData, "Binary data was corrupted.");
        }

        [TestMethod]
        public void Get_Entities_Binary_Array_Stream()
        {
            IGRRepository repo = TestUtils.GetNontGenericRepository(dbName);
            TestEntityBinaryArray entity = null;

            try
            {
                entity = repo.GRGet<TestEntityBinaryArray>(1);
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entity with binary data - {0}.", GRStringHelpers.GetExceptionString(exc));
            }

            Assert.IsTrue(entity.TestEntityBinaryData != null, "Entity data stream was not loaded.");

            byte[] data = entity.TestEntityBinaryData;

            string value = Encoding.UTF8.GetString(data);

            Assert.IsTrue(value == TestUtils.TestSPVarbinaryData, "Binary data was corrupted.");
        }

        [TestMethod]
        public async Task Get_Entities_Binary_Stream_Null_Condition()
        {
            IGRRepository repo = TestUtils.GetNontGenericRepository(dbName);
            TestEntityBinaryStream entity = null;

            try
            {
                entity = await repo.GRWhere<TestEntityBinaryStream>(e => e.TestEntityBinaryData == null).GRFirstOrDefaultAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entity with binary data - {0}.", GRStringHelpers.GetExceptionString(exc));
            }
        }

        [TestMethod]
        public async Task Get_Entities_Binary_Array_Null_Condition()
        {
            IGRRepository repo = TestUtils.GetNontGenericRepository(dbName);
            TestEntityBinaryArray entity = null;

            try
            {
                entity = await repo.GRWhere<TestEntityBinaryArray>(e => e.TestEntityBinaryData == null).GRFirstOrDefaultAsync();
            }
            catch (Exception exc)
            {
                Assert.Fail("Unable to get entity with binary data - {0}.", GRStringHelpers.GetExceptionString(exc));
            }
        }

        public static byte[] ConvertToByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }
    }
}
