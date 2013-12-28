using System;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;

namespace CastleExplorations.Caching
{
    public class CacheActionAttribute : Attribute
    {

    }

    interface ICacheKeyGenerator
    {
        string Generate();
    }
}