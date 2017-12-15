#region

using System;
using System.Diagnostics;
using NLog;

#endregion

namespace FutureState.Diagnostics
{
    /// <summary>
    /// A base class to manage critical system operations via an 'Aspect Oriented Programming model' that
    /// can be configured at runtime.
    /// </summary>
    public abstract class ManagedActionBase : IManagedAction, IDisposable
    {
        protected static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        protected Action _action;

        private string _name;

        protected object _tag;

        /// <summary>
        /// Gets whether or not the action has been disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Gets/sets the name of the method being invoked
        /// </summary>
        /// <exception cref="ArgumentException">
        /// Raised if ActionName is assigned a null or empty value.
        /// </exception>
        public string Name
        {
            get { return _name; }

            set
            {
                Guard.ArgumentNotNull(value, nameof(Name));

                _name = value;
            }
        }

        /// <summary>
        /// Gets or sets a tag to associate to the action.
        /// </summary>
        public object Tag
        {
            get { return _tag; }

            set { _tag = value; }
        }

        /// <summary>
        /// Disposes the object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Invokes a unit of work and records the resources used to execute it.
        /// </summary>
        public void Invoke()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException("The action has been disposed: {0}".Params(Name));
            }

            if (_action == null)
            {
                throw new InvalidOperationException("The action cannot be executed until Setup has been called.");
            }

            // all good execute the implementation
            EndInvoke();
        }

        /// <summary>
        /// Sets up the action.
        /// </summary>
        /// <param name="action">Required. The action to invoke.</param>
        /// <param name="name">
        /// Optional. The name of the action being invoked. If empty the name of the calling method will be
        /// used.
        /// </param>
        /// <param name="tag">An optional tag to associate with the result.</param>
        /// <exception cref="ArgumentNullException">Raised if the action is null.</exception>
        public void Setup(Action action, string name = null, object tag = null)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            if (IsDisposed)
            {
                throw new ObjectDisposedException(GetType().Name);
            }

            _action = action;

            if (string.IsNullOrEmpty(name))
            {
                name = new StackTrace().GetFrame(1).GetMethod().Name; // this is expensive.
            }

            Name = name;
            _tag = tag;
        }

        protected virtual void Dispose(bool disposing)
        {
            _tag = null;
            _action = null;
            IsDisposed = true;
        }

        /// <summary>
        /// The implementation of the action to invoke.
        /// </summary>
        protected abstract void EndInvoke();
    }
}