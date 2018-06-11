using GenericRepository.Enums;
using System;
using System.Linq.Expressions;

namespace GenericRepository.Models
{
    public abstract class GRCommandParam<T>
    {
    }

    public class GRCommandParamWhere<T> : GRCommandParam<T>
    {
        public Expression<Func<T, bool>> Expression { get; private set; }

        public GRCommandParamWhere(Expression<Func<T, bool>> exp)
        {
            this.Expression = exp;
        }
    }

    public class GRCommandParamLimit<T> : GRCommandParam<T>
    {
        public int Count { get; private set; }

        public GRCommandParamLimit(int count)
        {
            this.Count = count;
        }
    }

    public class GRCommandParamDistinct<T> : GRCommandParam<T>
    {
    }

    public abstract class GRCommandParamProperty<T> : GRCommandParam<T>
    {
        public GRDBProperty Property { get; protected set; }

        public GRCommandParamProperty(GRDBProperty property)
        {
            this.Property = property;
        }
    }

    public class GRCommandParamOrder<T> : GRCommandParamProperty<T>
    {
        public GRQueryOrderDirection Direction { get; set; }
        public GRCommandParamOrder(GRDBProperty property, GRQueryOrderDirection direction) : base(property)
        {
            Direction = direction;
        }
    }

    public class GRCommandParamInclude<T> : GRCommandParamProperty<T>
    {
        public GRCommandParamInclude(GRDBProperty property) : base(property)
        {

        }
    }

    public class GRCommandParamExcludeAll<T> : GRCommandParam<T>
    {

    }

    public class GRCommandParamExclude<T> : GRCommandParamProperty<T>
    {
        public GRCommandParamExclude(GRDBProperty property) : base(property)
        {

        }
    }

    public class GRCommandParamForceExclude<T> : GRCommandParamProperty<T>
    {
        public GRCommandParamForceExclude(GRDBProperty property) : base(property)
        {

        }
    }

    public class GRCommandParamNoLock<T> : GRCommandParam<T>
    {

    }
}
