using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class GRInsertOnlyAttribute : Attribute
    {

    }
}
