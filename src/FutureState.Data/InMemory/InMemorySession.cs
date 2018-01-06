namespace FutureState.Data
{
    /// <summary>
    ///     In memory implementation of ISession.
    /// </summary>
    public class InMemorySession : ISession
    {
        private InMemoryTran _tran;

        public bool IsOpen => false;

        public ITransaction BeginTran()
        {
            _tran = new InMemoryTran();

            return _tran;
        }

        public void Dispose()
        {
            _tran = null;
        }

        public ITransaction GetCurrentTran()
        {
            return _tran;
        }

        public void Close()
        {
        }
    }
}