using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Test.Models
{
    [GRTableName("TestEntityMultiID")]
    public class TestEntityMultiID_1
    {
        [GRIgnore]
        [GRIDProperty(Apply = GRAutoValueApply.AfterSelect | GRAutoValueApply.AfterInsert, Direction = GRAutoValueDirection.In)]
        public int? ID { get; set; }

        [GRPrimaryKey]
        public int TestEntityMulti1ID { get; set; }
        public int TestEntityMulti2ID { get; set; }
    }

    [GRTableName("TestEntityMultiID")]
    public class TestEntityMultiID_2
    {
        [GRIgnore]
        [GRIDProperty(Apply = GRAutoValueApply.AfterSelect | GRAutoValueApply.AfterInsert, Direction = GRAutoValueDirection.In)]
        public int? ID { get; set; }

        [GRPrimaryKey]
        public int TestEntityMulti1ID { get; set; }

        [GRPrimaryKey]
        public int TestEntityMulti2ID { get; set; }
    }

    [GRTableName("TestEntityMultiID")]
    public class TestEntityMultiID_3
    {
        [GRIgnore]
        [GRIDProperty(Apply = GRAutoValueApply.AfterSelect | GRAutoValueApply.AfterInsert, Direction = GRAutoValueDirection.In)]
        public int? ID { get; set; }

        public int TestEntityMulti1ID { get; set; }
        public int TestEntityMulti2ID { get; set; }
    }
}
