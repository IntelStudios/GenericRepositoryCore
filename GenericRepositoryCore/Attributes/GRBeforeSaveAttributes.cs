using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class GRBeforeUpdateAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GRBeforeInsertAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GRBeforeSaveAttribute : Attribute
    {
    }
}
