using System;
using System.Collections.Generic;
using System.Text;

namespace Percept.ObjectExtensions
{
    //delegate keeping used count to the user for performance.
    public class FixedArray<T>
    {
        public int Used { get; set; }
        public T[] Array { get; private set; }

        public FixedArray(int size)
        {
            Array = new T[size];
            Used = 0;
        }
    }
}
