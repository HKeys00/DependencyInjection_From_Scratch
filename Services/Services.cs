using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection_From_Scratch.Services
{
    public interface IA {
        public IB B { get; }
    }
    public interface IB {
        public IC C { get; }
    }
    public interface IC { }
    public interface ID { }

    public class A : IA
    {
        public IB B { get; }

        public A(IB b)
        {
            B = b;
        }
    }

    public class B : IB
    {
        public IC C { get; }

        public B(IC c)
        {
            C = c;
        }
    }

    public class C : IC
    {
    }

    public class D : ID
    {
    }

    public class MultiCtor
    {
        public IC C { get; }
        public IB B { get; }

        public MultiCtor(IC c) => C = c;
        public MultiCtor(IC c, IB b) { C = c; B = b; }
    }

    public class AmbiguousCtor
    {
        public AmbiguousCtor(IC c) { }
        public AmbiguousCtor(ID d) { }
    }

    public class CircularA
    {
        public CircularA(CircularB b) { }
    }

    public class CircularB
    {
        public CircularB(CircularA a) { }
    }

    public class DuplicateDependency
    {
        public IC C1 { get; }
        public IC C2 { get; }

        public DuplicateDependency(IC c1, IC c2)
        {
            C1 = c1;
            C2 = c2;
        }
    }

    public class ValueTypeCtor
    {
        public ValueTypeCtor(int x) { }
    }

    public class CreationCounter
    {
        public static int Count;

        public CreationCounter()
        {
            Count++;
        }
    }

    public interface IService { }

    public class Service : IService { }

    public class Consumer
    {
        public IService Service { get; }

        public Consumer(IService service)
        {
            Service = service;
        }
    }

}
