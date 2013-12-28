namespace NHNortwindPlayground.Domain
{
    public class Products
    {
        public virtual int Productid { get; set; }
        public virtual string Productname { get; set; }
        public virtual string Quantityperunit { get; set; }
        public virtual decimal? Unitprice { get; set; }
        public virtual short? Unitsinstock { get; set; }
        public virtual short? Unitsonorder { get; set; }
        public virtual short? Reorderlevel { get; set; }
        public virtual bool Discontinued { get; set; }
    }
}