using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Team16LonghornBank.Models
{
    public class Payee
    {
        public Int32 PayeeID { get; set; }

        [Required]
        public String Name { get; set; }

        [Required]
        public String Address { get; set; }

        [Required]
        public String City { get; set; }

        [Required]
        public String State { get; set; }

        [Required]
        [Display(Name = "Zip Code")]
        public String ZipCode { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public String PhoneNumber { get; set; }

        public String PayeeType { get; set; }

        //Navigational properties
        public virtual List<AppUser> Customers { get; set; }
        public virtual List<Transaction> Transactions { get; set; }
    }
}