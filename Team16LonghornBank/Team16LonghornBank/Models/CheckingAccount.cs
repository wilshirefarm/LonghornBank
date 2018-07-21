using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Team16LonghornBank.Models
{
    public class CheckingAccount
    {
        public Int32 CheckingAccountID { get; set; }

        [Display(Name = "Account Number")]
        public Int32 AccountNumber { get; set; }

        [Display(Name = "Account Number")]
        public String FourDigitNumber
        {
            get { return "XXXXXX" + AccountNumber.ToString().Substring(6); }
        }

        [Display(Name = "Account Name")]
        public String AccountName { get; set; }

        [DataType(DataType.Currency)]
        public Decimal Balance { get; set; }

        public String CheckingAccountDisplay
        {
            get { return AccountName + ", Account Number: XXXXXX" + AccountNumber.ToString().Substring(6) + ", Balance: $" + Balance.ToString(); }
        }

        //navigational properties
        public virtual AppUser Customer { get; set; }
        public virtual List<Transaction> Transactions { get; set; }
    }
}