using GenericRepository.Interfaces;
using GenericRepository.Repositories;
using GenericRepository.Test.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Test.Repositories
{
    public class QueueEmptyItemRepository : GRRepository<QueueEmptyItem>
    {
        public QueueEmptyItemRepository(IGRContext context) : base(context)
        {
        }

        public IGRContext PublicContext
        {
            get
            {
                return context;
            }
        }
    }

    public class QueueEmptyItemRepository2 : GRRepository<QueueEmptyItem2>
    {
        public QueueEmptyItemRepository2(IGRContext context) : base(context)
        {
        }

        public IGRContext PublicContext
        {
            get
            {
                return context;
            }
        }
    }

    public class QueueEmptyItemRepository3 : GRRepository<QueueEmptyItem3>
    {
        public QueueEmptyItemRepository3(IGRContext context) : base(context)
        {
        }

        public IGRContext PublicContext
        {
            get
            {
                return context;
            }
        }
    }
}
