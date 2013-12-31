using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using HibernatingRhinos.Profiler.Appender.NHibernate;
using NHibernate;
using NHibernate.Linq;
using NHNortwindPlayground.Domain;
using Xunit;

namespace NHNortwindPlayground
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
            using (ISession session = OpenNamedSession("CanGetCustomer"))
            {
                using (ITransaction tr = session.BeginTransaction())
                {
                    var cust = session.Get<Customers>("ALFKI");
                    Assert.NotNull(cust);
                    Assert.True(cust.Customerid == "ALFKI");
                    tr.Commit();
                }
            }
        }

        [Fact]
        public void Linq_CanGetCustomer()
        {
            using (ISession session = OpenNamedSession("CanGetCustomerWithLinq"))
            {
                using (ITransaction tr = session.BeginTransaction())
                {
                    Customers cust = session.Query<Customers>().Single(c => c.Customerid == "ALFKI");
                    Assert.NotNull(cust);
                    Assert.True(cust.Customerid == "ALFKI");
                    tr.Commit();
                }
            }
        }

        [Fact]
        public void Linq_CanGetCustomers()
        {
            using (ISession session = OpenNamedSession("CanGetCustomersWithLinq"))
            {
                using (ITransaction tr = session.BeginTransaction())
                {
                    List<Customers> cust = session.Query<Customers>().Where(c => c.Country == "Canada").ToList();
                    Assert.True(cust.Count > 0);
                    tr.Commit();
                }
            }
        }

        [Fact]
        public void Criteria_CanPage()
        {
            int pageSize = 5;
            int page = 2;
            using (ISession session = OpenNamedSession("CanPageWithCriteriaApi"))
            {
                ICriteria criteria = session.CreateCriteria<Customers>();
                using (ITransaction tr = session.BeginTransaction())
                {
                    ICriteria countCriteria = CriteriaTransformer.TransformToRowCount(criteria);
                    criteria.SetMaxResults(pageSize)
                        .SetFirstResult((page - 1)*pageSize);

                    IList multi = session.CreateMultiCriteria()
                        .Add(countCriteria)
                        .Add(criteria)
                        .List();

                    var result = new PagedResult<Customers>
                                 {
                                     CurrentPage = page,
                                     PageSize = pageSize,
                                     RowCount = (int) ((IList) multi[0])[0]
                                 };
                    double pageCount = (double) result.RowCount/result.PageSize;
                    result.PageCount = (int) Math.Ceiling(pageCount);
                    result.Results = ((ArrayList) multi[1]).Cast<Customers>().ToList();
                    tr.Commit();
                    Assert.NotNull(result);
                    Assert.True(result.PageSize == 5);
                    Assert.True(result.PageCount == 19);
                }
            }
        }

        [Fact]
        public void Linq_CanPage()
        {
            int pageSize = 5;
            int page = 2;
            using (ISession session = OpenNamedSession("CanPageWithLinq"))
            {
                using (ITransaction tr = session.BeginTransaction())
                {
                    IQueryable<Customers> query = session.Query<Customers>();

                    IEnumerable<Customers> futureResult =
                        query.OrderByDescending(c => c.Customerid).Skip(pageSize*(page - 1)).Take(pageSize).ToFuture();
                    IFutureValue<int> futureCount = query.ToFutureValue(c => c.Count());

                    var result = new PagedResult<Customers>
                                 {
                                     CurrentPage = page,
                                     PageSize = pageSize,
                                     RowCount = futureCount.Value
                                 };

                    double pageCount = (double) result.RowCount/result.PageSize;
                    result.PageCount = (int) Math.Ceiling(pageCount);
                    result.Results = futureResult.ToList();

                    Assert.NotNull(result);
                    Assert.True(result.PageSize == 5);
                    Assert.True(result.PageCount == 19);
                }
            }
        }

        [Fact]
        public void SLC_WillUseCachedEntity()
        {
            Customers cust;
            NHibernateHelper.ClearCaches(NHibernateHelper.SessionFactory);
            using (ISession session = OpenNamedSession("SLC_WillUseCachedEntity_UsesDb"))
            {
                using (ITransaction tr = session.BeginTransaction())
                {
                    cust = session.Get<Customers>("ALFKI");
                    Assert.NotNull(cust);
                    Assert.True(NHibernateHelper.SessionFactory.Statistics.SecondLevelCacheMissCount > 0);
                    tr.Commit();
                }
            }

            using (ISession session = OpenNamedSession("SLC_WillUseCachedEntity_UsesCache"))
            {
                using (ITransaction tr = session.BeginTransaction())
                {
                    cust = session.Get<Customers>("ALFKI");
                    Assert.NotNull(cust);
                    Assert.True(NHibernateHelper.SessionFactory.Statistics.SecondLevelCacheHitCount > 0);
                    tr.Commit();
                }
            }
        }

        [Fact]
        public void SLC_CachedEntityExpires()
        {
            Orders ord;
            NHibernateHelper.ClearCaches(NHibernateHelper.SessionFactory);
            using (ISession session = OpenNamedSession("SLC_CachedEntityExpires_UsesDb"))
            {
                using (ITransaction tr = session.BeginTransaction())
                {
                    ord = session.Query<Orders>().FirstOrDefault(o=>o.Customers.Customerid=="ALFKI");
                    Assert.NotNull(ord);
                    Assert.True(NHibernateHelper.SessionFactory.Statistics.SecondLevelCacheMissCount > 0);
                    tr.Commit();
                }
            }

            Thread.Sleep(6000);
            using (ISession session = OpenNamedSession("SLC_CachedEntityExpires_UsesDbAgain"))
            {
                using (ITransaction tr = session.BeginTransaction())
                {
                    ord = session.Query<Orders>().FirstOrDefault(o => o.Customers.Customerid == "ALFKI");
                    Assert.NotNull(ord);
                    Assert.True(NHibernateHelper.SessionFactory.Statistics.SecondLevelCacheMissCount > 0);
                    tr.Commit();
                }
            }
        }

        [Fact]
        public void SLC_SqlDependencyInvalidatesCache()
        {
            Customers cust;
            NHibernateHelper.ClearCaches(NHibernateHelper.SessionFactory);
            using (ISession session = OpenNamedSession("SqlDependency_UsesDb"))
            {
                using (ITransaction tr = session.BeginTransaction())
                {
                    cust = session.Get<Customers>("ALFKI");
                    tr.Commit();
                    Assert.NotNull(cust);
                    Assert.True(NHibernateHelper.SessionFactory.Statistics.SecondLevelCacheMissCount > 0);
                    
                }
            }
            using (ISession session = OpenNamedSession("SqlDependency_UsesCache"))
            {
                using (ITransaction tr = session.BeginTransaction())
                {
                    cust = session.Get<Customers>("ALFKI");
                    tr.Commit();
                    Assert.NotNull(cust);
                    Assert.True(NHibernateHelper.SessionFactory.Statistics.SecondLevelCacheHitCount > 0);
                }
            }

            UpdateReqionSql("EU");

            using (ISession session = OpenNamedSession("SqlDependency_shouldReCache"))
            {
                using (ITransaction tr = session.BeginTransaction())
                {
                    cust = session.Get<Customers>("ALFKI");
                    Assert.NotNull(cust);
                    Assert.True(NHibernateHelper.SessionFactory.Statistics.SecondLevelCacheMissCount > 0);
                    tr.Commit();
                }
            }
        }


        private void UpdateReqionSql(string region)
        {
            using (ISession session = OpenNamedSession("sqlUpdate"))
            {
                using (ITransaction tr = session.BeginTransaction())
                {
                    session
                        .CreateSQLQuery("update Customers set Region=:region where CustomerId = 'ALFKI'")
                        .SetString("region", region)
                        .ExecuteUpdate();
                    tr.Commit();
                }
            }
        }

        internal ISession OpenNamedSession(string name)
        {
            var session = NHibernateHelper.SessionFactory.OpenSession();
            NHibernateProfiler.RenameSessionInProfiler(session, name);
            return session;
        }
    }
}