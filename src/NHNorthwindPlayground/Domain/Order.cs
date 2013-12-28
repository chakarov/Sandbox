using System;
using System.Collections.Generic;

namespace ClassLibrary1
{
    public class Orders
    {
        public virtual int Orderid { get; set; }
        public virtual Customers Customers { get; set; }
        public virtual DateTime? Orderdate { get; set; }
        public virtual DateTime? Requireddate { get; set; }
        public virtual DateTime? Shippeddate { get; set; }
        public virtual decimal? Freight { get; set; }
        public virtual string Shipname { get; set; }
        public virtual string Shipaddress { get; set; }
        public virtual string Shipcity { get; set; }
        public virtual string Shipregion { get; set; }
        public virtual string Shippostalcode { get; set; }
        public virtual string Shipcountry { get; set; }
        public virtual IEnumerable<OrderDetails> OrderDetails { get; set; }
    }
}