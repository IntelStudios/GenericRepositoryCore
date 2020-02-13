using GenericRepository.Attributes;
using GenericRepository.Helpers;
using System.IO;
using System.Reflection;

namespace GenericRepository.Models
{
    public class GRDBProperty
    {
        public string DBColumnName { get; private set; }
        public bool IsPrimaryKey { get; private set; }
        public bool IsPrimaryKeyAutoIncremented { get; private set; }
        public bool IsAutoProperty { get; private set; }
        public bool IsAutoInsertProperty { get; private set; }
        public bool IsAutoUpdateProperty { get; private set; }

        public bool IsBinary
        {
            get
            {
                return PropertyInfo.PropertyType == typeof(byte[]) || PropertyInfo.PropertyType == typeof(Stream);
            }
        }

        public PropertyInfo PropertyInfo { get; private set; }

        public override string ToString()
        {
            return string.Format("Property: {0}, DB Column: {1}", PropertyInfo.Name, DBColumnName);
        }

        public GRDBProperty(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;

            IsAutoUpdateProperty = GRDataTypeHelper.IsAutoUpdateProperty(propertyInfo);
            IsAutoInsertProperty = GRDataTypeHelper.IsAutoInsertProperty(propertyInfo);
            IsAutoProperty = GRDataTypeHelper.IsAutoProperty(propertyInfo);

            DBColumnName = GRDataTypeHelper.GetDBColumnName(propertyInfo);
            IsPrimaryKey = GRDataTypeHelper.HasAttribute(propertyInfo, typeof(GRPrimaryKeyAttribute));
            IsPrimaryKeyAutoIncremented = GRDataTypeHelper.HasAttribute(propertyInfo, typeof(GRAIPrimaryKey));
        }
    }
}
