using System;

namespace MiddwarePrototype
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = new Container();
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
            d(new object() as IContext);
        }
    }
}
