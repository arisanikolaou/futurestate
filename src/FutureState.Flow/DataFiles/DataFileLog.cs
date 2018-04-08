using System.Collections.Generic;

namespace FutureState.Flow
{
    /// <summary>
    ///     A log of consumable data source files with their addresses that provide the data 
    ///     required to material a given entity.
    /// </summary>
    /// <remarks>
    ///     Data file logs are used to query the available data source files available to consume
    ///     so that the system is not coupled to directory structure configurations and can
    ///     source data file from multiple different paths and servers.
    /// </remarks>
    public class DataFileLog
    {
        /// <summary>
        ///     Gets the log entries.
        /// </summary>
        public virtual List<DataFileLogEntry> Entries { get; set; }

        /// <summary>
        ///     Gets the entity type produced and consumable within the data source files.
        /// </summary>
        public FlowEntity EntityType { get; set; }

        /// <summary>
        ///     Gets the types of files included in the log e.g. csv or txt files. If null all files will be included in a given log.
        /// </summary>
        public string FileTypes { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public DataFileLog()
        {
            Entries = new List<DataFileLogEntry>();
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="entityType">The entity encoded in the data sources.</param>
        /// <param name="filePattern">The file pattern to use to scan for data source snapshot files.</param>
        public DataFileLog(FlowEntity entityType, string filePattern) : this()
        {
            Guard.ArgumentNotNull(entityType, nameof(entityType));

            Entries = new List<DataFileLogEntry>();
            EntityType = entityType;
            FileTypes = filePattern;
        }
    }
}