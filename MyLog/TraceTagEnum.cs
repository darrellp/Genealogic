using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetTrace;

namespace MyLog
{
	public class TraceTagEnum
	{
		public string strName;
		public bool fShowTimeStamps;
		public bool fShowThreadIds;
		public bool fShowTagName;
		public bool fShowSeverity;
		public string strListeners;
		public TraceTag[] arttTags;
	}

}
