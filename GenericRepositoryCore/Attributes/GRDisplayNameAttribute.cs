using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Attributes
{
    public class GRDisplayNameAttribute : Attribute
    {
        public GRDisplayNameAttribute(string displayName)
        {
            this.DisplayName = displayName;
        }

        public GRDisplayNameAttribute()
        {

        }
        public string DisplayName { get; set; }
    }
}
