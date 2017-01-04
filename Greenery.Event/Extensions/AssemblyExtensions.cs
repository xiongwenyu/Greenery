using System;
using System.Reflection;

namespace Greenery.Event.Extensions
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// 通过接口遍历实现实例
        /// </summary>
        /// <typeparam name="TBaseInterface">接口类</typeparam>
        /// <param name="assembly">程序集</param>
        /// <param name="each">回调处理匹配类型</param>
        public static void GetImplementedObjectsByInterface<TBaseInterface>(this Assembly assembly, Action<TBaseInterface, Type> each = null)
        {
            Type targetType = typeof(TBaseInterface);
            Type[] arrType = assembly.GetExportedTypes();


            for (int i = 0; i < arrType.Length; i++)
            {
                var currentImplementType = arrType[i];

                if (currentImplementType.IsAbstract)
                    continue;

                if (!targetType.IsAssignableFrom(currentImplementType))
                    continue;
                var item = (TBaseInterface)Activator.CreateInstance(currentImplementType);
                if (each != null && item != null)
                {
                    each(item, currentImplementType);
                }
            }

        }

    }
}
