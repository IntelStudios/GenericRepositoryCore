using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Models
{
    public class GRPropertyCollectionItem
    {
        public string Prefix { get; private set; }
        public Dictionary<GRDBProperty, GRDBQueryProperty> Properties { get; private set; }
        public Type Type { get; private set; }
        public bool ApplyAutoProperties { get; set; } = true;
        public GRPropertyCollectionItem(string prefix, Type type)
        {
            Prefix = prefix;
            Type = type;
            Properties = new Dictionary<GRDBProperty, GRDBQueryProperty>();
        }
        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Prefix))
            {
                return $"{Prefix}: {Type.Name}, {Properties.Count} properties";
            }
            else
            {
                return $"{Type.Name}, {Properties.Count} properties";
            }
        }
    }
}
