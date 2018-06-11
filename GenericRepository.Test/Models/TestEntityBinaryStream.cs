using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Test.Models
{
    [GRTableName(TableName = "TestEntityBinaryTable")]
    public class TestEntityBinaryStream
    {
        [GRAIPrimaryKey]
        public int TestEntityBinaryID { get; set; }
        public Stream TestEntityBinaryData { get; set; }
    }
}
