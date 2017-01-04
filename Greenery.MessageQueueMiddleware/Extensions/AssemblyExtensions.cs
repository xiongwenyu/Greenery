using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Greenery.MessageQueueMiddleware.Extensions
{
    public static class AssemblyExtensions
    {

        /// <summary>
        /// 通过接口/基础类型获取所有实现程序中的继承实例【实例必须为有无参构造函数且为公共非抽象类】
        /// </summary>
        /// <typeparam name="TBaseInterface">接口或者基础类型.</typeparam>
        /// <param name="assembly">程序集</param>
        /// <returns>继承实例集合</returns>
        public static IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly, Func<Type, bool> filter = null)
            where TBaseInterface : class
        {
            Type[] arrType = assembly.GetExportedTypes();
            Type targetType = typeof(TBaseInterface);
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
        public static IEnumerable<TBaseInterface> GetImplementedObjectsByInterface<TBaseInterface>(this string assemblyFilePath, Func<Type, bool> filter = null)
            where TBaseInterface : class
        {
            if (!File.Exists(assemblyFilePath))
            {
                throw new FileNotFoundException("未加载程序，程序不存在！");
            }
            var assembly = Assembly.LoadFile(assemblyFilePath);
            return assembly.GetImplementedObjectsByInterface<TBaseInterface>(filter);
        }
    }
}
