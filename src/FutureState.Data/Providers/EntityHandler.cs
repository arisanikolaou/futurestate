namespace FutureState.Data.Providers
{
    /// <summary>
    ///     Helps process an entity through a pipe/filter.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class EntityHandler<TEntity,TKey>
    {
        public EntityHandler(
            IActiveHandler<TEntity> activateHandler,
            IAddHandler<TEntity> addHandler,
            IRemoveHandler<TKey> removeHandler)
        {
            this.ActivateHandler = activateHandler;
            this.AddHandler = addHandler;
            this.RemoveHandler = removeHandler;
        }

        public IAddHandler<TEntity> AddHandler { get; private set; }
        public IRemoveHandler<TKey> RemoveHandler { get; private set; }
        public IActiveHandler<TEntity> ActivateHandler { get; private set; }
    }

    public interface IRemoveHandler<TKey>
    {
        void Handle(TKey entity);
    }

    public interface IAddHandler<TEntity>
    {
        void Handle(TEntity entity);
    }

    public interface IActiveHandler<TEntity>
    {
        void Handle(TEntity entity);
    }
}
