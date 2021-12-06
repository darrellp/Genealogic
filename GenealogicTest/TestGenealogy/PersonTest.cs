using Microsoft.VisualStudio.TestTools.UnitTesting;
using GenealogicCore;

namespace TestGenealogy
{
    [TestClass]
    public class PersonTest
    {
        [TestMethod]
        public void TestPersonCreation()
        {
            var name = new Name("Darrell", "Alan", "Plank");
            var testPerson = new Person(name, new Addenda());
            Assert.IsNotNull(testPerson);
            Assert.AreEqual(name, testPerson.Name);
        }
    }
}