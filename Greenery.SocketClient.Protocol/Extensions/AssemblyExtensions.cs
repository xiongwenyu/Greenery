﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Greenery.SocketClient.Protocol.Extensions
{
    public static class AssemblyExtensions
    {
        public static IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly, Func<Type, bool> filter = null)
         where TBaseInterface : class
        {
            return GetImplementedObjectsByInterface<TBaseInterface>(assembly, typeof(TBaseInterface), filter);
        }

        /// <summary>
        /// Gets the implemented objects by interface.
        /// </summary>
        /// <typeparam name="TBaseInterface">The type of the base interface.</typeparam>
        /// <param name="assembly">The assembly.</param>
        /// <param name="targetType">Type of the target.</param>
        /// <returns></returns>
        public static IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly, Type targetType, Func<Type, bool> filter = null)
            where TBaseInterface : class
        {
            Type[] arrType = assembly.GetExportedTypes();

            var result = new List<TBaseInterface>();

            for (int i = 0; i < arrType.Length; i++)
            {
                var currentImplementType = arrType[i];

                if (currentImplementType.IsAbstract)
                    continue;

                if (!targetType.IsAssignableFrom(currentImplementType))
                    continue;
                if (filter != null && filter(currentImplementType))
                    continue;
                result.Add((TBaseInterface)Activator.CreateInstance(currentImplementType));
            }

            return result;
        }
    }
}
