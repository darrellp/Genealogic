using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NetTrace
{
    internal static class Tags
    {
        /// <summary>
        /// Dictionary which keeps all our tag information
        /// </summary>
        internal static Dictionary<string, DlgTagBinding>? DctNamesToBinding;

        /// <summary>
        /// List of trace tag enums which the dialog binds to
        /// </summary>
        internal static readonly List<Type> LstEnums = new();

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Finds all enums and places them in LstEnums which the dialog binds to. </summary>
        ///
        /// <remarks>   Darrell Plank, 12/6/2021. </remarks>
        ///
        /// <param name="tagNameToBindingDictionary">   Dictionary of tag name to bindings. </param>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        internal static void BindEnums(Dictionary<string, DlgTagBinding> tagNameToBindingDictionary)
        {
            Debug.Assert(DctNamesToBinding != null);
            // Use a hashset to ensure uniqueness?
            foreach (var binding in tagNameToBindingDictionary.Keys
                         .Select(strTag => DctNamesToBinding[strTag])
                         .Where(binding => LstEnums.All(tp => binding.EnumType != tp)))
            {
                LstEnums.Add(binding.EnumType);
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
            return strRet ?? (EnumInfo.StrCanonicalTypeName(tp));
        }
    }
}
