using GenericRepository.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Exceptions
{
    public class GRPreSaveException : ApplicationException
    {
        public MethodInfo Method { get; set; }
        public GRPreSaveActionType Action { get; set; }
        public Type Type { get; set; }

        public GRPreSaveException(Exception innerException, MethodInfo method, GRPreSaveActionType action, Type type) 
            : base(
                string.Format("Error occured while invoking method '{0}' before {1} {2}.",
                    method.Name,
                    action == GRPreSaveActionType.Insert ? "inserting" : "updating",
                    type.Name),
                innerException)
        {

        }
    }
}
