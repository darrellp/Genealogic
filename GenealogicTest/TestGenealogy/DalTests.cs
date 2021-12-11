using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DAL.Models;
using FizzWare.NBuilder;
using static Genealogic.MainWindow;

namespace TestGenealogy
{
    [TestClass]
    public class DalTest
    {
        #region Private Variables
        private const int CIndividuals = 10;
        private const string DbCreationName = @"c:\temp\dbCreationTest.glg";
        private const string DbPopulateName = @"c:\temp\dbPopulateTest.glg";
        #endregion

        #region Initialization
        [ClassInitialize()]
        public static void ClassInit(TestContext context)
        {
            CreatePopulatedDb();
        }

        private static void CreatePopulatedDb()
        {
            if (File.Exists(DbPopulateName))
            {
                return;
            }

            // Allocate CIndividuals - 1 random people and then me at the end
            var people = Builder<Person>.CreateListOfSize(CIndividuals - 1)
                .All()
                .With(c => c.GivenName = Faker.Name.First())
                .With(c => c.Surname = Faker.Name.Last())
                .With(c => c.MiddleName = Faker.Name.Middle())
                .Build();
            Dal.UseDbAt(DbPopulateName);
            // So we have at least one person we know in there
            people.Add(new Person() { GivenName = "Darrell", MiddleName = "Alan", Surname = "Plank" });
            Dal.WritePeople(people);
        }
        #endregion

        #region Tests
        [TestMethod]
        public void TestDbCreation()
        {
            if (File.Exists(DbCreationName))
            {
                File.Delete(DbCreationName);
            }

            Dal.UseDbAt(DbCreationName); 
            Assert.IsTrue(File.Exists(DbCreationName));

            // I cannot close the database and delete it because File.Delete() claims
            // the test runner is still accessing it.  I'm not sure why. By
            // disposing of the connection I would think we're giving up the
            // only access we had on it.
            // 
            // Dal.CloseDb();
            // File.Delete(DbCreationName);
        }

        [TestMethod]
        public void TestGetPerson()
        {
            Dal.UseDbAt(DbPopulateName);
            var me = Dal.GetPerson(CIndividuals);
            Assert.AreEqual("Plank", me.Surname );
            Assert.AreEqual("Darrell", me.GivenName );
            Assert.AreEqual("Alan", me.MiddleName );
        }

        [TestMethod]
        public void TestWritePerson()
        {
            Dal.UseDbAt(DbPopulateName);
            var person = new Person() { GivenName = "Sara", MiddleName = "Drew", Surname = "Jackson" };
            Dal.WritePerson(person);
            var readPerson = Dal.GetPerson(person.Id);
            Assert.AreEqual("Jackson", readPerson.Surname);
            Assert.AreEqual("Sara", readPerson.GivenName);
            Assert.AreEqual("Drew", readPerson.MiddleName);

            Dal.DeletePerson(person.Id);
        }

        [TestMethod]
        public void TestCountPeople()
        {
            Dal.UseDbAt(DbPopulateName);
            Assert.AreEqual(CIndividuals, Dal.CountPeople());
        }
        #endregion
    }
}
