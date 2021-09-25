using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MVC_F83345.Models;

namespace MVC_F83345.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        public ActionResult Registration()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Registration([Bind(Exclude = "EmailVerification, ActivationCode")]User user)
        {
            bool Status = false;
            string message = "";

            if (ModelState.IsValid)
            {
                var Valid = ValidEmail(user.EmailID);
                if (Valid)
                {
                    ModelState.AddModelError("ValidEmail", "This email already exists!");
                    return View(user);
                }

                user.ActivationCode = Guid.NewGuid();

                user.Password = PasswordHash.PHash(user.Password);
                user.ConfirmPassword = PasswordHash.PHash(user.ConfirmPassword);

                user.EmailVerification = false;

                using (DatabaseEntities dc = new DatabaseEntities())
                {
                    dc.Users.Add(user);
                    dc.SaveChanges();

                    VerLink(user.EmailID, user.ActivationCode.ToString());
                    message = "You have successfully registered!" +
                        "An account activation link has been sent to your email id:" + user.EmailID;
                    Status = true;
                }

            }
            else
            {
                message = "Invalid Request!";
            }

            ViewBag.Message = message;
            ViewBag.Status = Status;
            return View(user);
        }

        [HttpGet]
        public ActionResult VerAccount(string id)
        {
            bool status = false;
            using(DatabaseEntities dc = new DatabaseEntities())
            {
                dc.Configuration.ValidateOnSaveEnabled = false;
                var v = dc.Users.Where(a => a.ActivationCode == new Guid(id)).FirstOrDefault();
                if (v != null)
                {
                    v.EmailVerification = true;
                    dc.SaveChanges();
                    status = true;
                }
                else
                {
                    ViewBag.Message = "Request is invalid!";
                }
            }
            ViewBag.Status = status;
            return View();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserLogin log, string ReturnUrl = "")
        {
            string message = "";
            using (DatabaseEntities dc = new DatabaseEntities())
            {
                var v = dc.Users.Where(a => a.EmailID == log.EmailID).FirstOrDefault();
                if (v != null)
                {
                    if (string.Compare(PasswordHash.PHash(log.Password), v.Password) == 0)
                    {
                        int timeout = log.Remember ? 525600 : 18;
                        var ticket = new FormsAuthenticationTicket(log.EmailID, log.Remember, timeout);
                        string encr = FormsAuthentication.Encrypt(ticket);
                        var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encr);
                        cookie.Expires = DateTime.Now.AddMinutes(timeout);
                        cookie.HttpOnly = true;
                        Response.Cookies.Add(cookie);

                        if (Url.IsLocalUrl(ReturnUrl))
                        {
                            return Redirect(ReturnUrl);
                        }
                        else
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        message = "Invalid information is provided!Please check for spelling mistakes.";
                    }
                }
                else
                {
                    message = "Invalid information is provided!Please check for spelling mistakes.";
                }
            }
            ViewBag.Message = message;
            return View();
        }

        [Authorize]
        [HttpGet]
        public ActionResult ViewMyProfile()
        {
            using (DatabaseEntities de = new DatabaseEntities())
            {
                var user = de.Users.Where(a => a.EmailID == HttpContext.User.Identity.Name).FirstOrDefault();
                if (user != null)
                {
                    ViewBag.ViewProfileUser = user;
                    return View(user);
                }
            }
            ViewBag.ViewProfileUser = null;
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "User");
        }

        [NonAction]
        public bool ValidEmail(string EmailID)
        {
            using(DatabaseEntities dc = new DatabaseEntities())
            {
                var v = dc.Users.Where(a => a.EmailID == EmailID).FirstOrDefault();
                return v != null;
            }
        }

        [NonAction]
        public void VerLink(string EmailID, string ActCode)
        {
            //var scheme = Request.Url.Scheme;
            //var host = Request.Url.Host;
            //var port = Request.Url.Port;

            //string url = scheme + "://" + host + 

            var Verify = "/User/VerifyAccount/" + ActCode;
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, Verify);

            var From = new MailAddress("picuploader@gmail.com", "Picture Uploader");
            var To = new MailAddress(EmailID);

            var Frompass = "********";

            string sub = "Your account has been successfully created :D";

            string body = "<br/><br/>Congradulations for making an account in my Picture uploader site!" +
                "Please verify it by clicking the link below:<br/></br>" +
                " <a href = '"+link+"'>"+link+"</a> ";

            var smtp = new SmtpClient
            {
                Host = "stmp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(From.Address, Frompass)
            };

            using (var message = new MailMessage(From, To)
            {
                Subject = sub,
                Body = body,
                IsBodyHtml = true
            })
            smtp.Send(message);
        }
    }
}