using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Middlewares
{

    /// <summary>
    /// a Container that can register a sequence of work and build a WorkDelegate
    /// </summary>
    /// <typeparam name="TWorkContext"></typeparam>
    public class WorkBuilder<TWorkContext>
        where TWorkContext : IWorkContext
    {
        private List<Func<WorkDelegate<TWorkContext>, WorkDelegate<TWorkContext>>> _middlewares = new List<Func<WorkDelegate<TWorkContext>, WorkDelegate<TWorkContext>>>();

        public WorkBuilder()
        {
        }


        /// <summary>
        /// register a basic middleware 
        ///    In a nutshell, a middleware , as the `_middlewares` indicates, is a `Func<WorkDelegate<TContext>,WorkDelegate<TContext>>`
        ///        which represents a high-order function that accepts a WorkDelegate and return a new WorkDelegate.
        ///    A middleware is used to transform the WorkDelegate.
        /// </summary>
        /// <param name="mw"></param>
        /// <returns></returns>
        public WorkBuilder<TWorkContext> Use(Func<WorkDelegate<TWorkContext>, WorkDelegate<TWorkContext>> mw)
        {
            this._middlewares.Add(mw);
            return this;
        }

        /// <summary>
        /// register a in-line style middleware
        /// </summary>
        /// <param name="mw"></param>
        /// <returns></returns>
        public WorkBuilder<TWorkContext> Use(Func<TWorkContext,Func<Task>,Task> mw){
            return this.Use(next => {
                return async context =>{
                    // a handy wrapper around the `next` WorkDelegate that captures the context by a clousure.
                    Func<Task> _next = ()=> next(context); 
                    await mw(context, _next);
                };
            });
        }

        public WorkBuilder<TWorkContext> Use<TMiddleware>()
            where TMiddleware : IWorkMiddleware<TWorkContext>
        {
            return this.Use(next =>{
                return async context =>{
                    var sp = context.ServiceProvider;
                    var middlewareInstance = sp.GetRequiredService<TMiddleware>();
                    if(middlewareInstance == null)
                    {
                        throw new NullReferenceException($"无法获取{typeof(TMiddleware).FullName}实例!");
                    }
                    await middlewareInstance.InvokeAsync(context, next);
                };
            });
        }



        /// <summary>
        /// register a middleware that runs at the end .
        /// </summary>
        /// <param name="mw"></param>
        /// <returns></returns>
        public WorkBuilder<TWorkContext> Run(Func<TWorkContext,Task> mw)
        {
            return this.Use(next=>{
                return async context =>{
                    await mw(context);
                };
            });
        }

        /// <summary>
        /// register a branch middleware , Note it will terminate the pipeline if not matched!
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="mw"></param>
        /// <returns></returns>
        public WorkBuilder<TWorkContext> MapWhen(Func<TWorkContext, Task<bool>> predicate, Func<TWorkContext,Task> mw)
        {

            return this.Use(next=> {
                return async context => {
                    var flag = await predicate(context);
                    if(flag) {
                        await mw(context);
                    }
                };
            });
        }

        /// <summary>
        /// register a branch middleware , Note it will terminate the pipeline if not matched!
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="mw"></param>
        /// <returns></returns>
        public WorkBuilder<TWorkContext> MapWhen(Func<TWorkContext, Task<bool>> predicate, Func<WorkDelegate<TWorkContext>, WorkDelegate<TWorkContext>> mw)
        {

            return this.Use(next=> {
                return async context => {
                    var flag = await predicate(context);
                    if(flag) {
                        // transform the next WorkDelegate and get a new WorkDelegate
                        var x = mw(next);  
                        await x(context);
                    }
                };
            });
        }


        /// <summary>
        /// register a branch middleware , Note it won't terminate the pipeline if not matched!
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="mw"></param>
        /// <returns></returns>
        public WorkBuilder<TWorkContext> UseWhen(Func<TWorkContext, Task<bool>> predicate, Func<TWorkContext, Func<Task>, Task> mw)
        {
            return this.Use(next => {
                return async context => {
                    var flag = await predicate(context);
                    if (flag) {
                        Func<Task> _next = () => next(context);
                        await mw(context, _next);
                    }
                    else {
                        await next(context);
                    }
                };
            });
        }

        /// <summary>
        /// register a branch middleware, won't terminate the pipepline if not matched
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="mw"></param>
        /// <returns></returns>
        public WorkBuilder<TWorkContext> UseWhen(Func<TWorkContext, Task<bool>> predicate, Func<WorkDelegate<TWorkContext>, WorkDelegate<TWorkContext>> mw)
        {
            return this.Use(next => {
                return async context => {
                    var flag = await predicate(context);
                    if (flag) {
                        var x=mw(next);
                        await x(context);
                    }
                    else {
                        await next(context);
                    }
                };
            });
        }


        /// <summary>
        /// Build a final work delegate
        /// </summary>
        /// <returns></returns>
        public WorkDelegate<TWorkContext> Build()
        {
            // add a WorkDelegate that do nothing to prevent null object error happens 
            WorkDelegate<TWorkContext> last = context => Task.CompletedTask;
            // the combined WorkDelegate
            WorkDelegate<TWorkContext> work = last;

            this._middlewares.Reverse();
            foreach(var mw in this._middlewares)
            {
                work = mw(work);
            }
            return work;
        }
    }

}
