using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// a dummy interface 
public interface IContext { }

public delegate Task WorkDelegate(IContext context);

public class Container
{
    private List<Func<WorkDelegate, WorkDelegate>> _middlewares = new List<Func<WorkDelegate, WorkDelegate>>();

    public Container Use(Func<WorkDelegate, WorkDelegate> mw)
    {
        this._middlewares.Add(mw);
        return this;
    }

    public WorkDelegate Build()
    {
        // add a WorkDelegate that do nothing to prevent null object error happens 
        WorkDelegate last = context => Task.CompletedTask;
        // the combined WorkDelegate
        WorkDelegate work = last;

        this._middlewares.Reverse();
        foreach(var mw in this._middlewares)
        {
            work = mw(work);
        }
        return work;
    }
}