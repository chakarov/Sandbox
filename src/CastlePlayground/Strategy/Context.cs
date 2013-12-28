using System;

namespace CastleExplorations
{
    public interface IContext
    {
        string Name { get; set; }
    }

    public class Context : IContext
    {
        [ThreadStatic]
        private static Context currentContext;

        public string Name { get; set; }

        public static Context CurrentContext
        {
            get { return currentContext; }
            set { currentContext = value; }
        }
    }
}