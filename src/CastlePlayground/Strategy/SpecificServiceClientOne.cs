using System;

namespace CastleExplorations
{
    public class SpecificServiceClientOne:ISpecificService,IClientOne
    {
        public void DoThing()
        {
            Console.WriteLine("Doiing thing for client one");
            ;
        }
    }
}