using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTrace
{
    public class TraceTagEnum
    {
        public string StrName;
        public TraceTag[] ArttTags;
    }
    public class TraceSettings
    {
        public List<TraceTagEnum> TagEnums { get; } = new();
    }

}
