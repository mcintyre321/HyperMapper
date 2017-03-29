using System;

namespace HyperMapper.Mapper
{
    public static class DefaultServiceLocatorDelegate
    {
        public static ServiceLocatorDelegate CreateUsingEmptyCtorAndDisposeIfAvailable = type =>
        {
            var instance = Activator.CreateInstance(type);

            return Tuple.Create(instance, new Action(() =>
            {
                if (instance is IDisposable)
                {
                    ((IDisposable)instance).Dispose();
                }
            }));
        };

    }
}