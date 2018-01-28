namespace FutureState.Flow
{
    /// <summary>
    ///     An error encountered processing a given entity.
    /// </summary>
    /// <typeparam name="TEntityDto"></typeparam>
    public class ProcessError<TEntityDto>
    {
        /// <summary>
        ///     Gets the error message.
        /// </summary>
        public ErrorEvent Error { get; set; }

        public TEntityDto Item { get; set; }
    }
}