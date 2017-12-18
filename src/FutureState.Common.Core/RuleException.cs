using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace FutureState
{
    /// <summary>
    ///     An exception that can be composed of one or more model state errors.
    /// </summary>
    [Serializable]
    public class RuleException : ApplicationException
    {
        /// <summary>
        ///     Creates a new instance with no rule errors.
        /// </summary>
        /// <param name="message">The error message.</param>
        public RuleException(string message) : this(message, new Error[0], null)
        {
        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="message">The main error message.</param>
        /// <param name="errors">The collection of errors.</param>
        /// <param name="innerException"></param>
        public RuleException(string message, IEnumerable<Error> errors, Exception innerException = null)
            : base(message, innerException)
        {
            Guard.ArgumentNotNullOrEmpty(message, nameof(message));
            Guard.ArgumentNotNull(errors, nameof(errors));

            Errors = errors.ToArray();
        }

        // required for serialization purposes
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        protected RuleException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            if (info != null)
                Errors = info.GetValue(nameof(Errors), typeof(Error[])) as Error[];
        }

        /// <summary>
        ///     Gets the list of business rules associated with the current instance.
        /// </summary>
        public Error[] Errors { get; }


        // deserialize
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            if (info != null)
                info.AddValue(nameof(Errors), Errors);
        }
    }
}