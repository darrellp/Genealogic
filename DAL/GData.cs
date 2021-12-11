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
    ////////////////////////////////////////////////////////////////////////////////////////////////////
    /// <summary>   Interface for the data access layer. </summary>
    ///
    /// <remarks>   Darrell Plank, 12/11/2021. </remarks>
    ////////////////////////////////////////////////////////////////////////////////////////////////////

    public interface IGData
    {
        bool UseDbAt(string filename);
        bool WritePerson(Models.Person person);
        Person GetPerson(long id);
        bool DeletePerson(long id);
        bool WritePeople(IEnumerable<Models.Person> people);
        void CloseDb();
        long CountPeople();
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
            AppDomain.CurrentDomain.ProcessExit += (s, e) => _dbConnection?.Dispose();
        }
        #endregion

        #region IGData members
        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Creates and uses database at a file location if it doesn't already exist. </summary>
        ///
        /// <remarks>   Darrell Plank, 12/9/2021. </remarks>
        ///
        /// <param name="filename"> The file to create/use. </param>
        ///
        /// <returns>   True if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool UseDbAt(string filename)
        {
            var filePreExisted = File.Exists(filename);
            if (_dbConnection != null)
            {
                CloseDb();
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

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Closes the database. </summary>
        ///
        /// <remarks>   Darrell Plank, 12/11/2021. </remarks>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public void CloseDb()
        {
            _dbConnection?.Dispose();
            _dbConnection = null;
        }

        private void CheckDb()
        {
            if (_dbConnection == null)
            {
                throw new DataAccessException("Accessing database without opening a file");
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Writes a person into the DB. </summary>
        ///
        /// <remarks>   Darrell Plank, 12/11/2021. </remarks>
        ///
        /// <param name="person">   The person. </param>
        ///
        /// <returns>   True if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool WritePerson(Person person)
        {
            CheckDb();
            const string sql = @"INSERT INTO Individuals
                                    (Surname, GivenName, MiddleName) VALUES 
                                    (@Surname, @GivenName, @MiddleName);
                                    select last_insert_rowid();";
            person.Id = _dbConnection.Query<long>(sql, person).First();
            return true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Gets a person from the Db. </summary>
        ///
        /// <remarks>   Darrell Plank, 12/11/2021. </remarks>
        ///
        /// <param name="id">   The identifier for the person. </param>
        ///
        /// <returns>   The person. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public Person GetPerson(long id)
        {
            CheckDb();
            return _dbConnection.Query<Person>($"Select * from Individuals where Id = '{id}'").First();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Deletes the person described by ID. </summary>
        ///
        /// <remarks>   Darrell Plank, 12/11/2021. </remarks>
        ///
        /// <param name="id">   The identifier for the person. </param>
        ///
        /// <returns>   True if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool DeletePerson(long id)
        {
            CheckDb();
            var sql = $"DELETE FROM Individuals WHERE Id='{id}'";
            _dbConnection.Execute(sql);
            return true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Writes a list of people to the DB. </summary>
        ///
        /// <remarks>   Darrell Plank, 12/11/2021. </remarks>
        ///
        /// <param name="people">   The people to write. </param>
        ///
        /// <returns>   True if it succeeds, false if it fails. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool WritePeople(IEnumerable<Person> people)
        {
            CheckDb();
            const string sql = "INSERT INTO Individuals (Surname, GivenName, MiddleName) VALUES (@Surname, @GivenName, @MiddleName);";
            _dbConnection.Execute(sql, people);
            return true;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>   Count the number of people in the DB. </summary>
        ///
        /// <remarks>   Darrell Plank, 12/11/2021. </remarks>
        ///
        /// <returns>   The total number of people. </returns>
        ////////////////////////////////////////////////////////////////////////////////////////////////////

        public long CountPeople()
        {
            const string sql = "SELECT COUNT(Id) FROM Individuals;";
            return _dbConnection.Query<long>(sql).First();
        }
        #endregion
    }
}