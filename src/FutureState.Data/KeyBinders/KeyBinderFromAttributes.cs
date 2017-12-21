using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace FutureState.Data
{
    /// <summary>
    ///     Generic all purpose binder that uses reflection to determine
    ///     an entity's primary key so that it can be updated by the caller.
    /// </summary>
    /// <typeparam name="TEntity">The entity to bind the id value to.</typeparam>
    /// <typeparam name="TKey">The type of key to use.</typeparam>
    /// <remarks>
    ///     Looking for:
    ///     PrimaryKeyAttribute || KeyAttribute
    /// </remarks>
    public class KeyBinderFromAttributes<TEntity, TKey> : IKeyBinder<TEntity, TKey>
    {
        // ReSharper disable once StaticFieldInGenericType
        private static readonly PropertyGetterDelegate GetterFn;

        // ReSharper disable once StaticFieldInGenericType
        private static readonly PropertySetterDelegate SetterFn;

        static KeyBinderFromAttributes()
        {
            //todo: replace with function to get and set pk values in repository

            var pk = typeof(TEntity).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(m => m.GetCustomAttributes(true).Any(n => n is KeyAttribute));

            if (pk == null)
                throw new InvalidOperationException(
                    $"Unable to resolve the primary key field of entity {typeof(TEntity).Name} to build its getter and setter functions.");

            GetterFn = pk.GetPropertyGetterFn();
            SetterFn = pk.GetPropertySetterFn();
        }

        public TKey Get(TEntity entity)
        {
            return (TKey) GetterFn(entity); //don't check for null entity to avoid perf penalty
        }

        public void Set(TEntity entity, TKey key)
        {
            SetterFn(entity, key); //don't check for null entity
        }
    }
}