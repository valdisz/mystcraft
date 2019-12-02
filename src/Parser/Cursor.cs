namespace atlantis {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class Cursor<T> : IAsyncDisposable {
        public Cursor(int historySize, IAsyncEnumerable<T> source) {
            this.historySize = historySize;
            this.source = source.GetAsyncEnumerator();
        }

        private readonly int historySize;
        private readonly IAsyncEnumerator<T> source;
        private readonly LinkedList<T> buffer = new LinkedList<T>();
        private LinkedListNode<T> head;

        public bool HasValue { get; private set; } = false;

        public T Value {
            get {
                if (!HasValue) {
                    throw new InvalidOperationException();
                }
                return head.Value;
            }
        }

        public async Task<bool> NextAsync() {
            if (head != null && head.Next != null) {
                HasValue = true;
                head = head.Next;

                return HasValue;
            }

            HasValue = await source.MoveNextAsync();
            if (HasValue) {
                buffer.AddLast(source.Current);
                head = buffer.Last;

                if (buffer.Count > historySize) {
                    buffer.RemoveFirst();
                }
            }

            return HasValue;
        }

        public bool Back() {
            if (head == null || head.Previous == null) {
                return false;
            }

            head = head.Previous;

            return true;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual async ValueTask DisposeAsync(bool disposing) {
            if (!disposedValue) {
                await source.DisposeAsync();

                disposedValue = true;
            }
        }

        public ValueTask DisposeAsync()
        {
            return DisposeAsync(true);
        }
        #endregion
    }
}
