namespace DependencyInjection_From_Scratch
{
    internal interface IServiceContainer
    {
        public void AddTransient<TInterface, TImplementation>() where TImplementation : class;
        public void AddScoped<TInterface, TImplementation>() where TImplementation : class;
        public void AddSingleton<TInterface, TImplementation>() where TImplementation : class;

        T GetService<T>();

        public object? GetService(Type serviceType);
        public object GetRequiredService (Type serviceType);
    }
}
