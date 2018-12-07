using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Models
{
    public struct GRPropertyCollectionKey
    {
        public string Prefix { get; private set; }
        public Type Type { get; private set; }

        public GRPropertyCollectionKey(string prefix, Type type)
        {
            this.Type = type;
            this.Prefix = prefix;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(Prefix))
            {
                return Type.Name;
            }
            else
            {
                return string.Format("{0}: {1}", Prefix, Type.Name);
            }
        }
    }
}
