using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericRepository.Models
{
    public class GRJoinedList : IEnumerable<GRJoinedListItem>
    {
        public List<GRJoinedListItem> Items { get; set; }

        public List<T> Get<T>()
        {
            return Items.Select(i => i.Get<T>()).ToList();
        }

        public T First<T>()
        {
            if (!HasAny)
                return default(T);

            return Items[0].Get<T>();
        }

        public int Count
        {
            get
            {
                return Items == null ? 0 : Items.Count;
            }
        }

        public bool HasAny {
            get
            {
                return Count > 0;
            }
        }

        public IEnumerator<GRJoinedListItem> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public GRJoinedListItem this[int index]
        {
            get
            {
                return Items[index];
            }
        }

        public override string ToString()
        {

            if (Items == null) return "Not initialized";
            return string.Format("{0} items", Items.Count);
        }
    }

    public class GRJoinedListItem
    {
        public GRJoinedListItem()
        {
            Objects = new List<object>();
        }

        public List<object> Objects { get; private set; }

        public T Get<T>()
        {
            return Objects.Where(o => o is T).Select(o => (T)o).FirstOrDefault();
        }

        public override string ToString()
        {

            if (Objects == null) return "Not initialized";
            return string.Format("{0} objects", Objects.Count);
        }
    }
}
