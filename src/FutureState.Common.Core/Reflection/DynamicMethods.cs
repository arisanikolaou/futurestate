﻿#region

using System;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace FutureState
{
    // pinched from http://www.codeproject.com/Articles/22615/Dynamically-Invoke-Generic-Methods
    public static class DynamicMethods
    {
        public static DynamicInvoker GenericMethodInvokerMethod(
            Type type,
            string methodName,
            Type[] typeArguments,
            Type[] parameterTypes)
        {
            MethodInfo methodInfo;
            ParameterInfo[] parameters;

            // find the method to be invoked
            FindMethod(type, methodName, typeArguments, parameterTypes, out methodInfo, out parameters);

            var name = $"__MethodInvoker_{methodInfo.Name}_ON_{methodInfo.DeclaringType.Name}";
            var dynamicMethod = new DynamicMethod(
                name,
                typeof(object),
                new[] { typeof(object), typeof(object[]) },
                methodInfo.DeclaringType);

            var generator = dynamicMethod.GetILGenerator();

            // define local vars
            generator.DeclareLocal(typeof(object));

            // load first argument, the instace where the method is to be invoked
            generator.Emit(OpCodes.Ldarg_0);

            // cast to the correct type
            generator.Emit(OpCodes.Castclass, methodInfo.DeclaringType);

            for (var i = 0; i < parameters.Length; i++)
            {
                // load paramters they are passed as an object array
                generator.Emit(OpCodes.Ldarg_1);

                // load array element
                generator.Emit(OpCodes.Ldc_I4, i);
                generator.Emit(OpCodes.Ldelem_Ref);

                // cast or unbox parameter as needed
                var parameterType = parameters[i].ParameterType;
                if (parameterType.IsClass)
                {
                    generator.Emit(OpCodes.Castclass, parameterType);
                }
                else
                {
                    generator.Emit(OpCodes.Unbox_Any, parameterType);
                }
            }

            // call method
            generator.EmitCall(OpCodes.Callvirt, methodInfo, null);

            // handle method return if needed
            if (methodInfo.ReturnType == typeof(void))
            {
                // return null
                generator.Emit(OpCodes.Ldnull);
            }
            else
            {
                // box value if needed
                if (methodInfo.ReturnType.IsValueType)
                {
                    generator.Emit(OpCodes.Box, methodInfo.ReturnType);
                }
            }

            // store to the local var
            generator.Emit(OpCodes.Stloc_0);

            // load local and return
            generator.Emit(OpCodes.Ldloc_0);
            generator.Emit(OpCodes.Ret);

            // return delegate
            return (DynamicInvoker)dynamicMethod.CreateDelegate(typeof(DynamicInvoker));
        }

        public static DynamicInvoker GenericMethodInvokerMethod(Type type, string methodName, Type[] typeArguments)
        {
            return GenericMethodInvokerMethod(type, methodName, typeArguments, null);
        }

        private static void FindMethod(
            Type type,
            string methodName,
            Type[] typeArguments,
            Type[] parameterTypes,
            out MethodInfo methodInfo,
            out ParameterInfo[] parameters)
        {
            methodInfo = null;
            parameters = null;

            if (null == parameterTypes)
            {
                methodInfo = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
                methodInfo = methodInfo.MakeGenericMethod(typeArguments);
                parameters = methodInfo.GetParameters();
            }
            else
            {
                // Method is probably overloaded. As far as i know there's no other way to get the MethodInfo instance, we have to
                // search for it in all the type methods
                var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var method in methods)
                {
                    if (method.Name == methodName)
                    {
                        // create the generic method
                        var genericMethod = method.MakeGenericMethod(typeArguments);
                        parameters = genericMethod.GetParameters();

                        // compare the method parameters
                        if (parameters.Length == parameterTypes.Length)
                        {
                            for (var i = 0; i < parameters.Length; i++)
                            {
                                if (parameters[i].ParameterType != parameterTypes[i])
                                {
                                }
                            }

                            // if we'r here, we got the rigth method
                            methodInfo = genericMethod;
                            break;
                        }
                    }
                }

                if (null == methodInfo)
                {
                    throw new InvalidOperationException("Method not found");
                }
            }
        }
    }
}