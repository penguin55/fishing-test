using System;
using System.Collections.Generic;
namespace AriUtomo.Pattern
{
    public static class ServiceLocator
    {
        private static Dictionary<Type, object> services = new Dictionary<Type, object>();

        public static void Provide(object service) 
        {
            if (!IsExist(service.GetType())) services[service.GetType()] = service;
        }

        public static T GetService<T>()
        {
            if (IsExist(typeof(T))) return (T)services[typeof(T)];
            else return default(T);
        }

        private static bool IsExist(Type type) => services.ContainsKey(type);
    }
}