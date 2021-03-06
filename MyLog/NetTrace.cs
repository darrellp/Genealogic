using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace MyLog
{
    #region Dependency Injection interface
    public interface INetTrace
    {
        void Trace(object tag, string seqMsg, params object[] values);
        void SetTagStatus(object tag, bool fOn);
    }
    #endregion

    public class NetTraceOld : INetTrace
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
        private static readonly List<Assembly> LstAsms = new();

        private static ILogger _log;
        private static IConfiguration _config;

        internal static bool FStarted { get; }
        #endregion

        #region Constructors
        // Dependency injection constructor
        public NetTraceOld(ILogger log, IConfiguration config)
        {
            _log = log;
            _config = config;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        static NetTraceOld()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            Debug.WriteLine("Entering MyLog static constructor");
            try
            {
                foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
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
                strName != "MyLog" &&
                strName != "PresentationFramework" &&
                strName != "WindowsBase" &&
                strName != "PresentationCore" &&
                strName != "DirectWriteForwarder" &&
                strName != "UIAutomationTypes" &&
                !LstAsms.Contains(asm);
            if (fRet)
            {
                Debug.WriteLine($"Checking {asm.FullName}");
                LstAsms.Add(asm);
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
            TraceTypeInfo tti;

            if (!FTypeRegistered(tp))
            {
                tti = TtiRegisterType(tp);
            }
            else
            {
                tti = DctTypeToTypeInfo[tp];
            }
            return tti;
        }


        /// <summary>
        /// Load all the tags in an enum type.  "Loading" in this sense means looking at all the trace tag
        /// labelled enums in the assembly (or assemblies), setting up TraceTypeInfo structures for each of
        /// them and setting all their tags, either to true if they're not currently in the persist data or
        /// whatever value the persist data has for them if they are.
        /// </summary>
        /// <param name="tp">enum type whose tags will be loaded</param>
        static void LoadTags(Type tp)
        {
            if (!tp.IsEnum)
            {
                throw new ArgumentException(@"Non-enum type passed to Tracer.LoadTags()");
            }

            var tti = TtiFromTp(tp);
            foreach (object objEnum in Enum.GetValues(tp))
            {
                DctTagNameToTraceTypeInfo[objEnum.ToString() ?? ""] = tti;

                // Initially set to true.  This will take care of any tags which
                // haven't been previously registered and set them to true.  Any
                // tags in the persist data will overwrite this value in LoadFromPersistData.
                tti.SetTagNameStatus(objEnum.ToString() ?? "", true);
            }

            // Get any values in the persist data, overriding the "true" value set above.
            // tti.LoadFromPersistData();
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
            this.strDesc = strDesc;
        }

        public string strDesc { get; }
    }
    #endregion
}