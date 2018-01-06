namespace FutureState.Data
{
    /// <summary>
    ///     Returns an UpdateResult created after updating a given data structure.
    /// </summary>
    public class UpdateResult<T>
    {
        public T Object { get; set; }

        public UpdateResult Result { get; set; }
    }
}