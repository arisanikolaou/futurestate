using System;

namespace FutureState.Flow
{
    /// <summary>
    ///     A data source log entry indicating a unique time and date that a file was produced.
    /// </summary>
    public class DataFileLogEntry
    {
        /// <summary>
        ///     Gets the date the source file was produced.
        /// </summary>
        public DateTime DateLastUpdated { get; set; }

        /// <summary>
        ///     The address of the data source. This is typically the full file path.
        /// </summary>
        public string AddressId { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public DataFileLogEntry()
        {
            // required by the serializer
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="addressId">The address of the data source e.g. the file path.</param>
        /// <param name="dateLastUpdated">The date the data source was last updated.</param>
        public DataFileLogEntry(string addressId, DateTime dateLastUpdated)
        {
            this.AddressId = addressId;
            this.DateLastUpdated = dateLastUpdated;
        }
    }
}