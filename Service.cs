namespace DependencyInjection_From_Scratch
{
    public class Service
    {
        public ServiceLifetimes Lifetime { get; set; }

        public Type Interface { get; set; }

        public Type Implementation { get; set; }

        public Action<object> Factory { get; set; }
    }
}
