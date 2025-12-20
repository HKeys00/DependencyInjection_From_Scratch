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

}
