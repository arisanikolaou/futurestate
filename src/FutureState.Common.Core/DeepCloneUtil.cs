#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

#endregion

namespace FutureState
{
    /// <summary>
    ///     NOTE: this is originally from http://blog.nuclex-games.com/mono-dotnet/fast-deep-cloning/ with some changes.
    ///     Fast deep clone utility using expression trees.
    /// </summary>
    public static class DeepCloneUtil
    {
        /// <summary>Compiled cloners that perform deep clone operations</summary>
        private static readonly ConcurrentDictionary<Type, Func<object, Dictionary<object, object>, object>>
            _typeCloners =
                new ConcurrentDictionary<Type, Func<object, Dictionary<object, object>, object>>();

        /// <summary>
        ///     Creates a deep clone of the specified object, also creating clones of all child objects being referenced
        /// </summary>
        /// <typeparam name="TCloned">Type of the object that will be cloned</typeparam>
        /// <param name="objectToClone">Object that will be cloned</param>
        /// <returns>A deep clone of the provided object</returns>
        public static TCloned DeepFieldClone<TCloned>(TCloned objectToClone)
        {
            var creator = GetTypeCloner(typeof(TCloned));
            return (TCloned) creator(objectToClone, new Dictionary<object, object>());
        }

        /// <summary>
        ///     Retrieves the existing clone method for the specified type or compiles one if none exists for the type yet
        /// </summary>
        /// <param name="clonedType">Type for which a clone method will be retrieved</param>
        /// <returns>The clone method for the specified type</returns>
        private static Func<object, Dictionary<object, object>, object> GetTypeCloner(Type clonedType)
        {
            return _typeCloners.GetOrAdd(clonedType, type => new CloneExpressionBuilder(type).CreateTypeCloner());
        }

        private static class CloneExpressionHelper
        {
            private static readonly MethodInfo _dictionaryContainsKey =
                typeof(Dictionary<object, object>).GetMethod("ContainsKey");

            private static readonly MethodInfo _dictionaryGetItem =
                typeof(Dictionary<object, object>).GetMethod("get_Item");

            private static readonly MethodInfo _fieldInfoSetValueMethod = typeof(FieldInfo).GetMethod("SetValue",
                new[] {typeof(object), typeof(object)});

            private static readonly MethodInfo _getTypeClonerMethodInfo =
                typeof(DeepCloneUtil).GetMethod(nameof(GetTypeCloner), BindingFlags.NonPublic | BindingFlags.Static);

            private static readonly MethodInfo _getTypeMethodInfo = typeof(object).GetMethod("GetType");

            private static readonly MethodInfo _invokeMethodInfo =
                typeof(Func<object, Dictionary<object, object>, object>).GetMethod("Invoke");

            /// <summary>
            ///     Creates an expression that copies a coplex array value from the source to the target.
            ///     The value will be cloned as well using the dictionary to reuse already cloned objects.
            /// </summary>
            /// <param name="sourceField">The source field.</param>
            /// <param name="targetField">The target field.</param>
            /// <param name="type">The type.</param>
            /// <param name="objectDictionary">The object dictionary.</param>
            /// <returns>The expression.</returns>
            internal static Expression CreateCopyComplexArrayTypeFieldExpression(Expression sourceField,
                Expression targetField, Type type, Expression objectDictionary)
            {
                return Expression.IfThenElse(
                    Expression.Call(objectDictionary, _dictionaryContainsKey, sourceField),
                    Expression.Assign(targetField,
                        Expression.Convert(Expression.Call(objectDictionary, _dictionaryGetItem, sourceField), type)),
                    Expression.Assign(
                        targetField,
                        Expression.Convert(
                            Expression.Call(
                                Expression.Call(
                                    _getTypeClonerMethodInfo,
                                    Expression.Call(sourceField, _getTypeMethodInfo)),
                                _invokeMethodInfo,
                                sourceField,
                                objectDictionary),
                            type)));
            }

            /// <summary>
            ///     Creates an expression that copies a coplex value from the source to the target.
            ///     The value will be cloned as well using the dictionary to reuse already cloned objects.
            /// </summary>
            /// <param name="original">The original.</param>
            /// <param name="clone">The clone.</param>
            /// <param name="fieldInfo">The field information.</param>
            /// <param name="objectDictionary">The object dictionary.</param>
            /// <returns></returns>
            internal static Expression CreateCopyComplexFieldExpression(Expression original, Expression clone,
                FieldInfo fieldInfo, ParameterExpression objectDictionary)
            {
                Expression originalField = Expression.Field(original, fieldInfo);

                return Expression.IfThenElse(
                    Expression.Call(objectDictionary, _dictionaryContainsKey, originalField),
                    CreateSetFieldExpression(
                        clone,
                        Expression.Convert(Expression.Call(objectDictionary, _dictionaryGetItem, originalField),
                            fieldInfo.FieldType),
                        fieldInfo),
                    CreateSetFieldExpression(
                        clone,
                        Expression.Convert(
                            Expression.Call(
                                Expression.Call(
                                    _getTypeClonerMethodInfo,
                                    Expression.Call(originalField, _getTypeMethodInfo)),
                                _invokeMethodInfo,
                                originalField,
                                objectDictionary),
                            fieldInfo.FieldType),
                        fieldInfo));
            }

            /// <summary>
            ///     Creates an expression that copies a value from the original to the clone.
            /// </summary>
            /// <param name="original">The original.</param>
            /// <param name="clone">The clone.</param>
            /// <param name="fieldInfo">The field info.</param>
            /// <returns>The expression that copies a value from the original to the clone.</returns>
            internal static Expression CreateCopyFieldExpression(Expression original, Expression clone,
                FieldInfo fieldInfo)
            {
                return CreateSetFieldExpression(clone, Expression.Field(original, fieldInfo), fieldInfo);
            }

            internal static Expression CreateSetFieldExpression(Expression clone, Expression value, FieldInfo fieldInfo)
            {
                // workaround for readonly fields: use reflection, this is a lot slower but the only way except using il directly
                if (fieldInfo.IsInitOnly)
                    return Expression.Call(Expression.Constant(fieldInfo), _fieldInfoSetValueMethod, clone,
                        Expression.Convert(value, typeof(object)));

                return Expression.Assign(Expression.Field(clone, fieldInfo), value);
            }
        }

        private class CloneExpressionBuilder
        {
            private static readonly MethodInfo _arrayCloneMethodInfo = typeof(Array).GetMethod("Clone");

            private static readonly MethodInfo _arrayGetLengthMethodInfo = typeof(Array).GetMethod("GetLength");

            private static readonly MethodInfo _dictionaryAddMethodInfo =
                typeof(Dictionary<object, object>).GetMethod("Add");

            private static readonly MethodInfo _getUninitializedObjectMethodInfo = typeof(FormatterServices).GetMethod(
                "GetUninitializedObject",
                BindingFlags.Static | BindingFlags.Public);

            private readonly List<Expression> _expressions = new List<Expression>();

            private readonly ParameterExpression _objectDictionary =
                Expression.Parameter(typeof(Dictionary<object, object>), "objectDictionary");

            private readonly ParameterExpression _original = Expression.Parameter(typeof(object), "original");

            private readonly Type _type;

            private readonly List<ParameterExpression> _variables = new List<ParameterExpression>();

            private ParameterExpression _clone;

            private ParameterExpression _typedOriginal;

            internal CloneExpressionBuilder(Type type)
            {
                _type = type;
            }

            internal Func<object, Dictionary<object, object>, object> CreateTypeCloner()
            {
                Expression resultExpression;

                if (IsTypePrimitiveOrString(_type))
                {
                    _expressions.Add(_original);
                    resultExpression = _expressions[0];
                }
                else
                {
                    _expressions.Add(_objectDictionary);

                    // To access the fields of the original type, we need it to be of the actual type instead of an object, so perform a downcast
                    _typedOriginal = Expression.Variable(_type);
                    _variables.Add(_typedOriginal);
                    _expressions.Add(Expression.Assign(_typedOriginal, Expression.Convert(_original, _type)));

                    if (_type.IsArray)
                        CloneArray();
                    else
                        CloneObject();

                    resultExpression = Expression.Block(_variables, _expressions);
                }

                if (_type.IsValueType)
                    resultExpression = Expression.Convert(resultExpression, typeof(object));

                return
                    Expression.Lambda<Func<object, Dictionary<object, object>, object>>(resultExpression, _original,
                        _objectDictionary).Compile();
            }

            /// <summary>
            ///     Generates state transfer expressions to copy an array of primitive types
            /// </summary>
            /// <param name="elementType">Type of array that will be cloned</param>
            /// <param name="source">Variable expression for the original array</param>
            /// <returns>The variable holding the cloned array</returns>
            private static Expression GenerateFieldBasedPrimitiveArrayTransferExpressions(Type elementType,
                Expression source)
            {
                return
                    Expression.Convert(
                        Expression.Call(Expression.Convert(source, typeof(Array)), _arrayCloneMethodInfo), elementType);
            }

            /// <summary>
            ///     Returns all the fields of a type, working around a weird reflection issue
            ///     where explicitly declared fields in base classes are returned, but not
            ///     automatic property backing fields.
            /// </summary>
            /// <param name="type">Type whose fields will be returned</param>
            /// <param name="bindingFlags">Binding flags to use when querying the fields</param>
            /// <param name="skipCloneFields">The fields to skip deep cloning.</param>
            /// <returns>
            ///     All of the type's fields, including its base types
            /// </returns>
            private static IEnumerable<FieldInfo> GetFieldInfosIncludingBaseClasses(Type type,
                BindingFlags bindingFlags,
                out ISet<FieldInfo> skipCloneFields)
            {
                skipCloneFields =
                    new HashSet<FieldInfo>(
                        new LambdaComparer<FieldInfo>(
                            (field1, field2) =>
                                field1.DeclaringType == field2.DeclaringType && field1.Name == field2.Name));
                var fieldsToClone =
                    new HashSet<FieldInfo>(
                        new LambdaComparer<FieldInfo>(
                            (field1, field2) =>
                                field1.DeclaringType == field2.DeclaringType && field1.Name == field2.Name));

                while (type != typeof(object) && type != null)
                {
                    var fields = type.GetFields(bindingFlags);
                    foreach (var field in fields)
                    {
                        if (field.IsDefined(typeof(SkipDeepCloneAttribute), true))
                        {
                            skipCloneFields.Add(field);
                            continue;
                        }
                        if (field.Name.StartsWith("<") && field.Name.EndsWith(">k__BackingField"))
                        {
                            var propertyInfo =
                                type.GetProperty(field.Name.Substring(1,
                                    field.Name.IndexOf(">k__BackingField", StringComparison.Ordinal) - 1));
                            if (propertyInfo != null)
                                if (propertyInfo.IsDefined(typeof(SkipDeepCloneAttribute), true))
                                {
                                    skipCloneFields.Add(field);
                                    continue;
                                }
                        }

                        fieldsToClone.Add(field);
                    }

                    type = type.BaseType;
                }

                return fieldsToClone;
            }

            /// <summary>
            ///     Determines whether the specified type is primitive or a string.
            /// </summary>
            /// <param name="type">The type to check.</param>
            /// <returns><c>true</c> if the type is primitive of a string; otherwise <c>false</c>.</returns>
            private static bool IsTypePrimitiveOrString(Type type)
            {
                return type.IsPrimitive || type == typeof(string);
            }

            /// <summary>
            ///     Clones the array.
            /// </summary>
            private void CloneArray()
            {
                // Arrays need to be cloned element-by-element
                var elementType = _type.GetElementType();

                _expressions.Add(
                    IsTypePrimitiveOrString(elementType)
                        ? GenerateFieldBasedPrimitiveArrayTransferExpressions(_type, _original)
                        : GenerateFieldBasedComplexArrayTransferExpressions(_type, elementType, _typedOriginal,
                            _variables, _expressions));
            }

            /// <summary>
            ///     Clones the object.
            /// </summary>
            private void CloneObject()
            {
                // We need a variable to hold the clone because due to the assignments it won't be last in the block when we're finished
                _clone = Expression.Variable(_type);
                _variables.Add(_clone);

                _expressions.Add(
                    Expression.Block(
                        Expression.Assign(
                            _clone,
                            Expression.Convert(
                                Expression.Call(_getUninitializedObjectMethodInfo, Expression.Constant(_type)), _type)),
                        // create new instance and add to objectDictionary
                        Expression.Call(_objectDictionary, _dictionaryAddMethodInfo, _original,
                            Expression.Convert(_clone, typeof(object)))));

                // Generate the expressions required to transfer the type field by field
                GenerateFieldBasedComplexTypeTransferExpressions(_type, _typedOriginal, _clone, _expressions);

                // Make sure the clone is the last thing in the block to set the return value
                _expressions.Add(_clone);
            }

            /// <summary>
            ///     Generates state transfer expressions to copy an array of complex types
            /// </summary>
            /// <param name="arrayType">Type of array that will be cloned</param>
            /// <param name="elementType">Type of the elements of the array</param>
            /// <param name="originalArray">Variable expression for the original array</param>
            /// <param name="arrayVariables">Receives variables used by the transfer expressions</param>
            /// <param name="arrayExpressions">Receives the generated transfer expressions</param>
            /// <returns>The variable holding the cloned array</returns>
            private ParameterExpression GenerateFieldBasedComplexArrayTransferExpressions(
                Type arrayType,
                Type elementType,
                Expression originalArray,
                ICollection<ParameterExpression> arrayVariables,
                ICollection<Expression> arrayExpressions)
            {
                // We need a temporary variable in order to transfer the elements of the array
                var arrayClone = Expression.Variable(arrayType);
                arrayVariables.Add(arrayClone);

                var dimensionCount = arrayType.GetArrayRank();

                var lengths = new List<ParameterExpression>();
                var indexes = new List<ParameterExpression>();
                var labels = new List<LabelTarget>();

                // Retrieve the length of each of the array's dimensions
                for (var index = 0; index < dimensionCount; ++index)
                {
                    // Obtain the length of the array in the current dimension
                    lengths.Add(Expression.Variable(typeof(int)));
                    arrayVariables.Add(lengths[index]);
                    arrayExpressions.Add(Expression.Assign(lengths[index],
                        Expression.Call(originalArray, _arrayGetLengthMethodInfo, Expression.Constant(index))));

                    // Set up a variable to index the array in this dimension
                    indexes.Add(Expression.Variable(typeof(int)));
                    arrayVariables.Add(indexes[index]);

                    // Also set up a label than can be used to break out of the dimension's transfer loop
                    labels.Add(Expression.Label());
                }

                // Create a new (empty) array with the same dimensions and lengths as the original
                arrayExpressions.Add(Expression.Assign(arrayClone, Expression.NewArrayBounds(elementType, lengths)));

                // Initialize the indexer of the outer loop (indexers are initialized one up
                // in the loops (ie. before the loop using it begins), so we have to set this
                // one outside of the loop building code.
                arrayExpressions.Add(Expression.Assign(indexes[0], Expression.Constant(0)));

                // Build the nested loops (one for each dimension) from the inside out
                Expression innerLoop = null;
                for (var index = dimensionCount - 1; index >= 0; --index)
                {
                    var loopVariables = new List<ParameterExpression>();
                    var loopExpressions = new List<Expression>
                    {
                        Expression.IfThen(Expression.GreaterThanOrEqual(indexes[index], lengths[index]),
                            Expression.Break(labels[index]))
                    };

                    // If we reached the end of the current array dimension, break the loop
                    if (innerLoop == null)
                    {
                        // The innermost loop clones an actual array element
                        if (IsTypePrimitiveOrString(elementType))
                        {
                            loopExpressions.Add(Expression.Assign(Expression.ArrayAccess(arrayClone, indexes),
                                Expression.ArrayAccess(originalArray, indexes)));
                        }
                        else if (elementType.IsValueType)
                        {
                            GenerateFieldBasedComplexTypeTransferExpressions(
                                elementType,
                                Expression.ArrayAccess(originalArray, indexes),
                                Expression.ArrayAccess(arrayClone, indexes),
                                loopExpressions);
                        }
                        else
                        {
                            var nestedVariables = new List<ParameterExpression>();
                            var nestedExpressions = new List<Expression>();

                            // A nested array should be cloned by directly creating a new array (not invoking a cloner) since you cannot derive from an array
                            if (elementType.IsArray)
                            {
                                var nestedElementType = elementType.GetElementType();
                                var clonedElement = IsTypePrimitiveOrString(nestedElementType)
                                    ? GenerateFieldBasedPrimitiveArrayTransferExpressions(elementType,
                                        Expression.ArrayAccess(originalArray, indexes))
                                    : GenerateFieldBasedComplexArrayTransferExpressions(
                                        elementType,
                                        nestedElementType,
                                        Expression.ArrayAccess(originalArray, indexes),
                                        nestedVariables,
                                        nestedExpressions);

                                nestedExpressions.Add(Expression.Assign(Expression.ArrayAccess(arrayClone, indexes),
                                    clonedElement));
                            }
                            else
                            {
                                nestedExpressions.Add(
                                    CloneExpressionHelper.CreateCopyComplexArrayTypeFieldExpression(
                                        Expression.ArrayAccess(originalArray, indexes),
                                        Expression.ArrayAccess(arrayClone, indexes),
                                        elementType,
                                        _objectDictionary));
                            }

                            // Whether array-in-array of reference-type-in-array, we need a null check before // doing anything to avoid NullReferenceExceptions for unset members
                            loopExpressions.Add(
                                Expression.IfThen(
                                    Expression.NotEqual(Expression.ArrayAccess(originalArray, indexes),
                                        Expression.Constant(null)),
                                    Expression.Block(nestedVariables, nestedExpressions)));
                        }
                    }
                    else
                    {
                        // Outer loops of any level just reset the inner loop's indexer and execute the inner loop
                        loopExpressions.Add(Expression.Assign(indexes[index + 1], Expression.Constant(0)));
                        loopExpressions.Add(innerLoop);
                    }

                    // Each time we executed the loop instructions, increment the indexer
                    loopExpressions.Add(Expression.PreIncrementAssign(indexes[index]));

                    // Build the loop using the expressions recorded above
                    innerLoop = Expression.Loop(Expression.Block(loopVariables, loopExpressions), labels[index]);
                }

                // After the loop builder has finished, the innerLoop variable contains the entire hierarchy of nested loops, so add this to the clone expressions.
                arrayExpressions.Add(innerLoop);

                return arrayClone;
            }

            /// <summary>
            ///     Generates state transfer expressions to copy a complex type
            /// </summary>
            /// <param name="complexType">Complex type that will be cloned</param>
            /// <param name="source">Variable expression for the original instance</param>
            /// <param name="target">Variable expression for the cloned instance</param>
            /// <param name="expression">Receives the generated transfer expressions</param>
            private void GenerateFieldBasedComplexTypeTransferExpressions(Type complexType, Expression source,
                Expression target, ICollection<Expression> expression)
            {
                // Enumerate all of the type's fields and generate transfer expressions for each
                ISet<FieldInfo> skipCloneFieldInfos;
                var fieldInfos = GetFieldInfosIncludingBaseClasses(
                    complexType,
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    out skipCloneFieldInfos);

                // For those field which skip deep copying, do shallow copying by assigning the field value from the source to the target
                foreach (var fieldInfo in skipCloneFieldInfos)
                    expression.Add(CloneExpressionHelper.CreateCopyFieldExpression(source, target, fieldInfo));

                foreach (var fieldInfo in fieldInfos)
                {
                    var fieldType = fieldInfo.FieldType;
                    if (fieldType.Equals(typeof(DataTable)))
                        expression.Add(CloneExpressionHelper.CreateCopyFieldExpression(source, target, fieldInfo));
                    else if (IsTypePrimitiveOrString(fieldType))
                        expression.Add(CloneExpressionHelper.CreateCopyFieldExpression(source, target, fieldInfo));
                    else if (fieldType.IsValueType)
                        GenerateFieldBasedComplexTypeTransferExpressions(fieldType, Expression.Field(source, fieldInfo),
                            Expression.Field(target, fieldInfo), expression);
                    else
                        GenerateFieldBasedReferenceTypeTransferExpressions(source, target, expression, fieldInfo);
                }
            }

            /// <summary>
            ///     Generates the expressions to transfer a reference type (array or class)
            /// </summary>
            /// <param name="original">Original value that will be cloned</param>
            /// <param name="clone">Variable that will receive the cloned value</param>
            /// <param name="expressions">
            ///     Receives the expression generated to transfer the values
            /// </param>
            /// <param name="fieldInfo">Reflection informations about the field being cloned</param>
            private void GenerateFieldBasedReferenceTypeTransferExpressions(Expression original, Expression clone,
                ICollection<Expression> expressions, FieldInfo fieldInfo)
            {
                // Reference types and arrays require special care because they can be null, so gather the transfer expressions in a separate block for the null check
                var fieldExpressions = new List<Expression>();
                var fieldVariables = new List<ParameterExpression>();

                var fieldType = fieldInfo.FieldType;

                if (fieldType.IsArray)
                {
                    Expression fieldClone = GenerateFieldBasedComplexArrayTransferExpressions(
                        fieldType,
                        fieldType.GetElementType(),
                        Expression.Field(original, fieldInfo),
                        fieldVariables,
                        fieldExpressions);
                    fieldExpressions.Add(CloneExpressionHelper.CreateSetFieldExpression(clone, fieldClone, fieldInfo));
                }
                else
                {
                    fieldExpressions.Add(CloneExpressionHelper.CreateCopyComplexFieldExpression(original, clone,
                        fieldInfo, _objectDictionary));
                }

                expressions.Add(
                    Expression.IfThen(
                        Expression.NotEqual(Expression.Field(original, fieldInfo), Expression.Constant(null)),
                        Expression.Block(fieldVariables, fieldExpressions)));
            }
        }
    }
}