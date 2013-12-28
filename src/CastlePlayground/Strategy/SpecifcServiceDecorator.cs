using System;

namespace CastleExplorations
{
    public class SpecifcServiceDecorator:ISpecificService
    {
        internal ISpecificService wrappedSvc;

        public SpecifcServiceDecorator(ISpecificService svc)
        {
            wrappedSvc = svc;
        }

        public void DoThing()
        {
            Console.WriteLine("Doing things before the wrapper");
            wrappedSvc.DoThing();
        }
    }
}