using GenericRepository.Attributes;
using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using GenericRepository.Test.Models;
using GenericRepository.Test.Repositories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Test.Tests.CRUD
{
    [TestClass]
    public class Queue
    {
        static string dbBaseName = "xeelo-tests-gr-queue";
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
        public async Task Queue_Insert_Update()
        {
            QueueEmptyItemRepository repo = TestUtils.GetQueueEmptyItemRepository(dbName);

            for (int i = 0; i < 100; i++)
            {
                QueueEmptyItem item = new QueueEmptyItem
                {
                    Name = $"Name {i + 1}"
                };
                repo.GREnqueueInsert(item);
            }

            await repo.PublicContext.SaveChangesAsync();

            List<QueueEmptyItem> all = await repo.GRAll().GRToListAsync();

            Assert.IsTrue(all.Count == 100, "Incorrect number of inserted entities.");

            foreach (var item in all)
            {
                Assert.IsTrue(item.Name == $"Name {item.ID }", "Incorrect number of inserted entities.");

                item.Name += item.Name;
                repo.GREnqueueUpdate(item);
            }

            await repo.PublicContext.SaveChangesAsync();

            List<QueueEmptyItem> all2 = await repo.GRAll().GRToListAsync();

            Assert.IsTrue(all2.Count == 100, "Incorrect number of loaded entities.");

            for (int i = 1; i <= 100; i++)
            {
                var item1 = all.Single(item => item.ID == i);
                var item2 = all2.Single(item => item.ID == i);

                Assert.IsTrue(item1.Name == item2.Name, "Incorrectly updated entity.");
            }
        }

        [TestMethod]
        public async Task Queue_Insert_OneDirect()
        {
            QueueEmptyItemRepository2 repo = TestUtils.GetQueueEmptyItemRepository2(dbName);

            QueueEmptyItem2 directUpdate = null;
            IGRUpdatable<QueueEmptyItem2> directUpdatable = null;

            for (int i = 0; i < 100; i++)
            {
                QueueEmptyItem2 item = new QueueEmptyItem2
                {
                    Name = $"Name {i + 1}"
                };
                IGRUpdatable<QueueEmptyItem2> updatable = repo.GREnqueueInsert(item);

                if (i == 49)
                {
                    directUpdate = item;
                    directUpdatable = updatable;
                }
            }

            directUpdate.Name = "Not this";
            await directUpdatable.GRExecuteAsync();
            directUpdate.Name = $"Name {directUpdate}";

            await repo.PublicContext.SaveChangesAsync();

            List<QueueEmptyItem2> all = await repo.GRAll().GRToListAsync();

            Assert.IsTrue(all.Count == 100, "Incorrect number of inserted entities.");

            for (int i = 0; i < 100; i++)
            {
                QueueEmptyItem2 item = all[i];

                if (i == 0)
                {
                    Assert.IsTrue(item.ID == 1);
                    Assert.IsTrue(item.Name == "Not this", $"Incorrect name of {item.ID }.");
                    continue;
                }

                if (i < 50)
                {
                    Assert.IsTrue(item.Name == $"Name {item.ID - 1 }", $"Incorrect name of {item.ID }.");
                    continue;
                }

                Assert.IsTrue(item.Name == $"Name {item.ID }", $"Incorrect name of {item.ID }.");
            }
        }

        [TestMethod]
        public async Task Queue_Twice_Enqueued()
        {
            QueueEmptyItemRepository3 repo = TestUtils.GetQueueEmptyItemRepository3(dbName);

            List<Task> tasks = new List<Task>();

            for (int j = 0; j < 100; j++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    for (int i = 0; i < 100; i++)
                    {
                        string originalDate = DateTime.Now.ToString();

                        QueueEmptyItem3 item1 = new QueueEmptyItem3
                        {
                            Name = originalDate
                        };

                        await repo.GREnqueueInsert(item1).GRExecuteAsync();

                        QueueEmptyItem3 dbItem1 = repo.GRGet(item1.ID);
                        Assert.IsTrue(dbItem1.Name == originalDate);

                        string modifiedDate = DateTime.Now.ToString();
                        dbItem1.Name = modifiedDate;
                        await repo.GREnqueueUpdate(dbItem1).GRExecuteAsync();

                        QueueEmptyItem3 dbItem2 = repo.GRGet(dbItem1.ID);
                        Assert.IsTrue(dbItem2.Name == modifiedDate);
                    }
                }));
            }

            await Task.WhenAll(tasks);
        }
    }
}
