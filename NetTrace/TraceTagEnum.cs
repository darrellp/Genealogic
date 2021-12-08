using System.Collections.Generic;

namespace NetTrace
{
    /// <summary>
    /// Contains all tag info for serialization
    /// </summary>
    /// 
    /// <remarks>
    /// This class exists solely for serialization.
    /// </remarks>
    public class TraceSettings
    {
        /// <summary>
        /// List of all the enum structs we recognize
        /// </summary>
        /// 
        /// <value>
        /// The trace tag enum information
        /// </value>
        public List<TraceTagEnum> TagEnums { get; } = new();
    }

    /// <summary>
    /// Information for serializing a single trace tag enum.
    /// </summary>
    /// 
    /// <remarks>
    /// Includes the enum name/description and all the individual trace tags within this enum.
    /// </remarks>
    public class TraceTagEnum
    {
        // Enum name
        public readonly string StrName;
        // Trace tag info
        public readonly TraceTag[] ArttTags;

        public TraceTagEnum(string strName, int tagCount)
        {
            StrName = strName;
            ArttTags = new TraceTag[tagCount];
        }
    }

    public class TraceTag
    {
        public string? StrName { get; init; }
        public bool FOn { get; set; }
    }
}
