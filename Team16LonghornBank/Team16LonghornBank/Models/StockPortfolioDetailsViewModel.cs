﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;


namespace Team16LonghornBank.Models
{
    public class StockPortfolioDetailsViewModel
    {
        public int? StockPortfolioID { get; set; }
        public StockPortfolio StockPortfolio { get; set; }

        public List<StockPurchase> StockPurchases { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateFrom { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime DateTo { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}