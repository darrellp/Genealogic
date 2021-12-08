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
        /// The following hash table maps enums to their enum info structures.  This is done so that various
        /// projects can have their own tracing info/structures/config files.
        /// </summary>
        private static readonly Dictionary<Type, EnumInfo> DctEnumToEnumInfo = new();

        /// <summary>
        /// DctTagNameToEnumInfo maps trace tag names to the EnumInfo structure
        /// for that type which contains the status of it's individual trace tags, etc..  We need names so
        /// that we can deal with the names in the listbox of the trace dialog.
        /// </summary>
        private static readonly Dictionary<string, EnumInfo> DctTagNameToEnumInfo = new();

        // DI injected services
        private readonly ILogger _log;
        private readonly IConfiguration _config;

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

        static NetTrace()
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
                            var asmRef = Assembly.Load(an);
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
                // Probably should throw our own exception here for "NetTraceInitializationFailure" or some such
                // FStarted == false will be the indication of a failure
            }
        }
        #endregion

        #region Tags Dialog
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Return all the EnumInfo objects for all registered TraceTags enums. </summary>
        ///
        /// <remarks>	Darrellp, 10/5/2012. </remarks>
        ///
        /// <returns>	ICollection of EnumInfo. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private static IEnumerable<EnumInfo> AllEnumInfos()
        {
            return DctEnumToEnumInfo.Values;
        }

#if FUTURE
        // Keeping this in because I may use it in the future if I decide to have more mutable data
        // about individual tags

        private static void SaveTtiInfo()
        {
            // Tell each EnumInfo to hold its current data so we can restore it in case the
            // user hits cancel...
            foreach (var tti in AllEnumInfos())
            {
                tti.SetHeld();
            }
        }

        private static void RestoreTtiInfo(bool fOk)
        {
            // Copy back either the changed values or the old values depending
            // on whether the user hit OK or Cancel
            foreach (var tti in AllEnumInfos())
            {
                tti.SetReal(fOk);
            }
        }
#endif

        internal static string GetFullName(Type tp, object objEnum)
        {
            return tp.FullName + "." + objEnum;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Returns a hash table mapping all registered tag names to their dialog binding info. </summary>
        ///
        /// <remarks>	Darrellp, 10/5/2012. </remarks>
        ///
        /// <returns>	Dictionary from full tag names to the dialog bindings for those tags. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal static Dictionary<string, DlgTagBinding> TagNameToBindingDictionary()
        {
            var tagNameToBindingDictionary = new Dictionary<string, DlgTagBinding>();

            foreach (var enumInfo in AllEnumInfos())
            {
                foreach (var tag in Enum.GetValues(enumInfo.EnumType))
                {
                    var strTag = GetFullName(enumInfo.EnumType, tag);
                    tagNameToBindingDictionary[strTag] =
                        new DlgTagBinding(strTag,
                            enumInfo.GetTagNameStatus(strTag),
                            enumInfo.StrDescFromTagName(strTag),
                            enumInfo.EnumType);
                }
            }

            return tagNameToBindingDictionary;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Retrieves the EnumInfo from the Enum name. </summary>
        ///
        /// <remarks>	Darrellp, 10/5/2012. </remarks>
        ///
        /// <param name="strTag">	Enum name. </param>
        ///
        /// <returns>	EnumInfo for the tag name. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private static EnumInfo TtiFromTagName(string strTag)
        {
            return DctTagNameToEnumInfo[strTag];
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>	Turn tracing on or off for a trace tag. </summary>
        ///
        /// <remarks>	Darrellp, 10/5/2012. </remarks>
        ///
        /// <param name="strTag">	Trace tag. </param>
        /// <param name="tti">		It's EnumInfo. </param>
        /// <param name="fOn">		True turns it on, false turns it off. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        private static void SetTracing(string strTag, EnumInfo tti, bool fOn)
        {
            tti.SetTagNameStatus(strTag, fOn);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Put up the race dialog. </summary>
        ///
        /// <remarks>   Darrell Plank, 12/6/2021. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        public void TraceDialog()
        {
            // I used to be the one who decided where tags wrote to, printed their time stamps, etc..
            // This stuff has been taken over by the logger (preferably Serilog) that we just use
            // as a service.  I had this stuff available on a tag by tag basis and TtiInfo was the
            // place they were stored.  Now that I'm no longer responsible for them I don't need this
            // stuff but I'm leaving it commented out because I may want to put some sort of info in
            // in the future - for instance, right now I only print out informational messages but I
            // may change my mind about that in the future.
            
            // try
            // {
            //      SaveTtiInfo();
            var traceDialog = new TraceDialog();
            var fOk = traceDialog.ShowDialog() ?? false;

            // If they hit cancel or closed out then forget everything
            if (!fOk) return;

            // Otherwise, set any tags whose values have changed 
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
            foreach (var tti in AllEnumInfos())
            {
                tti.SaveToPersistData(_config);
            }
            // }
            //finally
            //{
            //    RestoreTtiInfo(fOk);
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

        /// <summary>
        /// Prints out the message if the tag has tracing enabled
        /// </summary>
        /// 
        /// <param name="tag">The tag which enables or disables tracing</param>
        /// <param name="str">Serilog style message string</param>
        /// <param name="arobjParm">Serilog style objects to print</param>
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
        /// Retrieves the EnumInfo from the tag object
        /// </summary>
        /// <param name="objTag">Tag object</param>
        /// <returns>EnumInfo for the tag object</returns>
        private static EnumInfo TtiFromTag(object objTag)
        {
            return DctEnumToEnumInfo[objTag.GetType()];
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
        /// <summary>	Return the EnumInfo for a given enum type. </summary>
        ///
        /// <remarks>	Darrellp, 10/5/2012. </remarks>
        ///
        /// <param name="tp">	The enum type. </param>
        ///
        /// <returns>	The EnumInfo for enumType. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        internal static EnumInfo TtiFromType(Type tp)
        {
            return DctEnumToEnumInfo[tp];
        }
        #endregion

        #region Assembly Wrangling
        /// <summary>
        /// All the assemblies referenced by the current assembly.  This is only
        /// used while doing assembly checks to keep from repeating an assembly
        /// that has already been checked.
        /// </summary>
        private static readonly List<Assembly> LstAssemblies = new();

        private static bool FPerformAsmCheck(Assembly asm)
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
            return DctEnumToEnumInfo.ContainsKey(tp);
        }

        /// <summary>
        /// Register a TraceTags enum and return it's EnumInfo
        /// </summary>
        /// <param name="tp">TraceTags enum to register</param>
        /// <returns>EnumInfo for newly registered TraceTags enum</returns>
        private static EnumInfo TtiRegisterType(Type tp)
        {
            EnumInfo tti = new(tp);
            DctEnumToEnumInfo.Add(tp, tti);
            return tti;
        }

        /// <summary>
        /// Get the EnumInfo for a TraceTags enum
        /// </summary>
        /// <param name="tp">Enum to return info for</param>
        /// <returns>EnumInfo for this enum</returns>
        private static EnumInfo TtiFromTp(Type tp)
        {
            return FTypeRegistered(tp) ? DctEnumToEnumInfo[tp] : TtiRegisterType(tp);
        }

        /// <summary>
        /// Load all the tags in an enum type.  "Loading" in this sense means looking at all the trace tag
        /// labeled enums in the assembly (or assemblies), setting up EnumInfo structures for each of
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
                DctTagNameToEnumInfo[strFullName] = tti;

                // Initially set to true.  This will take care of any tags which
                // haven't been previously registered and set them to true.  Any
                // tags in the persist data will overwrite this value in LoadFromPersistData.
                tti.SetTagNameStatus(GetFullName(tti.EnumType, objEnum), true);
                //var fullName = tti.EnumType.FullName + "." + objEnum;
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