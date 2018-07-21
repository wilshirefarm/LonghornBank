using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Team16LonghornBank.Models
{
    public class StockPurchase
    {
        public Int32 StockPurchaseID { get; set; }

        [Display(Name = "Purchase Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime PurchaseDate { get; set; }

        [Display(Name = "Initial Share Price")]
        [DataType(DataType.Currency)]
        public Decimal InitialSharePrice { get; set; }

        [Display(Name = "Number of shares")]
        public Int64 NumberOfShares { get; set; }

        [Display(Name = "Total Stock Value")]
        [DataType(DataType.Currency)]
        public Decimal TotalStockValue { get; set; }

        [Display(Name = "Change in price")]
        [DataType(DataType.Currency)]
        public Decimal ChangeInPrice { get; set; }

        [Display(Name = "Total Change")]
        [DataType(DataType.Currency)]
        public Decimal TotalChange { get; set; }

        public String StockPurchaseDisplay { get; set; }


        //navigational properties
        public virtual StockPortfolio StockPortfolio { get; set; }
        public virtual Stock Stock { get; set; }
    }
}