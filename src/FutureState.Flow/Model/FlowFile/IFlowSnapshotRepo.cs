namespace FutureState.Flow
{
    /// <summary>
    ///     Saves and gets a given process result to an underlying data store.
    /// </summary>
    /// <typeparam name="TProcessResult">The type of process result to save.</typeparam>
    public interface IFlowSnapshotRepo<TProcessResult> where TProcessResult : FlowSnapshot
    {
        /// <summary>
        ///     Gets/loads a process result from an underlying data store that matches a given process name, process id as well as
        ///     batch id.
        /// </summary>
        /// <returns></returns>
        TProcessResult Get(string processName, string flowCode, long batchId);

        /// <summary>
        ///     Gets the process results found on a given network file path or internet address.
        /// </summary>
        /// <returns></returns>
        TProcessResult Get(string dataSource);

        /// <summary>
        ///     Saves the process result.
        /// </summary>
        /// <param name="data">The data to save.</param>
        void Save(TProcessResult data);
    }
}