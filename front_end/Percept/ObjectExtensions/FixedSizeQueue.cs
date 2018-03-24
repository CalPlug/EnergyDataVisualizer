using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace Percept.ObjectExtensions
{
    public class FixedSizeQueue<T> : IEnumerable<T>
    {

        private Queue<T> q;

        private int _limit;

        public int Limit {
            get => _limit;
            set
            {
                if(value <= 0)
                {
                    throw new ArgumentOutOfRangeException("Invalid Limit");
                }
                _limit = value;
            }
        }

        public int Length
        {
            get => q.Count;
        }

        public bool IsFull()
        {
            return _limit == q.Count;
        }

        public FixedSizeQueue(int limit = 10)
        {
            Limit = limit;
            q = new Queue<T>(limit);
        }

        public void Enqueue(T element)
        {
            q.Enqueue(element);
            if(q.Count > Limit)
            {
                q.Dequeue();
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return q.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this;
        }
    }
}