using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Team16LonghornBank.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class AppUser : IdentityUser
    {

        //TODO: Put any additional fields that you need for your user here
        //For instance
        [Required]
        [Display(Name = "First Name")]
        public String FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public String LastName { get; set; }

        public String MI { get; set; }

        [Required]
        public String Address { get; set; }

        [Required]
        public String City { get; set; }

        [Required]
        public String State { get; set; }

        [Required]
        [Display(Name = "Zip Code")]
        public String ZipCode { get; set; }

        [Display(Name = "Date of Birth")]
        public DateTime DOB { get; set; }

        [Display(Name = "Active Account?")]
        public Boolean IsActive { get; set; }
        public Boolean HasAccount { get; set; }

        [Display(Name = "Is this employee terminated?")]
        public Boolean IsTerminated { get; set; }

        //navigational properties
        public virtual List<CheckingAccount> CheckingAccounts { get; set; }
        public virtual List<SavingsAccount> SavingsAccounts { get; set; }
        public virtual StockPortfolio StockPortfolio { get; set; }
        public virtual IRAccount IRAccount { get; set; }
        public virtual List<Payee> Payees { get; set; }

        //This method allows you to create a new user
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<AppUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    //TODO: Here's your db context for the project.  All of your db sets should go in here
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        //TODO:  Add dbsets here, for instance there's one for books
        //Remember, Identity adds a db set for users, so you shouldn't add that one - you will get an error
        //public DbSet<Book> Books { get; set; }
        public DbSet<CheckingAccount> CheckingAccounts { get; set; }
        public DbSet<SavingsAccount> SavingsAccounts { get; set; }
        public DbSet<StockPortfolio> StockPortfolios { get; set; }
        public DbSet<StockPurchase> StockPurchases { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<IRAccount> IRAccounts { get; set; }
        public DbSet<Dispute> Disputes { get; set; }
        public DbSet<Payee> Payees { get; set; }
        public DbSet<Stock> Stocks { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<AppUser>()
                .HasOptional(f => f.IRAccount)
                .WithRequired(s => s.Customer);
            modelBuilder.Entity<AppUser>()
                .HasOptional(s => s.StockPortfolio)
                .WithRequired(c => c.Customer);
            modelBuilder.Entity<Transaction>()
                .HasOptional(d => d.Dispute)
                .WithRequired(t => t.Transaction);
        }

        //TODO: Make sure that your connection string name is correct here.
        public AppDbContext()
            : base("MyDBConnection", throwIfV1Schema: false)
        {
        }

        public static AppDbContext Create()
        {
            return new AppDbContext();
        }

        public DbSet<AppRole> AppRoles { get; set; }
    }
}