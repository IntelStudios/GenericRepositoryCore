using GenericRepository.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Test.Models
{
    public class TestEntityPK
    {
        [GRPrimaryKey]
        public int TestEntityPKID { get; set; }
        public string TestEntityPKName { get; set; }
    }

    public class TestEntityAIPK
    {
        [GRAIPrimaryKey]
        public int TestEntityAIPKID { get; set; }
        public string TestEntityAIPKName { get; set; }
    }
}
