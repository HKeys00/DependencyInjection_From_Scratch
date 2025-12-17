// See https://aka.ms/new-console-template for more information

using DependencyInjection_From_Scratch;

Console.WriteLine("Hello, World!");

var container = new ServiceContainer();

container.AddScoped<ITestService, TestService>();
//var m = container.GetService<ITestService>();
var m = container.GetService(typeof(ITestService));

Console.ReadLine();

