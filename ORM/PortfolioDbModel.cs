namespace ORM
{
    using System;
    using System.Data.Entity;
    using System.Linq;

    public class PortfolioDbModel : DbContext
    {

        public PortfolioDbModel()
            : base("name=PortfolioDbModel")
        {
        }


        public virtual DbSet<PortfolioItem> PortfolioItems { get; set; }
        public virtual DbSet<PortfolioOperation> PortfolioOperations { get; set; }

    }


}