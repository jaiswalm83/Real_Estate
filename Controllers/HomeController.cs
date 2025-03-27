using ODHDEVELOPERS.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Mvc;
using System.Web.Security;
using System.Net.NetworkInformation;
using System.Web.SessionState;
using System.Text;
using System.Data.Entity;
using System.Net.Http;
using System.IO;
using RestSharp;

namespace ODHDEVELOPERS.Controllers
{
    public class HomeController : Controller
    {
        SqlConnection con = new SqlConnection();
        UsersContext db = new UsersContext();

        public String hashValue = "";
        string salt = "";
        protected MerchantAPI.ChecksumCalculator objCalc = new MerchantAPI.ChecksumCalculator();

        #region #Mz API-CALL
        public string Decriptdata(string encript)
        {
            string decript = string.Empty;
            UTF8Encoding encode = new UTF8Encoding();
            Decoder Decode = encode.GetDecoder();
            byte[] Todecoder_byte = Convert.FromBase64String(encript);
            int charcount = Decode.GetCharCount(Todecoder_byte, 0, Todecoder_byte.Length);
            char[] decode_char = new char[charcount];
            Decode.GetChars(Todecoder_byte, 0, Todecoder_byte.Length, decode_char, 0);
            decript = new string(decode_char);
            return decript;
        }

        #endregion

        public void addpc(string uid, string pcid)
        {
            var username = string.Empty;
            var nlr = db.NewLogins.Single(n => n.UserName == uid);
            if (nlr.type == "Admin")
            {
                username = "Admin";
                PC_Tab pc = new PC_Tab();
                pc.PCName = username;
                pc.userid = uid;
                pc.PCId = pcid;
                pc.addate = DateTime.Now;
                db.PC_Tabs.Add(pc);
                db.SaveChanges();
            }
            else if (nlr.type == "Branch")
            {
                username = db.Branchtabs.Single(b => b.BranchCode == uid).BranchName;
                PC_Tab pc = new PC_Tab();
                pc.PCName = username;
                pc.userid = uid;
                pc.PCId = pcid;
                pc.addate = DateTime.Now;
                db.PC_Tabs.Add(pc);
                db.SaveChanges();
            }
            else if (nlr.type == "Operator")
            {
                username = db.Operators.Single(b => b.OperatorId == uid).OperatorName;
                PC_Tab pc = new PC_Tab();
                pc.PCName = username;
                pc.userid = uid;
                pc.PCId = pcid;
                pc.addate = DateTime.Now;
                db.PC_Tabs.Add(pc);
                db.SaveChanges();
            }
            else if (nlr.type == "Receptionist")
            {
                username = db.DailyVisitors.Single(b => b.UserName == uid).ReceptionistName;
                PC_Tab pc = new PC_Tab();
                pc.PCName = username;
                pc.userid = uid;
                pc.PCId = pcid;
                pc.addate = DateTime.Now;
                db.PC_Tabs.Add(pc);
                db.SaveChanges();
            }
        }

        public int ck(string uid)
        {
            var a = 0;
            string host = Request.Url.Host.Replace(".", ""), PCC = host + "PCC", PCCVAL = host + "PCCVAL";
            System.Web.HttpCookie pcookie = new System.Web.HttpCookie(PCC);
            if (!string.IsNullOrEmpty(Convert.ToString(Request.Cookies[PCC])))
            {
                System.Web.HttpCookie pccookie = HttpContext.Request.Cookies.Get(PCC);
                string pcid = Convert.ToString(pccookie.Values[PCCVAL]);
                var count = db.PC_Tabs.Where(c => c.userid == uid && c.PCId == pcid).Count();
                if (count > 0)
                {
                    var i = db.PC_Tabs.Single(p => p.userid == uid && p.PCId == pcid);
                    if (i.status == 1)
                    {
                        a = 1;
                    }
                    else if (i.status == 0)
                    {
                        a = 2;
                    }
                }
                else
                {
                    addpc(uid, pcid);
                }
            }
            else
            {

                System.Web.HttpCookie pcookie1 = new System.Web.HttpCookie(PCC);
                var pcid = Guid.NewGuid().ToString();
                pcookie1.Values.Add(PCCVAL, pcid);
                pcookie1.Expires = DateTime.Now.AddDays(365);
                Response.SetCookie(pcookie1);
                Response.Cookies.Add(pcookie1);

                addpc(uid, pcid);
            }
            return a;
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
            return s.Substring(0, 6);


        }

        public JsonResult AutoCompleteAgentid(string term)
        {

            var list = (from r in db.AgentDetails where r.NewAgentId.ToLower().Contains(term.ToLower()) || r.name.ToLower().Contains(term.ToLower()) select new { r.NewAgentId, r.name }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult selectblock(int blist = 0)
        {
            var blistss = "";
            if (blist > 0)
            {
                var blists = db.BlockTabs.Where(a => a.Plancode == blist).ToList();
                return Json(blists, JsonRequestBehavior.AllowGet);
            }
            return Json(blistss, JsonRequestBehavior.AllowGet);
        }

        public JsonResult soldplot(int projectid = 0, int phaseid=0, int blockid = 0)
        {
            List<PlotList2> pt = new List<PlotList2>();

            //string str = "";

            var bb = db.BlockTabs.Single(a => a.Plancode == projectid && a.phaseid == phaseid && a.Id == blockid);
            var cc = db.Plans.Single(a => a.Plancode == projectid);


            var list = (from o in db.appltabs where o.projectid == projectid  && o.phaseid==phaseid && o.block == bb.block && o.plotno > 0 select new { o.plotno }).ToList();

            for (int i = bb.min; i <= bb.max; i++)
            {
                var hot = (from o in db.HoldingPlots where o.holdprojectid == projectid && o.holdphaseid == phaseid && o.holdblock == bb.Id && o.holdplotno == i select o).Count();

                if (hot == 0)
                {
                    var count = db.appltabs.Where(p => p.block == bb.block && p.projectid == projectid && p.phaseid == phaseid && p.plotno == i && p.status == 1).Count();

                    if (count == 0)
                    {
                        var pcount = db.appltabs.Where(p => p.block == bb.block && p.projectid == projectid && p.phaseid == phaseid && p.plotno == i && p.status == 1).Count();
                        if (pcount > 0)                        
                        {
                            pt.Add(new PlotList2 { id = i, availablity = "Available", status = 0, projectid = cc.Plancode, blockid = bb.Id, projectname = cc.Planname, blockname = bb.block });
                        }
                        else
                        {
                            var countplot = (from o in db.appltabs where o.projectid == projectid && o.phaseid == phaseid  && o.block == bb.block && o.plotno == i select o).Count();

                            var tcount = db.tempappltabs.Where(p => p.block == bb.block && p.projectid == projectid && p.phaseid == phaseid && p.plotno == i && p.status == 1).Count();
                            if (tcount == 0 && countplot == 0)
                            {
                                pt.Add(new PlotList2 { id = i, availablity = "Available", status = 0, projectid = cc.Plancode, blockid = bb.Id, projectname = cc.Planname, blockname = bb.block });
                            }
                            else if (tcount != 0 && countplot == 0)
                            {
                                pt.Add(new PlotList2 { id = i, availablity = "Unavailable", status = 1, projectid = cc.Plancode, blockid = bb.Id, projectname = cc.Planname, blockname = bb.block });
                            }
                            else if (tcount == 0 && countplot != 0)
                            {
                                pt.Add(new PlotList2 { id = i, availablity = "Unavailable", status = 1, projectid = cc.Plancode, blockid = bb.Id, projectname = cc.Planname, blockname = bb.block });
                            }
                            else
                            {
                                pt.Add(new PlotList2 { id = i, availablity = "Available", status = 0, projectid = cc.Plancode, blockid = bb.Id, projectname = Convert.ToString(cc.Planname), blockname = Convert.ToString(bb.block), plotno = i });
                            }
                        }
                    }
                    else
                    {

                        pt.Add(new PlotList2 { id = i, availablity = "Registered", status = 4, projectid = cc.Plancode, blockid = bb.Id, projectname = Convert.ToString(cc.Planname), blockname = Convert.ToString(bb.block), plotno = i });
                    }
                }
                else
                {
                    var hot2 = (from o in db.HoldingPlots where o.holdprojectid == projectid && o.holdphaseid == phaseid && o.holdblock == bb.Id && o.holdplotno == i select o).Count();
                    if (hot2 > 0)
                    {
                        pt.Add(new PlotList2 { id = i, availablity = "Unavailable", status = 1, projectid = cc.Plancode, blockid = bb.Id, projectname = Convert.ToString(cc.Planname), blockname = Convert.ToString(bb.block), plotno = i });
                    }
                    else
                    {
                        pt.Add(new PlotList2 { id = i, availablity = "Hold", status = 3, projectid = cc.Plancode, blockid = bb.Id, projectname = Convert.ToString(cc.Planname), blockname = Convert.ToString(bb.block), plotno = i });

                    }
                }
            }


            return Json(pt, JsonRequestBehavior.AllowGet);
        }

        public JsonResult Plotholding(string newagent, int blockid = 0, int sid = 0, int plotid = 0)
        {
            string msg = string.Empty;
            HoldingPlot hold = new HoldingPlot();

            var agentcount = db.AgentDetails.Where(k => k.NewIntroducerId == newagent).Count();

            if (agentcount != 0)
            {

                var count = db.HoldingPlots.Where(b => b.holdstatus == 1).Count();

                int maxid = 0;



                if (count == 0)
                {

                    hold.holdblock = blockid;
                    hold.holdprojectid = plotid;
                    hold.holdstatus = 1;
                    hold.holdby = newagent;
                    hold.holdplotno = sid;
                    hold.holddate = DateTime.Now;

                    db.HoldingPlots.Add(hold);
                    db.SaveChanges();
                    msg = "Successfull !";
                }
                else
                {
                    maxid = db.HoldingPlots.Where(b => b.holdstatus == 1).Max(d => d.Id);
                    hold.holdblock = blockid;
                    hold.holdprojectid = plotid;
                    hold.holdstatus = 1;
                    hold.holdby = newagent;
                    hold.holdplotno = sid;
                    hold.holddate = DateTime.Now;

                    db.HoldingPlots.Add(hold);
                    db.SaveChanges();
                    msg = "Successfull !";
                }
            }
            else
            {
                msg = "Introducer Not Available";
            }
            return Json(msg, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.Message = "Modify this template to jump-start your ASP.NET MVC application.";

            return View();
        }
        //second time
       
        [HttpGet]
        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        [HttpGet]
        public ActionResult Legal()
        {


            return View();
        }

        [HttpGet]
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
        [HttpPost]
        public ActionResult Contact(Contact ob)
        {
            var cr = db.CompanyInfos.Single(ci => ci.Id == 1);
            if (ModelState.IsValid)
            {
                db.Contacts.Add(ob);
                db.SaveChanges();
                //MyClass.Sendmsg(cr.Contact, "Mr. " + ob.name + " wants to contact on Subject: " + ob.subject + " call him on contact no " + ob.mobile);
                Response.Write("<script>alert('Thanks for contact us,we feedback you soon ')</script>");
            }

            return View();
        }
        [HttpGet]
        public ActionResult Inquery()
        {
            ViewBag.Message = "Your contact page.";
            return View();
        }
        [HttpPost]
        public ActionResult Inquery(Contact ob)
        {
            var cr = db.CompanyInfos.Single(ci => ci.Id == 1);
            Contact bc = new Contact();
            if (ModelState.IsValid)
            {
                bc.emailid = ob.emailid;
                bc.message = ob.message;
                bc.name = ob.name;
                bc.mobile = ob.mobile;
                bc.cdate = DateTime.Now;
                db.Contacts.Add(bc);
                db.SaveChanges();
                MyClass.Sendmsg(cr.Contact, "Mr. " + ob.name + " wants to contact on Subject: " + ob.subject + " call him on contact no " + ob.mobile);
                Response.Write("<script>alert('Thanks for query,we feedback you soon ')</script>");
            }

            return View();
           
        }
        //Second time
      

        [HttpGet]
        public ActionResult Login()
        {
            //if (User.Identity.IsAuthenticated)
            //{
                int count = (from n in db.NewLogins where n.UserName == User.Identity.Name select n.UserName).Count();
                if (count != 0)
                {
                    var log = db.NewLogins.Single(a => a.UserName == User.Identity.Name);
                    if (log.status == 1)
                    {
                        if (log.type == "Company")
                        {
                            FormsAuthentication.SetAuthCookie(log.UserName, true);
                            return RedirectToAction("Index", "Company");
                        }
                        else if (log.type == "Admin")
                        { 
                            FormsAuthentication.SetAuthCookie(log.UserName, true);
                            return RedirectToAction("Index", "Admin");
                        }

                        else if (log.type == "Subadmin")
                        {
                            FormsAuthentication.SetAuthCookie(log.UserName, true);
                            return RedirectToAction("Index", "Admin");
                        }

                        else if (log.type == "Branch")
                        {
                            FormsAuthentication.SetAuthCookie(log.UserName, true);
                            return RedirectToAction("Index", "Branch");
                        }
                        else if (log.type == "Operator")
                        {
                            FormsAuthentication.SetAuthCookie(log.UserName, true);
                            return RedirectToAction("Index", "Operator");
                        }
                        else if (log.type == "HR")
                        {
                            FormsAuthentication.SetAuthCookie(log.UserName, true);
                            return RedirectToAction("Index", "HR");
                        }
                        else if (log.type == "Agent")
                        {
                            FormsAuthentication.SetAuthCookie(log.UserName, true);
                            return RedirectToAction("Index", "Agent");
                        }
                        else if (log.type == "Customer")
                        {
                            FormsAuthentication.SetAuthCookie(log.UserName, true);
                            return RedirectToAction("Index", "Customer");
                        }
                    }
                    else
                    {
                        ViewBag.msg = "You Blocked By Admin Please Contact ";
                    }
                }
                else
                {
                    ViewBag.msg = "Wrong User Id and Password";
                }

            //}
            //else
            //{
                //con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                //SqlCommand cmd = new SqlCommand();
                //cmd.CommandText = "CancelPlot";
                //cmd.CommandType = CommandType.StoredProcedure;
                //cmd.Connection = con;
                //try
                //{
                //    con.Open();
                //    cmd.ExecuteNonQuery();
                //}

                //catch (Exception e)
                //{
                //    ViewBag.msg = e.Message;
                //}
                //finally
                //{
                //    con.Close();
                //}
                //var count1 = db.HoldingPlots.Where(ww => ww.holdstatus == 1).Count();

                //if (count1 != 0)
                //{
                //    var clear = db.HoldingPlots.FirstOrDefault(ww => ww.holdstatus == 1);

                //    DateTime checkdate = Convert.ToDateTime(clear.holddate);
                //    DateTime currentdate = DateTime.Now;

                //    if (checkdate.Date < currentdate.Date)
                //    {

                //        var holdlist = db.HoldingPlots.Where(a => a.holddate < DateTime.Now && a.holdby != "KDIPL").ToList();

                //        foreach (var hh in holdlist)
                //        {


                //            var endtime = Convert.ToDateTime(hh.holddate);
                //            TimeSpan span = DateTime.Now - endtime;
                //            var hours = span.Days * 24 + span.Hours;

                //            if (hours > 24)
                //            {
                //                var removehod = db.HoldingPlots.Single(a => a.Id == hh.Id);
                //                db.HoldingPlots.Remove(removehod);
                //                db.SaveChanges();
                //            }

                //        }


                //    }
                //}

            //}

            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        public ActionResult Login(NewLogin model)
        {
            var nlr = db.NewLogins.Single(n => n.UserName == model.UserName);
            if (ck(model.UserName) == 1 || nlr.type == "Customer" || nlr.type == "Agent" || nlr.type == "Company" || nlr.type == "Operator" || nlr.type == "HR" || nlr.type=="Subadmin")
            {
                if (ModelState.IsValid)
                {
                    string localIP = "";
                    foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                    {
                        if (nic.OperationalStatus == OperationalStatus.Up)
                        {
                            localIP += nic.GetPhysicalAddress().ToString();
                            break;
                        }
                    }
                    con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                    SqlCommand Cmd = new SqlCommand();
                    Cmd.CommandText = "login";
                    Cmd.CommandType = CommandType.StoredProcedure;
                    Cmd.Connection = con;
                    Cmd.Parameters.AddWithValue("@userid", model.UserName);
                    Cmd.Parameters.AddWithValue("@pwd", model.Password);
                    Cmd.Parameters.AddWithValue("@macaddress", localIP);

                    SqlParameter p1 = new SqlParameter("@mobile", SqlDbType.NVarChar, 50);
                    p1.Direction = ParameterDirection.Output;
                    Cmd.Parameters.Add(p1);

                    SqlParameter p = new SqlParameter("@Response", SqlDbType.Int);
                    p.Direction = ParameterDirection.Output;
                    Cmd.Parameters.Add(p);

                    try
                    {
                        con.Open();
                        Cmd.ExecuteNonQuery();
                        con.Close();


                        string response1 = Cmd.Parameters["@Response"].Value.ToString();
                        string mobile = Cmd.Parameters["@mobile"].Value.ToString();
                        int response = (Cmd.Parameters["@Response"].Value != DBNull.Value) ? Convert.ToInt32(Cmd.Parameters["@Response"].Value) : 0;

                        var dpcount = (from d in db.DailyDepositTabs where d.status == 0 && d.opid == model.UserName select d).Count();

                        if (nlr.type == "Subadmin") // Sub-Admin
                        {
                            FormsAuthentication.SetAuthCookie(model.UserName, true);
                            return RedirectToAction("Index", "Admin"); // Admin Panel Hi Open Hoga
                        }

                        if (dpcount == 0)
                        {
                            if (response == 1)
                            {
                                FormsAuthentication.SetAuthCookie(model.UserName, true);
                                return RedirectToAction("Index", "Company");
                            }

                            else if (response == 2)
                            {

                                FormsAuthentication.SetAuthCookie(model.UserName, true);
                                return RedirectToAction("Index", "Admin");
                            }

                            else if (response == 3)
                            {
                                var ed = db.SetMacTabs.Single(e => e.Id == 1);
                                if (ed.status == 1)
                                {
                                    var count = (from mt in db.MacTabs where mt.type == "Branch" && mt.macaddress == localIP select mt).Count();
                                    var ccount = (from mt in db.MacTabs where mt.type == "Branch" && mt.userid == model.UserName && mt.macaddress == localIP select mt).Count();
                                    MacTab edi = new MacTab();
                                    if (ccount != 0)
                                    {
                                        edi = db.MacTabs.Single(e => e.userid == model.UserName);
                                    }
                                    else
                                    {
                                        edi.status = 0;
                                    }

                                    if (edi.status == 1 || ccount == 0)
                                    {
                                        var br = db.Members.Single(b => b.Id == 1);

                                        if (count == 0)
                                        {
                                            MacTab mtob = new MacTab();
                                            mtob.userid = model.UserName;
                                            mtob.macaddress = localIP;
                                            mtob.type = "Branch";
                                            mtob.status = 1;
                                            db.MacTabs.Add(mtob);
                                            db.SaveChanges();

                                            FormsAuthentication.SetAuthCookie(model.UserName, true);
                                            return RedirectToAction("Index", "Branch");
                                        }
                                        else if (count == 1)
                                        {
                                            var bcount = (from mt in db.MacTabs where mt.type == "Branch" && mt.userid == model.UserName && mt.macaddress == localIP select mt).Count();
                                            if (bcount == 1)
                                            {
                                                FormsAuthentication.SetAuthCookie(model.UserName, true);
                                                return RedirectToAction("Index", "Branch");
                                            }
                                            else if (bcount == 0)
                                            {
                                                Response.Write("<script>alert('Sorry,This system already assigned to another " + br.branchname + "')</script>");
                                            }
                                        }
                                    }
                                    else if (edi.status == 0)
                                    {
                                        FormsAuthentication.SetAuthCookie(model.UserName, true);
                                        return RedirectToAction("Index", "Branch");
                                    }
                                }
                                else if (ed.status == 0)
                                {
                                    FormsAuthentication.SetAuthCookie(model.UserName, true);
                                    return RedirectToAction("Index", "Branch");
                                }

                            }

                            else if (response == 4)
                            {
                                var ed = db.SetMacTabs.Single(e => e.Id == 1);
                                if (ed.status == 1)
                                {
                                    var count = (from mt in db.MacTabs where mt.type == "Operator" && mt.macaddress == localIP select mt).Count();
                                    var ccount = (from mt in db.MacTabs where mt.type == "Operator" && mt.userid == model.UserName && mt.macaddress == localIP select mt).Count();
                                    MacTab edi = new MacTab();
                                    if (ccount != 0)
                                    {
                                        edi = db.MacTabs.Single(e => e.userid == model.UserName);
                                    }
                                    else
                                    {
                                        edi.status = 0;
                                    }
                                    if (edi.status == 1 || ccount == 0)
                                    {

                                        if (count == 0)
                                        {
                                            MacTab mtob = new MacTab();
                                            mtob.userid = model.UserName;
                                            mtob.macaddress = localIP;
                                            mtob.type = "Operator";
                                            mtob.status = 1;
                                            db.MacTabs.Add(mtob);
                                            db.SaveChanges();

                                            FormsAuthentication.SetAuthCookie(model.UserName, true);
                                            return RedirectToAction("Index", "Operator");
                                        }
                                        else if (count == 1)
                                        {
                                            var bcount = (from mt in db.MacTabs where mt.type == "Operator" && mt.userid == model.UserName && mt.macaddress == localIP select mt).Count();
                                            if (bcount == 1)
                                            {
                                                FormsAuthentication.SetAuthCookie(model.UserName, true);
                                                return RedirectToAction("Index", "Operator");
                                            }
                                            else if (bcount == 0)
                                            {
                                                Response.Write("<script>alert('Sorry,This system already assigned to another Operator')</script>");
                                            }
                                        }
                                    }
                                    else if (edi.status == 0)
                                    {
                                        FormsAuthentication.SetAuthCookie(model.UserName, true);
                                        return RedirectToAction("Index", "Operator");
                                    }
                                }
                                else if (ed.status == 0)
                                {
                                    FormsAuthentication.SetAuthCookie(model.UserName, true);
                                    return RedirectToAction("Index", "Operator");
                                }
                            }
                        

                            else if (response == 10)
                            {
                                var ed = db.SetMacTabs.Single(e => e.Id == 1);
                                if (ed.status == 1)
                                {
                                    var count = (from mt in db.MacTabs where mt.type == "HR" && mt.macaddress == localIP select mt).Count();
                                    var ccount = (from mt in db.MacTabs where mt.type == "HR" && mt.userid == model.UserName && mt.macaddress == localIP select mt).Count();
                                    MacTab edi = new MacTab();
                                    if (ccount != 0)
                                    {
                                        edi = db.MacTabs.Single(e => e.userid == model.UserName);
                                    }
                                    else
                                    {
                                        edi.status = 0;
                                    }
                                    if (edi.status == 1 || ccount == 0)
                                    {

                                        if (count == 0)
                                        {
                                            MacTab mtob = new MacTab();
                                            mtob.userid = model.UserName;
                                            mtob.macaddress = localIP;
                                            mtob.type = "HR";
                                            mtob.status = 1;
                                            db.MacTabs.Add(mtob);
                                            db.SaveChanges();

                                            FormsAuthentication.SetAuthCookie(model.UserName, true);
                                            return RedirectToAction("Index", "HR");
                                        }
                                        else if (count == 1)
                                        {
                                            var bcount = (from mt in db.MacTabs where mt.type == "Operator" && mt.userid == model.UserName && mt.macaddress == localIP select mt).Count();
                                            if (bcount == 1)
                                            {
                                                FormsAuthentication.SetAuthCookie(model.UserName, true);
                                                return RedirectToAction("Index", "HR");
                                            }
                                            else if (bcount == 0)
                                            {
                                                Response.Write("<script>alert('Sorry,This system already assigned to another HR')</script>");
                                            }
                                        }
                                    }
                                    else if (edi.status == 0)
                                    {
                                        FormsAuthentication.SetAuthCookie(model.UserName, true);
                                        return RedirectToAction("Index", "HR");
                                    }
                                }
                                else if (ed.status == 0)
                                {
                                    FormsAuthentication.SetAuthCookie(model.UserName, true);
                                    return RedirectToAction("Index", "HR");
                                }
                            }

                            else if (response == 5)
                            {
                                FormsAuthentication.SetAuthCookie(model.UserName, true);
                                return RedirectToAction("Index", "Agent");
                            }

                            else if (response == 6)
                            {

                                FormsAuthentication.SetAuthCookie(model.UserName, true);
                                return RedirectToAction("Index", "Customer");
                            }
                            else if (response == 15)
                            {
                                if (1 == db.NewLogins.Where(o => o.UserName == model.UserName && o.Password == model.Password && o.type == "Receptionist").Count())
                                {
                                    FormsAuthentication.SetAuthCookie(model.UserName, true);
                                    return RedirectToAction("DailyVisitorDetail", "Operator");
                                }
                            }
                            if (response == 7)
                            {
                                Response.Write("<script>alert('Sorry,You are blocked by Admin')</script>");
                            }

                            if (response == 8)
                            {
                                Response.Write("<script>alert('Sorry,Your Password wrong')</script>");
                            }
                            if (response == 9)
                            {
                                Response.Write("<script>alert('Sorry,Your User Name Wrong')</script>");
                            }
                            if (response == 11)
                            {
                                Response.Write("<script>alert('Sorry,Login Time Expired')</script>");
                            }
                            if (response == 12)
                            {
                                Response.Write("<script>alert('Sorry,You are not authorised for this System')</script>");
                            }
                        }
                        else
                        {
                            TempData["User"] = model.UserName;
                            if (nlr.type == "Operator")
                            {
                                return RedirectToAction("DailyClosing");
                            }
                            else if (nlr.type == "Branch")
                            {
                                return RedirectToAction("BranchDailyClosing");
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        ViewBag.msg = e.Message;
                        TempData["Error"] = e;
                        Response.Write("<script>alert('Error: " + e.Message + "')</script>");
                    }

               
                }
            }
            else { Response.Write("<script>alert('Sorry,You are not authorised for this System')</script>"); }
            return View();
        }
        //second time
    
        public JsonResult Jsonresult(string Project, string Recogniser)
        {
            if (Recogniser == "getPhase")
            {
                //var a = db.Plans.FirstOrDefault(o => o.Planname == Project);
                var id = Convert.ToInt16(Project);
                var list=db.PhaseTabs.Where(o => o.projectid == id).ToList();
                return Json(list, 0);
            }
            else if (Recogniser == "getBlock")
            {
                var id=Convert.ToInt16(Project);
                var a = db.BlockTabs.Where(o => o.phaseid ==id).ToList();
                return Json(a, 0);
            }
            //else if (Recogniser == "getPlot")
            //{ 
              
            
            //}

            return Json(0, 0);
        }

        [HttpGet]
        public ActionResult Project() 
        {


            //var count = db.HoldingPlots.Where(ww => ww.holdstatus == 1).Count();

            //if (count != 0)
            //{
            //    var clear = db.HoldingPlots.FirstOrDefault(ww => ww.holdstatus == 1);

            //    DateTime checkdate = Convert.ToDateTime(clear.holddate);
            //    DateTime currentdate = DateTime.Now;

            //    if (checkdate.Date < currentdate.Date)
            //    {

            //        var holdlist = db.HoldingPlots.Where(a => a.holddate < DateTime.Now).ToList();

            //        foreach (var hh in holdlist)
            //        {

            //            var starttime = Convert.ToDateTime(DateTime.Now);
            //            var endtime = Convert.ToDateTime(hh.holddate);


            //            var totaltime = Convert.ToDateTime(starttime).Subtract(Convert.ToDateTime(endtime));
            //            var tt = totaltime.Hours;

            //            if (tt > 24)
            //            {
            //                var removehod = db.HoldingPlots.Single(a => a.Id == hh.Id);
            //                db.HoldingPlots.Remove(removehod);
            //                db.SaveChanges();
            //            }

            //        }


            //    }
            //}
            return View();
        }
        //Second time
        [HttpGet]
        public ActionResult Project1()
        {


            //var count = db.HoldingPlots.Where(ww => ww.holdstatus == 1).Count();

            //if (count != 0)
            //{
            //    var clear = db.HoldingPlots.FirstOrDefault(ww => ww.holdstatus == 1);

            //    DateTime checkdate = Convert.ToDateTime(clear.holddate);
            //    DateTime currentdate = DateTime.Now;

            //    if (checkdate.Date < currentdate.Date)
            //    {

            //        var holdlist = db.HoldingPlots.Where(a => a.holddate < DateTime.Now).ToList();

            //        foreach (var hh in holdlist)
            //        {

            //            var starttime = Convert.ToDateTime(DateTime.Now);
            //            var endtime = Convert.ToDateTime(hh.holddate);


            //            var totaltime = Convert.ToDateTime(starttime).Subtract(Convert.ToDateTime(endtime));
            //            var tt = totaltime.Hours;

            //            if (tt > 24)
            //            {
            //                var removehod = db.HoldingPlots.Single(a => a.Id == hh.Id);
            //                db.HoldingPlots.Remove(removehod);
            //                db.SaveChanges();
            //            }

            //        }


            //    }
            //}
            return View();
        }

        [HttpGet]
        public ActionResult Gallery()
        {
            return View();
        }

        [HttpGet]
        public ActionResult News()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Termandcondition()
        {
            return View();
        }

        [HttpGet]
        public ActionResult DigitalServices()
        {
            return View();
        }    

        [HttpGet]
        public ActionResult privacypolicy()
        {
            return View();
        }

        [HttpGet]
        public ActionResult holdplot()
        {
            return View();
        }
        [HttpPost]
        public ActionResult holdplot(int projectid = 0, int blockid = 0, int id = 0)
        {
            appltab hl = new appltab();

            return RedirectToAction("Project", "Home");
        }

        public ActionResult Success(string id)
        {
            var decodeData = Decriptdata(id);
            var data = decodeData.Split('|');
            var CountPayee = db.PayU_Payments.Where(xn => xn.Transactionid == data[0] && xn.Clientid == data[1] && xn.status == 0).ToList().Count;
            if (CountPayee == 1)
            {
                PayU_Payment payee = db.PayU_Payments.Single(xn => xn.Transactionid == data[0] && xn.Clientid == data[1] && xn.status == 0);
                payee.status = 1;
                payee.Remark = "Payment added to wallet.";
                //db.Entry(payee).State = EntityState.Modified;
                db.Entry<PayU_Payment>(payee).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                var srno = (from aa in db.Wallet_Transactions where aa.agentid == id select aa.Sr_No).Max();
                var oldBalance = db.Wallet_Transactions.Single(bb => bb.Sr_No == srno).netamount;
                Wallet_Transaction wallet = new Wallet_Transaction();
                wallet.Credit = Convert.ToDouble(data[5]);
                wallet.Debit = 0;
                wallet.Type = "Credit";
                wallet.Remark = "Payment added sucessfuly to self-wallet via online payment on " + DateTime.Now.ToString("dd MMM yy HH:mm");
                wallet.Date_Time = DateTime.Now;
                wallet.Transaction_No = "wM" + gid().Substring(0, 4) + "m" + DateTime.Now.ToString("yyMMddHHmmssff") + "z" + gid().Substring(5, 8);
                wallet.status = 1;
                wallet.netamount = oldBalance + Convert.ToDouble(data[5]);
                wallet.agentid = id;
                wallet.Sender_Id = id;
                wallet.Sender_Name = data[2];
                wallet.Mode = "ONLINE";
                db.Wallet_Transactions.Add(wallet);
                db.SaveChanges();
            }

            return View();
        }

        public ActionResult Failure(string id)
        {
            var decodeData = Decriptdata(id);
            var data = decodeData.Split('|');

            PayU_Payment payee = db.PayU_Payments.Single(xn => xn.Transactionid == data[0] && xn.Clientid == data[1] && xn.status == 0);
            payee.status = 2;
            //db.Entry(payee).State = EntityState.Modified;
            db.Entry<PayU_Payment>(payee).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return View();
        }

        [HttpGet]
        public ActionResult DailyClosing()
        {
            ViewData["username"] = TempData["User"];
            //ViewData["username"] = "AANCHAL";
            return View();
        }
        [HttpPost]
        public ActionResult DailyClosing(DailyDepositTab ddt, string user, string date, string remark1, string remark2, Double transferamount1 = 0, Double transferamount2 = 0, Double Grandtotalcash = 0, Double bal = 0, int id = 0, int trid = 0)
        {
            ViewData["username"] = user;

            var msg = "";
            var Udetail = db.NewLogins.Single(a => a.UserName == user);
            DateTime sdate = DateTime.ParseExact(date, "dd/MM/yyyy", null);
            DailyDepositTab dt = db.DailyDepositTabs.Single(a => a.opid == user && a.Id == id && a.trid == trid && a.date == sdate);
            if (dt.transfertype == "Cash")
            {
                if (dt.twothousand != ddt.twothousand || dt.fivehundred != ddt.fivehundred || dt.twohundred != ddt.twohundred || dt.hundred != ddt.hundred || dt.fifty != ddt.fifty || dt.twenty != ddt.twenty || dt.ten != ddt.ten || dt.five != ddt.five || dt.two != ddt.two || dt.one != ddt.one)
                {
                    msg = "Fields does not Match with before fields";
                    Response.Write("<script>alert('Fields does not Match with before fields')</script>");
                }
                else
                {
                    dt.status = 1;
                    dt.matchingdate = DateTime.Now.Date;
                    db.SaveChanges();
                    msg = "Fields  Matched successfully with before fields";
                }
            }
            else if (dt.transfertype == "Bank")
            {
                if (dt.bankapp == 1)
                {
                    if (dt.twothousand != ddt.twothousand || dt.fivehundred != ddt.fivehundred || dt.twohundred != ddt.twohundred || dt.hundred != ddt.hundred || dt.fifty != ddt.fifty || dt.twenty != ddt.twenty || dt.ten != ddt.ten || dt.five != ddt.five || dt.two != ddt.two || dt.one != ddt.one)
                    {
                        msg = "Fields does not Match with before fields";
                        Response.Write("<script>alert('Fields does not Match with before fields')</script>");
                    }
                    else
                    {
                        dt.status = 1;
                        dt.matchingdate = DateTime.Now.Date;
                        db.SaveChanges();
                        msg = "Fields  Matched successfully with before fields";
                    }
                }
                else
                {

                    dt.status = 1;
                    dt.bankapp = 2;
                    dt.matchingdate = DateTime.Now.Date;
                    db.SaveChanges();

                    #region Insert
                    #region notselect
                    if (ddt.transfertype != "select")
                    {
                        Double remailning = 0;
                        Double chequebookpen = (from a in db.tempappltabs where a.paymethod == "Cheque" && a.opid == user && a.status == 1 && a.formdate == sdate select a.bookingamount).DefaultIfEmpty(0).Sum();
                        Double chequerenpen = (from a in db.TempInstallmenttabs where a.paymethod == "Cheque" && a.opid == user && a.status == 1 && a.paymentdate == sdate select a.payamount).DefaultIfEmpty(0).Sum();
                        if (ddt.transfertype == "Bank")
                        {
                            remailning = bal - transferamount1;
                        }
                        else if (ddt.transfertype == "Cash")
                        {
                            remailning = bal - transferamount2;
                        }
                        int trid1 = (from a in db.DailyDepositTabs where a.opid == user select a.trid).DefaultIfEmpty(0).Max() + 1;
                        DailyDepositTab dt1 = new DailyDepositTab();
                        dt1.date = sdate;
                        dt1.twothousand = ddt.twothousand;
                        dt1.fivehundred = ddt.fivehundred;
                        dt1.twohundred = ddt.twohundred;
                        dt1.hundred = ddt.hundred;
                        dt1.fifty = ddt.fifty;
                        dt1.twenty = ddt.twenty;
                        dt1.ten = ddt.ten;
                        dt1.five = ddt.five;
                        dt1.two = ddt.two;
                        dt1.one = ddt.one;

                        //-----Cashrecieved----
                        dt1.RecieveCashBookingApp = ddt.RecieveCashBookingApp;
                        dt1.RecieveBankBookingApp = ddt.RecieveBankBookingApp;
                        dt1.RecieveChequeBookingApp = ddt.RecieveChequeBookingApp;
                        dt1.RecieveCashRenApp = ddt.RecieveCashRenApp;
                        dt1.RecieveBankRenApp = ddt.RecieveBankRenApp;
                        dt1.RecieveChequeRenApp = ddt.RecieveChequeRenApp;
                        //-----Pendingrecieved----
                        dt1.RecieveBankBookingPending = ddt.RecieveBankBookingPending;
                        dt1.RecieveChequeBookingPending = chequebookpen;
                        dt1.RecieveBankRenPending = ddt.RecieveBankRenPending;
                        dt1.RecieveChequeRenPending = chequerenpen;
                        //-------Return-------------
                        dt1.ReturnCashVoucher = ddt.ReturnCashVoucher;
                        dt1.ReturnBanVoucher = ddt.ReturnBanVoucher;
                        dt1.ReturnChequeVoucher = ddt.ReturnChequeVoucher;
                        dt1.ReturnCashSpot = ddt.ReturnCashSpot;
                        dt1.ReturnBankSpot = ddt.ReturnBankSpot;
                        dt1.ReturnCashExp = ddt.ReturnCashExp;
                        dt1.ReturnChequeExp = ddt.ReturnChequeExp;
                        dt1.ReturnBankExp = ddt.ReturnBankExp;
                        //----------Other-----------
                        dt1.Latefine = ddt.Latefine;
                        dt1.Relief = ddt.Relief;
                        dt1.total = ddt.total;
                        dt1.opid = user;
                        dt1.branchcode = user;
                        dt1.status = 0;
                        dt1.bankappdate = DateTime.Now.Date;
                        dt1.RejectionReason = ddt.RejectionReason;
                        dt1.TotalCashamount = Grandtotalcash;
                        dt1.TotalChequeamount = ddt.TotalChequeamount;
                        dt1.TotalBankamount = ddt.TotalBankamount;
                        dt1.Depositedamount = bal;
                        dt1.bankname = ddt.bankname;
                        dt1.remainingamount = bal - ddt.total;
                        dt1.transfertype = ddt.transfertype;
                        dt1.trid = trid1;
                        dt1.matchingdate = DateTime.Now.Date;
                        if (dt1.transfertype == "Bank")
                        {
                            dt1.transferamount = transferamount1;
                            dt1.Remark = remark1;
                            dt1.bankapp = 0;
                        }
                        else if (dt1.transfertype == "Cash")
                        {
                            dt1.transferamount = transferamount2;
                            dt1.Remark = remark2;
                            dt1.bankapp = 1;
                        }
                        dt1.closingtype = "Matching";
                        db.DailyDepositTabs.Add(dt1);
                        db.SaveChanges();
                        var clcount = (from c in db.ClosingAmount_Tabs where c.opid == user && c.status == 0 select c).Count();
                        if (clcount > 0)
                        {
                            var cl = db.ClosingAmount_Tabs.Single(c => c.opid == user && c.status == 0);
                            cl.status = 1;
                            db.SaveChanges();
                        }
                        if (remailning > 0)
                        {
                            var max = (from a in db.ClosingAmount_Tabs where a.opid == user select a.Trid).DefaultIfEmpty().Max() + 1;
                            ClosingAmount_Tab ct = new ClosingAmount_Tab();
                            ct.remaininamount = remailning;
                            ct.Amount = bal;
                            ct.depositamount = ddt.total;
                            ct.Trid = max;
                            ct.status = 0;
                            ct.date = sdate;
                            ct.opid = user;
                            db.ClosingAmount_Tabs.Add(ct);
                            db.SaveChanges();
                        }
                    }
                    #endregion
                    #region other
                    else if (ddt.transfertype == "select")
                    {
                        Double chequebookpen = (from a in db.tempappltabs where a.paymethod == "Cheque" && a.opid == user && a.status == 1 && a.formdate == sdate select a.bookingamount).DefaultIfEmpty(0).Sum();
                        Double chequerenpen = (from a in db.TempInstallmenttabs where a.paymethod == "Cheque" && a.opid == user && a.status == 1 && a.paymentdate == sdate select a.payamount).DefaultIfEmpty(0).Sum();
                        int trid1 = (from a in db.DailyDepositTabs where a.opid == User.Identity.Name select a.trid).DefaultIfEmpty(0).Max() + 1;
                        TempDailyDepositTab tdt = new TempDailyDepositTab();
                        tdt.date = sdate;
                        tdt.twothousand = ddt.twothousand;
                        tdt.fivehundred = ddt.fivehundred;
                        tdt.twohundred = ddt.twohundred;
                        tdt.hundred = ddt.hundred;
                        tdt.fifty = ddt.fifty;
                        tdt.twenty = ddt.twenty;
                        tdt.ten = ddt.ten;
                        tdt.five = ddt.five;
                        tdt.two = ddt.two;
                        tdt.one = ddt.one;

                        //-----Cashrecieved----
                        tdt.RecieveCashBookingApp = ddt.RecieveCashBookingApp;
                        tdt.RecieveBankBookingApp = ddt.RecieveBankBookingApp;
                        tdt.RecieveChequeBookingApp = ddt.RecieveChequeBookingApp;
                        tdt.RecieveCashRenApp = ddt.RecieveCashRenApp;
                        tdt.RecieveBankRenApp = ddt.RecieveBankRenApp;
                        tdt.RecieveChequeRenApp = ddt.RecieveChequeRenApp;
                        //-----Pendingrecieved----
                        tdt.RecieveBankBookingPending = ddt.RecieveBankBookingPending;
                        tdt.RecieveChequeBookingPending = chequebookpen;
                        tdt.RecieveBankRenPending = ddt.RecieveBankRenPending;
                        tdt.RecieveChequeRenPending = chequerenpen;
                        //-------Return-------------
                        tdt.ReturnCashVoucher = ddt.ReturnCashVoucher;
                        tdt.ReturnBanVoucher = ddt.ReturnBanVoucher;
                        tdt.ReturnChequeVoucher = ddt.ReturnChequeVoucher;
                        tdt.ReturnCashSpot = ddt.ReturnCashSpot;
                        tdt.ReturnBankSpot = ddt.ReturnBankSpot;
                        tdt.ReturnCashExp = ddt.ReturnCashExp;
                        tdt.ReturnChequeExp = ddt.ReturnChequeExp;
                        tdt.ReturnBankExp = ddt.ReturnBankExp;
                        //----------Other-----------
                        tdt.Latefine = ddt.Latefine;
                        tdt.Relief = ddt.Relief;
                        tdt.total = ddt.total;
                        tdt.opid = user;
                        tdt.branchcode = user;
                        tdt.status = 0;
                        tdt.bankappdate = DateTime.Now.Date;
                        tdt.RejectionReason = ddt.RejectionReason;
                        tdt.TotalCashamount = Grandtotalcash;
                        tdt.TotalChequeamount = ddt.TotalChequeamount;
                        tdt.TotalBankamount = ddt.TotalBankamount;
                        tdt.Depositedamount = bal;
                        tdt.bankname = ddt.bankname;
                        tdt.transfertype = ddt.transfertype;
                        tdt.trid = trid1;
                        if (ddt.transfertype == "Bank")
                        {
                            tdt.transferamount = transferamount1;
                            tdt.remainingamount = bal - transferamount1;
                            tdt.Remark = remark1;
                            tdt.bankapp = 0;
                        }
                        else if (ddt.transfertype == "Cash")
                        {
                            tdt.transferamount = transferamount2;
                            tdt.remainingamount = bal - transferamount2;
                            tdt.Remark = remark2;
                            tdt.bankapp = 1;
                        }
                        tdt.OpeningBalance = ddt.OpeningBalance;
                        tdt.ClosingBalance = ddt.ClosingBalance;
                        tdt.closingtype = "Matching";
                        tdt.matchingdate = sdate;
                        db.TempDailyDepositTabs.Add(tdt);
                        db.SaveChanges();
                        var clcount = (from c in db.ClosingAmount_Tabs where c.opid == user && c.status == 0 select c).Count();
                        double beforedue = (from c in db.ClosingAmount_Tabs where c.opid == User.Identity.Name && c.status == 0 select c.remaininamount).DefaultIfEmpty().Sum();
                        if (clcount > 0)
                        {
                            var cl = db.ClosingAmount_Tabs.Single(c => c.opid == user && c.status == 0);
                            cl.status = 1;
                            db.SaveChanges();
                        }
                        var max = (from a in db.ClosingAmount_Tabs where a.opid == user select a.Trid).DefaultIfEmpty().Max() + 1;
                        ClosingAmount_Tab ct = new ClosingAmount_Tab();
                        ct.remaininamount = bal;
                        ct.Amount = bal;
                        ct.depositamount = ddt.total;
                        ct.Trid = max;
                        ct.status = 0;
                        ct.date = sdate;
                        ct.opid = User.Identity.Name;
                        db.ClosingAmount_Tabs.Add(ct);
                        db.SaveChanges();

                    }
                    #endregion
                    #endregion
                    msg = "Your New Closing Saved Successfylly";
                }
            }
            TempData["msg"] = msg;
            FormsAuthentication.SetAuthCookie(user, true);
            if (Udetail.type == "Operator")
            {
                return RedirectToAction("Index", "Operator");
            }
            else if (Udetail.type == "Branch")
            {
                return RedirectToAction("Index", "Branch");
            }

            return View();
        }
       
        [HttpGet]
        public ActionResult BranchDailyClosing()
        {
            ViewData["username"] = TempData["User"];
            return View();
        }
        [HttpPost]
        public ActionResult BranchDailyClosing(DailyDepositTab ddt, string user, string date, string remark1, string remark2, Double transferamount1 = 0, Double transferamount2 = 0, Double Grandtotalcash = 0, Double bal = 0, int id = 0, int trid = 0)
        {
            ViewData["username"] = user;

            var msg = "";
            var Udetail = db.NewLogins.Single(a => a.UserName == user);
            DateTime sdate = DateTime.ParseExact(date, "dd/MM/yyyy", null);
            DailyDepositTab dt = db.DailyDepositTabs.Single(a => a.opid == user && a.Id == id && a.trid == trid && a.date == sdate);
            if (dt.transfertype == "Cash")
            {
                if (dt.twothousand != ddt.twothousand || dt.fivehundred != ddt.fivehundred || dt.twohundred != ddt.twohundred || dt.hundred != ddt.hundred || dt.fifty != ddt.fifty || dt.twenty != ddt.twenty || dt.ten != ddt.ten || dt.five != ddt.five || dt.two != ddt.two || dt.one != ddt.one)
                {
                    msg = "Fields does not Match with before fields";
                    Response.Write("<script>alert('Fields does not Match with before fields')</script>");
                }
                else
                {
                    dt.status = 1;
                    dt.matchingdate = DateTime.Now.Date;
                    db.SaveChanges();
                    msg = "Fields  Matched successfully with before fields";
                }
            }
            else if (dt.transfertype == "Bank")
            {
                if (dt.bankapp == 1)
                {
                    if (dt.twothousand != ddt.twothousand || dt.fivehundred != ddt.fivehundred || dt.twohundred != ddt.twohundred || dt.hundred != ddt.hundred || dt.fifty != ddt.fifty || dt.twenty != ddt.twenty || dt.ten != ddt.ten || dt.five != ddt.five || dt.two != ddt.two || dt.one != ddt.one)
                    {
                        msg = "Fields does not Match with before fields";
                        Response.Write("<script>alert('Fields does not Match with before fields')</script>");
                    }
                    else
                    {
                        dt.status = 1;
                        dt.matchingdate = DateTime.Now.Date;
                        db.SaveChanges();
                        msg = "Fields  Matched successfully with before fields";
                    }
                }
                else
                {

                    dt.status = 1;
                    dt.bankapp = 2;
                    dt.matchingdate = DateTime.Now.Date;
                    db.SaveChanges();

                    #region Insert
                    #region notselect
                    if (ddt.transfertype != "select")
                    {
                        Double remailning = 0;
                        Double chequebookpen = (from a in db.tempappltabs where a.paymethod == "Cheque" && a.branchcode == user && a.status == 1 && a.formdate == sdate select a.bookingamount).DefaultIfEmpty(0).Sum();
                        Double chequerenpen = (from a in db.TempInstallmenttabs where a.paymethod == "Cheque" && a.branch == user && a.status == 1 && a.paymentdate == sdate select a.payamount).DefaultIfEmpty(0).Sum();
                        if (ddt.transfertype == "Bank")
                        {
                            remailning = bal - transferamount1;
                        }
                        else if (ddt.transfertype == "Cash")
                        {
                            remailning = bal - transferamount2;
                        }
                        int trid1 = (from a in db.DailyDepositTabs where a.opid == user select a.trid).DefaultIfEmpty(0).Max() + 1;
                        DailyDepositTab dt1 = new DailyDepositTab();
                        dt1.date = sdate;
                        dt1.twothousand = ddt.twothousand;
                        dt1.fivehundred = ddt.fivehundred;
                        dt1.twohundred = ddt.twohundred;
                        dt1.hundred = ddt.hundred;
                        dt1.fifty = ddt.fifty;
                        dt1.twenty = ddt.twenty;
                        dt1.ten = ddt.ten;
                        dt1.five = ddt.five;
                        dt1.two = ddt.two;
                        dt1.one = ddt.one;

                        //-----Cashrecieved----
                        dt1.RecieveCashBookingApp = ddt.RecieveCashBookingApp;
                        dt1.RecieveBankBookingApp = ddt.RecieveBankBookingApp;
                        dt1.RecieveChequeBookingApp = ddt.RecieveChequeBookingApp;
                        dt1.RecieveCashRenApp = ddt.RecieveCashRenApp;
                        dt1.RecieveBankRenApp = ddt.RecieveBankRenApp;
                        dt1.RecieveChequeRenApp = ddt.RecieveChequeRenApp;
                        //-----Pendingrecieved----
                        dt1.RecieveBankBookingPending = ddt.RecieveBankBookingPending;
                        dt1.RecieveChequeBookingPending = chequebookpen;
                        dt1.RecieveBankRenPending = ddt.RecieveBankRenPending;
                        dt1.RecieveChequeRenPending = chequerenpen;
                        //-------Return-------------
                        dt1.ReturnCashVoucher = ddt.ReturnCashVoucher;
                        dt1.ReturnBanVoucher = ddt.ReturnBanVoucher;
                        dt1.ReturnChequeVoucher = ddt.ReturnChequeVoucher;
                        dt1.ReturnCashSpot = ddt.ReturnCashSpot;
                        dt1.ReturnBankSpot = ddt.ReturnBankSpot;
                        dt1.ReturnCashExp = ddt.ReturnCashExp;
                        dt1.ReturnChequeExp = ddt.ReturnChequeExp;
                        dt1.ReturnBankExp = ddt.ReturnBankExp;
                        //----------Other-----------
                        dt1.Latefine = ddt.Latefine;
                        dt1.Relief = ddt.Relief;
                        dt1.total = ddt.total;
                        dt1.opid = user;
                        dt1.branchcode = user;
                        dt1.status = 0;
                        dt1.bankappdate = DateTime.Now.Date;
                        dt1.RejectionReason = ddt.RejectionReason;
                        dt1.TotalCashamount = Grandtotalcash;
                        dt1.TotalChequeamount = ddt.TotalChequeamount;
                        dt1.TotalBankamount = ddt.TotalBankamount;
                        dt1.Depositedamount = bal;
                        dt1.bankname = ddt.bankname;
                        dt1.remainingamount = bal - ddt.total;
                        dt1.transfertype = ddt.transfertype;
                        dt1.trid = trid1;
                        dt1.matchingdate = DateTime.Now.Date;
                        if (dt1.transfertype == "Bank")
                        {
                            dt1.transferamount = transferamount1;
                            dt1.Remark = remark1;
                            dt1.bankapp = 0;
                        }
                        else if (dt1.transfertype == "Cash")
                        {
                            dt1.transferamount = transferamount2;
                            dt1.Remark = remark2;
                            dt1.bankapp = 1;
                        }
                        dt1.closingtype = "Matching";
                        db.DailyDepositTabs.Add(dt1);
                        db.SaveChanges();
                        var clcount = (from c in db.ClosingAmount_Tabs where c.opid == user && c.status == 0 select c).Count();
                        if (clcount > 0)
                        {
                            var cl = db.ClosingAmount_Tabs.Single(c => c.opid == user && c.status == 0);
                            cl.status = 1;
                            db.SaveChanges();
                        }
                        if (remailning > 0)
                        {
                            var max = (from a in db.ClosingAmount_Tabs where a.opid == user select a.Trid).DefaultIfEmpty().Max() + 1;
                            ClosingAmount_Tab ct = new ClosingAmount_Tab();
                            ct.remaininamount = remailning;
                            ct.Amount = bal;
                            ct.depositamount = ddt.total;
                            ct.Trid = max;
                            ct.status = 0;
                            ct.date = sdate;
                            ct.opid = user;
                            db.ClosingAmount_Tabs.Add(ct);
                            db.SaveChanges();
                        }
                    }
                    #endregion
                    #region other
                    else if (ddt.transfertype == "select")
                    {
                        Double chequebookpen = (from a in db.tempappltabs where a.paymethod == "Cheque" && a.branchcode == user && a.status == 1 && a.formdate == sdate select a.bookingamount).DefaultIfEmpty(0).Sum();
                        Double chequerenpen = (from a in db.TempInstallmenttabs where a.paymethod == "Cheque" && a.branch == user && a.status == 1 && a.paymentdate == sdate select a.payamount).DefaultIfEmpty(0).Sum();
                        int trid1 = (from a in db.DailyDepositTabs where a.opid == User.Identity.Name select a.trid).DefaultIfEmpty(0).Max() + 1;
                        TempDailyDepositTab tdt = new TempDailyDepositTab();
                        tdt.date = sdate;
                        tdt.twothousand = ddt.twothousand;
                        tdt.fivehundred = ddt.fivehundred;
                        tdt.twohundred = ddt.twohundred;
                        tdt.hundred = ddt.hundred;
                        tdt.fifty = ddt.fifty;
                        tdt.twenty = ddt.twenty;
                        tdt.ten = ddt.ten;
                        tdt.five = ddt.five;
                        tdt.two = ddt.two;
                        tdt.one = ddt.one;

                        //-----Cashrecieved----
                        tdt.RecieveCashBookingApp = ddt.RecieveCashBookingApp;
                        tdt.RecieveBankBookingApp = ddt.RecieveBankBookingApp;
                        tdt.RecieveChequeBookingApp = ddt.RecieveChequeBookingApp;
                        tdt.RecieveCashRenApp = ddt.RecieveCashRenApp;
                        tdt.RecieveBankRenApp = ddt.RecieveBankRenApp;
                        tdt.RecieveChequeRenApp = ddt.RecieveChequeRenApp;
                        //-----Pendingrecieved----
                        tdt.RecieveBankBookingPending = ddt.RecieveBankBookingPending;
                        tdt.RecieveChequeBookingPending = chequebookpen;
                        tdt.RecieveBankRenPending = ddt.RecieveBankRenPending;
                        tdt.RecieveChequeRenPending = chequerenpen;
                        //-------Return-------------
                        tdt.ReturnCashVoucher = ddt.ReturnCashVoucher;
                        tdt.ReturnBanVoucher = ddt.ReturnBanVoucher;
                        tdt.ReturnChequeVoucher = ddt.ReturnChequeVoucher;
                        tdt.ReturnCashSpot = ddt.ReturnCashSpot;
                        tdt.ReturnBankSpot = ddt.ReturnBankSpot;
                        tdt.ReturnCashExp = ddt.ReturnCashExp;
                        tdt.ReturnChequeExp = ddt.ReturnChequeExp;
                        tdt.ReturnBankExp = ddt.ReturnBankExp;
                        //----------Other-----------
                        tdt.Latefine = ddt.Latefine;
                        tdt.Relief = ddt.Relief;
                        tdt.total = ddt.total;
                        tdt.opid = user;
                        tdt.branchcode = user;
                        tdt.status = 0;
                        tdt.bankappdate = DateTime.Now.Date;
                        tdt.RejectionReason = ddt.RejectionReason;
                        tdt.TotalCashamount = Grandtotalcash;
                        tdt.TotalChequeamount = ddt.TotalChequeamount;
                        tdt.TotalBankamount = ddt.TotalBankamount;
                        tdt.Depositedamount = bal;
                        tdt.bankname = ddt.bankname;
                        tdt.transfertype = ddt.transfertype;
                        tdt.trid = trid1;
                        if (ddt.transfertype == "Bank")
                        {
                            tdt.transferamount = transferamount1;
                            tdt.remainingamount = bal - transferamount1;
                            tdt.Remark = remark1;
                            tdt.bankapp = 0;
                        }
                        else if (ddt.transfertype == "Cash")
                        {
                            tdt.transferamount = transferamount2;
                            tdt.remainingamount = bal - transferamount2;
                            tdt.Remark = remark2;
                            tdt.bankapp = 1;
                        }
                        tdt.OpeningBalance = ddt.OpeningBalance;
                        tdt.ClosingBalance = ddt.ClosingBalance;
                        tdt.closingtype = "Matching";
                        tdt.matchingdate = sdate;
                        db.TempDailyDepositTabs.Add(tdt);
                        db.SaveChanges();
                        var clcount = (from c in db.ClosingAmount_Tabs where c.opid == user && c.status == 0 select c).Count();
                        double beforedue = (from c in db.ClosingAmount_Tabs where c.opid == User.Identity.Name && c.status == 0 select c.remaininamount).DefaultIfEmpty().Sum();
                        if (clcount > 0)
                        {
                            var cl = db.ClosingAmount_Tabs.Single(c => c.opid == user && c.status == 0);
                            cl.status = 1;
                            db.SaveChanges();
                        }
                        var max = (from a in db.ClosingAmount_Tabs where a.opid == user select a.Trid).DefaultIfEmpty().Max() + 1;
                        ClosingAmount_Tab ct = new ClosingAmount_Tab();
                        ct.remaininamount = bal;
                        ct.Amount = bal;
                        ct.depositamount = ddt.total;
                        ct.Trid = max;
                        ct.status = 0;
                        ct.date = sdate;
                        ct.opid = User.Identity.Name;
                        db.ClosingAmount_Tabs.Add(ct);
                        db.SaveChanges();

                    }
                    #endregion
                    #endregion
                    msg = "Your New Closing Saved Successfylly";
                }
            }
            TempData["msg"] = msg;
            FormsAuthentication.SetAuthCookie(user, true);
            if (Udetail.type == "Operator")
            {
                return RedirectToAction("Index", "Operator");
            }
            else if (Udetail.type == "Branch")
            {
                return RedirectToAction("Index", "Branch");
            }

            return View();
        }

        [HttpGet]
        public ActionResult Vissionandmission()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        [HttpGet]
        public ActionResult Companyprofile()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        [HttpGet]
        public ActionResult management()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult OurTeam()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult value()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult homes()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult mission()
        {
            return View();
        }
        public ActionResult Investment()
        {
            return View();
        }
        public ActionResult Service()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult OurBankers()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public ActionResult bhartypaymnetgetway(string mobile, int amount = 0)
        {
            var user = db.NewLogins.Single(x => x.Mobile == mobile);
            var payloadid = "1906101559191032";
            var Txntype = "Sale";
            var _amount = amount;
            var currency_code = "356";
            var orderid = db.Database.SqlQuery<string>("exec Add_money_AutoGenerateNewid").FirstOrDefault();
            var RETURN_URL = "http://www.odhdevelopers.in/Home/responseBhartyGetway?mobiles=" + mobile + "&usernames=" + user.UserName + "&orderids=" + orderid + "";
            AddMoney_Bhatrybanktransaction obj = new AddMoney_Bhatrybanktransaction();
            obj.mobile = mobile;
            obj.username = user.UserName;
            obj.status = null;
            obj.payloadid = payloadid;
            obj.orderid = orderid;
            obj.trandate = DateTime.Now;
            obj.PaymentMode = null;
            obj.amount = _amount;
            obj.paystatus = 0;
            obj.RESPONSE_DATE_TIME = DateTime.Now;
            db.AddMoney_Bhatrybanktransactions.Add(obj);
            db.SaveChanges();

            ViewData["paylodid"] = payloadid;
            ViewData["orderid"] = orderid;
            ViewData["txntype"] = Txntype;
            ViewData["amount"] = _amount + "00";
            ViewData["currnecy_code"] = currency_code;
            ViewData["return_url"] = RETURN_URL;
            ViewData["mobile"] = mobile;
            ViewData["username"] = user.UserName;
            ViewData["name"] = user.UserName;
            ViewData["email"] = "odhdevelopers@gmail.com";
            ViewData["hashValue"] = hashValue;

            return View();
        }

        public ActionResult send_wallet_to_wallet(string UserName, string trnsferto, double amount = 0)
        {
            var userdetail = db.NewLogins.Single(x => x.UserName == UserName);
            var tranftouser = db.NewLogins.Single(x => x.Mobile == trnsferto);
            ViewData["mobile"] = userdetail.Mobile;
            ViewData["name"] = userdetail.UserName;
            ViewData["trnmob"] = tranftouser.Mobile;
            ViewData["trnname"] = tranftouser.UserName;
            ViewData["amount"] = amount;
            return View();
        }

        public JsonResult Send_Amount_From_Wallet_to_wallet(string frommobile, string mobile, double amount = 0)
        {

            var messagess = 0;
            var messagestatus = 0;
            string trnxid = string.Empty;
            List<tempcircluser2> respon = new List<tempcircluser2>();
            var cridetial = db.credentials.Single(x => x.status == 1);
            var countlogin = db.NewLogins.Where(x => x.Mobile == frommobile && x.loginstatus == 1).Count();
            if (countlogin != 0)
            {
                var usr = db.NewLogins.Single(x => x.Mobile == frommobile);
                var walletcond = db.walletconditions.Single(x => x.status == 1);
                string respon_status = "";
                var pay_status = 0;
                if (usr.status == 1)
                {
                    var datee = DateTime.Now;
                    var date1 = datee.ToString("yyyy-MM-dd");
                    var transactiondate = db.Database.SqlQuery<DateTime>("select isnull(MAX (dateadd(dd,1,transactiondate)), GETDATE()) from WalletBalance where username='" + usr.UserName + "' and txn_status=1 and cast(transactiondate as date)='" + date1 + "'").FirstOrDefault();

                    var countuser = db.NewLogins.Where(x => x.Mobile == frommobile).Count();

                    if (countuser != 0)
                    {

                        var creditbalance = (from s in db.WalletBalances where s.mobile == frommobile && s.txn_status == 1 && s.type == "Credit" select s.amount).DefaultIfEmpty(0).Sum();
                        var debitbalance = (from s in db.WalletBalances where s.mobile == frommobile && s.txn_status == 1 && s.type == "Debit" select s.amount).DefaultIfEmpty(0).Sum();
                        var totalamount = creditbalance - debitbalance;

                        var transactamount = db.Database.SqlQuery<double>("select isnull(sum(amount),0) from WalletBalance where cast(transactiondate as date)='" + date1 + "' and type='Debit' and username='" + usr.UserName + "' and txn_status=1").FirstOrDefault();
                        var counttransacttime = db.Database.SqlQuery<int>("select count(*) from WalletBalance where cast(transactiondate as date)='" + date1 + "' and type='Debit' and username='" + usr.UserName + "' and txn_status=1").FirstOrDefault();
                        var _pgreferenceno = db.Database.SqlQuery<string>("exec AutoGenerateNewid2").FirstOrDefault();

                        if (totalamount >= amount)
                        {
                            if (amount >= walletcond.minimum)
                            {
                                if (walletcond.maxtransaction > counttransacttime)
                                {
                                    if (walletcond.maxwithdrow > transactamount)
                                    {
                                        var tranfrom = db.NewLogins.Single(x => x.Mobile == frommobile);
                                        var touser = db.NewLogins.Single(x => x.Mobile == mobile);
                                        WalletBalance objWB = new WalletBalance();
                                        objWB.username = touser.UserName;
                                        objWB.mobile = touser.Mobile;
                                        objWB.customer_name = touser.UserName;
                                        objWB.transactionid = db.Database.SqlQuery<string>("exec AutoGenerateWalletId").FirstOrDefault();
                                        objWB.transactiondate = DateTime.Now;
                                        objWB.amount = amount;
                                        objWB.type = "Credit";
                                        objWB.referenceno = "To";
                                        objWB.txn_status = 1;
                                        objWB.trxnmessage = "Wallet to wallet transfer amount " + amount + " from " + tranfrom.UserName + " (" + tranfrom.UserName + ")";
                                        db.WalletBalances.Add(objWB);
                                        db.SaveChanges();

                                        objWB.username = tranfrom.UserName;
                                        objWB.mobile = tranfrom.Mobile;
                                        objWB.customer_name = tranfrom.UserName;
                                        objWB.transactionid = db.Database.SqlQuery<string>("exec AutoGenerateWalletId").FirstOrDefault();
                                        objWB.transactiondate = DateTime.Now;
                                        objWB.amount = amount;
                                        objWB.type = "Debit";
                                        objWB.referenceno = "From";
                                        objWB.txn_status = 1;
                                        objWB.trxnmessage = "Wallet to wallet transfer amount " + amount + " to " + touser.UserName + " (" + touser.UserName + ")";
                                        db.WalletBalances.Add(objWB);
                                        db.SaveChanges();
                                        messagestatus = 1;

                                        MyClass.Sendmsg(touser.Mobile, "your have received amount Rs." + amount + " /- from : " + touser.UserName + "");
                                    }
                                    else
                                    {
                                        messagestatus = 5; //Your daily maximum withdrwal limit '+_maxwithdrwal +' exceeded
                                    }
                                }
                                else
                                {
                                    messagestatus = 6; //Your daily transaction limit is '+_maxtrans+' exceeded.
                                }
                            }
                            else
                            {
                                messagestatus = 7; //Please enter amount is greater than or equal to '+_minimum+' rupees.
                            }
                        }
                        else
                        {
                            messagestatus = 8;//Please enter amount is less than or equal to wallet amount
                        }

                    }
                    else
                    {
                        messagestatus = 9; // Invalid user
                    }

                }
                else
                {
                    messagestatus = 11;
                }
            }
            else
            {
                messagestatus = 10; // Invalid user
            }
            respon.Add(new tempcircluser2
            {
                payment_status = messagestatus,
                amount = amount,
                mobile = mobile,
                bankname = trnxid

            });

            return Json(respon, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendwalletAmounttoAcc(string mobile, double amount)
        {
            
            ViewData["mobile"] = mobile;
            ViewData["amount"] = amount;
            return View();
        }

        public ActionResult rechargemobileonline(string datas, string rechargemobile, string statecode, string amount, string services, string rechmob_name, string state_name, int opid = 0)
        {
            var amountBalstatus = getaccountbal();
            if (amountBalstatus == 0)
            {


                var counlogin = db.NewLogins.Where(x => x.Mobile == rechargemobile && x.loginstatus == 1).Count();
                if (counlogin > 0)
                {
                    var user = db.NewLogins.Single(x => x.Mobile == rechargemobile);
                    var payloadid = "1906101559191032";
                    var Txntype = "Sale";
                    var _amount = Convert.ToDouble(amount);
                    var currency_code = "356";
                    var clientuniqId = db.Database.SqlQuery<string>("exec AutoGenerateNewid_for_olie_mo_recharge").FirstOrDefault();
                    var RETURN_URL = "http://www.odhdevelopers.in/Home/responseBhartyGetway";//?mobiles=" + mobile + "&clientuniqId=" + clientuniqId + "&rechargemobile=" + rechargemobile + "&statecode=" + statecode + "&services=" + services + "&rechmob_name=" + rechmob_name + "&state_name=" + state_name + "&amount=" + amount + "&opid=" + opid + "";
                    Bhatrybanktransaction_for_recharge obj = new Bhatrybanktransaction_for_recharge();
                    obj.mobile = rechargemobile;
                    obj.username = user.UserName;
                    obj.status = null;
                    obj.payloadid = payloadid;
                    obj.orderid = clientuniqId;
                    obj.trandate = DateTime.Now;
                    obj.PaymentMode = null;
                    obj.amount = _amount;
                    obj.paystatus = 0;
                    obj.RESPONSE_DATE_TIME = DateTime.Now;
                    db.Bhatrybanktransaction_for_recharges.Add(obj);
                    db.SaveChanges();

                    //-----insert record of recharge----------
                    var uniq_id = db.Database.SqlQuery<string>("exec AutoUniqueIdforrecharge").FirstOrDefault();
                    var usr = db.NewLogins.Single(x => x.Mobile == rechargemobile);
                    var op_detail = db.operator_codes.Single(x => x.operatorid == opid);
                    var s_code = Convert.ToInt32(statecode);
                    recharge_entry objr = new recharge_entry();
                    objr.usermobile = rechargemobile;
                    objr.cutomer_name = usr.UserName;
                    objr.rechargemobile = rechargemobile;
                    objr.recharge_person_name = rechmob_name;
                    objr.N_Wcirlcle = state_name;
                    objr.operatorcode = opid.ToString();
                    objr.opratorname = op_detail.op_Name;
                    objr.op_image = op_detail.op_image;
                    objr.responce = null;
                    objr.payid = null;
                    objr.clientuniqueid = uniq_id;
                    objr.operator_ref = null;
                    objr.status = null;
                    objr.txstatus_desc = null;
                    objr.message = null;
                    objr.amount = Convert.ToDouble(amount);
                    objr.rechargedate = DateTime.Now;
                    objr.rechargetype = op_detail.type;
                    objr.op_status = op_detail.operatorid.ToString();
                    objr.N_Wcircle_id = s_code;
                    objr.rechargestatus = 0;
                    objr.trnxnid = clientuniqId;
                    db.recharge_entrys.Add(objr);
                    db.SaveChanges();
                    //------end-------------------------------

                    ViewData["paylodid"] = payloadid;
                    ViewData["orderid"] = clientuniqId;
                    ViewData["txntype"] = Txntype;
                    ViewData["amount"] = _amount + "00";
                    ViewData["currnecy_code"] = currency_code;
                    ViewData["return_url"] = RETURN_URL;
                    ViewData["mobile"] = rechargemobile;
                    ViewData["username"] = user.UserName;
                    ViewData["name"] = user.UserName;
                    ViewData["email"] = "odhdevelopers@gmail.com";
                    ViewData["hashValue"] = hashValue;
                }
                else
                {
                    ViewData["error"] = 0;
                }
            }
            else
            {
                return RedirectToAction("getbalance", "Recharges");
            }
            return View();
        }

        public ActionResult responseBhartyGetway()
        {
            List<Bhatrybanktransaction_for_recharge> list = new List<Bhatrybanktransaction_for_recharge>();
            Boolean verifyHash;
            string salt = "";
            var paylodid = Request.Form["PAY_ID"];
            var orderid = Request.Form["ORDER_ID"];
            var txntype = Request.Form["TXNTYPE"];
            var amount = Request.Form["AMOUNT"];
            var currnecy_code = Request.Form["CURRENCY_CODE"];
            var return_url = Request.Form["RETURN_URL"];
            var mobile = Request.Form["CUST_PHONE"];
            var name = Request.Form["CUST_NAME"];
            var emailid = Request.Form["CUST_EMAIL"];
            var RRN = Request.Form["RRN"];
            var Auth = Request.Form["AUTH_CODE"];
            var status = Request.Form["STATUS"];
            var RESPONSE_DATE_TIME = Request.Form["RESPONSE_DATE_TIME"];
            var RESPONSE_CODE = Request.Form["RESPONSE_CODE"];
            var TXN_ID = Request.Form["TXN_ID"];
            var DUPLICATE_YN = Request.Form["DUPLICATE_YN"];
            var ORIGN_TXN_ID = Request.Form["ORIGN_TXN_ID"];
            var CUST_NAM = Request.Form["CUST_NAM"];
            var PAYMENT_TYPE = Request.Form["PAYMENT_TYPE"];
            var TXNTYPE = Request.Form["TXNTYPE"];
            var RESPONSE_MESSAGE = Request.Form["RESPONSE_MESSAGE"];
            MerchantAPI.ChecksumCalculator objCalc1 = new MerchantAPI.ChecksumCalculator();

            try
            {
                salt = ConfigurationSettings.AppSettings["saltValue"].ToString();
                verifyHash = objCalc1.verifyHash(Request.Form, salt);
                if (verifyHash == false)
                {
                    var updatebank = db.Bhatrybanktransaction_for_recharges.Single(x => x.orderid == orderid);
                    updatebank.status = status;
                    updatebank.PaymentMode = PAYMENT_TYPE;
                    if ((status == "Success" || status == "SUCCESS") || (status == "Captured" || status == "CAPTURED"))
                    {
                        updatebank.paystatus = 1;
                    }
                    updatebank.RESPONSE_DATE_TIME = Convert.ToDateTime(RESPONSE_DATE_TIME);
                    updatebank.RESPONSE_CODE = RESPONSE_CODE;
                    updatebank.TXN_ID = TXN_ID;
                    updatebank.DUPLICATE_YN = DUPLICATE_YN;
                    updatebank.ORIGN_TXN_ID = ORIGN_TXN_ID;
                    updatebank.CUST_NAM = CUST_NAM;
                    updatebank.PAYMENT_TYPE = PAYMENT_TYPE;
                    updatebank.TXNTYPE = TXNTYPE;
                    updatebank.RESPONSE_MESSAGE = RESPONSE_MESSAGE;
                    updatebank.RRN = RRN;
                    updatebank.AUTH = Auth;
                    //db.Entry(updatebank).State = EntityState.Modified;
                    db.Entry<Bhatrybanktransaction_for_recharge>(updatebank).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                    TransectionTempTable_for_recharge tbl = new TransectionTempTable_for_recharge();
                    tbl.amount = Convert.ToInt32(updatebank.amount);
                    tbl.mobile = updatebank.mobile;
                    tbl.referenceno = orderid;
                    tbl.transectindate = DateTime.Now;
                    tbl.transactionstatus = 0;
                    tbl.marchantid = "with amount" + updatebank.amount;
                    tbl.joiningstatus = 11;
                    tbl.circlepayment = 0;
                    db.TransectionTempTable_for_recharges.Add(tbl);
                    db.SaveChanges();
                }
                else
                {

                }
            }
            catch (Exception ex)
            {

            }

            return RedirectToAction("checkbhartipaymnet", "Recharges", new { mobile = mobile, clientuniq = orderid });
        }

        public ActionResult checkbhartipaymnet(string mobile, string clientuniq)
        {
            var mobile_no = mobile;
            var orderid = clientuniq;
            var count = db.Database.SqlQuery<int>("select count(*) from TransectionTempTable_for_recharge where referenceno='" + clientuniq + "' and mobile='" + mobile + "' and transactionstatus=0 and joiningstatus=11 ").FirstOrDefault();
            var countrechager = db.recharge_entrys.Where(x => x.usermobile == mobile && x.trnxnid == clientuniq).Count();

            if (count != 0 && countrechager > 0)
            {
                string refno = string.Empty;
                int amount = 0;
                ViewData["clientuniq"] = clientuniq;
                try
                {
                    var transectionlist = db.TransectionTempTable_for_recharges.Single(x => x.referenceno == clientuniq && x.mobile == mobile && x.transactionstatus == 0 && x.joiningstatus == 11);
                    refno = transectionlist.referenceno;
                    amount = transectionlist.amount;
                    var payloadid = transectionlist.marchantid;
                    string redirecturl = "";
                    redirecturl += "https://merchant.bhartipay.com/crm/services/paymentServices/getStatus?";
                    redirecturl += "PAY_ID=1906101559191032";
                    redirecturl += "&ORDER_ID=" + refno;
                    HttpClient _client = new HttpClient();
                    var result = string.Empty;
                    var msg = new StringBuilder();
                    var errores = "";
                    var httpresponse = "";


                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(redirecturl);

                    httpWebRequest.KeepAlive = false;

                    try
                    {

                        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                        var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                        result = new StreamReader(httpWebResponse.GetResponseStream()).ReadToEnd();
                        tempbhartitranchk_status m = Newtonsoft.Json.JsonConvert.DeserializeObject<tempbhartitranchk_status>(result);
                        ViewData["urls"] = "status=" + m.status + ",responseCode=" + m.responseCode + ",approvedAmount=" + m.approvedAmount + ",transactionId=" + m.transactionId + ",message=" + m.message + ",orderid=" + refno;

                        var updatebank = db.Bhatrybanktransaction_for_recharges.Single(x => x.orderid == orderid);
                        updatebank.status = m.status;
                        var temptran = db.TransectionTempTable_for_recharges.Single(x => x.referenceno == clientuniq);
                        if ((((m.status == "Success" || m.status == "SUCCESS") || (m.status == "Captured" || m.status == "CAPTURED")) && m.responseCode == "000"))
                        {
                            updatebank.paystatus = 1;
                            var rechargedetail = db.recharge_entrys.Single(x => x.trnxnid == clientuniq);
                            var usr = db.NewLogins.Single(x => x.Mobile == rechargedetail.usermobile);
                            var recharge_API = db.recharge_apis.Single(x => x.status == 1 && x.id == 1).apikey;
                            string encryptredirecturl = "";
                            encryptredirecturl += "https://www.pay2all.in/web-api/paynow?";
                            encryptredirecturl += "api_token=" + recharge_API;
                            encryptredirecturl += "&number=" + rechargedetail.rechargemobile;
                            encryptredirecturl += "&provider_id=" + rechargedetail.operatorcode;
                            encryptredirecturl += "&amount=" + rechargedetail.amount;
                            encryptredirecturl += "&client_id=" + rechargedetail.clientuniqueid;
                            var requestdata = encryptredirecturl;

                            HttpClient _client1 = new HttpClient();
                            var httpWebRequest1 = (HttpWebRequest)WebRequest.Create(requestdata);
                            httpWebRequest1.KeepAlive = false;
                            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                            var httpWebResponse1 = (HttpWebResponse)httpWebRequest1.GetResponse();
                            var result1 = new StreamReader(httpWebResponse1.GetResponseStream()).ReadToEnd();
                            rechargeresponce m2 = Newtonsoft.Json.JsonConvert.DeserializeObject<rechargeresponce>(result1);

                            var payid = m2.payid;
                            var operator_ref = m2.operator_ref;
                            var status = m2.status;
                            var txstatus_desc = m2.txstatus_desc;
                            var message = m2.message;
                            var recharedetail = db.recharge_entrys.Single(x => x.rechargemobile == rechargedetail.rechargemobile && x.clientuniqueid == rechargedetail.clientuniqueid);
                            int ids = 0;
                            if (rechargedetail.rechargetype == "Mobile Recharge")
                            {
                                ids = 1;
                            }
                            if (rechargedetail.rechargetype == "DTH Recharge")
                            {
                                ids = 4;
                            }
                            if (rechargedetail.rechargetype == "Electricity Payment")
                            {
                                ids = 7;
                            }
                            if (rechargedetail.rechargetype == "GAS Payment")
                            {
                                ids = 6;
                            }
                            if (rechargedetail.rechargetype == "DataCard Recharge")
                            {
                                ids = 3;
                            }
                            if (rechargedetail.rechargetype == "Postpaid Bill Payment")
                            {
                                ids = 2;
                            }
                            if (rechargedetail.rechargetype == "Telephone Payment")
                            {
                                ids = 5;
                            }
                            if (rechargedetail.rechargetype == "Water Bill")
                            {
                                ids = 8;
                            }
                            temptran.marchantid = temptran.marchantid + " is status " + message;

                            recharedetail.payid = payid;
                            recharedetail.operator_ref = operator_ref;
                            recharedetail.status = status;
                            recharedetail.txstatus_desc = txstatus_desc;
                            recharedetail.message = message;
                            //db.Entry(recharedetail).State = EntityState.Modified;
                            db.Entry<recharge_entry>(rechargedetail).State = System.Data.Entity.EntityState.Modified;
                            db.SaveChanges();
                            if ((status == "success" || status == "SUCCESS") && (payid != "" || payid != null) && txstatus_desc == "Success")
                            {

                                temptran.transactionstatus = 1;

                                temptran.marchantid = "Your Mobile " + recharedetail.rechargemobile + " rechage with amount " + recharedetail.amount + " with trasection id " + clientuniq + "";
                                var msg1 = "Your mobile number " + recharedetail.rechargemobile + " is recharge with rs. " + recharedetail.amount + " and your recharge status is " + status + "";
                                var msg2 = "Your mobile number " + recharedetail.rechargemobile + " and TRXN id is " + clientuniq + " and your amount " + rechargedetail.amount + " will be debited and recharge status is " + status + "";
                                MyClass.Sendmsg(recharedetail.rechargemobile, msg1);
                                MyClass.Sendmsg(recharedetail.usermobile, msg2);
                                ViewData["rechargestatus"] = status;
                                ViewData["rechargemessage"] = message;
                            }
                            else
                            {
                                ViewData["rechargestatus"] = status;
                                ViewData["rechargemessage"] = message;
                            }

                        }
                        else
                        {
                            ViewData["rechargestatus"] = "Fail";
                            temptran.marchantid = temptran.marchantid + " is " + m.status;
                        }

                        updatebank.TXN_ID = m.transactionId;
                        //db.Entry(temptran).State = EntityState.Modified;
                        db.Entry<TransectionTempTable_for_recharge>(temptran).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();
                        //db.Entry(updatebank).State = EntityState.Modified;
                        db.Entry<Bhatrybanktransaction_for_recharge>(updatebank).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                    }
                    catch (Exception exception)
                    {
                        ViewData["urls"] = "error";
                    }



                }
                catch (Exception ex)
                {
                    ViewData["urls"] = "error";
                    var transectionlist = db.Bhatrybanktransaction_for_recharges.Single(x => x.orderid == orderid && x.mobile == mobile_no && x.paystatus == 0);
                    var users = db.NewLogins.Single(x => x.Mobile == transectionlist.mobile);
                    Mainpayment obj = new Mainpayment();
                    obj.referenceno = refno;
                    obj.mobile = transectionlist.mobile;
                    obj.username = users.UserName;
                    obj.InnerException = ex.InnerException.ToString();
                    obj.Messages = ex.Message;
                    obj.entrydate = DateTime.Now;
                    db.Mainpayments.Add(obj);
                    db.SaveChanges();

                    ViewData["exception"] = "inner exception :-" + ex.InnerException + "Exception message : -" + ex.Message;

                }
            }
            else
            {
                ViewData["urls"] = "error";
                ViewData["rechargestatus"] = 0011;
                ViewData["rechargemessage"] = "Invalid transactionid";
            }
            return View();
        }

        public static int getaccountbal()
        {
            double amount = 0;
            int balstatus = 0;
            UsersContext db = new UsersContext();
            var recharge_API = db.recharge_apis.Single(x => x.status == 1 && x.id == 1).apikey;
            string encryptredirecturl = "";
            encryptredirecturl += "https://www.pay2all.in/web-api/get-balance?";
            encryptredirecturl += "api_token=" + recharge_API;
            var requestdata = encryptredirecturl;

            HttpClient _client1 = new HttpClient();
            var httpWebRequest1 = (HttpWebRequest)WebRequest.Create(requestdata);
            httpWebRequest1.KeepAlive = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var httpWebResponse1 = (HttpWebResponse)httpWebRequest1.GetResponse();
            var result1 = new StreamReader(httpWebResponse1.GetResponseStream()).ReadToEnd();
            temprechargebalance m2 = Newtonsoft.Json.JsonConvert.DeserializeObject<temprechargebalance>(result1);

            amount = m2.balance;
            var ss = db.msgupdates.Single(x => x.Id == 1);
            if (amount <= 500)
            {
                balstatus = 1;
            }
            else
            {
                balstatus = 0;
            }

            return balstatus;
        }

        public ActionResult careerportal()
        {
            return View();
        }
        //Second 
        public ActionResult career()
        {
            return View();
        }

        public ActionResult Gallerys()
        {
            return View();
        }
        public ActionResult Video()
        {
            return View();
        }

        public JsonResult forgotpassword(string userid)
        {
            List<birthdaywishes> m = new List<birthdaywishes>();
            var rst = "";
            var countuser = db.NewLogins.Where(x => x.UserName == userid).Count();
          
            if (countuser != 0)
            {
                var typeuser = db.NewLogins.Single(x => x.UserName == userid);
                var countoprator = db.Operators.Where(x => x.OperatorId == userid && x.OperatorMobile != null).Count();
                var countbranch = db.Branchtabs.Where(x => x.BranchCode == userid && x.mobile != null).Count();
                var countagent = db.AgentDetails.Where(x => x.NewAgentId == userid && x.Mobile != null).Count();
                var countmobile = db.appltabs.Where(x => x.newbondid == userid && x.mobileno != null).Count();
                if (countagent != 0)
                {
                    if (typeuser.type == "Agent")
                    {
                        var detail = db.AgentDetails.Single(x => x.NewAgentId == userid);
                        var mob = detail.Mobile.Substring(6, 4);
                        var logindetails = db.NewLogins.Single(x => x.UserName == userid);
                        MyClass.Sendmsg(detail.Mobile, "Dear Agent Mr." + detail.name + " and your memberid " + detail.NewAgentId + " and your password is : " + logindetails.Password + " for login visit our website https://odhdevelopers.in/Home/Login");
                        TempData["messg"] = "Your password will be send on register mobile no." + mob;
                        rst = Convert.ToString(TempData["messg"]);
                    }
                }
                else if (countmobile != 0)
                {
                    if (typeuser.type == "Customer")
                    {
                        var detail = db.appltabs.Single(x => x.newbondid == userid);
                        var mob = detail.mobileno.Substring(6, 4);
                        var logindetails = db.NewLogins.Single(x => x.UserName == userid);
                        MyClass.Sendmsg(detail.mobileno, "Dear Customer Mr." + detail.name + " and your memberid " + detail.newbondid + " and your password is : " + logindetails.Password + " for login visit our website https://odhdevelopers.in/Home/Login");
                        TempData["messg"] = "Your password will be send on register mobile no." + mob;
                        rst = Convert.ToString(TempData["messg"]);
                    }
                }
                else if (countbranch != 0)
                {
                    if (typeuser.type == "Branch")
                    {
                        var detail = db.Branchtabs.Single(x => x.BranchCode == userid);
                        var mob = detail.mobile.Substring(6, 4);
                        var logindetails = db.NewLogins.Single(x => x.UserName == userid);
                        MyClass.Sendmsg(detail.mobile, "Dear Branch Mr." + detail.BranchName + " and your memberid " + detail.BranchCode + " and your password is : " + logindetails.Password + " for login visit our website https://odhdevelopers.in/Home/Login");
                        TempData["messg"] = "Your password will be send on register mobile no." + mob;
                        rst = Convert.ToString(TempData["messg"]);
                    }
                }
                else if (countoprator != 0)
                {
                    if (typeuser.type == "Operator")
                    {
                        var detail = db.Operators.Single(x => x.OperatorId == userid);
                        var mob = detail.OperatorMobile.Substring(6, 4);
                        var logindetails = db.NewLogins.Single(x => x.UserName == userid);
                        MyClass.Sendmsg(detail.OperatorMobile, "Dear Operator Mr." + detail.OperatorName + " and your memberid " + detail.OperatorId + " and your password is : " + logindetails.Password + " for login visit our website https://odhdevelopers.in/Home/Login");
                        TempData["messg"] = "Your password will be send on register mobile no." + mob;
                        rst = Convert.ToString(TempData["messg"]);
                    }
                }
                else
                {
                    TempData["messg"] = "You does not have mobile number";
                    rst = Convert.ToString(TempData["messg"]);
                }
            }
            else
            {
                TempData["messg"] = "Invalid Userid";
                rst = Convert.ToString(TempData["messg"]);
            }
           
            m.Add(new birthdaywishes { 
                sms=rst
            });
            return Json(m, JsonRequestBehavior.AllowGet);
        }

        #region 30-Mar-2019

        public JsonResult CheckPlotAvialablity(int projectid, int phaseid, int blockid)
        {
            List<PlotList2> plotnoList = new List<PlotList2>();
            var block = db.BlockTabs.Single(d => d.Plancode == projectid && d.phaseid == phaseid && d.Id == blockid);
            var plan = db.Plans.Single(d => d.Plancode == projectid);
            var Phase = db.PhaseTabs.Single(d => d.Id == phaseid);
                                 
            for (var i = block.min; i <= block.max; i++)
            {
                var rcount = db.appltabs.Where(d => d.projectid == projectid && d.phase == Phase.Phase && d.plotno == i && d.block == block.block).ToList();
                var tcount = db.tempappltabs.Where(d => d.projectid == projectid && d.phase == Phase.Phase && d.plotno == i && d.block == block.block).ToList();
                var hcount = db.HoldingPlots.Where(d => d.holdprojectid == projectid && d.holdphaseid == phaseid && d.holdblock == blockid && d.holdplotno == i).ToList();
                if (rcount.Count() > 0)
                {
                    plotnoList.Add(new PlotList2 { id = i, availablity = "Registered", status = 1, projectid = plan.Plancode, blockid = block.Id, projectname = plan.Planname, blockname = block.block, plotno = i });
                }
                else if (tcount.Count() > 0)
                {
                    plotnoList.Add(new PlotList2 { id = i, availablity = "ALREADY SOLD", status = 2, projectid = plan.Plancode, blockid = block.Id, projectname = plan.Planname, blockname = block.block, plotno = i });
                }
                else if (hcount.Count() > 0)
                {
                    plotnoList.Add(new PlotList2 { id = i, availablity = "HOLD", status = 3, projectid = plan.Plancode, blockid = block.Id, projectname = plan.Planname, blockname = block.block, plotno = i });
                }
                else
                {
                    plotnoList.Add(new PlotList2 { id = i, availablity = "AVAILABLE", status = 4, projectid = plan.Plancode, blockid = block.Id, projectname = plan.Planname, blockname = block.block, plotno = i });
                }
            }
            return Json(plotnoList, JsonRequestBehavior.AllowGet);
        }

        #endregion 30-Mar-2019

        public ActionResult transaction_status()
        {
            ViewBag.Message = "transaction_status";
            return View();
        }

        #region make_payment
        public ActionResult MakePayment()
        {
            return View();
        }

        [HttpPost]
        public ActionResult MakePayment(paymentGetwayICICI_Openconnect t1)
        {
            return View();
        }

        public ActionResult openICICIGetway(string datas, string amount)
        {
            //    ViewData["mobile"] = user.mobile;
            //    ViewData["email"] = user.customer_email;
            //    ViewData["amount"] = amount;

            return View();
        }

        string gid(int len)
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
            return s.Substring(0, len);


        }
        public JsonResult getorderid(double amount, string email, string contact, string mtx, string shacode, string timestamp, string name, string payment_remark)
        {
            List<tempiciciiorderid> result = new List<tempiciciiorderid>();
            paymentGetwayICICI_Openconnect obj = new paymentGetwayICICI_Openconnect();
            var trxnids = db.Database.SqlQuery<string>("exec Transactionid").FirstOrDefault();

            obj.userid = "UID" + gid(10);
            obj.name = name;
            obj.payment_remark = payment_remark;
            obj.mobile = contact;
            obj.email = email;
            obj.trxnid = trxnids;
            obj.banktrxn_status = "Initiate";
            obj.payment_id = null;
            obj.payment_token_id = null;
            obj.paymentdate = DateTime.Now;
            obj.remark = "Request for payment has been sent";
            obj.status = 0;
            obj.error_msg = null;
            obj.error_code = null;
            obj.amount = amount;
            obj.mtxid = mtx;
            obj.timestamp = timestamp;
            db.paymentGetwayICICI_Openconnects.Add(obj);
            db.SaveChanges();
            var auth = "Bearer " + payment_gateway.api_key + ":" + payment_gateway.secret_key;
            var client = new RestClient("https://icp-api.bankopen.co/api/payment_token");
            client.Timeout = -1;
            var request = new RestRequest(Method.POST);
            request.AddHeader("postman-token", "507a67df-6493-2df5-76aa-07db5cbe74f6");
            request.AddHeader("cache-control", "no-cache");
            request.AddHeader("access-Key", payment_gateway.api_key);
            request.AddHeader("x-o-timestamp", timestamp);
            request.AddHeader("content-type", "application/json");
            request.AddHeader("authorization", shacode);
            request.AddHeader("accept", "*/*");
            request.AddParameter("application/json", "{\"amount\":\"" + amount + "\",\r\n\"contact_number\":\"" + contact + "\",\r\n\"email_id\":\"" + email + "\",\r\n\"currency\":\"INR\",\"mtx\":\"" + mtx + "\"}", ParameterType.RequestBody);
            IRestResponse response = client.Execute(request);
            Console.WriteLine(response.Content);
            if (response.StatusCode.ToString() == "OK")
            {
                tempopenconnet_ICICIgetway m = Newtonsoft.Json.JsonConvert.DeserializeObject<tempopenconnet_ICICIgetway>(response.Content);
                var getpaytbl = db.paymentGetwayICICI_Openconnects.Single(x => x.mobile == contact && x.trxnid == trxnids);
                getpaytbl.payment_token_id = m.id;
                //db.Entry(getpaytbl).State = EntityState.Modified;
                db.Entry<paymentGetwayICICI_Openconnect>(getpaytbl).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                result.Add(new tempiciciiorderid
                {
                    status = 1,
                    response = response.Content
                });
            }
            else
            {
                result.Add(new tempiciciiorderid
                {
                    status = 0,
                    response = response.Content
                });
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult getOrderStatus(string status, string payment_id, string payment_token_id, string mobile)
        {

            var getpaytbl = db.paymentGetwayICICI_Openconnects.Single(x => x.payment_token_id == payment_token_id && x.mobile == mobile);
            getpaytbl.payment_id = payment_id;
            getpaytbl.banktrxn_status = status;
            if (status == "captured")
            {
                getpaytbl.status = 1;
            }
            //db.Entry(getpaytbl).State = EntityState.Modified;
            db.Entry<paymentGetwayICICI_Openconnect>(getpaytbl).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            return Json("", JsonRequestBehavior.DenyGet);
        }

        public ActionResult getorderstatus()
        {
            return View();
        }

        public JsonResult GetorderbyICICI(string orderid)
        {
            List<tempiciciiorderid> result = new List<tempiciciiorderid>();
            var countorder = db.paymentGetwayICICI_Openconnects.Where(x => x.trxnid == orderid).Count();
            if (countorder > 0)
            {
                var payment_toke_id = db.paymentGetwayICICI_Openconnects.Single(x => x.trxnid == orderid).payment_token_id;
                var auth = "Bearer " + payment_gateway.api_key + ":" + payment_gateway.secret_key;
                var client = new RestClient("https://icp-api.bankopen.co/api/payment_token/" + payment_toke_id);
                client.Timeout = -1;
                var request = new RestRequest(Method.GET);
                request.AddHeader("Accept", "*/*");
                request.AddHeader("Authorization", auth);
                request.AddHeader("Content-Type", "application/json");
                IRestResponse response = client.Execute(request);
                Console.WriteLine(response.Content);
                if (response.StatusCode.ToString() == "OK")
                {
                    result.Add(new tempiciciiorderid
                    {
                        status = 1,
                        response = response.Content
                    });
                }
                else
                {
                    result.Add(new tempiciciiorderid
                    {
                        status = 0,
                        response = response.Content
                    });
                }
            }
            else
            {
                result.Add(new tempiciciiorderid
                {
                    status = 7,

                });
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult check_exist(string mobile)
        {
            object data = null;
            var f = new paymentGetwayICICI_Openconnect();
            var max_id = db.paymentGetwayICICI_Openconnects.Where(d => d.mobile == mobile).Select(d => d.srno).DefaultIfEmpty(0).Max();
            if (max_id == 0)
            {
                data = "d".Select(d => new
                {
                    status = 0
                }).FirstOrDefault();
            }
            else
            {
                f = db.paymentGetwayICICI_Openconnects.Single(d => d.srno == max_id);
                data = "d".Select(d => new
                {
                    status = 1,
                    name = f.name,
                    email = f.email
                }).FirstOrDefault();
            }
            return Json(data, JsonRequestBehavior.DenyGet);
        }
        public ActionResult Transaction_Status()
        {
            return View();
        }

        #endregion make_payment

    }
    
}
