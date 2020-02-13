using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Helpers
{
    public class GRSQLReader 
    {
        public GRSQLReader(SqlDataReader reader)
        {
            Reader = reader;
        }

        public SqlDataReader Reader { get; }

        public bool Read()
        {
            bool ret = Reader.Read();
            return ret;
        }
    }
}
