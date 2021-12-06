using System;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Configuration;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace MyLog
{
	public class TraceSettings
	{
#if GONE
		public List<TraceTagEnum> TagEnums
		{
			get
			{
				return ((List<TraceTagEnum>)this["TagEnums"]);
			}
			set
			{
				this["TagEnums"] = (List<TraceTagEnum>)value;
			}
		}

		public List<string> lststrListeners
		{
			get
			{
				return ((List<string>)this["lststrListeners"]);
			}
			set
			{
				this["lststrListeners"] = (List<string>)value;
			}
		}
#endif
	}
}
