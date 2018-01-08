#region

//===============================================================================
// Microsoft patterns & practices
// Unity Application Block
//===============================================================================
// Copyright © Microsoft Corporation.  All rights reserved.
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
// OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
// FITNESS FOR A PARTICULAR PURPOSE.
//===============================================================================

using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime;
using System.Runtime.CompilerServices;

#endregion

namespace FutureState
{
    /// <summary>
    ///     A static helper class that includes various parameter checking routines.
    /// </summary>
    public static class Guard
    {
        /// <summary>
        ///     Throws <see cref="ArgumentNullException" /> if the given argument is null.
        /// </summary>
        /// <exception cref="ArgumentNullException"> if tested value if null.</exception>
        /// <param name="argumentValue">Argument value to test.</param>
        /// <param name="argumentName">Name of the argument being tested.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Critical to performance.")]
        [DebuggerNonUserCode]
        public static void ArgumentNotNull(
            object argumentValue,
            string argumentName)
        {
            if (argumentValue == null)
                throw new ArgumentNullException(argumentName);
        }

        /// <summary>
        ///     Throws an exception if the tested string argument is null or the empty string.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if string value is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the string is empty</exception>
        /// <param name="argumentValue">Argument value to check.</param>
        /// <param name="argumentName">Name of argument being checked.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Critical to performance.")]
        [DebuggerNonUserCode]
        public static void ArgumentNotNullOrEmpty(
            string argumentValue,
            string argumentName)
        {
            if (argumentValue == null)
                throw new ArgumentNullException(argumentName);

            if (argumentValue.Length == 0)
                throw new ArgumentException($"Argument '{argumentName}' cannot be empty.", argumentName);
        }

        /// <summary>
        ///     Throws an exception if the tested string argument is null or the empty string.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown if string value is null.</exception>
        /// <exception cref="ArgumentException">Thrown if the string is empty</exception>
        /// <param name="argumentValue">Argument value to check.</param>
        /// <param name="argumentName">Name of argument being checked.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Critical to performance.")]
        [DebuggerNonUserCode]
        public static void ArgumentNotNullOrEmptyOrWhiteSpace(
            string argumentValue,
            string argumentName)
        {
            if (argumentValue == null)
                throw new ArgumentNullException(argumentName);

            if (string.IsNullOrWhiteSpace(argumentValue))
                throw new ArgumentException($"Argument '{argumentName}' cannot be empty.", argumentName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Critical to performance.")]
        [DebuggerNonUserCode]
        public static void CheckForEmptyCollection(IEnumerable collection, string name)
        {
            ArgumentNotNull(collection, name);

            if (!collection.OfType<object>().Any())
                throw new ArgumentException("Collection is empty.", name);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Critical to performance.")]
        [DebuggerNonUserCode]
        public static void CheckForVoid(
            Expression<Func<object>> expression,
            string message = null,
            bool checkForWhiteSpace = true,
            bool checkForEmptyCollection = true)
        {
            var @object = expression.Compile()();

            // simple check
            if (@object == null)
            {
                message = message ?? new ArgumentNullException().Message;
                throw new ArgumentNullException(expression.GetParameterName(), message);
            }

            // string checking
            if (@object is string)
                if (checkForWhiteSpace
                    ? string.IsNullOrWhiteSpace((string) @object)
                    : string.IsNullOrEmpty((string) @object))
                {
                    message = message ?? "Null or white space string.";
                    throw new ArgumentException(message, expression.GetParameterName());
                }

            // collection checking
            if (checkForEmptyCollection && @object is IEnumerable && !(@object is string))
            {
                var enumerator = ((IEnumerable) @object).GetEnumerator();
                if (!enumerator.MoveNext())
                {
                    message = message ?? "Collection has no items.";
                    throw new ArgumentException(message, expression.GetParameterName());
                }
            }
        }

        /// <summary>
        ///     Throws an exception if a given condition is met.
        /// </summary>
        /// <param name="successCondition">The condition to evaluate.</param>
        /// <param name="exceptionGetter">Function constructing the exception to throw.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Critical to performance.")]
        [DebuggerNonUserCode]
        public static void Ensure(bool successCondition, Func<Exception> exceptionGetter)
        {
            if (!successCondition)
                throw exceptionGetter();
        }

        /// <summary>
        ///     Returns a validated not null instance of a given object or throws an argument null exception.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Critical to performance.")]
        [DebuggerNonUserCode]
        public static T GetNotNull<T>(T obj, string errorMessage = "Parameter cannot be null.")
            where T : class
        {
            if (obj == null)
                throw new ArgumentNullException(errorMessage);

            return obj;
        }

        /// <summary>
        ///     Fluently asserts an object is not null before raising a argument null exception.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [TargetedPatchingOptOut("Critical to performance.")]
        [DebuggerNonUserCode]
        public static T IsNotNull<T>(this T obj, string errorMessage = "Parameter cannot be null.")
            where T : class
        {
            if (obj == null)
                throw new ArgumentNullException(errorMessage);

            return obj;
        }

        public static void ArgumentInRange(int arg, int min, int max, string argumentName)
        {
            if (arg < min ||
                arg > max)
                throw new ArgumentOutOfRangeException(
                    $"Argument {argumentName}={arg} must be between {min} and {max}");
        }
    }
}