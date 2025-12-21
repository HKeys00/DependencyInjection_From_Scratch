using DependencyInjection_From_Scratch;
using DependencyInjection_From_Scratch.Services;
using Service = DependencyInjection_From_Scratch.Services.Service;
namespace Test
{
    public class UnitTest1
    {
        [Fact]
        public void Resolve_ConcreteType_WithNoDependencies_ReturnsInstance()
        {
            var container = new ServiceContainer();

            container.AddTransient<C>();

            var result = container.Resolve<C>();

            Assert.NotNull(result);
            Assert.IsType<C>(result);
        }

        [Fact]
        public void Resolve_InterfaceMapping_WithNoDependencies_ReturnsConcreteInstance()
        {
            var container = new ServiceContainer();

            container.AddTransient<IC, C>();

            var result = container.Resolve<IC>();

            Assert.NotNull(result);
            Assert.IsType<C>(result);
        }

        [Fact]
        public void Resolve_TypeWithSingleDependency_InjectsDependency()
        {
            var container = new ServiceContainer();

            container.AddTransient<IB, B>();
            container.AddTransient<IC, C>();

            var result = container.Resolve<IB>() as B;

            Assert.NotNull(result);
            Assert.NotNull(result.C);
            Assert.IsType<C>(result.C);
        }

        [Fact]
        public void Resolve_MultiLevelDependencyGraph_ResolvesRecursively()
        {
            var container = new ServiceContainer();

            container.AddTransient<IA, A>();
            container.AddTransient<IB, B>();
            container.AddTransient<IC, C>();

            var result = container.Resolve<IA>() as A;

            Assert.NotNull(result);
            Assert.NotNull(result.B);
            Assert.NotNull(result.B.C);

            Assert.IsType<B>(result.B);
            Assert.IsType<C>(result.B.C);
        }

        [Fact]
        public void Resolve_SameServiceTwice_ReturnsDifferentObjectGraphs()
        {
            var container = new ServiceContainer();

            container.AddTransient<IA, A>();
            container.AddTransient<IB, B>();
            container.AddTransient<IC, C>();

            var first = container.Resolve<IA>() as A;
            var second = container.Resolve<IA>() as A;

            Assert.NotSame(first, second);
            Assert.NotSame(first.B, second.B);
            Assert.NotSame(first.B.C, second.B.C);
        }

        [Fact]
        public void Resolve_MissingDependency_Throws()
        {
            var container = new ServiceContainer();

            container.AddTransient<IA, A>();
            container.AddTransient<IB, B>();
            // IC is missing

            Assert.Throws<InvalidOperationException>(() => container.Resolve<IA>());
        }

        [Fact]
        public void Resolve_ConcreteTypeWithDependencies_ResolvesRecursively()
        {
            var container = new ServiceContainer();

            container.AddTransient<B>();
            container.AddTransient<IC, C>();

            var result = container.Resolve<B>() as B;

            Assert.NotNull(result);
            Assert.NotNull(result.C);
        }

        [Fact]
        public void Register_Interface_DoesNotAllow_ResolveConcrete()
        {
            var container = new ServiceContainer();
            container.AddTransient<IC, C>();

            Assert.Throws<InvalidOperationException>(() => container.Resolve<C>());
        }

        [Fact]
        public void Register_Concrete_DoesNotAllow_ResolveInterface()
        {
            var container = new ServiceContainer();
            container.AddTransient<C>();

            Assert.Throws<InvalidOperationException>(() => container.Resolve<IC>());
        }

        [Fact]
        public void Resolve_UnregisteredInterface_Throws()
        {
            var container = new ServiceContainer();
            Assert.Throws<InvalidOperationException>(() => container.Resolve<IA>());
        }

        [Fact]
        public void Resolve_AbstractClass_Throws()
        {
            var container = new ServiceContainer();
            container.AddTransient<IB, B>();

            Assert.Throws<InvalidOperationException>(() => container.Resolve<IB>());
        }

        [Fact]
        public void Register_SameServiceTwice_LastRegistrationWins()
        {
            var container = new ServiceContainer();
            container.AddTransient<IC, C>();
            container.AddTransient<IC, D>();

            var result = container.Resolve<IC>();

            Assert.IsType<D>(result);
        }


        [Fact]
        public void Resolve_PicksConstructorWithMostParameters()
        {
            var container = new ServiceContainer();
            container.AddTransient<IC, C>();
            container.AddTransient<IB, B>();
            container.AddTransient<MultiCtor>();

            var result = container.Resolve<MultiCtor>();

            Assert.NotNull(result?.B);
        }

        [Fact]
        public void Resolve_MultipleBestConstructors_Throws()
        {
            var container = new ServiceContainer();
            container.AddTransient<AmbiguousCtor>();

            Assert.Throws<InvalidOperationException>(() => container.Resolve<AmbiguousCtor>());
        }

        [Fact]
        public void Resolve_ConstructorWithUnresolvableParameter_Throws()
        {
            var container = new ServiceContainer();
            container.AddTransient<IB, B>();
            container.AddTransient<MultiCtor>();

            Assert.Throws<InvalidOperationException>(() => container.Resolve<MultiCtor>());
        }

        [Fact]
        public void Resolve_CircularDependency_ThrowsMeaningfully()
        {
            var container = new ServiceContainer();
            container.AddTransient<CircularA>();
            container.AddTransient<CircularB>();

            Assert.Throws<InvalidOperationException>(() => container.Resolve<CircularA>());
        }

        [Fact]
        public void Resolve_SameDependencyInjectedTwice_CreatesDistinctInstances()
        {
            var container = new ServiceContainer();
            container.AddTransient<IC, C>();
            container.AddTransient<DuplicateDependency>();

            var result = container.Resolve<DuplicateDependency>();

            Assert.NotSame(result?.C1, result?.C2);
        }

        [Fact]
        public void Resolve_ValueTypeParameter_Throws()
        {
            var container = new ServiceContainer();
            container.AddTransient<ValueTypeCtor>();

            Assert.Throws<InvalidOperationException>(() => container.Resolve<ValueTypeCtor>());
        }

        [Fact]
        public void Transient_ResolveSameServiceTwice_ReturnsDifferentInstances()
        {
            var container = new ServiceContainer();
            container.AddTransient<IService, Service>();

            var first = container.Resolve<IService>();
            var second = container.Resolve<IService>();

            Assert.NotSame(first, second);
        }

        [Fact]
        public void Transient_InjectedMultipleTimes_CreatesDistinctInstances()
        {
            var container = new ServiceContainer();
            container.AddTransient<IService, Service>();
            container.AddTransient<Consumer>();

            var consumer = container.Resolve<Consumer>();

            Assert.NotNull(consumer.Service);
        }

        [Fact]
        public void Singleton_ResolveSameServiceTwice_ReturnsSameInstance()
        {
            var container = new ServiceContainer();
            container.AddSingleton<IService, Service>();

            var first = container.Resolve<IService>();
            var second = container.Resolve<IService>();

            Assert.Same(first, second);
        }

        [Fact]
        public void Singleton_InjectedIntoDifferentConsumers_IsSameInstance()
        {
            var container = new ServiceContainer();
            container.AddSingleton<IService, Service>();
            container.AddTransient<Consumer>();

            var firstConsumer = container.Resolve<Consumer>();
            var secondConsumer = container.Resolve<Consumer>();

            Assert.Same(firstConsumer.Service, secondConsumer.Service);
        }

        [Fact]
        public void Singleton_ConcreteType_IsSingleInstance()
        {
            var container = new ServiceContainer();
            container.AddSingleton<Service>();

            var first = container.Resolve<Service>();
            var second = container.Resolve<Service>();

            Assert.Same(first, second);
        }


        [Fact]
        public void Singleton_DependingOnTransient_CapturesSingleInstance()
        {
            var container = new ServiceContainer();
            container.AddTransient<IService, Service>();
            container.AddSingleton<Consumer>();

            var first = container.Resolve<Consumer>();
            var second = container.Resolve<Consumer>();

            Assert.Same(first, second);
            Assert.Same(first.Service, second.Service);
        }

        [Fact]
        public void Transient_DependingOnSingleton_GetsSameSingletonInstance()
        {
            var container = new ServiceContainer();
            container.AddSingleton<IService, Service>();
            container.AddTransient<Consumer>();

            var first = container.Resolve<Consumer>();
            var second = container.Resolve<Consumer>();

            Assert.Same(first.Service, second.Service);
        }

        [Fact]
        public void Singleton_IsCreatedOnFirstResolve_NotOnRegistration()
        {
            CreationCounter.Count = 0;

            var container = new ServiceContainer();
            container.AddSingleton<CreationCounter>();

            Assert.Equal(0, CreationCounter.Count);

            container.Resolve<CreationCounter>();

            Assert.Equal(1, CreationCounter.Count);
        }
    }
}


