using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Models
{
    public class GRTableRow
    {
        public GRTableRow()
        {
            Items = new List<GRTableItem>();
        }

        public List<GRTableItem> Items { get; private set; }

        public T Get<T>(string prefix)
        {
            return Items
                .Where(o => o.Type == typeof(T) && o.Prefix == prefix)
                .Select(o => (T)o.Entity)
                .SingleOrDefault();
        }

        public T Get<T>()
        {
            return Get<T>(null);
        }

        public override string ToString()
        {
            if (Items == null)
            {
                return "Not initialized";
            }

            if (Items.Count > 0)
            {
                return string.Format("{0} items", Items.Count);
            }
            else
            {
                return "1 item";
            }
        }
    }
}
