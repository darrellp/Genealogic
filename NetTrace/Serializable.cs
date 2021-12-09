using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NetTrace
{
#pragma warning disable CS8618
    /// <summary>
    /// Contains all tag info for serialization
    /// </summary>
    /// 
    /// <remarks>
    /// This class exists solely for serialization.  The EnumInfo class is not really
    /// suitable for serialization (though there may be a nice way to do it with a
    /// bunch of hook functions, etc. - something to check out some winter's day)
    /// so we have this class which has beautiful serializable data.
    /// </remarks>
    public class NetTraceSerializable
    {
        /// <summary>
        /// List of all the enum structs we recognize
        /// </summary>
        /// 
        /// <value>
        /// The trace tag enum information
        /// </value>
        public List<EnumSerializable> Enums { get; set; } = new();

        internal string ToJsonString()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            return JsonSerializer.Serialize(this, options);
        }

        internal static NetTraceSerializable? FromJsonString(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<NetTraceSerializable>(json);
            }
            catch (Exception)
            {
                return new NetTraceSerializable();
            }
        }
    }

    /// <summary>
    /// Information for serializing a single trace tag enum.
    /// </summary>
    /// 
    /// <remarks>
    /// Includes the enum name/description and all the individual trace tags within this enum.
    /// </remarks>
    public class EnumSerializable
    {
        // Enum name
        public string StrName { get; set; }
        // Trace tag info
        public TagSerializable[] TagList { get; set; }
    }

    public class TagSerializable
    {
        public string? StrName { get; set; }
        public bool FOn { get; set; }
    }
}
