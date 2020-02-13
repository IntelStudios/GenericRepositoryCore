using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Test.Models
{
    [GRTableName(TableName = "TestEntityBinaryTable")]
    public class TestEntityBinaryArray
    {
        [GRAIPrimaryKey]
        public int TestEntityBinaryID { get; set; }
        public byte[] TestEntityBinaryData { get; set; }
    }
}
