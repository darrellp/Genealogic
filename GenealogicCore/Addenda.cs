using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenealogicCore
{
    internal class Addenda
    {
        internal List<IAddendum>? _addenda;

        internal Addenda(List<IAddendum>? addenda = null)
        {
            _addenda = addenda??new();
        }
    }
}
