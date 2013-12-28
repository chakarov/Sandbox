using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using HibernatingRhinos.Profiler.Appender.NHibernate;
using NHibernate;
using NHibernate.Hql.Ast.ANTLR;
using NHibernate.Linq;
using Xunit;

namespace ClassLibrary1
{
    public class Test
    {
        public Test()
        {
            NHibernateProfiler.Initialize();
        }

        [Fact]
        public void CanGetCustomer()
        {
            ISession session = NHibernateHelper.SessionFactory.OpenSession();
            var cust = session.Get<Customers>("ALFKI");
            Assert.NotNull(cust);
            Assert.True(cust.Customerid == "ALFKI");
        }

        [Fact]
        public void Linq_CanGetCustomer()
        {
            ISession session = NHibernateHelper.SessionFactory.OpenSession();
            Customers cust = session.Query<Customers>().Single(c => c.Customerid == "ALFKI");
            Assert.NotNull(cust);
            Assert.True(cust.Customerid == "ALFKI");
        }

        [Fact]
        public void Linq_CanGetCustomers()
        {
            ISession session = NHibernateHelper.SessionFactory.OpenSession();
            List<Customers> cust = session.Query<Customers>().Where(c => c.Country == "Canada").ToList();
            Assert.True(cust.Count > 0);
        }

        [Fact]
        public void Criteria_CanPage()
        {
            var pageSize = 5;
            var page = 2;
            ISession session = NHibernateHelper.SessionFactory.OpenSession();
            var criteria = session.CreateCriteria<Customers>();
            using (ITransaction tr = session.BeginTransaction())
            {
                var countCriteria = CriteriaTransformer.TransformToRowCount(criteria);
                criteria.SetMaxResults(pageSize)
                        .SetFirstResult((page - 1) * pageSize);

                var multi = session.CreateMultiCriteria()
                            .Add(countCriteria)
                            .Add(criteria)
                            .List();

                var result = new PagedResult<Customers>
                             {
                                 CurrentPage = page,
                                 PageSize = pageSize,
                                 RowCount = (int) ((IList) multi[0])[0]
                             };
                var pageCount = (double)result.RowCount / result.PageSize;
                result.PageCount = (int)Math.Ceiling(pageCount);
                result.Results = ((ArrayList)multi[1]).Cast<Customers>().ToList();
                tr.Commit();
                Assert.NotNull(result);
                Assert.True(result.PageSize==5);
                Assert.True(result.PageCount == 19);
            }
        }

        [Fact]
        public void Linq_CanPage()
        {
            var pageSize = 5;
            var page = 2;
            ISession session = NHibernateHelper.SessionFactory.OpenSession();
            using (ITransaction tr = session.BeginTransaction())
            {
                IQueryable<Customers> query = session.Query<Customers>();

                IEnumerable<Customers> futureResult =
                    query.OrderByDescending(c => c.Customerid).Skip(pageSize*(page-1)).Take(pageSize).ToFuture();
                IFutureValue<int> futureCount = query.ToFutureValue(c => c.Count());

                var result = new PagedResult<Customers>
                {
                    CurrentPage = page,
                    PageSize = pageSize,
                    RowCount = futureCount.Value
                };
                
                var pageCount = (double)result.RowCount / result.PageSize;
                result.PageCount = (int)Math.Ceiling(pageCount);
                result.Results = futureResult.ToList();

                Assert.NotNull(result);
                Assert.True(result.PageSize==5);
                Assert.True(result.PageCount == 19);
            }
        }

        [Fact]
        public void SLC_WillUseCachedEntity()
        {
            Customers cust;
            NHibernateHelper.ClearCaches(NHibernateHelper.SessionFactory);
            using (var session = NHibernateHelper.SessionFactory.OpenSession())
            {
                cust = session.Get<Customers>("ALFKI");
                Assert.NotNull(cust);
                Assert.True(NHibernateHelper.SessionFactory.Statistics.SecondLevelCacheMissCount > 0);
            }

            using (var session = NHibernateHelper.SessionFactory.OpenSession())
            {
                cust = session.Get<Customers>("ALFKI");
                Assert.NotNull(cust);
                Assert.True(NHibernateHelper.SessionFactory.Statistics.SecondLevelCacheHitCount > 0);
            }
        }

        [Fact]
        public void SLC_CachedEntityExpires()
        {
            Customers cust;
            NHibernateHelper.ClearCaches(NHibernateHelper.SessionFactory);
            using (var session = NHibernateHelper.SessionFactory.OpenSession())
            {
                cust = session.Get<Customers>("ALFKI");
                Assert.NotNull(cust);
                Assert.True(NHibernateHelper.SessionFactory.Statistics.SecondLevelCacheMissCount > 0);
            }

            Thread.Sleep(6000);
            using (var session = NHibernateHelper.SessionFactory.OpenSession())
            {
                cust = session.Get<Customers>("ALFKI");
                Assert.NotNull(cust);
                Assert.True(NHibernateHelper.SessionFactory.Statistics.SecondLevelCacheMissCount > 0);
            }
        }

        [Fact]
        public void SLC_SqlDependencyInvalidatesCache()
        {
            var conn = ConfigurationManager.ConnectionStrings["db"].ConnectionString;
            UpdateReqionSql(null);
            SqlDependency.Start(conn);

            Customers cust;
            NHibernateHelper.ClearCaches(NHibernateHelper.SessionFactory);
            using (var session = NHibernateHelper.SessionFactory.OpenSession())
            {
                cust = session.Get<Customers>("ALFKI");
                Assert.NotNull(cust);
                Assert.True(NHibernateHelper.SessionFactory.Statistics.SecondLevelCacheMissCount > 0);
            }
            using (var session = NHibernateHelper.SessionFactory.OpenSession())
            {
                cust = session.Get<Customers>("ALFKI");
                Assert.NotNull(cust);
                Assert.True(NHibernateHelper.SessionFactory.Statistics.SecondLevelCacheHitCount > 0);
            }
            
            UpdateReqionSql("EU");

            using (var session = NHibernateHelper.SessionFactory.OpenSession())
            {
                cust = session.Get<Customers>("ALFKI");
                Assert.NotNull(cust);
                Assert.True(NHibernateHelper.SessionFactory.Statistics.SecondLevelCacheMissCount > 0);
            }

            SqlDependency.Stop(conn);
        }

        private void UpdateReqionSql(string region)
        {
            using (var session = NHibernateHelper.SessionFactory.OpenSession())
            {
                session
                    .CreateSQLQuery("update Customers set Region=:region where CustomerId = 'ALFKI'")
                    .SetString("region",region)
                    .ExecuteUpdate();
            }
        }
    }
}