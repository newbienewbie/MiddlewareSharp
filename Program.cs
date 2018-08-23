using System;
using Itminus.Middleware;

namespace MiddwarePrototype
{

    public interface IContext{
        bool Dummy {get;set;}
        void Hello();
    }

    public class XContext : IContext
    {
        public bool Dummy { get;set; }

        public void Hello()
        {
            System.Console.WriteLine("world");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var container = new WorkContainer<IContext>();
            container.Use(next =>
            {
                return async context =>
                {
                    Console.WriteLine("Thinking in Middleware - outermost");
                    await next(context);
                    Console.WriteLine("Thinking in Middleware - outermost");
                };
            })
            .Use(next =>
            {
                return async context =>
                {
                    Console.WriteLine("It works - innermost");
                    await next(context);
                    Console.WriteLine("It works - innermost");
                };
            });

            var d = container.Build();
            d(new XContext());
        }
    }
}
