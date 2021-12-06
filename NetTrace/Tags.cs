using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTrace
{
    internal class Tags
    {
        /// <summary>
        /// Dictionary which keeps all our tag information
        /// </summary>
        internal static Dictionary<string, DlgTagBinding> DctNamesToStatus;

        /// <summary>
        /// List of trace tag enum types
        /// </summary>
        internal static readonly List<Type> LsttpEnums = new List<Type>();

        internal static void Init(Dictionary<string, DlgTagBinding> dctNamesToStatus)
        {
            foreach (string strTag in dctNamesToStatus.Keys)
            {
                DlgTagBinding tsad = DctNamesToStatus[strTag];

                if (LsttpEnums.All(tp => tsad.TpEnum != tp))
                {
                    LsttpEnums.Add(tsad.TpEnum);
                }
            }
        }

        /// <summary>
        /// Returns a string description of a type
        /// </summary>
        /// <param name="tp">Enum type we're finding a description for</param>
        /// <returns>descriptive string</returns>
        internal static String StrDescFromTp(Type tp)
        {
            string strRet = null;
            foreach (var attr in Attribute.GetCustomAttributes(tp))
            {
                var enumDescAttribute = attr as EnumDescAttribute;
                if (enumDescAttribute != null)
                {
                    strRet = enumDescAttribute.StrDesc;
                }
            }
            return strRet ?? (TraceTypeInfo.StrCanonicalTypeName(tp));
        }
    }
}
