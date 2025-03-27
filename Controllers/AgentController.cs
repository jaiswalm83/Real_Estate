using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Security;
using ODHDEVELOPERS.Models;
using System.IO;
using CrystalDecisions.CrystalReports.Engine;
using System.Data.Entity;

namespace ODHDEVELOPERS.Controllers
{

    public class AgentController : Controller
    {

        SqlConnection con = new SqlConnection();
        private UsersContext db = new UsersContext();

        public static string NewAgentid;
        public static int  vmonth = 0, vvyear = 0;
        public static DateTime stdate = DateTime.Now, enddate = DateTime.Now, pdate = DateTime.Now;

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
        public ActionResult Bond()
        {
            List<appltab> agent = new List<appltab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {

                agent = (from pl in db.appltabs where pl.newintroducerid == User.Identity.Name select pl).ToList();
            }
            return View(agent);
        }
        public ActionResult PrintOfficialCustomerReport()
        {
            List<appltab> customer = new List<appltab>();
            customer = db.appltabs.Where(s => s.newintroducerid == User.Identity.Name).ToList();
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "CustomerList.rpt"));
            rd.SetDataSource(customer);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(stream, "application/pdf");
            }

            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
            finally
            {
                rd.Close();
                rd.Dispose();
            }
            return View();
        }

        [HttpGet]
        public ActionResult AgentProfile()
        {
            List<AgentDetail> dn = new List<AgentDetail>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                dn = db.AgentDetails.Where(c => c.NewAgentId == User.Identity.Name).ToList();
            }
            return View(dn);
        }

        [HttpGet]
        public ActionResult EditAgent(int id = 0)
        {
            if (User.Identity.IsAuthenticated)
            {
                AgentDetail op = db.AgentDetails.Single(d => d.AgencyCode == id);
                if (op == null)
                {
                    return HttpNotFound();
                }
                return View(op);

            }
            return RedirectToAction("Logout");

        }
        [HttpPost]
        public ActionResult EditAgent(AgentDetail model)
        {
            if (User.Identity.IsAuthenticated)
            {                            
                    var agd = db.AgentDetails.Single(d=>d.NewAgentId==model.NewAgentId);
                    agd.OtherMobile = model.OtherMobile;
                    agd.Panno = model.Panno;
                    agd.Passportno = model.Passportno;
                    agd.Drivinglno = model.Drivinglno;
                    agd.BankName = model.BankName;
                    agd.BankBranchName = model.BankBranchName;
                    agd.BankAccountno = model.BankAccountno;
                    agd.IFCCode = model.IFCCode;
                    agd.BankAddress = model.BankAddress;
                    agd.Aadhaar_No = model.Aadhaar_No;
                    if (agd.Aadhaar_No == null || agd.Aadhaar_No == "" || agd.Aadhaar_No == "N/A" || agd.Aadhaar_No == "NA")
                    {
                        agd.Aadhaar_status = 0;
                    }
                    else
                    {
                        agd.Aadhaar_status = 1;
                        agd.Aadhaar_AppDate = DateTime.Now.Date;
                    }
                    if (agd.Panno == null || agd.Panno == "" || agd.Aadhaar_No == "N/A" || agd.Aadhaar_No == "NA")
                    {
                        agd.PanStatus = 0;
                    }
                    else
                    {
                        agd.PanStatus = 1;
                        agd.PAN_AppDate = DateTime.Now.Date;
                    }
                db.Entry<AgentDetail>(agd).State = System.Data.Entity.EntityState.Modified;

                //db.Entry(agd).State = EntityState.Modified;
                    db.SaveChanges();
                    ViewBag.message = "Agent Updated Successfully.";                
                return View(model);
            }
            return RedirectToAction("Logout");

        }

        [HttpGet]
        public ActionResult BrokerChain()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Agent");
            }
            else
            {

                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                int rcount = 0;
                List<BrokerChain> bclist = new List<BrokerChain>();
                var br = db.AgentDetails.Single(a => a.NewAgentId == User.Identity.Name);
                var bb = db.Branchtabs.Single(o => o.BranchCode == br.BranchCode);
                bclist.Add(new BrokerChain { newagentid = br.NewAgentId, name = br.name, rankcode = br.RankCode, rankname = br.RankName, introducerid = br.NewIntroducerId, introname = br.IntroName, branchname = bb.BranchName, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                List<agents> dalist = new List<agents>();
                var alist = (from al in db.AgentDetails where al.NewIntroducerId == User.Identity.Name select new { al.AgencyCode }).Distinct();
                foreach (var a in alist)
                {
                    rcount = rcount + 1;
                    dalist.Add(new agents { sr = rcount, agentcode = a.AgencyCode });
                }

                var maxsragent = dalist.Count;
                var minsragent = 1;
                while (minsragent <= maxsragent)
                {

                    var da = dalist.Where(a => a.sr == minsragent);
                    foreach (var d in da.ToList())
                    {
                        var aalist = (from al in db.AgentDetails where al.IntroducerCode == d.agentcode select new { al.AgencyCode }).Distinct();
                        foreach (var aa in aalist)
                        {
                            rcount = rcount + 1;
                            dalist.Add(new agents { sr = rcount, agentcode = aa.AgencyCode });

                        }

                    }
                    minsragent = minsragent + 1;
                    maxsragent = dalist.Count;
                }
                foreach (var dd in dalist)
                {
                    var ad = db.AgentDetails.Single(a => a.AgencyCode == dd.agentcode);
                    bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = bb.BranchName, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });

                }
                NewAgentid = User.Identity.Name;
                return View(bclist);
            }
        }
        public ActionResult PrintBrokerChainReport()
        {

            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            int rcount = 0;
            List<BrokerChain> bclist = new List<BrokerChain>();
            var br = db.AgentDetails.Single(a => a.NewAgentId == NewAgentid);
            var bb = db.Branchtabs.Single(o => o.BranchCode == br.BranchCode);
            bclist.Add(new BrokerChain { newagentid = br.NewAgentId, name = br.name, rankcode = br.RankCode, rankname = br.RankName, introducerid = br.NewIntroducerId, introname = br.IntroName, branchname = bb.BranchName, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
            List<agents> dalist = new List<agents>();
            var alist = (from al in db.AgentDetails where al.NewIntroducerId == NewAgentid select new { al.AgencyCode }).Distinct();
            foreach (var a in alist)
            {
                rcount = rcount + 1;
                dalist.Add(new agents { sr = rcount, agentcode = a.AgencyCode });
            }

            var maxsragent = dalist.Count;
            var minsragent = 1;
            while (minsragent <= maxsragent)
            {

                var da = dalist.Where(a => a.sr == minsragent);
                foreach (var d in da.ToList())
                {
                    var aalist = (from al in db.AgentDetails where al.IntroducerCode == d.agentcode select new { al.AgencyCode }).Distinct();
                    foreach (var aa in aalist)
                    {
                        rcount = rcount + 1;
                        dalist.Add(new agents { sr = rcount, agentcode = aa.AgencyCode });
                    }

                }
                minsragent = minsragent + 1;
                maxsragent = dalist.Count;
            }
            foreach (var dd in dalist)
            {
                var ad = db.AgentDetails.Single(a => a.AgencyCode == dd.agentcode);
                bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = bb.BranchName, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });

            }

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports/Branch"), "AdvBrokerChain.rpt"));
            rd.SetDataSource(bclist);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(stream, "application/pdf");
            }

            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
            finally
            {
                rd.Close();
                rd.Dispose();
            }
            return View(bclist);
        }

        [HttpGet]
        public ActionResult BrokerAccount()
        {
            List<BrokerAccount> balist = new List<BrokerAccount>();
            if (!User.Identity.IsAuthenticated)
            {

                return RedirectToAction("Logout", "Agent");
            }

            else
            {
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var mr = db.Members.Single(c => c.Id == 1);
                var plan4 = db.Plans.Single(p4 => p4.Plancode == 4);
                var ar = db.AgentDetails.Single(n => n.NewAgentId == User.Identity.Name);
                var br = db.Branchtabs.Single(o => o.BranchCode == ar.BranchCode);
                int rcount = 0;
                List<BrokerChain> bclist = new List<BrokerChain>();
                bclist.Add(new BrokerChain { newagentid = ar.NewAgentId, name = ar.name, rankcode = ar.RankCode, rankname = ar.RankName, introducerid = ar.NewIntroducerId, introname = ar.IntroName, branchname = br.BranchName, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                List<agents> dalist = new List<agents>();
                var alist = (from al in db.AgentDetails where al.NewIntroducerId == ar.NewAgentId select new { al.AgencyCode }).Distinct();
                foreach (var a in alist)
                {
                    rcount = rcount + 1;
                    dalist.Add(new agents { sr = rcount, agentcode = a.AgencyCode });
                }
                var maxsragent = dalist.Count;
                var minsragent = 1;
                while (minsragent <= maxsragent)
                {
                    var da = dalist.Where(a => a.sr == minsragent);
                    foreach (var d in da.ToList())
                    {
                        var aalist = (from al in db.AgentDetails where al.IntroducerCode == d.agentcode select new { al.AgencyCode }).Distinct();
                        foreach (var aa in aalist)
                        {
                            rcount = rcount + 1;
                            dalist.Add(new agents { sr = rcount, agentcode = aa.AgencyCode });
                        }

                    }
                    minsragent = minsragent + 1;
                    maxsragent = dalist.Count;
                }
                foreach (var dd in dalist)
                {
                    var ad = db.AgentDetails.Single(a => a.AgencyCode == dd.agentcode);
                    if (ad.Status == 1)
                    {
                        bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = br.BranchName, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                    }
                }
                foreach (var bc in bclist)
                {
                    var blist = (from a in db.appltabs where a.newintroducerid == bc.newagentid orderby a.newbondid select a).ToList();
                    foreach (var b in blist)
                    {
                        var aa = db.AgentDetails.Single(r => r.NewAgentId == b.newintroducerid);
                        if (b.plantype == plan4.shortcutname)
                        {
                            balist.Add(new BrokerAccount { newintroducerid = b.newintroducerid, name = aa.name, newbondid = b.newbondid, cname = b.name, amount = b.totalcon, planname = b.planname, term = b.term, mode = b.mode, expirydate = b.expirydate, formdate = b.formdate, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                        }
                        else
                        {
                            balist.Add(new BrokerAccount { newintroducerid = b.newintroducerid, name = aa.name, newbondid = b.newbondid, cname = b.name, amount = b.payment, planname = b.planname, term = b.term, mode = b.mode, expirydate = b.expirydate, formdate = b.formdate, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                        }
                    }
                }
                NewAgentid = User.Identity.Name;
                return View(balist);
            }
        }
        public ActionResult PrintBrokerAccountReport()
        {
            List<BrokerAccount> balist = new List<BrokerAccount>();
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var mr = db.Members.Single(c => c.Id == 1);
            var plan4 = db.Plans.Single(p4 => p4.Plancode == 4);
            var ar = db.AgentDetails.Single(n => n.NewAgentId == NewAgentid);
            var br = db.Branchtabs.Single(o => o.BranchCode == ar.BranchCode);
            int rcount = 0;
            List<BrokerChain> bclist = new List<BrokerChain>();
            bclist.Add(new BrokerChain { newagentid = ar.NewAgentId, name = ar.name, rankcode = ar.RankCode, rankname = ar.RankName, introducerid = ar.NewIntroducerId, introname = ar.IntroName, branchname = br.BranchName, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
            List<agents> dalist = new List<agents>();
            var alist = (from al in db.AgentDetails where al.NewIntroducerId == ar.NewAgentId select new { al.AgencyCode }).Distinct();
            foreach (var a in alist)
            {
                rcount = rcount + 1;
                dalist.Add(new agents { sr = rcount, agentcode = a.AgencyCode });
            }
            var maxsragent = dalist.Count;
            var minsragent = 1;
            while (minsragent <= maxsragent)
            {
                var da = dalist.Where(a => a.sr == minsragent);
                foreach (var d in da.ToList())
                {
                    var aalist = (from al in db.AgentDetails where al.IntroducerCode == d.agentcode select new { al.AgencyCode }).Distinct();
                    foreach (var aa in aalist)
                    {
                        rcount = rcount + 1;
                        dalist.Add(new agents { sr = rcount, agentcode = aa.AgencyCode });
                    }

                }
                minsragent = minsragent + 1;
                maxsragent = dalist.Count;
            }
            foreach (var dd in dalist)
            {
                var ad = db.AgentDetails.Single(a => a.AgencyCode == dd.agentcode);
                if (ad.Status == 1)
                {
                    bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = br.BranchName, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                }
            }
            foreach (var bc in bclist)
            {
                var blist = (from a in db.appltabs where a.newintroducerid == bc.newagentid orderby a.newbondid select a).ToList();
                foreach (var b in blist)
                {
                    var aa = db.AgentDetails.Single(r => r.NewAgentId == b.newintroducerid);
                    if (b.plantype == plan4.shortcutname)
                    {
                        balist.Add(new BrokerAccount { newintroducerid = b.newintroducerid, name = aa.name, newbondid = b.newbondid, cname = b.name, amount = b.totalcon, planname = b.planname, term = b.term, mode = b.mode, expirydate = b.expirydate, formdate = b.formdate, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                    }
                    else
                    {
                        balist.Add(new BrokerAccount { newintroducerid = b.newintroducerid, name = aa.name, newbondid = b.newbondid, cname = b.name, amount = b.payment, planname = b.planname, term = b.term, mode = b.mode, expirydate = b.expirydate, formdate = b.formdate, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                    }
                }
            }

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "BrokerAccount.rpt"));
            rd.SetDataSource(balist);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(stream, "application/pdf");
            }

            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
            finally
            {
                rd.Close();
                rd.Dispose();
            }
            return View();
        }

        [HttpGet]
        public ActionResult Selfbusiness()
        {
            List<DatewiseDueCollection> dwl = new List<DatewiseDueCollection>();
            var ar = db.AgentDetails.Single(a => a.NewAgentId == User.Identity.Name);
            var bondintro = (from o in db.appltabs where o.newintroducerid == User.Identity.Name select o).ToList();
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            foreach (var c in bondintro)
            {
                var bookingamount = (from n in db.Installmenttabs where n.newbondid == c.newbondid && n.installmentno == 1 select n.payamount).DefaultIfEmpty(0).Sum();
                var emiamount = (from n in db.Installmenttabs where n.newbondid == c.newbondid && n.payamount != 0 && n.installmentno > 1 select n.payamount).DefaultIfEmpty(0).Sum();
                dwl.Add(new DatewiseDueCollection { newbondid = c.newbondid, name = c.name, newintroducerid = ar.NewAgentId, IntroName = ar.name, formdate = c.formdate, amount = bookingamount, installamount = emiamount, mobileno = c.mobileno, branchname = c.branchcode, companyname = cr.CompanyName });
            }
            return View(dwl);
        }
        public ActionResult PrintSelfbusiness()
        {
            List<DatewiseDueCollection> dwl = new List<DatewiseDueCollection>();
            var ar = db.AgentDetails.Single(a => a.NewAgentId == User.Identity.Name);
            var bondintro = (from o in db.appltabs where o.newintroducerid == User.Identity.Name select o).ToList();
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            foreach (var c in bondintro)
            {
                var bookingamount = (from n in db.Installmenttabs where n.newbondid == c.newbondid && n.installmentno == 1 select n.payamount).DefaultIfEmpty(0).Sum();
                var emiamount = (from n in db.Installmenttabs where n.newbondid == c.newbondid && n.payamount != 0 && n.installmentno > 1 select n.payamount).DefaultIfEmpty(0).Sum();
                dwl.Add(new DatewiseDueCollection { newbondid = c.newbondid, name = c.name, newintroducerid = ar.NewAgentId, IntroName = ar.name, formdate = c.formdate, amount = bookingamount, installamount = emiamount, mobileno = c.mobileno, branchname = c.branchcode, companyname = cr.CompanyName });
            }

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Selfbusiness.rpt"));
            rd.SetDataSource(dwl);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(stream, "application/pdf");
            }

            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
            finally
            {
                rd.Close();
                rd.Dispose();
            }


            return View();

        }

        [HttpGet]
        public ActionResult BrokerChainCollection()
        {
            List<TempBrokerChainCollection> bcclist = new List<TempBrokerChainCollection>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Agent");
            }
            else
            {
                return View(bcclist);
            }
        }
        [HttpPost]
        public ActionResult BrokerChainCollection(string newagentid, DateTime sdate, DateTime edate)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Agent");
            }
            else
            {

                List<TempBrokerChainCollection> bcclist = new List<TempBrokerChainCollection>();
                MonthName mn = new MonthName();
                newagentid = User.Identity.Name;
                var mon = mn.numbertomonthname(sdate.Month);
                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var ashish = db.BrokerCommLists.Where(a => a.newagentid == newagentid && a.month == mon && a.Year == sdate.Year).Count();
                if (ashish == 1)
                {
                    var ar = db.BrokerCommLists.Single(a => a.newagentid == newagentid && a.month == mon && a.Year == sdate.Year);

                    #region Select Broker Chain Start Here

                    List<BrokerChain> bclist = new List<BrokerChain>();
                    bclist.Add(new BrokerChain { newagentid = ar.newagentid, name = ar.name, rankcode = ar.rankcode, rankname = ar.rankname, introducerid = ar.newintroducerid, introname = ar.Introname, branchname = User.Identity.Name });
                    int rcount = 0;
                    List<agents> dalist = new List<agents>();
                    var alist = (from al in db.BrokerCommLists where al.newintroducerid == newagentid && al.month == mon && al.Year == sdate.Year select new { al.agencycode }).Distinct();
                    foreach (var a in alist)
                    {
                        rcount = rcount + 1;
                        dalist.Add(new agents { sr = rcount, agentcode = a.agencycode });
                    }

                    var maxsragent = dalist.Count;
                    var minsragent = 1;
                    while (minsragent <= maxsragent)
                    {

                        var da = dalist.Where(a => a.sr == minsragent);
                        foreach (var d in da.ToList())
                        {
                            var aalist = (from al in db.BrokerCommLists where al.introducerid == d.agentcode && al.month == mon && al.Year == sdate.Year select new { al.agencycode }).Distinct();
                            foreach (var aa in aalist)
                            {
                                rcount = rcount + 1;
                                dalist.Add(new agents { sr = rcount, agentcode = aa.agencycode });

                            }

                        }
                        minsragent = minsragent + 1;
                        maxsragent = dalist.Count;
                    }
                    foreach (var dd in dalist)
                    {
                        var ad = db.BrokerCommLists.Single(a => a.agencycode == dd.agentcode && a.month == mon && a.Year == sdate.Year);
                        bclist.Add(new BrokerChain { newagentid = ad.newagentid, name = ad.name, rankcode = ad.rankcode, rankname = ad.rankname, introducerid = ad.newintroducerid, introname = ad.Introname, branchname = User.Identity.Name });

                    }

                    #endregion

                    #region Select Broker Collectin Start Here

                    foreach (var bc in bclist)
                    {
                        List<Tempbond> tb = new List<Tempbond>();
                        List<TempSelfCollection> bsclist = new List<TempSelfCollection>();

                        var ag = db.BrokerCommLists.Single(hj => hj.newagentid == bc.newagentid && hj.month == mon && hj.Year == sdate.Year);

                        var tbd = (from ap in db.appltabs where ap.newintroducerid == bc.newagentid  select ap.bondid).ToList();
                        var bd = (from yt in db.Installmenttabs where yt.paymentdate >= sdate && yt.paymentdate <= edate && tbd.Contains(yt.bondid) select new { yt.bondid }).Distinct();
                        var count = tbd.Count();
                        if (count > 0)
                        {
                            foreach (var item in bd.ToList())
                            {
                                tb.Add(new Tempbond { bondid = item.bondid });
                            }

                            foreach (var t in tb.ToList())
                            {
                                List<SelfCollection> sf = new List<SelfCollection>();
                                int bondid = t.bondid;
                                var list = (from o in db.Installmenttabs
                                            join u in db.appltabs on o.newbondid equals u.newbondid
                                            where o.bondid == bondid && o.payamount != 0 && o.paymentdate >= sdate && o.paymentdate <= edate
                                            orderby u.newbondid
                                            select new { o.payamount, o.installmentno, o.year, o.planname, u.plantype, u.plancode, u.totalcon }).ToList();

                                foreach (var g in list.ToList())
                                {
                                    string subplan = g.plantype;
                                    if (subplan == "FD")
                                    {
                                        sf.Add(new SelfCollection { sis = g.payamount });
                                    }
                                    if (subplan == "MIS" && g.installmentno == 1 && g.year == 1)
                                    {
                                        sf.Add(new SelfCollection { PPSFresh = g.totalcon });
                                    }
                                    if (subplan == "NFP" && g.installmentno == 1 && g.year == 1)
                                    {
                                        sf.Add(new SelfCollection { NFPFresh = g.payamount });
                                    }
                                    if (subplan == "RD" && g.installmentno == 1 && g.year == 1)
                                    {
                                        sf.Add(new SelfCollection { MISFresh = g.payamount });
                                    }

                                    if (subplan == "RD" && g.installmentno > 1 && g.year == 1)
                                    {
                                        sf.Add(new SelfCollection { MIS1 = g.payamount });
                                    }

                                    if (subplan == "RD" && g.installmentno > 1 && g.year == 2)
                                    {
                                        sf.Add(new SelfCollection { MIS2 = g.payamount });
                                    }

                                    if (subplan == "RD" && g.installmentno > 1 && g.year == 3)
                                    {
                                        sf.Add(new SelfCollection { MIS3 = g.payamount });
                                    }

                                    if (subplan == "RD" && g.installmentno > 1 && g.year == 4)
                                    {
                                        sf.Add(new SelfCollection { MIS4 = g.payamount });
                                    }

                                    if (subplan == "RD" && g.installmentno > 1 && g.year == 5)
                                    {
                                        sf.Add(new SelfCollection { MIS5 = g.payamount });
                                    }

                                    if (subplan == "RD" && g.installmentno > 1 && g.year == 6)
                                    {
                                        sf.Add(new SelfCollection { MIS6 = g.payamount });
                                    }

                                    if (subplan == "RD" && g.installmentno > 1 && g.year >= 7)
                                    {
                                        sf.Add(new SelfCollection { MIS7a = g.payamount });
                                    }
                                }

                                var brow = db.appltabs.Single(bg => bg.bondid == bondid);
                                double sissum = 0, misfsum = 0, mis1sum = 0, mis2sum = 0, mis3sum = 0, mis4sum = 0, mis5sum = 0, mis6sum = 0, mis7sum = 0, pppsum = 0, nfssum = 0;

                                sissum = sf.Sum(tr => tr.sis);
                                misfsum = sf.Sum(tr => tr.MISFresh);
                                mis1sum = sf.Sum(tr => tr.MIS1);
                                mis2sum = sf.Sum(tr => tr.MIS2);
                                mis3sum = sf.Sum(tr => tr.MIS3);
                                mis4sum = sf.Sum(tr => tr.MIS4);
                                mis5sum = sf.Sum(tr => tr.MIS5);
                                mis6sum = sf.Sum(tr => tr.MIS6);
                                mis7sum = sf.Sum(tr => tr.MIS7a);
                                pppsum = sf.Sum(tr => tr.PPSFresh);
                                nfssum = sf.Sum(tr => tr.NFPFresh);
                                bsclist.Add(new TempSelfCollection { Newbondid = brow.newbondid, Customername = brow.name, brokername = ag.name, brokercode = ag.newagentid, tsis = sissum, tMISFresh = misfsum, tPPSFresh = pppsum, tNFPFresh = nfssum, tMIS1 = mis1sum, tMIS2 = mis2sum, tMIS3 = mis3sum, tMIS4 = mis4sum, tMIS5 = mis5sum, tMIS6 = mis6sum, tMIS7a = mis7sum, companyname = cr.CompanyName, branchname = User.Identity.Name, branch = mr.branchname, bond = mr.custname, agent = mr.agentname, plan = mr.planname, sdate = sdate, edate = edate });
                            }


                        }
                        double bsissum = 0, bmisfsum = 0, bppsum = 0, bnfpsum = 0, bmis1sum = 0, bmis2sum = 0, bmis3sum = 0, bmis4sum = 0, bmis5sum = 0, bmis6sum = 0, bmis7sum = 0;
                        bsissum = bsclist.Sum(tr => tr.tsis);
                        bmisfsum = bsclist.Sum(tr => tr.tMISFresh);
                        bmis1sum = bsclist.Sum(tr => tr.tMIS1);
                        bmis2sum = bsclist.Sum(tr => tr.tMIS2);
                        bmis3sum = bsclist.Sum(tr => tr.tMIS3);
                        bmis4sum = bsclist.Sum(tr => tr.tMIS4);
                        bmis5sum = bsclist.Sum(tr => tr.tMIS5);
                        bmis6sum = bsclist.Sum(tr => tr.tMIS6);
                        bmis7sum = bsclist.Sum(tr => tr.tMIS7a);
                        bppsum = bsclist.Sum(tr => tr.tPPSFresh);
                        bnfpsum = bsclist.Sum(tr => tr.tNFPFresh);


                        if (bsissum + bmisfsum + bmis1sum + bmis2sum + bmis3sum + bmis4sum + bmis5sum + bmis6sum + bmis7sum + bppsum + bnfpsum > 0)
                        {
                            bcclist.Add(new TempBrokerChainCollection { brokercode = newagentid, brokername = ar.name, newagentid = bc.newagentid, name = bc.name, tsis = bsissum, tMISFresh = bmisfsum, tppp = bppsum, tMIP = bnfpsum, tMIS1 = bmis1sum, tMIS2 = bmis2sum, tMIS3 = bmis3sum, tMIS4 = bmis4sum, tMIS5 = bmis5sum, tMIS6 = bmis6sum, tMIS7a = bmis7sum, companyname = cr.CompanyName, branchname = User.Identity.Name, branch = ag.mobile, bond = mr.custname, agent = mr.agentname, plan = mr.planname, sdate = sdate, edate = edate, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                        }

                    }

                    #endregion
                }
                else
                {
                    ViewBag.msg = "This Month Chain Not Released";
                }

                NewAgentid = newagentid;
                stdate = sdate;
                enddate = edate;

                return View(bcclist);
            }
        }
        public ActionResult PrintBrokerChainCollection()
        {
            MonthName mn = new MonthName();
            var mon = mn.numbertomonthname(stdate.Month);
            var mr = db.Members.Single(m => m.Id == 1);
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var ar = db.BrokerCommLists.Single(a => a.newagentid == NewAgentid && a.month == mon && a.Year == stdate.Year);
            List<TempBrokerChainCollection> bcclist = new List<TempBrokerChainCollection>();
            #region Select Broker Chain Start Here

            List<BrokerChain> bclist = new List<BrokerChain>();
            bclist.Add(new BrokerChain { newagentid = ar.newagentid, name = ar.name, rankcode = ar.rankcode, rankname = ar.rankname, introducerid = ar.newintroducerid, introname = ar.Introname, branchname = User.Identity.Name });
            int rcount = 0;
            List<agents> dalist = new List<agents>();
            var alist = (from al in db.BrokerCommLists where al.newintroducerid == NewAgentid && al.month == mon && al.Year == stdate.Year select new { al.agencycode }).Distinct();
            foreach (var a in alist)
            {
                rcount = rcount + 1;
                dalist.Add(new agents { sr = rcount, agentcode = a.agencycode });
            }

            var maxsragent = dalist.Count;
            var minsragent = 1;
            while (minsragent <= maxsragent)
            {

                var da = dalist.Where(a => a.sr == minsragent);
                foreach (var d in da.ToList())
                {
                    var aalist = (from al in db.BrokerCommLists where al.introducerid == d.agentcode && al.month == mon && al.Year == stdate.Year select new { al.agencycode }).Distinct();
                    foreach (var aa in aalist)
                    {
                        rcount = rcount + 1;
                        dalist.Add(new agents { sr = rcount, agentcode = aa.agencycode });

                    }

                }
                minsragent = minsragent + 1;
                maxsragent = dalist.Count;
            }
            foreach (var dd in dalist)
            {
                var ad = db.BrokerCommLists.Single(a => a.agencycode == dd.agentcode && a.month == mon && a.Year == stdate.Year);
                bclist.Add(new BrokerChain { newagentid = ad.newagentid, name = ad.name, rankcode = ad.rankcode, rankname = ad.rankname, introducerid = ad.newintroducerid, introname = ad.Introname, branchname = User.Identity.Name });

            }

            #endregion

            #region Select Broker Collectin Start Here

            foreach (var bc in bclist)
            {
                List<Tempbond> tb = new List<Tempbond>();
                List<TempSelfCollection> bsclist = new List<TempSelfCollection>();

                var ag = db.BrokerCommLists.Single(hj => hj.newagentid == bc.newagentid && hj.month == mon && hj.Year == stdate.Year);


                var tbd = (from ap in db.appltabs where ap.newintroducerid == bc.newagentid  select ap.bondid).ToList();
                var bd = (from yt in db.Installmenttabs where yt.paymentdate >= stdate && yt.paymentdate <= enddate && tbd.Contains(yt.bondid) select new { yt.bondid }).Distinct();
                var count = tbd.Count();
                if (count > 0)
                {
                    foreach (var item in bd.ToList())
                    {
                        tb.Add(new Tempbond { bondid = item.bondid });
                    }

                    foreach (var t in tb.ToList())
                    {
                        List<SelfCollection> sf = new List<SelfCollection>();
                        int bondid = t.bondid;
                        var list = (from o in db.Installmenttabs
                                    join u in db.appltabs on o.newbondid equals u.newbondid
                                    where o.bondid == bondid && o.payamount != 0 && o.paymentdate >= stdate && o.paymentdate <= enddate
                                    orderby u.newbondid
                                    select new { o.payamount, o.installmentno, o.year, o.planname, u.plantype, u.plancode, u.totalcon }).ToList();

                        foreach (var g in list.ToList())
                        {
                            string subplan = g.plantype;
                            if (subplan == "FD")
                            {
                                sf.Add(new SelfCollection { sis = g.payamount });
                            }
                            if (subplan == "MIS" && g.installmentno == 1 && g.year == 1)
                            {
                                sf.Add(new SelfCollection { PPSFresh = g.totalcon });
                            }
                            if (subplan == "NFP" && g.installmentno == 1 && g.year == 1)
                            {
                                sf.Add(new SelfCollection { NFPFresh = g.payamount });
                            }
                            if (subplan == "RD" && g.installmentno == 1 && g.year == 1)
                            {
                                sf.Add(new SelfCollection { MISFresh = g.payamount });
                            }

                            if (subplan == "RD" && g.installmentno > 1 && g.year == 1)
                            {
                                sf.Add(new SelfCollection { MIS1 = g.payamount });
                            }

                            if (subplan == "RD" && g.installmentno > 1 && g.year == 2)
                            {
                                sf.Add(new SelfCollection { MIS2 = g.payamount });
                            }

                            if (subplan == "RD" && g.installmentno > 1 && g.year == 3)
                            {
                                sf.Add(new SelfCollection { MIS3 = g.payamount });
                            }

                            if (subplan == "RD" && g.installmentno > 1 && g.year == 4)
                            {
                                sf.Add(new SelfCollection { MIS4 = g.payamount });
                            }

                            if (subplan == "RD" && g.installmentno > 1 && g.year == 5)
                            {
                                sf.Add(new SelfCollection { MIS5 = g.payamount });
                            }

                            if (subplan == "RD" && g.installmentno > 1 && g.year == 6)
                            {
                                sf.Add(new SelfCollection { MIS6 = g.payamount });
                            }

                            if (subplan == "RD" && g.installmentno > 1 && g.year >= 7)
                            {
                                sf.Add(new SelfCollection { MIS7a = g.payamount });
                            }
                        }

                        var brow = db.appltabs.Single(bg => bg.bondid == bondid);
                        double sissum = 0, misfsum = 0, mis1sum = 0, mis2sum = 0, mis3sum = 0, mis4sum = 0, mis5sum = 0, mis6sum = 0, mis7sum = 0, pppsum = 0, nfssum = 0;

                        sissum = sf.Sum(tr => tr.sis);
                        misfsum = sf.Sum(tr => tr.MISFresh);
                        mis1sum = sf.Sum(tr => tr.MIS1);
                        mis2sum = sf.Sum(tr => tr.MIS2);
                        mis3sum = sf.Sum(tr => tr.MIS3);
                        mis4sum = sf.Sum(tr => tr.MIS4);
                        mis5sum = sf.Sum(tr => tr.MIS5);
                        mis6sum = sf.Sum(tr => tr.MIS6);
                        mis7sum = sf.Sum(tr => tr.MIS7a);
                        pppsum = sf.Sum(tr => tr.PPSFresh);
                        nfssum = sf.Sum(tr => tr.NFPFresh);
                        bsclist.Add(new TempSelfCollection { Newbondid = brow.newbondid, Customername = brow.name, brokername = ag.name, brokercode = ag.newagentid, tsis = sissum, tMISFresh = misfsum, tPPSFresh = pppsum, tNFPFresh = nfssum, tMIS1 = mis1sum, tMIS2 = mis2sum, tMIS3 = mis3sum, tMIS4 = mis4sum, tMIS5 = mis5sum, tMIS6 = mis6sum, tMIS7a = mis7sum, companyname = cr.CompanyName, branchname = User.Identity.Name, branch = mr.branchname, bond = mr.custname, agent = mr.agentname, plan = mr.planname, sdate = stdate, edate = enddate });
                    }


                }
                double bsissum = 0, bmisfsum = 0, bppsum = 0, bnfpsum = 0, bmis1sum = 0, bmis2sum = 0, bmis3sum = 0, bmis4sum = 0, bmis5sum = 0, bmis6sum = 0, bmis7sum = 0;
                bsissum = bsclist.Sum(tr => tr.tsis);
                bmisfsum = bsclist.Sum(tr => tr.tMISFresh);
                bmis1sum = bsclist.Sum(tr => tr.tMIS1);
                bmis2sum = bsclist.Sum(tr => tr.tMIS2);
                bmis3sum = bsclist.Sum(tr => tr.tMIS3);
                bmis4sum = bsclist.Sum(tr => tr.tMIS4);
                bmis5sum = bsclist.Sum(tr => tr.tMIS5);
                bmis6sum = bsclist.Sum(tr => tr.tMIS6);
                bmis7sum = bsclist.Sum(tr => tr.tMIS7a);
                bppsum = bsclist.Sum(tr => tr.tPPSFresh);
                bnfpsum = bsclist.Sum(tr => tr.tNFPFresh);


                if (bsissum + bmisfsum + bmis1sum + bmis2sum + bmis3sum + bmis4sum + bmis5sum + bmis6sum + bmis7sum + bppsum + bnfpsum > 0)
                {
                    bcclist.Add(new TempBrokerChainCollection { brokercode = NewAgentid, brokername = ar.name, newagentid = bc.newagentid, name = bc.name, tsis = bsissum, tMISFresh = bmisfsum, tppp = bppsum, tMIP = bnfpsum, tMIS1 = bmis1sum, tMIS2 = bmis2sum, tMIS3 = bmis3sum, tMIS4 = bmis4sum, tMIS5 = bmis5sum, tMIS6 = bmis6sum, tMIS7a = bmis7sum, companyname = cr.CompanyName, branchname = User.Identity.Name, branch = ag.mobile, bond = mr.custname, agent = mr.agentname, plan = mr.planname, sdate = stdate, edate = enddate, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                }

            }

            #endregion


            ReportDocument rdp = new ReportDocument();
            rdp.Load(Path.Combine(Server.MapPath("~/Reports"), "BrokerChainCollection.rpt"));
            rdp.SetDataSource(bcclist);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rdp.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(stream, "application/pdf");
            }

            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
            finally
            {
                rdp.Close();
                rdp.Dispose();
            }
            return View();
        }


        [HttpGet]
        public ActionResult RenewalReceipt()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                List<Tempbond> tb = new List<Tempbond>();
                List<SelfdueCollection> sd = new List<SelfdueCollection>();

                var bd = db.appltabs.Where(n => n.newintroducerid == User.Identity.Name);
                var ar = db.AgentDetails.Single(a => a.NewAgentId == User.Identity.Name);
                var br = db.Branchtabs.Single(o => o.BranchCode == ar.BranchCode);
                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var llr = db.TDSLF_tabs.Single(l => l.Id == 1);
                int count = bd.Count();
                if (count > 0)
                {
                    foreach (var item in bd.ToList())
                    {
                        tb.Add(new Tempbond { bondid = item.bondid });
                    }
                    foreach (var t in tb.ToList())
                    {
                        Double latefine = 0;
                        var duelist = (from i in db.Installmenttabs where i.bondid == t.bondid && i.payamount == 0 && i.expirydate <= DateTime.Now select i).ToList();
                        foreach (var rsl in duelist)
                        {
                            if (rsl.mode == "Monthly")
                            {
                                DateTime pdate = Convert.ToDateTime(rsl.prevexpirydate).AddDays(15);
                                if (DateTime.Now.Date > pdate.Date)
                                {
                                    int diff = Convert.ToInt32(DateTime.Now.Date.Subtract(pdate).TotalDays);
                                    int r = diff / 30;
                                    latefine = Math.Round(((rsl.amount * llr.latefine) / 100) * (r + 1), 2);
                                }
                            }
                            else if (rsl.mode == "Quarterly")
                            {
                                DateTime pdate = Convert.ToDateTime(rsl.prevexpirydate).AddMonths(1);
                                if (DateTime.Now.Date > pdate.Date)
                                {
                                    int diff = Convert.ToInt32(DateTime.Now.Date.Subtract(pdate).TotalDays);
                                    int r = diff / 91;
                                    latefine = Math.Round(((rsl.amount * llr.latefine) / 100) * (r + 1), 2);
                                }
                            }
                            else if (rsl.mode == "Halfyearly")
                            {
                                DateTime pdate = Convert.ToDateTime(rsl.prevexpirydate).AddMonths(1);
                                if (DateTime.Now.Date > pdate.Date)
                                {
                                    int diff = Convert.ToInt32(DateTime.Now.Date.Subtract(pdate).TotalDays);
                                    int r = diff / 182;
                                    latefine = Math.Round(((rsl.amount * llr.latefine) / 100) * (r + 1), 2);

                                }
                            }
                            else if (rsl.mode == "Yearly")
                            {
                                DateTime pdate = Convert.ToDateTime(rsl.prevexpirydate).AddMonths(1);
                                if (DateTime.Now.Date > pdate.Date)
                                {
                                    int diff = Convert.ToInt32(DateTime.Now.Date.Subtract(pdate).TotalDays);
                                    int r = diff / 365;
                                    latefine = Math.Round(((rsl.amount * llr.latefine) / 100) * (r + 1), 2);
                                }
                            }
                        }
                        var dueinst = (from i in db.Installmenttabs where i.bondid == t.bondid && i.payamount == 0 && i.expirydate <= DateTime.Now select i).Count();
                        var dueamount = (from i in db.Installmenttabs where i.bondid == t.bondid && i.payamount == 0 && i.expirydate <= DateTime.Now select i.amount).DefaultIfEmpty(0).Sum();
                        var row = db.appltabs.Single(r => r.bondid == t.bondid);
                        sd.Add(new SelfdueCollection { newagentid = ar.NewAgentId, name = ar.name, newbondid = row.newbondid, cname = row.name, expirydate = row.expirydate, planname = row.planname, term = latefine, mode = row.mode, amount = dueamount, installmentno = dueinst, companyname = cr.CompanyName, branchname = br.BranchName, branch = mr.branchname, bond = mr.custname, agent = mr.agentname, plan = mr.planname, mobileno = row.mobileno, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                    }
                }
                else
                {
                    Response.Write("<script>alert('There is No data Related To this agent')</script>");

                }
                NewAgentid = User.Identity.Name;
                return View(sd);

            }
        }
        public ActionResult PrintBrokerSelfDueReport()
        {
            List<Tempbond> tb = new List<Tempbond>();
            List<SelfdueCollection> sd = new List<SelfdueCollection>();
            var bd = db.appltabs.Where(n => n.newintroducerid == User.Identity.Name);
            var ar = db.AgentDetails.Single(a => a.NewAgentId == User.Identity.Name);
            var br = db.Branchtabs.Single(o => o.BranchCode == ar.BranchCode);
            var mr = db.Members.Single(m => m.Id == 1);
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var llr = db.TDSLF_tabs.Single(l => l.Id == 1);
            int count = bd.Count();
            if (count > 0)
            {

                foreach (var item in bd.ToList())
                {
                    tb.Add(new Tempbond { bondid = item.bondid });
                }

                foreach (var t in tb.ToList())
                {
                    Double latefine = 0;
                    var duelist = (from i in db.Installmenttabs where i.bondid == t.bondid && i.payamount == 0 && i.expirydate <= DateTime.Now select i).ToList();
                    foreach (var rsl in duelist)
                    {
                        if (rsl.mode == "Monthly")
                        {
                            DateTime pdate = Convert.ToDateTime(rsl.prevexpirydate).AddDays(15);
                            if (DateTime.Now.Date > pdate.Date)
                            {
                                int diff = Convert.ToInt32(DateTime.Now.Date.Subtract(pdate).TotalDays);
                                int r = diff / 30;
                                latefine = Math.Round(((rsl.amount * llr.latefine) / 100) * (r + 1), 2);
                            }
                        }
                        else if (rsl.mode == "Quarterly")
                        {
                            DateTime pdate = Convert.ToDateTime(rsl.prevexpirydate).AddMonths(1);
                            if (DateTime.Now.Date > pdate.Date)
                            {
                                int diff = Convert.ToInt32(DateTime.Now.Date.Subtract(pdate).TotalDays);
                                int r = diff / 91;
                                latefine = Math.Round(((rsl.amount * llr.latefine) / 100) * (r + 1), 2);
                            }
                        }
                        else if (rsl.mode == "Halfyearly")
                        {
                            DateTime pdate = Convert.ToDateTime(rsl.prevexpirydate).AddMonths(1);
                            if (DateTime.Now.Date > pdate.Date)
                            {
                                int diff = Convert.ToInt32(DateTime.Now.Date.Subtract(pdate).TotalDays);
                                int r = diff / 182;
                                latefine = Math.Round(((rsl.amount * llr.latefine) / 100) * (r + 1), 2);

                            }
                        }
                        else if (rsl.mode == "Yearly")
                        {
                            DateTime pdate = Convert.ToDateTime(rsl.prevexpirydate).AddMonths(1);
                            if (DateTime.Now.Date > pdate.Date)
                            {
                                int diff = Convert.ToInt32(DateTime.Now.Date.Subtract(pdate).TotalDays);
                                int r = diff / 365;
                                latefine = Math.Round(((rsl.amount * llr.latefine) / 100) * (r + 1), 2);
                            }
                        }
                    }
                    var dueinst = (from i in db.Installmenttabs where i.bondid == t.bondid && i.payamount == 0 && i.expirydate <= DateTime.Now select i).Count();
                    var dueamount = (from i in db.Installmenttabs where i.bondid == t.bondid && i.payamount == 0 && i.expirydate <= DateTime.Now select i.amount).DefaultIfEmpty(0).Sum();
                    var row = db.appltabs.Single(r => r.bondid == t.bondid);
                    sd.Add(new SelfdueCollection { newagentid = ar.NewAgentId, name = ar.name, newbondid = row.newbondid, cname = row.name, expirydate = row.expirydate, planname = row.planname, term = latefine, mode = row.mode, amount = dueamount, installmentno = dueinst, companyname = cr.CompanyName, branchname = br.BranchName, branch = mr.branchname, bond = mr.custname, agent = mr.agentname, plan = mr.planname, mobileno = row.mobileno, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                }

            }

            else
            {
                Response.Write("<script>alert('There is No data Related To this agent')</script>");

            }

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "BrokerSelfDueCollection.rpt"));
            rd.SetDataSource(sd);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(stream, "application/pdf");
            }

            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
            finally
            {
                rd.Close();
                rd.Dispose();
            }
            return View(sd);
        }

        [HttpGet]
        public ActionResult BrokerChainDueCollection()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Agent");
            }
            else
            {
                List<ChaindueCollection> bcdclist = new List<ChaindueCollection>();
                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var ar = db.AgentDetails.Single(a => a.NewAgentId == User.Identity.Name);

                #region Select Broker Chain Start Here

                List<BrokerChain> bclist = new List<BrokerChain>();
                bclist.Add(new BrokerChain { newagentid = ar.NewAgentId, name = ar.name, rankcode = ar.RankCode, rankname = ar.RankName, introducerid = ar.NewIntroducerId, introname = ar.IntroName, branchname = User.Identity.Name });
                int rcount = 0;
                List<agents> dalist = new List<agents>();
                var alist = (from al in db.AgentDetails where al.NewIntroducerId == ar.NewAgentId select new { al.AgencyCode }).Distinct();
                foreach (var a in alist)
                {
                    rcount = rcount + 1;
                    dalist.Add(new agents { sr = rcount, agentcode = a.AgencyCode });
                }

                var maxsragent = dalist.Count;
                var minsragent = 1;
                while (minsragent <= maxsragent)
                {

                    var da = dalist.Where(a => a.sr == minsragent);
                    foreach (var d in da.ToList())
                    {
                        var aalist = (from al in db.AgentDetails where al.IntroducerCode == d.agentcode select new { al.AgencyCode }).Distinct();
                        foreach (var aa in aalist)
                        {
                            rcount = rcount + 1;
                            dalist.Add(new agents { sr = rcount, agentcode = aa.AgencyCode });

                        }

                    }
                    minsragent = minsragent + 1;
                    maxsragent = dalist.Count;
                }
                foreach (var dd in dalist)
                {
                    var ad = db.AgentDetails.Single(a => a.AgencyCode == dd.agentcode);
                    bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = User.Identity.Name });

                }

                #endregion
                #region Select Due Collectin Start Here
                foreach (var bc in bclist)
                {

                    DateTime date = DateTime.Now.Date;
                    var tb = (from n in db.appltabs where n.newintroducerid == bc.newagentid && n.plantype == "RD" && n.status == 1 select n);
                    foreach (var t in tb.ToList())
                    {
                        double amount = 0;
                        List<BondDueCollection> bsdclist = new List<BondDueCollection>();
                        var bwdilist = (from i in db.Installmenttabs where i.bondid == t.bondid && i.payamount == 0 && i.prevexpirydate < date select i).ToList();
                        foreach (var bwdi in bwdilist)
                        {
                            bsdclist.Add(new BondDueCollection { amount = bwdi.amount });
                        }


                        amount = bsdclist.Sum(tr => tr.amount);
                        if (amount > 0)
                        {
                            bcdclist.Add(new ChaindueCollection { brokercode = ar.NewAgentId, brokername = ar.name, newagentid = bc.newagentid, name = bc.name, newbondid = t.newbondid, cname = t.name, expirydate = t.expirydate, planname = t.planname, term = t.term, mode = t.mode, amount = amount, companyname = cr.CompanyName, branchname = User.Identity.Name, branch = mr.branchname, bond = mr.custname, agent = mr.agentname, plan = mr.planname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice, mobileno = t.mobileno });
                        }
                    }

                }
                #endregion

                NewAgentid = ar.NewAgentId;
                return View(bcdclist);
            }
        }
        public ActionResult PrintBrokerChainDueReport()
        {
            List<ChaindueCollection> bcdclist = new List<ChaindueCollection>();
            var mr = db.Members.Single(m => m.Id == 1);
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var ar = db.AgentDetails.Single(a => a.NewAgentId == NewAgentid);

            #region Select Broker Chain Start Here

            List<BrokerChain> bclist = new List<BrokerChain>();
            bclist.Add(new BrokerChain { newagentid = ar.NewAgentId, name = ar.name, rankcode = ar.RankCode, rankname = ar.RankName, introducerid = ar.NewIntroducerId, introname = ar.IntroName, branchname = User.Identity.Name });
            int rcount = 0;
            List<agents> dalist = new List<agents>();
            var alist = (from al in db.AgentDetails where al.NewIntroducerId == NewAgentid select new { al.AgencyCode }).Distinct();
            foreach (var a in alist)
            {
                rcount = rcount + 1;
                dalist.Add(new agents { sr = rcount, agentcode = a.AgencyCode });
            }

            var maxsragent = dalist.Count;
            var minsragent = 1;
            while (minsragent <= maxsragent)
            {

                var da = dalist.Where(a => a.sr == minsragent);
                foreach (var d in da.ToList())
                {
                    var aalist = (from al in db.AgentDetails where al.IntroducerCode == d.agentcode select new { al.AgencyCode }).Distinct();
                    foreach (var aa in aalist)
                    {
                        rcount = rcount + 1;
                        dalist.Add(new agents { sr = rcount, agentcode = aa.AgencyCode });

                    }

                }
                minsragent = minsragent + 1;
                maxsragent = dalist.Count;
            }
            foreach (var dd in dalist)
            {
                var ad = db.AgentDetails.Single(a => a.AgencyCode == dd.agentcode);
                bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = User.Identity.Name });

            }

            #endregion
            #region Select Due Collectin Start Here
            foreach (var bc in bclist)
            {

                DateTime date = DateTime.Now.Date;
                var tb = (from n in db.appltabs where n.newintroducerid == bc.newagentid && n.plantype == "RD" && n.status == 1 select n);
                foreach (var t in tb.ToList())
                {
                    double amount = 0;
                    List<BondDueCollection> bsdclist = new List<BondDueCollection>();
                    var bwdilist = (from i in db.Installmenttabs where i.bondid == t.bondid && i.payamount == 0 && i.prevexpirydate < date select i).ToList();
                    foreach (var bwdi in bwdilist)
                    {
                        bsdclist.Add(new BondDueCollection { amount = bwdi.amount });
                    }


                    amount = bsdclist.Sum(tr => tr.amount);
                    if (amount > 0)
                    {
                        bcdclist.Add(new ChaindueCollection { brokercode = NewAgentid, brokername = ar.name, newagentid = bc.newagentid, name = bc.name, newbondid = t.newbondid, cname = t.name, expirydate = t.expirydate, planname = t.planname, term = t.term, mode = t.mode, amount = amount, companyname = cr.CompanyName, branchname = User.Identity.Name, branch = mr.branchname, bond = mr.custname, agent = mr.agentname, plan = mr.planname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice, mobileno = t.mobileno });
                    }
                }

            }
            #endregion

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "BrokerChainDueCollection.rpt"));
            rd.SetDataSource(bcdclist);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(stream, "application/pdf");
            }

            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
            finally
            {
                rd.Close();
                rd.Dispose();
            }
            return View();
        }

        [HttpGet]
        public ActionResult QuotaCollection()
        {
            List<QuotaCollectionList> quotalist = new List<QuotaCollectionList>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Agent");
            }
            else
            {
                int count = (from n in db.NewLogins where n.UserName == User.Identity.Name select n.UserName).Count();
                if (count == 1)
                {
                    var log = db.NewLogins.Single(a => a.UserName == User.Identity.Name);
                    if (log.status == 1 && log.type == "Agent")
                    {
                        return View(quotalist);
                    }
                    else
                    {
                        return RedirectToAction("Logout", "Agent");
                    }
                }
                else
                {
                    return RedirectToAction("Logout", "Agent");
                }
            }
        }
        [HttpPost]
        public ActionResult QuotaCollection(DateTime sdate, DateTime edate)
        {
            List<QuotaCollectionList> quotalist = new List<QuotaCollectionList>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Agent");
            }
            else
            {
                MonthName mn = new MonthName();
                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var dalist = db.AgentDetails.Where(a => a.NewAgentId == User.Identity.Name).ToList();

                #region Select Broker Chain Start Here

                List<BrokerChain> bclist = new List<BrokerChain>();
                foreach (var dd in dalist)
                {
                    var ad = db.AgentDetails.Single(a => a.AgencyCode == dd.AgencyCode);
                    bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = ad.BranchCode });
                }

                #endregion

                foreach (var bc in bclist)
                {


                    var bwr = db.AgentDetails.Single(bw => bw.NewAgentId == bc.newagentid);
                    var rr = db.Ranktabs.Single(rw => rw.RankCode == bc.rankcode);

                    var br = db.Branchtabs.Single(b => b.BranchCode == bwr.BranchCode);

                    #region Select Sub Broker Chain Start Here

                    List<BrokerChain> subbclist = new List<BrokerChain>();
                    subbclist.Add(new BrokerChain { newagentid = bwr.NewAgentId, name = bwr.name, rankcode = bwr.RankCode, rankname = bwr.RankName, introducerid = bwr.NewIntroducerId, introname = bwr.IntroName, branchname = User.Identity.Name });
                    int srcount = 0;
                    List<agents> sdalist = new List<agents>();
                    var salist = (from sal in db.AgentDetails where sal.NewIntroducerId == bwr.NewAgentId select new { sal.AgencyCode }).Distinct();
                    foreach (var sa in salist)
                    {
                        srcount = srcount + 1;
                        sdalist.Add(new agents { sr = srcount, agentcode = sa.AgencyCode });
                    }

                    var smaxsragent = sdalist.Count;
                    var sminsragent = 1;
                    while (sminsragent <= smaxsragent)
                    {

                        var sda = sdalist.Where(sa => sa.sr == sminsragent);
                        foreach (var sd in sda.ToList())
                        {
                            var saalist = (from sal in db.AgentDetails where sal.IntroducerCode == sd.agentcode select new { sal.AgencyCode }).Distinct();
                            foreach (var saa in saalist)
                            {
                                srcount = srcount + 1;
                                sdalist.Add(new agents { sr = srcount, agentcode = saa.AgencyCode });

                            }

                        }
                        sminsragent = sminsragent + 1;
                        smaxsragent = sdalist.Count;
                    }

                    foreach (var sdd in sdalist)
                    {
                        var sad = db.AgentDetails.Single(sa => sa.AgencyCode == sdd.agentcode);
                        subbclist.Add(new BrokerChain { newagentid = sad.NewAgentId, name = sad.name, rankcode = sad.RankCode, rankname = sad.RankName, introducerid = sad.NewIntroducerId, introname = sad.IntroName, branchname = User.Identity.Name });
                    }

                    #endregion

                    List<InstallmentList> inslist = new List<InstallmentList>();
                    Double btotalcoll = 0, bquotaamount = 0, bipptotalcoll = 0, bdpptotalcoll = 0, Difference = 0;
                    var status = string.Empty;
                    var prankcount = 0;
                    foreach (var sbc in subbclist)
                    {

                        var tbd = (from ap in db.appltabs where ap.newintroducerid == sbc.newagentid select ap.bondid);
                        var bondlist = (from yt in db.Installmenttabs where yt.paymentdate >= sdate && yt.paymentdate <= edate && tbd.Contains(yt.bondid) select new { yt.newbondid }).Distinct().ToList();
                        foreach (var bond in bondlist)
                        {
                            var planlist = (from pl in db.Installmenttabs where pl.paymentdate >= sdate && pl.paymentdate <= edate && pl.newbondid == bond.newbondid && pl.type != "Adjustment" && pl.year == 1 select pl).ToList();
                            foreach (var plan in planlist)
                            {
                                inslist.Add(new InstallmentList { newbondid = plan.newbondid, installmentno = plan.installmentno, mode = plan.mode, year = plan.year, term = plan.term_plan, planname = plan.planname, amount = plan.payamount });
                            }
                        }
                    }

                    var brocount = db.AgentDetails.Where(sc => sc.NewIntroducerId == bc.newagentid).Count();
                    var pnlist = inslist.Select(i => new { i.planname, i.term }).Distinct();
                    foreach (var pn in pnlist)
                    {
                        var pc = db.Plans.Single(t => t.shortcutname == pn.planname);
                        var p = db.RateTabs.Single(i => i.projectname == pc.Planname && i.term == pn.term);
                        Double totalcoll = 0, quotaamount = 0;
                        totalcoll = inslist.Where(ftr => ftr.planname == pn.planname && ftr.term == pn.term).Sum(r => (Double)r.amount);
                        quotaamount = Math.Round(((totalcoll * p.quotaper) / 100), 2);
                        btotalcoll = btotalcoll + totalcoll;
                        bquotaamount = bquotaamount + quotaamount;
                    }

                    if (rr.TeamQuota > bquotaamount)
                    {
                        Difference = rr.TeamQuota - bquotaamount;
                        status = "NA";
                    }
                    else
                    {
                        Difference = 0;
                        status = "ACHIEVED";
                    }
                    if (btotalcoll > 0)
                    {
                        //quotalist.Add(new QuotaCollectionList { newagentid = bc.newagentid, name = bc.name, rankname = bc.rankname, newintroducerid = bwr.NewIntroducerId, IPPBusiness = bipptotalcoll, DPPBusiness = bdpptotalcoll, business = btotalcoll, quotaamount = bquotaamount, PromotionTarget = rr.TeamQuota, rankcount = brocount, prankcount = prankcount, Difference = Difference, sdate = sdate, edate = edate, companyname = cr.CompanyName, branchname = br.BranchName, branch = mr.branchname, bond = mr.custname, plan = mr.planname, agent = mr.agentname, rankcode = bc.rankcode, status = status });
                    }

                }

                stdate = sdate;
                enddate = edate;
                return View(quotalist.OrderByDescending(q => q.rankcode));
            }
        }
        public ActionResult PrintQuotaCollection()
        {
            List<QuotaCollectionList> quotalist = new List<QuotaCollectionList>();
            MonthName mn = new MonthName();
            var mr = db.Members.Single(m => m.Id == 1);
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var dalist = db.AgentDetails.Where(a => a.NewAgentId == User.Identity.Name).ToList();

            #region Select Broker Chain Start Here

            List<BrokerChain> bclist = new List<BrokerChain>();
            foreach (var dd in dalist)
            {
                var ad = db.AgentDetails.Single(a => a.AgencyCode == dd.AgencyCode);
                bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = ad.BranchCode });
            }

            #endregion

            foreach (var bc in bclist)
            {


                var bwr = db.AgentDetails.Single(bw => bw.NewAgentId == bc.newagentid);
                var rr = db.Ranktabs.Single(rw => rw.RankCode == bc.rankcode);
                var br = db.Branchtabs.Single(b => b.BranchCode == bwr.BranchCode);

                #region Select Sub Broker Chain Start Here

                List<BrokerChain> subbclist = new List<BrokerChain>();
                subbclist.Add(new BrokerChain { newagentid = bwr.NewAgentId, name = bwr.name, rankcode = bwr.RankCode, rankname = bwr.RankName, introducerid = bwr.NewIntroducerId, introname = bwr.IntroName, branchname = br.BranchName });
                int srcount = 0;
                List<agents> sdalist = new List<agents>();
                var salist = (from sal in db.AgentDetails where sal.NewIntroducerId == bwr.NewAgentId select new { sal.AgencyCode }).Distinct();
                foreach (var sa in salist)
                {
                    srcount = srcount + 1;
                    sdalist.Add(new agents { sr = srcount, agentcode = sa.AgencyCode });
                }

                var smaxsragent = sdalist.Count;
                var sminsragent = 1;
                while (sminsragent <= smaxsragent)
                {

                    var sda = sdalist.Where(sa => sa.sr == sminsragent);
                    foreach (var sd in sda.ToList())
                    {
                        var saalist = (from sal in db.AgentDetails where sal.IntroducerCode == sd.agentcode select new { sal.AgencyCode }).Distinct();
                        foreach (var saa in saalist)
                        {
                            srcount = srcount + 1;
                            sdalist.Add(new agents { sr = srcount, agentcode = saa.AgencyCode });

                        }

                    }
                    sminsragent = sminsragent + 1;
                    smaxsragent = sdalist.Count;
                }

                foreach (var sdd in sdalist)
                {
                    var sad = db.AgentDetails.Single(sa => sa.AgencyCode == sdd.agentcode);
                    subbclist.Add(new BrokerChain { newagentid = sad.NewAgentId, name = sad.name, rankcode = sad.RankCode, rankname = sad.RankName, introducerid = sad.NewIntroducerId, introname = sad.IntroName, branchname = br.BranchName });
                }

                #endregion

                List<InstallmentList> inslist = new List<InstallmentList>();
                Double btotalcoll = 0, bquotaamount = 0, bipptotalcoll = 0, bdpptotalcoll = 0, Difference = 0;
                var status = string.Empty;
                var prankcount = 0;
                foreach (var sbc in subbclist)
                {

                    var tbd = (from ap in db.appltabs where ap.newintroducerid == sbc.newagentid select ap.bondid);
                    var bondlist = (from yt in db.Installmenttabs where yt.paymentdate >= stdate && yt.paymentdate <= enddate && tbd.Contains(yt.bondid) select new { yt.newbondid }).Distinct().ToList();
                    foreach (var bond in bondlist)
                    {
                        var planlist = (from pl in db.Installmenttabs where pl.paymentdate >= stdate && pl.paymentdate <= enddate && pl.newbondid == bond.newbondid && pl.type != "Adjustment" && pl.year == 1 select pl).ToList();
                        foreach (var plan in planlist)
                        {
                            inslist.Add(new InstallmentList { newbondid = plan.newbondid, installmentno = plan.installmentno, mode = plan.mode, year = plan.year, term = plan.term_plan, planname = plan.planname, amount = plan.payamount });
                        }
                    }
                }

                var brocount = db.AgentDetails.Where(sc => sc.NewIntroducerId == bc.newagentid).Count();
                var pnlist = inslist.Select(i => new { i.planname, i.term }).Distinct();
                foreach (var pn in pnlist)
                {
                    var pc = db.Plans.Single(t => t.shortcutname == pn.planname);
                    var p = db.RateTabs.Single(i => i.projectname == pc.Planname && i.term == pn.term);
                    Double totalcoll = 0, quotaamount = 0;
                    totalcoll = inslist.Where(ftr => ftr.planname == pn.planname && ftr.term == pn.term).Sum(r => (Double)r.amount);
                    quotaamount = Math.Round(((totalcoll * p.quotaper) / 100), 2);
                    btotalcoll = btotalcoll + totalcoll;
                    bquotaamount = bquotaamount + quotaamount;
                }

                if (rr.TeamQuota > bquotaamount)
                {
                    Difference = rr.TeamQuota - bquotaamount;
                    status = "NA";
                }
                else
                {
                    Difference = 0;
                    status = "ACHIEVED";

                }
                if (btotalcoll > 0)
                {
                    quotalist.Add(new QuotaCollectionList { newagentid = bc.newagentid, name = bc.name, rankname = bc.rankname, newintroducerid = bwr.NewIntroducerId, IPPBusiness = bipptotalcoll, DPPBusiness = bdpptotalcoll, business = btotalcoll, quotaamount = bquotaamount, PromotionTarget = rr.TeamQuota, rankcount = brocount, prankcount = prankcount, Difference = Difference, sdate = stdate, edate = enddate, companyname = cr.CompanyName, branchname = br.BranchName, branch = mr.branchname, bond = mr.custname, plan = mr.planname, agent = mr.agentname, rankcode = bc.rankcode, status = status });
                }

            }

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports/Branch"), "QuotaCollection.rpt"));
            rd.SetDataSource(quotalist.OrderByDescending(q => q.rankcode));

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(stream, "application/pdf");
            }

            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
            finally
            {
                rd.Close();
                rd.Dispose();
            }


            return View();
        }

        [HttpGet]
        public ActionResult VoucherList()
        {
            List<Voucher> tb = new List<Voucher>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                return View(tb);
            }
        }
        [HttpPost]
        public ActionResult VoucherList(string year,int month = 0)
        {
            List<Voucher> tb = new List<Voucher>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                int yr = Convert.ToInt32(year);
                var bb = db.AgentDetails.Single(nb => nb.NewAgentId == User.Identity.Name);
                var dt = (from d in db.commission_tabs
                          where d.newagentid == bb.NewAgentId && d.date.Year==yr && (from m in db.ReleaseTabs where m.month == month && m.year == year select m.month)
                              .Contains(d.date.Month)
                          select d).ToList();
               // var bd = db.commission_tabs.Where(n => n.newagentid == bb.NewAgentId && n.date.Month == month && n.date.Year == year);
                int count = dt.Count();
                if (count > 0)
                {
                    foreach (var item in dt.ToList())
                    {
                        tb.Add(new Voucher { newagentid = User.Identity.Name, planname = item.planname, year = item.year, business = item.amount, tds = item.percentage, commission = item.commission, netamount = 0, branch = item.branchcode, date = item.date, newbondid = item.newbondid, name = item.bondname });
                    }
                }
                else
                {
                    Response.Write("<script>alert('Your Commission of this Month is not release ..')</script>");

                }
                
                return View(tb);

            }
        }
        public ActionResult PrintVoucher()
        {
            List<Voucher> tb = new List<Voucher>();
            var bb = db.AgentDetails.Single(nb => nb.NewAgentId == User.Identity.Name);
            var bd = db.commission_tabs.Where(n => n.newagentid == bb.NewAgentId && n.date.Month == vmonth && n.date.Year == vvyear);
            int count = bd.Count();
            if (count > 0)
            {

                foreach (var item in bd.ToList())
                {
                    tb.Add(new Voucher { newagentid = User.Identity.Name, planname = item.planname, year = item.year, business = item.amount, tds = item.percentage, commission = item.commission, netamount = 0, branch = item.branchcode, date = item.date, newbondid = item.newbondid, name = item.bondname });
                }

            }
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Voucher.rpt"));
            rd.SetDataSource(tb);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);

                return new FileStreamResult(stream, "application/pdf");
            }

            catch (Exception ex)
            {
                Response.Write("<script>alert('" + ex.Message + "')</script>");
            }
            finally
            {
                rd.Close();
                rd.Dispose();
            }


            return View();
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

        #region vishal vehicle request code

        [HttpGet]
        public ActionResult VehicleRequest()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {

            }
            return View();
        }
        //vehicle request done by advisor here
        [HttpPost]
        public ActionResult VehicleRequest(VehicleRequestDetail model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                var count = db.VehicleRequestDetails.Where(o => o.RequestToken == model.RequestToken).Count();
                if (count == 0)
                {
                    VehicleRequestDetail v = new VehicleRequestDetail();
                    v.RequestToken = model.RequestToken;
                    v.NoOfSeatRequired = model.NoOfSeatRequired;
                    v.AgentId = User.Identity.Name;
                    v.VisitDate = model.VisitDate;
                    v.ReturnDate = model.ReturnDate;
                    v.ApprovedDate = DateTime.Now;
                    v.RequestedDate = DateTime.Now.Date;
                    v.RequestTime = DateTime.Now.ToShortTimeString();
                    v.TravelDistance = model.TravelDistance;

                    v.ApprovTime = DateTime.Now.ToShortTimeString();
                    v.Status = 1;

                    db.VehicleRequestDetails.Add(v);
                    int a = db.SaveChanges();
                    if (a == 1)
                    {
                        ViewBag.msg = "Your Request Has Been Sent To Admin !!";
                    }
                    else
                    {
                        ViewBag.msg = "Aww Request Not Send...Try Again !!";

                    }
                }

            }


            return View();
        }
        //vehicle request history
        public ActionResult VehicleRequestHistory(int a = 0)
        {

            return View();
        }



        #endregion



    }
}


