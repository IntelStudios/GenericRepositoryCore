using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Models
{
    public class GRTableItem
    {
        public object Entity { get; set; }
        public string Prefix { get; set; }
        public Type Type { get; set; }

        public override string ToString()
        {
            if (!string.IsNullOrEmpty(Prefix))
            {
                return $"{Prefix}: {Type}";
            } else
            {
                return $"{Type}";
            }
        }
    }
}
