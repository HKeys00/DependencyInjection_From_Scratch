namespace DependencyInjection_From_Scratch
{
    public class ServiceContainer : IServiceContainer
    {
        private Dictionary<Type, Service> _services;

        public ServiceContainer()
        {
            _services = new Dictionary<Type, Service>();
        }

        public void AddScoped<TInterface, TImplementation>(TInterface instance, TImplementation instanceImplementation) where TImplementation : class
        {
            throw new NotImplementedException();
        }

        public void AddSingleton<TInterface, TImplementation>(TInterface instance, TImplementation instanceImplementation) where TImplementation : class
        {
            throw new NotImplementedException();
        }

        public void AddTransient<TInterface, TImplementation>(TInterface instance, TImplementation instanceImplementation) where TImplementation : class
        {
            throw new NotImplementedException();
        }

        public object GetRequiredService(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public T GetService<T>()
        {
            throw new NotImplementedException();
        }

        public object? GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }

        private void AddService<TInterface, TImplementation>(TInterface instance, TImplementation instanceImplementation, ServiceLifetimes lifetime) where TImplementation : class
        {
            var service = new Service()
            {
                Lifetime = lifetime,
                Interface = typeof(TInterface),
                Implementation = typeof(TImplementation)
            };

            _services.Add(typeof(TInterface), service);
        }
    }
}
