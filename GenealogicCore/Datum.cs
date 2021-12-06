using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenealogicCore
{
    // Data with a reference
    internal class Datum<T>
    {
        Addenda References { get; init; }
        T Value { get; set; }

        internal Datum(T value, Addenda references)
        {
            Value = value;
            References = references;
        }
    }
}