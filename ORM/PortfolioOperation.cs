namespace ORM
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PortfolioOperation
    {
        public PortfolioOperation() { }
        public PortfolioOperation(PortfolioItem item)
        {
            this.Symbol = item.Symbol;
            this.UserId = item.UserId;
            this.SharesNumber = item.SharesNumber;            
        }
        [Required]
        [StringLength(10)]
        public string Symbol { get; set; }

        [StringLength(10)]
        public string Operation { get; set; }

        public int UserId { get; set; }

        public int? SharesNumber { get; set; }

        public int Id { get; set; }
    }
}