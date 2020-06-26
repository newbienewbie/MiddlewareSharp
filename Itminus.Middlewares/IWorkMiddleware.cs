using System.Threading.Tasks;

namespace Itminus.Middlewares
{
    public interface IWorkMiddleware<TWorkConext>
        where TWorkConext : IWorkContext
    {
        Task InvokeAsync(TWorkConext context, WorkDelegate<TWorkConext> next);
    }

}
