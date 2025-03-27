using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using ODHDEVELOPERS.Models;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Net;
using System.Data.Entity;

namespace ODHDEVELOPERS.Controllers
{
    public class CompanyController : Controller
    {
        UsersContext db = new UsersContext();
        SqlConnection con = new SqlConnection();
        Member mr = new Member();
        CompanyInfo cg = new CompanyInfo();
        public CompanyController() {
            mr = db.Members.Single(d => d.Id == 1);
            cg = db.CompanyInfos.Single(d => d.Id == 1);
        }
        string gid()
        {

            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {

                i *= ((int)b + 1);
            }


            if (i < 0)
            {
                i = -i;
            }
            string s = i.ToString();
            return s.Substring(0, 10);
        }
        public JsonResult AutoCompletePanel(string term)
        {
            var list = (from r in db.NewLogins where (r.type.ToLower().Contains(term.ToLower()) && (r.status == 1)) select new { r.type }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult CompanyReg()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult CompanyReg(CompanyInfo model, HttpPostedFileBase CompanyLogo)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {

                if (ModelState.IsValid)
                {

                    CompanyInfo ci = new CompanyInfo();
                    ci.CompanyName = model.CompanyName;
                    if (CompanyLogo != null)
                    {

                        CompanyLogo.SaveAs(HttpContext.Server.MapPath("~/Photo/CompanyLogo.png"));

                    }
                    ci.CompanyLogo = "~/Photo/CompanyLogo.png";
                    ci.Address = model.Address;
                    ci.AdminId = model.AdminId;
                    ci.Password = model.Password;
                    ci.RegDate = DateTime.Now;
                    ci.HeadOffice = model.HeadOffice;
                    ci.Emailid = model.Emailid; 
                    ci.Contact = model.Contact;
                    ci.RegistrationNo = model.RegistrationNo;
                    ci.DirectorName = model.DirectorName;
                    ci.Zipcode = model.Zipcode;
                    ci.State = model.State;
                    ci.Companysrtcut = model.Companysrtcut;
                    ci.IncomeType = model.IncomeType;
                    ci.Status = "1";


                    NewLogin nl = new NewLogin();
                    nl.UserName = model.AdminId;
                    nl.Password = model.Password;
                    nl.Mobile = model.Contact;
                    nl.type = "Admin";
                    nl.status = 1;

                    db.NewLogins.Add(nl);
                    db.CompanyInfos.Add(ci);
                    db.SaveChanges();
                    ViewBag.msg = "Company Created Successfully ";


                }

            }
            return View();
        }

        [HttpGet]
        public ActionResult UpdateCompany()
        {
            List<CompanyInfo> cilist = new List<CompanyInfo>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {

                return View(cilist);
            }
        }
        [HttpPost]
        public ActionResult UpdateCompany(string command, string AdminId, string cid, string CompanyName, string Companysrtcut, string RegistrationNo, string HeadOffice, string Address, string State, string Zipcode, string IncomeType, string DirectorName, string Contact, string Emailid, DateTime? RegDate, HttpPostedFileBase CompanyLogo)
        {
            List<CompanyInfo> cilist = new List<CompanyInfo>();

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                switch (command)
                {
                    case "Search":

                        cilist = db.CompanyInfos.Where(c => c.AdminId == AdminId).ToList();
                        break;

                    case "Update":

                        CompanyInfo ci = db.CompanyInfos.Single(a => a.AdminId == cid);
                        ci.CompanyName = CompanyName;
                        ci.Companysrtcut = Companysrtcut;
                        ci.RegistrationNo = RegistrationNo;
                        ci.HeadOffice = HeadOffice;
                        ci.Address = Address;
                        ci.State = State;
                        ci.Zipcode = Zipcode;
                        ci.IncomeType = IncomeType;
                        ci.DirectorName = DirectorName;
                        ci.Contact = Contact;
                        ci.Emailid = Emailid;
                        ci.RegDate = Convert.ToDateTime(RegDate);
                        if (CompanyLogo != null)
                        {

                            ci.CompanyLogo = "~/Photo/CompanyLogo.png";
                            CompanyLogo.SaveAs(HttpContext.Server.MapPath("~/Photo/") + "CompanyLogo.png");

                        }
                        else
                        {

                            ci.CompanyLogo = "~/Photo/CompanyLogo.png";

                        }
                        //db.Entry(ci).State = EntityState.Modified;
                        db.Entry<CompanyInfo>(ci).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        ViewBag.msg = "Company Updated Successfully....";
                        cilist = db.CompanyInfos.Where(c => c.AdminId == cid).ToList();
                        break;
                }
            }
            return View(cilist);
        }

        [HttpGet]
        public ActionResult UpdateMember()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
               
                return View();
            }
        }
        [HttpPost]
        public ActionResult UpdateMember(string branchname, string planname, string agentname, string custname, string rankname)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                if (ModelState.IsValid)
                {
                    Member ob = db.Members.Single(a => a.Id == 1);
                    ob.branchname = branchname;
                    ob.planname = planname;
                    ob.agentname = agentname;
                    ob.custname = custname;
                    ob.rankname = rankname;
                    //db.Entry(ob).State = EntityState.Modified;
                    db.Entry<Member>(ob).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    Response.Write("<script>alert('Name updated successfully ')</script>");
                }
                return View();
            }
        }

        [HttpGet]
        public ActionResult UpdateVoucherName()
        {
            List<VFormatTab> op = new List<VFormatTab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                op = db.VFormatTabs.ToList();
                return View(op);
            }
        }
        public JsonResult EditVoucher(VFormatTab ob)
        {
            List<VFormatTab> oplist = new List<VFormatTab>();
            VFormatTab opn = db.VFormatTabs.Single(c => c.Id == ob.Id);
            opn.type = ob.type;
            //db.Entry(opn).State = EntityState.Modified;
            db.Entry<VFormatTab>(opn).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            oplist = db.VFormatTabs.ToList();
            return Json(oplist, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult MacAddress(string userid, int status = 0)
        {
          
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                List<MacTab> maclist = new List<MacTab>();
                if (userid == null && status == 2)
                {
                    SetMacTab ob = db.SetMacTabs.Single(s => s.Id == 1);
                    ob.status = 0;
                    db.SaveChanges();
                }
                if (userid == null && status == 3)
                {
                    SetMacTab ob = db.SetMacTabs.Single(s => s.Id == 1);
                    ob.status = 1;
                    db.SaveChanges();

                }
                if (userid == null && status == 0)
                {
                    maclist = (from pl in db.MacTabs select pl).ToList();
                   

                }

                if (userid != null && status == 1)
                {
                    
                    MacTab ob = db.MacTabs.Single(p => p.userid == userid);
                    ob.status = 0;
                    db.SaveChanges();
                    
                }
                else if (userid != null && status == 0)
                {
                  
                    MacTab ob = db.MacTabs.Single(p => p.userid == userid);
                    ob.status = 1;
                    db.SaveChanges();
                   
                }
                maclist = (from pl in db.MacTabs select pl).ToList();
                return View(maclist);
            }
        }

        [HttpGet]
        public ActionResult LoginDetail(string UserName, int status = 0)
        {
            List<NewLogin> nllist = new List<NewLogin>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                if (UserName != null)
                {
                    var ur = db.NewLogins.Single(u => u.UserName == UserName);
                    MyClass.Sendmsg(ur.Mobile, " Your Userid: " + ur.UserName + " and Password:" + ur.Password + " Please Visit "+cg.HeadOffice);
                    Response.Write("<script>alert('Login-Detail sent successfully ')</script>");
                    nllist = (from nl in db.NewLogins where nl.type == ur.type select nl).ToList();
                }
                return View(nllist);
            }
        }
        [HttpPost]
        public ActionResult LoginDetail(string type)
        {
            List<NewLogin> nllist = new List<NewLogin>();
            ViewData["type"] = type;
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                nllist = (from nl in db.NewLogins where nl.type == type select nl).ToList();
                return View(nllist);
            }
        }

        [HttpGet]
        public ActionResult Approvewebsite(int Id = 0, int status = 0)
        {
            
            if (!IsLoggedIn())
            {
                return RedirectToAction("Logout", "Company");
            }
            else
            {
                if (Id > 0)
                {
                    var list = db.PC_Tabs.Single(o => o.Id == Id);
                    if (status == 2)
                    {
                        db.PC_Tabs.Remove(list);
                        db.SaveChanges();
                    }
                    else
                    {
                        list.status = status;
                        db.SaveChanges();
                    }
                }
                return View(db.PC_Tabs);
            }
        }

        [HttpGet]
        public ActionResult ChangePassword()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult ChangePassword(string Password, string NewPassword, string ConfirmPassword)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                if (NewPassword == ConfirmPassword)
                {

                    con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "changepassword";
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@user", User.Identity.Name);
                    cmd.Parameters.AddWithValue("@oldpass", Password);
                    cmd.Parameters.AddWithValue("@newpass", NewPassword);

                    SqlParameter p = new SqlParameter("@ans", SqlDbType.Int);
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);
                    try
                    {

                        con.Open();
                        cmd.ExecuteNonQuery();


                        string response1 = cmd.Parameters["@ans"].Value.ToString();
                        int a = Convert.ToInt32(response1);

                        if (a == 0)
                        {
                            ViewBag.msg = "Sorry,Old Password Not matching";

                        }
                        else if (a == 1)
                        {
                            ViewBag.msg = "Password Changed successfully";

                        }
                    }

                    catch (SqlException e)
                    {
                        ViewBag.msg = e.Message;

                    }
                    finally
                    {
                        con.Close();
                    }
                }
                else
                {
                    ViewBag.msg = "Password not matching";
                }
            }
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Home");
        }
        public bool IsLoggedIn()
        {

            try
            {
                if (User.Identity.IsAuthenticated)
                {
                    int count = (from n in db.NewLogins where n.UserName == User.Identity.Name select n.UserName).Count();
                    if (count == 1)
                    {
                        var log = db.NewLogins.Single(a => a.UserName == User.Identity.Name);
                        if (log.status == 1 && log.type == "Company")
                        {
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
            return false;
        }
    }
}
