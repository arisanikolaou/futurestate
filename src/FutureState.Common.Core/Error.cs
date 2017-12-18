using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace FutureState
{
    /// <summary>
    ///     Base class used to communicate the error details in a given generic <see cref="FsException" />
    ///     class.
    /// </summary>
    [Serializable]
    public class Error : IExceptionData, ISerializable
    {
        private string _category;

        private string _message;

        private string _type;

        /// <summary>
        ///     Creates a new error object
        /// </summary>
        /// <param name="category">
        ///     Optional. Gets the category.
        /// </param>
        /// <param name="type">
        ///     Optional. Gets the type of error.
        /// </param>
        /// <param name="message">
        ///     Required. Gets the error message;
        /// </param>
        public Error(string message, string type = "", string category = "")
        {
            Guard.ArgumentNotNullOrEmpty(message, nameof(message));

            _category = category;
            _message = message;
            _type = type;
        }

        private Error(SerializationInfo si, StreamingContext ctx)
        {
            _type = si.GetString(nameof(Type));
            _message = si.GetString(nameof(Message));
            _category = si.GetString(nameof(Category));
        }

        /// <summary>
        ///     Gets the error category.
        /// </summary>
        public string Category
        {
            get => _category;

            protected set => _category = value;
            // required for jsv serialization support
        }

        /// <summary>
        ///     Gets the error type.
        /// </summary>
        public string Type
        {
            get => _type;

            protected set => _type = value;
            // required for jsv serialization support
        }

        /// <summary>
        ///     Gets the error message.
        /// </summary>
        public string Message
        {
            get => _message;

            protected set => _message = value;
            // required for jsv serialization support
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext ctx)
        {
            info.AddValue(nameof(Type), Type);
            info.AddValue(nameof(Message), Message);
            info.AddValue(nameof(Category), Category);
        }

        /// <summary>
        ///     Gets the category and error message.
        /// </summary>
        public override string ToString()
        {
            return $"{Category} {Message}";
        }
    }
}