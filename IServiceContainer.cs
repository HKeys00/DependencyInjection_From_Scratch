namespace DependencyInjection_From_Scratch
{
    internal interface IServiceContainer
    {
        public void AddTransient<TInterface, TImplementation>(TInterface instance, TImplementation instanceImplementation) where TImplementation : class;
        public void AddScoped<TInterface, TImplementation>() where TImplementation : class;
        public void AddSingleton<TInterface, TImplementation>(TInterface instance, TImplementation instanceImplementation) where TImplementation : class;

        T GetService<T>();

        public object? GetService(Type serviceType);
        public object GetRequiredService (Type serviceType);
    }
}
