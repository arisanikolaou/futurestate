namespace FutureState.Data
{
    /// <summary>
    /// In memory implementation of <see cref="ITransaction"/>.
    /// </summary>
    public class InMemoryTran : ITransaction
    {
        public bool IsPending => true;

        public void Commit()
        {
        }

        public void Dispose()
        {
        }

        public void Rollback()
        {
        }
    }
}