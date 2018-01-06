namespace FutureState.Data
{
    public class InMemorySessionFactory : ISessionFactory
    {
        public string Id { get; set; }

        public ISession Create()
        {
            return new InMemorySession();
        }

        public override string ToString()
        {
            return "MemSessionFactory";
        }
    }
}