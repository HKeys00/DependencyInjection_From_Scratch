using DependencyInjection_From_Scratch;
using DependencyInjection_From_Scratch.Services;

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


    }
}
