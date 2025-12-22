using System.Reflection;

namespace DependencyInjection_From_Scratch
{
    public class ServiceContainer : IServiceContainer
    {
        private Stack<Type> _resolveStack;
        
        private Dictionary<Type, Service> _services;
        private Dictionary<Type, object> _singletonInstances;

        public ServiceContainer()
        {
            _resolveStack = new Stack<Type>();
            _services = new Dictionary<Type, Service>();
            _singletonInstances = new Dictionary<Type, object>();
        }
        public void AddTransient<TInterface, TImplementation>() where TImplementation : class
        {
            AddService<TInterface, TImplementation>(ServiceLifetimes.Transient);
        }

        public void AddTransient<TImplementation>() where TImplementation : class
        {
            AddService<TImplementation>(ServiceLifetimes.Transient);
        }

        public void AddScoped<TInterface, TImplementation>() where TImplementation : class
        {
            AddService<TInterface, TImplementation>(ServiceLifetimes.Scoped);
        }

        public void AddScoped<TImplementation>() where TImplementation : class
        {
            AddService<TImplementation>(ServiceLifetimes.Scoped);
        }

        public void AddSingleton<TInterface, TImplementation>() where TImplementation : class
        {
            AddService<TInterface, TImplementation>(ServiceLifetimes.Singleton);
            _singletonInstances.Add(typeof(TImplementation), CreateServiceInstance(typeof(TImplementation)));
        }
        public void AddSingleton<TImplementation>() where TImplementation : class
        {
            AddService<TImplementation>(ServiceLifetimes.Singleton);
        }

        private object GetRequiredService(Type serviceType)
        {
            _services.TryGetValue(serviceType, out var service);
            if (service == null)
            {
                throw new InvalidOperationException($"Cannot resolve service for {serviceType}");
            }

            var instance = ResolveService(service);

            if (instance == null)
            {
                throw new InvalidOperationException($"Cannot create instance of {service.Implementation}");
            }

            return instance;
        }

        public T? Resolve<T>() where T : class
        {
            _resolveStack = new Stack<Type>();
            return GetRequiredService(typeof(T)) as T;
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

            _services[typeof(TInterface)] = service;
        }

        private void AddService<TImplementation>(ServiceLifetimes lifetime) where TImplementation : class
        {
            var service = new Service()
            {
                Lifetime = lifetime,
                Interface = null,
                Implementation = typeof(TImplementation),
            };

            _services[typeof(TImplementation)] = service;
        }

        private object? ResolveService(Service service)
        {
            switch (service.Lifetime)
            {
                case ServiceLifetimes.Transient:
                case ServiceLifetimes.Scoped:
                    return CreateServiceInstance(service.Implementation);

                case ServiceLifetimes.Singleton:
                    _singletonInstances.TryGetValue(service.Implementation, out var singleton);
                    if (singleton == null)
                    {
                        var instance = CreateServiceInstance(service.Implementation);
                        if (instance == null)
                        {
                            throw new InvalidOperationException("Service could not be created");
                        }

                        _singletonInstances[service.Implementation] = instance;
                    }

                    return _singletonInstances[service.Implementation];

                default: 
                    return null;
            }
        }

        private object? CreateServiceInstance(Type type)
        {
            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);

            List<ConstructorInfo> suitableConstructors = new List<ConstructorInfo>();

            foreach (var c in constructors)
            {
                var parameters = c.GetParameters();
                int paramCount = parameters.Length;
                int foundCount = parameters.Count(p => _services.ContainsKey(p.ParameterType));

                if (paramCount == foundCount)
                {
                    suitableConstructors.Add(c);
                }
            }

            if (suitableConstructors.Count == 0)
            {
                throw new InvalidOperationException($"Failed to find suitable constructor for Type: {type}");
            }

            var parameterCounts = suitableConstructors.Select(c => c.GetParameters().Length).ToList();
            int max = parameterCounts.Max();
            if (parameterCounts.Count(x => x == max) > 1)
            {
                throw new InvalidOperationException($"Multiple valid constructors found for {type}");
            }

            var constructor = suitableConstructors[parameterCounts.IndexOf(max)];
            List<object> parameterInstances = new List<object>();
            foreach (var parameter in constructor.GetParameters())
            {
                if (_resolveStack.Contains(parameter.ParameterType))
                {
                    throw new InvalidOperationException(
                        $"Cyclical dependency detected: {parameter.ParameterType} for {type}");
                }

                _resolveStack.Push(parameter.ParameterType);
                var instance = GetRequiredService(parameter.ParameterType);

                if (instance == null)
                {
                    throw new InvalidOperationException($"Failed to create instance for Type: {type}");
                }

                parameterInstances.Add(instance);
            }

            return constructor.Invoke(parameterInstances.ToArray());
        }
    }
}
