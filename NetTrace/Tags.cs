using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NetTrace
{
    internal class Tags
    {
        /// <summary>
        /// Dictionary which keeps all our tag information
        /// </summary>
        internal static Dictionary<string, DlgTagBinding>? DctNamesToBinding;

        /// <summary>
        /// List of trace tag enum types
        /// </summary>
        internal static readonly List<Type> LstTraceTagEnums = new();

        internal static void Init(Dictionary<string, DlgTagBinding> dctNamesToBindings)
        {
            Debug.Assert(DctNamesToBinding != null);
            foreach (var binding in dctNamesToBindings.Keys
                         .Select(strTag => DctNamesToBinding[strTag])
                         .Where(binding => LstTraceTagEnums.All(tp => binding.TpEnum != tp)))
            {
                LstTraceTagEnums.Add(binding.TpEnum);
            }
        }

        /// <summary>
        /// Returns a string description of a type
        /// </summary>
        /// <param name="tp">Enum type we're finding a description for</param>
        /// <returns>descriptive string</returns>
        internal static string StrDescFromTp(Type tp)
        {
            string? strRet = null;
            foreach (var attr in Attribute.GetCustomAttributes(tp))
            {
                if (attr is EnumDescAttribute enumDescAttribute)
                {
                    strRet = enumDescAttribute.StrDesc;
                }
            }
            return strRet ?? (TraceTypeInfo.StrCanonicalTypeName(tp));
        }
    }
}
