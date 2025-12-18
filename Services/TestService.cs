using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjection_From_Scratch.Services
{
    public interface ITestService
    {
    }

    public class TestService : ITestService
    {

        public TestService(int i) { }

        public TestService() { }

        public TestService(INestedService nestedService) 
        {
            var m = nestedService; //Line to put breakpoint.
        }
        
    }
}
