namespace FutureState.Data
{
    #region NoOpSessionFactory

    public class NoOpSessionFactory : ISessionFactory
    {
        public string Id { get; set; }

        public ISession Create()
        {
            return new NoOpSession();
        }

        private class NoOpSession : ISession
        {
            private NoOpTransaction _current;

            public bool IsOpen => true;

            public ITransaction BeginTran()
            {
                return _current = new NoOpTransaction();
            }

            public void Dispose()
            {
                _current = null;
            }

            public ITransaction GetCurrentTran()
            {
                return _current;
            }

            private class NoOpTransaction : ITransaction
            {
                internal NoOpTransaction()
                {
                    IsPending = true;
                }

                public bool IsPending { get; private set; }

                public void Commit()
                {
                }

                public void Dispose()
                {
                    IsPending = false;
                }

                public void Rollback()
                {
                }
            }
        }
    }

    #endregion NoOpSessionFactory
}