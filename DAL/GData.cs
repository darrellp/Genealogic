using Microsoft.Extensions.Configuration;
using NetTrace;
using System.Data.SQLite;
using DAL.Models;
using Dapper;

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
        private string _connection;
        #endregion

        #region Constructor
        public GData(INetTrace netTrace, IConfiguration config)
        {
            _netTrace = netTrace;
            _config = config;
            _connection = string.Empty;
        }
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
            _connection = $"Data Source={filename};";
            if (System.IO.File.Exists(filename))
            {
                return true;
            }

            using var connection = new SQLiteConnection(_connection);
            connection.Open();  //  <== The database file is created here.
            var command = connection.CreateCommand();

            command.CommandText = 
                @"CREATE TABLE Individuals (
                            Id INT,
                            Surname TEXT,
                            Given TEXT,
                            Middle TEXT,
                            Birth REAL,
                            Death REAL
                        )";
            command.ExecuteNonQuery();

            return true;
        }

        public bool WritePerson(Person person)
        {
            if (_connection == string.Empty)
            {
                throw new DataAccessException("Writing without opening a file");
            }
            using var connection = new SQLiteConnection(_connection);
            connection.Open();
            const string sql = "INSERT INTO Individuals (Surname, Given, Middle) Values (@Surname, @Given, @Middle);";

            connection.Execute(sql, person);
            return true;
        }

        public Person GetPerson(int id)
        {
            using var connection = new SQLiteConnection(_connection);
            connection.Open();
            return connection.Query<Person>($"Select * from Individuals where Id = '{id}'").First();
        }

        public bool DeletePerson(int id)
        {
            throw new NotImplementedException();
        }

        public bool WritePeople(IEnumerable<Person> people)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}