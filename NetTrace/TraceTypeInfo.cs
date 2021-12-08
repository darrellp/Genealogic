using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace NetTrace
{
	/// <summary>
	/// Each individual [TraceTags] enum type in the assembly is mapped one to one to an EnumInfo object.
	/// This object holds the status of each member of that enum (turned on or off) and the information pertaining
	/// to that enum in general.
	/// </summary>
	internal class EnumInfo
	{
		#region Private variables
		/// <summary>
		/// Hashtable which maps members of this enum to their status (on/off)
		/// </summary>
		private readonly Dictionary<string, bool> _dctTagNameToEnabled = new();

		/// <summary>
		/// Maps actual tag (rather than the tag name) to it's status.  This table is redundant
		/// and is only here to speed up Trace calls.
		/// </summary>
		private readonly Dictionary<object, bool> _dctTagToEnabled = new();

		/// <summary>
		/// Mapping from tag names to their descriptions
		/// </summary>
		private readonly Dictionary<string, string> _dctDescs = new();

        /// <summary>
        /// Enum type for the tags in this EnumInfo
        /// </summary>
        private readonly Type _enumType;

        /// <summary>
        /// TraceSettings structure which persists our data to user.config.
        /// </summary>
        private static TraceSettings? _traceSettings;

        public Type EnumType => _enumType;

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Returns a unique string corresponding to an enum type which consists of the assembly
        /// containing the type followed by the type name. 
        /// </summary>
        ///
        /// <remarks>	Darrellp, 10/5/2012. </remarks>
        ///
        /// <param name="tp">	The enum type we want a string for. </param>
        ///
        /// <returns>	The unique name for this enum type. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal static string StrCanonicalTypeName(Type tp)
        {
            return Assembly.GetAssembly(tp)?.GetName().Name + ":" + tp;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Retrieves the TraceTagEnum object for this EnumInfo from the persistence data. 
        /// </summary>
        ///
        /// <remarks>	Darrellp, 10/5/2012. </remarks>
        ///
        /// <returns>	The TraceTagEnum or null if none found. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        TraceTagEnum? TraceTagEnumFromTraceSettings()
        {
            _traceSettings ??= new TraceSettings();
            var canonicalName = StrCanonicalTypeName(_enumType);
            return _traceSettings.TagEnums.Find(tteTest => tteTest.StrName == canonicalName);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
		/// <summary>	Save this EnumInfo to persistent data. </summary>
		///
		/// <remarks>	Darrellp, 10/5/2012. </remarks>
		////////////////////////////////////////////////////////////////////////////////////////////////////
		internal void SaveToPersistData(IConfiguration config)
        {
            var tte = TraceTagEnumFromTraceSettings();
            if (tte == null)
            {
                tte = new TraceTagEnum(StrCanonicalTypeName(_enumType), _dctTagNameToEnabled.Count);
                _traceSettings?.TagEnums.Add(tte);
            }
            var iTag = 0;
            foreach (var strTag in _dctTagNameToEnabled.Keys)
            {
                tte.ArttTags[iTag] = new TraceTag { StrName = strTag };
                tte.ArttTags[iTag++].FOn = _dctTagNameToEnabled[strTag];
            }
        }

        public void ChangeTagNameStatus(String strTag, bool fOn)
        {
            if (_dctTagNameToEnabled.ContainsKey(strTag))
            {
                SetTagNameStatus(strTag, fOn);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Load this EnumInfo from persist data. </summary>
        ///
        /// <remarks>	Darrellp, 10/5/2012. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal void LoadFromPersistData()
        {
            var tte = TraceTagEnumFromTraceSettings();
            if (tte == null)
            {
                return;
            }

            foreach (var ttg in tte.ArttTags)
            {
                Debug.Assert(ttg.StrName != null, "ttg.StrName != null");
                ChangeTagNameStatus(ttg.StrName, ttg.FOn);
            }
        }

		public string? StrDescFromTagName(string strTag)
        {
            string? strRet = null;

            if (_dctDescs.ContainsKey(strTag))
            {
                strRet = _dctDescs[strTag];
            }
            return strRet;
        }

        public bool GetTagNameStatus(String strTag)
        {
            return _dctTagNameToEnabled[strTag];
        }

		/// <summary>
		/// In order to accommodate the Cancel button cancelling all changes we have two copies of
		/// the general parameters for the EnumInfo.  The HeldInfo property returns the proper
		/// one based on _fHolding.  Before the trace dialog, SetHeld is called which copies the
		/// "real" data from _hiReal to the held data in _hiHeld.  This is the data used during
		/// the dialog.  If the user hits OK in that dialog then the held data is copied back into the
		/// real data and the changes become official.  If the user hits cancel, then the real (original)
		/// data is maintained and all the changes from the dialog are effectively thrown away.
		/// </summary>
#pragma warning disable CS0414
        private bool _fHolding;
#pragma warning restore CS0414
        private readonly HeldInfo _hiReal = new();
        private readonly HeldInfo _hiHeld = new();

		public void SetDescs()
		{
			foreach (Enum tag in Enum.GetValues(_enumType))
			{
				var fi = _enumType.GetField(tag.ToString());
                if (fi == null)
                {
                    continue;
                }
				var arattr =
					(TagDescAttribute[])fi.GetCustomAttributes(
					typeof(TagDescAttribute), false);
				if (arattr.Length > 0)
				{
					_dctDescs[NetTrace.GetFullName(_enumType, tag)] = arattr[0].StrDesc;
				}
			}
		}

		public void SetTagNameStatus(String strTag, bool fOn)
		{
                object objTag = Enum.Parse(_enumType, ShortTagName(strTag));
                _dctTagNameToEnabled[strTag] = fOn;
			    _dctTagToEnabled[objTag] = fOn;
		}

        private string ShortTagName(string strTag)
        {
            return strTag.Substring(strTag.LastIndexOf('.') + 1);
        }

#if LATER
		/// <summary>
		/// The delegate to be called whenever this trace is triggered
		/// </summary>
		private TraceTrigger m_ttfn;
		/// <summary>
		/// In order to accommodate the Cancel button cancelling all changes we have two copies of
		/// the general parameters for the EnumInfo.  The HeldInfo property returns the proper
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

        public IEnumerable<string> TagNames => _dctTagNameToEnabled.Keys;

        #region Constructors / Loaders
		/// <summary>
		/// Constructs a new EnumInfo based on the TraceTags enum type passed in
		/// </summary>
		/// <param name="enumType">Enum holding the trace tags for this EnumInfo objects</param>
		public EnumInfo(Type enumType)
		{
			_enumType = enumType;
		}
		#endregion

        public bool GetTagStatus(object objTag)
        {
            return _dctTagToEnabled[objTag];
        }
    #region Internal classes
    internal class HeldInfo
    {
        /// <summary>
        /// Supplemental info flags...
        /// </summary>

        // ReSharper disable once MemberCanBeMadeStatic.Global
        // ReSharper disable once UnusedParameter.Global
        public void CopyTo(HeldInfo hi)
        {
        }
    }
	#endregion

	#region Held operations
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>	Set us to use real data rather than the held data. </summary>
    ///
    /// <remarks>	Darrellp, 10/5/2012. </remarks>
    ///
    /// <param name="fCopy">	Copy the data from the held data into the real if true. </param>
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    // ReSharper disable once UnusedMember.Global
    public void SetReal(bool fCopy)
    {
        _fHolding = false;
        if (fCopy)
        {
            _hiHeld.CopyTo(_hiReal);
        }
    }

#if FUTURE
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	
        /// Set us to use held data rather than the real data and copy our current real data into the
        /// held. 
        /// </summary>
        ///
        /// <remarks>	Darrellp, 10/5/2012. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public void SetHeld()
    {
        _fHolding = true;
        _hiReal.CopyTo(_hiHeld);
    }
#endif
#endregion
	}
}
