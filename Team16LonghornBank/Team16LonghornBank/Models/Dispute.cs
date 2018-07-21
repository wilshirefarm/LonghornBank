using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace Team16LonghornBank.Models
{
    public class Dispute
    {
        //Dispute sends message to managers asking them to review transaction

        //Creates a DisputeID
        [Key]
        public Int32 DisputeID { get; set; }

        //Requires a comment
        [Required(ErrorMessage = "Comments are required.")]
        [Display(Name = "Comments for dispute")]
        public String DisputeComments { get; set; }

        //Requires the transaction amount name
        [DataType(DataType.Currency)]
        [Display(Name = "Corrected tansaction amount")]
        public Decimal CorrectAmount { get; set; }

        //Requires option of deleting transaction
        [Required(ErrorMessage = "Must pick an option")]
        [Display(Name = "Delete transaction?")]
        public Boolean DeleteTransaction { get; set; }

        public String CurrentStatus { get; set; }

        //Navigational property for one-to-one relationship
        public virtual Transaction Transaction { get; set; }

        
    }
}