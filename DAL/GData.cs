using Microsoft.Extensions.Configuration;
using NetTrace;
using System.Data.SQLite;
using DAL.Models;
using Dapper;

#region Trace Tag enums
[TraceTags,
 EnumDesc("Tags in DAL")]
internal enum t
{
    [TagDesc("Loading")]
    Load,
    [TagDesc("Exception creation")]
    Exceptions,
}
#endregion

namespace DAL
{
    #region Dependency Injection Interface
    public interface IGData
    {
        bool UseDBAt(string filename);
        bool WritePerson(Models.Person person);
        Person GetPerson(int id);
        bool DeletePerson(int id);
        bool WritePeople(IEnumerable<Models.Person> people);
    }
    #endregion

    public class GData : IGData
    {
        #region Private Variables
        public static INetTrace _netTrace;
        private readonly IConfiguration _config;
        private SQLiteConnection? _dbConnection;
        #endregion

        #region Constructors
        public GData(INetTrace netTrace, IConfiguration config)
        {
            _netTrace = netTrace;
            _config = config;
        }
        #endregion

        #region Event Handlers

        #endregion

        #region IGData members
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Creates database at a file location if it doesn't already exist. </summary>
        ///
        /// <remarks>   Darrell Plank, 12/9/2021. </remarks>
        ///
        /// <param name="filename"> The file to create. </param>
        ///
        /// <returns>   True if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool UseDBAt(string filename)
        {
            var filePreExisted = File.Exists(filename);
            if (_dbConnection == null)
            {
                AppDomain.CurrentDomain.ProcessExit += (s, e) => _dbConnection?.Dispose();
            }
            else
            {
                _dbConnection.Dispose();
            }

            var strConnection = $"Data Source={filename};";
            _dbConnection = new SQLiteConnection(strConnection);
            _dbConnection.Open();
            if (filePreExisted)
            {
                return true;
            }

            var command = _dbConnection.CreateCommand();

            command.CommandText =
                @"CREATE TABLE Individuals (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Surname TEXT,
                            GivenName TEXT,
                            MiddleName TEXT,
                            Birth REAL,
                            Death REAL
                        );";
            command.ExecuteNonQuery();

            return true;
        }

        public bool WritePerson(Person person)
        {
            if (_dbConnection == null)
            {
                throw new DataAccessException("Writing without opening a file");
            }
            const string sql = "INSERT INTO Individuals (Surname, GivenName, MiddleName) VALUES (@Surname, @GivenName, @MiddleName);";
            _dbConnection.Execute(sql, person);
            return true;
        }

        public Person GetPerson(int id)
        {
            if (_dbConnection == null)
            {
                throw new DataAccessException("Writing without opening a file");
            }
            return _dbConnection.Query<Person>($"Select * from Individuals where Id = '{id}'").First();
        }

        public bool DeletePerson(int id)
        {
            throw new NotImplementedException();
        }

        public bool WritePeople(IEnumerable<Person> people)
        {
            if (_dbConnection == null)
            {
                throw new DataAccessException("Writing without opening a file");
            }
            const string sql = "INSERT INTO Individuals (Surname, GivenName, MiddleName) VALUES (@Surname, @GivenName, @MiddleName);";
            _dbConnection.Execute(sql, people);
            return true;
        }
        #endregion
    }
}