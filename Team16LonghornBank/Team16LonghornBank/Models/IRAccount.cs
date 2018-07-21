using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Team16LonghornBank.Models
{

    public class IRAccount
        {

        //Properties
        public Int32 IRAccountID { get; set; }

        [Display(Name ="Account Number")]
        public Int32 AccountNumber { get; set; }

        [Display(Name = "Account Number")]
        public String FourDigitNumber
        {
            get { return "XXXXXXX" + AccountNumber.ToString().Substring(6); }
        }

        [Display(Name ="Account Name")]
        public String AccountName { get; set; }

        [DataType(DataType.Currency)]
        public Decimal Balance { get; set; }

        [DataType(DataType.Currency)]
        public Decimal MaxContribution { get; set; }
        
        public Boolean isQualified { get; set; }
        public String IRAccountDisplay
        {
            get { return AccountName + ", Account Number: XXXXXX" + AccountNumber.ToString().Substring(6) + ", Balance: $" + Balance.ToString(); }
        }
        //navigational properties
        public virtual AppUser Customer { get; set; }
        public virtual List<Transaction> Transactions { get; set; }
    }

}

