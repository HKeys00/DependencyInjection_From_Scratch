namespace DependencyInjection_From_Scratch
{
    public class ServiceContainer : IServiceContainer
    {
        private Dictionary<Type, Service> _services;
        private Dictionary<Type, object> _singletonInstances;

        public ServiceContainer()
        {
            _services = new Dictionary<Type, Service>();
            _singletonInstances = new Dictionary<Type, object>();
        }
        public void AddTransient<TInterface, TImplementation>() where TImplementation : class
        {
            AddService<TInterface, TImplementation>(ServiceLifetimes.Transient);
        }

        public void AddScoped<TInterface, TImplementation>() where TImplementation : class
        {
            AddService<TInterface, TImplementation>(ServiceLifetimes.Scoped);
        }

        public void AddSingleton<TInterface, TImplementation>() where TImplementation : class
        {
            AddService<TInterface, TImplementation>(ServiceLifetimes.Singleton);
            _singletonInstances.Add(typeof(TInterface), GetRequiredService(typeof(TInterface)));
        }

        public object GetRequiredService(Type serviceType)
        {
            _services.TryGetValue(serviceType, out var service);
            if (service == null)
            {
                throw new InvalidOperationException($"Cannot resolve service for {serviceType}");
            }

            var instance = Activator.CreateInstance(service.Implementation);

            if (instance == null)
            {
                throw new InvalidOperationException($"Cannot create instance of {service.Implementation}");
            }

            return instance;
        }

        public object? GetService(Type serviceType)
        {
            _services.TryGetValue(serviceType, out var service);
            return Activator.CreateInstance(service.Implementation);
        }

        public T GetService<T>()
        {
            _services.TryGetValue(typeof(T), out var service);
            //return Activator.CreateInstance(service.Implementation);
            throw new NotImplementedException();
        }

        private void AddService<TInterface, TImplementation>(ServiceLifetimes lifetime) where TImplementation : class
        {
            var service = new Service()
            {
                Lifetime = lifetime,
                Interface = typeof(TInterface),
                Implementation = typeof(TImplementation),
            };

            _services.Add(typeof(TInterface), service);
        }
    }
}
