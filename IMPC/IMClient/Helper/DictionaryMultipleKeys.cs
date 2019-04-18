using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IMClient.Helper
{
    public class Dictionary<TKey1, TKey2, TValue> : Dictionary<Tuple<TKey1, TKey2>, TValue>, IDictionary<Tuple<TKey1, TKey2>, TValue>
    {

        public TValue this[TKey1 key1, TKey2 key2]
        {
            get { return base[Tuple.Create(key1, key2)]; }
            set { base[Tuple.Create(key1, key2)] = value; }
        }

        public void Add(TKey1 key1, TKey2 key2, TValue value)
        {
            try
            {
                if (!ContainsKey(key1, key2))
                    base.Add(Tuple.Create(key1, key2), value);
            }
            catch (Exception ex)
            {

            }
        }

        public bool ContainsKey(TKey1 key1, TKey2 key2)
        {
            return base.ContainsKey(Tuple.Create(key1, key2));
        }
    }
}
