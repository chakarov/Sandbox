using System.Collections.Generic;

namespace ClassLibrary1
{
    public class Customers
    {
        public virtual string Address { get; set; }

        public virtual string City { get; set; }

        public virtual string Companyname { get; set; }

        public virtual string Contactname { get; set; }

        public virtual string Contacttitle { get; set; }

        public virtual string Country { get; set; }

        public virtual string Customerid { get; set; }
        public virtual string Fax { get; set; }

        public virtual IEnumerable<Orders> Orders { get; set; }

        public virtual string Phone { get; set; }

        public virtual string Postalcode { get; set; }

        public virtual string Region { get; set; }
    }
}