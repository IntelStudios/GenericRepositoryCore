using System.IO;
using System.Reflection;

namespace GenericRepository.Models
{
    public class GRDBProperty
    {
        public string DBColumnName { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsPrimaryKeyAutoIncremented { get; set; }
        public bool IsBinary
        {
            get
            {
                return PropertyInfo.PropertyType == typeof(byte[]) || PropertyInfo.PropertyType == typeof(Stream);
            }
        }
        public PropertyInfo PropertyInfo { get; set; }

        public override string ToString()
        {
            return string.Format("Property: {0}, DB Column: {1}", PropertyInfo.Name, DBColumnName);
        }
    }
}
