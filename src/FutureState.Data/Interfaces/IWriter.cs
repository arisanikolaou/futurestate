using System.Collections.Generic;

namespace FutureState.Data
{
    public interface IWriter <in TEntity, in TKey> :
        IInserter<TEntity>,
        IUpdater<TEntity>,
        IDeleter<TEntity, TKey>,
        IInserter<IEnumerable<TEntity>>
    {
        //Defined at a repository level as repositories can read/write and therefore can get all and delete all as well.
        /// <summary>
        /// Deletes all entities present in the repository.
        /// </summary>
        void DeleteAll();
    }
}