using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Test.Models
{
    [GRTableName(TableName = "Queue_Empty")]
    public class QueueEmptyItem
    {
        [GRAIPrimaryKey]
        public int ID { get; set; }
        public string Name { get; set; }
    }

    [GRTableName(TableName = "Queue_Empty_2")]
    public class QueueEmptyItem2
    {
        [GRAIPrimaryKey]
        public int ID { get; set; }
        public string Name { get; set; }
    }
}
