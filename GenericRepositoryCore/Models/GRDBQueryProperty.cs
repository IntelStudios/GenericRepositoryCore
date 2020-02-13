using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Models
{
    public class GRDBQueryProperty
    {
        public GRDBProperty Property { get; private set; }
        public bool IsTemporary { get; private set; } = false;

        public GRDBQueryProperty(GRDBProperty property)
        {
            this.Property = property;
        }

        public GRDBQueryProperty(GRDBProperty property, bool isTemporary)
        {
            this.Property = property;
            this.IsTemporary = isTemporary;
        }

        public override bool Equals(object obj)
        {
            return Property.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Property.GetHashCode();
        }

        public override string ToString()
        {
            if (IsTemporary) {
                return $"{Property} (temp)";
            }

            return Property.ToString();
        }
    }
}
