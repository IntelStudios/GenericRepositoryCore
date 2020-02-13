using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Models
{
    public class GRTable : IEnumerable<GRTableRow>
    {
        public List<GRTableRow> Rows { get; set; }

        public List<T> Get<T>()
        {
            return Get<T>(null);
        }

        public List<T> Get<T>(string prefix)
        {
            return Rows.Select(i => i.Get<T>(prefix)).ToList();
        }

        public T First<T>()
        {
            if (!HasAny)
                return default(T);

            return Rows[0].Get<T>();
        }

        public int Count
        {
            get
            {
                return Rows == null ? 0 : Rows.Count;
            }
        }

        public bool HasAny
        {
            get
            {
                return Rows != null && Rows.Any();
            }
        }

        public IEnumerator<GRTableRow> GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Rows.GetEnumerator();
        }

        public GRTableRow this[int index]
        {
            get
            {
                return Rows[index];
            }
        }

        public override string ToString()
        {
            if (Rows == null)
            {
                return "Not initialized";
            }
            if (Rows.Count > 1)
            {
                return string.Format("{0} items", Rows.Count);
            } else
            {
                return "1 item";
            }
        }
    }
}
