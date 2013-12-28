using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate;
using NHibernate.Bytecode;
using NHibernate.Caches.SysCache3;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;

namespace ClassLibrary1
{
    public class NHibernateHelper
    {
        public static ISessionFactory SessionFactory;

        static NHibernateHelper()
        {
            SessionFactory = Create();
        }

        public static void ClearCaches(ISessionFactory factory)
        {
            factory.EvictQueries();
            foreach (var collectionMetadata in factory.GetAllCollectionMetadata())
            {
                factory.EvictCollection(collectionMetadata.Key);
            }
            foreach (var classMetadata in factory.GetAllClassMetadata())
            {
                factory.EvictEntity(classMetadata.Key);
            }
        }

        private static ISessionFactory Create()
        {
            var mapper = new ModelMapper();
            mapper.AddMappings(typeof (CustomersMap).Assembly.GetTypes());

            var cfg = new Configuration();
            cfg
                .Proxy(x => x.ProxyFactoryFactory<DefaultProxyFactoryFactory>())
                .DataBaseIntegration(db =>
                                     {
                                         db.ConnectionStringName = "db";
                                         db.Dialect<MsSql2008Dialect>();
                                         db.LogSqlInConsole = true;
                                         db.LogFormattedSql = true;
                                     })
                .Cache(c =>
                       {
                           c.UseQueryCache = true;
                           c.Provider<SysCacheProvider>();
                       })
                ;
            cfg.AddMapping(mapper.CompileMappingForAllExplicitlyAddedEntities());
            cfg.SessionFactory().GenerateStatistics();
            return cfg.BuildSessionFactory();
        }
    }

    public static class Extensions
    {
        public static IFutureValue<TResult> ToFutureValue<TSource, TResult>(
            this IQueryable<TSource> source,
            Expression<Func<IQueryable<TSource>, TResult>> selector)
            where TResult : struct
        {
            var provider = (INhQueryProvider) source.Provider;
            MethodInfo method = ((MethodCallExpression) selector.Body).Method;
            MethodCallExpression expression = Expression.Call(null, method, source.Expression);
            return (IFutureValue<TResult>) provider.ExecuteFuture(expression);
        }
    }
}