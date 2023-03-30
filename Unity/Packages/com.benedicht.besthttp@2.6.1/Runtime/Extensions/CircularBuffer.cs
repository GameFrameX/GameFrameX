using System;

namespace BestHTTP.Extensions
{
    public sealed class CircularBuffer<T>
    {
        public int Capacity { get; private set; }
        public int Count { get; private set; }
        public int StartIdx { get { return this.startIdx; } }
        public int EndIdx { get { return this.endIdx; } }

        public T this[int idx]
        {
            get
            {
                int realIdx = (this.startIdx + idx) % this.Capacity;

                return this.buffer[realIdx];
            }

            set
            {
                int realIdx = (this.startIdx + idx) % this.Capacity;

                this.buffer[realIdx] = value;
            }
        }

        private T[] buffer;
        private int startIdx;
        private int endIdx;

        public CircularBuffer(int capacity)
        {
            this.Capacity = capacity;
        }

        public void Add(T element)
        {
            if (this.buffer == null)
                this.buffer = new T[this.Capacity];

            this.buffer[this.endIdx] = element;

            this.endIdx = (this.endIdx + 1) % this.Capacity;
            if (this.endIdx == this.startIdx)
                this.startIdx = (this.startIdx + 1) % this.Capacity;

            this.Count = Math.Min(this.Count + 1, this.Capacity);
        }

        public void Clear()
        {
            this.Count = this.startIdx = this.endIdx = 0;
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder("[");

            int idx = this.startIdx;
            while (idx != this.endIdx)
            {
                sb.Append(this.buffer[idx].ToString());

                idx = (idx + 1) % this.Capacity;
                if (idx != this.endIdx)
                    sb.Append("; ");
            }
            sb.Append("]");

            return sb.ToString();
        }
    }
}
