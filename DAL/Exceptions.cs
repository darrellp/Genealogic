using NetTrace;
using static DAL.GData;

namespace DAL
{
    public class DataAccessException : Exception
    {
        public DataAccessException(string message) : base(message)
        {
            _netTrace.Trace(t.Exceptions, $"{message}");
        }
    }
}
