using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHNortwindPlayground.Domain;

namespace NHNortwindPlayground.Mappings
{
    public class ProductsMap : ClassMapping<Products>
    {
        public ProductsMap()
        {
            Schema("dbo");
            Lazy(true);
            Id(x => x.Productid, map => map.Generator(Generators.Identity));
            Property(x => x.Productname, map => map.NotNullable(true));
            Property(x => x.Quantityperunit);
            Property(x => x.Unitprice);
            Property(x => x.Unitsinstock);
            Property(x => x.Unitsonorder);
            Property(x => x.Reorderlevel);
            Property(x => x.Discontinued, map => map.NotNullable(true));
            Cache(cm => cm.Usage(CacheUsage.NonstrictReadWrite));
        }
    }
}