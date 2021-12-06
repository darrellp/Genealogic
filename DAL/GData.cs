using Microsoft.Extensions.Configuration;
using NetTrace;

#region Trace Tag enums
[TraceTags,
 // Optional Description of the enum to be used in the trace dialog
 EnumDesc("Tags in DAL")]
internal enum t
{
    // Optionally, user can give descriptions for
    // trace tags which will be used in the trace tags dialog.
    // Any tags which don't have a description
    // will use the enum name in the dialog.

    [TagDesc("Loading")]
    Load,
}
#endregion

namespace DAL
{
    #region Dependency Injection Interface
    public interface IGData
    {
        string GetData();
    }
    #endregion

    public class GData : IGData
    {
        #region Private Variables
        private readonly INetTrace _netTrace;
        private readonly IConfiguration _config;
        #endregion

        #region Constructor
        public GData(INetTrace netTrace, IConfiguration config)
        {
            _netTrace = netTrace;
            _config = config;
        }
        #endregion

        #region IGData members
        public string GetData()
        {
            _netTrace.TraceDialog();
            _netTrace.Trace(t.Load, "Loading DAL...");
            return "Here's a string!";
        }
        #endregion
    }
}