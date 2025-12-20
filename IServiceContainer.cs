namespace DependencyInjection_From_Scratch
{
    internal interface IServiceContainer
    {
        public void AddTransient<TInterface, TImplementation>() where TImplementation : class;
        public void AddTransient<TImplementation>() where TImplementation : class;
        public void AddScoped<TInterface, TImplementation>() where TImplementation : class;
        public void AddScoped<TImplementation>() where TImplementation : class;
        public void AddSingleton<TInterface, TImplementation>() where TImplementation : class;
        public void AddSingleton<TImplementation>()where TImplementation : class;

        T GetService<T>();

        public object? Resolve<T>() where T : class;
        public object GetRequiredService (Type serviceType);
    }
}
