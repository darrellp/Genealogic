namespace GenealogicCore
{
    internal class Name
    {
        internal string First { get; init; } = String.Empty;
        internal string Middle { get; init; } = String.Empty;
        internal string Last { get; init; } = String.Empty;
        internal string Prefix { get; init; } = String.Empty;
        internal string Suffix { get; init; } = String.Empty;
        internal string ExtraNames { get; init; } = String.Empty;
        internal string NickName { get; init; } = String.Empty;
        internal string SurnamePrefix { get; init; } = String.Empty;    // Optional Surname prefix not used in sorting such as "Van" or "De"

        internal Name(
            string first, 
            string middle, 
            string last, 
            string prefix = "", 
            string suffix = "", 
            string extraNames = "", 
            string nickName = "",
            string surnamePrefix = "")
        {
            First = first;
            Middle = middle;
            Last = last;
            Prefix = prefix;
            Suffix = suffix;
            ExtraNames = extraNames;
            NickName = nickName;
            SurnamePrefix = surnamePrefix;
        }
    }
}
