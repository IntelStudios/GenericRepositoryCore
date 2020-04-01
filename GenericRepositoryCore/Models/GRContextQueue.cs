using GenericRepository.Exceptions;
using GenericRepository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Models
{
    public class GRContextQueue
    {
        protected readonly System.Threading.SemaphoreSlim semaphore = new System.Threading.SemaphoreSlim(1);

        // queue of entyties to insert/update/delete
        List<GRContextQueueItem> queue = new List<GRContextQueueItem>();

        internal void Add(GRContextQueueItem item)
        {
            semaphore.Wait();

            try
            {
                queue.Add(item);
            }
            finally
            {
                semaphore.Release();
            }
        }

        internal GRContextQueueItem Dequeue(IGRUpdatable updatable)
        {
            semaphore.Wait();

            try
            {
                GRContextQueueItem item = null;

                const string commonError = "Avoid reusing the same entity for multiple operations.";

                try
                {
                    item = queue.Where(i => i.Item == updatable).SingleOrDefault();

                    if (item == null)
                    {
                        throw new GRQueryExecutionFailedException("Entity is not presented in a context queue! " + commonError);
                    }
                }
                catch (Exception exc)
                {
                    throw new GRQueryExecutionFailedException(exc, "Entity was presented in a context queue more than once! " + commonError);
                }

                queue.Remove(item);
                return item;
            }
            finally
            {
                semaphore.Release();
            }
        }

        internal GRContextQueueItem Dequeue()
        {
            semaphore.Wait();

            try
            {
                if (!queue.Any())
                {
                    return null;
                }

                GRContextQueueItem ret = queue[0];
                queue.RemoveAt(0);
                return ret;
            }
            finally
            {
                semaphore.Release();
            }
        }
    }
}
