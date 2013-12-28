using System;

namespace CastleExplorations
{
    public class SpecificServiceClientTwo: ISpecificService, IClientTwo
    {
        public void DoThing()
        {
            Console.WriteLine("Doing thing for client two");
        }
    }
}