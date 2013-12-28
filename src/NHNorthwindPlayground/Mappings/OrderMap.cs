using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace ClassLibrary1
{
    public class OrdersMap : ClassMapping<Orders>
    {
        public OrdersMap()
        {
            Schema("dbo");
            Lazy(true);
            Id(x => x.Orderid, map => map.Generator(Generators.Identity));
            Property(x => x.Orderdate);
            Property(x => x.Requireddate);
            Property(x => x.Shippeddate);
            Property(x => x.Freight);
            Property(x => x.Shipname);
            Property(x => x.Shipaddress);
            Property(x => x.Shipcity);
            Property(x => x.Shipregion);
            Property(x => x.Shippostalcode);
            Property(x => x.Shipcountry);

            Cache(cm=>cm.Usage(CacheUsage.NonstrictReadWrite));

            ManyToOne(x => x.Customers, map =>
                                        {
                                            map.Column("CustomerID");
                                            map.NotNullable(true);
                                            map.Cascade(Cascade.None);
                                        });

            Bag(x => x.OrderDetails, colmap =>
                                     {
                                         colmap.Key(x => x.Column("OrderID"));
                                         colmap.Inverse(true);
                                     }, map => { map.OneToMany(); });
        }
    }
}