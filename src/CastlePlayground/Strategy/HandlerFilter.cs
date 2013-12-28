using System;
using Castle.MicroKernel;
using System.Linq;

namespace CastleExplorations
{
    public class HandlerFilter: IHandlersFilter
    {
        public bool HasOpinionAbout(Type service)
        {
            return typeof (IService).IsAssignableFrom(service);
        }

        public IHandler[] SelectHandlers(Type service, IHandler[] handlers)
        {
            var client = Context.CurrentContext.Name;
            var typeToRemove = client == "ClientOne" ? typeof (IClientTwo) : typeof(IClientOne);
            var remove = handlers.Where(h => typeToRemove.IsAssignableFrom(h.ComponentModel.Implementation));
            return handlers.Except(remove).ToArray();
        }
    }
}