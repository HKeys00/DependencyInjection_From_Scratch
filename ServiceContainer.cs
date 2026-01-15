using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace DependencyInjection_From_Scratch
{
    public class ServiceContainer : IServiceContainer
    {
        private Stack<Type> _resolveStack;

        private Dictionary<Type, Func<Stack<Type>, ServiceContainer, object>> _expressionGetters;
        private Dictionary<Type, Service> _services;
        private Dictionary<Type, object> _singletonInstances;

        public ServiceContainer()
        {
            _resolveStack = new Stack<Type>();
            _services = new Dictionary<Type, Service>();
            _singletonInstances = new Dictionary<Type, object>();
            _expressionGetters = new Dictionary<Type, Func<Stack<Type>, ServiceContainer, object>>();
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
            _singletonInstances.Add(typeof(TImplementation), CreateServiceInstanceReflection(typeof(TImplementation)));
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
                    return CreateServiceInstanceExpression(service.Implementation);

                case ServiceLifetimes.Singleton:
                    _singletonInstances.TryGetValue(service.Implementation, out var singleton);
                    if (singleton == null)
                    {
                        var instance = CreateServiceInstanceExpression(service.Implementation);
                        //var instance = CreateServiceInstanceReflection(service.Implementation);
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

        private object? CreateServiceInstanceReflection(Type type)
        {
            var constructor = GetMostSuitableConstructor(type);
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

        private object? CreateServiceInstanceExpression(Type type)
        {
            if (_expressionGetters.TryGetValue(type, out var getter))
            {
                return getter.Invoke(_resolveStack, this);
            }

            var foundConstructor = GetMostSuitableConstructor(type);
            var param = foundConstructor.GetParameters();
            var stack = Expression.Parameter(typeof(Stack<Type>), "resolveStack");
            var container = Expression.Parameter(typeof(ServiceContainer), "container");
            var parameterExpressions = new List<Expression>();

            foreach (var parameter in param)
            {
                var checkStackCall = Expression.IfThen(
                    Expression.Call(
                        stack,
                        typeof(Stack<Type>).GetMethod("Contains")!,
                        Expression.Constant(parameter.ParameterType)
                    ),
                    Expression.Throw(
                        Expression.New(typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) })!,
                            Expression.Constant($"Cyclical dependency detected: {parameter.ParameterType} for {type}"))
                    )
                );

                var pushCall = Expression.Call(stack, typeof(Stack<Type>).GetMethod("Push")!,
                    Expression.Constant(parameter.ParameterType));

                var getServiceCall = Expression.Call(container,
                    typeof(ServiceContainer).GetMethod("GetRequiredService")!, container);

                var throwIfNull = Expression.IfThen(
                    Expression.Equal(getServiceCall, Expression.Constant(null)),
                    Expression.Throw(
                        Expression.New(typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) })!,
                            Expression.Constant($"Failed to create instance for Type: {type}"))
                    )
                );

                var block = Expression.Block(checkStackCall, pushCall, getServiceCall, throwIfNull);
                parameterExpressions.Add(block);
            }

            var newExpression = Expression.New(foundConstructor, parameterExpressions);

            try
            {
                var lambda = Expression.Lambda<Func<Stack<Type>, ServiceContainer, object>>(newExpression, stack, container);
                _expressionGetters[type] = lambda.Compile();
            }
            catch (Exception ex)
            {
                var m = ex;
            }

            return _expressionGetters[type].Invoke(_resolveStack, this);
        }

        private ConstructorInfo GetMostSuitableConstructor(Type type)
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

            return suitableConstructors[parameterCounts.IndexOf(max)];
        }
    }
}
