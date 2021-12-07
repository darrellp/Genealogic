using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace NetTrace
{
    #region Dependency Injection interface
    public interface INetTrace
    {
        void Trace(object tag, string seqMsg, params object[] values);
        // ReSharper disable once UnusedMember.Global
        void SetTagStatus(object tag, bool fOn);
        void TraceDialog();
    }
    #endregion

    public class NetTrace : INetTrace
    {
        #region Private variables
        /// <summary>
        /// The following hash table maps types to their type info structures.  This is done so that various
        /// projects can have their own tracing info/structures/config files.
        /// </summary>
        private static readonly Dictionary<Type, TraceTypeInfo> DctTypeToTypeInfo = new();

        /// <summary>
        /// s_htTagNameToTraceTypeInfo maps trace tag names to the TraceTypeInfo structure
        /// for that type which contains the status of it's individual trace tags, etc..  We need names so
        /// that we can deal with the names in the listbox of the trace dialog.
        /// </summary>
        private static readonly Dictionary<string, TraceTypeInfo> DctTagNameToTraceTypeInfo = new();

        /// <summary>
        /// All the assemblies referenced by the current assembly
        /// </summary>
        private static readonly List<Assembly> LstAssemblies = new();

        private static ILogger _log;
        private static IConfiguration _config;

        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        internal static bool FStarted { get; }
        #endregion

        #region Constructors
        // Dependency injection constructor
        public NetTrace(ILogger log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        static NetTrace()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            try
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (FPerformAsmCheck(asm))
                    {
                        CheckAssemblyForTraceTags(asm);

                        foreach (var an in asm.GetReferencedAssemblies())
                        {
                            Assembly asmRef = Assembly.Load(an);
                            if (FPerformAsmCheck(asmRef))
                            {
                                CheckAssemblyForTraceTags(asmRef);
                            }
                        }
                    }
                }
                FStarted = true;
            }
            catch (Exception)
            {
                // FStarted == false will be the indication of a failure
            }
        }
        #endregion

        #region Tags Dialog
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Return all the TraceTypeInfo objects for all registered TraceTags enums. </summary>
        ///
        /// <remarks>	Darrellp, 10/5/2012. </remarks>
        ///
        /// <returns>	ICollection of TraceTypeInfo. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private static IEnumerable<TraceTypeInfo> AllTraceTypeInfos()
        {
            return DctTypeToTypeInfo.Values;
        }

        // ReSharper disable once UnusedMember.Local
        private static void SaveTtiInfo()
        {
            // Tell each TraceTypeInfo to hold its current data so we can restore it in case the
            // user hits cancel...
            foreach (TraceTypeInfo tti in AllTraceTypeInfos())
            {
                tti.SetHeld();
            }
        }

        internal static string GetFullName(Type tp, object objEnum)
        {
            return tp.FullName + "." + objEnum;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Returns a hash table mapping all registered tag names to their current status. </summary>
        ///
        /// <remarks>	Darrellp, 10/5/2012. </remarks>
        ///
        /// <returns>	Hashtable. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private static Dictionary<string, DlgTagBinding> GetGlobalNamesStatusDictionary()
        {
            var dctNamesToStatus = new Dictionary<string, DlgTagBinding>();

            foreach (TraceTypeInfo tti in AllTraceTypeInfos())
            {
                foreach (var objEnum in Enum.GetValues(tti.Tp))
                {
                    string strTag = GetFullName(tti.Tp, objEnum);
                    dctNamesToStatus[strTag] =
                        new DlgTagBinding(strTag,
                            tti.GetTagNameStatus(strTag),
                            tti.StrDescFromTagName(strTag),
                            tti.Tp);
                }
            }

            return dctNamesToStatus;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Retrieves the TraceTypeInfo from the tag name. </summary>
        ///
        /// <remarks>	Darrellp, 10/5/2012. </remarks>
        ///
        /// <param name="strTag">	Tag name. </param>
        ///
        /// <returns>	TraceTypeInfo for the tag name. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private static TraceTypeInfo TtiFromTagName(string strTag)
        {
            return DctTagNameToTraceTypeInfo[strTag];
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Turn tracing on or off for a trace tag. </summary>
        ///
        /// <remarks>	Darrellp, 10/5/2012. </remarks>
        ///
        /// <param name="strTag">	Trace tag. </param>
        /// <param name="tti">		It's TraceTypeInfo. </param>
        /// <param name="fOn">		True turns it on, false turns it off. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void SetTracing(string strTag, TraceTypeInfo tti, bool fOn)
        {
            tti.SetTagNameStatus(strTag, fOn);
        }

        public void TraceDialog()
        {
            // try
            // {
                // SaveTtiInfo();
            var traceDialog = new TraceDialog(GetGlobalNamesStatusDictionary());
            var fOk = traceDialog.ShowDialog();

            // If they hit cancel or closed out then forget everything
            if (!(fOk ?? false)) return;

            // Reset any tags whose values have changed 
            foreach (var tagInfo in traceDialog.TagList)
            {
                var strTag = tagInfo.StrTag;
                var tti = TtiFromTagName(strTag);
                var fOn = traceDialog[strTag];

                if (tti.GetTagNameStatus(strTag) != fOn)
                {
                    SetTracing(strTag, tti, fOn);
                }
            }

            // Persist the changes
            foreach (var tti in AllTraceTypeInfos())
            {
                tti.SaveToPersistData(_config);
            }
            // }
            //finally
            //{
            //    //RestoreTtiInfo(fOk ?? false);
            //}

        }
        #endregion

        #region Tracing
        /// <summary>
        /// Returns true if we're tracing a tag
        /// </summary>
        /// 
        /// <param name="tag">Tag we're querying</param>
        /// <returns> true if we're tracing, else false </returns>
        public static bool FTracing(object tag)
        {
            return TtiFromTag(tag).GetTagStatus(tag);
        }

         public void Trace(object tag, string str, params object[] arobjParm)
         {
            if (FTracing(tag))
            {
                _log.Information("({tag}): " + str, tag, arobjParm);
            }
        }
        #endregion

        #region TTI Info
        /// <summary>
        /// Retrieves the TraceTypeInfo from the tag object
        /// </summary>
        /// <param name="objTag">Tag object</param>
        /// <returns>TraceTypeInfo for the tag object</returns>
        private static TraceTypeInfo TtiFromTag(object objTag)
        {
            return DctTypeToTypeInfo[objTag.GetType()];
        }

        public void SetTagStatus(object tag, bool fOn)
        {
            var name = tag.ToString();
            if (name != null)
            {
                TtiFromTag(tag).SetTagNameStatus(name, fOn);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Return the TraceTypeInfo for a given enum type. </summary>
        ///
        /// <remarks>	Darrellp, 10/5/2012. </remarks>
        ///
        /// <param name="tp">	The enum type. </param>
        ///
        /// <returns>	The TraceTypeInfo for tp. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal static TraceTypeInfo TtiFromType(Type tp)
        {
            return DctTypeToTypeInfo[tp];
        }
        #endregion

        #region Assembly Wrangling
        static bool FPerformAsmCheck(Assembly asm)
        {
            // Can the null case happen?  I don't think so but if it does
            // give it a name it will reject
            string strName = asm.GetName().Name ?? "netstandard";
            bool fRet = strName != "netstandard" &&
                !strName.StartsWith("System") &&
                !strName.StartsWith("Microsoft.") &&
                !strName.StartsWith("Serilog") &&
                strName != "vshost" &&
                strName != "NetTrace" &&
                strName != "PresentationFramework" &&
                strName != "WindowsBase" &&
                strName != "PresentationCore" &&
                strName != "DirectWriteForwarder" &&
                strName != "UIAutomationTypes" &&
                !LstAssemblies.Contains(asm);
            if (fRet)
            {
                Debug.WriteLine($"Checking {asm.FullName}");
                LstAssemblies.Add(asm);
            }
            return fRet;
        }

        private static void CheckAssemblyForTraceTags(Assembly asm)
		{
            foreach (var tp in asm.GetTypes())
			{
				var fIsTagEnum = false;
				foreach (var attr in Attribute.GetCustomAttributes(tp))
				{
					if (attr is TraceTagsAttribute)
					{
						Debug.Assert(tp.IsEnum, "Somehow got [TraceTags] attribute on a non-enum");
						LoadTags(tp);
						fIsTagEnum = true;
					}
				}
				if (fIsTagEnum)
				{
					TtiFromTp(tp).SetDescs();
				}
			}
		}
        #endregion

        #region Loading/Registering tags
        /// <summary>
        /// Determine whether this enum type has already been registered with the trace machinery.
        /// </summary>
        /// <param name="tp">Type to check</param>
        /// <returns>True if it's currently registered</returns>
        private static bool FTypeRegistered(Type tp)
        {
            return DctTypeToTypeInfo.ContainsKey(tp);
        }

        /// <summary>
        /// Register a TraceTags enum and return it's TraceTypeInfo
        /// </summary>
        /// <param name="tp">TraceTags enum to register</param>
        /// <returns>TraceTypeInfo for newly registered TraceTags enum</returns>
        private static TraceTypeInfo TtiRegisterType(Type tp)
        {
            TraceTypeInfo tti = new(tp);
            DctTypeToTypeInfo.Add(tp, tti);
            return tti;
        }

        /// <summary>
        /// Get the TraceTypeInfo for a TraceTags enum
        /// </summary>
        /// <param name="tp">Enum to return info for</param>
        /// <returns>TraceTypeInfo for this enum</returns>
        private static TraceTypeInfo TtiFromTp(Type tp)
        {
            return FTypeRegistered(tp) ? DctTypeToTypeInfo[tp] : TtiRegisterType(tp);
        }

        /// <summary>
        /// Load all the tags in an enum type.  "Loading" in this sense means looking at all the trace tag
        /// labeled enums in the assembly (or assemblies), setting up TraceTypeInfo structures for each of
        /// them and setting all their tags, either to true if they're not currently in the persist data or
        /// whatever value the persist data has for them if they are.
        /// </summary>
        /// 
        /// <param name="tp">enum type whose tags will be loaded</param>
        private static void LoadTags(Type tp)
        {
            if (!tp.IsEnum)
            {
                throw new ArgumentException(@"Non-enum type passed to Tracer.LoadTags()");
            }

            var tti = TtiFromTp(tp);
            foreach (var objEnum in Enum.GetValues(tp))
            {
                var strFullName = GetFullName(tp, objEnum);
                DctTagNameToTraceTypeInfo[strFullName] = tti;

                // Initially set to true.  This will take care of any tags which
                // haven't been previously registered and set them to true.  Any
                // tags in the persist data will overwrite this value in LoadFromPersistData.
                tti.SetTagNameStatus(GetFullName(tti.Tp, objEnum), true);
                //var fullName = tti.Tp.FullName + "." + objEnum;
                //tti.SetTagNameStatus(fullName, true);
            }

            // Get any values in the persist data, overriding the "true" value set above.
            tti.LoadFromPersistData();
        }
        #endregion
	}

    #region Attributes
    /// <summary>
    /// Marks an enum as a set of trace tags
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public sealed class TraceTagsAttribute : Attribute
    {
    }

    /// <summary>
    /// Allows the user to give a descriptive name for a trace tags enum
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public sealed class EnumDescAttribute : Attribute
    {
        public EnumDescAttribute(string strDesc)
        {
            StrDesc = strDesc;
        }

        public string StrDesc { get; }
    }

    /// <summary>
    /// Allows the user to give a descriptive name for a tag
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public sealed class TagDescAttribute : Attribute
    {
        public TagDescAttribute(string strDesc)
        {
            this.StrDesc = strDesc;
        }

        public string StrDesc { get; }
    }
    #endregion
}