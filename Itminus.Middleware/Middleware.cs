using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Itminus.Middleware{

    /// <summary>
    /// a delegate that deal with context
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    /// <param name="context"></param>
    /// <returns></returns>
    public delegate Task WorkDelegate<TContext>(TContext context);

    /// <summary>
    /// a Container that can register a sequence of work and build a WorkDelegate
    /// </summary>
    /// <typeparam name="TContext"></typeparam>
    public class WorkContainer<TContext>
    {
        private List<Func<WorkDelegate<TContext>, WorkDelegate<TContext>>> _middlewares = new List<Func<WorkDelegate<TContext>, WorkDelegate<TContext>>>();

        public WorkContainer()
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
        public WorkContainer<TContext> Use(Func<WorkDelegate<TContext>, WorkDelegate<TContext>> mw)
        {
            this._middlewares.Add(mw);
            return this;
        }

        /// <summary>
        /// register a in-line style middleware
        /// </summary>
        /// <param name="mw"></param>
        /// <returns></returns>
        public WorkContainer<TContext> Use(Func<TContext,Func<Task>,Task> mw){
            return this.Use(next => {
                return async context =>{
                    // a handy wrapper around the `next` WorkDelegate that captures the context by a clousure.
                    Func<Task> _next = ()=> next(context); 
                    await mw(context, _next);
                };
            });
        }



        /// <summary>
        /// register a middleware that runs at the end .
        /// </summary>
        /// <param name="mw"></param>
        /// <returns></returns>
        public WorkContainer<TContext> Run(Func<TContext,Task> mw)
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
        public WorkContainer<TContext> MapWhen(Func<TContext, Task<bool>> predicate, Func<TContext,Task> mw)
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
        public WorkContainer<TContext> MapWhen(Func<TContext, Task<bool>> predicate, Func<WorkDelegate<TContext>, WorkDelegate<TContext>> mw)
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
        public WorkContainer<TContext> UseWhen(Func<TContext, Task<bool>> predicate, Func<TContext, Func<Task>, Task> mw)
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
        public WorkContainer<TContext> UseWhen(Func<TContext, Task<bool>> predicate, Func<WorkDelegate<TContext>, WorkDelegate<TContext>> mw)
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
        public WorkDelegate<TContext> Build()
        {
            // add a WorkDelegate that do nothing to prevent null object error happens 
            WorkDelegate<TContext> last = context => Task.CompletedTask;
            // the combined WorkDelegate
            WorkDelegate<TContext> work = last;

            this._middlewares.Reverse();
            foreach(var mw in this._middlewares)
            {
                work = mw(work);
            }
            return work;
        }
    }

}
