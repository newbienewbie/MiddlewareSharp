using System;
using System.Threading.Tasks;
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
                    Console.WriteLine("It works - mw2");
                    await next(context);
                    Console.WriteLine("It works - mw2");
                };
            })
            .Use(async (context,next) =>{
                Console.WriteLine("It works - mw3");
                await next();
                Console.WriteLine("It works - mw3");
            })
            .Run((context) =>{
                Console.WriteLine("It works - innermost");
                Console.WriteLine("It works - innermost");
                return Task.CompletedTask;
            })
            ;

            var d = container.Build();
            d(new XContext());
        }
    }
}
