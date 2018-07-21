using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Team16LonghornBank.Models;
using Team16LonghornBank.Utilities;
using Microsoft.AspNet.Identity;

namespace Team16LonghornBank.Controllers
{
    public enum FeeOptions { IncludeFee, AdditionalFee }

    public enum MaxOptions { Auto, Input, Abandon }

    [Authorize]
    public class IRAccountsController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: IRAccounts
        public ActionResult Index()
        {

            return View();

        }


            //    return View(db.IRAccounts.ToList());
            //}


            ////search setup
            //public ActionResult Details(int? id, String DescriptionString, String SelectedType, PriceRange SelectedPrice, String PriceRangeTo, String PriceRangeFrom, String TransactionString, DateRange SelectedDate, String DateRangeTo, String DateRangeFrom)
            //{
            //    List<Transaction> TransactionList = new List<Transaction>();
            //    List<Transaction> SelectedTransaction = new List<Transaction>();

            //    //set up query
            //    var query = from t in db.Transactions
            //                select t;


        public ActionResult Details(int? id)
        { 
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IRAccount iRAccount = db.IRAccounts.Find(id);
            if (iRAccount == null)
            {
                return HttpNotFound();
            }
            IRAccountDetailsViewModel model = new IRAccountDetailsViewModel { IRAccountID = id, IRAccount = iRAccount, Transactions = iRAccount.Transactions };
            ViewBag.Count = iRAccount.Transactions.Count();
            ViewBag.TransactionTypes = new SelectList(Utilities.Utility.TranscationTypes);
            return View(model);
        }

        //POST: CheckingAccounts/Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Details(IRAccountDetailsViewModel model, String TransactionNumber, String DateRange, String Description, String TransactionType, String PriceRange, String RangeFrom, String RangeTo, SortBy TransactionNumberSort, SortBy TransactionTypeSort, SortBy DescriptionSort, SortBy AmountSort, SortBy DateSort)
        {
            int? id = model.IRAccountID;
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IRAccount iRAccount = db.IRAccounts.Find(id);
            IRAccountDetailsViewModel modelToPass = new IRAccountDetailsViewModel { IRAccountID = id, IRAccount = iRAccount, Transactions = iRAccount.Transactions };
            if (iRAccount == null)
            {
                return HttpNotFound();
            }

            var query = from t in iRAccount.Transactions
                        select t;
            if (TransactionNumber != null && TransactionNumber != "")
            {
                Int32 number;
                try
                {
                    number = Convert.ToInt32(TransactionNumber);
                    query = query.Where(t => t.TransactionID.Equals(number));
                }
                catch
                {
                    ViewBag.TransactionNumberValidation = "Enter a whole number";
                    ViewBag.Count = iRAccount.Transactions.Count();
                    ViewBag.TransactionTypes = new SelectList(Utilities.Utility.TranscationTypes);
                    return View(modelToPass);
                }
            }
            query = query.Where(t => t.Description.Contains(Description));
            if (!DateRange.Equals("Custom") && !DateRange.Equals("All"))
            {
                if (DateRange.Equals("Last 15 days")) { query = query.Where(t => t.TransactionDate >= DateTime.Now.AddDays(-1)); }
                else if (DateRange.Equals("Last 30 days")) { query = query.Where(t => t.TransactionDate >= DateTime.Now.AddDays(-30)); }
                else { query = query.Where(t => t.TransactionDate >= DateTime.Now.AddDays(-60)); }
            }
            if (DateRange.Equals("Custom"))
            {
                query = query.Where(t => t.TransactionDate >= model.DateFrom && t.TransactionDate <= model.DateTo);
            }
            if (!TransactionType.Equals("All"))
            {
                query = query.Where(t => t.TransactionType.Equals(TransactionType));
            }
            if (!PriceRange.Equals("Custom"))
            {
                if (PriceRange.Equals("$0-$100")) { query = query.Where(t => t.Amount >= 0 && t.Amount <= 100); }
                else if (PriceRange.Equals("$100-$200")) { query = query.Where(t => t.Amount >= 100 && t.Amount <= 200); }
                else if (PriceRange.Equals("$200-$300")) { query = query.Where(t => t.Amount >= 200 && t.Amount <= 300); }
                else { query = query.Where(t => t.Amount >= 300); }
            }
            else
            {
                Decimal rangeFrom;
                Decimal rangeTo;
                try
                {
                    rangeFrom = Convert.ToDecimal(RangeFrom);
                    rangeTo = Convert.ToDecimal(RangeTo);
                    query = query.Where(t => t.Amount >= rangeFrom && t.Amount <= rangeTo);
                }
                catch
                {
                    ViewBag.Message = "Enter a valid range of numbers";
                    ViewBag.Count = iRAccount.Transactions.Count();
                    ViewBag.TransactionTypes = new SelectList(Utilities.Utility.TranscationTypes);
                    return View(modelToPass);
                }
            }

            if (TransactionNumberSort.Equals(SortBy.Ascending))
            {
                if (TransactionTypeSort.Equals(SortBy.Ascending))
                {
                    if (DescriptionSort.Equals(SortBy.Ascending))
                    {
                        if (AmountSort.Equals(SortBy.Ascending))
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderBy(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenBy(t => t.Description).ThenBy(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderBy(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenBy(t => t.Description).ThenBy(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                        else
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderBy(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenBy(t => t.Description).ThenByDescending(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderBy(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenBy(t => t.Description).ThenByDescending(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                    }
                    else
                    {
                        if (AmountSort.Equals(SortBy.Ascending))
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderBy(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenByDescending(t => t.Description).ThenBy(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderBy(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenByDescending(t => t.Description).ThenBy(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                        else
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderBy(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenByDescending(t => t.Description).ThenByDescending(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderBy(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenByDescending(t => t.Description).ThenByDescending(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                    }
                }
                else
                {
                    if (DescriptionSort.Equals(SortBy.Ascending))
                    {
                        if (AmountSort.Equals(SortBy.Ascending))
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderBy(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenBy(t => t.Description).ThenBy(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderBy(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenBy(t => t.Description).ThenBy(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                        else
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderBy(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenBy(t => t.Description).ThenByDescending(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderBy(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenBy(t => t.Description).ThenByDescending(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                    }
                    else
                    {
                        if (AmountSort.Equals(SortBy.Ascending))
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderBy(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenByDescending(t => t.Description).ThenBy(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderBy(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenByDescending(t => t.Description).ThenBy(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                        else
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderBy(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenByDescending(t => t.Description).ThenByDescending(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderBy(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenByDescending(t => t.Description).ThenByDescending(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                    }
                }
            }
            else
            {
                if (TransactionTypeSort.Equals(SortBy.Ascending))
                {
                    if (DescriptionSort.Equals(SortBy.Ascending))
                    {
                        if (AmountSort.Equals(SortBy.Ascending))
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderByDescending(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenBy(t => t.Description).ThenBy(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderByDescending(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenBy(t => t.Description).ThenBy(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                        else
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderByDescending(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenBy(t => t.Description).ThenByDescending(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderByDescending(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenBy(t => t.Description).ThenByDescending(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                    }
                    else
                    {
                        if (AmountSort.Equals(SortBy.Ascending))
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderByDescending(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenByDescending(t => t.Description).ThenBy(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderByDescending(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenByDescending(t => t.Description).ThenBy(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                        else
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderByDescending(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenByDescending(t => t.Description).ThenByDescending(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderByDescending(t => t.TransactionID).ThenBy(t => t.TransactionType).ThenByDescending(t => t.Description).ThenByDescending(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                    }
                }
                else
                {
                    if (DescriptionSort.Equals(SortBy.Ascending))
                    {
                        if (AmountSort.Equals(SortBy.Ascending))
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderByDescending(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenBy(t => t.Description).ThenBy(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderByDescending(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenBy(t => t.Description).ThenBy(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                        else
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderByDescending(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenBy(t => t.Description).ThenByDescending(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderByDescending(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenBy(t => t.Description).ThenByDescending(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                    }
                    else
                    {
                        if (AmountSort.Equals(SortBy.Ascending))
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderByDescending(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenByDescending(t => t.Description).ThenBy(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderByDescending(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenByDescending(t => t.Description).ThenBy(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                        else
                        {
                            if (DateSort.Equals(SortBy.Ascending)) { query.OrderByDescending(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenByDescending(t => t.Description).ThenByDescending(t => t.Amount).ThenBy(t => t.TransactionDate); }
                            else { query.OrderByDescending(t => t.TransactionID).ThenByDescending(t => t.TransactionType).ThenByDescending(t => t.Description).ThenByDescending(t => t.Amount).ThenByDescending(t => t.TransactionDate); }
                        }
                    }
                }
            }

            query.OrderByDescending(t => t.TransactionID);
            List<Transaction> list = query.ToList();
            modelToPass = new IRAccountDetailsViewModel { IRAccountID = id, IRAccount = iRAccount, Transactions = list };
            ViewBag.Count = list.Count();
            ViewBag.TransactionTypes = new SelectList(Utilities.Utility.TranscationTypes);
            return View(modelToPass);
        }



        //GET: IRAccounts/Create
        public ActionResult Create()
        {

            return View();

        }

        //POST: IRAccounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "IRAccountID,Balance")] IRAccount iRAccount)
        {

            AppUser user = db.Users.Find(User.Identity.GetUserId());
            iRAccount.Customer = user;


            if (user.IRAccount != null)
            {
                return View("Error", new string[] { "You have already created an IRAccount." });
            }

            else
            {
                if (ModelState.IsValid)
                {

                    iRAccount.AccountNumber = Utility.AccountNumber;
                    Utility.AccountNumber += 1;

                    iRAccount.AccountName = "Longhorn IRA";

                    iRAccount.MaxContribution = 5000;


                    iRAccount.Customer.HasAccount = true;


                    Transaction transaction = new Transaction();
                    transaction.IRAccountAffected = iRAccount;
                    transaction.TransactionType = "Deposit";
                    transaction.TransactionDate = DateTime.Now;
                    transaction.Amount = iRAccount.Balance;
                    iRAccount.MaxContribution -= transaction.Amount;
                    transaction.Description = "Initial Deposit into " + iRAccount.AccountNumber;
                    transaction.isBeingDisputed = false;
                    transaction.EmployeeComments = "";
                    if (iRAccount.Balance > 5000)
                    {
                        transaction.isPending = true;
                        iRAccount.Balance = 0;
                    }
                    else
                    {
                        transaction.isPending = false;
                    }



                    DateTime now = DateTime.Today;
                    int age = now.Year - user.DOB.Year;
                    if (age > 65)
                    {
                        iRAccount.isQualified = true;

                    }


                    iRAccount.isQualified = false;

                    db.Transactions.Add(transaction);
                    iRAccount.Transactions = new List<Transaction>();
                    iRAccount.Transactions.Add(transaction);
                    db.IRAccounts.Add(iRAccount);
                    db.SaveChanges();
                    return RedirectToAction("BankAccountCreatedConfirmation", "Account");
                }



            }

            return View(iRAccount);
        }


        // GET: IRAccount/Edit/5
        public ActionResult Edit(int? id)
        {
            AppUser customer = db.Users.Find(User.Identity.GetUserId());
            if (customer.IsActive)
            {
                if (id == null)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
                }
                IRAccount iRAccount = db.IRAccounts.Find(id);
                if (iRAccount == null)
                {
                    return HttpNotFound();
                }
                return View(iRAccount);
            }
            return View("Error", new string[] { "Your account is inactive." });
        }

        // POST: IRAccount/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(IRAccount iRAccount)
        {
            if (ModelState.IsValid)
            {
                db.Entry(iRAccount).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("MyBankAccounts", "Account");
            }
            return View(iRAccount);
        }

        // GET: IRAccounts/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            IRAccount iRAccount = db.IRAccounts.Find(id);
            if (iRAccount == null)
            {
                return HttpNotFound();
            }
            return View(iRAccount);
        }

        // POST: IRAccounts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            IRAccount iRAccount = db.IRAccounts.Find(id);
            db.IRAccounts.Remove(iRAccount);
            db.SaveChanges();
            return RedirectToAction("MyBankAccounts", "Account");
        }

        // GET: IRAccounts/Deposit
        public ActionResult Deposit()
        {
            //get the user id in order to reference later 
            AppUser user = db.Users.Find(User.Identity.GetUserId());

            //setting up viewbag so an IRAccount is chosen for sure 
            ViewBag.iRAccounts = GetIRAccounts();



            // Save today's date.
            var today = DateTime.Today;
            // Calculate the age.
            var age = today.Year - user.DOB.Year;



            // checking to make sure only someone younger than 70 is contributing to IRA
            if (age > 70)
            {
                return View("ErrorAge",new string[] { "You must be younger than 70 to contribute to an IRA" });

            }
            else
            {
                return View();

            }
        }

        // POST: IRAccounts/Deposit
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Deposit([Bind(Include = "TransactionID,TransactionDate,Amount,Description")] Transaction transaction, int IRAccountID)
        {
            //finding user 
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            if (ModelState.IsValid)
            {

                transaction.TransactionType = "Deposit";
                transaction.isBeingDisputed = false;
                transaction.EmployeeComments = "";
                IRAccount IRAccountToChange = db.IRAccounts.Find(IRAccountID);


                if (transaction.Amount > IRAccountToChange.MaxContribution)
                {
                    return RedirectToAction("MaxContrib", "IRAccounts");

                }
                else
                {
                    transaction.isPending = false;
                    IRAccountToChange.Balance += transaction.Amount;
                    IRAccountToChange.MaxContribution -= transaction.Amount;

                }
                if (transaction.Description == null)
                {
                    transaction.Description = "";
                }
                transaction.IRAccountAffected = IRAccountToChange;
                IRAccountToChange.Transactions.Add(transaction);
                db.Transactions.Add(transaction);
                db.SaveChanges();
                return RedirectToAction("MyBankAccounts", "Account");


            }
            //setting up viewbag so an IRAccount is chosen for sure 
            ViewBag.iRAccounts = GetIRAccounts();
            return View(transaction);
        }

        // GET: IRAccount/Withdrawal
        public ActionResult Withdrawal()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());

            //setting up viewbag so an IRAccount is chosen for sure 
            ViewBag.iRAccounts = GetIRAccounts();

            return View();
        }

        // POST: IRAccount/Withdrawal
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Withdrawal([Bind(Include = "TransactionID,TransactionDate,Amount,Description")] Transaction transaction, FeeOptions SelectedFee, int IRAccountID)
        {
            if (ModelState.IsValid)
            {
                transaction.TransactionType = "Withdrawal";
                transaction.isBeingDisputed = false;
                transaction.EmployeeComments = "";
                transaction.isPending = false;
                if (transaction.Description == null)
                {
                    transaction.Description = "";
                }
                IRAccount IRAAccountToChange = db.IRAccounts.Find(IRAccountID);
                if (IRAAccountToChange.Balance < 0)
                {
                    return View("Error", new string[] { "You can't withdraw from an overdrawn account." });
                }
                if (transaction.Amount > IRAAccountToChange.Balance)
                {
                    return View("Error", new string[] { "You don't have enough funds to withdraw that much money." });
                }
                if (IRAAccountToChange.isQualified == false)
                {
                    if (transaction.Amount > 3000)
                    {
                        return View("Error", new string[] { "You cannot withdraw more than $3,000." });

                    }


                    switch (SelectedFee)
                    {
                        case FeeOptions.IncludeFee:
                            IRAAccountToChange.Balance -= transaction.Amount;
                            transaction.Amount -= 30;
                            Transaction transactionIncludeFee = new Transaction();
                            transactionIncludeFee.TransactionType = "Fee";
                            transactionIncludeFee.TransactionDate = DateTime.Now;
                            transactionIncludeFee.Amount = 30;
                            transactionIncludeFee.isBeingDisputed = false;
                            transactionIncludeFee.EmployeeComments = "";
                            transactionIncludeFee.Description = "";
                            transactionIncludeFee.isPending = false;
                            transactionIncludeFee.IRAccountAffected = IRAAccountToChange;
                            IRAAccountToChange.Transactions.Add(transactionIncludeFee);
                            db.Transactions.Add(transactionIncludeFee);
                            


                            transaction.IRAccountAffected = IRAAccountToChange;
                            IRAAccountToChange.Transactions.Add(transaction);
                            db.Transactions.Add(transaction);
                            db.SaveChanges();
                            
                            break;

                        case FeeOptions.AdditionalFee:
                            IRAAccountToChange.Balance -= (transaction.Amount + 30);
                            Transaction transactionAddFee = new Transaction();
                            transactionAddFee.TransactionType = "Fee";
                            transactionAddFee.TransactionDate = DateTime.Now;
                            transactionAddFee.Amount = 30;
                            transactionAddFee.isBeingDisputed = false;
                            transactionAddFee.EmployeeComments = "";
                            transactionAddFee.Description = "";
                            transactionAddFee.isPending = false;
                            transactionAddFee.IRAccountAffected = IRAAccountToChange;
                            IRAAccountToChange.Transactions.Add(transactionAddFee);
                            db.Transactions.Add(transactionAddFee);

                            transaction.IRAccountAffected = IRAAccountToChange;
                            IRAAccountToChange.Transactions.Add(transaction);
                            db.Transactions.Add(transaction);
                            db.SaveChanges();

                            break;

                    }

                    return RedirectToAction("MyBankAccounts", "Account");
                }

                
            }

            AppUser user = db.Users.Find(User.Identity.GetUserId());
            //setting up viewbag so an IRAccount is chosen for sure 
            ViewBag.iRAccounts = GetIRAccounts();
            return View(transaction);
        }

        // GET: IRAccount/ChooseAnAccount
        public ActionResult ChooseAnAccount()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            ViewBag.iRAccount = GetIRAccounts();
            return View();
        }

        // POST: IRAccount/ChooseAnAccount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChooseAnAccount(int IRAccountID)
        {
            return View();
        }

        //GET: IRAccount/MaxContrib
        public ActionResult MaxContrib()
        {
            return View();
        }


        //POST: IRAccount/MaxContrib
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MaxContrib(MaxOptions SelectedMaxOption)
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            IRAccount IRAccountToChange = user.IRAccount;

            switch (SelectedMaxOption)
            {
                case MaxOptions.Auto:
                    {
                        if (IRAccountToChange.MaxContribution == 0)
                        {
                            return View("ErrorAge", new string[] { "You cannot deposit. You have maxed on your contribution." });
                        }
                      
                        Transaction transaction = new Transaction();
                        transaction.IRAccountAffected = IRAccountToChange;
                        transaction.Amount = IRAccountToChange.MaxContribution;
                        transaction.TransactionType = "Deposit";
                        transaction.TransactionDate = DateTime.Now;
                        transaction.isBeingDisputed = false;
                        transaction.EmployeeComments = "";
                        transaction.Description = "";
                        transaction.isPending = false;
                        IRAccountToChange.Balance += transaction.Amount;
                        IRAccountToChange.MaxContribution -= transaction.Amount;
                        IRAccountToChange.Transactions.Add(transaction);
                        db.Transactions.Add(transaction);
                        db.SaveChanges();
                        return RedirectToAction("MyBankAccounts", "Account");


                    }

                case MaxOptions.Input:
                    {

                        return RedirectToAction("Deposit", "IRAccounts");
                    }

                case MaxOptions.Abandon:
                    {
                        return RedirectToAction("MyBankAccounts", "Account");
                    }

            }

            return RedirectToAction("MyBankAccounts", "Account");
        }

       




        public SelectList GetIRAccounts()
        {

            AppUser user = db.Users.Find(User.Identity.GetUserId());
            IRAccount iRAccount = user.IRAccount;
            List<IRAccount> iRAccountlist = new List<IRAccount>();
            iRAccountlist.Add(user.IRAccount);
            SelectList list = new SelectList(iRAccountlist, "IRAccountID", "AccountName");
            return list;
        }

        



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

