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

        /// <summary>
        /// register a basic middleware
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
