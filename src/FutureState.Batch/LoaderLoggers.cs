using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FutureState.Batch
{


    public interface ILoaderErrorLogWriter
    {
        void Error(Exception ex);
        void Error(string message);
        void Error(Exception ex, string message);
    }

    public interface ILoaderWarningLogWriter
    {
        void Warn(string msg);
    }

    /// <summary>
    ///     Logs the errors/warnings and other events processing data from a given data store.
    /// </summary>
    public interface ILoaderLogWriter : ILoaderErrorLogWriter, ILoaderWarningLogWriter
    {
        void Info(string message);
    }

    /// <summary>
    ///     Logs errors/warnings and other events processing data from a given data store.
    /// </summary>
    public class LoaderLogWriter : ILoaderLogWriter
    {
        readonly Logger _logger;

        public LoaderLogWriter(Logger logger)
        {
            Guard.ArgumentNotNull(logger, nameof(logger));

            _logger = logger;
        }

        public void Error(Exception ex)
        {
            if (_logger.IsErrorEnabled)
                _logger.Error(ex);
        }

        public void Error(string message)
        {
            if (_logger.IsErrorEnabled)
                _logger.Error(message);
        }

        public void Error(Exception ex, string message)
        {
            if (_logger.IsErrorEnabled)
                _logger.Error(ex, message);
        }

        public void Info(string message)
        {
            if (_logger.IsInfoEnabled)
                _logger.Info(message);
        }

        public void Warn(string message)
        {
            if (_logger.IsWarnEnabled)
                _logger.Warn(message);
        }
    }

    /// <summary>
    ///     Logs loader events into a set of in memory datasets.
    /// </summary>
    public class LoaderLogWriterInMemory : ILoaderLogWriter
    {
        public List<String> Warnings { get; }

        public List<Exception> Errors { get; }

        public List<String> Messages { get; }

        public LoaderLogWriterInMemory()
        {
            Warnings = new List<string>();
            Messages = new List<string>();
            Errors = new List<Exception>();
        }

        public void Error(Exception ex)
        {
            Errors.Add(ex);
        }

        public void Warn(string msg)
        {
            Warnings.Add(msg);
        }

        public void Info(string message)
        {
            Messages.Add(message);
        }

        public void Error(Exception ex, string message)
        {
            Errors.Add(new Exception(message, ex));
        }

        public void Error(string message)
        {
            Errors.Add(new Exception(message));
        }
    }
}
