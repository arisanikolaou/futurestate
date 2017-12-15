namespace FutureState.Data
{
    public class InMemorySessionFactory : ISessionFactory
    {
        public string Id { get; set; }

        public ISession OpenSession()
        {
            return new InMemorySession();
        }

        public override string ToString()
        {
            return "MemSessionFactory";
        }
    }
}