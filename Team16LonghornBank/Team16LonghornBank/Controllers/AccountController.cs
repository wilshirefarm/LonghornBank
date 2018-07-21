using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Team16LonghornBank.Models;
using Team16LonghornBank.Utilities;
using System.Data.Entity;

namespace Team16LonghornBank.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private AppUserManager _userManager;
        private AppDbContext db = new AppDbContext();

        public AccountController()
        {
        }

        public AccountController(AppUserManager userManager, ApplicationSignInManager signInManager )
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public AppUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<AppUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (HttpContext.User.Identity.IsAuthenticated) //user has been redirected here from a page they're not authorized to see
            {
                return View("Error", new string[] { "Access Denied" });
            }
            AuthenticationManager.SignOut(); //this removes any old cookies hanging around
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToAction("LoginAccountVerification", new { str = returnUrl });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        public ActionResult LoginAccountVerification(string str)
        {
            String UserId;
            UserId = User.Identity.GetUserId();
            AppUser user = db.Users.Find(UserId);
            if (User.IsInRole("Customer"))
            {
                if (!user.HasAccount)
                {
                    return RedirectToAction("ApplyForBankingAccount", "Account");
                }
                else
                {
                    return RedirectToAction("MyBankAccounts", "Account");
                }
            }
            //if (!user.HasAccount && User.IsInRole("Customer"))
            //{
            //    return RedirectToAction("ApplyForBankingAccount", "Account");
            //}
            else if (User.IsInRole("Employee") || User.IsInRole("Manager"))
            {
                if (user.IsTerminated)
                {
                    AuthenticationManager.SignOut();
                    return View("Error", new string[] { "You are terminated. You will be logged out." });
                }
                return RedirectToAction("EmployeePortal", "Manage");
            }
            else
            {
                return RedirectToLocal(str);
            }
        }


        //
        // GET: /Account/Register
        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                //TODO: Add fields to user here so they will be saved to do the database
                var user = new AppUser { UserName = model.Email, Email = model.Email, FirstName = model.FirstName, LastName = model.LastName, MI = model.MI, Address = model.Address, City = model.City, State = model.State, ZipCode = model.ZipCode, PhoneNumber = model.PhoneNumber, DOB = model.DOB, IsActive = true, HasAccount = false };
                var result = await UserManager.CreateAsync(user, model.Password);

                //TODO:  Once you get roles working, you may want to add users to roles upon creation
                // await UserManager.AddToRoleAsync(user.Id, "User");
                // --OR--
                // await UserManager.AddToRoleAsync(user.Id, "Employee");
                


                if (result.Succeeded)
                {
                    await UserManager.AddToRoleAsync(user.Id, "Customer");
                    await SignInManager.SignInAsync(user, isPersistent:false, rememberBrowser:false);
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // Send an email with this link
                    // string code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
                    // var callbackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");
                    
                    return RedirectToAction("ApplyForBankingAccount", "Account");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await UserManager.FindByNameAsync(model.Email);
          
                if (user == null || user.DOB.Year != model.DOB) // || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("IncorrectPasswordValidation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                //        string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id); 
                //        var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                //      await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");

               EmailMessaging.SendEmail(user.Email, "Reset Password", "Please reset your password by clicking the following link. "); 
               return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut();
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ExternalLoginFailure
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [Authorize(Roles = "Customer")]
        public ActionResult ApplyForBankingAccount()
        {
            AppUser customer = db.Users.Find(User.Identity.GetUserId());
            if (customer.IsActive)
            {
                ViewBag.AccountTypes = new SelectList(Utility.AccountTypes);
                return View();
            }
            return View("Error", new string[] { "Your account is inactive." });
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        [ValidateAntiForgeryToken]
        public ActionResult ApplyForBankingAccount(String AccountType)
        {
            if (AccountType == "Checking Account")
            {
                return RedirectToAction("Create", "CheckingAccounts");
            }
            else if (AccountType == "Savings Account")
            {
                return RedirectToAction("Create", "SavingsAccounts");
            }
            else if (AccountType == "IRA")
            {
                return RedirectToAction("Create", "IRAccounts");
            }
            else
            {
                return RedirectToAction("Create", "StockPortfolios");
            }
        }

        [Authorize]
        public ActionResult MyBankAccounts()
        {
            AppUser user = db.Users.Find(User.Identity.GetUserId());
            if (User.IsInRole("Customer"))
            {
                if (!user.HasAccount)
                {
                    return RedirectToAction("ApplyForBankingAccount", "Account");
                }

                if (user.StockPortfolio != null)
                {
                    StockPortfolio stockPortfolio = user.StockPortfolio;
                    foreach (var s in db.Stocks)
                    {
                        s.CurrentPrice = GetQuote.GetStock(s.TickerSymbol).PreviousClose;
                    }
                    foreach (var p in db.StockPurchases)
                    {
                        p.TotalStockValue = p.Stock.CurrentPrice * p.NumberOfShares;
                    }
                    if (stockPortfolio.StockPurchases.Count != 0)
                    {
                        stockPortfolio.StockPortionValue = 0;
                        foreach (var v in stockPortfolio.StockPurchases)
                        {
                            stockPortfolio.StockPortionValue += v.TotalStockValue;
                            v.ChangeInPrice = v.InitialSharePrice - v.Stock.CurrentPrice;
                            v.TotalChange = (v.NumberOfShares * v.InitialSharePrice) - v.TotalStockValue;
                            v.StockPurchaseDisplay = v.Stock.StockName + ", Current Price: $" + v.Stock.CurrentPrice.ToString() + ", Number of shares: " + v.NumberOfShares.ToString();
                        }
                    }
                    db.SaveChanges();
                }

                return View(user);
            }
            else
            {
                return View("Error", new string[] { "You are an employee, so you cannot have bank accounts" });
            }
        }

        [Authorize(Roles = "Customer")]
        public ActionResult BankAccountCreatedConfirmation()
        {
            return View();
        }

        [Authorize(Roles = "Customer")]
        public ActionResult StockPortfolioPendingMessage()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider { get; set; }
            public string RedirectUri { get; set; }
            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = RedirectUri };
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
        #endregion
    }
}