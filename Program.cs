// See https://aka.ms/new-console-template for more information

using DependencyInjection_From_Scratch;
using DependencyInjection_From_Scratch.Services;

Console.WriteLine("Hello, World!");

var container = new ServiceContainer();

container.AddScoped<ITestService, TestService>();
container.AddScoped<INestedService, NestedService>();
//var m = container.GetService<ITestService>();
var m = container.GetRequiredService(typeof(ITestService));

Console.ReadLine();

