using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Attributes
{
    [Flags]
    public enum GRAutoValueApply
    {
        // insert
        BeforeInsert = 1,
        AfterInsert = 2,

        // update
        BeforeUpdate = 4,
        AfterUpdate = 8,

        // select
        BeforeSelect = 16,
        AfterSelect = 32
    };
    public enum GRAutoValueDirection
    {
        In,
        Out
    };

    [AttributeUsage(AttributeTargets.Property)]
    public abstract class GRAutoValueAttribute : Attribute
    {
        public GRAutoValueApply Apply { get; set; }
    }

    public class GRCurrentDatetimeAttribute : GRAutoValueAttribute
    {
    }

    public class GRNewGuidAttribute : GRAutoValueAttribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class GRPropertyAttribute : GRAutoValueAttribute
    {
        public GRAutoValueDirection Direction { get; set; }
        public string PropertyName { get; set; }
        public GRPropertyAttribute(string propertyName)
        {
            this.PropertyName = propertyName;
        }
        public GRPropertyAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class GRRepositoryPropertyAttribute : GRAutoValueAttribute
    {
        public GRAutoValueDirection Direction { get; set; }
        public string PropertyName { get; set; }
        public GRRepositoryPropertyAttribute(string propertyName, GRAutoValueDirection direction)
        {
            this.PropertyName = propertyName;
            this.Direction = direction;
        }
        public GRRepositoryPropertyAttribute()
        {

        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class GRIDPropertyAttribute : GRAutoValueAttribute
    {
        public GRAutoValueDirection Direction { get; set; }
        public GRIDPropertyAttribute()
        {
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class GRAttributeAttribute : GRAutoValueAttribute
    {
        public Type Attribute { get; set; }
        public string PropertyName { get; set; }

        public GRAttributeAttribute(Type attr, string propName)
        {
            this.Attribute = attr;
            this.PropertyName = propName;
        }
        public GRAttributeAttribute()
        {
        }
    }
}
