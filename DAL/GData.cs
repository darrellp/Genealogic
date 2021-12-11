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
        bool CreateDBAt(string filename);
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
        private string _strConnection;
        private SQLiteConnection? _dbConnection;
        #endregion

        #region Constructors
        public GData(INetTrace netTrace, IConfiguration config)
        {
            _netTrace = netTrace;
            _config = config;
            _strConnection = string.Empty;
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

        public bool CreateDBAt(string filename)
        {
            if (_dbConnection == null)
            {
                AppDomain.CurrentDomain.ProcessExit += (s, e) => _dbConnection?.Dispose();
            }
            else
            {
                _dbConnection.Dispose();
            }

            _strConnection = $"Data Source={filename};";
            if (File.Exists(filename))
            {
                return true;
            }

            _dbConnection = new SQLiteConnection(_strConnection);
            _dbConnection.Open();  //  <== The database file is created here.
            var command = _dbConnection.CreateCommand();

            command.CommandText =
                @"CREATE TABLE Individuals (
                            Surname TEXT,
                            Given TEXT,
                            Middle TEXT,
                            Birth REAL,
                            Death REAL
                        );
                ALTER TABLE Individuals ADD Id INT IDENTITY(1,1)";
            command.ExecuteNonQuery();

            return true;
        }

        public bool WritePerson(Person person)
        {
            if (_dbConnection == null)
            {
                throw new DataAccessException("Writing without opening a file");
            }
            const string sql = "INSERT INTO Individuals (Surname, Given, Middle) VALUES (@Surname, @GivenName, @MiddleName);";
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
            const string sql = "INSERT INTO Individuals (Surname, Given, Middle) VALUES (@Surname, @GivenName, @MiddleName);";
            _dbConnection.Execute(sql, people);
            return true;
        }
        #endregion
    }
}