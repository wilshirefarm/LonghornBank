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
    [Authorize]
    public class TransfersController : Controller
    {
        private AppDbContext db = new AppDbContext();

        //GET: Transfers/ChooseAccounts
        public ActionResult ChooseAccountTypes()
        {
            AppUser customer = db.Users.Find(User.Identity.GetUserId());
            if (customer.IsActive)
            {
                ViewBag.AccountTypes = new SelectList(Utility.AccountTypes);
                return View();
            }
            return View("Error", new string[] { "Your account is inactive." });
        }

        //POST: Transfers/ChooseAccounts
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChooseAccountTypes(String AccountFrom, String AccountTo)
        {
            if (AccountFrom == "Checking Account")
            {
                if (AccountTo == "Checking Account")
                {
                    return RedirectToAction("CheckingToChecking");
                }
                else if (AccountTo == "Savings Account")
                {
                    return RedirectToAction("CheckingToSavings");
                }
                else if (AccountTo == "IRA")
                {
                    return RedirectToAction("CheckingToIRA");
                }
                else
                {
                    return RedirectToAction("CheckingToStockPortfolio");
                }
            }
            else if (AccountFrom == "Savings Account")
            {
                if (AccountTo == "Checking Account")
                {
                    return RedirectToAction("SavingsToChecking");
                }
                else if (AccountTo == "Savings Account")
                {
                    return RedirectToAction("SavingsToSavings");
                }
                else if (AccountTo == "IRA")
                {
                    return RedirectToAction("SavingsToIRA");
                }
                else
                {
                    return RedirectToAction("SavingsToStockPortfolio");
                }
            }
            else if (AccountFrom == "IRA")
            {
                if (AccountTo == "Checking Account")
                {
                    return RedirectToAction("IRAToChecking");
                }
                else if (AccountTo == "Savings Account")
                {
                    return RedirectToAction("IRAToSavings");
                }
                else if (AccountTo == "IRA")
                {
                    return View("IRAToIRA");
                }
                else
                {
                    return RedirectToAction("IRAToStockPortfolio");
                }
            }
            else
            {
                if (AccountTo == "Checking Account")
                {
                    return RedirectToAction("StockPortfolioToChecking");
                }
                else if (AccountTo == "Savings Account")
                {
                    return RedirectToAction("StockPortfolioToSavings");
                }
                else if (AccountTo == "IRA")
                {
                    return RedirectToAction("StockPortfolioToIRA");
                }
                else
                {
                    return View("StockPortfolioToStockPortfolio");
                }
            }
        }

        public ActionResult TransferConfirmationMessage()
        {
            return View();
        }

        //GET: Transfers/CheckingToChecking
        public ActionResult CheckingToChecking()
        {
            ViewBag.FromCheckingAccounts = GetCheckingAccounts();
            ViewBag.ToCheckingAccounts = GetCheckingAccounts();
            return View();
        }

        //POST: Transfer/CheckingToChecking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckingToChecking([Bind(Include = "TransactionID,TransactionDate,Amount")] Transaction transaction, int FromCheckingAccountID, int ToCheckingAccountID)
        {
            if ((ModelState.IsValid) && (FromCheckingAccountID != ToCheckingAccountID))
            {
                CheckingAccount FromCheckingAccountToChange = db.CheckingAccounts.Find(FromCheckingAccountID);
                CheckingAccount ToCheckingAccountToChange = db.CheckingAccounts.Find(ToCheckingAccountID);

                if (FromCheckingAccountToChange.Balance<0)
                {
                    return View("Error", new string[] { "You cannot transfer money with a negative balance" });

                }



                

                if (transaction.Amount>FromCheckingAccountToChange.Balance)
                {
                    if ((transaction.Amount - FromCheckingAccountToChange.Balance)>50)
                    {
                        return View("Error", new string[] { "The transaction exceeds the $50 overdraft limit. Please try to input a transfer amount within Balance range or $50 overdraft limit range" });
                    }
                    else
                    {
                        Transaction TransactionODFee = new Transaction();
                        TransactionODFee.Amount = 30;
                        FromCheckingAccountToChange.Balance -= TransactionODFee.Amount;
                        TransactionODFee.TransactionDate = transaction.TransactionDate;
                        TransactionODFee.TransactionType = "Fee";
                        TransactionODFee.isBeingDisputed = false;
                        TransactionODFee.EmployeeComments = "";
                        TransactionODFee.isPending = false;
                        TransactionODFee.Description = "Overdraft Fee";

                        TransactionODFee.CheckingAccountAffected = FromCheckingAccountToChange;
                        EmailMessaging.SendEmail(transaction.CheckingAccountAffected.Customer.Email, "Overdraft", "Your account is now in overdraft status. ");


                        
                        FromCheckingAccountToChange.Transactions.Add(TransactionODFee);
                        db.Transactions.Add(TransactionODFee);                  
                    }                 
                }

                FromCheckingAccountToChange.Balance -= transaction.Amount;
                ToCheckingAccountToChange.Balance += transaction.Amount;

                Transaction TransactionFrom = new Transaction();
                TransactionFrom.Amount = transaction.Amount;
                TransactionFrom.TransactionDate = transaction.TransactionDate;
                TransactionFrom.TransactionType = "Transfer";
                TransactionFrom.isBeingDisputed = false;
                TransactionFrom.EmployeeComments = "";
                TransactionFrom.isPending = false;

                Transaction TransactionTo = new Transaction();
                TransactionTo.Amount = transaction.Amount;
                TransactionTo.TransactionDate = transaction.TransactionDate;
                TransactionTo.TransactionType = "Transfer";
                TransactionTo.isBeingDisputed = false;
                TransactionTo.EmployeeComments = "";
                TransactionTo.isPending = false;

                TransactionFrom.Description = "Transfer From Account " + FromCheckingAccountToChange.AccountNumber.ToString();
                TransactionTo.Description = "Transfer To Account " + ToCheckingAccountToChange.AccountNumber.ToString();
                TransactionFrom.CheckingAccountAffected = ToCheckingAccountToChange;
                TransactionTo.CheckingAccountAffected = FromCheckingAccountToChange;
                
                FromCheckingAccountToChange.Transactions.Add(TransactionTo);
                ToCheckingAccountToChange.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionTo);
                db.SaveChanges();
                return RedirectToAction("TransferConfirmationMessage", "Transfers");
            }

            ViewBag.FromCheckingAccounts = GetCheckingAccounts();
            ViewBag.ToCheckingAccounts = GetCheckingAccounts();
            return View(transaction);
        }

        //GET: Transfers/CheckingToSavings
        public ActionResult CheckingToSavings()
        {
            ViewBag.FromCheckingAccounts = GetCheckingAccounts();
            ViewBag.ToSavingsAccounts = GetSavingsAccounts();
            return View();
        }

        //POST: Transfer/CheckingToSavings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckingToSavings([Bind(Include = "TransactionID,TransactionDate,Amount")] Transaction transaction, int FromCheckingAccountID, int ToSavingsAccountID)
        {
            if (ModelState.IsValid)
            {
                CheckingAccount FromCheckingAccountToChange = db.CheckingAccounts.Find(FromCheckingAccountID);
                SavingsAccount ToSavingsAccountToChange = db.SavingsAccounts.Find(ToSavingsAccountID);

                if (FromCheckingAccountToChange.Balance < 0)
                {
                    return View("Error", new string[] { "You cannot transfer money with a negative balance" });

                }





                if (transaction.Amount > FromCheckingAccountToChange.Balance)
                {
                    if ((transaction.Amount - FromCheckingAccountToChange.Balance) > 50)
                    {
                        return View("Error", new string[] { "The transaction exceeds the $50 overdraft limit. Please try to input a transfer amount within Balance range or $50 overdraft limit range" });
                    }
                    else
                    {
                        Transaction TransactionODFee = new Transaction();
                        TransactionODFee.Amount = 30;
                        FromCheckingAccountToChange.Balance -= TransactionODFee.Amount;
                        TransactionODFee.TransactionDate = transaction.TransactionDate;
                        TransactionODFee.TransactionType = "Fee";
                        TransactionODFee.isBeingDisputed = false;
                        TransactionODFee.EmployeeComments = "";
                        TransactionODFee.isPending = false;
                        TransactionODFee.Description = "Overdraft Fee";

                        TransactionODFee.CheckingAccountAffected = FromCheckingAccountToChange;
                        EmailMessaging.SendEmail(transaction.CheckingAccountAffected.Customer.Email, "Overdraft", "Your account is now in overdraft status. ");



                        
                        FromCheckingAccountToChange.Transactions.Add(TransactionODFee);
                        db.Transactions.Add(TransactionODFee);




                    }


                }



                FromCheckingAccountToChange.Balance -= transaction.Amount;
                ToSavingsAccountToChange.Balance += transaction.Amount;

                Transaction TransactionFrom = new Transaction();
                TransactionFrom.Amount = transaction.Amount;
                TransactionFrom.TransactionDate = transaction.TransactionDate;
                TransactionFrom.TransactionType = "Transfer";
                TransactionFrom.isBeingDisputed = false;
                TransactionFrom.EmployeeComments = "";
                TransactionFrom.isPending = false;

                Transaction TransactionTo = new Transaction();
                TransactionTo.Amount = transaction.Amount;
                TransactionTo.TransactionDate = transaction.TransactionDate;
                TransactionTo.TransactionType = "Transfer";
                TransactionTo.isBeingDisputed = false;
                TransactionTo.EmployeeComments = "";
                TransactionTo.isPending = false;

                TransactionFrom.Description = "Transfer From Account " + FromCheckingAccountToChange.AccountNumber.ToString();
                TransactionTo.Description = "Transfer To Account " + ToSavingsAccountToChange.AccountNumber.ToString();
                TransactionFrom.SavingsAccountAffected = ToSavingsAccountToChange;
                TransactionTo.CheckingAccountAffected = FromCheckingAccountToChange;

                FromCheckingAccountToChange.Transactions.Add(TransactionTo);
                ToSavingsAccountToChange.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionTo);
                db.SaveChanges();
                return RedirectToAction("TransferConfirmationMessage", "Transfers");
            }

            ViewBag.FromCheckingAccounts = GetCheckingAccounts();
            ViewBag.ToSavingsAccounts = GetSavingsAccounts();
            return View(transaction);
        }

        //GET: Transfers/CheckingToStockPortfolio
        public ActionResult CheckingToStockPortfolio()
        {
            ViewBag.ToStock = GetStockPortfolio();
            ViewBag.FromCheckingAccounts = GetCheckingAccounts();
            return View();
        }

        //POST: Transfers/CheckingToStockPortfolio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckingToStockPortfolio([Bind(Include = "TransactionID,TransactionDate,Amount")] Transaction transaction, int FromCheckingAccountID, int ToStockPortfolioID)
        {
            if (ModelState.IsValid)
            {
                CheckingAccount FromCheckingAccountToChange = db.CheckingAccounts.Find(FromCheckingAccountID);
                StockPortfolio ToStockPortfolioToChange = db.StockPortfolios.Find(ToStockPortfolioID);

                if (FromCheckingAccountToChange.Balance < 0)
                {
                    return View("Error", new string[] { "You cannot transfer money with a negative balance" });

                }





                if (transaction.Amount > FromCheckingAccountToChange.Balance)
                {
                    if ((transaction.Amount - FromCheckingAccountToChange.Balance) > 50)
                    {
                        return View("Error", new string[] { "The transaction exceeds the $50 overdraft limit. Please try to input a transfer amount within balance range or $50 overdraft limit range" });
                    }
                    else
                    {
                        Transaction TransactionODFee = new Transaction();
                        TransactionODFee.Amount = 30;
                        FromCheckingAccountToChange.Balance -= TransactionODFee.Amount;
                        TransactionODFee.TransactionDate = transaction.TransactionDate;
                        TransactionODFee.TransactionType = "Fee";
                        TransactionODFee.isBeingDisputed = false;
                        TransactionODFee.EmployeeComments = "";
                        TransactionODFee.isPending = false;
                        TransactionODFee.Description = "Overdraft Fee";

                        TransactionODFee.CheckingAccountAffected = FromCheckingAccountToChange;
                        EmailMessaging.SendEmail(transaction.CheckingAccountAffected.Customer.Email, "Overdraft", "Your account is now in overdraft status. ");


                        
                        FromCheckingAccountToChange.Transactions.Add(TransactionODFee);
                        db.Transactions.Add(TransactionODFee);




                    }


                }


                FromCheckingAccountToChange.Balance -= transaction.Amount;
                ToStockPortfolioToChange.CashValueBalance += transaction.Amount;

                Transaction TransactionFrom = new Transaction();
                TransactionFrom.Amount = transaction.Amount;
                TransactionFrom.TransactionDate = transaction.TransactionDate;
                TransactionFrom.TransactionType = "Transfer";
                TransactionFrom.isBeingDisputed = false;
                TransactionFrom.EmployeeComments = "";
                TransactionFrom.isPending = false;

                Transaction TransactionTo = new Transaction();
                TransactionTo.Amount = transaction.Amount;
                TransactionTo.TransactionDate = transaction.TransactionDate;
                TransactionTo.TransactionType = "Transfer";
                TransactionTo.isBeingDisputed = false;
                TransactionTo.EmployeeComments = "";
                TransactionTo.isPending = false;

                TransactionFrom.Description = "Transfer From Account " + FromCheckingAccountToChange.AccountNumber.ToString();
                TransactionTo.Description = "Transfer To Account " + ToStockPortfolioToChange.AccountNumber.ToString();
                TransactionFrom.StockPortfolioAffected = ToStockPortfolioToChange;
                TransactionTo.CheckingAccountAffected = FromCheckingAccountToChange;

                FromCheckingAccountToChange.Transactions.Add(TransactionTo);
                ToStockPortfolioToChange.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionTo);
                db.SaveChanges();
                return RedirectToAction("TransferConfirmationMessage", "Transfers");
            }

            ViewBag.ToStock = GetStockPortfolio();
            ViewBag.FromCheckingAccounts = GetCheckingAccounts();
            return View(transaction);
        }

        //GET: Transfers/CheckingtoIRA
        public ActionResult CheckingToIRA()
        {
            ViewBag.ToIRAccount = GetIRAccount();
            ViewBag.FromCheckingAccounts = GetCheckingAccounts();
            return View();
        }


        //POST: Transfers/CheckingToIRA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CheckingToIRA([Bind(Include = "TransactionID,TransactionDate,Amount")] Transaction transaction, int FromCheckingAccountID, int ToIRAccountID)
        {
            if (ModelState.IsValid)
            {
                CheckingAccount FromCheckingAccountToChange = db.CheckingAccounts.Find(FromCheckingAccountID);
                IRAccount ToIRAccountToChange = db.IRAccounts.Find(ToIRAccountID);

                if (FromCheckingAccountToChange.Balance < 0)
                {
                    return View("Error", new string[] { "You cannot transfer money with a negative balance" });

                }





                if (transaction.Amount > FromCheckingAccountToChange.Balance)
                {
                    if ((transaction.Amount - FromCheckingAccountToChange.Balance) > 50)
                    {
                        return View("Error", new string[] { "The transaction exceeds the $50 overdraft limit. Please try to input a transfer amount within Balance range or $50 overdraft limit range" });
                    }
                    else
                    {
                        Transaction TransactionODFee = new Transaction();
                        TransactionODFee.Amount = 30;
                        FromCheckingAccountToChange.Balance -= TransactionODFee.Amount;
                        TransactionODFee.TransactionDate = transaction.TransactionDate;
                        TransactionODFee.TransactionType = "Fee";
                        TransactionODFee.isBeingDisputed = false;
                        TransactionODFee.EmployeeComments = "";
                        TransactionODFee.isPending = false;
                        TransactionODFee.Description = "Overdraft Fee";

                        TransactionODFee.CheckingAccountAffected = FromCheckingAccountToChange;
                        EmailMessaging.SendEmail(transaction.CheckingAccountAffected.Customer.Email, "Overdraft", "Your account is now in overdraft status. ");


                        
                        FromCheckingAccountToChange.Transactions.Add(TransactionODFee);
                        db.Transactions.Add(TransactionODFee);




                    }


                }


                FromCheckingAccountToChange.Balance -= transaction.Amount;
                ToIRAccountToChange.Balance += transaction.Amount;

                if (transaction.Amount > ToIRAccountToChange.MaxContribution)
                {
                    return RedirectToAction("MaxContribTransferChecking", new { id = FromCheckingAccountID });

                }
                else
                {
                    transaction.isPending = false;

                    ToIRAccountToChange.MaxContribution -= transaction.Amount;




                    Transaction TransactionFrom = new Transaction();
                    TransactionFrom.Amount = transaction.Amount;
                    TransactionFrom.TransactionDate = transaction.TransactionDate;
                    TransactionFrom.TransactionType = "Transfer";
                    TransactionFrom.isBeingDisputed = false;
                    TransactionFrom.EmployeeComments = "";
                    TransactionFrom.isPending = false;

                    Transaction TransactionTo = new Transaction();
                    TransactionTo.Amount = transaction.Amount;
                    TransactionTo.TransactionDate = transaction.TransactionDate;
                    TransactionTo.TransactionType = "Transfer";
                    TransactionTo.isBeingDisputed = false;
                    TransactionTo.EmployeeComments = "";
                    TransactionTo.isPending = false;

                    TransactionFrom.Description = "Transfer From Account " + FromCheckingAccountToChange.AccountNumber.ToString();
                    TransactionTo.Description = "Transfer To Account " + ToIRAccountToChange.AccountNumber.ToString();
                    TransactionFrom.IRAccountAffected = ToIRAccountToChange;
                    TransactionTo.CheckingAccountAffected = FromCheckingAccountToChange;

                    FromCheckingAccountToChange.Transactions.Add(TransactionTo);
                    ToIRAccountToChange.Transactions.Add(TransactionFrom);
                    db.Transactions.Add(TransactionFrom);
                    db.Transactions.Add(TransactionTo);
                    db.SaveChanges();
                    return RedirectToAction("TransferConfirmationMessage", "Transfers");
                }

            }
            ViewBag.ToIRAccount = GetIRAccount();
            ViewBag.FromCheckingAccounts = GetCheckingAccounts();
            return View(transaction);
        }


        //GET: Transfers/SavingsToSavings
        public ActionResult SavingsToSavings()
        {
            ViewBag.FromSavingsAccounts = GetSavingsAccounts();
            ViewBag.ToSavingsAccounts = GetSavingsAccounts();
            return View();
        }

        //POST: Transfer/SavingsToSavings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SavingsToSavings([Bind(Include = "TransactionID,TransactionDate,Amount")] Transaction transaction, int FromSavingsAccountID, int ToSavingsAccountID)
        {
            if ((ModelState.IsValid) && (FromSavingsAccountID != ToSavingsAccountID))
            {
                SavingsAccount FromSavingsAccountToChange = db.SavingsAccounts.Find(FromSavingsAccountID);
                SavingsAccount ToSavingsAccountToChange = db.SavingsAccounts.Find(ToSavingsAccountID);

                if (FromSavingsAccountToChange.Balance < 0)
                {
                    return View("Error", new string[] { "You cannot transfer money with a negative balance" });

                }





                if (transaction.Amount > FromSavingsAccountToChange.Balance)
                {
                    if ((transaction.Amount - FromSavingsAccountToChange.Balance) > 50)
                    {
                        return View("Error", new string[] { "The transaction exceeds the $50 overdraft limit. Please try to input a transfer amount within Balance range or $50 overdraft limit range" });
                    }
                    else
                    {
                        Transaction TransactionODFee = new Transaction();
                        TransactionODFee.Amount = 30;
                        FromSavingsAccountToChange.Balance -= TransactionODFee.Amount;
                        TransactionODFee.TransactionDate = transaction.TransactionDate;
                        TransactionODFee.TransactionType = "Fee";
                        TransactionODFee.isBeingDisputed = false;
                        TransactionODFee.EmployeeComments = "";
                        TransactionODFee.isPending = false;
                        TransactionODFee.Description = "Overdraft Fee";

                        TransactionODFee.SavingsAccountAffected = FromSavingsAccountToChange;
                        EmailMessaging.SendEmail(transaction.SavingsAccountAffected.Customer.Email, "Overdraft", "Your account is now in overdraft status. ");


                        
                        FromSavingsAccountToChange.Transactions.Add(TransactionODFee);
                        db.Transactions.Add(TransactionODFee);




                    }


                }


                FromSavingsAccountToChange.Balance -= transaction.Amount;
                ToSavingsAccountToChange.Balance += transaction.Amount;

                Transaction TransactionFrom = new Transaction();
                TransactionFrom.Amount = transaction.Amount;
                TransactionFrom.TransactionDate = transaction.TransactionDate;
                TransactionFrom.TransactionType = "Transfer";
                TransactionFrom.isBeingDisputed = false;
                TransactionFrom.EmployeeComments = "";
                TransactionFrom.isPending = false;

                Transaction TransactionTo = new Transaction();
                TransactionTo.Amount = transaction.Amount;
                TransactionTo.TransactionDate = transaction.TransactionDate;
                TransactionTo.TransactionType = "Transfer";
                TransactionTo.isBeingDisputed = false;
                TransactionTo.EmployeeComments = "";
                TransactionTo.isPending = false;

                TransactionFrom.Description = "Transfer From Account " + FromSavingsAccountToChange.AccountNumber.ToString();
                TransactionTo.Description = "Transfer To Account " + ToSavingsAccountToChange.AccountNumber.ToString();
                TransactionFrom.SavingsAccountAffected = ToSavingsAccountToChange;
                TransactionTo.SavingsAccountAffected = FromSavingsAccountToChange;

                FromSavingsAccountToChange.Transactions.Add(TransactionTo);
                ToSavingsAccountToChange.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionTo);
                db.SaveChanges();
                return RedirectToAction("TransferConfirmationMessage", "Transfers");
            }

            ViewBag.FromSavingsAccounts = GetSavingsAccounts();
            ViewBag.ToSavingsAccounts = GetSavingsAccounts();
            return View(transaction);
        }

        //GET: Transfers/SavingsToChecking
        public ActionResult SavingsToChecking()
        {
            ViewBag.FromSavingsAccounts = GetSavingsAccounts();
            ViewBag.ToCheckingAccounts = GetCheckingAccounts();
            return View();
        }

        //POST: Transfers/SavingsToChecking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SavingsToChecking([Bind(Include = "TransactionID,TransactionDate,Amount")] Transaction transaction, int FromSavingsAccountID, int ToCheckingAccountID)
        {
            if (ModelState.IsValid)
            {
                SavingsAccount FromSavingsAccountToChange = db.SavingsAccounts.Find(FromSavingsAccountID);
                CheckingAccount ToCheckingAccountToChange = db.CheckingAccounts.Find(ToCheckingAccountID);


                if (FromSavingsAccountToChange.Balance < 0)
                {
                    return View("Error", new string[] { "You cannot transfer money with a negative balance" });

                }





                if (transaction.Amount > FromSavingsAccountToChange.Balance)
                {
                    if ((transaction.Amount - FromSavingsAccountToChange.Balance) > 50)
                    {
                        return View("Error", new string[] { "The transaction exceeds the $50 overdraft limit. Please try to input a transfer amount within Balance range or $50 overdraft limit range" });
                    }
                    else
                    {
                        Transaction TransactionODFee = new Transaction();
                        TransactionODFee.Amount = 30;
                        FromSavingsAccountToChange.Balance -= TransactionODFee.Amount;
                        TransactionODFee.TransactionDate = transaction.TransactionDate;
                        TransactionODFee.TransactionType = "Fee";
                        TransactionODFee.isBeingDisputed = false;
                        TransactionODFee.EmployeeComments = "";
                        TransactionODFee.isPending = false;
                        TransactionODFee.Description = "Overdraft Fee";


                        TransactionODFee.SavingsAccountAffected = FromSavingsAccountToChange;
                        EmailMessaging.SendEmail(TransactionODFee.SavingsAccountAffected.Customer.Email, "Overdraft", "Your account is now in overdraft status. ");
                        FromSavingsAccountToChange.Transactions.Add(TransactionODFee);
                        db.Transactions.Add(TransactionODFee);




                    }


                }


                FromSavingsAccountToChange.Balance -= transaction.Amount;
                ToCheckingAccountToChange.Balance += transaction.Amount;

                Transaction TransactionFrom = new Transaction();
                TransactionFrom.Amount = transaction.Amount;
                TransactionFrom.TransactionDate = transaction.TransactionDate;
                TransactionFrom.TransactionType = "Transfer";
                TransactionFrom.isBeingDisputed = false;
                TransactionFrom.EmployeeComments = "";
                TransactionFrom.isPending = false;

                Transaction TransactionTo = new Transaction();
                TransactionTo.Amount = transaction.Amount;
                TransactionTo.TransactionDate = transaction.TransactionDate;
                TransactionTo.TransactionType = "Transfer";
                TransactionTo.isBeingDisputed = false;
                TransactionTo.EmployeeComments = "";
                TransactionTo.isPending = false;

                TransactionFrom.Description = "Transfer From Account " + FromSavingsAccountToChange.AccountNumber.ToString();
                TransactionTo.Description = "Transfer To Account " + ToCheckingAccountToChange.AccountNumber.ToString();
                TransactionFrom.CheckingAccountAffected = ToCheckingAccountToChange;
                TransactionTo.SavingsAccountAffected = FromSavingsAccountToChange;

                FromSavingsAccountToChange.Transactions.Add(TransactionTo);
                ToCheckingAccountToChange.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionTo);
                db.SaveChanges();
                return RedirectToAction("TransferConfirmationMessage", "Transfers");
            }

            ViewBag.FromSavingsAccounts = GetSavingsAccounts();
            ViewBag.ToCheckingAccounts = GetCheckingAccounts();
            return View(transaction);
        }

        //GET: Transfers/SavingsToStockPortfolio
        public ActionResult SavingsToStockPortfolio()
        {
            ViewBag.ToStock = GetStockPortfolio();
            ViewBag.FromSavingsAccounts = GetSavingsAccounts();
            return View();
        }

        //POST: Transfers/SavingsToStockPortfolio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SavingsToStockPortfolio([Bind(Include = "TransactionID,TransactionDate,Amount")] Transaction transaction, int FromSavingsAccountID, int ToStockPortfolioID)
        {
            if (ModelState.IsValid)
            {
                SavingsAccount FromSavingsAccountToChange = db.SavingsAccounts.Find(FromSavingsAccountID);
                StockPortfolio ToStockPortfolioToChange = db.StockPortfolios.Find(ToStockPortfolioID);

                if (FromSavingsAccountToChange.Balance < 0)
                {
                    return View("Error", new string[] { "You cannot transfer money with a negative balance" });

                }





                if (transaction.Amount > FromSavingsAccountToChange.Balance)
                {
                    if ((transaction.Amount - FromSavingsAccountToChange.Balance) > 50)
                    {
                        return View("Error", new string[] { "The transaction exceeds the $50 overdraft limit. Please try to input a transfer amount within Balance range or $50 overdraft limit range" });
                    }
                    else
                    {
                        Transaction TransactionODFee = new Transaction();
                        TransactionODFee.Amount = 30;
                        FromSavingsAccountToChange.Balance -= TransactionODFee.Amount;
                        TransactionODFee.TransactionDate = transaction.TransactionDate;
                        TransactionODFee.TransactionType = "Fee";
                        TransactionODFee.isBeingDisputed = false;
                        TransactionODFee.EmployeeComments = "";
                        TransactionODFee.isPending = false;
                        TransactionODFee.Description = "Overdraft Fee";

                        TransactionODFee.SavingsAccountAffected = FromSavingsAccountToChange;
                        EmailMessaging.SendEmail(transaction.SavingsAccountAffected.Customer.Email, "Overdraft", "Your account is now in overdraft status. ");


                        
                        FromSavingsAccountToChange.Transactions.Add(TransactionODFee);
                        db.Transactions.Add(TransactionODFee);




                    }


                }

           

                FromSavingsAccountToChange.Balance -= transaction.Amount;
                ToStockPortfolioToChange.CashValueBalance += transaction.Amount;

                Transaction TransactionFrom = new Transaction();
                TransactionFrom.Amount = transaction.Amount;
                TransactionFrom.TransactionDate = transaction.TransactionDate;
                TransactionFrom.TransactionType = "Transfer";
                TransactionFrom.isBeingDisputed = false;
                TransactionFrom.EmployeeComments = "";
                TransactionFrom.isPending = false;

                Transaction TransactionTo = new Transaction();
                TransactionTo.Amount = transaction.Amount;
                TransactionTo.TransactionDate = transaction.TransactionDate;
                TransactionTo.TransactionType = "Transfer";
                TransactionTo.isBeingDisputed = false;
                TransactionTo.EmployeeComments = "";
                TransactionTo.isPending = false;

                TransactionFrom.Description = "Transfer From Account " + FromSavingsAccountToChange.AccountNumber.ToString();
                TransactionTo.Description = "Transfer To Account " + ToStockPortfolioToChange.AccountNumber.ToString();
                TransactionFrom.StockPortfolioAffected = ToStockPortfolioToChange;
                TransactionTo.SavingsAccountAffected = FromSavingsAccountToChange;

                FromSavingsAccountToChange.Transactions.Add(TransactionTo);
                ToStockPortfolioToChange.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionTo);
                db.SaveChanges();
                return RedirectToAction("TransferConfirmationMessage", "Transfers");
            }

            ViewBag.ToStock = GetStockPortfolio();
            ViewBag.FromSavingsAccounts = GetSavingsAccounts();
            return View(transaction);
        }

        //GET: Transfers/SavingsToIRA
        public ActionResult SavingsToIRA()
        {
            ViewBag.ToIRAccount = GetIRAccount();

            ViewBag.FromSavingsAccounts = GetSavingsAccounts();
            return View();
        }

        //POST: Transfers/SavingsToIRA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SavingsToIRA([Bind(Include = "TransactionID,TransactionDate,Amount")] Transaction transaction, int FromSavingsAccountID, int ToIRAccountID)
        {
            if (ModelState.IsValid)
            {
                SavingsAccount FromSavingsAccountToChange = db.SavingsAccounts.Find(FromSavingsAccountID);
                IRAccount ToIRAccountToChange = db.IRAccounts.Find(ToIRAccountID);


                if (FromSavingsAccountToChange.Balance < 0)
                {
                    return View("Error", new string[] { "You cannot transfer money with a negative balance" });

                }





                if (transaction.Amount > FromSavingsAccountToChange.Balance)
                {
                    if ((transaction.Amount - FromSavingsAccountToChange.Balance) > 50)
                    {
                        return View("Error", new string[] { "The transaction exceeds the $50 overdraft limit. Please try to input a transfer amount within Balance range or $50 overdraft limit range" });
                    }
                    else
                    {
                        Transaction TransactionODFee = new Transaction();
                        TransactionODFee.Amount = 30;
                        FromSavingsAccountToChange.Balance -= TransactionODFee.Amount;
                        TransactionODFee.TransactionDate = transaction.TransactionDate;
                        TransactionODFee.TransactionType = "Fee";
                        TransactionODFee.isBeingDisputed = false;
                        TransactionODFee.EmployeeComments = "";
                        TransactionODFee.isPending = false;
                        TransactionODFee.Description = "Overdraft Fee";

                        TransactionODFee.SavingsAccountAffected = FromSavingsAccountToChange;
                        EmailMessaging.SendEmail(transaction.SavingsAccountAffected.Customer.Email, "Overdraft", "Your account is now in overdraft status. ");


                        
                        FromSavingsAccountToChange.Transactions.Add(TransactionODFee);
                        db.Transactions.Add(TransactionODFee);




                    }


                }

                FromSavingsAccountToChange.Balance -= transaction.Amount;
                ToIRAccountToChange.Balance += transaction.Amount;

                if (transaction.Amount > ToIRAccountToChange.MaxContribution)
                {
                    return RedirectToAction("MaxContribTransferSaving", new { id = FromSavingsAccountID });
                }
                else
                {
                    transaction.isPending = false;
                    ToIRAccountToChange.MaxContribution -= transaction.Amount;

                }


                Transaction TransactionFrom = new Transaction();
                TransactionFrom.Amount = transaction.Amount;
                TransactionFrom.TransactionDate = transaction.TransactionDate;
                TransactionFrom.TransactionType = "Transfer";
                TransactionFrom.isBeingDisputed = false;
                TransactionFrom.EmployeeComments = "";
                TransactionFrom.isPending = false;

                Transaction TransactionTo = new Transaction();
                TransactionTo.Amount = transaction.Amount;
                TransactionTo.TransactionDate = transaction.TransactionDate;
                TransactionTo.TransactionType = "Transfer";
                TransactionTo.isBeingDisputed = false;
                TransactionTo.EmployeeComments = "";
                TransactionTo.isPending = false;

                TransactionFrom.Description = "Transfer From Account " + FromSavingsAccountToChange.AccountNumber.ToString();
                TransactionTo.Description = "Transfer To Account " + ToIRAccountToChange.AccountNumber.ToString();
                TransactionFrom.IRAccountAffected = ToIRAccountToChange;
                TransactionTo.SavingsAccountAffected = FromSavingsAccountToChange;

                FromSavingsAccountToChange.Transactions.Add(TransactionTo);
                ToIRAccountToChange.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionTo);
                db.SaveChanges();
                return RedirectToAction("TransferConfirmationMessage", "Transfers");
            }

            ViewBag.ToIRAccount = GetIRAccount();
            ViewBag.FromSavings = GetSavingsAccounts();
            return View(transaction);
        }

        //GET: Transfers/StockPortfolioToChecking
        public ActionResult StockPortfolioToChecking()
        {
            ViewBag.FromStock = GetStockPortfolio();
            ViewBag.ToCheckingAccounts = GetCheckingAccounts();
            return View();
        }

        //POST: Transfers/StockPortfolioToChecking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StockPortfolioToChecking([Bind(Include = "TransactionID,TransactionDate,Amount")] Transaction transaction, int ToCheckingAccountID, int FromStockPortfolioID)
        {
            if (ModelState.IsValid)
            {
                StockPortfolio FromStockPortfolioToChange = db.StockPortfolios.Find(FromStockPortfolioID);
                CheckingAccount ToCheckingAccountToChange = db.CheckingAccounts.Find(ToCheckingAccountID);


                if (FromStockPortfolioToChange.CashValueBalance < 0)
                {
                    return View("Error", new string[] { "You cannot transfer money with a negative balance" });

                }





                if (transaction.Amount > FromStockPortfolioToChange.CashValueBalance)
                {
                    if ((transaction.Amount - FromStockPortfolioToChange.CashValueBalance) > 50)
                    {
                        return View("Error", new string[] { "The transaction exceeds the $50 overdraft limit. Please try to input a transfer amount within Balance range or $50 overdraft limit range" });
                    }
                    else
                    {
                        Transaction TransactionODFee = new Transaction();
                        TransactionODFee.Amount = 30;
                        FromStockPortfolioToChange.CashValueBalance -= TransactionODFee.Amount;
                        TransactionODFee.TransactionDate = transaction.TransactionDate;
                        TransactionODFee.TransactionType = "Fee";
                        TransactionODFee.isBeingDisputed = false;
                        TransactionODFee.EmployeeComments = "";
                        TransactionODFee.isPending = false;
                        TransactionODFee.Description = "Overdraft Fee";

                        TransactionODFee.StockPortfolioAffected = FromStockPortfolioToChange;

                        EmailMessaging.SendEmail(transaction.StockPortfolioAffected.Customer.Email, "Overdraft", "Your account is now in overdraft status. ");


                        FromStockPortfolioToChange.Transactions.Add(TransactionODFee);
                        db.Transactions.Add(TransactionODFee);




                    }


                }


                FromStockPortfolioToChange.CashValueBalance -= transaction.Amount;
                ToCheckingAccountToChange.Balance += transaction.Amount;

                Transaction TransactionFrom = new Transaction();
                TransactionFrom.Amount = transaction.Amount;
                TransactionFrom.TransactionDate = transaction.TransactionDate;
                TransactionFrom.TransactionType = "Transfer";
                TransactionFrom.isBeingDisputed = false;
                TransactionFrom.EmployeeComments = "";
                TransactionFrom.isPending = false;

                Transaction TransactionTo = new Transaction();
                TransactionTo.Amount = transaction.Amount;
                TransactionTo.TransactionDate = transaction.TransactionDate;
                TransactionTo.TransactionType = "Transfer";
                TransactionTo.isBeingDisputed = false;
                TransactionTo.EmployeeComments = "";
                TransactionTo.isPending = false;

                TransactionFrom.Description = "Transfer From Account " + FromStockPortfolioToChange.AccountNumber.ToString();
                TransactionTo.Description = "Transfer To Account " + ToCheckingAccountToChange.AccountNumber.ToString();
                TransactionFrom.CheckingAccountAffected = ToCheckingAccountToChange;
                TransactionTo.StockPortfolioAffected = FromStockPortfolioToChange;

                FromStockPortfolioToChange.Transactions.Add(TransactionTo);
                ToCheckingAccountToChange.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionTo);
                db.SaveChanges();
                return RedirectToAction("TransferConfirmationMessage", "Transfers");
            }

            ViewBag.FromStock = GetStockPortfolio();
            ViewBag.ToCheckingAccounts = GetCheckingAccounts();
            return View(transaction);
        }

        //GET: Transfers/StockPortfolioToSavings
        public ActionResult StockPortfolioToSavings()
        {
            ViewBag.FromStock = GetStockPortfolio();
            ViewBag.ToSavingsAccounts = GetSavingsAccounts();
            return View();
        }

        //POST: Transfers/StockPortfolioToSavings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StockPortfolioToSavings([Bind(Include = "TransactionID,TransactionDate,Amount")] Transaction transaction, int ToSavingsAccountID, int FromStockPortfolioID)
        {
            if (ModelState.IsValid)
            {
                StockPortfolio FromStockPortfolioToChange = db.StockPortfolios.Find(FromStockPortfolioID);
                SavingsAccount ToSavingsAccountToChange = db.SavingsAccounts.Find(ToSavingsAccountID);

                if (FromStockPortfolioToChange.CashValueBalance < 0)
                {
                    return View("Error", new string[] { "You cannot transfer money with a negative balance" });

                }





                if (transaction.Amount > FromStockPortfolioToChange.CashValueBalance)
                {
                    if ((transaction.Amount - FromStockPortfolioToChange.CashValueBalance) > 50)
                    {
                        return View("Error", new string[] { "The transaction exceeds the $50 overdraft limit. Please try to input a transfer amount within Balance range or $50 overdraft limit range" });
                    }
                    else
                    {
                        Transaction TransactionODFee = new Transaction();
                        TransactionODFee.Amount = 30;
                        FromStockPortfolioToChange.CashValueBalance -= TransactionODFee.Amount;
                        TransactionODFee.TransactionDate = transaction.TransactionDate;
                        TransactionODFee.TransactionType = "Fee";
                        TransactionODFee.isBeingDisputed = false;
                        TransactionODFee.EmployeeComments = "";
                        TransactionODFee.isPending = false;
                        TransactionODFee.Description = "Overdraft Fee";

                        TransactionODFee.StockPortfolioAffected = FromStockPortfolioToChange;

                        EmailMessaging.SendEmail(transaction.StockPortfolioAffected.Customer.Email, "Overdraft", "Your account is now in overdraft status. ");


                        FromStockPortfolioToChange.Transactions.Add(TransactionODFee);
                        db.Transactions.Add(TransactionODFee);




                    }


                }


                FromStockPortfolioToChange.CashValueBalance -= transaction.Amount;
                ToSavingsAccountToChange.Balance += transaction.Amount;

                Transaction TransactionFrom = new Transaction();
                TransactionFrom.Amount = transaction.Amount;
                TransactionFrom.TransactionDate = transaction.TransactionDate;
                TransactionFrom.TransactionType = "Transfer";
                TransactionFrom.isBeingDisputed = false;
                TransactionFrom.EmployeeComments = "";
                TransactionFrom.isPending = false;

                Transaction TransactionTo = new Transaction();
                TransactionTo.Amount = transaction.Amount;
                TransactionTo.TransactionDate = transaction.TransactionDate;
                TransactionTo.TransactionType = "Transfer";
                TransactionTo.isBeingDisputed = false;
                TransactionTo.EmployeeComments = "";
                TransactionTo.isPending = false;

                TransactionFrom.Description = "Transfer From Account " + FromStockPortfolioToChange.AccountNumber.ToString();
                TransactionTo.Description = "Transfer To Account " + ToSavingsAccountToChange.AccountNumber.ToString();
                TransactionFrom.SavingsAccountAffected = ToSavingsAccountToChange;
                TransactionTo.StockPortfolioAffected = FromStockPortfolioToChange;

                FromStockPortfolioToChange.Transactions.Add(TransactionTo);
                ToSavingsAccountToChange.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionTo);
                db.SaveChanges();
                return RedirectToAction("TransferConfirmationMessage", "Transfers");
            }

            ViewBag.FromStock = GetStockPortfolio();
            ViewBag.ToSavingsAccounts = GetSavingsAccounts();
            return View(transaction);
        }

        //GET: Transactions/StockPortfolioToIRA
        public ActionResult StockPortfolioToIRA()
        {
            ViewBag.FromStock = GetStockPortfolio();
            ViewBag.ToIRAccount = GetIRAccount();

            return View();
        }


        //POST: Transactions/StockPortfolioToIRA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult StockPortfolioToIRA([Bind(Include = "TransactionID,TransactionDate,Amount")] Transaction transaction, int ToIRAccountID, int FromStockPortfolioID)
        {
            if (ModelState.IsValid)
            {
                StockPortfolio FromStockPortfolioToChange = db.StockPortfolios.Find(FromStockPortfolioID);
                IRAccount ToIRAccountToChange = db.IRAccounts.Find(ToIRAccountID);
                FromStockPortfolioToChange.CashValueBalance -= transaction.Amount;
                ToIRAccountToChange.Balance += transaction.Amount;

                if (transaction.Amount > ToIRAccountToChange.MaxContribution)
                {
                    return RedirectToAction("MaxContribTransferStock");
                }
                else
                {
                    transaction.isPending = false;
                    ToIRAccountToChange.MaxContribution -= transaction.Amount;

                }



                Transaction TransactionFrom = new Transaction();
                TransactionFrom.Amount = transaction.Amount;
                TransactionFrom.TransactionDate = transaction.TransactionDate;
                TransactionFrom.TransactionType = "Transfer";
                TransactionFrom.isBeingDisputed = false;
                TransactionFrom.EmployeeComments = "";
                TransactionFrom.isPending = false;

                Transaction TransactionTo = new Transaction();
                TransactionTo.Amount = transaction.Amount;
                TransactionTo.TransactionDate = transaction.TransactionDate;
                TransactionTo.TransactionType = "Transfer";
                TransactionTo.isBeingDisputed = false;
                TransactionTo.EmployeeComments = "";
                TransactionTo.isPending = false;

                TransactionFrom.Description = "Transfer From Account " + FromStockPortfolioToChange.AccountNumber.ToString();
                TransactionTo.Description = "Transfer To Account " + ToIRAccountToChange.AccountNumber.ToString();
                TransactionFrom.IRAccountAffected = ToIRAccountToChange;
                TransactionTo.StockPortfolioAffected = FromStockPortfolioToChange;

                FromStockPortfolioToChange.Transactions.Add(TransactionTo);
                ToIRAccountToChange.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionTo);
                db.SaveChanges();
                return RedirectToAction("TransferConfirmationMessage", "Transfers");
            }

            ViewBag.FromStock = GetStockPortfolio();
            ViewBag.ToIRAccount = GetIRAccount();
            return View(transaction);
        }

        //GET: Transfers/IRAtoChecking
        public ActionResult IRAtoChecking()
        {
            ViewBag.FromIRAccount = GetIRAccount();
            ViewBag.ToCheckingAccounts = GetCheckingAccounts();
            return View();
        }


        //POST: Transfers/IRAtoChecking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IRAToChecking([Bind(Include = "TransactionID,TransactionDate,Amount")] Transaction transaction, int ToCheckingAccountID, int FromIRAccountID, FeeOptions SelectedFee)
        {
            if (ModelState.IsValid)
            {
                IRAccount FromIRAccountToChange = db.IRAccounts.Find(FromIRAccountID);
                CheckingAccount ToCheckingAccountToChange = db.CheckingAccounts.Find(ToCheckingAccountID);

                if (FromIRAccountToChange.Balance < 0)
                {
                    return View("Error", new string[] { "You cannot transfer money with a negative balance" });

                }





                if (transaction.Amount > FromIRAccountToChange.Balance)
                {
                    if ((transaction.Amount - FromIRAccountToChange.Balance) > 50)
                    {
                        return View("Error", new string[] { "The transaction exceeds the $50 overdraft limit. Please try to input a transfer amount within Balance range or $50 overdraft limit range" });
                    }
                    else
                    {
                        Transaction TransactionODFee = new Transaction();
                        TransactionODFee.Amount = 30;
                        FromIRAccountToChange.Balance -= TransactionODFee.Amount;
                        TransactionODFee.TransactionDate = transaction.TransactionDate;
                        TransactionODFee.TransactionType = "Fee";
                        TransactionODFee.isBeingDisputed = false;
                        TransactionODFee.EmployeeComments = "";
                        TransactionODFee.isPending = false;
                        TransactionODFee.Description = "Overdraft Fee";


                        TransactionODFee.IRAccountAffected = FromIRAccountToChange;

                        EmailMessaging.SendEmail(transaction.IRAccountAffected.Customer.Email, "Overdraft", "Your account is now in overdraft status. ");


                        FromIRAccountToChange.Transactions.Add(TransactionODFee);
                        db.Transactions.Add(TransactionODFee);




                    }


                }


                FromIRAccountToChange.Balance -= transaction.Amount;
                ToCheckingAccountToChange.Balance += transaction.Amount;

                if (FromIRAccountToChange.isQualified == false)
                {
                    if (transaction.Amount > 3000)
                    {
                        return View("Error", new string[] { "You cannot withdraw more than $3,000." });

                    }


                    switch (SelectedFee)
                    {
                        case FeeOptions.IncludeFee:
                            FromIRAccountToChange.Balance -= transaction.Amount;
                            transaction.Amount -= 30;
                            Transaction transactionIncludeFee = new Transaction();
                            transactionIncludeFee.TransactionType = "Fee";
                            transactionIncludeFee.TransactionDate = DateTime.Now;
                            transactionIncludeFee.Amount = 30;
                            transactionIncludeFee.isBeingDisputed = false;
                            transactionIncludeFee.EmployeeComments = "";
                            transactionIncludeFee.isPending = false;
                            transactionIncludeFee.IRAccountAffected = FromIRAccountToChange;
                            FromIRAccountToChange.Transactions.Add(transactionIncludeFee);
                            db.Transactions.Add(transactionIncludeFee);



                            transaction.IRAccountAffected = FromIRAccountToChange;
                            FromIRAccountToChange.Transactions.Add(transaction);
                            db.Transactions.Add(transaction);
                            db.SaveChanges();



                            break;

                        case FeeOptions.AdditionalFee:
                            FromIRAccountToChange.Balance -= (transaction.Amount + 30);
                            Transaction transactionAddFee = new Transaction();
                            transactionAddFee.TransactionType = "Fee";
                            transactionAddFee.TransactionDate = DateTime.Now;
                            transactionAddFee.Amount = 30;
                            transactionAddFee.isBeingDisputed = false;
                            transactionAddFee.EmployeeComments = "";
                            transactionAddFee.isPending = false;
                            transactionAddFee.IRAccountAffected = FromIRAccountToChange;
                            FromIRAccountToChange.Transactions.Add(transactionAddFee);
                            db.Transactions.Add(transactionAddFee);

                            transaction.IRAccountAffected = FromIRAccountToChange;
                            FromIRAccountToChange.Transactions.Add(transaction);
                            db.Transactions.Add(transaction);
                            db.SaveChanges();

                            break;

                    }

                }

                Transaction TransactionFrom = new Transaction();
                TransactionFrom.Amount = transaction.Amount;
                TransactionFrom.TransactionDate = transaction.TransactionDate;
                TransactionFrom.TransactionType = "Transfer";
                TransactionFrom.isBeingDisputed = false;
                TransactionFrom.EmployeeComments = "";
                TransactionFrom.isPending = false;

                Transaction TransactionTo = new Transaction();
                TransactionTo.Amount = transaction.Amount;
                TransactionTo.TransactionDate = transaction.TransactionDate;
                TransactionTo.TransactionType = "Transfer";
                TransactionTo.isBeingDisputed = false;
                TransactionTo.EmployeeComments = "";
                TransactionTo.isPending = false;

                TransactionFrom.Description = "Transfer From Account " + FromIRAccountToChange.AccountNumber.ToString();
                TransactionTo.Description = "Transfer To Account " + ToCheckingAccountToChange.AccountNumber.ToString();
                TransactionFrom.CheckingAccountAffected = ToCheckingAccountToChange;
                TransactionTo.IRAccountAffected = FromIRAccountToChange;

                FromIRAccountToChange.Transactions.Add(TransactionTo);
                ToCheckingAccountToChange.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionTo);
                db.SaveChanges();
                return RedirectToAction("TransferConfirmationMessage", "Transfers");
            }

            ViewBag.FromIRAccount = GetIRAccount();
            ViewBag.ToCheckingAccounts = GetCheckingAccounts();
            return View(transaction);
        }


        


        //GET: Transfers/IRAtoSavings
        public ActionResult IRAToSavings()
        {
            ViewBag.FromIRAccount = GetIRAccount();

            ViewBag.ToSavingsAccounts = GetSavingsAccounts();
            return View();
        }

        //POST: Transfers/IRAtoSavings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IRAtoSavings([Bind(Include = "TransactionID,TransactionDate,Amount")] Transaction transaction, int ToSavingsAccountID, int FromIRAccountID, FeeOptions SelectedFee)
        {
            if (ModelState.IsValid)
            {
                IRAccount FromIRAccountToChange = db.IRAccounts.Find(FromIRAccountID);
                SavingsAccount ToSavingsAccountToChange = db.SavingsAccounts.Find(ToSavingsAccountID);

                if (FromIRAccountToChange.Balance < 0)
                {
                    return View("Error", new string[] { "You cannot transfer money with a negative balance" });

                }





                if (transaction.Amount > FromIRAccountToChange.Balance)
                {
                    if ((transaction.Amount - FromIRAccountToChange.Balance) > 50)
                    {
                        return View("Error", new string[] { "The transaction exceeds the $50 overdraft limit. Please try to input a transfer amount within Balance range or $50 overdraft limit range" });
                    }
                    else
                    {
                        Transaction TransactionODFee = new Transaction();
                        TransactionODFee.Amount = 30;
                        FromIRAccountToChange.Balance -= TransactionODFee.Amount;
                        TransactionODFee.TransactionDate = transaction.TransactionDate;
                        TransactionODFee.TransactionType = "Fee";
                        TransactionODFee.isBeingDisputed = false;
                        TransactionODFee.EmployeeComments = "";
                        TransactionODFee.isPending = false;
                        TransactionODFee.Description = "Overdraft Fee";

                        TransactionODFee.IRAccountAffected = FromIRAccountToChange;

                        EmailMessaging.SendEmail(transaction.IRAccountAffected.Customer.Email, "Overdraft", "Your account is now in overdraft status. ");


                        FromIRAccountToChange.Transactions.Add(TransactionODFee);
                        db.Transactions.Add(TransactionODFee);




                    }


                }

                FromIRAccountToChange.Balance -= transaction.Amount;
                ToSavingsAccountToChange.Balance += transaction.Amount;

                if (FromIRAccountToChange.isQualified == false)
                {
                    if (transaction.Amount > 3000)
                    {
                        return View("Error", new string[] { "You cannot withdraw more than $3,000." });

                    }


                    switch (SelectedFee)
                    {
                        case FeeOptions.IncludeFee:
                            FromIRAccountToChange.Balance -= transaction.Amount;
                            transaction.Amount -= 30;
                            Transaction transactionIncludeFee = new Transaction();
                            transactionIncludeFee.TransactionType = "Fee";
                            transactionIncludeFee.TransactionDate = DateTime.Now;
                            transactionIncludeFee.Amount = 30;
                            transactionIncludeFee.isBeingDisputed = false;
                            transactionIncludeFee.EmployeeComments = "";
                            transactionIncludeFee.isPending = false;
                            transactionIncludeFee.IRAccountAffected = FromIRAccountToChange;
                            FromIRAccountToChange.Transactions.Add(transactionIncludeFee);
                            db.Transactions.Add(transactionIncludeFee);



                            transaction.IRAccountAffected = FromIRAccountToChange;
                            FromIRAccountToChange.Transactions.Add(transaction);
                            db.Transactions.Add(transaction);
                            db.SaveChanges();

                            break;

                        case FeeOptions.AdditionalFee:
                            FromIRAccountToChange.Balance -= (transaction.Amount + 30);
                            Transaction transactionAddFee = new Transaction();
                            transactionAddFee.TransactionType = "Fee";
                            transactionAddFee.TransactionDate = DateTime.Now;
                            transactionAddFee.Amount = 30;
                            transactionAddFee.isBeingDisputed = false;
                            transactionAddFee.EmployeeComments = "";
                            transactionAddFee.isPending = false;
                            transactionAddFee.IRAccountAffected = FromIRAccountToChange;
                            FromIRAccountToChange.Transactions.Add(transactionAddFee);
                            db.Transactions.Add(transactionAddFee);

                            transaction.IRAccountAffected = FromIRAccountToChange;
                            FromIRAccountToChange.Transactions.Add(transaction);
                            db.Transactions.Add(transaction);
                            db.SaveChanges();
                            break;

                    }

                }



                Transaction TransactionFrom = new Transaction();
                TransactionFrom.Amount = transaction.Amount;
                TransactionFrom.TransactionDate = transaction.TransactionDate;
                TransactionFrom.TransactionType = "Transfer";
                TransactionFrom.isBeingDisputed = false;
                TransactionFrom.EmployeeComments = "";
                TransactionFrom.isPending = false;

                Transaction TransactionTo = new Transaction();
                TransactionTo.Amount = transaction.Amount;
                TransactionTo.TransactionDate = transaction.TransactionDate;
                TransactionTo.TransactionType = "Transfer";
                TransactionTo.isBeingDisputed = false;
                TransactionTo.EmployeeComments = "";
                TransactionTo.isPending = false;

                TransactionFrom.Description = "Transfer From Account " + FromIRAccountToChange.AccountNumber.ToString();
                TransactionTo.Description = "Transfer To Account " + ToSavingsAccountToChange.AccountNumber.ToString();
                TransactionFrom.SavingsAccountAffected = ToSavingsAccountToChange;
                TransactionTo.IRAccountAffected = FromIRAccountToChange;

                FromIRAccountToChange.Transactions.Add(TransactionTo);
                ToSavingsAccountToChange.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionTo);
                db.SaveChanges();
                return RedirectToAction("TransferConfirmationMessage", "Transfers");
            }

            ViewBag.FromIRAccount = GetIRAccount();

            ViewBag.ToSavingsAccounts = GetSavingsAccounts();
            return View(transaction);
        }


        //GET: Transfers/IRAtoStockPortfolio
        public ActionResult IRAToStockPortfolio()
        {
            ViewBag.FromIRAccount = GetIRAccount();
            ViewBag.ToStock = GetStockPortfolio();
            return View();
        }
        //POST: Transfers/IRAtoStockPortfolio
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult IRAToStockPortfolio([Bind(Include = "TransactionID,TransactionDate,Amount")] Transaction transaction, int FromIRAccountID, int ToStockPortfolioID, FeeOptions SelectedFee)
        {
            if (ModelState.IsValid)
            {
                IRAccount FromIRAccountToChange = db.IRAccounts.Find(FromIRAccountID);
                StockPortfolio ToStockPortfolioToChange = db.StockPortfolios.Find(ToStockPortfolioID);

                if (FromIRAccountToChange.Balance < 0)
                {
                    return View("Error", new string[] { "You cannot transfer money with a negative balance" });

                }





                if (transaction.Amount > FromIRAccountToChange.Balance)
                {
                    if ((transaction.Amount - FromIRAccountToChange.Balance) > 50)
                    {
                        return View("Error", new string[] { "The transaction exceeds the $50 overdraft limit. Please try to input a transfer amount within Balance range or $50 overdraft limit range" });
                    }
                    else
                    {
                        Transaction TransactionODFee = new Transaction();
                        TransactionODFee.Amount = 30;
                        FromIRAccountToChange.Balance -= TransactionODFee.Amount;
                        TransactionODFee.TransactionDate = transaction.TransactionDate;
                        TransactionODFee.TransactionType = "Fee";
                        TransactionODFee.isBeingDisputed = false;
                        TransactionODFee.EmployeeComments = "";
                        TransactionODFee.isPending = false;
                        TransactionODFee.Description = "Overdraft Fee";

                        TransactionODFee.IRAccountAffected = FromIRAccountToChange;

                        EmailMessaging.SendEmail(transaction.IRAccountAffected.Customer.Email, "Overdraft", "Your account is now in overdraft status. ");


                        FromIRAccountToChange.Transactions.Add(TransactionODFee);
                        db.Transactions.Add(TransactionODFee);




                    }


                }

                FromIRAccountToChange.Balance -= transaction.Amount;
                ToStockPortfolioToChange.CashValueBalance += transaction.Amount;

                if (FromIRAccountToChange.isQualified == false)
                {
                    if (transaction.Amount > 3000)
                    {
                        return View("Error", new string[] { "You cannot withdraw more than $3,000." });

                    }


                    switch (SelectedFee)
                    {
                        case FeeOptions.IncludeFee:
                            FromIRAccountToChange.Balance -= transaction.Amount;
                            transaction.Amount -= 30;
                            Transaction transactionIncludeFee = new Transaction();
                            transactionIncludeFee.TransactionType = "Fee";
                            transactionIncludeFee.TransactionDate = DateTime.Now;
                            transactionIncludeFee.Amount = 30;
                            transactionIncludeFee.isBeingDisputed = false;
                            transactionIncludeFee.EmployeeComments = "";
                            transactionIncludeFee.isPending = false;
                            transactionIncludeFee.IRAccountAffected = FromIRAccountToChange;
                            FromIRAccountToChange.Transactions.Add(transactionIncludeFee);
                            db.Transactions.Add(transactionIncludeFee);



                            transaction.IRAccountAffected = FromIRAccountToChange;
                            FromIRAccountToChange.Transactions.Add(transaction);
                            db.Transactions.Add(transaction);
                            db.SaveChanges();

                            break;

                        case FeeOptions.AdditionalFee:
                            FromIRAccountToChange.Balance -= (transaction.Amount + 30);
                            Transaction transactionAddFee = new Transaction();
                            transactionAddFee.TransactionType = "Fee";
                            transactionAddFee.TransactionDate = DateTime.Now;
                            transactionAddFee.Amount = 30;
                            transactionAddFee.isBeingDisputed = false;
                            transactionAddFee.EmployeeComments = "";
                            transactionAddFee.isPending = false;
                            transactionAddFee.IRAccountAffected = FromIRAccountToChange;
                            FromIRAccountToChange.Transactions.Add(transactionAddFee);
                            db.Transactions.Add(transactionAddFee);

                            transaction.IRAccountAffected = FromIRAccountToChange;
                            FromIRAccountToChange.Transactions.Add(transaction);
                            db.Transactions.Add(transaction);
                            db.SaveChanges();
                            break;

                    }

                }


                Transaction TransactionFrom = new Transaction();
                TransactionFrom.Amount = transaction.Amount;
                TransactionFrom.TransactionDate = transaction.TransactionDate;
                TransactionFrom.TransactionType = "Transfer";
                TransactionFrom.isBeingDisputed = false;
                TransactionFrom.EmployeeComments = "";
                TransactionFrom.isPending = false;

                Transaction TransactionTo = new Transaction();
                TransactionTo.Amount = transaction.Amount;
                TransactionTo.TransactionDate = transaction.TransactionDate;
                TransactionTo.TransactionType = "Transfer";
                TransactionTo.isBeingDisputed = false;
                TransactionTo.EmployeeComments = "";
                TransactionTo.isPending = false;

                TransactionFrom.Description = "Transfer From Account " + FromIRAccountToChange.AccountNumber.ToString();
                TransactionTo.Description = "Transfer To Account " + ToStockPortfolioToChange.AccountNumber.ToString();
                TransactionFrom.StockPortfolioAffected = ToStockPortfolioToChange;
                TransactionTo.IRAccountAffected = FromIRAccountToChange;

                FromIRAccountToChange.Transactions.Add(TransactionTo);
                ToStockPortfolioToChange.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionFrom);
                db.Transactions.Add(TransactionTo);
                db.SaveChanges();
                return RedirectToAction("TransferConfirmationMessage", "Transfers");
            }

            ViewBag.FromIRAccount = GetIRAccount();
            ViewBag.ToStock = GetStockPortfolio();
            return View(transaction);
        }


        //GET: IRAccount/MaxContribTransferChecking
        public ActionResult MaxContribTransferChecking(int id)
        {
            CheckingAccount checkingAccount = db.CheckingAccounts.Find(id);
            List<CheckingAccount> CheckingAccount = new List<CheckingAccount>();
            CheckingAccount.Add(checkingAccount);
            ViewBag.CheckingAccount = new SelectList(CheckingAccount, "CheckingAccountID","AccountName");
            return View();
        }


        //POST: IRAccount/MaxContribTransferChecking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MaxContribTransferChecking(MaxOptions SelectedMaxOption, int FromCheckingAccountID, int ToIRAccountID)
        {

           
            
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            IRAccount IRAccountToChange = user.IRAccount;

            switch (SelectedMaxOption)
            {
                case MaxOptions.Auto:
                    {
                        CheckingAccount FromCheckingAccountToChange = db.CheckingAccounts.Find(FromCheckingAccountID);
                        IRAccount ToIRAccountToChange = db.IRAccounts.Find(ToIRAccountID);

                        if (IRAccountToChange.MaxContribution == 0)
                        {
                            return View("Error", new string[] { "You cannot deposit. You have maxed on your contribution." });
                        }

                        Transaction TransactionFrom = new Transaction();
                        TransactionFrom.Amount = IRAccountToChange.MaxContribution;
                        FromCheckingAccountToChange.Balance -= TransactionFrom.Amount;
                        TransactionFrom.TransactionDate = DateTime.Today;
                        TransactionFrom.TransactionType = "Transfer";
                        TransactionFrom.isBeingDisputed = false;
                        TransactionFrom.EmployeeComments = "";
                        TransactionFrom.isPending = false;

                        Transaction TransactionTo = new Transaction();
                        TransactionTo.Amount = IRAccountToChange.MaxContribution;
                        ToIRAccountToChange.Balance += TransactionFrom.Amount;
                        ToIRAccountToChange.MaxContribution -= TransactionFrom.Amount;
                        TransactionTo.TransactionDate = DateTime.Today;
                        TransactionTo.TransactionType = "Transfer";
                        TransactionTo.isBeingDisputed = false;
                        TransactionTo.EmployeeComments = "";
                        TransactionTo.isPending = false;

                        TransactionFrom.Description = "Transfer From Account "+FromCheckingAccountToChange.AccountNumber.ToString();
                        FromCheckingAccountToChange.AccountNumber.ToString();
                        TransactionTo.Description = "Transfer To Account " + ToIRAccountToChange.AccountNumber.ToString();
                        TransactionFrom.IRAccountAffected = ToIRAccountToChange;
                        TransactionTo.CheckingAccountAffected = FromCheckingAccountToChange;

                        FromCheckingAccountToChange.Transactions.Add(TransactionTo);
                        ToIRAccountToChange.Transactions.Add(TransactionFrom);
                        db.Transactions.Add(TransactionFrom);
                        db.Transactions.Add(TransactionTo);
                        db.SaveChanges();
                        return RedirectToAction("TransferConfirmationMessage", "Transfers");


                    }

                case MaxOptions.Input:
                    {

                        return RedirectToAction("CheckingToIRA", "Transfers");
                    }

                case MaxOptions.Abandon:
                    {
                        return RedirectToAction("MyBankAccounts", "Account");
                    }

            }

            return RedirectToAction("MyBankAccounts", "Account");
        }



        //GET: IRAccount/MaxContribTransferSaving
        public ActionResult MaxContribTransferSaving(int? id)
        {
            

            SavingsAccount savingsAccount = db.SavingsAccounts.Find(id);
            List<SavingsAccount> SavingsAccount = new List<SavingsAccount>();
            SavingsAccount.Add(savingsAccount);
            ViewBag.SavingsAccount = new SelectList(SavingsAccount, "SavingsAccountID", "AccountName");

            return View();
        }


        //POST: IRAccount/MaxContribTransferSaving
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MaxContribTransferSaving(MaxOptions SelectedMaxOption, int FromSavingsAccountID, int ToIRAccountID)
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            IRAccount IRAccountToChange = user.IRAccount;

            switch (SelectedMaxOption)
            {
                case MaxOptions.Auto:
                    {
                        SavingsAccount FromSavingsAccountToChange = db.SavingsAccounts.Find(FromSavingsAccountID);
                        IRAccount ToIRAccountToChange = db.IRAccounts.Find(ToIRAccountID);
                        if (IRAccountToChange.MaxContribution == 0)
                        {
                            return View("Error", new string[] { "You cannot deposit. You have maxed on your contribution." });
                        }

                        Transaction TransactionFrom = new Transaction();
                        TransactionFrom.Amount = IRAccountToChange.MaxContribution;
                        FromSavingsAccountToChange.Balance -= TransactionFrom.Amount;
                        TransactionFrom.TransactionDate = DateTime.Today;
                        TransactionFrom.TransactionType = "Transfer";
                        TransactionFrom.isBeingDisputed = false;
                        TransactionFrom.EmployeeComments = "";
                        TransactionFrom.isPending = false;

                        Transaction TransactionTo = new Transaction();
                        TransactionTo.Amount = IRAccountToChange.MaxContribution;
                        ToIRAccountToChange.Balance += TransactionFrom.Amount;
                        ToIRAccountToChange.MaxContribution -= TransactionFrom.Amount;
                        TransactionTo.TransactionDate = DateTime.Today;
                        TransactionTo.TransactionType = "Transfer";
                        TransactionTo.isBeingDisputed = false;
                        TransactionTo.EmployeeComments = "";
                        TransactionTo.isPending = false;

                        TransactionFrom.Description = "Transfer From Account " + FromSavingsAccountToChange.AccountNumber.ToString();
                        FromSavingsAccountToChange.AccountNumber.ToString();
                        TransactionTo.Description = "Transfer To Account " + ToIRAccountToChange.AccountNumber.ToString();
                        TransactionFrom.IRAccountAffected = ToIRAccountToChange;
                        TransactionTo.SavingsAccountAffected = FromSavingsAccountToChange;

                        FromSavingsAccountToChange.Transactions.Add(TransactionTo);
                        ToIRAccountToChange.Transactions.Add(TransactionFrom);
                        db.Transactions.Add(TransactionFrom);
                        db.Transactions.Add(TransactionTo);
                        db.SaveChanges();
                        return RedirectToAction("TransferConfirmationMessage", "Transfers");


                    }

                case MaxOptions.Input:
                    {

                        return RedirectToAction("CheckingToIRA", "Transfers");
                    }

                case MaxOptions.Abandon:
                    {
                        return RedirectToAction("MyBankAccounts", "Account");
                    }

            }

            return RedirectToAction("MyBankAccounts", "Account");
        }

        //GET: IRAccount/MaxContribTransferStock
        public ActionResult MaxContribTransferStock(int? id)
        {

            ViewBag.StockPortfolio = GetStockPortfolio();

            return View();
        }


        //POST: IRAccount/MaxContribTransferStock
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MaxContribTransferStock(MaxOptions SelectedMaxOption, int ToIRAccountID, int FromStockPortfolioID)
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            IRAccount IRAccountToChange = user.IRAccount;

            switch (SelectedMaxOption)
            {
                case MaxOptions.Auto:
                    {
                        StockPortfolio FromStockPortfolioToChange = db.StockPortfolios.Find(FromStockPortfolioID);
                        IRAccount ToIRAccountToChange = db.IRAccounts.Find(ToIRAccountID);

                        if (IRAccountToChange.MaxContribution == 0)

                            if (IRAccountToChange.MaxContribution == 0)
                        {
                            return View("Error", new string[] { "You cannot deposit. You have maxed on your contribution." });
                        }

                        Transaction TransactionFrom = new Transaction();
                        TransactionFrom.Amount = IRAccountToChange.MaxContribution;
                        FromStockPortfolioToChange.CashValueBalance -= TransactionFrom.Amount;
                        TransactionFrom.TransactionDate = DateTime.Today;
                        TransactionFrom.TransactionType = "Transfer";
                        TransactionFrom.isBeingDisputed = false;
                        TransactionFrom.EmployeeComments = "";
                        TransactionFrom.isPending = false;

                        Transaction TransactionTo = new Transaction();
                        TransactionTo.Amount = IRAccountToChange.MaxContribution;
                        ToIRAccountToChange.Balance += TransactionFrom.Amount;
                        ToIRAccountToChange.MaxContribution -= TransactionFrom.Amount;
                        TransactionTo.TransactionDate = DateTime.Today;
                        TransactionTo.TransactionType = "Transfer";
                        TransactionTo.isBeingDisputed = false;
                        TransactionTo.EmployeeComments = "";
                        TransactionTo.isPending = false;

                        TransactionFrom.Description = "Transfer From Account " + FromStockPortfolioToChange.AccountNumber.ToString();
                        FromStockPortfolioToChange.AccountNumber.ToString();
                        TransactionTo.Description = "Transfer To Account " + ToIRAccountToChange.AccountNumber.ToString();
                        TransactionFrom.IRAccountAffected = ToIRAccountToChange;
                        TransactionTo.StockPortfolioAffected = FromStockPortfolioToChange;

                        FromStockPortfolioToChange.Transactions.Add(TransactionTo);
                        ToIRAccountToChange.Transactions.Add(TransactionFrom);
                        db.Transactions.Add(TransactionFrom);
                        db.Transactions.Add(TransactionTo);
                        db.SaveChanges();
                        return RedirectToAction("TransferConfirmationMessage", "Transfers");


                    }

                case MaxOptions.Input:
                    {

                        return RedirectToAction("CheckingToIRA", "Transfers");
                    }

                case MaxOptions.Abandon:
                    {
                        return RedirectToAction("MyBankAccounts", "Account");
                    }

            }

            return RedirectToAction("MyBankAccounts", "Account");
        }


        public SelectList GetCheckingAccounts()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            List<CheckingAccount> CheckingAccounts = user.CheckingAccounts;
            SelectList list = new SelectList(CheckingAccounts, "CheckingAccountID", "CheckingAccountDisplay");
            return list;
        }

        public SelectList GetSavingsAccounts()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            List<SavingsAccount> SavingsAccounts = user.SavingsAccounts;
            SelectList list = new SelectList(SavingsAccounts, "SavingsAccountID", "SavingsAccountDisplay");
            return list;
        }

        public SelectList GetStockPortfolio()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            StockPortfolio stockPortfolio = user.StockPortfolio;
            List<StockPortfolio> StockPortfolioList = new List<StockPortfolio>();
            StockPortfolioList.Add(stockPortfolio);
            SelectList list = new SelectList(StockPortfolioList, "StockPortfolioID", "StockPortfolioDisplay");
 
 
            return list;
        }
 
        public SelectList GetIRAccount()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            IRAccount iRAccount = user.IRAccount;
            List<IRAccount> IRAccountList = new List<IRAccount>();
            IRAccountList.Add(iRAccount);
            SelectList list = new SelectList(IRAccountList, "IRAccountID", "IRAccountDisplay");
             
 
            return list;
        }


    }
}