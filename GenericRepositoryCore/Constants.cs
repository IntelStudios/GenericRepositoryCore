using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository
{
    public class Constants
    {
        public static class Exceptions
        {
            public const string NoColumnsSelected = "There is nothing to select - query statement hasn't any specified resulting columns. If you want to get count only, use GRCount() or GRCountAsync() methods.";
        }
    }
}
