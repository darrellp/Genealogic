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
        private const int CIndividuals = 10;
        private const string DbCreationName = @"c:\temp\dbCreationTest.glg";
        private const string DbPopulateName = @"c:\temp\dbPopulateTest.glg";

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

            var people = Builder<Person>.CreateListOfSize(CIndividuals)
                .All()
                .With(c => c.GivenName = Faker.Name.First())
                .With(c => c.Surname = Faker.Name.Last())
                .With(c => c.MiddleName = Faker.Name.Middle())
                .Build();
            Dal.CreateDBAt(DbPopulateName);
            // So we have at least one person we know in there
            people.Add(new Person() { GivenName = "Darrell", MiddleName = "Alan", Surname = "Plank" });
            Dal.WritePeople(people);
        }

        [TestMethod]
        public void TestDbCreation()
        {
            if (File.Exists(DbCreationName))
            {
                File.Delete(DbCreationName);
            }

            Dal.CreateDBAt(DbCreationName); 
            Assert.IsTrue(File.Exists(DbCreationName));
        }
    }
}
