using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Enums
{
    [Flags]
    public enum GRContextLogLevel
    {
        Debug = 1,
        Error = 2,
        Warning = 4
    }
}
