namespace FutureState.Data
{
    //todo: revisit design

    #region NoOpSessionFactory

    public class NoOpSessionFactory : ISessionFactory
    {
        public string Id { get; set; }

        public ISession OpenSession()
        {
            return new NoOpSession();
        }

        public override string ToString()
        {
            return nameof(NoOpSessionFactory);
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

            public void Close()
            {
                Dispose();
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