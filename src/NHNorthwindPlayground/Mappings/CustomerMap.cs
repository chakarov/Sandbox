using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHNortwindPlayground.Domain;

namespace NHNortwindPlayground.Mappings
{
    public class CustomersMap : ClassMapping<Customers>
    {
        public CustomersMap()
        {
            Schema("dbo");
            Lazy(true);
            Id(x => x.Customerid, map => map.Generator(Generators.Assigned));
            Property(x => x.Companyname, map => map.NotNullable(true));
            Property(x => x.Contactname);
            Property(x => x.Contacttitle);
            Property(x => x.Address);
            Property(x => x.City);
            Property(x => x.Region);
            Property(x => x.Postalcode);
            Property(x => x.Country);
            Property(x => x.Phone);
            Property(x => x.Fax);

            Cache(cm =>
                  {
                      cm.Region("DefaultCache");
                      cm.Usage(CacheUsage.NonstrictReadWrite);
                  });

            Bag(x => x.Orders, colmap =>
                               {
                                   colmap.Key(x => x.Column("CustomerID"));
                                   colmap.Inverse(true);
                               }, map => { map.OneToMany(); });
        }
    }
}