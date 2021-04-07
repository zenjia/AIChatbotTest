using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIChatbotTest.Utils
{
    /// <summary>
    /// Represents a pool of objects with a size limit.
    /// </summary>
    /// <typeparam name="T">The type of object in the pool.</typeparam>
    public sealed class ObjectPool<T> : IDisposable
        where T : new()
    {
        private readonly int size;
        private readonly object locker;
        private readonly Queue<T> queue;
        private int count;


        /// <summary>
        /// Initializes a new instance of the ObjectPool class.
        /// </summary>
        /// <param name="size">The size of the object pool.</param>
        public ObjectPool(int size)
        {
            if (size <= 0)
            {
                const string message = "The size of the pool must be greater than zero.";
                throw new ArgumentOutOfRangeException("size", size, message);
            }

            this.size = size;
            this.locker = new object();
            this.queue = new Queue<T>();
        }


        /// <summary>
        /// Retrieves an item from the pool. 
        /// </summary>
        /// <returns>The item retrieved from the pool.</returns>
        public T Get()
        {
            lock (this.locker)
            {
                if (this.queue.Count > 0)
                {
                    return this.queue.Dequeue();
                }

                this.count++;
                return new T();
            }
        }

        /// <summary>
        /// Places an item in the pool.
        /// </summary>
        /// <param name="item">The item to place to the pool.</param>
        public void Put(T item)
        {
            lock (this.locker)
            {
                if (this.count < this.size)
                {
                    this.queue.Enqueue(item);
                }
                else
                {
                    using (item as IDisposable)
                    {
                        this.count--;
                    }
                }
            }
        }

        /// <summary>
        /// Disposes of items in the pool that implement IDisposable.
        /// </summary>
        public void Dispose()
        {
            lock (this.locker)
            {
                this.count = 0;
                while (this.queue.Count > 0)
                {
                    using (this.queue.Dequeue() as IDisposable)
                    {

                    }
                }
            }
        }
    }
}
