namespace GenealogicCore
{
    internal class Person
    {
        private Addenda _references;
        internal Name Name { get; init; }

        internal Person(Name name, Addenda references)
        {
            Name = name;
            _references = references;
        }
    }
}