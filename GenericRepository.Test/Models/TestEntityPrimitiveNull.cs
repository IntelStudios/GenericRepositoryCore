using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Test.Models
{
    [GRTableName(TableName = "TestEntityPrimitiveNullTable")]
    public class TestEntityPrimitiveNull
    {
        [GRAIPrimaryKey]
        public int TestEntityPrimitiveNullID { get; set; }
        public int? TestEntityPrimitiveNullInt { get; set; }
        public bool? TestEntityPrimitiveNullBool { get; set; }
        public DateTime? TestEntityPrimitiveNullDate { get; set; }
        public string TestEntityPrimitiveNullString { get; set; }
    }
}
