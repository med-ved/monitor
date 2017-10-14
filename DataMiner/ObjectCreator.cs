namespace DataMiner
{
    using System;
    using DataMiner.Database;
    using Ninject;

    public static class ObjectCreator
    {
        private static IKernel _kernel;
        private static Action<IKernel> _register;

        public static void Init(IKernel kernel)
        {
            _kernel = kernel;
        }

        public static void Init(Action<IKernel> register = null)
        {
            _register = register;
        }

        public static T Get<T>()
        {
            if (_kernel == null)
            {
                _kernel = new StandardKernel();
                _register(_kernel); // register the objects
            }
            
            return _kernel.Get<T>();
        }
    }
}
