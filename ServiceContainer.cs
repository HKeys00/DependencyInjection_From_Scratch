using System.Reflection;

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
            _singletonInstances.Add(typeof(TImplementation), CreateServiceInstance(typeof(TImplementation)));
        }

        public object GetRequiredService(Type serviceType)
        {
            _services.TryGetValue(serviceType, out var service);
            if (service == null)
            {
                throw new InvalidOperationException($"Cannot resolve service for {serviceType}");
            }

            var instance = ResolveService(service.Lifetime, service.Implementation);

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

        private object? ResolveService(ServiceLifetimes lifetime, Type implementationType)
        {
            switch (lifetime)
            {
                case ServiceLifetimes.Transient:
                case ServiceLifetimes.Scoped:
                    return CreateServiceInstance(implementationType);

                case ServiceLifetimes.Singleton:
                    return _singletonInstances.TryGetValue(implementationType, out var singleton);

                default: 
                    return null;
            }
        }

        private object? CreateServiceInstance(Type type)
        {
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            int mostParametersMatching = 0;
            int bestSuitedContructorIndex = -1;

            for (int i = 0; i < constructors.Length; i++)
            {
                var parameters = constructors[i].GetParameters();
                int paramCount = parameters.Length;
                int foundCount = parameters.Count(p => _services.ContainsKey(p.ParameterType));

                if (paramCount == foundCount)
                {
                    if (bestSuitedContructorIndex == -1)
                    {
                        bestSuitedContructorIndex = i;
                    }

                    if (foundCount > mostParametersMatching)
                    {
                        mostParametersMatching = foundCount;
                        bestSuitedContructorIndex = i;
                    }
                }
            }

            if (bestSuitedContructorIndex == -1)
            {
                throw new InvalidOperationException($"Failed to find suitable constructor for Type: {type}");
            }

            var constructor = constructors[bestSuitedContructorIndex];
            List<object> parameterInstances = new List<object>();
            foreach (var parameter in constructor.GetParameters())
            {
                var instance = GetRequiredService(parameter.ParameterType);

                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to create instance for Type: {type}");
                }

                parameterInstances.Add(instance);
            }

            return constructors[bestSuitedContructorIndex].Invoke(parameterInstances.ToArray());
        }
    }
}
