using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Test.Models
{
    public class TestEntityJoining
    {
        [GRAIPrimaryKey]
        public int TestEntityJoiningID { get; set; }
        public int TestEntityAutoPropertiesID { get; set; }
        [GRColumnName(ColumnName = "TestEntityJoiningDescription")]
        public string Description { get; set; }
    }

    public class TestEntityJoiningType1
    {
        public int TestEntityJoiningType1ID { get; set; }
        public int TestEntityJoiningType2ID { get; set; }
        public string TestEntityJoiningType1Name { get; set; }
    }
    public class TestEntityJoiningType2
    {
        public int TestEntityJoiningType2ID { get; set; }
        public int TestEntityJoiningType1ID { get; set; }
        public string TestEntityJoiningType2Name { get; set; }
    }
}
