namespace System.Collections.Generic
{
    public class BidirectionalDictionary<TKey, TValue>
    {
        private Dictionary<TKey, TValue> forwardDictionary;
        private Dictionary<TValue, TKey> reverseDictionary;

        public BidirectionalDictionary()
        {
            forwardDictionary = new Dictionary<TKey, TValue>();
            reverseDictionary = new Dictionary<TValue, TKey>();
        }

        public bool TryGetKey(TValue value, out TKey key)
        {
            return reverseDictionary.TryGetValue(value, out key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return forwardDictionary.TryGetValue(key, out value);
        }

        public void Clear()
        {
            forwardDictionary.Clear();
            reverseDictionary.Clear();
        }

        public bool TryAdd(TKey key, TValue value)
        {
            if (!forwardDictionary.ContainsKey(key))
            {
                forwardDictionary.Add(key, value);
                reverseDictionary.Add(value, key);
                return true;
            }

            return false;
        }
    }
}