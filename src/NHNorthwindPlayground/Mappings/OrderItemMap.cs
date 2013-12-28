using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace ClassLibrary1
{
    public class OrderDetailsMap : ClassMapping<OrderDetails>
    {

        public OrderDetailsMap()
        {
            Table("Order Details");
            Schema("dbo");
            Lazy(true);
            ComposedId(compId =>
            {
                compId.Property(x => x.Orderid, m => m.Column("OrderID"));
                compId.Property(x => x.Productid, m => m.Column("ProductID"));
            });
            Property(x => x.Unitprice, map => map.NotNullable(true));
            Property(x => x.Quantity, map => map.NotNullable(true));
            Property(x => x.Discount, map => map.NotNullable(true));
            Cache(cm=>cm.Usage(CacheUsage.NonstrictReadWrite));

            ManyToOne(x => x.Orders, map =>
            {
                map.Column("OrderID");
                map.PropertyRef("Orderid");
                map.Cascade(Cascade.None);
            });

            ManyToOne(x => x.Products, map =>
            {
                map.Column("ProductID");
                map.PropertyRef("Productid");
                map.Cascade(Cascade.None);
            });

        }
    }
}