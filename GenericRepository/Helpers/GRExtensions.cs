using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Helpers
{
    public static class GRExtensions
    {
        public static bool ColumnExists(this IDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static HashSet<string> GetColumnNames(this IDataReader reader)
        {
            HashSet<string> ret = new HashSet<string>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string colName = reader.GetName(i);

                ret.Add(colName);
            }

            return ret;
        }
    }
}
