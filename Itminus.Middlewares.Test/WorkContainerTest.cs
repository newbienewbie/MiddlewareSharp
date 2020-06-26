using System;
using Xunit;
using Itminus.Middlewares;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Itminus.Middlewares.Test
{
    public class WorkContainerUnitTest1
    {
        private class MyWorkContext : IWorkContext
        {
            public IServiceProvider ServiceProvider { get; private set; }
            public IList<string> Data{get;set;} = new List<string>();
        }

        private readonly WorkBuilder<MyWorkContext>  WorkContainer;

        public WorkContainerUnitTest1(){
            this.WorkContainer= new WorkBuilder<MyWorkContext>();
        }

        private string MessageCall(int i , bool before) {
            var call = before ? "calling" : "called";
            return $"mw{i}-{call}";
        }


        [Fact]
        public void TestUseWhenAndMapWhen()
        {
            var container = new WorkBuilder<MyWorkContext>();
            Func<MyWorkContext, Task<bool>> predicateTrue = context => Task.FromResult(true);
            Func<MyWorkContext, Task<bool>> predicateFalse = context => Task.FromResult(false);

            container.UseWhen( predicateTrue, next => {
                return async context => {
                    context.Data.Add(MessageCall(1, true));
                    await next(context);
                    context.Data.Add(MessageCall(1, false));
                };
            })
            .UseWhen( predicateFalse, next => {
                return async context => {
                    context.Data.Add(MessageCall(2, true));
                    await next(context);
                    context.Data.Add(MessageCall(2, false));
                };
            })
            .UseWhen( predicateTrue, async (context,next) => {
                context.Data.Add(MessageCall(3, true));
                await next();
                context.Data.Add(MessageCall(3, false));
            })
            .UseWhen( predicateFalse, async (context, next) => {
                context.Data.Add(MessageCall(4, true));
                await next();
                context.Data.Add(MessageCall(4, false));
            })
            .MapWhen( predicateTrue, (context) => {
                context.Data.Add(MessageCall(5, false));
                return Task.CompletedTask;
            })
            .MapWhen( predicateFalse, (context) => {
                context.Data.Add(MessageCall(6, false));
                return Task.CompletedTask;
            })
            .MapWhen( predicateTrue, (context) => {
                context.Data.Add(MessageCall(7, false));
                return Task.CompletedTask;
            })
            ;
            
            var d = container.Build();
            var _context = new MyWorkContext { 
                Data = new List<string>() 
            };
            d(_context);
            Assert.Equal(5, _context.Data.Count);
            Assert.Equal(MessageCall(1, true), _context.Data[0]);
            Assert.Equal(MessageCall(3, true), _context.Data[1]);
            Assert.Equal(MessageCall(5, false), _context.Data[2]);
            Assert.Equal(MessageCall(3, false), _context.Data[3]);
            Assert.Equal(MessageCall(1, false), _context.Data[4]);
        }

        [Fact]
        public void TestUseAndRun()
        {
            var container = new WorkBuilder<MyWorkContext>();
            container.Use(next =>
            {
                return async context =>
                {
                    context.Data.Add(MessageCall(1,true));
                    await next(context);
                    context.Data.Add(MessageCall(1,false));
                };
            })
            .Use(next =>
            {
                return async context =>
                {
                    context.Data.Add(MessageCall(2,true));
                    await next(context);
                    context.Data.Add(MessageCall(2,false));
                };
            })
            .Use(async (context, next) =>
            {
                context.Data.Add(MessageCall(3,true));
                await next();
                context.Data.Add(MessageCall(3,false));
            })
            .Run((context) =>
            {
                context.Data.Add(MessageCall(4,true));
                return Task.CompletedTask;
            });

            var d = container.Build();
            var _context = new MyWorkContext { Data = new List<string>() };
            d(_context);
            Assert.Equal(7,_context.Data.Count);
            Assert.Equal(MessageCall(1,true),_context.Data[0]);
            Assert.Equal(MessageCall(2,true),_context.Data[1]);
            Assert.Equal(MessageCall(3,true),_context.Data[2]);
            Assert.Equal(MessageCall(4,true),_context.Data[3]);
            Assert.Equal(MessageCall(3,false),_context.Data[4]);
            Assert.Equal(MessageCall(2,false),_context.Data[5]);
            Assert.Equal(MessageCall(1,false),_context.Data[6]);
        }
    }

}
