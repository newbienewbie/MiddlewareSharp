using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Itminus.Middlewares
{

    /// <summary>
    /// a delegate that deal with context
    /// </summary>
    /// <typeparam name="TWorkContext"></typeparam>
    /// <param name="context"></param>
    /// <returns></returns>
    public delegate Task WorkDelegate<TWorkContext>(TWorkContext context)
        where TWorkContext : IWorkContext ;
}
