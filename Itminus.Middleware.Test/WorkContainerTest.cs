using System;
using Xunit;
using Itminus.Middleware;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Itminus.Middleware.Test
{
    public class WorkContainerUnitTest1
    {

        private readonly WorkContainer<Dictionary<string,string>>  WorkContainer;

        public WorkContainerUnitTest1(){
            this.WorkContainer= new WorkContainer<Dictionary<string,string>>();
        }

        private string MessageCall(int i , bool before) {
            var call = before ? "calling" : "called";
            return $"mw{i}-{call}";
        }


        [Fact]
        public void TestUseWhenAndMapWhen()
        {
            var container = new WorkContainer<List<string>>();
            Func<List<string>, Task<bool>> predicateTrue = async context => await Task.FromResult<bool>(true);
            Func<List<string>, Task<bool>> predicateFalse = async context => await Task.FromResult<bool>(false);

            container.UseWhen( predicateTrue, next => {
                return async context => {
                    context.Add(MessageCall(1, true));
                    await next(context);
                    context.Add(MessageCall(1, false));
                };
            })
            .UseWhen( predicateFalse, next => {
                return async context => {
                    context.Add(MessageCall(2, true));
                    await next(context);
                    context.Add(MessageCall(2, false));
                };
            })
            .UseWhen( predicateTrue, async (context,next) => {
                context.Add(MessageCall(3, true));
                await next();
                context.Add(MessageCall(3, false));
            })
            .UseWhen( predicateFalse, async (context, next) => {
                context.Add(MessageCall(4, true));
                await next();
                context.Add(MessageCall(4, false));
            })
            .MapWhen( predicateTrue, (context) => {
                context.Add(MessageCall(5, false));
                return Task.CompletedTask;
            })
            .MapWhen( predicateFalse, (context) => {
                context.Add(MessageCall(6, false));
                return Task.CompletedTask;
            })
            .MapWhen( predicateTrue, (context) => {
                context.Add(MessageCall(7, false));
                return Task.CompletedTask;
            })
            ;
            
            var d = container.Build();
            var _context = new List<string>();
            d(_context);
            Assert.Equal(5, _context.Count);
            Assert.Equal(MessageCall(1, true), _context[0]);
            Assert.Equal(MessageCall(3, true), _context[1]);
            Assert.Equal(MessageCall(5, false), _context[2]);
            Assert.Equal(MessageCall(3, false), _context[3]);
            Assert.Equal(MessageCall(1, false), _context[4]);
        }

        [Fact]
        public void TestUseAndRun()
        {
            var container = new WorkContainer<List<string>>();
            container.Use(next =>
            {
                return async context =>
                {
                    context.Add(MessageCall(1,true));
                    await next(context);
                    context.Add(MessageCall(1,false));
                };
            })
            .Use(next =>
            {
                return async context =>
                {
                    context.Add(MessageCall(2,true));
                    await next(context);
                    context.Add(MessageCall(2,false));
                };
            })
            .Use(async (context, next) =>
            {
                context.Add(MessageCall(3,true));
                await next();
                context.Add(MessageCall(3,false));
            })
            .Run((context) =>
            {
                context.Add(MessageCall(4,true));
                return Task.CompletedTask;
            });

            var d = container.Build();
            var _context = new List<string>();
            d(_context);
            Assert.Equal(7,_context.Count);
            Assert.Equal(MessageCall(1,true),_context[0]);
            Assert.Equal(MessageCall(2,true),_context[1]);
            Assert.Equal(MessageCall(3,true),_context[2]);
            Assert.Equal(MessageCall(4,true),_context[3]);
            Assert.Equal(MessageCall(3,false),_context[4]);
            Assert.Equal(MessageCall(2,false),_context[5]);
            Assert.Equal(MessageCall(1,false),_context[6]);
        }
    }

}
