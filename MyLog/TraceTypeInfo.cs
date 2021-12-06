using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace MyLog
{
	/// <summary>
	/// Each individual [TraceTags] enum type in the assembly is mapped one to one to a TraceTypeInfo object.
	/// This object holds the status of each member of that enum (turned on or off) and the information pertaining
	/// to that enum in general.
	/// </summary>
	internal class TraceTypeInfo
	{
		#region Private variables
		/// <summary>
		/// Hashtable which maps members of this enum to their status (on/off)
		/// </summary>
		private Dictionary<string, bool> m_dctTagNameToStatus = new Dictionary<string, bool>();

		/// <summary>
		/// Maps actual tag (rather than the tag name) to it's status.  This table is redundant
		/// and is only here to speed up Trace calls.
		/// </summary>
		private Dictionary<object, bool> m_dctTagToStatus = new Dictionary<object, bool>();

		/// <summary>
		/// Mapping from tag names to their descriptions
		/// </summary>
		private Dictionary<string, string> m_DctDescs = new Dictionary<string, string>();

		/// <summary>
		/// TraceTags enum type for the tags in this TraceTypeInfo
		/// </summary>
		private Type m_tp;

		public void SetDescs()
		{
			foreach (Enum tag in Enum.GetValues(m_tp))
			{
				FieldInfo fi = m_tp.GetField(tag.ToString());
				TagDescAttribute[] arattr =
					(TagDescAttribute[])fi.GetCustomAttributes(
					typeof(TagDescAttribute), false);
				if (arattr.Length > 0)
				{
					m_DctDescs[tag.ToString()] = arattr[0].strDesc;
				}
			}
		}

		public void SetTagNameStatus(String strTag, bool fOn)
		{
			object objTag = Enum.Parse(m_tp, strTag);
			m_dctTagNameToStatus[strTag] = fOn;
			m_dctTagToStatus[objTag] = fOn;
		}

#if LATER
		/// <summary>
		/// The delegate to be called whenever this trace is triggered
		/// </summary>
		private TraceTrigger m_ttfn;
		/// <summary>
		/// In order to accommodate the Cancel button cancelling all changes we have two copies of
		/// the general parameters for the TraceTypeInfo.  The HeldInfo property returns the proper
		/// one based on m_fHolding.  Before the trace dialog, SetHeld is called which copies the
		/// "real" data from m_hiReal to the held data in m_hiHeld.  This is the data used during
		/// the dialog.  If the user hits OK in that dialog then the held data is copied back into the
		/// real data and the changes become official.  If the user hits cancel, then the real (original)
		/// data is maintained and all the changes from the dialog are effectively thrown away.
		/// </summary>
		private bool m_fHolding = false;
		private HeldInfo m_hiReal = new HeldInfo();
		private HeldInfo m_hiHeld = new HeldInfo();
#endif

		#endregion

		#region Constructors / Loaders
		/// <summary>
		/// Constructs a new TraceTypeInfo based on the TraceTags enum type passed in
		/// </summary>
		/// <param name="tp">Enum holding the trace tags for this TraceTypeInfo objects</param>
		public TraceTypeInfo(Type tp)
		{
			m_tp = tp;
		}
		#endregion

        public bool GetTagStatus(object objTag)
        {
            return m_dctTagToStatus[objTag];
        }
	}
}

#if GONE
		// The character which splits the listener string info
		internal static char[] archSplit = new char[1] { ';' };

		/// <summary>
		/// Return the individual listener names from the global listener string
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		private string[] ParseListenerString(string str)
		{
			return str.Split(archSplit);
		}

		/// <summary>
		/// Produce a listener string which contains all the listeners the enum for this
		/// TraceTypeInfo prints to.  This is the string placed in the persist data and is
		/// just a semicolon separated list of listener names.
		/// </summary>
		/// <returns>The string of listeners</returns>
		private string GetListenersString()
		{
			StringBuilder strRet = new StringBuilder();
			bool fNoSemi = true;

			foreach (TraceListener tlis in AlListeners)
			{
				Debug.Assert(tlis.Name != string.Empty);

				if (fNoSemi)
				{
					fNoSemi = false;
				}
				else
				{
					strRet.Append(";");
				}
				strRet.Append(tlis.Name);
			}
			return strRet.ToString();
		}

		/// <summary>
		/// Retrieves the TraceTagEnum object for this TraceTypeInfo from the
		/// persistence data.
		/// </summary>
		/// <returns>The TraceTagEnum or null if none found</returns>
		TraceTagEnum TraceTagEnumFromTraceSettings()
		{
			if ((s_TraceSettings.TagEnums == null))
			{
				s_TraceSettings.TagEnums = new List<TraceTagEnum>();
			}
			return s_TraceSettings.TagEnums.Find(
				delegate (TraceTagEnum tteTest) { return tteTest.strName == StrCanonicalTypeName(m_tp); });
		}

		/// <summary>
		/// Save this TraceTypeInfo to persistent data.
		/// </summary>
		internal void SaveToPersistData()
		{
			TraceTagEnum tte = TraceTagEnumFromTraceSettings();
			if (tte == null)
			{
				tte = new TraceTagEnum();
				tte.strName = StrCanonicalTypeName(m_tp);
				s_TraceSettings.TagEnums.Add(tte);
			}
			tte.fShowTimeStamps = FShowTimeStamps;
			tte.fShowSeverity = FShowSeverity;
			tte.fShowThreadIds = FShowThreadIds;
			tte.fShowTagName = FShowTagName;
			tte.tlFilter = TlFilter;
			tte.strListeners = GetListenersString();
			tte.arttTags = new TraceTag[m_dctTagNameToStatus.Count];
			int iTag = 0;
			foreach (string strTag in m_dctTagNameToStatus.Keys)
			{
				tte.arttTags[iTag] = new TraceTag();
				tte.arttTags[iTag].strName = strTag;
				tte.arttTags[iTag++].fOn = m_dctTagNameToStatus[strTag];
			}
		}

		/// <summary>
		/// Load this TraceTypeInfo from persist data
		/// </summary>
		internal void LoadFromPersistData()
		{
			TraceTagEnum tte = TraceTagEnumFromTraceSettings();
			if (tte != null)
			{
				FShowTimeStamps = tte.fShowTimeStamps;
				FShowSeverity = tte.fShowSeverity;
				FShowThreadIds = tte.fShowThreadIds;
				FShowTagName = tte.fShowTagName;
				TlFilter = tte.tlFilter;
				if (tte.strListeners != string.Empty)
				{
					foreach (string strName in ParseListenerString(tte.strListeners))
					{
						try
						{
							AddListener(strName);
						}
						catch (Exception)
						{
							continue;
						}
					}
				}
				foreach (TraceTag ttg in tte.arttTags)
				{
					ChangeTagNameStatus(ttg.strName, ttg.fOn);
				}
			}
			else
			{
				// Turn on the default listener by...well, default.
				AddListener("Default");
			}
		}

#region Held operations
		/// <summary>
		/// Set us to use real data rather than the held data
		/// </summary>
		/// <param name="fCopy">Copy the data from the held data into the real if true</param>
		public void SetReal(bool fCopy)
		{
			m_fHolding = false;
			if (fCopy)
			{
				m_hiHeld.CopyTo(m_hiReal);
			}
		}

		/// <summary>
		/// Set us to use held data rather than the real data and copy our current real data
		/// into the held.
		/// </summary>
		public void SetHeld()
		{
			m_fHolding = true;
			m_hiReal.CopyTo(m_hiHeld);
		}
#endregion

#region Listeners
		/// <summary>
		/// Add this listener to those we print to
		/// </summary>
		/// <param name="tlis">Listener to be added</param>
		internal void AddListener(TraceListener tlis)
		{
			if (!AlListeners.Contains(tlis))
			{
				AlListeners.Add(tlis);
			}
		}

		/// <summary>
		/// Add listener to those we print to
		/// </summary>
		/// <param name="strName">Name of the listener to be added</param>
		internal void AddListener(string strName)
		{
			foreach (TraceListener tlis in System.Diagnostics.Trace.Listeners)
			{
				try
				{
					if (tlis.Name == strName)
					{
						AddListener(tlis);
						break;
					}
				}
				catch (Exception)
				{
					continue;
				}
			}
		}

		/// <summary>
		/// Remove listener from those we print to
		/// </summary>
		/// <param name="tlis">Listener to be removed</param>
		internal void RemoveListener(TraceListener tlis)
		{
			AlListeners.Remove(tlis);
		}
#endregion

#region Buffering
		/// <summary>
		/// One write record is queued up for the background printer for each ShowTrace performed.  These are then
		/// dequeued by the backgrnd printer to be printed to the appropriate listeners.
		/// </summary>
		internal struct TtiWriteRecord
		{
			/// <summary>
			/// Constructor
			/// </summary>
			/// <param name="ttiParm">TraceTypeInfo which caused this record to be created</param>
			/// <param name="strMsgParm">String to write to listeners</param>
			public TtiWriteRecord(TraceTypeInfo ttiParm, string strMsgParm)
			{
				tti = ttiParm;
				strMessage = strMsgParm;
				fEndRecord = false;
			}

			/// <summary>
			/// Special marker record to indicate that there's nothing left in the queue
			/// </summary>
			/// <returns>A TtiWriteRecord with fEndRecord turned on</returns>
			public static TtiWriteRecord EndRecord()
			{
				TtiWriteRecord twr = new TtiWriteRecord();
				twr.fEndRecord = true;
				return twr;
			}

			/// <summary>
			/// Indicates that there are no more records in the queue
			/// </summary>
			public bool fEndRecord;

			/// <summary>
			/// The TraceTypeInfo which created this record
			/// </summary>
			public TraceTypeInfo tti;

			/// <summary>
			/// The message to be printed
			/// </summary>
			public string strMessage;
		}

		/// <summary>
		/// The class which holds the buffering methods for async IO.
		/// </summary>
		static internal class TextBuffer
		{
			/// <summary>
			/// The queue of write records to be written by the background thread
			/// </summary>
			static Queue<TtiWriteRecord> qrec = new Queue<TtiWriteRecord>();

			/// <summary>
			/// StringBuilder which is appended onto in order to produce a final string
			/// to be written
			/// </summary>
			static StringBuilder s_sbld = new StringBuilder();

			/// <summary>
			/// Handle which tells the background thread that there is some new
			/// data to be printed
			/// </summary>
			static EventWaitHandle s_ewh = new AutoResetEvent(false);

			/// <summary>
			/// Handle which tells the bacground thread to exit
			/// </summary>
			static EventWaitHandle s_exwh = new ManualResetEvent(false);

			/// <summary>
			/// Array so the background thread can wait on either of the above simultaneously
			/// </summary>
			static WaitHandle[] s_arwh = new WaitHandle[] { s_ewh, s_exwh };

			/// <summary>
			/// Tell the background thread to exit
			/// </summary>
			public static void Exit()
			{
				s_exwh.Set();
			}

			/// <summary>
			/// Wait on either a data ready event or an exit event.
			/// </summary>
			/// <returns>true if the newly arrived event is for new text, otherwise false</returns>
			public static bool FNextEventIsNewText()
			{
				return WaitHandle.WaitAny(s_arwh) != 1;
			}

			/// <summary>
			/// Write text to the current TtiWriteRecord without a CR.  This doesn't enqueu
			/// anything.
			/// </summary>
			/// <param name="str"></param>
			static public void Write(string str)
			{
				s_sbld.Append(str);
			}

			/// <summary>
			/// Write text along with a CR to the TtiWriteRecord and enqueu the write record.
			/// Wake up the background thread to print it out.
			/// </summary>
			/// <param name="str">String to be written</param>
			/// <param name="tti">TraceTypeInfo that caused this</param>
			static public void WriteLine(string str, TraceTypeInfo tti)
			{
				s_sbld.Append(str);
				lock (qrec)
				{
					qrec.Enqueue(new TtiWriteRecord(tti, s_sbld.ToString()));
				}
				s_sbld = new StringBuilder();
				s_ewh.Set();
			}

			/// <summary>
			/// Retrieve the next TtiWriteRecord.  If there are none, return an EndRecord.
			/// </summary>
			/// <returns></returns>
			static public TtiWriteRecord RetrieveTtiWriteRecord()
			{
				lock (qrec)
				{
					if (qrec.Count == 0)
					{
						return TtiWriteRecord.EndRecord();
					}
					return qrec.Dequeue();
				}
			}
		}

		/// <summary>
		/// The simple thread function for the background process.
		/// </summary>
		internal static void NetTraceIOThread()
		{
			while (TextBuffer.FNextEventIsNewText())
			{
				TtiWriteRecord twr;
				while (true)
				{
					twr = TextBuffer.RetrieveTtiWriteRecord();
					if (twr.fEndRecord)
					{
						break;
					}
					foreach (TraceListener tl in twr.tti.AlListeners)
					{
						tl.Write(twr.strMessage + '\n');
						tl.Flush();
					}
				}
			}
		}
#endregion

#region Retrieval/Setting
		public bool FIsListening(TraceListener tlis)
		{
			foreach (TraceListener tlisCur in AlListeners)
			{
				if (tlisCur == tlis)
				{
					return true;
				}
			}
			return false;
		}

		public void SetDescs()
		{
			foreach (Enum tag in Enum.GetValues(m_tp))
			{
				FieldInfo fi = m_tp.GetField(tag.ToString());
				TagDescAttribute[] arattr =
					(TagDescAttribute[])fi.GetCustomAttributes(
					typeof(TagDescAttribute), false);
				if (arattr.Length > 0)
				{
					m_DctDescs[tag.ToString()] = arattr[0].strDesc;
				}
			}
		}

		public string StrDescFromTagName(string strTag)
		{
			string strRet = null;

			if (m_DctDescs.ContainsKey(strTag))
			{
				strRet = m_DctDescs[strTag] as string;
			}
			return strRet;
		}

		public void ChangeTagNameStatus(String strTag, bool fOn)
		{
			if (m_dctTagNameToStatus.ContainsKey(strTag))
			{
				SetTagNameStatus(strTag, fOn);
			}
		}

		public void SetTagNameStatus(String strTag, bool fOn)
		{
			object objTag = Enum.Parse(m_tp, strTag);
			m_dctTagNameToStatus[strTag] = fOn;
			m_dctTagToStatus[objTag] = fOn;
		}

		public bool GetTagNameStatus(String strTag)
		{
			return m_dctTagNameToStatus[strTag];
		}

		public bool GetTagStatus(object objTag)
		{
			return m_dctTagToStatus[objTag];
		}
#endregion

#region Properties
		public TraceTrigger TraceTrigger
		{
			get
			{
				return m_ttfn;
			}
			set
			{
				m_ttfn = value;
			}
		}

		private HeldInfo HI
		{
			get
			{
				return m_fHolding ? m_hiHeld : m_hiReal;
			}
		}

		public List<TraceListener> AlListeners
		{
			get
			{
				return HI.m_lstListeners;
			}
			set
			{
				HI.m_lstListeners = value;
			}
		}

		public Type TpTraceTagEnumType
		{
			get
			{
				return m_tp;
			}
		}

		public bool FPrintSupplementalInfo
		{
			get
			{
				return FShowTimeStamps | FShowThreadIds | FShowTagName | FShowSeverity;
			}
		}

		public bool FShowTimeStamps
		{
			get
			{
				return HI.m_fShowTimeStamps;
			}
			set
			{
				HI.m_fShowTimeStamps = value;
			}
		}

		public bool FShowThreadIds
		{
			get
			{
				return HI.m_fShowThreadIds;
			}
			set
			{
				HI.m_fShowThreadIds = value;
			}
		}

		public bool FShowSeverity
		{
			get
			{
				return HI.m_fShowSeverity;
			}
			set
			{
				HI.m_fShowSeverity = value;
			}
		}

		public bool FShowTagName
		{
			get
			{
				return HI.m_fShowTagName;
			}
			set
			{
				HI.m_fShowTagName = value;
			}
		}

		public TraceLevel TlFilter
		{
			get
			{
				return HI.m_tlFilter;
			}
			set
			{
				HI.m_tlFilter = value;
			}
		}

		public ICollection<string> TagNames
		{
			get
			{
				return m_dctTagNameToStatus.Keys;
			}
		}
#endregion

#region Internal classes
		private class HeldInfo
		{
			/// <summary>
			/// An arrayList of listeners the tag in this enum will write to
			/// </summary>
			public List<TraceListener> m_lstListeners = new List<TraceListener>();

			/// <summary>
			/// If a trace has a level, then trace only if its <= m_tlFilter
			/// </summary>
			public TraceLevel m_tlFilter = TraceLevel.Verbose;

			/// <summary>
			/// Supplemental info flags...
			/// </summary>
			public bool m_fShowTimeStamps = false;
			public bool m_fShowThreadIds = false;
			public bool m_fShowTagName = false;
			public bool m_fShowSeverity = false;

			public void CopyTo(HeldInfo hi)
			{
				hi.m_lstListeners = new List<TraceListener>(m_lstListeners);
				hi.m_tlFilter = m_tlFilter;
				hi.m_fShowTimeStamps = m_fShowTimeStamps;
				hi.m_fShowThreadIds = m_fShowThreadIds;
				hi.m_fShowTagName = m_fShowTagName;
				hi.m_fShowSeverity = m_fShowSeverity;
			}
		}
#endregion
}
#endif