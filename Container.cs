using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Itminus.Middleware{

    public delegate Task WorkDelegate<TContext>(TContext context);

    public class WorkContainer<TContext>
    {
        private List<Func<WorkDelegate<TContext>, WorkDelegate<TContext>>> _middlewares = new List<Func<WorkDelegate<TContext>, WorkDelegate<TContext>>>();

        public WorkContainer<TContext> Use(Func<WorkDelegate<TContext>, WorkDelegate<TContext>> mw)
        {
            this._middlewares.Add(mw);
            return this;
        }

        public WorkContainer<TContext> Use(Func<TContext,Func<Task>,Task> mw){
            return this.Use(next => {
                return async context =>{
                    Func<Task> _next = ()=> next(context); 
                    await mw(context, _next);
                };
            });
        }

        public WorkContainer<TContext> Run(Func<TContext,Task> mw)
        {
            return this.Use(next=>{
                return async context =>{
                    await mw(context);
                };
            });
        }

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
