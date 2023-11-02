namespace System.Collections.Generic
{
    public class BidirectionalDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _forwardDictionary;
        private readonly Dictionary<TValue, TKey> _reverseDictionary;

        public BidirectionalDictionary(int capacity = 8)
        {
            _forwardDictionary = new Dictionary<TKey, TValue>(capacity);
            _reverseDictionary = new Dictionary<TValue, TKey>(capacity);
        }

        public bool TryGetKey(TValue value, out TKey key)
        {
            return _reverseDictionary.TryGetValue(value, out key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _forwardDictionary.TryGetValue(key, out value);
        }

        public void Clear()
        {
            _forwardDictionary.Clear();
            _reverseDictionary.Clear();
        }

        public bool TryAdd(TKey key, TValue value)
        {
            if (!_forwardDictionary.ContainsKey(key))
            {
                _forwardDictionary.Add(key, value);
                _reverseDictionary.Add(value, key);
                return true;
            }

            return false;
        }

        public bool TryRemoveByKey(TKey key)
        {
            if (_forwardDictionary.TryGetValue(key, out var value))
            {
                _forwardDictionary.Remove(key);
                _reverseDictionary.Remove(value);
                return true;
            }

            return false;
        }

        public bool TryRemoveByValue(TValue value)
        {
            if (_reverseDictionary.TryGetValue(value, out var key))
            {
                _reverseDictionary.Remove(value);
                _forwardDictionary.Remove(key);
                return true;
            }

            return false;
        }
    }
}