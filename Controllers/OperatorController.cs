using ODHDEVELOPERS.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Text;
using System.Net.NetworkInformation;
using CrystalDecisions.CrystalReports.Engine;
using System.Data.Entity;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using System.Globalization;

namespace ODHDEVELOPERS.Controllers
{
    public class OperatorController : Controller
    {
        public OperatorController()
        {
            mrg = db.Members.Single(d => d.Id == 1);
            cg = db.CompanyInfos.Single(d => d.Id == 1);
        }

        #region Public Variable Start Here
        ReportDocument rd = new ReportDocument();
        SqlConnection con = new SqlConnection();
        UsersContext db = new UsersContext();

        public static string NewBondid, bond, acno, NewMemberid, BondType = string.Empty, typ;
        public static int paymentno = 0, vmonth = 0, vvyear = 0, scss = 0, ecss, lstsr = 0, ltsr = 0, sfilter = 0, PId;
        public static string ptype;
        public static string vnewagentid = string.Empty, transactionid = string.Empty, newaccountid = string.Empty;
        public static string NewAgentid;

        public static DateTime stdate = DateTime.Now.Date, enddate = DateTime.Now.Date, pdate = DateTime.Now.Date;
        public static DateTime ab = DateTime.Now.Date, b = DateTime.Now.Date;
        //private static DateTime start = Convert.ToDateTime("10/14/1774 12:00:00 AM");
        private static DateTime start = DateTime.ParseExact("10/14/1774 12:00:00 AM",
                                                    "MM/dd/yyyy hh:mm:ss tt",
                                                    System.Globalization.CultureInfo.InvariantCulture);

        //private static DateTime end = Convert.ToDateTime("10/14/1774 12:00:00 AM");
        private static DateTime end = DateTime.ParseExact("10/14/1774 12:00:00 AM",
                                                  "MM/dd/yyyy hh:mm:ss tt",
                                                  System.Globalization.CultureInfo.InvariantCulture);

        //private static DateTime edate1 = Convert.ToDateTime("10/14/1774 12:00:00 AM");
        //private static DateTime sdate1 = Convert.ToDateTime("10/14/1774 12:00:00 AM");

        private static DateTime edate1 = DateTime.ParseExact("10/14/1774 12:00:00 AM",
                                                     "MM/dd/yyyy hh:mm:ss tt",
                                                     System.Globalization.CultureInfo.InvariantCulture);

        private static DateTime sdate1 = DateTime.ParseExact("10/14/1774 12:00:00 AM",
                                                             "MM/dd/yyyy hh:mm:ss tt",
                                                             System.Globalization.CultureInfo.InvariantCulture);

        Member mrg = new Member();
        CompanyInfo cg = new CompanyInfo();
        #endregion

        #region Public Class Start Here

        public String GenerateImageTag(HttpPostedFileBase up)
        {
            String image = "";
            var imgtemp = Guid.NewGuid().ToString();
            if (up != null)
            {

                var chfl = true;
                while (chfl)
                {
                    image = "/images/original/" + imgtemp + ".jpg";
                    FileInfo fl = new FileInfo(Server.MapPath(image));
                    chfl = fl.Exists;
                    imgtemp = Guid.NewGuid().ToString();
                }
                Image im = Image.FromStream(up.InputStream);
                im.Save(Server.MapPath(image), ImageFormat.Jpeg);
            }
            return image;
        }
        public DateTime GetCleanDate(string date)
        {
            DateTime dt = new DateTime(1990, 1, 1);
            DateTime.TryParse(date, out dt);
            return dt;
        }
        public string gid()
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

        public void SaveVoucher(string newagentid, int month, int year, DateTime paymentdate)
        {
            UsersContext db = new UsersContext();
            MonthName mn = new MonthName();
            var monthname = mn.numbertomonthname(month) + "," + year;
            var tdsr = db.TDSLF_tabs.Single(t => t.Id == 1);
            var bonusamount = (from cts in db.bonus_tabs where cts.agentcode == newagentid && cts.month == month && cts.year == year select cts.bonusamount).DefaultIfEmpty(0).Sum();
            var advancepayment = (from cts in db.AdvDeductionVouchers where cts.newagentid == newagentid && cts.month == month && cts.year == year select cts.amount).DefaultIfEmpty(0).Sum();
            var mon = mn.numbertomonthname(month);

            List<plan> pclist = new List<plan>();
            List<require> rlist = new List<require>();
            var br = db.BrokerCommLists.Single(a => a.newagentid == newagentid && a.month == mon && a.Year == year);
            Double tdsper = 0;
            if (br.panno == null || br.panno == "Form-61")
            {
                tdsper = tdsr.NPCTDS;
            }
            else
            {
                tdsper = tdsr.TDS;
            }
            var voucherno = (from cts in db.Voucher_Reports where cts.agentid == br.agencycode select cts).Count();
            var vyear = year.ToString();

            var vcount = (from cts in db.Voucher_Reports where cts.agentid == br.agencycode && cts.month == month && cts.year == vyear select cts).Count();
            //------Inserting self voucher------------
            var spclist = (from ct in db.commission_tabs where ct.bondintroducerid == br.agencycode && ct.newagentid == newagentid && ct.date.Month == month && ct.date.Year == year orderby ct.plancode select new { ct.plancode }).Distinct();
            foreach (var p in spclist.ToList())
            {
                pclist.Add(new plan { plancode = p.plancode });
            }

            foreach (var pc in pclist)
            {

                var pr = db.NewPlans.Single(p => p.Plancode == pc.plancode);
                var spylist = (from sct in db.commission_tabs where sct.bondintroducerid == br.agencycode && sct.newagentid == newagentid && sct.date.Month == month && sct.date.Year == year && sct.plancode == pc.plancode orderby sct.year select new { sct.year }).Distinct();
                foreach (var spy in spylist.ToList())
                {

                    var business = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == br.agencycode && cts.newagentid == newagentid && cts.date.Month == month && cts.date.Year == year select cts.amount).DefaultIfEmpty(0).Sum();
                    var commission = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == br.agencycode && cts.newagentid == newagentid && cts.date.Month == month && cts.date.Year == year select cts.commission).DefaultIfEmpty(0).Sum();
                    var percentage = (commission / business) * 100;
                    rlist.Add(new require { agentid = br.agencycode, newagentid = newagentid, name = br.name, rankname = br.rankname, planname = pr.Planname, year = spy.year, business = business, percentage = percentage, commission = commission, tds = tdsper, bonusamount = bonusamount, advancepayment = advancepayment, brokerid = newagentid, brokername = br.name, brokerrank = br.rankname, newintroducerid = br.newintroducerid, month = monthname, voucherno = voucherno + 1 });

                }
            }

            //------End Inserting self voucher----------------

            //----------selecting team business and commission of selected month---------------

            var arlist = (from a in db.BrokerCommLists where a.newintroducerid == newagentid && a.month == mon && a.Year == year select a).ToList();
            List<agents> alist = new List<agents>();
            foreach (var aar in arlist)
            {
                alist.Add(new agents { agentcode = aar.agencycode });
            }

            foreach (var a in alist)
            {
                List<teamagents> tlist = new List<teamagents>();
                List<trequire> trlist = new List<trequire>();
                var nagentid = db.BrokerCommLists.Single(na => na.agencycode == a.agentcode && na.month == mon && na.Year == year);
                int rcount = 0;
                rcount = rcount + 1;
                tlist.Add(new teamagents { sr = rcount, tagentcode = a.agentcode });
                var maxsragent = tlist.Count;
                var minsragent = 1;
                while (minsragent <= maxsragent)
                {

                    var da = tlist.Where(t => t.sr == minsragent);
                    foreach (var d in da.ToList())
                    {
                        var aalist = (from al in db.BrokerCommLists where al.introducerid == d.tagentcode && al.month == mon && al.Year == year select new { al.agencycode }).Distinct();
                        foreach (var aa in aalist)
                        {
                            rcount = rcount + 1;
                            tlist.Add(new teamagents { sr = rcount, tagentcode = aa.agencycode });

                        }

                    }
                    minsragent = minsragent + 1;
                    maxsragent = tlist.Count;
                }

                foreach (var tt in tlist)
                {
                    var neaid = db.BrokerCommLists.Single(n => n.agencycode == tt.tagentcode && n.month == mon && n.Year == year);
                    var nctrlist = (from nn in db.commission_tabs where nn.bondintroducerid == tt.tagentcode && nn.newagentid == newagentid && nn.date.Month == month && nn.date.Year == year select nn).ToList();
                    foreach (var nctr in nctrlist)
                    {
                        trlist.Add(new trequire { newagentid = neaid.newagentid, plancode = nctr.plancode, year = nctr.year, business = nctr.amount, percentage = nctr.percentage, commission = nctr.commission });
                    }

                }

                var tnrname = db.BrokerCommLists.Single(t => t.newagentid == nagentid.newagentid && t.month == mon && t.Year == year);
                var dpclist = trlist.Select(tr => new { tr.plancode }).Distinct();
                foreach (var dpc in dpclist)
                {

                    var ppr = db.NewPlans.Single(pp => pp.Plancode == dpc.plancode);
                    var dpylist = trlist.Select(tr => new { tr.year }).Distinct();
                    foreach (var dpy in dpylist)
                    {
                        var tbusiness = trlist.Where(tr => tr.plancode == dpc.plancode && tr.year == dpy.year).Sum(tr => tr.business);
                        var tcommission = trlist.Where(tr => tr.plancode == dpc.plancode && tr.year == dpy.year).Sum(tr => tr.commission);
                        var tpercentage = (tcommission / tbusiness) * 100;
                        rlist.Add(new require { agentid = nagentid.agencycode, newagentid = nagentid.newagentid, name = tnrname.name, rankname = tnrname.rankname, planname = ppr.Planname, year = dpy.year, business = tbusiness, percentage = tpercentage, commission = tcommission, tds = tdsper, bonusamount = bonusamount, advancepayment = advancepayment, brokerid = newagentid, brokername = br.name, brokerrank = br.rankname, newintroducerid = br.newintroducerid, month = monthname, voucherno = voucherno + 1 });
                    }
                }

            }


            //----------End selecting team business and commission of selected month---------------

            var gbusiness = rlist.Sum(tr => tr.business);
            var gcommission = rlist.Sum(tr => tr.commission);
            var lesstds = Math.Round((gcommission * tdsper) / 100, 2);
            var netamount = Math.Round((gcommission + bonusamount - lesstds - advancepayment), 2);


            DateTime sdt, edt;
            string sdate = string.Empty;
            //----------End selecting team business and commission of selected month---------------
            if (month < 10)
            {
                sdate = "0" + month.ToString() + "/01/" + year.ToString();
            }
            else if (month >= 10)
            {
                sdate = month.ToString() + "/01/" + year.ToString();
            }
            sdt = Convert.ToDateTime(sdate);
            edt = sdt.AddMonths(1);

            var bb = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
            var mr = db.Members.Single(m => m.Id == 1);
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            Voucher_Report vob = new Voucher_Report();
            vob.agentid = br.agencycode;
            vob.bussiness = gbusiness;
            vob.commission = gcommission;
            vob.tds = lesstds;
            vob.netamount = netamount;
            vob.sdate = sdt.Date;
            vob.edate = edt.Date;
            vob.date = paymentdate;
            vob.month = month;
            vob.year = year.ToString();
            vob.branchcode = bb.BranchCode;
            vob.Operatorid = User.Identity.Name;

            db.Voucher_Reports.Add(vob);
            db.SaveChanges();

            Double credit = (from aa in db.Wallet_Transactions where aa.agentid == newagentid select aa.Credit).DefaultIfEmpty(0).Sum();
            Double debit = (from aa in db.Wallet_Transactions where aa.agentid == newagentid select aa.Debit).DefaultIfEmpty(0).Sum();
            Double total = credit - debit;

            var bn = db.Operators.Single(c => c.OperatorId == User.Identity.Name);

            var apr = db.AppSettingTabs.Single(a => a.status == 1);
            if (apr.status == 1)
            {
                WalletBalance objWB = new WalletBalance();
                objWB.username = br.newagentid;
                objWB.mobile = br.mobile;
                objWB.customer_name = br.name;
                objWB.transactionid = db.Database.SqlQuery<string>("exec AutoGenerateWalletId").FirstOrDefault();
                objWB.transactiondate = DateTime.Now;
                objWB.amount = netamount;
                objWB.type = "Credit";
                objWB.referenceno = objWB.transactionid;
                objWB.txn_status = 1;
                objWB.trxnmessage = "Rs. " + netamount + " /- will be credited in your wallet";
                db.WalletBalances.Add(objWB);
                db.SaveChanges();
            }

            MyClass.Sendmsg(br.mobile, "Dear " + br.name + ", Your Voucher for " + monthname + " has been paid of amount:" + netamount + " on dated:" + DateTime.Now.ToString("dd/MM/yyyy") + " by Operator " + bb.OperatorName);

        }
        public void BWSaveVoucher(string newagentid, int month, int year, DateTime paymentdate)
        {

            MonthName mn = new MonthName();
            var monthname = mn.numbertomonthname(month) + "," + year;

            List<BondwiseVoucherlist> bwvlist = new List<BondwiseVoucherlist>();
            List<AgentDetail> blist = new List<AgentDetail>();
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var mr = db.Members.Single(m => m.Id == 1);
            var pr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
            var bb = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);
            var br = db.AgentDetails.Single(a => a.NewAgentId == newagentid);
            var tdsr = db.TDSLF_tabs.Single(t => t.Id == 1);
            Double tdsper = 0;
            if (br.Panno == null || br.Panno == "")
            {
                tdsper = tdsr.NPCTDS;
            }
            else
            {
                tdsper = tdsr.TDS;
            }
            var bonusamount = (from cts in db.bonus_tabs where cts.agentcode == newagentid && cts.month == month && cts.year == year select cts.bonusamount).DefaultIfEmpty(0).Sum();
            ViewData["bonusamount"] = bonusamount;

            var voucherno = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode select cts).Count();
            var vyear = year.ToString();
            var vcount = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode && cts.month == month && cts.year == vyear select cts).Count();
            var commlist = (from co in db.commission_tabs where co.newagentid == newagentid && co.date.Month == month && co.date.Year == year select co).ToList();
            foreach (var com in commlist)
            {
                var appr = db.appltabs.Single(b => b.newbondid == com.newbondid);
                bwvlist.Add(new BondwiseVoucherlist { newbondid = com.newbondid, bondname = com.bondname, bondintroducerid = appr.newintroducerid, planname = appr.planname, mode = appr.mode, percentage = com.percentage, business = com.amount, commission = com.commission, newrenew = com.newrenew, tds = tdsper, bonusamount = bonusamount, brokerid = newagentid, brokername = br.name, brokerrank = br.RankName, newintroducerid = br.NewIntroducerId, month = monthname, voucherno = voucherno, branchname = bb.BranchName, companyname = cr.CompanyName, branch = mr.branchname, agent = mr.agentname, bond = mr.custname, plan = mr.planname });
            }

            var gbusiness = bwvlist.Sum(tr => tr.business);
            var gcommission = bwvlist.Sum(tr => tr.commission);
            var lesstds = Math.Round((gcommission * tdsper) / 100, 2);
            var netamount = Math.Round((gcommission + bonusamount - lesstds), 2);

            DateTime sdt, edt;
            string sdate = string.Empty;
            //----------End selecting team business and commission of selected month---------------
            if (month < 10)
            {
                sdate = "0" + month.ToString() + "/01/" + year.ToString();
            }
            else if (month >= 10)
            {
                sdate = month.ToString() + "/01/" + year.ToString();
            }
            sdt = Convert.ToDateTime(sdate);
            edt = sdt.AddMonths(1);


            Voucher_Report vob = new Voucher_Report();
            vob.agentid = br.AgencyCode;
            vob.bussiness = gbusiness;
            vob.commission = gcommission;
            vob.tds = lesstds;
            vob.netamount = netamount;
            vob.sdate = sdt.Date;
            vob.edate = edt.Date;
            vob.date = paymentdate;
            vob.month = month;
            vob.year = year.ToString();
            vob.branchcode = pr.BranchCode;
            vob.Operatorid = User.Identity.Name;

            db.Voucher_Reports.Add(vob);
            db.SaveChanges();

            Double credit = (from aa in db.Wallet_Transactions where aa.agentid == newagentid select aa.Credit).DefaultIfEmpty(0).Sum();
            Double debit = (from aa in db.Wallet_Transactions where aa.agentid == newagentid select aa.Debit).DefaultIfEmpty(0).Sum();
            Double total = credit - debit;

            var bn = db.Operators.Single(c => c.OperatorId == User.Identity.Name);

            var apr = db.AppSettingTabs.Single(a => a.status == 1);
            if (apr.status == 1)
            {
                WalletBalance objWB = new WalletBalance();
                objWB.username = br.NewAgentId;
                objWB.mobile = br.Mobile;
                objWB.customer_name = br.name;
                objWB.transactionid = db.Database.SqlQuery<string>("exec AutoGenerateWalletId").FirstOrDefault();
                objWB.transactiondate = DateTime.Now;
                objWB.amount = netamount;
                objWB.type = "Credit";
                objWB.referenceno = objWB.transactionid;
                objWB.txn_status = 1;
                objWB.trxnmessage = "Rs. " + netamount + " /- will be credited in your wallet";
                db.WalletBalances.Add(objWB);
                db.SaveChanges();
            }

            MyClass.Sendmsg(br.Mobile, "Dear " + br.name + ", Your Voucher for " + monthname + " has been paid of amount:" + netamount + " on dated:" + DateTime.Now.ToString("dd/MM/yyyy") + " by Operator " + pr.OperatorName);

        }
        public void CCSaveVoucher(string newagentid, int month, int year, DateTime paymentdate)
        {

            MonthName mn = new MonthName();
            var monthname = mn.numbertomonthname(month) + "," + year;

            #region Selecting Complete Chain Start Here

            int rcount = 0;
            List<BrokerChain> bclist = new List<BrokerChain>();
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var mr = db.Members.Single(m => m.Id == 1);
            var br = db.AgentDetails.Single(a => a.NewAgentId == newagentid);
            var opr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
            var bb = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
            var tdsr = db.TDSLF_tabs.Single(t => t.Id == 1);
            Double tdsper = 0;
            if (br.Panno == null || br.Panno == "")
            {
                tdsper = tdsr.NPCTDS;
            }
            else
            {
                tdsper = tdsr.TDS;
            }
            bclist.Add(new BrokerChain { newagentid = br.NewAgentId, name = br.name, rankcode = br.RankCode, rankname = br.RankName, introducerid = br.NewIntroducerId, introname = br.IntroName, branchname = bb.BranchName });
            List<agents> dalist = new List<agents>();
            var alist = (from al in db.AgentDetails where al.NewIntroducerId == newagentid select new { al.AgencyCode }).Distinct();
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
                bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = bb.BranchName });

            }

            #endregion

            #region Selecting CC Voucher Start Here

            List<require> rlist = new List<require>();
            var bonusamount = (from cts in db.bonus_tabs where cts.agentcode == newagentid && cts.month == month && cts.year == year select cts.bonusamount).DefaultIfEmpty(0).Sum();
            ViewData["bonusamount"] = bonusamount;
            var voucherno = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode select cts).Count();
            var vyear = year.ToString();
            var vcount = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode && cts.month == month && cts.year == vyear select cts).Count();

            foreach (var bc in bclist)
            {

                var ar = db.AgentDetails.Single(a => a.NewAgentId == bc.newagentid);
                List<plan> pclist = new List<plan>();

                //------Inserting self voucher------------
                var spclist = (from ct in db.commission_tabs where ct.bondintroducerid == ar.AgencyCode && ct.newagentid == br.NewAgentId && ct.date.Month == month && ct.date.Year == year orderby ct.plancode select new { ct.plancode }).Distinct();
                foreach (var p in spclist.ToList())
                {
                    pclist.Add(new plan { plancode = p.plancode });
                }

                foreach (var pc in pclist)
                {

                    var pr = db.NewPlans.Single(p => p.Plancode == pc.plancode);
                    var spylist = (from sct in db.commission_tabs where sct.bondintroducerid == ar.AgencyCode && sct.newagentid == br.NewAgentId && sct.date.Month == month && sct.date.Year == year && sct.plancode == pc.plancode orderby sct.year select new { sct.year }).Distinct();
                    foreach (var spy in spylist.ToList())
                    {

                        var business = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == ar.AgencyCode && cts.newagentid == br.NewAgentId && cts.date.Month == month && cts.date.Year == year select cts.amount).DefaultIfEmpty(0).Sum();
                        var commission = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == ar.AgencyCode && cts.newagentid == br.NewAgentId && cts.date.Month == month && cts.date.Year == year select cts.commission).DefaultIfEmpty(0).Sum();
                        var percentage = (commission / business) * 100;
                        rlist.Add(new require { agentid = ar.AgencyCode, newagentid = ar.NewAgentId, name = ar.name, rankname = ar.RankName, planname = pr.Planname, year = spy.year, business = business, percentage = percentage, commission = commission, tds = tdsper, bonusamount = bonusamount, brokerid = br.NewAgentId, brokername = br.name, brokerrank = br.RankName, newintroducerid = br.NewIntroducerId, month = monthname, voucherno = voucherno + 1 });

                    }
                }

                //------End Inserting self voucher----------------

            }
            var gbusiness = rlist.Sum(tr => tr.business);
            var gcommission = rlist.Sum(tr => tr.commission);
            var lesstds = Math.Round((gcommission * tdsper) / 100, 2);
            var netamount = Math.Round((gcommission + bonusamount - lesstds), 2);


            #endregion

            DateTime sdt, edt;
            string sdate = string.Empty;
            //----------End selecting team business and commission of selected month---------------
            if (month < 10)
            {
                sdate = "0" + month.ToString() + "/01/" + year.ToString();
            }
            else if (month >= 10)
            {
                sdate = month.ToString() + "/01/" + year.ToString();
            }
            sdt = Convert.ToDateTime(sdate);
            edt = sdt.AddMonths(1);


            Voucher_Report vob = new Voucher_Report();
            vob.agentid = br.AgencyCode;
            vob.bussiness = gbusiness;
            vob.commission = gcommission;
            vob.tds = lesstds;
            vob.netamount = netamount;
            vob.sdate = sdt.Date;
            vob.edate = edt.Date;
            vob.date = paymentdate;
            vob.month = month;
            vob.year = year.ToString();
            vob.branchcode = opr.BranchCode;
            vob.Operatorid = User.Identity.Name;

            db.Voucher_Reports.Add(vob);
            db.SaveChanges();
            Double credit = (from aa in db.Wallet_Transactions where aa.agentid == newagentid select aa.Credit).DefaultIfEmpty(0).Sum();
            Double debit = (from aa in db.Wallet_Transactions where aa.agentid == newagentid select aa.Debit).DefaultIfEmpty(0).Sum();
            Double total = credit - debit;

            var bn = db.Operators.Single(c => c.OperatorId == User.Identity.Name);

            var apr = db.AppSettingTabs.Single(a => a.status == 1);
            if (apr.status == 1)
            {
                WalletBalance objWB = new WalletBalance();
                objWB.username = br.NewAgentId;
                objWB.mobile = br.Mobile;
                objWB.customer_name = br.name;
                objWB.transactionid = db.Database.SqlQuery<string>("exec AutoGenerateWalletId").FirstOrDefault();
                objWB.transactiondate = DateTime.Now;
                objWB.amount = netamount;
                objWB.type = "Credit";
                objWB.referenceno = objWB.transactionid;
                objWB.txn_status = 1;
                objWB.trxnmessage = "Rs. " + netamount + " /- will be credited in your wallet";
                db.WalletBalances.Add(objWB);
                db.SaveChanges();
            }

            MyClass.Sendmsg(br.Mobile, "Dear " + br.name + ", Your Voucher for " + monthname + " has been paid of amount:" + netamount + " on dated:" + DateTime.Now.ToString("dd/MM/yyyy") + " by Operator " + opr.OperatorName);

        }

        #endregion

        #region Json Method Start Here
        public JsonResult AutoCompletetempBondId(string term)
        {
            var mlist = db.tempappltabs.Where(g => g.plantype != "FI" && g.opid == User.Identity.Name).ToList();
            var list = (from r in mlist where r.newbondid.ToLower().Contains(term.ToLower()) || r.name.ToLower().Contains(term.ToLower()) select new { r.newbondid, r.name }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteAccountno(string term)
        {
            var list = (from r in db.SavingAccountInfos where r.accountno.ToLower().Contains(term.ToLower()) || r.acholdername.ToLower().Contains(term.ToLower()) select new { r.accountno, r.acholdername }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AutoCompleteAgentid(string term)
        {
            var list = (from r in db.AgentDetails where r.NewAgentId.ToLower().Contains(term.ToLower()) || r.name.ToLower().Contains(term.ToLower()) select new { r.NewAgentId, r.name }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteBondid(string term)
        {
            var list = (from r in db.appltabs where r.newbondid.ToLower().Contains(term.ToLower()) || r.name.ToLower().Contains(term.ToLower()) select new { r.newbondid, r.name }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult AutoCompleteRank(string term)
        {

            var list = (from r in db.Ranktabs where r.RankName.ToLower().Contains(term.ToLower()) select new { r.RankCode, r.RankName }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SelectIntroName(string intro)
        {
            var list = (from o in db.AgentDetails where o.NewAgentId == intro select new { o.name, o.Father });
            return Json(list, JsonRequestBehavior.AllowGet);

        }

        public JsonResult AutoCompleteEid(string term)
        {

            var list = (from r in db.Emp_Regs where r.empid.ToLower().Contains(term.ToLower()) || r.name.ToLower().Contains(term.ToLower()) select new { r.empid, r.name }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult selecttype(string type)
        {
            if (type == "Broker")
            {
                type = "Agent";
            }
            List<NewLogin> ct = new List<NewLogin>();
            ct = (from dcl in db.NewLogins where dcl.type == type orderby dcl.UserName select dcl).ToList();
            return Json(ct, JsonRequestBehavior.AllowGet);
        }
        public JsonResult selectcssno(DateTime date)
        {

            var cssnolist = (from ins in db.Installmenttabs where ins.paymentdate == date && ins.opid == User.Identity.Name orderby ins.cssno select new { ins.cssno }).Distinct();
            return Json(cssnolist, JsonRequestBehavior.AllowGet);
        }

        public JsonResult selectinstallno(string newbondid)
        {

            var cssnolist = (from ins in db.Installmenttabs where ins.newbondid == newbondid orderby ins.installmentno select new { ins.installmentno }).Distinct();
            return Json(cssnolist, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompletememberid(string term)
        {
            var list = (from r in db.Member_tabs where r.NewMemberId.ToLower().Contains(term.ToLower()) || r.MemberName.ToLower().Contains(term.ToLower()) select new { r.NewMemberId, r.MemberName }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult AutoCompleteBWMemberid(string term)
        {
            var br = db.Operators.Single(ci => ci.OperatorId == User.Identity.Name);
            var mlist = db.Member_tabs.Where(m => m.BranchCode == br.BranchCode).ToList();
            var list = (from r in mlist where r.NewMemberId.ToLower().Contains(term.ToLower()) || r.MemberName.ToLower().Contains(term.ToLower()) select new { r.NewMemberId, r.MemberName }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AutoCompleteOperatorAgentid(string term)
        {
            var br = db.Operators.Single(ci => ci.OperatorId == User.Identity.Name);
            var mlist = db.AgentDetails.Where(m => m.BranchCode == br.BranchCode).ToList();
            var list = (from r in mlist where r.NewAgentId.ToLower().Contains(term.ToLower()) || r.name.ToLower().Contains(term.ToLower()) select new { r.NewAgentId, r.name }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AutoCompleteBWBondid(string term)
        {
            var br = db.Operators.Single(ci => ci.OperatorId == User.Identity.Name);
            var mlist = db.appltabs.Where(m => m.branchcode == br.BranchCode).ToList();
            var list = (from r in mlist where r.newbondid.ToLower().Contains(term.ToLower()) || r.name.ToLower().Contains(term.ToLower()) select new { r.newbondid, r.name }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult AutoCompletePPSBondId(string term)
        {
            var p4 = db.Plans.Single(c => c.Plancode == 4);
            var br = db.Operators.Single(ci => ci.OperatorId == User.Identity.Name);
            var plist = (from b in db.appltabs where b.plantype == p4.shortcutname && b.branchcode == br.BranchCode select b);
            var list = (from r in plist where r.newbondid.ToLower().Contains(term.ToLower()) || r.name.ToLower().Contains(term.ToLower()) select new { r.newbondid, r.name }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Menu Start Here
        [HttpGet]
        public ActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                ViewData["msg"] = TempData["msg"];
                var aa = ViewData["msg"];
                if (ViewData["msg"] != null)
                {
                    Response.Write("<script>alert('" + aa + "')</script>");
                }
                return View();
            }
        }
        [HttpGet]
        public ActionResult Bond()
        {
            List<Member_tab> mt = new List<Member_tab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                DateTime maxdate1 = (from a in db.DailyDepositTabs where a.opid == User.Identity.Name select a.date).DefaultIfEmpty().Max();
                int trid = (from a in db.DailyDepositTabs where a.opid == User.Identity.Name && a.date == maxdate1 select a.trid).DefaultIfEmpty().Max();
                DateTime maxdate = (from a in db.DailyDepositTabs where a.opid == User.Identity.Name && a.trid == trid select a.date).DefaultIfEmpty().Max();

                var clcount = (from c in db.ClosingMatchingTabs where c.date == maxdate && c.opid == User.Identity.Name && c.pagename == "Bond" select c).Count();
                int dcount = (from a in db.DailyDepositTabs where a.opid == User.Identity.Name && a.trid == trid && a.date == maxdate select a).Count();
                if (dcount > 0)
                {
                    if (clcount == 0)
                    {
                        TempData["pagename"] = "Bond";
                        return RedirectToAction("MatchingDailyClosing", "Operator");
                    }
                }
                return View(mt);
            }
        }
        [HttpPost]
        public ActionResult Bond(appltab bd, tempappltab tbd, HttpPostedFileBase photo, HttpPostedFileBase Chequeimage, HttpPostedFileBase spanphoto, HttpPostedFileBase sphoto, HttpPostedFileBase saadharphoto, string command, string memberid, string trbank, string transactionid, string transactiondate, string tramount, string trpmethod, string holderacno, string sdob = "", string dob = "", string sanniversarydate = "")
        {
            List<Member_tab> mt = new List<Member_tab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                switch (command)
                {
                    case "Search":

                        switch (memberid)
                        {
                            case "":
                                Response.Write("<script>alert('Please Select Contributor Id First')</script>");
                                break;
                            default:

                                mt = db.Member_tabs.Where(c => c.NewMemberId == memberid).ToList();
                                return View(mt);
                        }
                        break;

                    case "Submit":
                        var ds = db.Blockdates.Where(c => c.date == bd.formdate && c.branchcode == User.Identity.Name && c.status == 0).Count();
                        if (ds > 0)
                        {
                            Response.Write("<script>alert('This Date is closed please select another date')</script>");
                        }
                        else
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
                            if (bd.mode == "PART" && bd.term == 0)
                            {
                                bd.term = 1;
                            }
                            var phr = db.PhaseTabs.FirstOrDefault(c => c.Id == bd.phaseid);
                            var pt = db.Plans.Single(c => c.Plancode == bd.projectid);
                            var mr = db.Members.Single(m => m.Id == 1);
                            var br = db.Branchtabs.Single(bc => bc.BranchCode == bd.branchcode);
                            var cr = db.CompanyInfos.Single(csn => csn.AdminId == br.companyid);
                            int max = (from a in db.appltabs select a).Count();
                            var bondid = max + 1;
                            string dd = DateTime.Now.Date.Month.ToString("00");

                            int yr = DateTime.Now.Date.Year;
                            var newbondid = br.prefix + bd.branchcode + "/C0" + bondid;
                            //var newbondid = br.prefix + bd.branchcode + "/C0" + dd + bondid.ToString().Substring(0, 2) + DateTime.Now.Date.Day.ToString("00") + bondid.ToString().Substring(2) + yr.ToString().Substring(2);
                            var adr = db.AgentDetails.Single(ad => ad.NewAgentId == bd.newintroducerid);

                            ProTerm pr = new ProTerm();
                            if (bd.paymenttype != "EMI")
                            {
                                pr = db.ProTerms.FirstOrDefault();
                            }
                            else
                            {
                                pr = db.ProTerms.Single(p => p.projectid == bd.projectid && p.term == bd.term);
                            }
                            if (bd.paymenttype == "FULL") { bd.bookingamount = bd.totalcon; }
                            TimeSpan span = bd.formdate.Subtract(bd.dob);
                            int years = (int)(span.Days / 365.25);
                            int remainder = (int)(span.Days % 365.25);
                            int months = remainder / 30;
                            if (months >= 12)
                            {
                                int y = months / 12;
                                int m = months % 12;
                                months = m;
                                years = years + y;
                            }
                            int days = remainder % 30;
                            var age = years + " years " + months + " months " + days + " days";

                            var row = db.Member_tabs.Single(ui => ui.NewMemberId == bd.newmemberid);

                            #region APPLTAB
                            if (bd.paymethod != "Cheque" && bd.paymethod != "banktransaction" && bd.discountper == 0)
                            {

                                bd.bondid = bondid;//
                                bd.newbondid = newbondid;//

                                bd.introducerid = adr.AgencyCode;//
                                bd.intrankcode = adr.RankCode;//
                                bd.intrankname = adr.RankName;//

                                bd.age = age;//

                                bd.memberid = row.MemberId;//


                                if (photo != null)
                                {
                                    string imgname = gid();
                                    bd.photo = "~/Photo/" + imgname + ".jpg";
                                    photo.SaveAs(HttpContext.Server.MapPath("~/Photo/" + imgname + ".jpg"));
                                }
                                else
                                {
                                    bd.photo = "~/Photo/default.jpg";

                                }

                                bd.plantype = "RD";
                                bd.planname = pt.shortcutname;//
                                bd.plancode = pr.Id;//
                                bd.bonusper = 0;//


                               // bd.pdate = bd.formdate;
                                bd.opid = User.Identity.Name;//
                                bd.status = 1;//

                                bd.type = "Customer";//
                                bd.Macaddress = localIP;//

                                bd.downpayment = 0;//
                                bd.Time = DateTime.Now.ToShortTimeString();//

                                bd.printstatus = 0;//
                                bd.phaseid = bd.phaseid;
                                bd.phase = phr == null ? "NA" : phr.Phase;

                                bd.Chequedate = bd.pdate;
                                bd.transactiontype = "NA";//
                                if (Chequeimage != null)
                                {
                                    string imgname = gid();
                                    bd.Chequeimage = "~/Photo/" + imgname + ".jpg";
                                    Chequeimage.SaveAs(HttpContext.Server.MapPath("~/Photo/" + imgname + ".jpg"));
                                }
                                else
                                {
                                    bd.Chequeimage = "~/Photo/default.jpg";
                                }
                                bd.chequeappdate = bd.pdate;

                                bd.Aadhaar_No = "N/A";//
                                bd.Aadhaar_ReqDate = Convert.ToDateTime("1991-01-01");//
                                bd.Aadhaar_status = 0;//
                                bd.Aadhaar_AppDate = Convert.ToDateTime("1991-01-01");//
                                bd.PAN_AppDate = Convert.ToDateTime("1991-01-01");//
                                bd.PAN_ReqDate = Convert.ToDateTime("1991-01-01");//
                                bd.PanStatus = 0;//
                                if (sphoto != null)
                                {
                                    bd.sphoto = GenerateImageTag(sphoto);
                                }
                                if (spanphoto != null)
                                {
                                    bd.spanphoto = GenerateImageTag(spanphoto);
                                }
                                if (saadharphoto != null)
                                {
                                    bd.saadharphoto = GenerateImageTag(saadharphoto);
                                }
                                //date section

                                bd.dob = GetCleanDate(dob);
                                // bd.sanniversarydate = GetCleanDate(sanniversarydate);
                                // bd.sdob = GetCleanDate(sdob);
                                //
                                int propertyid = 0, count = 0; string loanid = ""; Int64 om = 0;
                                if (bd.mobileno != null && Int64.TryParse(bd.mobileno, out om))
                                {
                                    propertyid = Convert.ToInt32(bd.mobileno.Substring(bd.mobileno.Length - 4));
                                    count = db.appltabs.Count(d => d.propertyid == propertyid);
                                    while (count > 0)
                                    {
                                        propertyid = Convert.ToInt32(gid().Substring(0, 4));
                                        count = db.appltabs.Count(d => d.propertyid == propertyid);
                                    }
                                    if (bd.paymenttype == "EMI")
                                    {
                                        loanid = bd.mobileno + "0";
                                        count = db.appltabs.Count(d => d.loanid == loanid);
                                        while (count > 0)
                                        {
                                            loanid = gid() + "A";
                                            count = db.appltabs.Count(d => d.propertyid == propertyid);
                                        }
                                    }
                                }
                                else
                                {
                                    propertyid = Convert.ToInt32(bd.mobileno.Substring(bd.mobileno.Length - 4));
                                    count = db.appltabs.Count(d => d.propertyid == propertyid);
                                    while (count > 0)
                                    {
                                        propertyid = Convert.ToInt32(gid().Substring(0, 4));
                                        count = db.appltabs.Count(d => d.propertyid == propertyid);
                                    }
                                    if (bd.paymenttype == "EMI")
                                    {
                                        loanid = loanid = gid() + "A";

                                    }
                                }
                                //set propertyid
                                bd.propertyid = propertyid;
                                bd.loanid = loanid;
                                /*---------------------------------ref no----------------**/
                                //count = 0;
                                //count = db.appltabs.Count();
                                //string csr = "";
                                //if (count == 0) { csr = "001"; }
                                //else
                                //{
                                //    count = count + 1;
                                //    if (count < 10)
                                //    {
                                //        csr = "00" + count;
                                //    }
                                //    else if (count >= 10 && count < 100)
                                //    {
                                //        csr = "0" + count;
                                //    }
                                //    else if (count >= 100)
                                //    {
                                //        csr = count.ToString();
                                //    }
                                //}
                                var csr = gidref();

                                int year = DateTime.Now.Year, syear = year + 1;
                                string refyear = year.ToString().Substring(2, 2) + "-" + syear.ToString().Substring(2, 2);
                                string refno = "ODH HOMES/" + refyear + "/" + csr;
                                //                                
                                bd.refno = refno;

                                db.appltabs.Add(bd);
                                db.SaveChanges();

                                RevivalTab rt = new RevivalTab();
                                rt.newbondid = newbondid;
                                rt.status = 1;
                                db.RevivalTabs.Add(rt);
                                db.SaveChanges();

                                NewLogin nl = new NewLogin();
                                nl.UserName = newbondid;
                                nl.Password = bd.mobileno;
                                nl.Mobile = bd.mobileno;
                                nl.type = "Customer";
                                nl.status = 1;
                                db.NewLogins.Add(nl);
                                db.SaveChanges();

                                con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                                SqlCommand cmd = new SqlCommand();
                                cmd.CommandText = "InsertAppTab";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = con;

                                cmd.Parameters.AddWithValue("@introducerid", adr.AgencyCode);
                                cmd.Parameters.AddWithValue("@plancode", pr.Id);
                                cmd.Parameters.AddWithValue("@planname", pt.shortcutname);
                                cmd.Parameters.AddWithValue("@payment", bd.payment);
                                cmd.Parameters.AddWithValue("@bondid", bondid);
                                cmd.Parameters.AddWithValue("@mode", bd.mode);
                                cmd.Parameters.AddWithValue("@formdate", bd.formdate);
                                cmd.Parameters.AddWithValue("@pdate", bd.pdate);
                                cmd.Parameters.AddWithValue("@term", bd.term);
                                cmd.Parameters.AddWithValue("@opid", User.Identity.Name);


                                try
                                {
                                    con.Open();
                                    cmd.ExecuteNonQuery();
                                    //MyClass.Sendmsg(bd.mobileno, "Dear " + bd.name + ", your Loginid:" + newbondid + " Password:" + bd.mobileno + " For your A/c details please visit "+cg.HeadOffice);
                                    //MyClass.Sendmsg(bd.mobileno, "Dear Customer your Application no- " + newbondid + " has been activated with Rs. " + bd.bookingamount + " in the plan " + pt.Planname + "Thank you for your kind believe us for help contact. "+cg.CompanyName);
                                    if (bd.PYN == "y" || bd.PYN == "Y")
                                    {
                                        MyClass.Sendmsg(bd.mobileno, "Dear " + bd.name + ", Thank you for purchasing a plot in " + pr.projectname + ". Your Plot No. " + bd.plotno + "," + bd.block + " ," + bd.phase + " at " + pr.projectname + " is Allotted Successfully. Regards- " + cr.CompanyName);

                                    }
                                    else
                                    {
                                        MyClass.Sendmsg(bd.mobileno, "Dear " + bd.name + ", Thank you for purchasing a plot in " + pr.projectname + ". Regards- " + cr.CompanyName);

                                    }
                                    MyClass.Sendmsg(bd.mobileno, "Dear " + bd.name + ", login on " + cr.HeadOffice + " with your user ID as " + nl.UserName + " and password is " + nl.Password + ". Regards- " + cr.CompanyName);

                                    Response.Write("<script>alert('" + mr.custname + " created Successfully....')</script>");
                                    ViewBag.msg = mr.custname + " created Successfully....";
                                    NewBondid = newbondid;
                                    ViewData["bondtype"] = "Normal";
                                    ViewData["newbondid"] = newbondid;
                                    ViewData["discount"] = bd.discountper;
                                    BondType = "Normal";

                                }

                                catch (Exception e)
                                {
                                    ViewBag.msg = e.Message;
                                }
                                finally
                                {
                                    con.Close();
                                }
                            }
                            #endregion

                            #region tempappltab
                            else
                            {
                                if (tbd.mode == "PART" && tbd.term == 0)
                                {
                                    tbd.term = 1;
                                }
                                int bid = (from a in db.tempappltabs select a.bondid).DefaultIfEmpty(0).Max();
                                int bond = bid + 1;
                                tempappltab td = new tempappltab();
                                td.branchcode = tbd.branchcode;
                                td.bondid = bond;
                                td.newbondid = bond.ToString();
                                td.name = tbd.name;
                                td.RelationOf = tbd.RelationOf;
                                td.fathername = tbd.fathername;
                                td.addr = tbd.addr;
                                td.mobileno = tbd.mobileno;
                                td.introducerid = adr.AgencyCode;
                                td.intrankcode = adr.RankCode;
                                td.intrankname = adr.RankName;
                                td.newintroducerid = bd.newintroducerid;
                                td.formdate = Convert.ToDateTime(bd.formdate);
                                td.dob = bd.dob;
                                td.age = age;
                                td.nationality = tbd.nationality;
                                td.newmemberid = tbd.newmemberid;
                                td.memberid = row.MemberId;
                                td.guardianname = tbd.guardianname;
                                td.gurage = tbd.gurage;
                                td.gurrel = tbd.gurrel;
                                td.guraddr = tbd.guraddr;


                                if (photo != null)
                                {
                                    string imgname = gid();
                                    td.photo = "~/Photo/" + imgname + ".jpg";
                                    photo.SaveAs(HttpContext.Server.MapPath("~/Photo/" + imgname + ".jpg"));
                                }
                                else
                                {
                                    td.photo = "~/Photo/default.jpg";

                                }

                                td.panno = tbd.panno;
                                td.bankname = tbd.bankname;
                                td.accountno = tbd.accountno;
                                td.IFSC = tbd.IFSC;

                                td.plantype = "RD";
                                td.planname = pt.shortcutname;
                                td.plancode = pr.Id;
                                td.bonusper = 0;
                                td.amountword = tbd.amountword;
                                td.term = tbd.term;
                                td.mode = tbd.mode;
                                td.payment = tbd.payment;
                                td.applicationcharge = tbd.applicationcharge;
                                td.nolandunit = tbd.nolandunit;
                                td.totalcon = tbd.totalcon;
                                td.expectedraisablevalue = tbd.expectedraisablevalue;
                                td.expirydate = tbd.expirydate;

                                td.nomineename = tbd.nomineename;
                                td.nomage = tbd.nomage;
                                td.nomrel = tbd.nomrel;
                                td.nomaddr = tbd.nomaddr;

                                td.paymethod = tbd.paymethod;
                                td.pdate = tbd.pdate;
                                td.checkorddno = tbd.checkorddno;
                                td.drawno = tbd.drawno;
                                td.branchpay = tbd.branchpay;
                                td.opid = User.Identity.Name;
                                td.status = 1;

                                td.type = "Customer";
                                td.Macaddress = localIP;
                                td.discountper = tbd.discountper;
                                td.bookingamount = tbd.bookingamount;
                                td.downpayment = 0;
                                td.Time = DateTime.Now.ToShortTimeString();
                                td.plotno = tbd.plotno;
                                td.PYN = tbd.PYN;
                                td.projectid = tbd.projectid;
                                td.block = tbd.block;
                                td.phaseid = bd.phaseid;
                                td.phase = phr == null ? "NA" : phr.Phase;

                                td.Aadhaar_No = "N/A";
                                td.Aadhaar_ReqDate = Convert.ToDateTime("1991-01-01");
                                td.Aadhaar_status = 0;
                                td.Aadhaar_AppDate = Convert.ToDateTime("1991-01-01");
                                td.PAN_AppDate = Convert.ToDateTime("1991-01-01");
                                td.PAN_ReqDate = Convert.ToDateTime("1991-01-01");
                                td.PanStatus = 0;
                                td.paymenttype = tbd.paymenttype;

                                if (td.paymethod == "Cheque")
                                {
                                    td.bank = tbd.bank;
                                    td.Account = tbd.Account;
                                    td.chequeno = tbd.chequeno;
                                    td.ACholdername = tbd.ACholdername;
                                    td.Branch = tbd.Branch;
                                    td.IFSCCode = tbd.IFSCCode;
                                    td.ChequeAmount = tbd.ChequeAmount;
                                    td.Chequedate = tbd.Chequedate;
                                    td.transactiontype = "NA";
                                    if (Chequeimage != null)
                                    {
                                        string img = gid();
                                        td.Chequeimage = "~/Photo/" + img + ".jpg";
                                        Chequeimage.SaveAs(HttpContext.Server.MapPath("~/Photo/" + img + ".jpg"));
                                    }
                                    else
                                    {
                                        td.Chequeimage = "~/Photo/default.jpg";

                                    }
                                }
                                else if (td.paymethod == "banktransaction")
                                {
                                    td.bank = trbank;
                                    td.Account = holderacno;
                                    td.chequeno = transactionid;
                                    td.ACholdername = "NA";
                                    td.Branch = "NA";
                                    td.IFSCCode = "NA";
                                    td.ChequeAmount = tramount;
                                    td.Chequedate = Convert.ToDateTime(transactiondate);
                                    td.Chequeimage = "~/Photo/default.jpg";
                                    td.transactiontype = tbd.transactiontype;
                                }
                                else
                                {
                                    td.bank = tbd.bank;
                                    td.Account = tbd.Account;
                                    td.chequeno = tbd.chequeno;
                                    td.ACholdername = tbd.ACholdername;
                                    td.Branch = tbd.Branch;
                                    td.IFSCCode = tbd.IFSCCode;
                                    td.ChequeAmount = tbd.ChequeAmount;
                                    td.Chequedate = DateTime.Now.Date;
                                    td.Chequeimage = "~/Photo/default.jpg";
                                    td.transactiontype = "NA";
                                }
                                db.tempappltabs.Add(td);
                                db.SaveChanges();

                                if (td.discountper > 2)
                                {
                                    Response.Write("<script>alert('your request is pending please contact to Admin')</script>");
                                    ViewBag.msg = " your request is pending please contact to Admin";
                                }
                                else
                                {
                                    Response.Write("<script>alert('your request is pending So Approve Bond')</script>");
                                    ViewBag.msg = " your request is pending So Approve Bond";
                                }

                                ViewData["bondtype"] = "Discount";
                                ViewData["newbondid"] = bond.ToString();
                                ViewData["discount"] = tbd.discountper;
                                BondType = "Discount";
                            }
                            #endregion

                            ViewData["paymethod"] = bd.paymethod;
                            var count1 = db.BlockTabs.Where(ui => ui.Plancode == bd.projectid && ui.phaseid == bd.phaseid && ui.block == bd.block).Count();
                            if (count1 > 0)
                            {
                                var block = db.BlockTabs.Single(ui => ui.Plancode == bd.projectid && ui.phaseid == bd.phaseid && ui.block == bd.block);
                                var cnt = db.HoldingPlots.Where(a => a.holdby == bd.newintroducerid && a.holdplotno == bd.plotno && a.holdblock == block.Id && a.holdprojectid == block.Plancode).Count();

                                if (cnt > 0)
                                {
                                    HoldingPlot cb = db.HoldingPlots.Single(a => a.holdby == bd.newintroducerid && a.holdplotno == bd.plotno && a.holdblock == block.Id && a.holdprojectid == block.Plancode);
                                    cb.holdstatus = 0;
                                    db.SaveChanges();
                                }
                            }
                        }
                        break;
                }

            }

            return View(mt);

        }



        public ActionResult Createmenu()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View();
            }
        }
        [HttpGet]
        public ActionResult PaymentMenu()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult PassBookMenu()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View();
            }
        }
        public ActionResult Duplicatemenu()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View();
            }
        }
        [HttpGet]
        public ActionResult Voucher()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View();
            }
        }


        public ActionResult Fieldmenu()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View();
            }
        }



        public ActionResult Officialmenu()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View();
            }
        }

        public ActionResult Searchmenu()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View();
            }
        }
        #endregion

        #region Create Menu Start Here

        [HttpGet]
        public ActionResult Member()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {


                return View();
            }
        }
        [HttpPost]
        public ActionResult Member(Member_tab model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var ds = db.Blockdates.Where(c => c.date == model.Cdate && c.branchcode == User.Identity.Name && c.status == 0).Count();
                if (ds > 0)
                {
                    Response.Write("<script>alert('This Date is closed please select another date')</script>");
                }
                else
                {
                    var oprdetail = db.Operators.Single(x => x.OperatorId == User.Identity.Name);
                    var row1 = db.Branchtabs.Single(c => c.BranchCode == oprdetail.BranchCode);
                    var csrt = db.CompanyInfos.Single(a => a.AdminId == row1.companyid);
                    int max = 0;
                    int count = (from a in db.Member_tabs select a.MemberId).Count();
                    if (count == 0)
                    {
                        max = 1;
                    }
                    else
                    {
                        max = count + 1;
                    }

                    int bwcount = (from a in db.Member_tabs where a.BranchCode == row1.BranchCode select a).Count();
                    var newmemberid = row1.prefix + row1.BranchCode + "/M0" + (bwcount + 1);

                    var Duplicate = (from o in db.Member_tabs where o.MemberName == model.MemberName && o.Father == model.Father select o).ToList();

                    if (Duplicate.Count > 0)
                    {
                        Response.Write("<script>alert('This Member Already exist ')</script>");
                    }

                    else
                    {

                        //var r = new Member_tab();
                        model.MemberId = max;//
                        model.NewMemberId = newmemberid;//
                        model.BranchCode = oprdetail.BranchCode;//
                        model.Opid = User.Identity.Name;//
                        db.Member_tabs.Add(model);
                        db.SaveChanges();
                        ViewBag.msg = "Member Created Successfully Your Member Id is " + newmemberid + " ....";
                    }
                }
                return View();
            }
        }

        [HttpGet]
        public ActionResult CreateAccount()
        {
            List<Member_tab> mt = new List<Member_tab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(mt);
            }
        }
        [HttpPost]
        public ActionResult CreateAccount(SavingAccountInfo model, HttpPostedFileBase photo, HttpPostedFileBase photo2, HttpPostedFileBase idproof, HttpPostedFileBase idproof2, HttpPostedFileBase sign, HttpPostedFileBase sign2, string command, string memberid, Double openingamount = 0)
        {
            List<Member_tab> mt = new List<Member_tab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {


                switch (command)
                {
                    case "Search":

                        switch (memberid)
                        {
                            case "":
                                Response.Write("<script>alert('Please Select Member Id First')</script>");
                                break;
                            default:

                                mt = db.Member_tabs.Where(c => c.NewMemberId == memberid).ToList();
                                return View(mt);
                        }

                        break;

                    case "Submit":


                        var count = (from c in db.SavingAccountInfos where c.newmemberid == model.newmemberid select c).Count();
                        if (count == 0)
                        {
                            var r = new SavingAccountInfo();
                            var opr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                            var row1 = db.Branchtabs.Single(c => c.BranchCode == opr.BranchCode);
                            var csrt = db.CompanyInfos.Single(a => a.AdminId == row1.companyid);

                            var count2 = db.SavingAccountInfos.Where(s => s.branchcode == row1.BranchCode).Count();

                            var aa = "S" + System.DateTime.Now.Year.ToString() + row1.BranchCode + "00" + Convert.ToInt32(count2 + 1);
                            SavingAccountInfo ad = new SavingAccountInfo();

                            ad.formfee = model.formfee;
                            ad.branchcode = opr.BranchCode;
                            ad.accountno = aa;
                            ad.actype = model.actype;
                            ad.acholdername = model.acholdername;
                            ad.acholdername2 = model.acholdername2;
                            ad.fathername = model.fathername;
                            ad.fathername2 = model.fathername2;
                            ad.address = model.address;
                            ad.address2 = model.address2;
                            ad.mobileno = model.mobileno;
                            ad.mobileno2 = model.mobileno2;
                            ad.emailid = model.emailid;
                            ad.emailid2 = model.emailid2;
                            ad.gender = model.gender;
                            ad.gender2 = model.gender2;
                            ad.newmemberid = model.newmemberid;
                            ad.dob = model.dob;
                            ad.dob2 = model.dob2;

                            ad.Doj = model.Doj;
                            ad.bloodgroup = model.bloodgroup;
                            ad.bloodgroup2 = model.bloodgroup2;
                            ad.occupation = model.occupation;
                            ad.occupation2 = model.occupation2;
                            ad.education = model.education;
                            ad.education2 = model.education2;

                            ad.annualincome2 = model.annualincome2;
                            ad.annualincome = model.annualincome;
                            ad.panno = model.panno;
                            ad.ifsccode = model.ifsccode;
                            ad.bankname = model.bankname;
                            ad.bankaddress = model.bankaddress;

                            ad.guardianname = model.guardianname;
                            ad.gurage = model.gurage;
                            ad.gurrel = model.gurrel;
                            ad.guraddr = model.guraddr;


                            ad.nomineename = model.nomineename;
                            ad.nomage = model.nomage;
                            ad.nomrel = model.nomrel;
                            ad.nomaddr = model.nomaddr;



                            if (photo != null)
                            {
                                string imgname = gid();
                                ad.photo = "~/Photo/" + imgname + ".jpg";
                                photo.SaveAs(HttpContext.Server.MapPath("~/Photo/") + imgname + ".jpg");
                            }
                            else
                            {
                                ad.photo = "~/Photo/default.jpg";

                            }

                            if (photo2 != null)
                            {
                                string imgname = gid();
                                ad.photo2 = "~/Photo/" + imgname + ".jpg";
                                photo2.SaveAs(HttpContext.Server.MapPath("~/Photo/") + imgname + ".jpg");

                            }
                            else
                            {
                                ad.photo2 = "~/Photo/default.jpg";
                            }


                            if (idproof != null)
                            {
                                string imgname = gid();
                                ad.idproof = "~/Photo/" + imgname + ".jpg";
                                idproof.SaveAs(HttpContext.Server.MapPath("~/Photo/") + imgname + ".jpg");
                            }
                            else
                            {
                                ad.idproof = "~/Photo/default.jpg";

                            }

                            if (idproof2 != null)
                            {
                                string imgname = gid();
                                ad.idproof2 = "~/Photo/" + imgname + ".jpg";
                                idproof2.SaveAs(HttpContext.Server.MapPath("~/Photo/") + imgname + ".jpg");

                            }
                            else
                            {
                                ad.idproof2 = "~/Photo/default.jpg";

                            }



                            if (sign != null)
                            {
                                string imgname = gid();
                                ad.sign = "~/Photo/" + imgname + ".jpg";
                                sign.SaveAs(HttpContext.Server.MapPath("~/Photo/") + imgname + ".jpg");

                            }
                            else
                            {
                                ad.sign = "~/Photo/default.jpg";

                            }


                            if (sign2 != null)
                            {
                                string imgname = gid();
                                ad.sign2 = "~/Photo/" + imgname + ".jpg";
                                sign2.SaveAs(HttpContext.Server.MapPath("~/Photo/") + imgname + ".jpg");

                            }
                            else
                            {
                                ad.sign2 = "~/Photo/default.jpg";
                            }

                            ad.opid = User.Identity.Name;
                            ad.status = 1;
                            ad.Time = DateTime.Now.ToShortTimeString();


                            string trid = gid();
                            TransactionTab td = new TransactionTab();
                            var j = db.TransactionTabs.Where(s => s.accountno == aa).Count();
                            td.transactionid = trid;
                            td.branchcode = row1.BranchCode;
                            td.accountno = aa;
                            td.actype = model.actype;
                            td.acholdername = model.acholdername;
                            td.paymethod = "CASH";
                            td.pdate = model.Doj;
                            td.credit = openingamount;
                            td.debit = 0;
                            td.balance = openingamount;
                            td.remark = "OPEN";
                            td.opid = User.Identity.Name;
                            td.status = (j + 1);
                            td.type = "Credit";
                            td.Time = DateTime.Now.ToShortTimeString();

                            db.TransactionTabs.Add(td);
                            db.SavingAccountInfos.Add(ad);


                            db.SaveChanges();
                            MyClass.Sendmsg(model.mobileno, "Dear " + model.acholdername + ", Your A/c no. " + aa + " credited with INR " + openingamount + " is successfully in " + csrt.CompanyName + " Visit us " + cg.HeadOffice);
                            if (model.mobileno2 != "" || model.mobileno2 != null)
                            {
                                MyClass.Sendmsg(model.mobileno2, "Dear " + model.acholdername2 + ", Your A/c no. " + aa + " credited with INR " + openingamount + " is successfully in " + csrt.CompanyName + " Visit us " + cg.HeadOffice);
                            }
                            ViewBag.msg = "Dear " + model.acholdername + ", Your A/c has been created successfully in " + csrt.CompanyName + ", your A/c no: " + aa;
                        }
                        else
                        {
                            Response.Write("<script>alert('Sorry,Your A/c already created')</script>");
                        }
                        break;
                }
            }
            return View(mt);
        }
        string gidref()
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
            return s.Substring(0, 4);


        }
        [HttpGet]
        public ActionResult NewAgent()
        {
            List<Member_tab> mt = new List<Member_tab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(mt);
            }
        }
        [HttpPost]
        public ActionResult NewAgent(AgentDetail model, HttpPostedFileBase Photo, string command, string memberid, string BranchCode)
        {
            List<Member_tab> mt = new List<Member_tab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {


                switch (command)
                {
                    case "Search":

                        switch (memberid)
                        {
                            case "":
                                Response.Write("<script>alert('Please Select Member Id First')</script>");
                                break;
                            default:

                                mt = db.Member_tabs.Where(c => c.NewMemberId == memberid).ToList();
                                return View(mt);
                        }

                        break;

                    case "Submit":
                        var ds = db.Blockdates.Where(c => c.date == model.Doj && c.branchcode == User.Identity.Name && c.status == 0).Count();
                        if (ds > 0)
                        {
                            Response.Write("<script>alert('This Date is closed please select another date')</script>");
                        }
                        else
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

                            var Duplicate = (from o in db.AgentDetails where o.newmemberid == model.newmemberid select o).ToList();
                            if (Duplicate.Count > 0)
                            {
                                Response.Write("<script>alert('This Member already exist as a Broker')</script>");
                            }
                            else
                            {
                                string passw = gid();
                                var r = new AgentDetail();
                                var opdetails = db.Operators.Single(x => x.OperatorId == User.Identity.Name);
                                var row1 = db.Branchtabs.Single(c => c.BranchCode == opdetails.BranchCode);
                                var csrt = db.CompanyInfos.Single(a => a.AdminId == row1.companyid);
                                int max = (from a in db.AgentDetails select a).Count();
                                int bwcount = (from a in db.AgentDetails where a.BranchCode == opdetails.BranchCode select a).Count();
                                var row = db.Ranktabs.Single(c => c.RankCode == model.RankCode);
                                var newagentid = row1.prefix + "-" + row1.BranchCode + "/A0" + (bwcount + 1);
                                int bcnt = bwcount;
                                var mrow = db.Member_tabs.Single(p => p.NewMemberId == model.newmemberid);

                                if (max > 0)
                                {
                                    r = db.AgentDetails.Single(c => c.NewAgentId == model.NewIntroducerId);
                                }

                                TimeSpan span = model.Doj.Subtract(model.Dob);
                                int years = (int)(span.Days / 365.25);
                                int remainder = (int)(span.Days % 365.25);
                                int months = remainder / 30;
                                if (months >= 12)
                                {
                                    int y = months / 12;
                                    int m = months % 12;
                                    months = m;
                                    years = years + y;
                                }
                                int days = remainder % 30;
                                var age = years + " years " + months + " months " + days + " days";

                                AgentDetail ad = new AgentDetail();

                                ad.formfee = model.formfee;
                                ad.BranchCode = model.BranchCode;
                                ad.name = model.name;
                                ad.Father = model.Father;
                                ad.Mother = model.Mother;
                                ad.Gender = model.Gender;
                                ad.NewAgentId = newagentid;
                                if (max > 0)
                                {
                                    ad.IntroducerCode = r.AgencyCode;
                                }
                                else
                                {
                                    ad.IntroducerCode = max;
                                }
                                ad.NewIntroducerId = model.NewIntroducerId;
                                ad.IntroName = model.IntroName;
                                ad.RankCode = model.RankCode;
                                ad.Nationality = model.Nationality;
                                ad.Dob = model.Dob;
                                ad.Age = age;
                                ad.BloodGroup = model.BloodGroup;
                                ad.Occupation = model.Occupation;
                                ad.Qualification = model.Qualification;
                                ad.Panno = model.Panno;
                                ad.Passportno = model.Passportno;
                                ad.Drivinglno = model.Drivinglno;
                                ad.Icardno = model.Icardno;
                                ad.Issueon = model.Doj;
                                ad.Validupto = model.Doj.AddYears(1);
                                ad.BankName = model.BankName;
                                ad.BankAccountno = model.BankAccountno;
                                ad.IFCCode = model.IFCCode;
                                ad.BankAddress = model.BankAddress;
                                ad.Address = model.Address;
                                ad.District = model.District;
                                ad.State = model.State;
                                ad.PinCode = model.PinCode;
                                ad.Landlineno = model.Landlineno;
                                ad.Mobile = model.Mobile;
                                ad.Email = model.Email;
                                ad.NomineeName = model.NomineeName;
                                ad.NomineeAge = model.NomineeAge;
                                ad.NomineeAddress = model.NomineeAddress;
                                ad.Organization = model.Organization;
                                ad.Doj = model.Doj;
                                ad.areaofoccupation = model.areaofoccupation;
                                ad.approximatenoofactive = model.approximatenoofactive;
                                ad.operatorid = User.Identity.Name;
                                ad.Company = model.Company;
                                ad.Yoe = model.Yoe;
                                ad.Mobileno = model.Mobileno;
                                ad.cmpnyaddress = model.cmpnyaddress;
                                ad.Experience = model.Experience;
                                ad.Password = passw;
                                ad.RankName = row.RankName;

                                ad.newmemberid = model.newmemberid;
                                ad.memberid = mrow.MemberId;
                                var ro = db.Member_tabs.Single(ui => ui.NewMemberId == model.newmemberid);
                                ad.memberid = ro.MemberId;
                                if (Photo != null)
                                {
                                    string imgname = gid();
                                    ad.Photo = "~/Photo/" + imgname + ".jpg";
                                    Photo.SaveAs(HttpContext.Server.MapPath("~/Photo/") + imgname + ".jpg");


                                }
                                else
                                {
                                    ad.Photo = "~/Photo/default.jpg";

                                }
                                ad.Type = "Agent";
                                ad.Status = 1;
                                ad.Macaddress = localIP;
                                ad.Time = DateTime.Now.ToShortTimeString();
                                /**/
                                ad.BankBranchName = "N/A";
                                ad.OtherMobile = 0;

                                ad.Aadhaar_No = "N/A";
                                ad.Aadhaar_ReqDate = Convert.ToDateTime("1991-01-01");
                                ad.Aadhaar_status = 0;
                                ad.Aadhaar_AppDate = Convert.ToDateTime("1991-01-01");
                                ad.PAN_AppDate = Convert.ToDateTime("1991-01-01");
                                ad.PAN_ReqDate = Convert.ToDateTime("1991-01-01");
                                ad.PanStatus = 0;
                                var csr = gidref();
                                int year = DateTime.Now.Year, syear = year + 1;
                                string refyear = year.ToString().Substring(2, 2) + "-" + syear.ToString().Substring(2, 2);
                                string refno = "ODH/WEL/" + refyear + "/" + csr;
                                ad.refno = refno;
                                /**/
                                NewLogin nl = new NewLogin();
                                nl.UserName = newagentid;
                                nl.Password = passw;
                                nl.Mobile = model.Mobile;
                                nl.type = "Agent";
                                nl.status = 1;
                                db.NewLogins.Add(nl);
                                db.AgentDetails.Add(ad);
                                db.SaveChanges();
                                var cr = db.CompanyInfos.Single(d => d.Id == 1);
                                //MyClass.Sendmsg(model.Mobile, "Dear " + model.name + " Thank you for Being a part of " + cr.CompanyName + " . Regards- " + cr.CompanyName + ".");
                                MyClass.Sendmsg(model.Mobile, "Dear " + ad.name + ", Thank you for Being a part of " + cr.CompanyName + " Login on " + cr.HeadOffice + " with your user ID as " + nl.UserName + " and password is " + nl.Password + ". Use " + nl.UserName + " as referral ID. Regards- " + cr.CompanyName + ".");
                                var memag = db.Members.Single(agm => agm.Id == 1);
                                Response.Write("<script>alert('" + memag.agentname + " SuccessFully Created Your Id is " + newagentid + " ')</script>");
                                ViewBag.msg = "" + memag.agentname + " SuccessFully Created Your Id is " + newagentid + " ";

                            }
                        }
                        break;
                }

            }
            return View(mt);
        }

        [HttpGet]
        public ActionResult NewAgreement()
        {
            List<Member_tab> mt = new List<Member_tab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }

            else
            {

                return View(mt);
            }
        }
        [HttpPost]
        public ActionResult NewAgreement(appltab bd, tempappltab tbd, HttpPostedFileBase photo, HttpPostedFileBase Chequeimage, HttpPostedFileBase spanphoto, HttpPostedFileBase sphoto, HttpPostedFileBase saadharphoto, string command, string memberid, string trbank, string transactionid, string transactiondate, string tramount, string trpmethod, string holderacno, string sdob = "", string dob = "", string sanniversarydate = "")
        {
            List<Member_tab> mt = new List<Member_tab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                switch (command)
                {
                    case "Search":

                        switch (memberid)
                        {
                            case "":
                                Response.Write("<script>alert('Please Select Contributor Id First')</script>");
                                break;
                            default:

                                mt = db.Member_tabs.Where(c => c.NewMemberId == memberid).ToList();
                                return View(mt);
                        }


                        break;

                    case "Submit":
                        var ds = db.Blockdates.Where(c => c.date == bd.formdate && c.branchcode == bd.branchcode && c.status == 0).Count();
                        if (ds > 0)
                        {
                            Response.Write("<script>alert('This Date is closed please select another date')</script>");
                        }
                        else
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

                            var phr = db.PhaseTabs.Single(c => c.Id == bd.phaseid);
                            var pt = db.Plans.Single(c => c.Plancode == bd.projectid);
                            var mr = db.Members.Single(m => m.Id == 1);
                            var br = db.Branchtabs.Single(bc => bc.BranchCode == bd.branchcode);
                            var cr = db.CompanyInfos.Single(csn => csn.AdminId == br.companyid);
                            int max = (from a in db.appltabs select a).Count();
                            var bondid = max + 1;
                            string dd = DateTime.Now.Date.Month.ToString("00");

                            int yr = DateTime.Now.Date.Year;
                            var newbondid = br.prefix + bd.branchcode + "/C0" + bondid;
                            //var newbondid = br.prefix + bd.branchcode + "/C0" + dd + bondid.ToString().Substring(0, 2) + DateTime.Now.Date.Day.ToString("00") + bondid.ToString().Substring(2) + yr.ToString().Substring(2);
                            var adr = db.AgentDetails.Single(ad => ad.NewAgentId == bd.newintroducerid);
                            ProTerm pr = new ProTerm();
                            if (bd.paymenttype != "EMI")
                            {
                                pr = db.ProTerms.FirstOrDefault();
                            }
                            else
                            {
                                pr = db.ProTerms.Single(p => p.projectid == bd.projectid && p.term == bd.term);
                            }
                            if (bd.paymenttype != "EMI") { bd.bookingamount = bd.totalcon; }
                            TimeSpan span = bd.formdate.Subtract(bd.dob);
                            int years = (int)(span.Days / 365.25);
                            int remainder = (int)(span.Days % 365.25);
                            int months = remainder / 30;
                            if (months >= 12)
                            {
                                int y = months / 12;
                                int m = months % 12;
                                months = m;
                                years = years + y;
                            }
                            int days = remainder % 30;
                            var age = years + " years " + months + " months " + days + " days";

                            var row = db.Member_tabs.Single(ui => ui.NewMemberId == bd.newmemberid);

                            #region APPLTAB
                            if (bd.paymethod != "Cheque" && bd.paymethod != "banktransaction" && bd.discountper == 0)
                            {

                                bd.bondid = bondid;//
                                bd.newbondid = newbondid;//

                                bd.introducerid = adr.AgencyCode;//
                                bd.intrankcode = adr.RankCode;//
                                bd.intrankname = adr.RankName;//

                                bd.age = age;//

                                bd.memberid = row.MemberId;//


                                if (photo != null)
                                {
                                    string imgname = gid();
                                    bd.photo = "~/Photo/" + imgname + ".jpg";
                                    photo.SaveAs(HttpContext.Server.MapPath("~/Photo/" + imgname + ".jpg"));
                                }
                                else
                                {
                                    bd.photo = "~/Photo/default.jpg";

                                }

                                bd.plantype = "RD";
                                bd.planname = pt.shortcutname;//
                                bd.plancode = pr.Id;//
                                bd.bonusper = 0;//


                                bd.pdate = bd.formdate;
                                bd.opid = User.Identity.Name;//
                                bd.status = 1;//

                                bd.type = "Customer";//
                                bd.Macaddress = localIP;//

                                bd.downpayment = 0;//
                                bd.Time = DateTime.Now.ToShortTimeString();//

                                bd.printstatus = 0;//
                                bd.phaseid = bd.phaseid;
                                bd.phase = phr.Phase;

                                bd.Chequedate = bd.pdate;
                                bd.transactiontype = "NA";//
                                if (Chequeimage != null)
                                {
                                    string imgname = gid();
                                    bd.Chequeimage = "~/Photo/" + imgname + ".jpg";
                                    Chequeimage.SaveAs(HttpContext.Server.MapPath("~/Photo/" + imgname + ".jpg"));
                                }
                                else
                                {
                                    bd.Chequeimage = "~/Photo/default.jpg";
                                }
                                bd.chequeappdate = bd.pdate;

                                bd.Aadhaar_No = "N/A";//
                                bd.Aadhaar_ReqDate = Convert.ToDateTime("1991-01-01");//
                                bd.Aadhaar_status = 0;//
                                bd.Aadhaar_AppDate = Convert.ToDateTime("1991-01-01");//
                                bd.PAN_AppDate = Convert.ToDateTime("1991-01-01");//
                                bd.PAN_ReqDate = Convert.ToDateTime("1991-01-01");//
                                bd.PanStatus = 0;//
                                if (sphoto != null)
                                {
                                    bd.sphoto = GenerateImageTag(sphoto);
                                }
                                if (spanphoto != null)
                                {
                                    bd.spanphoto = GenerateImageTag(spanphoto);
                                }
                                if (saadharphoto != null)
                                {
                                    bd.saadharphoto = GenerateImageTag(saadharphoto);
                                }
                                //date section

                                bd.dob = GetCleanDate(dob);
                                // bd.sanniversarydate = GetCleanDate(sanniversarydate);
                                // bd.sdob = GetCleanDate(sdob);
                                //
                                int propertyid = 0, count = 0; string loanid = ""; Int64 om = 0;
                                if (bd.mobileno != null && Int64.TryParse(bd.mobileno, out om))
                                {
                                    propertyid = Convert.ToInt32(bd.mobileno.Substring(bd.mobileno.Length - 4));
                                    count = db.appltabs.Count(d => d.propertyid == propertyid);
                                    while (count > 0)
                                    {
                                        propertyid = Convert.ToInt32(gid().Substring(0, 4));
                                        count = db.appltabs.Count(d => d.propertyid == propertyid);
                                    }
                                    if (bd.paymenttype == "EMI")
                                    {
                                        loanid = bd.mobileno + "0";
                                        count = db.appltabs.Count(d => d.loanid == loanid);
                                        while (count > 0)
                                        {
                                            loanid = gid() + "0";
                                            count = db.appltabs.Count(d => d.propertyid == propertyid);
                                        }
                                    }
                                }
                                else
                                {
                                    propertyid = Convert.ToInt32(bd.mobileno.Substring(bd.mobileno.Length - 4));
                                    count = db.appltabs.Count(d => d.propertyid == propertyid);
                                    while (count > 0)
                                    {
                                        propertyid = Convert.ToInt32(gid().Substring(0, 4));
                                        count = db.appltabs.Count(d => d.propertyid == propertyid);
                                    }
                                    if (bd.paymenttype == "EMI")
                                    {
                                        loanid = bd.mobileno + "0";
                                        count = db.appltabs.Count(d => d.loanid == loanid);
                                        while (count > 0)
                                        {
                                            loanid = gid() + "0";
                                            count = db.appltabs.Count(d => d.propertyid == propertyid);
                                        }
                                    }
                                }
                                //set propertyid
                                bd.propertyid = propertyid;
                                bd.loanid = loanid;
                                /*---------------------------------ref no----------------**/
                                count = 0;
                                count = db.appltabs.Count();
                                string csr = "";
                                if (count == 0) { csr = "001"; }
                                else
                                {
                                    count = count + 1;
                                    if (count < 10)
                                    {
                                        csr = "00" + count;
                                    }
                                    else if (count >= 10 && count < 100)
                                    {
                                        csr = "0" + count;
                                    }
                                    else if (count >= 100)
                                    {
                                        csr = count.ToString();
                                    }
                                }


                                int year = DateTime.Now.Year, syear = year + 1;
                                string refyear = year.ToString().Substring(2, 2) + "-" + syear.ToString().Substring(2, 2);
                                string refno = "ODH HOMES/" + refyear + "/" + csr;
                                //                                
                                bd.refno = refno;
                                db.appltabs.Add(bd);
                                db.SaveChanges();

                                RevivalTab rt = new RevivalTab();
                                rt.newbondid = newbondid;
                                rt.status = 1;
                                db.RevivalTabs.Add(rt);
                                db.SaveChanges();

                                NewLogin nl = new NewLogin();
                                nl.UserName = newbondid;
                                nl.Password = bd.mobileno;
                                nl.Mobile = bd.mobileno;
                                nl.type = "Customer";
                                nl.status = 1;
                                db.NewLogins.Add(nl);
                                db.SaveChanges();

                                con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                                SqlCommand cmd = new SqlCommand();
                                cmd.CommandText = "InsertAppTab";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = con;

                                cmd.Parameters.AddWithValue("@introducerid", adr.AgencyCode);
                                cmd.Parameters.AddWithValue("@plancode", pr.Id);
                                cmd.Parameters.AddWithValue("@planname", pt.shortcutname);
                                cmd.Parameters.AddWithValue("@payment", bd.payment);
                                cmd.Parameters.AddWithValue("@bondid", bondid);
                                cmd.Parameters.AddWithValue("@mode", bd.mode);
                                cmd.Parameters.AddWithValue("@formdate", bd.formdate);
                                cmd.Parameters.AddWithValue("@pdate", bd.pdate);
                                cmd.Parameters.AddWithValue("@term", bd.term);
                                cmd.Parameters.AddWithValue("@opid", User.Identity.Name);


                                try
                                {
                                    con.Open();
                                    cmd.ExecuteNonQuery();
                                    //MyClass.Sendmsg(bd.mobileno, "Dear " + bd.name + ", your Loginid:" + newbondid + " Password:" + bd.mobileno + " For your A/c details please visit "+cg.HeadOffice);
                                    //MyClass.Sendmsg(bd.mobileno, "Dear Customer your Application no- " + newbondid + " has been activated with Rs. " + bd.bookingamount + " in the plan " + pt.Planname + "Thank you for your kind believe us for help contact. "+cg.CompanyName);

                                    if (bd.PYN == "y" || bd.PYN == "Y")
                                    {
                                        MyClass.Sendmsg(bd.mobileno, "Dear " + bd.name + ", Thank you for purchasing a plot in " + pr.projectname + ". Your Plot No. " + bd.plotno + "," + bd.block + " ," + bd.phase + " at " + pr.projectname + " is Allotted Successfully. Regards- " + cr.CompanyName);

                                    }
                                    else
                                    {
                                        MyClass.Sendmsg(bd.mobileno, "Dear " + bd.name + ", Thank you for purchasing a plot in " + pr.projectname + ". Regards- " + cr.CompanyName);

                                    }
                                    MyClass.Sendmsg(bd.mobileno, "Dear " + bd.name + ", login on " + cr.HeadOffice + " with your user ID as " + nl.UserName + " and password is " + nl.Password + ". Regards- " + cr.CompanyName);

                                    Response.Write("<script>alert('" + mr.custname + " created Successfully....')</script>");
                                    ViewBag.msg = mr.custname + " created Successfully....";
                                    NewBondid = newbondid;
                                    ViewData["bondtype"] = "Normal";
                                    ViewData["newbondid"] = newbondid;
                                    ViewData["discount"] = bd.discountper;
                                    BondType = "Normal";

                                }

                                catch (Exception e)
                                {
                                    ViewBag.msg = e.Message;
                                }
                                finally
                                {
                                    con.Close();
                                }
                            }
                            #endregion

                            #region tempappltab
                            else
                            {

                                int bid = (from a in db.tempappltabs select a.bondid).DefaultIfEmpty(0).Max();
                                int bond = bid + 1;
                                tempappltab td = new tempappltab();
                                td.branchcode = tbd.branchcode;
                                td.bondid = bond;
                                td.newbondid = bond.ToString();
                                td.name = tbd.name;
                                td.RelationOf = tbd.RelationOf;
                                td.fathername = tbd.fathername;
                                td.addr = tbd.addr;
                                td.mobileno = tbd.mobileno;
                                td.introducerid = adr.AgencyCode;
                                td.intrankcode = adr.RankCode;
                                td.intrankname = adr.RankName;
                                td.newintroducerid = bd.newintroducerid;
                                td.formdate = Convert.ToDateTime(bd.formdate);
                                td.dob = bd.dob;
                                td.age = age;
                                td.nationality = tbd.nationality;
                                td.newmemberid = tbd.newmemberid;
                                td.memberid = row.MemberId;
                                td.guardianname = tbd.guardianname;
                                td.gurage = tbd.gurage;
                                td.gurrel = tbd.gurrel;
                                td.guraddr = tbd.guraddr;

                                if (photo != null)
                                {
                                    string imgname = gid();
                                    td.photo = "~/Photo/" + imgname + ".jpg";
                                    photo.SaveAs(HttpContext.Server.MapPath("~/Photo/" + imgname + ".jpg"));
                                }
                                else
                                {
                                    td.photo = "~/Photo/default.jpg";

                                }

                                td.panno = tbd.panno;
                                td.bankname = tbd.bankname;
                                td.accountno = tbd.accountno;
                                td.IFSC = tbd.IFSC;

                                td.plantype = "RD";
                                td.planname = pt.shortcutname;
                                td.plancode = pr.Id;
                                td.bonusper = 0;
                                td.amountword = tbd.amountword;
                                td.term = tbd.term;
                                td.mode = tbd.mode;
                                td.payment = tbd.payment;
                                td.applicationcharge = tbd.applicationcharge;
                                td.nolandunit = tbd.nolandunit;
                                td.totalcon = tbd.totalcon;
                                td.expectedraisablevalue = tbd.expectedraisablevalue;
                                td.expirydate = tbd.expirydate;

                                td.nomineename = tbd.nomineename;
                                td.nomage = tbd.nomage;
                                td.nomrel = tbd.nomrel;
                                td.nomaddr = tbd.nomaddr;

                                td.paymethod = tbd.paymethod;
                                td.pdate = tbd.formdate;
                                td.checkorddno = tbd.checkorddno;
                                td.drawno = tbd.drawno;
                                td.branchpay = tbd.branchpay;
                                td.opid = User.Identity.Name;
                                td.status = 1;

                                td.type = "Customer";
                                td.Macaddress = localIP;
                                td.discountper = tbd.discountper;
                                td.bookingamount = tbd.bookingamount;
                                td.downpayment = 0;
                                td.Time = DateTime.Now.ToShortTimeString();
                                td.plotno = tbd.plotno;
                                td.PYN = tbd.PYN;
                                td.projectid = tbd.projectid;
                                td.block = tbd.block;
                                td.phaseid = bd.phaseid;
                                td.phase = phr.Phase;

                                td.Aadhaar_No = "N/A";
                                td.Aadhaar_ReqDate = Convert.ToDateTime("1991-01-01");
                                td.Aadhaar_status = 0;
                                td.Aadhaar_AppDate = Convert.ToDateTime("1991-01-01");
                                td.PAN_AppDate = Convert.ToDateTime("1991-01-01");
                                td.PAN_ReqDate = Convert.ToDateTime("1991-01-01");
                                td.PanStatus = 0;
                                td.paymenttype = tbd.paymenttype;

                                if (td.paymethod == "Cheque")
                                {
                                    td.bank = tbd.bank;
                                    td.Account = tbd.Account;
                                    td.chequeno = tbd.chequeno;
                                    td.ACholdername = tbd.ACholdername;
                                    td.Branch = tbd.Branch;
                                    td.IFSCCode = tbd.IFSCCode;
                                    td.ChequeAmount = tbd.ChequeAmount;
                                    td.Chequedate = tbd.Chequedate;
                                    td.transactiontype = "NA";
                                    if (Chequeimage != null)
                                    {
                                        string img = gid();
                                        td.Chequeimage = "~/Photo/" + img + ".jpg";
                                        Chequeimage.SaveAs(HttpContext.Server.MapPath("~/Photo/" + img + ".jpg"));
                                    }
                                    else
                                    {
                                        td.Chequeimage = "~/Photo/default.jpg";

                                    }
                                }
                                else if (td.paymethod == "banktransaction")
                                {
                                    td.bank = trbank;
                                    td.Account = holderacno;
                                    td.chequeno = transactionid;
                                    td.ACholdername = "NA";
                                    td.Branch = "NA";
                                    td.IFSCCode = "NA";
                                    td.ChequeAmount = tramount;
                                    td.Chequedate = Convert.ToDateTime(transactiondate);
                                    td.Chequeimage = "~/Photo/default.jpg";
                                    td.transactiontype = tbd.transactiontype;
                                }
                                else
                                {
                                    td.bank = tbd.bank;
                                    td.Account = tbd.Account;
                                    td.chequeno = tbd.chequeno;
                                    td.ACholdername = tbd.ACholdername;
                                    td.Branch = tbd.Branch;
                                    td.IFSCCode = tbd.IFSCCode;
                                    td.ChequeAmount = tbd.ChequeAmount;
                                    td.Chequedate = DateTime.Now.Date;
                                    td.Chequeimage = "~/Photo/default.jpg";
                                    td.transactiontype = "NA";
                                }
                                db.tempappltabs.Add(td);
                                db.SaveChanges();

                                if (td.discountper > 2)
                                {
                                    Response.Write("<script>alert('your request is pending please contact to Admin')</script>");
                                    ViewBag.msg = " your request is pending please contact to Admin";
                                }
                                else
                                {
                                    Response.Write("<script>alert('your request is pending So Approve Bond')</script>");
                                    ViewBag.msg = " your request is pending So Approve Bond";
                                }

                                ViewData["bondtype"] = "Discount";
                                ViewData["newbondid"] = bond.ToString();
                                ViewData["discount"] = tbd.discountper;
                                BondType = "Discount";
                            }
                            #endregion

                            ViewData["paymethod"] = bd.paymethod;
                            var count1 = db.BlockTabs.Where(ui => ui.block == bd.block && ui.Plancode == bd.projectid).Count();
                            if (count1 > 0)
                            {
                                var block = db.BlockTabs.Single(ui => ui.block == bd.block && ui.Plancode == bd.projectid);
                                var cnt = db.HoldingPlots.Where(a => a.holdby == bd.newintroducerid && a.holdplotno == bd.plotno && a.holdblock == block.Id && a.holdprojectid == block.Plancode).Count();

                                if (cnt > 0)
                                {
                                    HoldingPlot cb = db.HoldingPlots.Single(a => a.holdby == bd.newintroducerid && a.holdplotno == bd.plotno && a.holdblock == block.Id && a.holdprojectid == block.Plancode);
                                    cb.holdstatus = 0;
                                    db.SaveChanges();
                                }
                            }
                        }
                        break;
                }

            }

            return View(mt);
        }

        [HttpGet]
        public ActionResult ManualBond()
        {
            List<Member_tab> mt = new List<Member_tab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(mt);
            }
        }
        [HttpPost]
        public ActionResult ManualBond(appltab model, HttpPostedFileBase photo, string command, string memberid)
        {
            List<Member_tab> mt = new List<Member_tab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                switch (command)
                {
                    case "Search":

                        switch (memberid)
                        {
                            case "":
                                Response.Write("<script>alert('Please Select Contributor Id First')</script>");
                                break;
                            default:

                                mt = db.Member_tabs.Where(c => c.NewMemberId == memberid).ToList();
                                return View(mt);
                        }


                        break;

                    case "Submit":
                        var ds = db.Blockdates.Where(c => c.date == model.formdate && c.branchcode == User.Identity.Name && c.status == 0).Count();
                        if (ds > 0)
                        {
                            Response.Write("<script>alert('This Date is closed please select another date')</script>");
                        }
                        else
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

                            var pty = db.Plans.Single(c => c.Plancode == 4);
                            var mr = db.Members.Single(m => m.Id == 1);
                            var br = db.Branchtabs.Single(bc => bc.BranchCode == model.branchcode);
                            var cr = db.CompanyInfos.Single(csn => csn.AdminId == br.companyid);
                            int max = (from a in db.appltabs select a).Count();
                            var bondid = max + 1;
                            var newbondid = br.prefix + model.branchcode + "/C0" + bondid;
                            var adr = db.AgentDetails.Single(ad => ad.NewAgentId == model.newintroducerid);
                            //var pr = db.NewPlans.Single(pt => pt.Plancode == model.plancode);

                            var discount = Math.Round(model.payment * (model.discountper / 100), 0);
                            var totaldiscount = Math.Round(model.totalcon * (model.discountper / 100), 0);
                            var payment = model.payment - discount;
                            var totalcon = model.totalcon - totaldiscount;

                            TimeSpan span = model.formdate.Subtract(model.dob);
                            int years = (int)(span.Days / 365.25);
                            int remainder = (int)(span.Days % 365.25);
                            int months = remainder / 30;
                            if (months >= 12)
                            {
                                int y = months / 12;
                                int m = months % 12;
                                months = m;
                                years = years + y;
                            }
                            int days = remainder % 30;
                            var age = years + " years " + months + " months " + days + " days";

                            var row = db.Member_tabs.Single(ui => ui.NewMemberId == model.newmemberid);
                            appltab bd = new appltab();
                            bd.branchcode = model.branchcode;
                            bd.bondid = bondid;
                            bd.newbondid = newbondid;
                            bd.name = model.name;
                            bd.RelationOf = model.RelationOf;
                            bd.fathername = model.fathername;
                            bd.addr = model.addr;
                            bd.mobileno = model.mobileno;
                            bd.introducerid = adr.AgencyCode;
                            bd.intrankcode = adr.RankCode;
                            bd.intrankname = adr.RankName;
                            bd.newintroducerid = model.newintroducerid;
                            bd.formdate = Convert.ToDateTime(model.formdate);
                            bd.dob = model.dob;
                            bd.age = age;
                            bd.nationality = model.nationality;
                            bd.newmemberid = model.newmemberid;
                            bd.memberid = row.MemberId;
                            bd.guardianname = model.guardianname;
                            bd.gurage = model.gurage;
                            bd.gurrel = model.gurrel;
                            bd.guraddr = model.guraddr;

                            if (photo != null)
                            {
                                string imgname = gid();
                                bd.photo = "~/Photo/" + imgname + ".jpg";
                                photo.SaveAs(HttpContext.Server.MapPath("~/Photo/" + imgname + ".jpg"));
                            }
                            else
                            {
                                bd.photo = "~/Photo/default.jpg";

                            }

                            bd.panno = model.panno;
                            bd.bankname = model.bankname;
                            bd.accountno = model.accountno;
                            bd.IFSC = model.IFSC;

                            bd.plantype = model.plantype;
                            bd.planname = model.planname;
                            bd.plancode = 0;
                            bd.bonusper = 0;

                            if (model.plantype == pty.shortcutname)
                            {
                                NumberToEnglish nw = new NumberToEnglish();
                                bd.amountword = nw.changeToWords(totalcon.ToString(), true);

                            }
                            else
                            {
                                bd.amountword = model.amountword;
                            }

                            bd.term = model.term;
                            bd.mode = model.mode;
                            bd.payment = payment;
                            bd.applicationcharge = model.applicationcharge;
                            bd.nolandunit = model.nolandunit;
                            bd.totalcon = totalcon;
                            bd.expectedraisablevalue = model.expectedraisablevalue;
                            bd.expirydate = model.expirydate;

                            bd.nomineename = model.nomineename;
                            bd.nomage = model.nomage;
                            bd.nomrel = model.nomrel;
                            bd.nomaddr = model.nomaddr;

                            bd.paymethod = model.paymethod;
                            bd.pdate = model.formdate;
                            bd.checkorddno = model.checkorddno;
                            bd.drawno = model.drawno;
                            bd.branchpay = model.branchpay;
                            bd.opid = User.Identity.Name;

                            if (model.plantype == pty.shortcutname)
                            {
                                bd.status = 0;
                            }
                            else
                            {
                                bd.status = 1;
                            }

                            bd.type = "Customer";
                            bd.Macaddress = localIP;
                            bd.discountper = model.discountper;
                            bd.bookingamount = model.bookingamount;
                            bd.downpayment = model.downpayment;
                            bd.Time = DateTime.Now.ToShortTimeString();

                            RevivalTab rt = new RevivalTab();
                            rt.newbondid = newbondid;
                            rt.status = 1;
                            db.RevivalTabs.Add(rt);

                            NewLogin nl = new NewLogin();
                            nl.UserName = newbondid;
                            nl.Password = model.mobileno;
                            nl.Mobile = model.mobileno;
                            nl.type = "Customer";
                            nl.status = 1;
                            db.NewLogins.Add(nl);

                            db.appltabs.Add(bd);
                            db.SaveChanges();
                            ViewData["newbondid"] = newbondid;
                            con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                            SqlCommand cmd = new SqlCommand();
                            cmd.CommandText = "InsertAppTab";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = con;

                            cmd.Parameters.AddWithValue("@introducerid", adr.AgencyCode);
                            cmd.Parameters.AddWithValue("@plancode", 0);
                            cmd.Parameters.AddWithValue("@planname", model.planname);
                            cmd.Parameters.AddWithValue("@payment", payment);
                            cmd.Parameters.AddWithValue("@bondid", bondid);
                            cmd.Parameters.AddWithValue("@mode", model.mode);
                            cmd.Parameters.AddWithValue("@formdate", model.formdate);
                            cmd.Parameters.AddWithValue("@pdate", model.pdate);
                            cmd.Parameters.AddWithValue("@term", model.term);
                            cmd.Parameters.AddWithValue("@opid", User.Identity.Name);


                            try
                            {
                                con.Open();
                                cmd.ExecuteNonQuery();
                                MyClass.Sendmsg(model.mobileno, "Dear " + model.name + ", your Loginid:" + newbondid + " Password:" + model.mobileno + " For your A/c details please visit " + cg.HeadOffice);
                                MyClass.Sendmsg(model.mobileno, "Dear Customer your Application no- " + newbondid + " has been activated with Rs. " + payment + " in the plan " + model.planname + "Thank you for your kind believe us for help contact. " + cg.CompanyName);
                                Response.Write("<script>alert('" + mr.custname + " created Successfully....')</script>");
                                ViewBag.msg = mr.custname + " created Successfully....";
                                NewBondid = newbondid;

                            }

                            catch (Exception e)
                            {
                                ViewBag.msg = e.Message;
                            }
                            finally
                            {
                                con.Close();
                            }
                        }
                        break;
                }

            }

            return View(mt);

        }

        [HttpGet]
        public ActionResult Adjustmentbond()
        {
            List<Member_tab> mt = new List<Member_tab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var opr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
                DateTime dt = DateTime.Now.Date;
                var tcss = (from cs in db.Installmenttabs where cs.paymentdate == dt && cs.payamount != 0 && cs.branch == br.BranchCode select cs.cssno).Count();
                var ncss = tcss + 1;
                ViewData["cssno"] = ncss;
                int mmax = (from a in db.appltabs select a).Count();
                mmax = mmax + 1;
                var newbondid = br.prefix + br.BranchCode + "/C0" + mmax;
                ViewData["Abondid"] = newbondid;
                return View(mt);
            }
        }
        [HttpPost]
        public ActionResult Adjustmentbond(appltab model, HttpPostedFileBase photo, string command, string memberid, int noofrenewel = 0)
        {

            List<Member_tab> mt = new List<Member_tab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                switch (command)
                {
                    case "Search":

                        switch (memberid)
                        {
                            case "":
                                Response.Write("<script>alert('Please Select Member Id First')</script>");
                                break;
                            default:

                                var opr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
                                var br = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
                                mt = db.Member_tabs.Where(c => c.NewMemberId == memberid).ToList();
                                DateTime dt = DateTime.Now.Date;
                                var tcss = (from cs in db.Installmenttabs where cs.paymentdate == dt && cs.payamount != 0 && cs.branch == br.BranchCode select cs.cssno).Count();
                                var ncss = tcss + 1;
                                ViewData["cssno"] = ncss;
                                int mmax = (from a in db.appltabs select a).Count();
                                mmax = mmax + 1;
                                var newbondid = br.prefix + br.BranchCode + "/C0" + mmax;
                                ViewData["Abondid"] = newbondid;
                                return View(mt);
                        }

                        break;

                    case "Submit":

                        var ds = db.Blockdates.Where(c => c.date == model.formdate && c.branchcode == model.branchcode && c.status == 0).Count();
                        if (ds > 0)
                        {
                            Response.Write("<script>alert('This Date is closed please select another date')</script>");
                        }
                        else
                        {
                            var mr = db.Members.Single(m => m.Id == 1);
                            var bcount = (from a in db.appltabs where a.newbondid == model.newbondid select a).Count();
                            if (bcount > 0)
                            {
                                Response.Write("<script>alert('This " + mr.custname + " Id Already Exist')</script>");
                            }
                            else
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

                                int max = (from a in db.appltabs select a.bondid).Max();
                                int bondid = max + 1;


                                var adr = db.AgentDetails.Single(ad => ad.NewAgentId == model.newintroducerid);
                                var pr = db.NewPlans.Single(pt => pt.Plancode == model.plancode);

                                appltab bd = new appltab();
                                bd.branchcode = model.branchcode;
                                bd.bondid = bondid;
                                bd.newbondid = model.newbondid;
                                bd.name = model.name;
                                bd.RelationOf = model.RelationOf;
                                bd.fathername = model.fathername;
                                bd.addr = model.addr;
                                bd.mobileno = model.mobileno;
                                bd.introducerid = adr.AgencyCode;
                                bd.intrankcode = adr.RankCode;
                                bd.intrankname = adr.RankName;
                                bd.newintroducerid = model.newintroducerid;
                                bd.formdate = Convert.ToDateTime(model.formdate);

                                bd.age = model.age;
                                bd.dob = DateTime.Now.Date;

                                bd.nationality = model.nationality;
                                bd.newmemberid = model.newmemberid;
                                var row = db.Member_tabs.Single(ui => ui.NewMemberId == model.newmemberid);
                                bd.memberid = row.MemberId;
                                bd.guardianname = model.guardianname;
                                bd.gurage = model.gurage;
                                bd.gurrel = model.gurrel;
                                bd.guraddr = model.guraddr;

                                if (photo != null)
                                {
                                    string imgname = gid();
                                    bd.photo = "~/Photo/" + imgname + ".jpg";
                                    photo.SaveAs(HttpContext.Server.MapPath("~/Photo/" + imgname + ".jpg"));
                                }
                                else
                                {
                                    bd.photo = "~/Photo/default.jpg";
                                }

                                bd.panno = model.panno;
                                bd.bankname = model.bankname;
                                bd.accountno = model.accountno;
                                bd.IFSC = model.IFSC;


                                bd.plantype = model.plantype;
                                bd.planname = pr.Planname;
                                bd.plancode = pr.Plancode;
                                bd.bonusper = 0;
                                bd.term = model.term;
                                bd.mode = model.mode;
                                bd.payment = model.payment;
                                bd.applicationcharge = model.applicationcharge;
                                bd.nolandunit = model.nolandunit;
                                bd.totalcon = model.totalcon;
                                bd.expectedraisablevalue = model.expectedraisablevalue;
                                bd.expirydate = model.expirydate;

                                bd.nomineename = model.nomineename;
                                bd.nomage = model.nomage;
                                bd.nomrel = model.nomrel;
                                bd.nomaddr = model.nomaddr;

                                bd.paymethod = model.paymethod;
                                bd.pdate = Convert.ToDateTime(model.formdate);
                                bd.checkorddno = model.checkorddno;
                                bd.drawno = model.drawno;
                                bd.branchpay = model.branchpay;
                                bd.amountword = model.amountword;
                                bd.opid = User.Identity.Name;
                                bd.status = 1;
                                bd.type = "Adjustment";
                                bd.Macaddress = localIP;
                                bd.Time = DateTime.Now.ToShortTimeString();
                                bd.discountper = model.discountper;
                                bd.bookingamount = model.bookingamount;
                                bd.downpayment = 0;

                                RevivalTab rt = new RevivalTab();
                                rt.newbondid = model.newbondid;
                                rt.status = 1;
                                db.RevivalTabs.Add(rt);

                                NewLogin nl = new NewLogin();
                                nl.UserName = model.newbondid;
                                nl.Password = model.mobileno;
                                nl.Mobile = model.mobileno;
                                nl.type = "Customer";
                                nl.status = 1;
                                db.NewLogins.Add(nl);

                                db.appltabs.Add(bd);
                                db.SaveChanges();

                                ViewData["newbondid"] = model.newbondid;
                                con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                                SqlCommand cmd = new SqlCommand();
                                cmd.CommandText = "InsertAppTabAdjust";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = con;

                                cmd.Parameters.AddWithValue("@introducerid", adr.AgencyCode);
                                cmd.Parameters.AddWithValue("@plancode", pr.Plancode);
                                cmd.Parameters.AddWithValue("@planname", pr.Planname);
                                cmd.Parameters.AddWithValue("@payment", model.payment);
                                cmd.Parameters.AddWithValue("@bondid", bondid);
                                cmd.Parameters.AddWithValue("@mode", model.mode);
                                cmd.Parameters.AddWithValue("@formdate", model.formdate);
                                cmd.Parameters.AddWithValue("@pdate", model.pdate);
                                cmd.Parameters.AddWithValue("@term", model.term);
                                cmd.Parameters.AddWithValue("@opid", User.Identity.Name);

                                try
                                {
                                    con.Open();
                                    cmd.ExecuteNonQuery();
                                    con.Close();
                                    // MyClass.Sendmsg(model.mobileno, "Dear " + model.name + ", your Loginid:" + model.newbondid + " Password:" + model.mobileno + " For your A/c details please visit "+cg.HeadOffice);
                                    // MyClass.Sendmsg(model.mobileno, "Dear Customer your Application no- " + model.newbondid + " has been activated with Rs. " + model.payment + " in the plan " + pr.Planname + "Thank you for your kind believe us for help contact. "+cg.CompanyName);
                                    ViewBag.msg = mr.custname + " Created Successfully....";
                                    ViewData["newbondid"] = model.newbondid;

                                    if (noofrenewel > 0)
                                    {
                                        for (int i = 1; i <= noofrenewel; i++)
                                        {
                                            var cr = db.appltabs.Single(c => c.newbondid == model.newbondid);
                                            SqlCommand cmd2 = new SqlCommand();
                                            cmd2.CommandText = "adjustrenewel";
                                            cmd2.CommandType = CommandType.StoredProcedure;
                                            cmd2.Connection = con;

                                            cmd2.Parameters.AddWithValue("@newbondid", model.newbondid);
                                            cmd2.Parameters.AddWithValue("@planname", cr.planname);
                                            cmd2.Parameters.AddWithValue("@term", cr.term);
                                            cmd2.Parameters.AddWithValue("@mode", cr.mode);
                                            cmd2.Parameters.AddWithValue("@installmentno", i);
                                            cmd2.Parameters.AddWithValue("@paymethod", cr.paymethod);
                                            cmd2.Parameters.AddWithValue("@pdate", cr.pdate);
                                            cmd2.Parameters.AddWithValue("@payamount", cr.payment);
                                            cmd2.Parameters.AddWithValue("@amountinword", cr.amountword);
                                            cmd2.Parameters.AddWithValue("@opid", User.Identity.Name);
                                            cmd2.Parameters.AddWithValue("@macaddress", localIP);
                                            cmd2.Parameters.AddWithValue("@time", DateTime.Now.ToShortTimeString());

                                            try
                                            {
                                                con.Open();
                                                cmd2.ExecuteNonQuery();

                                            }

                                            catch (Exception e)
                                            {
                                                ViewBag.msg = e.Message;
                                            }
                                            finally
                                            {
                                                con.Close();
                                            }
                                        }
                                    }

                                }

                                catch (Exception e)
                                {
                                    ViewBag.msg = e.Message;
                                }
                                finally
                                {
                                    con.Close();
                                }

                            }

                            var aopr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
                            var abr = db.Branchtabs.Single(b => b.BranchCode == aopr.BranchCode);
                            mt = db.Member_tabs.Where(c => c.NewMemberId == memberid).ToList();
                            DateTime adt = DateTime.Now.Date;
                            var atcss = (from cs in db.Installmenttabs where cs.paymentdate == adt && cs.payamount != 0 && cs.branch == abr.BranchCode select cs.cssno).Count();
                            var ancss = atcss + 1;
                            ViewData["cssno"] = ancss;
                            int ammax = (from a in db.appltabs select a).Count();
                            ammax = ammax + 1;
                            var anewbondid = abr.prefix + abr.BranchCode + "/C0" + ammax;
                            ViewData["Abondid"] = anewbondid;
                        }
                        break;
                }
            }

            return View(mt);
        }
        public ActionResult PrintBond(string newbondid, Double discount = 0)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                if (discount == 0)
                {
                    con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "Bond";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@newbondid", newbondid);


                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }

                    catch (Exception e)
                    {
                        ViewBag.msg = e.Message;
                    }
                    finally
                    {
                        con.Close();
                    }

                    List<Bond_report> bond = new List<Bond_report>();
                    bond = db.Bond_reports.ToList();
                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "report.rpt"));
                    rd.SetDataSource(bond);

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
                else
                {
                    con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "TempBond";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@newbondid", newbondid);


                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }

                    catch (Exception e)
                    {
                        ViewBag.msg = e.Message;
                    }
                    finally
                    {
                        con.Close();
                    }

                    List<Bond_report> bond = new List<Bond_report>();
                    bond = db.Bond_reports.ToList();
                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "report.rpt"));
                    rd.SetDataSource(bond);

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
            }
        }
        public ActionResult PrintReceipt(string newbondid)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Branch");
            }
            else
            {
                List<tempcheque> bond = new List<tempcheque>();
                if (BondType == "Discount")
                {
                    var chequeamount = "";
                    var chequedate = "";
                    var chequeno = "";
                    var ta = db.tempappltabs.Single(t => t.newbondid == newbondid);
                    if (ta.paymethod == "Cheque")
                    {
                        chequeamount = "Amount of Cheque (Rs.)";
                        chequedate = "Date of Cheque";
                        chequeno = "Cheque Number";
                    }
                    else if (ta.paymethod == "banktransaction")
                    {
                        chequeamount = "Amount of transaction (Rs.)";
                        chequedate = "Date of transaction";
                        chequeno = "Transaction Number";
                    }
                    bond.Add(new tempcheque { colChequedate = chequedate, colchequeno = chequeno, colChequeAmount = chequeamount, newbondid = ta.newbondid, name = ta.name, nolandunit = ta.nolandunit, opid = ta.opid, bookingamount = ta.bookingamount, bank = ta.bank, Account = ta.Account, chequeno = ta.chequeno, ACholdername = ta.ACholdername, Branch = ta.Branch, IFSCCode = ta.IFSCCode, ChequeAmount = ta.ChequeAmount, Chequedate = ta.Chequedate, Chequeimage = ta.Chequeimage });
                }
                else
                {
                    var ta = db.appltabs.Single(t => t.newbondid == newbondid);
                    bond.Add(new tempcheque { newbondid = ta.newbondid, name = ta.name, nolandunit = ta.nolandunit, opid = ta.opid, bookingamount = ta.bookingamount, bank = ta.bank, Account = ta.Account, chequeno = ta.chequeno, ACholdername = ta.ACholdername, Branch = ta.Branch, IFSCCode = ta.IFSCCode, ChequeAmount = ta.ChequeAmount, Chequedate = ta.Chequedate, Chequeimage = ta.Chequeimage });
                }

                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "MasterAckReceipt.rpt"));
                rd.SetDataSource(bond);

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
        }

        [HttpGet]
        public ActionResult AddExpense()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {


                return View();
            }
        }
        [HttpPost]
        public ActionResult AddExpense(Expense ep, string head, string remarks, string trbank, string transactionid, string transactiondate, string tramount, string trpmethod, string holderacno, string ddno, HttpPostedFileBase Chequeimage, DateTime date, Double amount = 0, Double DDamount = 0, int type = 0)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var br = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                var ds = db.Blockdates.Where(c => c.date == date && c.branchcode == br.BranchCode && c.status == 0).Count();
                if (ds > 0)
                {
                    Response.Write("<script>alert('This Date is closed please select another date')</script>");
                }
                else
                {

                    Expense ob = new Expense();
                    ob.head = head;
                    ob.Remark = remarks;
                    ob.amount = amount;
                    ob.date_time = date;
                    ob.branchcode = br.BranchCode;
                    ob.opid = User.Identity.Name;
                    ob.type = type;
                    ob.paymethod = ep.paymethod;
                    if (ob.paymethod == "Cheque")
                    {
                        ob.bank = ep.bank;
                        ob.Account = ep.Account;
                        ob.chequeno = ep.chequeno;
                        ob.ACholdername = ep.ACholdername;
                        ob.Branch = ep.Branch;
                        ob.IFSCCode = ep.IFSCCode;
                        ob.ChequeAmount = ep.ChequeAmount;
                        ob.Chequedate = ep.Chequedate;
                        ob.transactiontype = "NA";

                        if (Chequeimage != null)
                        {
                            string img = gid();
                            ob.Chequeimage = "~/Photo/" + img + ".jpg";
                            Chequeimage.SaveAs(HttpContext.Server.MapPath("~/Photo/" + img + ".jpg"));
                        }
                        else
                        {
                            ob.Chequeimage = "~/Photo/default.jpg";

                        }
                    }
                    else if (ob.paymethod == "banktransaction")
                    {
                        ob.bank = trbank;
                        ob.Account = holderacno;
                        ob.chequeno = transactionid;
                        ob.ACholdername = "NA";
                        ob.Branch = "NA";
                        ob.IFSCCode = "NA";
                        ob.ChequeAmount = tramount;
                        ob.Chequedate = Convert.ToDateTime(transactiondate);
                        ob.Chequeimage = "~/Photo/default.jpg";
                        ob.transactiontype = ep.transactiontype;
                    }
                    else if (ob.paymethod == "DD")
                    {
                        ob.bank = "NA";
                        ob.Account = "NA";
                        ob.chequeno = ddno;
                        ob.ACholdername = "NA";
                        ob.Branch = "NA";
                        ob.IFSCCode = "NA";
                        ob.ChequeAmount = DDamount.ToString();
                        ob.Chequedate = DateTime.Now.Date;
                        ob.Chequeimage = "~/Photo/default.jpg";
                        ob.transactiontype = "NA";
                    }
                    else if (ob.paymethod == "Cash")
                    {
                        ob.bank = "NA";
                        ob.Account = "NA";
                        ob.chequeno = "NA";
                        ob.ACholdername = "NA";
                        ob.Branch = "NA";
                        ob.IFSCCode = "NA";
                        ob.ChequeAmount = amount.ToString();
                        ob.Chequedate = DateTime.Now.Date;
                        ob.Chequeimage = "~/Photo/default.jpg";
                        ob.transactiontype = "NA";
                    }

                    db.Expenses.Add(ob);
                    db.SaveChanges();
                    Response.Write("<script>alert('Expense added Successfully')</script>");
                }
                return View();
            }
        }

        [HttpGet]
        public ActionResult PrintId()
        {
            List<Icard> ic = new List<Icard>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(ic);
            }
        }
        [HttpPost]
        public ActionResult PrintId(string agentid)
        {
            List<Icard> ic = new List<Icard>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var br = db.Operators.Single(ob => ob.OperatorId == User.Identity.Name);
                var agcount = db.AgentDetails.Where(agc => agc.NewAgentId == agentid && agc.BranchCode == br.BranchCode).Count();

                if (agcount > 0)
                {
                    var bname = db.Members.Single(m => m.Id == 1);
                    var com = db.CompanyInfos.Single(co => co.Id == 1);
                    var item = db.AgentDetails.Single(nic => nic.NewAgentId == agentid);
                    int age = DateTime.Now.Year - item.Dob.Year;
                    if (item.AgencyCode > 1)
                    {
                        var brch = db.Branchtabs.Single(bi => bi.BranchCode == item.BranchCode);
                        ic.Add(new Icard { newagentid = item.NewAgentId, name = item.name, newintroducerid = item.NewIntroducerId, age = age, address = item.Address, branch = brch.BranchName, btype = bname.branchname, companyname = com.CompanyName, comaddress = com.Address, comcontact = com.Contact, dob = item.Dob, fname = item.Father, rank = item.RankName, issuedate = item.Issueon, validdate = item.Validupto, state = item.State, dist = item.District, pin = item.PinCode, mobileno = item.Mobileno });
                    }
                    else
                    {
                        ic.Add(new Icard { newagentid = item.NewAgentId, name = item.name, newintroducerid = item.NewIntroducerId, age = age, address = item.Address, branch = "Head Office", btype = bname.branchname, companyname = com.CompanyName, comaddress = com.Address, comcontact = com.Contact, dob = item.Dob, fname = item.Father, rank = item.RankName, issuedate = item.Issueon, validdate = item.Validupto, state = item.State, dist = item.District, pin = item.PinCode, mobileno = item.Mobileno });
                    }

                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "IdCard.rpt"));
                    rd.SetDataSource(ic);

                    Response.Buffer = false;
                    Response.ClearContent();
                    Response.ClearHeaders();


                    try
                    {
                        Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                        stream.Seek(0, SeekOrigin.Begin);
                        string dt = pdate.ToShortDateString();

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
                }

                else
                {
                    Response.Write("<script>alert('Please Enter a Id That is valid Or Related To this Branch')</script>");
                }
                return View(ic);
            }
        }

        #endregion

        #region Passbook Menu Start Here

        [HttpGet]
        public ActionResult PrintSavingPassBookDetail()
        {
            List<SavingAccountInfo> pass = new List<SavingAccountInfo>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(pass);
            }
        }
        [HttpPost]
        public ActionResult PrintSavingPassBookDetail(string accno)
        {
            List<SavingAccountInfo> pass = new List<SavingAccountInfo>();
            var count = db.SavingAccountInfos.Where(c => c.accountno == accno).Count();
            if (count == 1)
            {
                var pass1 = db.SavingAccountInfos.Where(b => b.accountno == accno).ToList();
                return View(pass1);
            }
            else
            {
                ViewBag.msg = "This Account No. Does Not Exist Or Account FD";
                return View(pass);

            }
        }
        public ActionResult PDFPrintSavingPassBookDetail(string accno)
        {
            List<SavingAccountInfo> pass = new List<SavingAccountInfo>();
            var count = db.SavingAccountInfos.Where(c => c.accountno == accno).Count();
            if (count == 1)
            {
                var pass1 = db.SavingAccountInfos.Where(b => b.accountno == accno).ToList();
                var type = db.SavingAccountInfos.Single(d => d.accountno == accno);

                ReportDocument rd = new ReportDocument();
                if (type.actype == "Self")
                {
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "savingdetail.rpt"));
                }
                else
                {
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "jointdetail.rpt"));
                }
                rd.SetDataSource(pass1);


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


            }
            return View();
        }


        [HttpGet]
        public ActionResult SavingTransiction()
        {
            List<printpassbook> print = new List<printpassbook>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }

            return View(print);
        }
        [HttpPost]
        public ActionResult SavingTransiction(string accountno, int lastsr)
        {
            List<printpassbook> print = new List<printpassbook>();
            var count = db.TransactionTabs.Where(c => c.accountno == accountno && c.status <= lastsr).Count();
            while (count > 0)
            {
                print.Add(new printpassbook { sr = null, trandate = null, remark = null, tranid = null, mode = null, status = null, installno = null, widthdraw = null, diposit = null, balance = null });

                count = count - 1;
            }
            var install = db.TransactionTabs.Where(b => b.accountno == accountno && b.status > lastsr).ToList();
            foreach (var r in install)
            {
                //var check = db.TransactionTabs.Where(s => s.accountno == accountno && s.status == r.status  && s.transactionid == r.transactionid).Count();
                //if (check == 1)
                //{
                //var balance = (from o in db.DailyInstallments where o.installmentno <= r.installmentno && o.payamount != 0 && o.newbondid == newbond select o.payamount).Sum();
                //var detail = db.DailyInstallments.Single(p => p.receiptno == r.receiptno && p.newbondid == newbond && p.installmentno == r.installmentno);
                print.Add(new printpassbook { sr = r.status.ToString(), remark = r.remark, trandate = r.pdate.ToString("dd/MM/yyyy"), tranid = r.transactionid, mode = r.paymethod, status = r.status.ToString(), installno = r.status.ToString(), widthdraw = r.debit.ToString(), diposit = r.credit.ToString(), balance = r.balance.ToString() });
                //}
            }
            acno = accountno;
            ltsr = lastsr;
            return View(print);
        }
        public ActionResult PDFSavingTransiction()
        {
            List<printpassbook> print = new List<printpassbook>();
            var count = db.TransactionTabs.Where(c => c.accountno == acno && c.status <= ltsr).Count();
            while (count > 0)
            {
                print.Add(new printpassbook { sr = null, trandate = null, tranid = null, mode = null, status = null, installno = null, widthdraw = null, diposit = null, balance = null });

                count = count - 1;
            }
            var install = db.TransactionTabs.Where(b => b.accountno == acno && b.status > ltsr).ToList();
            foreach (var r in install)
            {
                print.Add(new printpassbook { sr = r.status.ToString(), trandate = r.pdate.ToString("dd/MM/yyyy"), tranid = r.transactionid, mode = r.paymethod, status = "1", installno = r.status.ToString(), widthdraw = r.debit.ToString(), diposit = r.credit.ToString(), balance = r.balance.ToString() });

            }
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Savingtransiction.rpt"));
            rd.SetDataSource(print);

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
        public ActionResult PrintPassBookDetail()
        {
            List<appltab> pass = new List<appltab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(pass);
            }
        }
        [HttpPost]
        public ActionResult PrintPassBookDetail(string newbondid)
        {
            List<appltab> pass = new List<appltab>();
            var count = db.appltabs.Where(c => c.newbondid == newbondid && c.plantype == "RD").Count();
            if (count == 1)
            {
                var pass1 = db.appltabs.Where(b => b.newbondid == newbondid && b.plantype == "RD").ToList();
                return View(pass1);
            }
            else
            {
                ViewBag.msg = "This Account No. Does Not Exist Or Account FD";
                return View(pass);

            }


        }
        public ActionResult PDFPrintPassBookDetail(string newbondid)
        {
            List<appltab> pass = new List<appltab>();
            var count = db.appltabs.Where(c => c.newbondid == newbondid && c.plantype == "RD").Count();
            if (count == 1)
            {
                var pass1 = db.appltabs.Where(b => b.newbondid == newbondid && b.plantype == "RD").ToList();
                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "RdDetail.rpt"));
                rd.SetDataSource(pass1);

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


            }
            return View();

        }


        [HttpGet]
        public ActionResult PrintPassBookColumn()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View();
            }
        }

        public ActionResult PDFPrintSavingColumn()
        {


            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "columndetail.rpt"));


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


        public ActionResult PDFPrintRDColumn()
        {
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "RDcolumndetail.rpt"));


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
        public ActionResult PrintPassBook()
        {
            List<printpassbook> print = new List<printpassbook>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }

            return View(print);
        }

        [HttpPost]
        public ActionResult PrintPassBook(string newbond, int lastsr)
        {
            List<printpassbook> print = new List<printpassbook>();
            int days = 0;
            var a = db.appltabs.Where(f => f.newbondid == newbond && f.plantype == "RD").Count();
            if (a == 1)
            {
                var bb = db.appltabs.Single(p => p.newbondid == newbond);

                var count = db.Installmenttabs.Where(c => c.newbondid == newbond && c.payamount != 0 && c.installmentno <= lastsr).Count();
                while (count > 0)
                {
                    print.Add(new printpassbook { sr = null, lateday = null, trandate = null, tranid = null, mode = null, status = null, installno = null, widthdraw = null, diposit = null, balance = null });

                    count = count - 1;
                }
                var install = db.Installmenttabs.Where(b => b.newbondid == newbond && b.payamount != 0 && b.installmentno > lastsr).ToList();
                foreach (var r in install)
                {
                    var check = db.Installmenttabs.Where(s => s.newbondid == newbond && s.installmentno == r.installmentno && s.payamount != 0 && s.receiptno == r.receiptno).Count();
                    if (check == 1)
                    {
                        if (r.payamount != 0)
                        {
                            if (r.paymentdate > r.prevexpirydate)
                            {
                                DateTime dt1 = Convert.ToDateTime(r.paymentdate);
                                DateTime dt2 = r.prevexpirydate;
                                TimeSpan ts = dt1.Subtract(dt2);
                                days = ts.Days;
                            }
                            else
                            {
                                days = 0;
                            }
                        }
                        else
                        {
                            days = 0;
                        }
                        var balance = (from o in db.Installmenttabs where o.installmentno <= r.installmentno && o.payamount != 0 && o.newbondid == newbond select o.payamount).Sum();
                        var detail = db.Installmenttabs.Single(p => p.receiptno == r.receiptno && p.newbondid == newbond && p.installmentno == r.installmentno);
                        print.Add(new printpassbook { lateday = days.ToString(), sr = detail.installmentno.ToString(), trandate = detail.paymentdate.Value.ToString("dd/MM/yyyy"), tranid = detail.receiptno, mode = detail.mode, status = "1", installno = detail.installmentno.ToString(), widthdraw = detail.reliefrs.ToString(), latefine = detail.latefine.ToString(), diposit = detail.payamount.ToString(), balance = balance.ToString() });
                    }
                }
                bond = newbond;
                lstsr = lastsr;


            }
            else
            {
                ViewBag.msg = "Please Check Account No or Account is FD Type";
            }
            return View(print);
        }

        public ActionResult PDFPrintPassBook()
        {
            int days = 0;
            List<printpassbook> print = new List<printpassbook>();
            var bb = db.appltabs.Single(p => p.newbondid == bond);


            var count = db.Installmenttabs.Where(c => c.newbondid == bond && c.payamount != 0 && c.installmentno <= lstsr).Count();
            while (count > 0)
            {
                print.Add(new printpassbook { sr = null, lateday = null, trandate = null, tranid = null, mode = null, status = null, installno = null, widthdraw = null, diposit = null, balance = null });

                count = count - 1;
            }
            var install = db.Installmenttabs.Where(b => b.newbondid == bond && b.payamount != 0 && b.installmentno > lstsr).ToList();
            foreach (var r in install)
            {
                var check = db.Installmenttabs.Where(s => s.newbondid == bond && s.installmentno == r.installmentno && s.payamount != 0 && s.receiptno == r.receiptno).Count();
                if (check == 1)
                {
                    if (r.payamount != 0)
                    {
                        if (r.paymentdate > r.prevexpirydate)
                        {
                            DateTime dt1 = Convert.ToDateTime(r.paymentdate);
                            DateTime dt2 = r.prevexpirydate;
                            TimeSpan ts = dt1.Subtract(dt2);
                            days = ts.Days;
                        }
                        else
                        {
                            days = 0;
                        }
                    }
                    else
                    {
                        days = 0;
                    }
                    var balance = (from o in db.Installmenttabs where o.installmentno <= r.installmentno && o.payamount != 0 && o.newbondid == bond select o.payamount).Sum();
                    var detail = db.Installmenttabs.Single(p => p.receiptno == r.receiptno && p.newbondid == bond && p.installmentno == r.installmentno);
                    print.Add(new printpassbook { lateday = days.ToString(), sr = detail.installmentno.ToString(), trandate = detail.paymentdate.Value.ToString("dd/MM/yyyy"), tranid = detail.receiptno, mode = detail.mode, status = "1", installno = detail.installmentno.ToString(), widthdraw = detail.reliefrs.ToString(), latefine = detail.latefine.ToString(), diposit = detail.payamount.ToString(), balance = balance.ToString() });
                }
            }

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "transiction.rpt"));
            rd.SetDataSource(print);

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


        #endregion Passbook Menu End Here

        #region Upload Menu Start Here

        [HttpGet]
        public ActionResult UploadDocument()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }

            else
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult UploadDocument(string memberid, string information, string type, HttpPostedFileBase document)
        {
            if (ModelState.IsValid)
            {
                if (type == "Broker")
                {
                    type = "Agent";
                }
                DocumentTab dt = new DocumentTab();
                dt.memberid = memberid;
                dt.information = information;
                dt.type = type;
                if (document != null)
                {
                    string imgname = gid();
                    dt.document = "~/Document/" + imgname + ".jpg";
                    document.SaveAs(HttpContext.Server.MapPath("~/Document/") + imgname + ".jpg");
                }

                db.DocumentTabs.Add(dt);
                db.SaveChanges();
                ViewBag.msg = "Document successfully uploaded...";
            }
            return View();
        }

        #endregion

        #region Payment Start Here

        [HttpGet]
        public ActionResult PaymentRenewal()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                DateTime maxdate1 = (from a in db.DailyDepositTabs where a.opid == User.Identity.Name select a.date).DefaultIfEmpty().Max();
                int trid = (from a in db.DailyDepositTabs where a.opid == User.Identity.Name && a.date == maxdate1 select a.trid).DefaultIfEmpty().Max();
                DateTime maxdate = (from a in db.DailyDepositTabs where a.opid == User.Identity.Name && a.trid == trid select a.date).DefaultIfEmpty().Max();
                var clcount = (from c in db.ClosingMatchingTabs where c.date == maxdate && c.opid == User.Identity.Name && c.pagename == "PaymentRenewal" select c).Count();
                int dcount = (from a in db.DailyDepositTabs where a.opid == User.Identity.Name && a.trid == trid && a.date == maxdate select a).Count();
                if (dcount > 0)
                {
                    if (clcount == 0)
                    {
                        TempData["pagename"] = "PaymentRenewal";
                        return RedirectToAction("MatchingDailyClosing", "Operator");
                    }
                }
                return View();
            }
        }
        [HttpPost]
        public ActionResult PaymentRenewal(TempInstallmenttab tp, HttpPostedFileBase Chequeimage, string newbondid2, string planname, Double term, string mode, Double latefine, int from, int to, string paymethod, DateTime pdate, string checkorddno, string drawno, Double relief, Double payamount, string amountinword, string trbank, string transactionid, string transactiondate, string tramount, string trpmethod, string holderacno, DateTime prevexpirydate)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var tempcount = db.TempInstallmenttabs.Where(c => c.newbondid == newbondid2 && c.status == 1).Count();
                var ds = db.Blockdates.Where(c => c.date == pdate && c.branchcode == User.Identity.Name && c.status == 0).Count();
                if (ds > 0)
                {
                    Response.Write("<script>alert('This Date is closed please select another date')</script>");
                }
                else
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
                    if (tempcount == 0)
                    {
                        if (paymethod == "Cheque" || paymethod == "banktransaction")
                        {
                            if (paymethod == "Cheque" && (tp.chequeno == null || tp.Chequedate == null || tp.ChequeAmount == null || tp.Bbranch == null || tp.bank == null || tp.Account == null || tp.Account == null || tp.IFSCCode == null))
                            {
                                Response.Write("<script>alert('Please Complete Cheque Detail')</script>");
                            }
                            else
                            {
                                var cr = db.appltabs.Single(c => c.newbondid == newbondid2);
                                con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                                SqlCommand cmd = new SqlCommand();
                                cmd.CommandText = "tempinsertrcptinfo";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = con;

                                cmd.Parameters.AddWithValue("@newbondid", newbondid2);
                                cmd.Parameters.AddWithValue("@planname", planname);
                                cmd.Parameters.AddWithValue("@term", term);
                                cmd.Parameters.AddWithValue("@mode", mode);
                                cmd.Parameters.AddWithValue("@latefine", latefine);
                                cmd.Parameters.AddWithValue("@from", from);
                                cmd.Parameters.AddWithValue("@to", to);
                                cmd.Parameters.AddWithValue("@paymethod", paymethod);
                                cmd.Parameters.AddWithValue("@pdate", pdate);
                                cmd.Parameters.AddWithValue("@checkorddno", checkorddno);
                                cmd.Parameters.AddWithValue("@drawno", drawno);
                                cmd.Parameters.AddWithValue("@relief", relief);
                                cmd.Parameters.AddWithValue("@payamount", (payamount) - (latefine - relief));
                                cmd.Parameters.AddWithValue("@amountinword", amountinword);
                                cmd.Parameters.AddWithValue("@opid", User.Identity.Name);

                                cmd.Parameters.AddWithValue("@macaddress", localIP);
                                if (paymethod == "Cheque")
                                {
                                    cmd.Parameters.AddWithValue("@Chequeno", tp.chequeno);
                                    cmd.Parameters.AddWithValue("@Account", tp.Account);
                                    cmd.Parameters.AddWithValue("@acholdername", tp.ACholdername);
                                    cmd.Parameters.AddWithValue("@bank", tp.bank);
                                    cmd.Parameters.AddWithValue("@branch", tp.Bbranch);
                                    cmd.Parameters.AddWithValue("@chequedate", tp.Chequedate);
                                    if (Chequeimage != null)
                                    {

                                        string imgname = gid();
                                        //  tp.Chequeimage = "~/Photo/" + imgname + ".jpg";
                                        Chequeimage.SaveAs(HttpContext.Server.MapPath("~/Photo/" + imgname + ".jpg"));
                                        cmd.Parameters.AddWithValue("@chequeimg", "~/Photo/" + imgname + ".jpg");
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue("@chequeimg", "~/Photo/default.jpg");
                                    }
                                    cmd.Parameters.AddWithValue("@chequeamount", tp.ChequeAmount);
                                    cmd.Parameters.AddWithValue("@time", DateTime.Now.ToShortTimeString());
                                    cmd.Parameters.AddWithValue("@IFSCCode", tp.IFSCCode);
                                    cmd.Parameters.AddWithValue("@transactiontype", "NA");
                                }

                                else if (paymethod == "banktransaction")
                                {
                                    cmd.Parameters.AddWithValue("@Chequeno", transactionid);
                                    cmd.Parameters.AddWithValue("@Account", holderacno);
                                    cmd.Parameters.AddWithValue("@acholdername", "NA");
                                    cmd.Parameters.AddWithValue("@bank", trbank);
                                    cmd.Parameters.AddWithValue("@branch", "NA");
                                    cmd.Parameters.AddWithValue("@chequedate", transactiondate);
                                    cmd.Parameters.AddWithValue("@chequeimg", "~/Photo/default.jpg");
                                    cmd.Parameters.AddWithValue("@chequeamount", tramount);
                                    cmd.Parameters.AddWithValue("@time", DateTime.Now.ToShortTimeString());
                                    cmd.Parameters.AddWithValue("@IFSCCode", "NA");
                                    cmd.Parameters.AddWithValue("@transactiontype", tp.transactiontype);
                                }

                                SqlParameter p = new SqlParameter("@paymentno", SqlDbType.Int);
                                p.Direction = ParameterDirection.Output;
                                cmd.Parameters.Add(p);

                                try
                                {
                                    con.Open();
                                    cmd.ExecuteNonQuery();

                                    paymentno = Convert.ToInt32(cmd.Parameters["@paymentno"].Value.ToString());
                                    ViewData["newbondid"] = newbondid2;
                                    ViewData["paymethod"] = paymethod;
                                    ViewData["paymentno"] = paymentno;

                                    ViewBag.msg = "Renewel pending...";
                                    return View();
                                }

                                catch (Exception e)
                                {
                                    ViewBag.msg = e.Message;
                                }
                                finally
                                {
                                    con.Close();
                                }
                            }
                        }

                        else
                        {

                            var cr = db.appltabs.Single(c => c.newbondid == newbondid2);
                            con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                            SqlCommand cmd = new SqlCommand();
                            cmd.CommandText = "insertrcptinfo";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = con;
                            var Payamount = (payamount) - (latefine - relief);
                            cmd.Parameters.AddWithValue("@newbondid", newbondid2);
                            cmd.Parameters.AddWithValue("@planname", planname);
                            cmd.Parameters.AddWithValue("@term", term);
                            cmd.Parameters.AddWithValue("@mode", mode);
                            cmd.Parameters.AddWithValue("@latefine", latefine);
                            cmd.Parameters.AddWithValue("@from", from);
                            cmd.Parameters.AddWithValue("@to", to);
                            cmd.Parameters.AddWithValue("@paymethod", paymethod);
                            cmd.Parameters.AddWithValue("@pdate", pdate);
                            cmd.Parameters.AddWithValue("@checkorddno", checkorddno);
                            cmd.Parameters.AddWithValue("@drawno", drawno);
                            cmd.Parameters.AddWithValue("@relief", relief);
                            cmd.Parameters.AddWithValue("@payamount", Payamount);
                            cmd.Parameters.AddWithValue("@amountinword", amountinword);
                            cmd.Parameters.AddWithValue("@opid", User.Identity.Name);
                            cmd.Parameters.AddWithValue("@macaddress", localIP);
                            cmd.Parameters.AddWithValue("@time", DateTime.Now.ToShortTimeString());
                            cmd.Parameters.AddWithValue("@transactiontype", "NA");

                            SqlParameter p = new SqlParameter("@paymentno", SqlDbType.Int);
                            p.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(p);

                            try
                            {
                                con.Open();
                                cmd.ExecuteNonQuery();
                                paymentno = Convert.ToInt32(cmd.Parameters["@paymentno"].Value.ToString());
                                ViewData["newbondid"] = newbondid2;
                                ViewData["paymethod"] = paymethod;
                                ViewData["paymentno"] = paymentno;
                                tp.chequeno = tp.chequeno == null ? "" : "no. " + tp.chequeno;
                                var cm = db.CompanyInfos.Single(d => d.Id == 1);
                                //MyClass.Sendmsg(cr.mobileno, "Dear " + cr.name + ",Your renewal of Rs " + payamount + " is received for plan " + planname + " for Bond: " + newbondid2 + " on date: " + pdate.ToString("dd/MM/yyyy"));
                                //MyClass.Sendmsg(cr.mobileno, "Dear Customer " + cr.name + ", Your EMI of Rs." + payamount + " for the month " + prevexpirydate.ToString("MMMMMMMMMM") + " for loan a/c no. " + cr.loanid + " vide " + paymethod + " " + tp.chequeno + " has been deposited successfully. Regards- " + cm.CompanyName);
                                string Paymethod = "", chequeno = "";
                                if (paymethod == "banktransaction")
                                {
                                    Paymethod = "Bank Transaction" + "/" + tp.transactiontype;
                                    chequeno = "Transaction Id " + tp.chequeno;
                                }
                                else if (paymethod != "Cash")
                                {
                                    chequeno = paymethod + " No " + tp.chequeno;
                                    Paymethod = paymethod;
                                }
                                else
                                {
                                    Paymethod = paymethod;
                                }
                                MyClass.Sendmsg(cr.mobileno, "Dear " + cr.name + ", Your EMI of Rs." + payamount + " for the month " + pdate.ToString("MMMM") + " for loan a/c no. " + cr.loanid + " vide " + Paymethod + " " + chequeno + " has been deposited successfully. Regards- " + cg.CompanyName);

                                ViewBag.msg = "Renewel Done Successfully....";
                                return View();
                            }

                            catch (Exception e)
                            {
                                ViewBag.msg = e.Message;
                            }
                            finally
                            {
                                con.Close();
                            }
                        }
                    }
                    else
                    {
                        Response.Write("<script>alert('Your Last Renewal is pending Please Contact to Admin')</script>");
                    }



                }

            }

            return View();
        }

        [HttpGet]
        public ActionResult PartPayment()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                return View();
            }
        }
        [HttpPost]
        public ActionResult PartPayment(TempInstallmenttab tp, HttpPostedFileBase Chequeimage, string newbondid2, string planname, Double term, string mode, Double latefine, int from, string paymethod, DateTime pdate, string checkorddno, string drawno, Double payamount, string amountinword, string trbank, string transactionid, string transactiondate, string tramount, string trpmethod, string holderacno)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var tempcount = db.TempInstallmenttabs.Where(c => c.newbondid == newbondid2 && c.status == 1).Count();
                var ds = db.Blockdates.Where(c => c.date == pdate && c.branchcode == User.Identity.Name && c.status == 0).Count();
                if (ds > 0)
                {
                    Response.Write("<script>alert('This Date is closed please select another date')</script>");
                }
                else
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
                    if (tempcount == 0)
                    {
                        if (paymethod == "Cheque" || paymethod == "banktransaction")
                        {
                            if (paymethod == "Cheque" && (tp.chequeno == null || tp.Chequedate == null || tp.ChequeAmount == null || tp.Bbranch == null || tp.bank == null || tp.Account == null || tp.Account == null || tp.IFSCCode == null))
                            {
                                Response.Write("<script>alert('Please Complete Cheque Detail')</script>");
                            }
                            else
                            {
                                var cr = db.appltabs.Single(c => c.newbondid == newbondid2);
                                con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                                SqlCommand cmd = new SqlCommand();
                                cmd.CommandText = "tempinsertrcptinfo";
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Connection = con;

                                cmd.Parameters.AddWithValue("@newbondid", newbondid2);
                                cmd.Parameters.AddWithValue("@planname", planname);
                                cmd.Parameters.AddWithValue("@term", term);
                                cmd.Parameters.AddWithValue("@mode", mode);
                                cmd.Parameters.AddWithValue("@latefine", latefine);
                                cmd.Parameters.AddWithValue("@from", from);
                                cmd.Parameters.AddWithValue("@to", from);
                                cmd.Parameters.AddWithValue("@paymethod", paymethod);
                                cmd.Parameters.AddWithValue("@pdate", pdate);
                                cmd.Parameters.AddWithValue("@checkorddno", checkorddno);
                                cmd.Parameters.AddWithValue("@drawno", drawno);
                                cmd.Parameters.AddWithValue("@relief", 0);
                                cmd.Parameters.AddWithValue("@payamount", payamount - latefine);
                                cmd.Parameters.AddWithValue("@amountinword", amountinword);
                                cmd.Parameters.AddWithValue("@opid", User.Identity.Name);

                                cmd.Parameters.AddWithValue("@macaddress", localIP);
                                if (paymethod == "Cheque")
                                {
                                    cmd.Parameters.AddWithValue("@Chequeno", tp.chequeno);
                                    cmd.Parameters.AddWithValue("@Account", tp.Account);
                                    cmd.Parameters.AddWithValue("@acholdername", tp.ACholdername);
                                    cmd.Parameters.AddWithValue("@bank", tp.bank);
                                    cmd.Parameters.AddWithValue("@branch", tp.Bbranch);
                                    cmd.Parameters.AddWithValue("@chequedate", tp.Chequedate);
                                    if (Chequeimage != null)
                                    {

                                        string imgname = gid();
                                        //  tp.Chequeimage = "~/Photo/" + imgname + ".jpg";
                                        Chequeimage.SaveAs(HttpContext.Server.MapPath("~/Photo/" + imgname + ".jpg"));
                                        cmd.Parameters.AddWithValue("@chequeimg", "~/Photo/" + imgname + ".jpg");
                                    }
                                    else
                                    {
                                        cmd.Parameters.AddWithValue("@chequeimg", "~/Photo/default.jpg");
                                    }
                                    cmd.Parameters.AddWithValue("@chequeamount", tp.ChequeAmount);
                                    cmd.Parameters.AddWithValue("@time", DateTime.Now.ToShortTimeString());
                                    cmd.Parameters.AddWithValue("@IFSCCode", tp.IFSCCode);
                                    cmd.Parameters.AddWithValue("@transactiontype", "NA");
                                }

                                else if (paymethod == "banktransaction")
                                {
                                    cmd.Parameters.AddWithValue("@Chequeno", transactionid);
                                    cmd.Parameters.AddWithValue("@Account", holderacno);
                                    cmd.Parameters.AddWithValue("@acholdername", "NA");
                                    cmd.Parameters.AddWithValue("@bank", trbank);
                                    cmd.Parameters.AddWithValue("@branch", "NA");
                                    cmd.Parameters.AddWithValue("@chequedate", transactiondate);
                                    cmd.Parameters.AddWithValue("@chequeimg", "~/Photo/default.jpg");
                                    cmd.Parameters.AddWithValue("@chequeamount", tramount);
                                    cmd.Parameters.AddWithValue("@time", DateTime.Now.ToShortTimeString());
                                    cmd.Parameters.AddWithValue("@IFSCCode", "NA");
                                    cmd.Parameters.AddWithValue("@transactiontype", tp.transactiontype);
                                }

                                SqlParameter p = new SqlParameter("@paymentno", SqlDbType.Int);
                                p.Direction = ParameterDirection.Output;
                                cmd.Parameters.Add(p);

                                try
                                {
                                    con.Open();
                                    cmd.ExecuteNonQuery();

                                    paymentno = Convert.ToInt32(cmd.Parameters["@paymentno"].Value.ToString());
                                    ViewData["newbondid"] = newbondid2;
                                    ViewData["paymethod"] = paymethod;
                                    ViewData["paymentno"] = paymentno;

                                    ViewBag.msg = "Renewel pending...";
                                    return View();
                                }

                                catch (Exception e)
                                {
                                    ViewBag.msg = e.Message;
                                }
                                finally
                                {
                                    con.Close();
                                }
                            }
                        }

                        else
                        {

                            var cr = db.appltabs.Single(c => c.newbondid == newbondid2);
                            con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                            SqlCommand cmd = new SqlCommand();
                            cmd.CommandText = "insertrcptinfo";
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Connection = con;
                            var Payamount = payamount - latefine;
                            cmd.Parameters.AddWithValue("@newbondid", newbondid2);
                            cmd.Parameters.AddWithValue("@planname", planname);
                            cmd.Parameters.AddWithValue("@term", term);
                            cmd.Parameters.AddWithValue("@mode", mode);
                            cmd.Parameters.AddWithValue("@latefine", latefine);
                            cmd.Parameters.AddWithValue("@from", from);
                            cmd.Parameters.AddWithValue("@to", from);
                            cmd.Parameters.AddWithValue("@paymethod", paymethod);
                            cmd.Parameters.AddWithValue("@pdate", pdate);
                            cmd.Parameters.AddWithValue("@checkorddno", checkorddno);
                            cmd.Parameters.AddWithValue("@drawno", drawno);
                            cmd.Parameters.AddWithValue("@relief", 0);
                            cmd.Parameters.AddWithValue("@payamount", Payamount);
                            cmd.Parameters.AddWithValue("@amountinword", amountinword);
                            cmd.Parameters.AddWithValue("@opid", User.Identity.Name);
                            cmd.Parameters.AddWithValue("@macaddress", localIP);
                            cmd.Parameters.AddWithValue("@time", DateTime.Now.ToShortTimeString());
                            cmd.Parameters.AddWithValue("@transactiontype", "NA");

                            SqlParameter p = new SqlParameter("@paymentno", SqlDbType.Int);
                            p.Direction = ParameterDirection.Output;
                            cmd.Parameters.Add(p);

                            try
                            {
                                con.Open();
                                cmd.ExecuteNonQuery();
                                paymentno = Convert.ToInt32(cmd.Parameters["@paymentno"].Value.ToString());
                                ViewData["newbondid"] = newbondid2;
                                ViewData["paymethod"] = paymethod;
                                ViewData["paymentno"] = paymentno;
                                //MyClass.Sendmsg(cr.mobileno, "Dear " + cr.name + ",Your renewal of Rs " + payamount + " is received for plan " + planname + " for Bond: " + newbondid2 + " on date: " + pdate.ToString("dd/MM/yyyy"));
                                string Paymethod = "", chequeno = "";
                                if (paymethod == "banktransaction")
                                {
                                    Paymethod = "Bank Transaction" + "/" + tp.transactiontype;
                                    chequeno = "Transaction Id " + tp.chequeno;
                                }
                                else if (paymethod != "Cash")
                                {
                                    chequeno = paymethod + ", No. " + tp.chequeno;
                                    Paymethod = paymethod;
                                }
                                else
                                {
                                    Paymethod = paymethod;
                                }
                                MyClass.Sendmsg(cr.mobileno, "Dear " + cr.name + ", Your PAYMENT of Rs." + payamount + ",OF YOUR ID NO.-" + cr.newbondid + " vide " + Paymethod + " " + chequeno + " has been deposited successfully. Regards-" + cg.CompanyName);

                                ViewBag.msg = "Renewel Done Successfully....";
                                return View();
                            }

                            catch (Exception e)
                            {
                                ViewBag.msg = e.Message;
                            }
                            finally
                            {
                                con.Close();
                            }
                        }
                    }
                    else
                    {
                        Response.Write("<script>alert('Your Last Renewal is pending Please Contact to Admin')</script>");
                    }



                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult Printrenewal()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                ViewData["Paymentno"] = paymentno;
                return View();
            }

        }
        public ActionResult PrintRenewalReport()
        {
            List<RecieptTab> bond = new List<RecieptTab>();
            bond = db.RecieptTabs.ToList();
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "NewRenewal.rpt"));
            rd.SetDataSource(bond);

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


        public ActionResult PrintRenewalReportCheque(string newbondid, int paymentno = 0)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Branch");
            }
            else
            {
                List<tempcheque> bond = new List<tempcheque>();


                var max = (from t in db.TempInstallmenttabs where t.newbondid == newbondid && t.paymentno == paymentno && t.status == 1 select t.installmentno).Max();
                var amount = (from t in db.TempInstallmenttabs where t.newbondid == newbondid && t.paymentno == paymentno && t.status == 1 select t.payamount).Sum();
                var ta = db.TempInstallmenttabs.Single(t => t.newbondid == newbondid && t.paymentno == paymentno && t.installmentno == max && t.status == 1);
                var list = db.appltabs.Single(a => a.newbondid == newbondid);
                var chequeamount = "";
                var chequedate = "";
                var chequeno = "";
                if (ta.paymethod == "Cheque")
                {
                    chequeamount = "Amount of Cheque (Rs.)";
                    chequedate = "Date of Cheque";
                    chequeno = "Cheque Number";
                }
                else if (ta.paymethod == "banktransaction")
                {
                    chequeamount = "Amount of transaction (Rs.)";
                    chequedate = "Date of transaction";
                    chequeno = "Transaction Number";
                }
                bond.Add(new tempcheque { colChequedate = chequedate, colchequeno = chequeno, colChequeAmount = chequeamount, newbondid = ta.newbondid, name = list.name, nolandunit = list.nolandunit, opid = ta.opid, bookingamount = amount, bank = ta.bank, Account = ta.Account, chequeno = ta.chequeno, ACholdername = ta.ACholdername, Branch = ta.Bbranch, IFSCCode = ta.IFSCCode, ChequeAmount = ta.ChequeAmount, Chequedate = Convert.ToDateTime(ta.Chequedate), Chequeimage = ta.Chequeimage });

                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "MasterRenewAckReceipt.rpt"));
                rd.SetDataSource(bond);

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
        }

        [HttpGet]
        public ActionResult ChequePayment()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                return View();
            }
        }
        [HttpPost]
        public ActionResult ChequePayment(string newbondid2, string planname, Double term, string mode, Double latefine, int from, int to, string paymethod, DateTime pdate, string checkorddno, string drawno, Double relief, Double payamount, string amountinword)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var br = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                var ds = db.Blockdates.Where(c => c.date == pdate && c.branchcode == br.BranchCode && c.status == 0).Count();
                if (ds > 0)
                {
                    Response.Write("<script>alert('This Date is closed please select another date')</script>");
                }
                else
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
                    var rc = db.Installmenttabs.Single(c => c.receiptno == newbondid2);
                    var cr = db.appltabs.Single(c => c.newbondid == rc.newbondid);
                    con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "insertrcptinfo";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;

                    cmd.Parameters.AddWithValue("@newbondid", rc.newbondid);
                    cmd.Parameters.AddWithValue("@planname", planname);
                    cmd.Parameters.AddWithValue("@term", term);
                    cmd.Parameters.AddWithValue("@mode", mode);
                    cmd.Parameters.AddWithValue("@latefine", latefine);
                    cmd.Parameters.AddWithValue("@from", from);
                    cmd.Parameters.AddWithValue("@to", to);
                    cmd.Parameters.AddWithValue("@paymethod", paymethod);
                    cmd.Parameters.AddWithValue("@pdate", pdate);
                    cmd.Parameters.AddWithValue("@checkorddno", checkorddno);
                    cmd.Parameters.AddWithValue("@drawno", drawno);
                    cmd.Parameters.AddWithValue("@relief", relief);
                    cmd.Parameters.AddWithValue("@payamount", payamount);
                    cmd.Parameters.AddWithValue("@amountinword", amountinword);
                    cmd.Parameters.AddWithValue("@opid", User.Identity.Name);
                    cmd.Parameters.AddWithValue("@macaddress", localIP);
                    cmd.Parameters.AddWithValue("@time", DateTime.Now.ToShortTimeString());

                    SqlParameter p = new SqlParameter("@paymentno", SqlDbType.Int);
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);

                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                        paymentno = Convert.ToInt32(cmd.Parameters["@paymentno"].Value.ToString());
                        // MyClass.Sendmsg(cr.mobileno, "Dear " + cr.name + ",Your renewal of Rs " + payamount + " is received for plan " + planname + " for Bond: " + newbondid2 + " on date: " + pdate.ToString("dd/MM/yyyy"));
                        ViewBag.msg = "Payment Done Successfully....";
                        Response.Write("<script>alert('Payment Done Successfully....')</script>");
                        // ViewBag.msg2 = "Your Payment No. is " + paymentno.ToString() + " ";
                    }

                    catch (Exception e)
                    {
                        ViewBag.msg = e.Message;
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }
            return View();
        }

        [HttpGet]
        public ActionResult Printcheck()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                ViewData["Paymentno"] = paymentno;
                return View();
            }

        }
        public ActionResult PrintcheckReport()
        {
            List<RecieptTab> bond = new List<RecieptTab>();
            bond = db.RecieptTabs.ToList();
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Chequebook.rpt"));
            rd.SetDataSource(bond);

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
        public ActionResult PaymentPension()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                return View();
            }
        }
        [HttpPost]
        public ActionResult PaymentPension(string newbondid2, string planname, Double term, string mode, int from, int to, string paymethod, DateTime pdate, Double payamount, string amountinword)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var br = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                var ds = db.Blockdates.Where(c => c.date == pdate && c.branchcode == br.BranchCode && c.status == 0).Count();
                if (ds > 0)
                {
                    Response.Write("<script>alert('This Date is closed please select another date')</script>");
                }
                else
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

                    var cr = db.appltabs.Single(c => c.newbondid == newbondid2);
                    con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "insertpensioninfo";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;

                    cmd.Parameters.AddWithValue("@newbondid", newbondid2);
                    cmd.Parameters.AddWithValue("@planname", planname);
                    cmd.Parameters.AddWithValue("@term", term);
                    cmd.Parameters.AddWithValue("@mode", mode);
                    cmd.Parameters.AddWithValue("@from", from);
                    cmd.Parameters.AddWithValue("@to", to);
                    cmd.Parameters.AddWithValue("@paymethod", paymethod);
                    cmd.Parameters.AddWithValue("@pdate", pdate);
                    cmd.Parameters.AddWithValue("@payamount", payamount);
                    cmd.Parameters.AddWithValue("@amountinword", amountinword);
                    cmd.Parameters.AddWithValue("@opid", User.Identity.Name);
                    cmd.Parameters.AddWithValue("@macaddress", localIP);
                    cmd.Parameters.AddWithValue("@time", DateTime.Now.ToShortTimeString());

                    SqlParameter p = new SqlParameter("@paymentno", SqlDbType.Int);
                    p.Direction = ParameterDirection.Output;
                    cmd.Parameters.Add(p);

                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                        ViewData["paymentno"] = Convert.ToInt32(cmd.Parameters["@paymentno"].Value.ToString());

                        MyClass.Sendmsg(cr.mobileno, "Dear " + cr.name + ",Your pension of Rs " + payamount + " is paid for plan " + planname + " for CustomerId: " + newbondid2 + " on date: " + pdate.ToString("dd/MM/yyyy"));
                        ViewBag.msg = "Payment Done Successfully....";

                    }

                    catch (Exception e)
                    {
                        ViewBag.msg = e.Message;
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }
            return View();
        }
        public ActionResult PrintPensionReport(int paymentno = 0)
        {
            List<RecieptTab> bond = new List<RecieptTab>();
            bond = db.RecieptTabs.ToList();
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "NewRenewal.rpt"));
            rd.SetDataSource(bond);

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

        #endregion

        #region Duplicate Start Here

        [HttpGet]
        public ActionResult DuplicateReceipt(string newbondid, int paymentno = 0, Double payamount = 0)
        {
            List<DupliReceipt> dplist = new List<DupliReceipt>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                if (newbondid != null)
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
                    NumberToEnglish n = new NumberToEnglish();
                    var amountinwords = n.changeToWords(payamount.ToString(), true);
                    int from = (from p in db.Installmenttabs where p.newbondid == newbondid && p.paymentno == paymentno select p.installmentno).Min();
                    int to = (from p in db.Installmenttabs where p.newbondid == newbondid && p.paymentno == paymentno select p.installmentno).Max();

                    con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "Receipt";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;

                    cmd.Parameters.AddWithValue("@newbondid", newbondid);
                    cmd.Parameters.AddWithValue("@paymentno", paymentno);
                    cmd.Parameters.AddWithValue("@opid", User.Identity.Name);
                    cmd.Parameters.AddWithValue("@payamount", payamount);
                    cmd.Parameters.AddWithValue("@amountinword", amountinwords);
                    cmd.Parameters.AddWithValue("@from", from);
                    cmd.Parameters.AddWithValue("@to", to);


                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();


                        DuplicateTab dt = new DuplicateTab();
                        dt.newbondid = newbondid;
                        dt.payamount = payamount;
                        dt.paymentno = paymentno;
                        dt.opid = User.Identity.Name;
                        dt.date = DateTime.Now;
                        dt.macaddress = localIP;
                        dt.type = "Receipt";
                        db.DuplicateTabs.Add(dt);
                        db.SaveChanges();

                        List<RecieptTab> drlist = new List<RecieptTab>();

                        foreach (var cc in db.RecieptTabs.ToList())
                        {


                            DateTime luck = new DateTime((Convert.ToInt32(cc.date.Split('/')[2])), (Convert.ToInt32(cc.date.Split('/')[1])), (Convert.ToInt32(cc.date.Split('/')[0])));
                            drlist.Add(new RecieptTab
                            {


                                newbondid = cc.newbondid,
                                branchname = cc.branchname,
                                branchaddress = cc.branchaddress,
                                branchcode = cc.branchcode,
                                cssno = cc.cssno,
                                paymentno = cc.paymentno,
                                ledgerfolio = cc.ledgerfolio,
                                installmentno = cc.installmentno,
                                receiptno = cc.receiptno,
                                date = cc.date,



                                duedate = luck.ToString("MMMM") + " - " + luck.ToString("yyyy"),
                                nextduedate = cc.nextduedate,
                                expirydate = cc.expirydate,

                                name = cc.name,
                                father = cc.father,
                                relation = cc.relation,
                                age = cc.age,
                                address = cc.address,
                                dateofcommencement = cc.dateofcommencement,

                                totalconsideration = cc.totalconsideration,
                                maturityamount = cc.maturityamount,
                                yearsubscrib = cc.yearsubscrib,
                                unitcode = cc.unitcode,

                                planname = cc.planname,
                                term = cc.term,
                                landunit = cc.landunit,
                                mode = cc.mode,
                                amount = cc.amount,
                                agencycode = cc.agencycode,
                                payamount = cc.payamount,
                                paymethod = cc.paymethod,
                                othercharge = cc.othercharge,
                                amountinwords = n.changeToWords(cc.payamount.ToString(), true),
                                opid = cc.opid
                            });
                        }

                        ReportDocument rd = new ReportDocument();
                        rd.Load(Path.Combine(Server.MapPath("~/Reports"), "NewRenewal.rpt"));
                        rd.SetDataSource(drlist);

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



                    }

                    catch (Exception e)
                    {
                        ViewBag.msg = e.Message;
                    }
                    finally
                    {
                        con.Close();
                    }

                }
                return View(dplist);
            }
        }
        [HttpPost]
        public ActionResult DuplicateReceipt(string newbondid)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var ap = db.appltabs.Single(a => a.newbondid == newbondid);
                List<DupliReceipt> dplist = new List<DupliReceipt>();
                Double payamount = 0;
                if (ap.status == 0)
                {
                    Response.Write("<script>alert('This Customer is Block')</script>");
                }
                else
                {
                    var rlist = (from p in db.Installmenttabs where p.newbondid == newbondid && p.paymentno != 0 select new { p.paymentno }).Distinct();
                    foreach (var r in rlist.ToList())
                    {
                        payamount = (from u in db.Installmenttabs where u.newbondid == newbondid && u.paymentno == r.paymentno select u.payamount).DefaultIfEmpty(0).Sum();
                        var f = db.Installmenttabs.FirstOrDefault(d => d.paymentno == r.paymentno && d.newbondid == newbondid);
                        //payamount = (payamount + f.latefine) - f.reliefrs;                        
                        dplist.Add(new DupliReceipt { newbondid = newbondid, paymentno = r.paymentno, payamount = payamount });
                    }
                }
                return View(dplist);
            }
        }

        [HttpGet]
        public ActionResult DuplicateBond()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult DuplicateBond(string newbond)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var st = db.appltabs.Single(a => a.newbondid == newbond);
                if (st.status == 0)
                {
                    Response.Write("<script>alert('This Customer is Block!')</script>");
                }
                else
                {
                    int count = (from dt in db.DuplicateTabs where dt.newbondid == newbond && dt.opid == User.Identity.Name && dt.type == "Customer" select dt).Count();
                    if (count == 0)
                    {
                        con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandText = "Bond";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Connection = con;

                        cmd.Parameters.AddWithValue("@newbondid", newbond);


                        try
                        {
                            con.Open();
                            cmd.ExecuteNonQuery();
                        }

                        catch (Exception e)
                        {
                            ViewBag.msg = e.Message;
                        }
                        finally
                        {
                            con.Close();
                        }

                        string localIP = "";
                        foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
                        {
                            if (nic.OperationalStatus == OperationalStatus.Up)
                            {
                                localIP += nic.GetPhysicalAddress().ToString();
                                break;
                            }
                        }

                        var ar = db.appltabs.Single(a => a.newbondid == newbond);
                        DuplicateTab dt = new DuplicateTab();
                        dt.newbondid = newbond;
                        dt.payamount = ar.payment;
                        dt.paymentno = 1;
                        dt.opid = User.Identity.Name;
                        dt.date = DateTime.Now;
                        dt.macaddress = localIP;
                        dt.type = "Bond";
                        db.DuplicateTabs.Add(dt);
                        db.SaveChanges();


                        List<Bond_report> bond = new List<Bond_report>();
                        bond = db.Bond_reports.ToList();

                        ReportDocument rd = new ReportDocument();
                        rd.Load(Path.Combine(Server.MapPath("~/Reports"), "report.rpt"));
                        rd.SetDataSource(bond);

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
                    }
                    else if (count >= 0)
                    {
                        Response.Write("<script>alert('Sorry!,You already print duplicate of this')</script>");
                    }
                }
                return View();
            }

        }

        [HttpGet]
        public ActionResult PrintMoneyBack()
        {
            List<Printcheckbook> print = new List<Printcheckbook>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }

            return View(print);
        }

        [HttpPost]
        public ActionResult PrintMoneyBack(string account, int install = 0, int lastinstall = 0)
        {
            List<Printcheckbook> print = new List<Printcheckbook>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                var app = db.appltabs.Single(d => d.newbondid == account);
                if (app.status == 1)
                {
                    var ReturnAmount = db.Installmenttabs.Where(s => s.newbondid == account && s.installmentno >= install && s.installmentno <= lastinstall).ToList();

                    foreach (var s in ReturnAmount)
                    {
                        var dt = DateTime.Now;
                        if (s.mode == "Monthly")
                        {
                            dt = s.expirydate;
                        }
                        if (s.mode == "Quarterly")
                        {
                            dt = s.expirydate;
                        }
                        if (s.mode == "Halfyearly")
                        {
                            dt = s.expirydate;
                        }
                        if (s.mode == "Yearly")
                        {
                            dt = s.expirydate;
                        }


                        string sdt = dt.ToString("dd/MM/yyyy");

                        NumberToEnglish ne = new NumberToEnglish();
                        var amountword = ne.changeToWords(s.amount.ToString(), true);
                        print.Add(new Printcheckbook { Name = app.name, Tranid = s.receiptno, Amount = s.amount, term = s.term_plan, Planname = s.planname, AccountNo = s.newbondid, Amountword = amountword, date = sdt });
                    }
                }
                else
                {
                    Response.Write("<script>alert('Please approve check printing from Admin..')</script>");
                }
                NewBondid = account;
                ltsr = install;
                lstsr = lastinstall;

            }

            return View(print);
        }

        public ActionResult PDFPrintMoneyBack()
        {
            List<Printcheckbook> print = new List<Printcheckbook>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                var app = db.appltabs.Single(d => d.newbondid == NewBondid);
                var ReturnAmount = db.Installmenttabs.Where(s => s.newbondid == NewBondid && s.installmentno >= ltsr && s.installmentno <= lstsr).ToList();
                foreach (var s in ReturnAmount)
                {
                    var dt = DateTime.Now;
                    if (s.mode == "Monthly")
                    {
                        dt = s.expirydate;
                    }
                    if (s.mode == "Quarterly")
                    {
                        dt = s.expirydate;
                    }
                    if (s.mode == "Halfyearly")
                    {
                        dt = s.expirydate;
                    }
                    if (s.mode == "Yearly")
                    {
                        dt = s.expirydate;
                    }


                    string sdt = dt.ToString("dd/MM/yyyy");
                    NumberToEnglish ne = new NumberToEnglish();
                    var amountword = ne.changeToWords(s.amount.ToString(), true);
                    print.Add(new Printcheckbook { Name = app.name, Tranid = s.receiptno, Amount = s.amount, term = s.term_plan, Planname = s.planname, AccountNo = s.newbondid, Amountword = amountword, date = sdt });
                }

                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Moneybackcheck.rpt"));
                rd.SetDataSource(print);

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
        }

        #endregion

        #region Voucher Menu Start Here

        [HttpGet]
        public ActionResult DCBrokerVoucher(string newagentid, int month = 0, int year = 0, int status = 0)
        {
            List<VoucherList> vlist = new List<VoucherList>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                if (newagentid != null)
                {
                    SaveVoucher(newagentid, month, year, pdate);

                    /////////Selecting Voucher List Start Here//////
                    MonthName mn = new MonthName();
                    var monthname = mn.numbertomonthname(month) + "," + year;
                    List<AgentDetail> blist = new List<AgentDetail>();
                    var br = db.AgentDetails.Single(a => a.NewAgentId == newagentid);
                    blist = (from c in db.AgentDetails where c.AgencyCode != 1 orderby c.NewAgentId select c).ToList();
                    var tdsr = db.TDSLF_tabs.Single(t => t.Id == 1);
                    Double tdsper = 0;
                    if (br.Panno == null || br.Panno == "")
                    {
                        tdsper = tdsr.NPCTDS;
                    }
                    else
                    {
                        tdsper = tdsr.TDS;
                    }
                    //foreach (var b in blist)
                    //{
                    var bonusamount = (from cts in db.bonus_tabs where cts.agentcode == br.NewAgentId && cts.month == month && cts.year == year select cts.bonusamount).DefaultIfEmpty(0).Sum();
                    ViewData["bonusamount"] = bonusamount;
                    var advancepayment = (from cts in db.AdvDeductionVouchers where cts.newagentid == newagentid && cts.month == month && cts.year == year select cts.amount).DefaultIfEmpty(0).Sum();
                    ViewData["tdsper"] = tdsper;
                    List<plan> pclist = new List<plan>();
                    List<require> rlist = new List<require>();

                    var voucherno = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode select cts).Count();
                    var vyear = year.ToString();

                    var vcount = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode && cts.month == month && cts.year == vyear select cts).Count();
                    //------Inserting self voucher------------
                    var spclist = (from ct in db.commission_tabs where ct.bondintroducerid == br.AgencyCode && ct.newagentid == br.NewAgentId && ct.date.Month == month && ct.date.Year == year orderby ct.plancode select new { ct.plancode }).Distinct();
                    foreach (var p in spclist.ToList())
                    {
                        pclist.Add(new plan { plancode = p.plancode });
                    }

                    foreach (var pc in pclist)
                    {

                        var pr = db.NewPlans.Single(p => p.Plancode == pc.plancode);
                        var spylist = (from sct in db.commission_tabs where sct.bondintroducerid == br.AgencyCode && sct.newagentid == br.NewAgentId && sct.date.Month == month && sct.date.Year == year && sct.plancode == pc.plancode orderby sct.year select new { sct.year }).Distinct();
                        foreach (var spy in spylist.ToList())
                        {
                            var business = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == br.AgencyCode && cts.newagentid == br.NewAgentId && cts.date.Month == month && cts.date.Year == year select cts.amount).DefaultIfEmpty(0).Sum();
                            var commission = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == br.AgencyCode && cts.newagentid == br.NewAgentId && cts.date.Month == month && cts.date.Year == year select cts.commission).DefaultIfEmpty(0).Sum();
                            var percentage = (commission / business) * 100;
                            rlist.Add(new require { agentid = br.AgencyCode, newagentid = br.NewAgentId, name = br.name, rankname = br.RankName, planname = pr.Planname, year = spy.year, business = business, percentage = percentage, commission = commission, tds = tdsper, bonusamount = bonusamount, advancepayment = advancepayment, brokerid = br.NewAgentId, brokername = br.name, brokerrank = br.RankName, newintroducerid = br.NewIntroducerId, month = monthname, voucherno = voucherno + 1 });

                        }
                    }

                    //------End Inserting self voucher----------------

                    //----------selecting team business and commission of selected month---------------

                    var arlist = (from a in db.AgentDetails where a.NewIntroducerId == br.NewAgentId select a).ToList();
                    List<agents> alist = new List<agents>();
                    foreach (var aar in arlist)
                    {
                        alist.Add(new agents { agentcode = aar.AgencyCode });
                    }

                    foreach (var a in alist)
                    {
                        List<teamagents> tlist = new List<teamagents>();
                        List<trequire> trlist = new List<trequire>();
                        var nagentid = db.AgentDetails.Single(na => na.AgencyCode == a.agentcode);
                        int rcount = 0;
                        rcount = rcount + 1;
                        tlist.Add(new teamagents { sr = rcount, tagentcode = a.agentcode });
                        var maxsragent = tlist.Count;
                        var minsragent = 1;
                        while (minsragent <= maxsragent)
                        {

                            var da = tlist.Where(t => t.sr == minsragent);
                            foreach (var d in da.ToList())
                            {
                                var aalist = (from al in db.AgentDetails where al.IntroducerCode == d.tagentcode select new { al.AgencyCode }).Distinct();
                                foreach (var aa in aalist)
                                {
                                    rcount = rcount + 1;
                                    tlist.Add(new teamagents { sr = rcount, tagentcode = aa.AgencyCode });

                                }

                            }
                            minsragent = minsragent + 1;
                            maxsragent = tlist.Count;
                        }

                        foreach (var tt in tlist)
                        {
                            var neaid = db.AgentDetails.Single(n => n.AgencyCode == tt.tagentcode);
                            var nctrlist = (from nn in db.commission_tabs where nn.bondintroducerid == tt.tagentcode && nn.newagentid == br.NewAgentId && nn.date.Month == month && nn.date.Year == year select nn).ToList();
                            foreach (var nctr in nctrlist)
                            {
                                trlist.Add(new trequire { newagentid = neaid.NewAgentId, plancode = nctr.plancode, year = nctr.year, business = nctr.amount, percentage = nctr.percentage, commission = nctr.commission });
                            }

                        }
                        var tnrname = db.AgentDetails.Single(t => t.NewAgentId == nagentid.NewAgentId);
                        var dpclist = trlist.Select(tr => new { tr.plancode }).Distinct();
                        foreach (var dpc in dpclist)
                        {

                            var ppr = db.NewPlans.Single(pp => pp.Plancode == dpc.plancode);
                            var dpylist = trlist.Select(tr => new { tr.year }).Distinct();
                            foreach (var dpy in dpylist)
                            {
                                var tbusiness = trlist.Where(tr => tr.plancode == dpc.plancode && tr.year == dpy.year).Sum(tr => tr.business);
                                var tcommission = trlist.Where(tr => tr.plancode == dpc.plancode && tr.year == dpy.year).Sum(tr => tr.commission);
                                var tpercentage = (tcommission / tbusiness) * 100;
                                rlist.Add(new require { agentid = nagentid.AgencyCode, newagentid = nagentid.NewAgentId, name = tnrname.name, rankname = tnrname.RankName, planname = ppr.Planname, year = dpy.year, business = tbusiness, percentage = tpercentage, commission = tcommission, tds = tdsper, bonusamount = bonusamount, advancepayment = advancepayment, brokerid = br.NewAgentId, brokername = br.name, brokerrank = br.RankName, newintroducerid = br.NewIntroducerId, month = monthname, voucherno = voucherno + 1 });
                            }
                        }

                    }


                    //----------End selecting team business and commission of selected month---------------

                    var gbusiness = rlist.Sum(tr => tr.business);
                    var gcommission = rlist.Sum(tr => tr.commission);
                    var lesstds = Math.Round((gcommission * tdsper) / 100, 2);
                    var netamount = Math.Round((gcommission + bonusamount - lesstds - advancepayment), 2);
                    vlist.Add(new VoucherList { newagentid = br.NewAgentId, name = br.name, introducerid = br.NewIntroducerId, advancepayment = advancepayment, amount = netamount, status = vcount, month = month, year = year });

                    //}


                    //----------End selecting team business and commission of selected month---------------
                    /////////Selecting Voucher List End Here//////

                }

                return View(vlist);
            }
        }
        [HttpPost]
        public ActionResult DCBrokerVoucher(string newagentid, DateTime paydate, int month = 0, int year = 0)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                MonthName mn = new MonthName();
                var monthname = mn.numbertomonthname(month) + "," + year;
                List<VoucherList> vlist = new List<VoucherList>();
                var yyear = year.ToString();
                var vrcount = db.ReleaseTabs.Where(c => c.month == month && c.year == yyear).Count();
                if (vrcount > 0)
                {
                    List<AgentDetail> blist = new List<AgentDetail>();
                    var or = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                    var br = db.AgentDetails.Single(a => a.NewAgentId == newagentid);
                    blist = (from c in db.AgentDetails where c.AgencyCode != 1 && c.BranchCode == or.BranchCode orderby c.NewAgentId select c).ToList();
                    var tdsr = db.TDSLF_tabs.Single(t => t.Id == 1);
                    Double tdsper = 0;
                    if (br.Panno == null || br.Panno == "")
                    {
                        tdsper = tdsr.NPCTDS;
                    }
                    else
                    {
                        tdsper = tdsr.TDS;
                    }
                    //foreach (var b in blist)
                    //{
                    var bonusamount = (from cts in db.bonus_tabs where cts.agentcode == br.NewAgentId && cts.month == month && cts.year == year select cts.bonusamount).DefaultIfEmpty(0).Sum();
                    ViewData["bonusamount"] = bonusamount;
                    var advancepayment = (from cts in db.AdvDeductionVouchers where cts.newagentid == newagentid && cts.month == month && cts.year == year select cts.amount).DefaultIfEmpty(0).Sum();
                    ViewData["tdsper"] = tdsper;
                    List<plan> pclist = new List<plan>();
                    List<require> rlist = new List<require>();

                    var voucherno = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode select cts).Count();
                    var vyear = year.ToString();

                    var vcount = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode && cts.month == month && cts.year == vyear select cts).Count();
                    //------Inserting self voucher------------
                    var spclist = (from ct in db.commission_tabs where ct.bondintroducerid == br.AgencyCode && ct.newagentid == br.NewAgentId && ct.date.Month == month && ct.date.Year == year orderby ct.plancode select new { ct.plancode }).Distinct();
                    foreach (var p in spclist.ToList())
                    {
                        pclist.Add(new plan { plancode = p.plancode });
                    }

                    foreach (var pc in pclist)
                    {

                        var pr = db.NewPlans.Single(p => p.Plancode == pc.plancode);
                        var spylist = (from sct in db.commission_tabs where sct.bondintroducerid == br.AgencyCode && sct.newagentid == br.NewAgentId && sct.date.Month == month && sct.date.Year == year && sct.plancode == pc.plancode orderby sct.year select new { sct.year }).Distinct();
                        foreach (var spy in spylist.ToList())
                        {

                            var business = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == br.AgencyCode && cts.newagentid == br.NewAgentId && cts.date.Month == month && cts.date.Year == year select cts.amount).DefaultIfEmpty(0).Sum();
                            var commission = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == br.AgencyCode && cts.newagentid == br.NewAgentId && cts.date.Month == month && cts.date.Year == year select cts.commission).DefaultIfEmpty(0).Sum();
                            var percentage = (commission / business) * 100;
                            rlist.Add(new require { agentid = br.AgencyCode, newagentid = br.NewAgentId, name = br.name, rankname = br.RankName, planname = pr.Planname, year = spy.year, business = business, percentage = percentage, commission = commission, tds = tdsper, bonusamount = bonusamount, advancepayment = advancepayment, brokerid = br.NewAgentId, brokername = br.name, brokerrank = br.RankName, newintroducerid = br.NewIntroducerId, month = monthname, voucherno = voucherno + 1 });

                        }
                    }

                    //------End Inserting self voucher----------------

                    //----------selecting team business and commission of selected month---------------

                    var arlist = (from a in db.AgentDetails where a.NewIntroducerId == br.NewAgentId select a).ToList();
                    List<agents> alist = new List<agents>();
                    foreach (var aar in arlist)
                    {
                        alist.Add(new agents { agentcode = aar.AgencyCode });
                    }

                    foreach (var a in alist)
                    {
                        List<teamagents> tlist = new List<teamagents>();
                        List<trequire> trlist = new List<trequire>();
                        var nagentid = db.AgentDetails.Single(na => na.AgencyCode == a.agentcode);
                        int rcount = 0;
                        rcount = rcount + 1;
                        tlist.Add(new teamagents { sr = rcount, tagentcode = a.agentcode });
                        var maxsragent = tlist.Count;
                        var minsragent = 1;
                        while (minsragent <= maxsragent)
                        {

                            var da = tlist.Where(t => t.sr == minsragent);
                            foreach (var d in da.ToList())
                            {
                                var aalist = (from al in db.AgentDetails where al.IntroducerCode == d.tagentcode select new { al.AgencyCode }).Distinct();
                                foreach (var aa in aalist)
                                {
                                    rcount = rcount + 1;
                                    tlist.Add(new teamagents { sr = rcount, tagentcode = aa.AgencyCode });

                                }

                            }
                            minsragent = minsragent + 1;
                            maxsragent = tlist.Count;
                        }

                        foreach (var tt in tlist)
                        {
                            var neaid = db.AgentDetails.Single(n => n.AgencyCode == tt.tagentcode);
                            var nctrlist = (from nn in db.commission_tabs where nn.bondintroducerid == tt.tagentcode && nn.newagentid == br.NewAgentId && nn.date.Month == month && nn.date.Year == year select nn).ToList();
                            foreach (var nctr in nctrlist)
                            {
                                trlist.Add(new trequire { newagentid = neaid.NewAgentId, plancode = nctr.plancode, year = nctr.year, business = nctr.amount, percentage = nctr.percentage, commission = nctr.commission });
                            }

                        }
                        var tnrname = db.AgentDetails.Single(t => t.NewAgentId == nagentid.NewAgentId);
                        var dpclist = trlist.Select(tr => new { tr.plancode }).Distinct();
                        foreach (var dpc in dpclist)
                        {

                            var ppr = db.NewPlans.Single(pp => pp.Plancode == dpc.plancode);
                            var dpylist = trlist.Select(tr => new { tr.year }).Distinct();
                            foreach (var dpy in dpylist)
                            {
                                var tbusiness = trlist.Where(tr => tr.plancode == dpc.plancode && tr.year == dpy.year).Sum(tr => tr.business);
                                var tcommission = trlist.Where(tr => tr.plancode == dpc.plancode && tr.year == dpy.year).Sum(tr => tr.commission);
                                var tpercentage = (tcommission / tbusiness) * 100;
                                rlist.Add(new require { agentid = nagentid.AgencyCode, newagentid = nagentid.NewAgentId, name = tnrname.name, rankname = tnrname.RankName, planname = ppr.Planname, year = dpy.year, business = tbusiness, percentage = tpercentage, commission = tcommission, tds = tdsper, bonusamount = bonusamount, advancepayment = advancepayment, brokerid = br.NewAgentId, brokername = br.name, brokerrank = br.RankName, newintroducerid = br.NewIntroducerId, month = monthname, voucherno = voucherno + 1 });
                            }
                        }

                    }


                    //----------End selecting team business and commission of selected month---------------

                    var gbusiness = rlist.Sum(tr => tr.business);
                    var gcommission = rlist.Sum(tr => tr.commission);
                    var lesstds = Math.Round((gcommission * tdsper) / 100, 2);
                    var netamount = Math.Round((gcommission + bonusamount - lesstds - advancepayment), 2);
                    vlist.Add(new VoucherList { newagentid = br.NewAgentId, name = br.name, introducerid = br.NewIntroducerId, amount = netamount, advancepayment = advancepayment, status = vcount, month = month, year = year });
                }
                else
                {
                    Response.Write("<script>alert('Voucher is not released For this month..')</script>");
                }
                //}
                pdate = paydate;
                return View(vlist);
            }

        }
        [HttpGet]
        public ActionResult PaymentOBVOne(string newagentid, int month = 0, int year = 0)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                MonthName mn = new MonthName();
                var monthname = mn.numbertomonthname(month) + "," + year;
                var tdsr = db.TDSLF_tabs.Single(t => t.Id == 1);
                var bonusamount = (from cts in db.bonus_tabs where cts.agentcode == newagentid && cts.month == month && cts.year == year select cts.bonusamount).DefaultIfEmpty(0).Sum();
                ViewData["bonusamount"] = bonusamount;
                var advancepayment = (from cts in db.AdvDeductionVouchers where cts.newagentid == newagentid && cts.month == month && cts.year == year select cts.amount).DefaultIfEmpty(0).Sum();
                List<plan> pclist = new List<plan>();
                List<require> rlist = new List<require>();
                var br = db.AgentDetails.Single(a => a.NewAgentId == newagentid);
                Double tdsper = 0;
                if (br.Panno == null || br.Panno == "")
                {
                    tdsper = tdsr.NPCTDS;
                }
                else
                {
                    tdsper = tdsr.TDS;
                }
                ViewData["tdsper"] = tdsper;
                var voucherno = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode select cts).Count();
                var vyear = year.ToString();


                //------Inserting self voucher------------
                var spclist = (from ct in db.commission_tabs where ct.bondintroducerid == br.AgencyCode && ct.newagentid == newagentid && ct.date.Month == month && ct.date.Year == year orderby ct.plancode select new { ct.plancode }).Distinct();
                foreach (var p in spclist.ToList())
                {
                    pclist.Add(new plan { plancode = p.plancode });
                }

                foreach (var pc in pclist)
                {

                    var pr = db.NewPlans.Single(p => p.Plancode == pc.plancode);
                    var spylist = (from sct in db.commission_tabs where sct.bondintroducerid == br.AgencyCode && sct.newagentid == newagentid && sct.date.Month == month && sct.date.Year == year && sct.plancode == pc.plancode orderby sct.year select new { sct.year }).Distinct();
                    foreach (var spy in spylist.ToList())
                    {

                        var business = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == br.AgencyCode && cts.newagentid == newagentid && cts.date.Month == month && cts.date.Year == year select cts.amount).DefaultIfEmpty(0).Sum();
                        var commission = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == br.AgencyCode && cts.newagentid == newagentid && cts.date.Month == month && cts.date.Year == year select cts.commission).DefaultIfEmpty(0).Sum();
                        var percentage = (commission / business) * 100;
                        rlist.Add(new require { agentid = br.AgencyCode, newagentid = newagentid, name = br.name, rankname = br.RankName, planname = pr.Planname, year = spy.year, business = business, percentage = percentage, commission = commission, tds = tdsper, bonusamount = bonusamount, advancepayment = advancepayment, brokerid = newagentid, brokername = br.name, brokerrank = br.RankName, newintroducerid = br.NewIntroducerId, month = monthname, voucherno = voucherno + 1 });

                    }
                }

                //------End Inserting self voucher----------------

                //----------selecting team business and commission of selected month---------------

                var arlist = (from a in db.AgentDetails where a.NewIntroducerId == newagentid select a).ToList();
                List<agents> alist = new List<agents>();
                foreach (var aar in arlist)
                {
                    alist.Add(new agents { agentcode = aar.AgencyCode });
                }

                foreach (var a in alist)
                {
                    List<teamagents> tlist = new List<teamagents>();
                    List<trequire> trlist = new List<trequire>();
                    var nagentid = db.AgentDetails.Single(na => na.AgencyCode == a.agentcode);
                    int rcount = 0;
                    rcount = rcount + 1;
                    tlist.Add(new teamagents { sr = rcount, tagentcode = a.agentcode });
                    var maxsragent = tlist.Count;
                    var minsragent = 1;
                    while (minsragent <= maxsragent)
                    {

                        var da = tlist.Where(t => t.sr == minsragent);
                        foreach (var d in da.ToList())
                        {
                            var aalist = (from al in db.AgentDetails where al.IntroducerCode == d.tagentcode select new { al.AgencyCode }).Distinct();
                            foreach (var aa in aalist)
                            {
                                rcount = rcount + 1;
                                tlist.Add(new teamagents { sr = rcount, tagentcode = aa.AgencyCode });

                            }

                        }
                        minsragent = minsragent + 1;
                        maxsragent = tlist.Count;
                    }
                    foreach (var tt in tlist)
                    {
                        var neaid = db.AgentDetails.Single(n => n.AgencyCode == tt.tagentcode);
                        var nctrlist = (from nn in db.commission_tabs where nn.bondintroducerid == tt.tagentcode && nn.newagentid == newagentid && nn.date.Month == month && nn.date.Year == year select nn).ToList();
                        foreach (var nctr in nctrlist)
                        {
                            trlist.Add(new trequire { newagentid = neaid.NewAgentId, plancode = nctr.plancode, year = nctr.year, business = nctr.amount, percentage = nctr.percentage, commission = nctr.commission });
                        }

                    }
                    var tnrname = db.AgentDetails.Single(t => t.NewAgentId == nagentid.NewAgentId);
                    var dpclist = trlist.Select(tr => new { tr.plancode }).Distinct();
                    foreach (var dpc in dpclist)
                    {

                        var ppr = db.NewPlans.Single(pp => pp.Plancode == dpc.plancode);
                        var dpylist = trlist.Select(tr => new { tr.year }).Distinct();
                        foreach (var dpy in dpylist)
                        {
                            var tbusiness = trlist.Where(tr => tr.plancode == dpc.plancode && tr.year == dpy.year).Sum(tr => tr.business);
                            var tcommission = trlist.Where(tr => tr.plancode == dpc.plancode && tr.year == dpy.year).Sum(tr => tr.commission);
                            var tpercentage = (tcommission / tbusiness) * 100;
                            rlist.Add(new require { agentid = nagentid.AgencyCode, newagentid = nagentid.NewAgentId, name = tnrname.name, rankname = tnrname.RankName, planname = ppr.Planname, year = dpy.year, business = tbusiness, percentage = tpercentage, commission = tcommission, tds = tdsper, bonusamount = bonusamount, advancepayment = advancepayment, brokerid = newagentid, brokername = br.name, brokerrank = br.RankName, newintroducerid = br.NewIntroducerId, month = monthname, voucherno = voucherno + 1 });
                        }
                    }

                }


                //----------End selecting team business and commission of selected month---------------

                vnewagentid = newagentid;
                vmonth = month;
                vvyear = year;
                return View(rlist);
            }
        }
        public ActionResult PrintDCVoucher()
        {
            List<require> rlist = new List<require>();
            var tdsr = db.TDSLF_tabs.Single(t => t.Id == 1);
            var bonusamount = (from cts in db.bonus_tabs where cts.agentcode == vnewagentid && cts.month == vmonth && cts.year == vvyear select cts.bonusamount).DefaultIfEmpty(0).Sum();
            ViewData["bonusamount"] = bonusamount;
            var advancepayment = (from cts in db.AdvDeductionVouchers where cts.newagentid == vnewagentid && cts.month == vmonth && cts.year == vvyear select cts.amount).DefaultIfEmpty(0).Sum();
            List<plan> pclist = new List<plan>();
            MonthName mn = new MonthName();
            var monthname = mn.numbertomonthname(vmonth) + "," + vvyear;
            var mon = mn.numbertomonthname(vmonth);
            var br = db.BrokerCommLists.Single(a => a.newagentid == vnewagentid && a.month == mon && a.Year == vvyear);
            var car = db.AgentDetails.Single(rrr => rrr.NewAgentId == vnewagentid);
            Double tdsper = 0;
            if (br.panno == null || br.panno == "Form-61")
            {
                tdsper = tdsr.NPCTDS;
            }
            else
            {
                tdsper = tdsr.TDS;
            }
            var voucherno = (from cts in db.Voucher_Reports where cts.agentid == br.agencycode select cts).Count();
            var rankname = string.Empty;
            //------Inserting self voucher------------
            var spclist = (from ct in db.commission_tabs where ct.bondintroducerid == br.agencycode && ct.newagentid == vnewagentid && ct.date.Month == vmonth && ct.date.Year == vvyear orderby ct.planname select new { ct.planname }).Distinct();
            foreach (var p in spclist.ToList())
            {
                pclist.Add(new plan { planname = p.planname });
            }

            foreach (var pc in pclist)
            {

                var spylist = (from sct in db.commission_tabs where sct.bondintroducerid == br.agencycode && sct.newagentid == vnewagentid && sct.date.Month == vmonth && sct.date.Year == vvyear && sct.planname == pc.planname orderby sct.year select new { sct.year }).Distinct();
                foreach (var spy in spylist.ToList())
                {
                    var business = (from cts in db.commission_tabs where cts.planname == pc.planname && cts.year == spy.year && cts.bondintroducerid == br.agencycode && cts.newagentid == vnewagentid && cts.date.Month == vmonth && cts.date.Year == vvyear && cts.comtype == "Voucher" select cts.amount).DefaultIfEmpty(0).Sum();
                    var commission = (from cts in db.commission_tabs where cts.planname == pc.planname && cts.year == spy.year && cts.bondintroducerid == br.agencycode && cts.newagentid == vnewagentid && cts.date.Month == vmonth && cts.date.Year == vvyear && cts.comtype == "Voucher" select cts.commission).DefaultIfEmpty(0).Sum();
                    if (business > 0)
                    {
                        var percentage = (commission / business) * 100;
                        rlist.Add(new require { agentid = br.agencycode, newagentid = br.newagentid, name = br.name, emailid = br.panno, rankname = car.RankName, planname = pc.planname, year = spy.year, business = business, percentage = percentage, commission = commission, tds = tdsper, bonusamount = bonusamount, advancepayment = advancepayment, brokerid = vnewagentid, brokername = br.name, brokerrank = car.RankName, newintroducerid = br.newintroducerid, month = monthname, voucherno = voucherno + 1, panno = br.panno, commtype = "Voucher" });
                    }

                    var sbusiness = (from cts in db.commission_tabs where cts.planname == pc.planname && cts.year == spy.year && cts.bondintroducerid == br.agencycode && cts.newagentid == vnewagentid && cts.date.Month == vmonth && cts.date.Year == vvyear && cts.comtype == "Spot" select cts.amount).DefaultIfEmpty(0).Sum();
                    var scommission = (from cts in db.commission_tabs where cts.planname == pc.planname && cts.year == spy.year && cts.bondintroducerid == br.agencycode && cts.newagentid == vnewagentid && cts.date.Month == vmonth && cts.date.Year == vvyear && cts.comtype == "Spot" select cts.commission).DefaultIfEmpty(0).Sum();
                    if (sbusiness > 0)
                    {
                        var spercentage = (scommission / sbusiness) * 100;
                        rlist.Add(new require { agentid = br.agencycode, newagentid = br.newagentid, name = br.name, emailid = br.panno, rankname = car.RankName, planname = pc.planname, year = spy.year, business = sbusiness, percentage = spercentage, commission = scommission, tds = tdsper, bonusamount = bonusamount, advancepayment = advancepayment, brokerid = vnewagentid, brokername = br.name, brokerrank = car.RankName, newintroducerid = br.newintroducerid, month = monthname, voucherno = voucherno + 1, panno = br.panno, commtype = "Spot" });
                    }
                }
            }

            //------End Inserting self voucher----------------

            //----------selecting team business and commission of selected month---------------

            var arlist = (from a in db.BrokerCommLists where a.newintroducerid == vnewagentid && a.month == mon && a.Year == vvyear select a).ToList();
            List<agents> alist = new List<agents>();
            foreach (var aar in arlist)
            {
                alist.Add(new agents { agentcode = aar.agencycode });
            }

            foreach (var a in alist)
            {
                List<teamagents> tlist = new List<teamagents>();
                List<trequire> trlist = new List<trequire>();
                var nagentid = db.BrokerCommLists.Single(na => na.agencycode == a.agentcode && na.month == mon && na.Year == vvyear);
                int rcount = 0;
                rcount = rcount + 1;
                tlist.Add(new teamagents { sr = rcount, tagentcode = a.agentcode });
                var maxsragent = tlist.Count;
                var minsragent = 1;
                while (minsragent <= maxsragent)
                {

                    var da = tlist.Where(t => t.sr == minsragent);
                    foreach (var d in da.ToList())
                    {
                        var aalist = (from al in db.BrokerCommLists where al.introducerid == d.tagentcode && al.month == mon && al.Year == vvyear select new { al.agencycode }).Distinct();
                        foreach (var aa in aalist)
                        {
                            rcount = rcount + 1;
                            tlist.Add(new teamagents { sr = rcount, tagentcode = aa.agencycode });

                        }

                    }
                    minsragent = minsragent + 1;
                    maxsragent = tlist.Count;
                }

                foreach (var tt in tlist)
                {
                    var neaid = db.BrokerCommLists.Single(n => n.agencycode == tt.tagentcode && n.month == mon && n.Year == vvyear);

                    var nctrlist = (from nn in db.commission_tabs where nn.bondintroducerid == tt.tagentcode && nn.newagentid == vnewagentid && nn.date.Month == vmonth && nn.date.Year == vvyear select nn).ToList();
                    foreach (var nctr in nctrlist)
                    {
                        trlist.Add(new trequire { newagentid = neaid.newagentid, planname = nctr.planname, plancode = nctr.plancode, year = nctr.year, business = nctr.amount, percentage = nctr.percentage, commission = nctr.commission, commtype = nctr.comtype });

                    }

                }

                var tnrname = db.BrokerCommLists.Single(t => t.newagentid == nagentid.newagentid && t.month == mon && t.Year == vvyear);
                var tcar = db.AgentDetails.Single(trrr => trrr.AgencyCode == a.agentcode);
                var dpclist = trlist.Select(tr => new { tr.planname }).Distinct();
                foreach (var dpc in dpclist)
                {
                    var dpylist = trlist.Select(tr => new { tr.year }).Distinct();
                    foreach (var dpy in dpylist)
                    {

                        var tbusiness = trlist.Where(tr => tr.planname == dpc.planname && tr.year == dpy.year && tr.commtype == "Voucher").Sum(tr => tr.business);
                        var tcommission = trlist.Where(tr => tr.planname == dpc.planname && tr.year == dpy.year && tr.commtype == "Voucher").Sum(tr => tr.commission);
                        if (tbusiness > 0)
                        {
                            var tpercentage = (tcommission / tbusiness) * 100;
                            rlist.Add(new require { agentid = nagentid.agencycode, newagentid = tnrname.newagentid, name = tnrname.name, rankname = tcar.RankName, planname = dpc.planname, year = dpy.year, business = tbusiness, percentage = tpercentage, commission = tcommission, tds = tdsper, bonusamount = bonusamount, advancepayment = advancepayment, brokerid = vnewagentid, brokername = br.name, brokerrank = car.RankName, panno = br.panno, newintroducerid = br.newintroducerid, month = monthname, voucherno = voucherno + 1, commtype = "Voucher" });
                        }

                        var stbusiness = trlist.Where(tr => tr.planname == dpc.planname && tr.year == dpy.year && tr.commtype == "Spot").Sum(tr => tr.business);
                        var stcommission = trlist.Where(tr => tr.planname == dpc.planname && tr.year == dpy.year && tr.commtype == "Spot").Sum(tr => tr.commission);
                        if (stbusiness > 0)
                        {
                            var stpercentage = (stcommission / stbusiness) * 100;
                            rlist.Add(new require { agentid = nagentid.agencycode, newagentid = tnrname.newagentid, name = tnrname.name, rankname = tcar.RankName, planname = dpc.planname, year = dpy.year, business = stbusiness, percentage = stpercentage, commission = stcommission, tds = tdsper, bonusamount = bonusamount, advancepayment = advancepayment, brokerid = vnewagentid, brokername = br.name, brokerrank = car.RankName, panno = br.panno, newintroducerid = br.newintroducerid, month = monthname, voucherno = voucherno + 1, commtype = "Spot" });
                        }
                    }
                }
            }

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "CompleteDCVoucher.rpt"));
            rd.SetDataSource(rlist);

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
        public ActionResult CCBrokerVoucher(string newagentid, int month = 0, int year = 0, int status = 0)
        {
            List<VoucherList> vlist = new List<VoucherList>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                if (newagentid != null)
                {
                    CCSaveVoucher(newagentid, month, year, pdate);
                    MonthName mn = new MonthName();
                    var monthname = mn.numbertomonthname(month) + "," + year;

                    #region Selecting Complete Chain Start Here

                    int rcount = 0;
                    List<BrokerChain> bclist = new List<BrokerChain>();
                    var br = db.AgentDetails.Single(a => a.NewAgentId == newagentid);
                    var opr = db.Branchtabs.Single(b => b.BranchCode == User.Identity.Name);
                    var bb = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
                    var tdsr = db.TDSLF_tabs.Single(t => t.Id == 1);
                    Double tdsper = 0;
                    if (br.Panno == null || br.Panno == "")
                    {
                        tdsper = tdsr.NPCTDS;
                    }
                    else
                    {
                        tdsper = tdsr.TDS;
                    }
                    bclist.Add(new BrokerChain { newagentid = br.NewAgentId, name = br.name, rankcode = br.RankCode, rankname = br.RankName, introducerid = br.NewIntroducerId, introname = br.IntroName, branchname = bb.BranchName });
                    List<agents> dalist = new List<agents>();
                    var alist = (from al in db.AgentDetails where al.NewIntroducerId == newagentid select new { al.AgencyCode }).Distinct();
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
                        bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = bb.BranchName });

                    }

                    #endregion

                    #region Selecting CC Voucher Start Here

                    List<require> rlist = new List<require>();
                    var bonusamount = (from cts in db.bonus_tabs where cts.agentcode == newagentid && cts.month == month && cts.year == year select cts.bonusamount).DefaultIfEmpty(0).Sum();
                    ViewData["bonusamount"] = bonusamount;
                    ViewData["tdsper"] = tdsper;
                    var voucherno = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode select cts).Count();
                    var vyear = year.ToString();
                    var vcount = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode && cts.month == month && cts.year == vyear select cts).Count();

                    foreach (var bc in bclist)
                    {

                        var ar = db.AgentDetails.Single(a => a.NewAgentId == bc.newagentid);
                        List<plan> pclist = new List<plan>();

                        //------Inserting self voucher------------
                        var spclist = (from ct in db.commission_tabs where ct.bondintroducerid == ar.AgencyCode && ct.newagentid == br.NewAgentId && ct.date.Month == month && ct.date.Year == year orderby ct.plancode select new { ct.plancode }).Distinct();
                        foreach (var p in spclist.ToList())
                        {
                            pclist.Add(new plan { plancode = p.plancode });
                        }

                        foreach (var pc in pclist)
                        {

                            var pr = db.NewPlans.Single(p => p.Plancode == pc.plancode);
                            var spylist = (from sct in db.commission_tabs where sct.bondintroducerid == ar.AgencyCode && sct.newagentid == br.NewAgentId && sct.date.Month == month && sct.date.Year == year && sct.plancode == pc.plancode orderby sct.year select new { sct.year }).Distinct();
                            foreach (var spy in spylist.ToList())
                            {

                                var business = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == ar.AgencyCode && cts.newagentid == br.NewAgentId && cts.date.Month == month && cts.date.Year == year select cts.amount).DefaultIfEmpty(0).Sum();
                                var commission = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == ar.AgencyCode && cts.newagentid == br.NewAgentId && cts.date.Month == month && cts.date.Year == year select cts.commission).DefaultIfEmpty(0).Sum();
                                var percentage = (commission / business) * 100;
                                rlist.Add(new require { agentid = ar.AgencyCode, newagentid = ar.NewAgentId, name = ar.name, rankname = ar.RankName, planname = pr.Planname, year = spy.year, business = business, percentage = percentage, commission = commission, tds = tdsper, bonusamount = bonusamount, brokerid = br.NewAgentId, brokername = br.name, brokerrank = br.RankName, newintroducerid = br.NewIntroducerId, month = monthname, voucherno = voucherno + 1 });

                            }
                        }

                        //------End Inserting self voucher----------------

                    }
                    var gbusiness = rlist.Sum(tr => tr.business);
                    var gcommission = rlist.Sum(tr => tr.commission);
                    var lesstds = Math.Round((gcommission * tdsper) / 100, 2);
                    var netamount = Math.Round((gcommission + bonusamount - lesstds), 2);
                    vlist.Add(new VoucherList { newagentid = br.NewAgentId, name = br.name, introducerid = br.NewIntroducerId, amount = netamount, status = vcount, month = month, year = year });

                    #endregion
                }
            }
            return View(vlist);
        }
        [HttpPost]
        public ActionResult CCBrokerVoucher(string newagentid, DateTime paydate, int month = 0, int year = 0)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                MonthName mn = new MonthName();
                var monthname = mn.numbertomonthname(month) + "," + year;
                List<VoucherList> vlist = new List<VoucherList>();
                var yyear = year.ToString();
                var vrcount = db.ReleaseTabs.Where(c => c.month == month && c.year == yyear).Count();
                if (vrcount > 0)
                {
                    #region Selecting Complete Chain Start Here

                    int rcount = 0;
                    List<BrokerChain> bclist = new List<BrokerChain>();
                    var br = db.AgentDetails.Single(a => a.NewAgentId == newagentid);
                    var opr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
                    var bb = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
                    var tdsr = db.TDSLF_tabs.Single(t => t.Id == 1);
                    Double tdsper = 0;
                    if (br.Panno == null || br.Panno == "")
                    {
                        tdsper = tdsr.NPCTDS;
                    }
                    else
                    {
                        tdsper = tdsr.TDS;
                    }
                    bclist.Add(new BrokerChain { newagentid = br.NewAgentId, name = br.name, rankcode = br.RankCode, rankname = br.RankName, introducerid = br.NewIntroducerId, introname = br.IntroName, branchname = bb.BranchName });
                    List<agents> dalist = new List<agents>();
                    var alist = (from al in db.AgentDetails where al.NewIntroducerId == newagentid select new { al.AgencyCode }).Distinct();
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
                        bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = bb.BranchName });

                    }

                    #endregion

                    #region Selecting CC Voucher Start Here

                    List<require> rlist = new List<require>();
                    var bonusamount = (from cts in db.bonus_tabs where cts.agentcode == newagentid && cts.month == month && cts.year == year select cts.bonusamount).DefaultIfEmpty(0).Sum();
                    ViewData["bonusamount"] = bonusamount;
                    ViewData["tdsper"] = tdsper;
                    var voucherno = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode select cts).Count();
                    var vyear = year.ToString();
                    var vcount = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode && cts.month == month && cts.year == vyear select cts).Count();

                    foreach (var bc in bclist)
                    {

                        var ar = db.AgentDetails.Single(a => a.NewAgentId == bc.newagentid);
                        List<plan> pclist = new List<plan>();

                        //------Inserting self voucher------------
                        var spclist = (from ct in db.commission_tabs where ct.bondintroducerid == ar.AgencyCode && ct.newagentid == br.NewAgentId && ct.date.Month == month && ct.date.Year == year orderby ct.plancode select new { ct.plancode }).Distinct();
                        foreach (var p in spclist.ToList())
                        {
                            pclist.Add(new plan { plancode = p.plancode });
                        }

                        foreach (var pc in pclist)
                        {

                            var pr = db.NewPlans.Single(p => p.Plancode == pc.plancode);
                            var spylist = (from sct in db.commission_tabs where sct.bondintroducerid == ar.AgencyCode && sct.newagentid == br.NewAgentId && sct.date.Month == month && sct.date.Year == year && sct.plancode == pc.plancode orderby sct.year select new { sct.year }).Distinct();
                            foreach (var spy in spylist.ToList())
                            {

                                var business = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == ar.AgencyCode && cts.newagentid == br.NewAgentId && cts.date.Month == month && cts.date.Year == year select cts.amount).DefaultIfEmpty(0).Sum();
                                var commission = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == ar.AgencyCode && cts.newagentid == br.NewAgentId && cts.date.Month == month && cts.date.Year == year select cts.commission).DefaultIfEmpty(0).Sum();
                                var percentage = (commission / business) * 100;
                                rlist.Add(new require { agentid = ar.AgencyCode, newagentid = ar.NewAgentId, name = ar.name, rankname = ar.RankName, planname = pr.Planname, year = spy.year, business = business, percentage = percentage, commission = commission, tds = tdsper, bonusamount = bonusamount, brokerid = br.NewAgentId, brokername = br.name, brokerrank = br.RankName, newintroducerid = br.NewIntroducerId, month = monthname, voucherno = voucherno + 1 });

                            }
                        }

                        //------End Inserting self voucher----------------

                    }
                    var gbusiness = rlist.Sum(tr => tr.business);
                    var gcommission = rlist.Sum(tr => tr.commission);
                    var lesstds = Math.Round((gcommission * tdsper) / 100, 2);
                    var netamount = Math.Round((gcommission + bonusamount - lesstds), 2);
                    vlist.Add(new VoucherList { newagentid = br.NewAgentId, name = br.name, introducerid = br.NewIntroducerId, amount = netamount, status = vcount, month = month, year = year });

                    #endregion
                }
                else
                {
                    Response.Write("<script>alert('Voucher is not released For this month..')</script>");
                }
                pdate = paydate;
                return View(vlist);
            }

        }
        [HttpGet]
        public ActionResult PaymentOBVTwo(string newagentid, int month = 0, int year = 0)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                MonthName mn = new MonthName();
                var monthname = mn.numbertomonthname(month) + "," + year;
                List<VoucherList> vlist = new List<VoucherList>();

                #region Selecting Complete Chain Start Here

                int rcount = 0;
                List<BrokerChain> bclist = new List<BrokerChain>();
                var br = db.AgentDetails.Single(a => a.NewAgentId == newagentid);
                var opr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
                var bb = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
                var tdsr = db.TDSLF_tabs.Single(t => t.Id == 1);
                Double tdsper = 0;
                if (br.Panno == null || br.Panno == "")
                {
                    tdsper = tdsr.NPCTDS;
                }
                else
                {
                    tdsper = tdsr.TDS;
                }
                bclist.Add(new BrokerChain { newagentid = br.NewAgentId, name = br.name, rankcode = br.RankCode, rankname = br.RankName, introducerid = br.NewIntroducerId, introname = br.IntroName, branchname = bb.BranchName });
                List<agents> dalist = new List<agents>();
                var alist = (from al in db.AgentDetails where al.NewIntroducerId == newagentid select new { al.AgencyCode }).Distinct();
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
                    bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = bb.BranchName });

                }

                #endregion

                #region Selecting CC Voucher Start Here

                List<require> rlist = new List<require>();
                var bonusamount = (from cts in db.bonus_tabs where cts.agentcode == newagentid && cts.month == month && cts.year == year select cts.bonusamount).DefaultIfEmpty(0).Sum();
                ViewData["bonusamount"] = bonusamount;
                ViewData["tdsper"] = tdsper;
                var voucherno = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode select cts).Count();
                var vyear = year.ToString();
                var vcount = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode && cts.month == month && cts.year == vyear select cts).Count();

                foreach (var bc in bclist)
                {

                    var ar = db.AgentDetails.Single(a => a.NewAgentId == bc.newagentid);
                    List<plan> pclist = new List<plan>();

                    //------Inserting self voucher------------
                    var spclist = (from ct in db.commission_tabs where ct.bondintroducerid == ar.AgencyCode && ct.newagentid == br.NewAgentId && ct.date.Month == month && ct.date.Year == year orderby ct.plancode select new { ct.plancode }).Distinct();
                    foreach (var p in spclist.ToList())
                    {
                        pclist.Add(new plan { plancode = p.plancode });
                    }

                    foreach (var pc in pclist)
                    {

                        var pr = db.NewPlans.Single(p => p.Plancode == pc.plancode);
                        var spylist = (from sct in db.commission_tabs where sct.bondintroducerid == ar.AgencyCode && sct.newagentid == br.NewAgentId && sct.date.Month == month && sct.date.Year == year && sct.plancode == pc.plancode orderby sct.year select new { sct.year }).Distinct();
                        foreach (var spy in spylist.ToList())
                        {

                            var business = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == ar.AgencyCode && cts.newagentid == br.NewAgentId && cts.date.Month == month && cts.date.Year == year select cts.amount).DefaultIfEmpty(0).Sum();
                            var commission = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == ar.AgencyCode && cts.newagentid == br.NewAgentId && cts.date.Month == month && cts.date.Year == year select cts.commission).DefaultIfEmpty(0).Sum();
                            var percentage = (commission / business) * 100;
                            rlist.Add(new require { agentid = ar.AgencyCode, newagentid = ar.NewAgentId, name = ar.name, rankname = ar.RankName, planname = pr.Planname, year = spy.year, business = business, percentage = percentage, commission = commission, tds = tdsper, bonusamount = bonusamount, brokerid = br.NewAgentId, brokername = br.name, brokerrank = br.RankName, newintroducerid = br.NewIntroducerId, month = monthname, voucherno = voucherno + 1 });

                        }
                    }

                    //------End Inserting self voucher----------------

                }

                #endregion
                vnewagentid = newagentid;
                vmonth = month;
                vvyear = year;
                return View(rlist);
            }
        }
        public ActionResult PrintCCVoucher()
        {
            MonthName mn = new MonthName();
            var monthname = mn.numbertomonthname(vmonth) + "," + vvyear;
            List<VoucherList> vlist = new List<VoucherList>();

            #region Selecting Complete Chain Start Here

            int rcount = 0;
            List<BrokerChain> bclist = new List<BrokerChain>();
            var br = db.AgentDetails.Single(a => a.NewAgentId == vnewagentid);
            var opr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
            var bb = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
            var tdsr = db.TDSLF_tabs.Single(t => t.Id == 1);
            Double tdsper = 0;
            if (br.Panno == null || br.Panno == "")
            {
                tdsper = tdsr.NPCTDS;
            }
            else
            {
                tdsper = tdsr.TDS;
            }
            ViewData["tdsper"] = tdsper;
            bclist.Add(new BrokerChain { newagentid = br.NewAgentId, name = br.name, rankcode = br.RankCode, rankname = br.RankName, introducerid = br.NewIntroducerId, introname = br.IntroName, branchname = bb.BranchName });
            List<agents> dalist = new List<agents>();
            var alist = (from al in db.AgentDetails where al.NewIntroducerId == vnewagentid select new { al.AgencyCode }).Distinct();
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
                bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = bb.BranchName });

            }

            #endregion

            #region Selecting CC Voucher Start Here

            List<require> rlist = new List<require>();
            var bonusamount = (from cts in db.bonus_tabs where cts.agentcode == vnewagentid && cts.month == vmonth && cts.year == vvyear select cts.bonusamount).DefaultIfEmpty(0).Sum();
            ViewData["bonusamount"] = bonusamount;
            var voucherno = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode select cts).Count();
            var vyear = vvyear.ToString();
            var vcount = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode && cts.month == vmonth && cts.year == vyear select cts).Count();

            foreach (var bc in bclist)
            {

                var ar = db.AgentDetails.Single(a => a.NewAgentId == bc.newagentid);
                List<plan> pclist = new List<plan>();

                //------Inserting self voucher------------
                var spclist = (from ct in db.commission_tabs where ct.bondintroducerid == ar.AgencyCode && ct.newagentid == br.NewAgentId && ct.date.Month == vmonth && ct.date.Year == vvyear orderby ct.plancode select new { ct.plancode }).Distinct();
                foreach (var p in spclist.ToList())
                {
                    pclist.Add(new plan { plancode = p.plancode });
                }

                foreach (var pc in pclist)
                {

                    var pr = db.NewPlans.Single(p => p.Plancode == pc.plancode);
                    var spylist = (from sct in db.commission_tabs where sct.bondintroducerid == ar.AgencyCode && sct.newagentid == br.NewAgentId && sct.date.Month == vmonth && sct.date.Year == vvyear && sct.plancode == pc.plancode orderby sct.year select new { sct.year }).Distinct();
                    foreach (var spy in spylist.ToList())
                    {

                        var business = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == ar.AgencyCode && cts.newagentid == br.NewAgentId && cts.date.Month == vmonth && cts.date.Year == vvyear select cts.amount).DefaultIfEmpty(0).Sum();
                        var commission = (from cts in db.commission_tabs where cts.planname == pr.Planname && cts.year == spy.year && cts.bondintroducerid == ar.AgencyCode && cts.newagentid == br.NewAgentId && cts.date.Month == vmonth && cts.date.Year == vvyear select cts.commission).DefaultIfEmpty(0).Sum();
                        var percentage = (commission / business) * 100;
                        NumberToEnglish n = new NumberToEnglish();
                        var amountinwords = n.changeToWords(0.ToString(), true);
                        rlist.Add(new require { domainname = cg.HeadOffice, amountinword = amountinwords, agentid = ar.AgencyCode, newagentid = ar.NewAgentId, name = ar.name, rankname = ar.RankName, planname = pr.Planname, year = spy.year, business = business, percentage = percentage, commission = commission, tds = tdsper, bonusamount = bonusamount, brokerid = br.NewAgentId, brokername = br.name, brokerrank = br.RankName, panno = br.Panno, newintroducerid = br.NewIntroducerId, month = monthname, voucherno = voucherno + 1 });

                    }
                }

                //------End Inserting self voucher----------------

            }

            #endregion


            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "CompleteCCVoucher.rpt"));
            rd.SetDataSource(rlist);

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
        public ActionResult BWBrokerVoucher(string newagentid, int month = 0, int year = 0, int status = 0)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                List<VoucherList> vlist = new List<VoucherList>();
                if (newagentid != null)
                {
                    BWSaveVoucher(newagentid, month, year, pdate);

                    /////////Selecting Voucher List Start Here//////
                    MonthName mn = new MonthName();
                    var monthname = mn.numbertomonthname(month) + "," + year;

                    List<BondwiseVoucherlist> bwvlist = new List<BondwiseVoucherlist>();
                    List<AgentDetail> blist = new List<AgentDetail>();
                    var cr = db.CompanyInfos.Single(c => c.Id == 1);
                    var mr = db.Members.Single(m => m.Id == 1);
                    var opr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
                    var bb = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
                    var br = db.AgentDetails.Single(a => a.NewAgentId == newagentid);
                    var tdsr = db.TDSLF_tabs.Single(t => t.Id == 1);
                    Double tdsper = 0;
                    if (br.Panno == null || br.Panno == "")
                    {
                        tdsper = tdsr.NPCTDS;
                    }
                    else
                    {
                        tdsper = tdsr.TDS;
                    }
                    var bonusamount = (from cts in db.bonus_tabs where cts.agentcode == newagentid && cts.month == month && cts.year == year select cts.bonusamount).DefaultIfEmpty(0).Sum();
                    ViewData["bonusamount"] = bonusamount;
                    ViewData["tdsper"] = tdsper;
                    var voucherno = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode select cts).Count();
                    var vyear = year.ToString();
                    var vcount = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode && cts.month == month && cts.year == vyear select cts).Count();
                    var commlist = (from co in db.commission_tabs where co.newagentid == newagentid && co.date.Month == month && co.date.Year == year select co).ToList();
                    foreach (var com in commlist)
                    {
                        var appr = db.appltabs.Single(b => b.newbondid == com.newbondid);
                        bwvlist.Add(new BondwiseVoucherlist { newbondid = com.newbondid, bondname = com.bondname, bondintroducerid = appr.newintroducerid, planname = appr.planname, mode = appr.mode, percentage = com.percentage, business = com.amount, commission = com.commission, newrenew = com.newrenew, tds = tdsper, bonusamount = bonusamount, brokerid = newagentid, brokername = br.name, brokerrank = br.RankName, newintroducerid = br.NewIntroducerId, month = monthname, voucherno = voucherno, branchname = bb.BranchName, companyname = cr.CompanyName, branch = mr.branchname, agent = mr.agentname, bond = mr.custname, plan = mr.planname });
                    }

                    var gbusiness = bwvlist.Sum(tr => tr.business);
                    var gcommission = bwvlist.Sum(tr => tr.commission);
                    var lesstds = Math.Round((gcommission * tdsper) / 100, 2);
                    var netamount = Math.Round((gcommission + bonusamount - lesstds), 2);
                    vlist.Add(new VoucherList { newagentid = br.NewAgentId, name = br.name, introducerid = br.NewIntroducerId, amount = netamount, status = vcount, month = month, year = year });

                }
                return View(vlist);
            }

        }
        [HttpPost]
        public ActionResult BWBrokerVoucher(string newagentid, DateTime paydate, int month = 0, int year = 0)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                MonthName mn = new MonthName();
                var monthname = mn.numbertomonthname(month) + "," + year;
                List<VoucherList> vlist = new List<VoucherList>();
                var yyear = year.ToString();
                var vrcount = db.ReleaseTabs.Where(c => c.month == month && c.year == yyear).Count();
                if (vrcount > 0)
                {
                    List<BondwiseVoucherlist> bwvlist = new List<BondwiseVoucherlist>();
                    List<AgentDetail> blist = new List<AgentDetail>();
                    var cr = db.CompanyInfos.Single(c => c.Id == 1);
                    var mr = db.Members.Single(m => m.Id == 1);
                    var opr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
                    var bb = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
                    var br = db.AgentDetails.Single(a => a.NewAgentId == newagentid);
                    var tdsr = db.TDSLF_tabs.Single(t => t.Id == 1);
                    Double tdsper = 0;
                    if (br.Panno == null || br.Panno == "")
                    {
                        tdsper = tdsr.NPCTDS;
                    }
                    else
                    {
                        tdsper = tdsr.TDS;
                    }
                    var bonusamount = (from cts in db.bonus_tabs where cts.agentcode == newagentid && cts.month == month && cts.year == year select cts.bonusamount).DefaultIfEmpty(0).Sum();
                    ViewData["bonusamount"] = bonusamount;
                    ViewData["tdsper"] = tdsper;
                    var voucherno = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode select cts).Count();
                    var vyear = year.ToString();
                    var vcount = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode && cts.month == month && cts.year == vyear select cts).Count();
                    var commlist = (from co in db.commission_tabs where co.newagentid == newagentid && co.date.Month == month && co.date.Year == year select co).ToList();
                    foreach (var com in commlist)
                    {
                        var appr = db.appltabs.Single(b => b.newbondid == com.newbondid);
                        bwvlist.Add(new BondwiseVoucherlist { newbondid = com.newbondid, bondname = com.bondname, bondintroducerid = appr.newintroducerid, planname = appr.planname, mode = appr.mode, percentage = com.percentage, business = com.amount, commission = com.commission, newrenew = com.newrenew, tds = tdsper, bonusamount = bonusamount, brokerid = newagentid, brokername = br.name, brokerrank = br.RankName, newintroducerid = br.NewIntroducerId, month = monthname, voucherno = voucherno, branchname = bb.BranchName, companyname = cr.CompanyName, branch = mr.branchname, agent = mr.agentname, bond = mr.custname, plan = mr.planname });
                    }

                    var gbusiness = bwvlist.Sum(tr => tr.business);
                    var gcommission = bwvlist.Sum(tr => tr.commission);
                    var lesstds = Math.Round((gcommission * tdsper) / 100, 2);
                    var netamount = Math.Round((gcommission + bonusamount - lesstds), 2);
                    vlist.Add(new VoucherList { newagentid = br.NewAgentId, name = br.name, introducerid = br.NewIntroducerId, amount = netamount, status = vcount, month = month, year = year });
                }
                else
                {
                    Response.Write("<script>alert('Voucher is not released For this month..')</script>");
                }

                pdate = paydate;
                return View(vlist);


            }

        }

        [HttpGet]
        public ActionResult PaymentOBVThree(string newagentid, int month = 0, int year = 0)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                MonthName mn = new MonthName();
                var monthname = mn.numbertomonthname(month) + "," + year;
                List<VoucherList> vlist = new List<VoucherList>();
                List<BondwiseVoucherlist> bwvlist = new List<BondwiseVoucherlist>();
                List<AgentDetail> blist = new List<AgentDetail>();
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var mr = db.Members.Single(m => m.Id == 1);
                var opr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
                var bb = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
                var br = db.AgentDetails.Single(a => a.NewAgentId == newagentid);
                var tdsr = db.TDSLF_tabs.Single(t => t.Id == 1);
                Double tdsper = 0;
                if (br.Panno == null || br.Panno == "")
                {
                    tdsper = tdsr.NPCTDS;
                }
                else
                {
                    tdsper = tdsr.TDS;
                }
                var bonusamount = (from cts in db.bonus_tabs where cts.agentcode == newagentid && cts.month == month && cts.year == year select cts.bonusamount).DefaultIfEmpty(0).Sum();
                ViewData["bonusamount"] = bonusamount;
                ViewData["tdsper"] = tdsper;
                var voucherno = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode select cts).Count();
                var vyear = year.ToString();
                var vcount = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode && cts.month == month && cts.year == vyear select cts).Count();
                var commlist = (from co in db.commission_tabs where co.newagentid == newagentid && co.date.Month == month && co.date.Year == year select co).ToList();
                foreach (var com in commlist)
                {
                    var appr = db.appltabs.Single(b => b.newbondid == com.newbondid);
                    bwvlist.Add(new BondwiseVoucherlist { newbondid = com.newbondid, bondname = com.bondname, bondintroducerid = appr.newintroducerid, planname = appr.planname, mode = appr.mode, percentage = com.percentage, business = com.amount, commission = com.commission, newrenew = com.newrenew, tds = tdsper, bonusamount = bonusamount, brokerid = newagentid, brokername = br.name, brokerrank = br.RankName, newintroducerid = br.NewIntroducerId, month = monthname, voucherno = voucherno, branchname = bb.BranchName, companyname = cr.CompanyName, branch = mr.branchname, agent = mr.agentname, bond = mr.custname, plan = mr.planname });
                }
                vnewagentid = newagentid;
                vmonth = month;
                vvyear = year;
                return View(bwvlist);
            }
        }
        public ActionResult PrintBWVoucher()
        {
            MonthName mn = new MonthName();
            var monthname = mn.numbertomonthname(vmonth) + "," + vvyear;
            List<VoucherList> vlist = new List<VoucherList>();
            List<BondwiseVoucherlist> bwvlist = new List<BondwiseVoucherlist>();
            List<AgentDetail> blist = new List<AgentDetail>();
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var mr = db.Members.Single(m => m.Id == 1);
            var opr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
            var bb = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
            var br = db.AgentDetails.Single(a => a.NewAgentId == vnewagentid);
            var tdsr = db.TDSLF_tabs.Single(t => t.Id == 1);
            Double tdsper = 0;
            if (br.Panno == null || br.Panno == "")
            {
                tdsper = tdsr.NPCTDS;
            }
            else
            {
                tdsper = tdsr.TDS;
            }
            ViewData["tdsper"] = tdsper;
            var bonusamount = (from cts in db.bonus_tabs where cts.agentcode == vnewagentid && cts.month == vmonth && cts.year == vvyear select cts.bonusamount).DefaultIfEmpty(0).Sum();
            ViewData["bonusamount"] = bonusamount;

            var voucherno = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode select cts).Count();
            var vyear = vvyear.ToString();
            var vcount = (from cts in db.Voucher_Reports where cts.agentid == br.AgencyCode && cts.month == vmonth && cts.year == vyear select cts).Count();
            var commlist = (from co in db.commission_tabs where co.newagentid == vnewagentid && co.date.Month == vmonth && co.date.Year == vvyear select co).ToList();
            foreach (var com in commlist)
            {
                var appr = db.appltabs.Single(b => b.newbondid == com.newbondid);
                bwvlist.Add(new BondwiseVoucherlist { newbondid = com.newbondid, bondname = com.bondname, bondintroducerid = appr.newintroducerid, planname = appr.planname, mode = appr.mode, percentage = com.percentage, business = com.amount, commission = com.commission, newrenew = com.newrenew, tds = tdsper, bonusamount = bonusamount, brokerid = vnewagentid, brokername = br.name, brokerrank = br.RankName, panno = br.Panno, newintroducerid = br.NewIntroducerId, month = monthname, voucherno = voucherno, branchname = bb.BranchName, companyname = cr.CompanyName, branch = mr.branchname, agent = mr.agentname, bond = mr.custname, plan = mr.planname });
            }



            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "CompleteBWVoucher.rpt"));
            rd.SetDataSource(bwvlist);

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

        #endregion

        #region Field Reports Start Here

        [HttpGet]
        public ActionResult Broker(string brokerid, int s = 0)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                if (brokerid == "" || brokerid == null)
                {
                    var br = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                    var dr = (from o in db.AgentDetails where o.BranchCode == br.BranchCode && o.Status == 1 orderby o.NewAgentId select o).ToList();

                    return View(dr);
                }
                else
                {
                    var dn = (from a in db.AgentDetails where a.NewAgentId == brokerid select a).ToList();
                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Broker.rpt"));
                    rd.SetDataSource(dn);

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

                    return View(dn);
                }

            }
        }
        public ActionResult PrintBrokerList()
        {

            var opr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
            var br = db.Branchtabs.Single(o => o.BranchCode == opr.BranchCode);
            List<BrokerList> blist = new List<BrokerList>();
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var brolist = (from o in db.AgentDetails where o.BranchCode == br.BranchCode && o.Status == 1 orderby o.NewAgentId select o).ToList();
            foreach (var b in brolist)
            {
                var tp = "";
                var aa = db.NewLogins.Where(n => n.UserName == b.NewAgentId && n.type == "Agent").Count();
                if (aa == 1)
                {
                    var ng = db.NewLogins.Single(p => p.UserName == b.NewAgentId && p.type == "Agent");
                    tp = ng.Password;
                }
                blist.Add(new BrokerList { password = tp, newagentid = b.NewAgentId, name = b.name, rankname = b.RankName, introducerid = b.NewIntroducerId, mobile = b.Mobile, doj = b.Doj, branchname = br.BranchName, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
            }

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Brokerlist.rpt"));
            rd.SetDataSource(blist);

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
        public ActionResult DatewiseBroker()
        {
            List<BrokerList> dwblist = new List<BrokerList>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {


                return View(dwblist);
            }
        }
        [HttpPost]
        public ActionResult DatewiseBroker(DateTime sdate, DateTime edate)
        {
            List<BrokerList> dwblist = new List<BrokerList>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var pr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);
                var blist = (from g in db.AgentDetails where g.BranchCode == pr.BranchCode && g.Doj >= sdate && g.Doj <= edate && g.Status == 1 orderby g.NewAgentId select g).ToList();
                foreach (var b in blist)
                {
                    dwblist.Add(new BrokerList { newagentid = b.NewAgentId, name = b.name, rankname = b.RankName, introducerid = b.NewIntroducerId, mobile = b.Mobile, doj = b.Doj, branchname = br.BranchName, sdate = sdate, edate = edate });
                }

                stdate = sdate;
                enddate = edate;
                return View(dwblist);

            }
        }
        public ActionResult PrintDatewiseBroker()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                List<BrokerList> dwblist = new List<BrokerList>();
                var pr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var blist = (from g in db.AgentDetails where g.BranchCode == pr.BranchCode && g.Doj >= stdate && g.Doj <= enddate && g.Status == 1 orderby g.NewAgentId select g).ToList();
                foreach (var b in blist)
                {
                    dwblist.Add(new BrokerList { newagentid = b.NewAgentId, name = b.name, rankname = b.RankName, introducerid = b.NewIntroducerId, mobile = b.Mobile, doj = b.Doj, branchname = br.BranchName, sdate = stdate, edate = enddate, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                }

                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "DatewiseBroker.rpt"));
                rd.SetDataSource(dwblist);

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

                return View(dwblist);
            }
        }

        [HttpGet]
        public ActionResult BrokerChain()
        {
            List<BrokerChain> bclist = new List<BrokerChain>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(bclist);
            }
        }
        [HttpPost]
        public ActionResult BrokerChain(string newagentid)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                var bb = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);
                int rcount = 0;
                List<BrokerChain> bclist = new List<BrokerChain>();
                var br = db.AgentDetails.Single(a => a.NewAgentId == newagentid);
                bclist.Add(new BrokerChain { newagentid = br.NewAgentId, name = br.name, rankcode = br.RankCode, rankname = br.RankName, introducerid = br.NewIntroducerId, introname = br.IntroName, branchname = bb.BranchName, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                List<agents> dalist = new List<agents>();
                var alist = (from al in db.AgentDetails where al.NewIntroducerId == newagentid select new { al.AgencyCode }).Distinct();
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
                        bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = bb.BranchName, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                    }
                }
                NewAgentid = newagentid;
                return View(bclist);
            }
        }
        public ActionResult PrintBrokerChainReport()
        {
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var pr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
            var bb = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);
            int rcount = 0;
            List<BrokerChain> bclist = new List<BrokerChain>();
            var br = db.AgentDetails.Single(a => a.NewAgentId == NewAgentid);
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
                if (ad.Status == 1)
                {
                    bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = bb.BranchName, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                }
            }

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports/Branch"), "BrokerChain.rpt"));
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
        public ActionResult BrokerPaymentDetail()
        {
            List<BrokerPayment> bplist = new List<BrokerPayment>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(bplist);
            }
        }
        [HttpPost]
        public ActionResult BrokerPaymentDetail(string newagentid)
        {
            List<BrokerPayment> bplist = new List<BrokerPayment>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                TempData["newagentid"] = newagentid;
                DateTime fdate = DateTime.Now.Date;
                DateTime sdate = DateTime.Now.Date;
                DateTime fdate1 = DateTime.Now.Date;
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var mr = db.Members.Single(c => c.Id == 1);
                var pr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);
                var ar = db.AgentDetails.Single(a => a.NewAgentId == newagentid);
                int min = (from o in db.commission_tabs where o.agentid == ar.AgencyCode && o.branchcode == br.BranchCode select o.Id).DefaultIfEmpty(0).Min();
                var comm = db.commission_tabs.Single(s => s.Id == min);
                DateTime dd = comm.date;

                while (dd <= DateTime.Now.Date)
                {
                    var year = dd.Year;
                    var year1 = year.ToString();
                    var month = dd.Month;
                    int mon = DateTime.DaysInMonth(year, month);
                    if (month > 9)
                    {
                        sdate = Convert.ToDateTime(month + "/01/" + year);
                        fdate = Convert.ToDateTime(month + "/" + mon + "/" + year);
                    }
                    else
                    {
                        sdate = Convert.ToDateTime("0" + month + "/01/" + year);
                        fdate = Convert.ToDateTime("0" + month + "/" + mon + " /" + year);
                    }
                    var count = (from co in db.Voucher_Reports where co.month == month && co.year == year1 && co.branchcode == br.BranchCode && co.agentid == ar.AgencyCode select co.Id).Count();
                    if (count >= 1)
                    {
                        MonthName mn = new MonthName();
                        var monthname = mn.numbertomonthname(month);
                        var bv = db.Voucher_Reports.Single(s => s.agentid == ar.AgencyCode && comm.branchcode == br.BranchCode && s.month == month && s.year == year1);
                        bplist.Add(new BrokerPayment { status = "Paid", newagentid = newagentid, name = ar.name, Business = bv.bussiness, commission = bv.commission, tds = bv.tds, netamount = bv.netamount, month = monthname, year = bv.year, pdate = bv.date, branchname = User.Identity.Name, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                    }
                    else
                    {
                        int mon1 = DateTime.DaysInMonth(year, month);
                        if (month > 9)
                        {
                            sdate = Convert.ToDateTime(month + "/01/" + year);
                            fdate = Convert.ToDateTime(month + "/" + mon1 + "/" + year);
                        }
                        else
                        {
                            sdate = Convert.ToDateTime("0" + month + "/01/" + year);
                            fdate = Convert.ToDateTime("0" + month + "/" + mon1 + " /" + year);
                        }
                        MonthName mn = new MonthName();
                        var monthname = mn.numbertomonthname(month);
                        var sumcomm = (from o in db.commission_tabs where o.date >= sdate && o.date <= fdate && o.branchcode == br.BranchCode && o.agentid == ar.AgencyCode select o.commission).DefaultIfEmpty(0).Sum();
                        var business = (from o in db.commission_tabs where o.date >= sdate && o.date <= fdate && o.branchcode == br.BranchCode && o.agentid == ar.AgencyCode select o.amount).DefaultIfEmpty(0).Sum();
                        bplist.Add(new BrokerPayment { status = "UnPaid", newagentid = newagentid, name = ar.name, Business = business, commission = sumcomm, tds = 0, netamount = 0, month = monthname, year = year1, pdate = DateTime.Now.Date, branchname = User.Identity.Name, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                    }
                    dd = sdate.AddMonths(1);
                }
                NewAgentid = newagentid;
                return View(bplist);
            }
        }
        public ActionResult PrintBrokerPaymentReport(string newagentid)
        {
            List<BrokerPayment> bplist = new List<BrokerPayment>();
            DateTime fdate = DateTime.Now.Date;
            DateTime sdate = DateTime.Now.Date;
            DateTime fdate1 = DateTime.Now.Date;
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var mr = db.Members.Single(c => c.Id == 1);
            var pr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
            var br = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);
            var ar = db.AgentDetails.Single(a => a.NewAgentId == newagentid);
            int min = (from o in db.commission_tabs where o.agentid == ar.AgencyCode && o.branchcode == br.BranchCode select o.Id).DefaultIfEmpty(0).Min();
            var comm = db.commission_tabs.Single(s => s.Id == min);
            DateTime dd = comm.date;

            while (dd <= DateTime.Now.Date)
            {
                var year = dd.Year;
                var year1 = year.ToString();
                var month = dd.Month;
                int mon = DateTime.DaysInMonth(year, month);
                if (month > 9)
                {
                    sdate = Convert.ToDateTime(month + "/01/" + year);
                    fdate = Convert.ToDateTime(month + "/" + mon + "/" + year);
                }
                else
                {
                    sdate = Convert.ToDateTime("0" + month + "/01/" + year);
                    fdate = Convert.ToDateTime("0" + month + "/" + mon + " /" + year);
                }
                var count = (from co in db.Voucher_Reports where co.month == month && co.year == year1 && co.branchcode == br.BranchCode && co.agentid == ar.AgencyCode select co.Id).Count();
                if (count >= 1)
                {
                    MonthName mn = new MonthName();
                    var monthname = mn.numbertomonthname(month);
                    var bv = db.Voucher_Reports.Single(s => s.agentid == ar.AgencyCode && comm.branchcode == br.BranchCode && s.month == month && s.year == year1);
                    bplist.Add(new BrokerPayment { status = "Paid", newagentid = NewAgentid, name = ar.name, Business = bv.bussiness, commission = bv.commission, tds = bv.tds, netamount = bv.netamount, month = monthname, year = bv.year, pdate = bv.date, branchname = User.Identity.Name, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                }
                else
                {
                    int mon1 = DateTime.DaysInMonth(year, month);
                    if (month > 9)
                    {
                        sdate = Convert.ToDateTime(month + "/01/" + year);
                        fdate = Convert.ToDateTime(month + "/" + mon1 + "/" + year);
                    }
                    else
                    {
                        sdate = Convert.ToDateTime("0" + month + "/01/" + year);
                        fdate = Convert.ToDateTime("0" + month + "/" + mon1 + " /" + year);
                    }
                    MonthName mn = new MonthName();
                    var monthname = mn.numbertomonthname(month);
                    var sumcomm = (from o in db.commission_tabs where o.date >= sdate && o.date <= fdate && o.branchcode == br.BranchCode && o.agentid == ar.AgencyCode select o.commission).DefaultIfEmpty(0).Sum();
                    var business = (from o in db.commission_tabs where o.date >= sdate && o.date <= fdate && o.branchcode == br.BranchCode && o.agentid == ar.AgencyCode select o.amount).DefaultIfEmpty(0).Sum();
                    bplist.Add(new BrokerPayment { status = "UnPaid", newagentid = NewAgentid, name = ar.name, Business = business, commission = sumcomm, tds = 0, netamount = 0, month = monthname, year = year1, pdate = DateTime.Now.Date, branchname = User.Identity.Name, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                }
                dd = sdate.AddMonths(1);
            }

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "BrokerPayment.rpt"));
            rd.SetDataSource(bplist);

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
            return View(bplist);
        }

        [HttpGet]
        public ActionResult Selfbusiness()
        {
            List<DatewiseDueCollection> dwl = new List<DatewiseDueCollection>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(dwl);
            }
        }
        [HttpPost]
        public ActionResult Selfbusiness(string newintroducerid)
        {
            List<DatewiseDueCollection> dwl = new List<DatewiseDueCollection>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var ar = db.AgentDetails.Single(a => a.NewAgentId == newintroducerid);
                var amt = 0.0;
                var bondintro = (from o in db.appltabs where o.newintroducerid == newintroducerid select o).ToList();
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                foreach (var c in bondintro)
                {
                    if (c.plantype == "PPS")
                    {
                        amt = c.totalcon;
                    }
                    else
                    {
                        amt = (from n in db.Installmenttabs where n.newbondid == c.newbondid && n.payamount != 0 select n.payamount).DefaultIfEmpty(0).Sum();
                    }
                    dwl.Add(new DatewiseDueCollection { newbondid = c.newbondid, name = c.name, newintroducerid = newintroducerid, IntroName = ar.name, formdate = c.formdate, amount = amt, branchname = c.branchcode, companyname = cr.CompanyName });
                }
                NewAgentid = newintroducerid;
                return View(dwl);
            }
        }
        public ActionResult PrintSelfbusiness()
        {
            List<DatewiseDueCollection> dwl = new List<DatewiseDueCollection>();
            var bondintro = (from o in db.appltabs where o.newintroducerid == NewAgentid select o).ToList();
            var amt = 0.0;
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var ar = db.AgentDetails.Single(a => a.NewAgentId == NewAgentid);
            foreach (var c in bondintro)
            {
                if (c.plantype == "PPS")
                {
                    amt = c.totalcon;
                }
                else
                {
                    amt = (from n in db.Installmenttabs where n.newbondid == c.newbondid && n.payamount != 0 select n.payamount).DefaultIfEmpty(0).Sum();
                }
                dwl.Add(new DatewiseDueCollection { newbondid = c.newbondid, name = c.name, newintroducerid = NewAgentid, IntroName = ar.name, formdate = c.formdate, amount = amt, branchname = c.branchcode, companyname = cr.CompanyName });
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
        public ActionResult BrokerAccount()
        {
            List<BrokerAccount> balist = new List<BrokerAccount>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(balist);
            }
        }
        [HttpPost]
        public ActionResult BrokerAccount(string newintroducerid)
        {
            List<BrokerAccount> balist = new List<BrokerAccount>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var plan4 = db.Plans.Single(p4 => p4.Plancode == 4);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var mr = db.Members.Single(c => c.Id == 1);
                var pr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);
                var ar = db.AgentDetails.Single(n => n.NewAgentId == newintroducerid);

                int rcount = 0;
                List<BrokerChain> bclist = new List<BrokerChain>();
                bclist.Add(new BrokerChain { newagentid = ar.NewAgentId, name = ar.name, rankcode = ar.RankCode, rankname = ar.RankName, introducerid = ar.NewIntroducerId, introname = ar.IntroName, branchname = br.BranchName, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                List<agents> dalist = new List<agents>();
                var alist = (from al in db.AgentDetails where al.NewIntroducerId == newintroducerid select new { al.AgencyCode }).Distinct();
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
                    var arr = db.AgentDetails.Single(a => a.NewAgentId == bc.newagentid);
                    Double business = 0;
                    var blist = (from a in db.appltabs where a.newintroducerid == bc.newagentid && a.status == 1 orderby a.newbondid select a).ToList();
                    foreach (var b in blist)
                    {
                        var bamount = (from bb in db.Installmenttabs where bb.newbondid == b.newbondid select bb.payamount).DefaultIfEmpty(0).Sum();
                        business = business + bamount;
                    }
                    balist.Add(new BrokerAccount { newintroducerid = bc.newagentid, name = arr.name, newbondid = arr.NewIntroducerId, cname = arr.IntroName, planname = arr.Mobile, formdate = arr.Doj, amount = business, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });

                }
                NewAgentid = newintroducerid;
                return View(balist);
            }
        }
        public ActionResult PrintBrokerAccountReport()
        {
            List<BrokerAccount> balist = new List<BrokerAccount>();
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var mr = db.Members.Single(c => c.Id == 1);
            var pr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
            var br = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);
            var ar = db.AgentDetails.Single(n => n.NewAgentId == NewAgentid);
            var plan4 = db.Plans.Single(p4 => p4.Plancode == 4);

            int rcount = 0;
            List<BrokerChain> bclist = new List<BrokerChain>();
            bclist.Add(new BrokerChain { newagentid = ar.NewAgentId, name = ar.name, rankcode = ar.RankCode, rankname = ar.RankName, introducerid = ar.NewIntroducerId, introname = ar.IntroName, branchname = br.BranchName, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
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
                if (ad.Status == 1)
                {
                    bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = br.BranchName, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                }
            }

            foreach (var bc in bclist)
            {
                var arr = db.AgentDetails.Single(a => a.NewAgentId == bc.newagentid);
                Double business = 0;
                var blist = (from a in db.appltabs where a.newintroducerid == bc.newagentid && a.status == 1 orderby a.newbondid select a).ToList();
                foreach (var b in blist)
                {
                    var bamount = (from bb in db.Installmenttabs where bb.newbondid == b.newbondid select bb.payamount).DefaultIfEmpty(0).Sum();
                    business = business + bamount;
                }
                balist.Add(new BrokerAccount { newintroducerid = bc.newagentid, name = arr.name, newbondid = arr.NewIntroducerId, cname = arr.IntroName, planname = arr.Mobile, formdate = arr.Doj, amount = business, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });

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
            return View(balist);
        }

        [HttpGet]
        public ActionResult BrokerwiseAccountDetail()
        {
            List<BrokerwiseAccount> bwalist = new List<BrokerwiseAccount>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(bwalist);
            }
        }
        [HttpPost]
        public ActionResult BrokerwiseAccountDetail(string newagentid, DateTime sdate, DateTime edate)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var mr = db.Members.Single(c => c.Id == 1);
                var pr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);
                var ar = db.AgentDetails.Single(n => n.NewAgentId == newagentid);
                var plan4 = db.Plans.Single(p4 => p4.Plancode == 4);
                var bd = (from mn in db.appltabs where mn.newintroducerid == newagentid && mn.pdate >= sdate && mn.pdate <= edate && mn.status == 1 orderby mn.newbondid select new { mn.bondid }).Distinct();
                List<Tempbond> tb = new List<Tempbond>();
                List<BrokerwiseAccount> bwalist = new List<BrokerwiseAccount>();
                int count = bd.Count();
                //if (count > 0)
                //{
                foreach (var item in bd.ToList())
                {
                    tb.Add(new Tempbond { bondid = item.bondid });
                }

                foreach (var t in tb.ToList())
                {
                    var bondid = t.bondid;
                    var applist = db.appltabs.Single(a => a.bondid == bondid);
                    var totalinstall = (from io in db.Installmenttabs where io.bondid == bondid select io).Count();
                    var maxinstall = (from u in db.Installmenttabs where u.bondid == bondid && u.payamount != 0 select u.installmentno).DefaultIfEmpty(0).Max();
                    string nextinstall = (maxinstall + 1).ToString();
                    var row = db.Installmenttabs.Single(r => r.bondid == bondid && r.installmentno == maxinstall + 1);
                    string dt = (row.prevexpirydate).ToString("dd/MM/yyyy");
                    if (applist.plantype == plan4.shortcutname)
                    {
                        bwalist.Add(new BrokerwiseAccount { newintroducerid = newagentid, intname = ar.name, newbondid = applist.newbondid, issuedate = applist.formdate, name = applist.name, planname = applist.planname, term = applist.term, mode = applist.mode, maturitydate = applist.expirydate, totalinstall = totalinstall, nextinstall = nextinstall, amount = row.amount, nextdate = dt, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice, sdate = sdate, edate = edate });
                    }
                    else if (applist.plantype == "FD")
                    {
                        bwalist.Add(new BrokerwiseAccount { newintroducerid = newagentid, intname = ar.name, newbondid = applist.newbondid, issuedate = applist.formdate, name = applist.name, planname = applist.planname, term = applist.term, mode = applist.mode, maturitydate = applist.expirydate, totalinstall = totalinstall, nextinstall = "No Renewal", amount = row.amount, nextdate = "No Due", branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice, sdate = sdate, edate = edate });
                    }
                    else
                    {
                        bwalist.Add(new BrokerwiseAccount { newintroducerid = newagentid, intname = ar.name, newbondid = applist.newbondid, issuedate = applist.formdate, name = applist.name, planname = applist.planname, term = applist.term, mode = applist.mode, maturitydate = applist.expirydate, totalinstall = totalinstall, nextinstall = nextinstall, amount = row.amount, nextdate = dt, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice, sdate = sdate, edate = edate });
                    }
                }
                //}

                //else
                //{
                //    Response.Write("<script>alert('There is no bond related to this Agent')<script");

                //}

                NewAgentid = newagentid;
                stdate = sdate;
                enddate = edate;

                return View(bwalist);
            }
        }
        public ActionResult PrintBrokerwiseAccountReport()
        {
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var mr = db.Members.Single(c => c.Id == 1);
            var pr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
            var br = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);
            var ar = db.AgentDetails.Single(n => n.NewAgentId == NewAgentid);
            var plan4 = db.Plans.Single(p4 => p4.Plancode == 4);
            var bd = (from mn in db.appltabs where mn.newintroducerid == NewAgentid && mn.pdate >= stdate && mn.pdate <= enddate && mn.status == 1 orderby mn.newbondid select new { mn.bondid }).Distinct();
            List<Tempbond> tb = new List<Tempbond>();
            List<BrokerwiseAccount> bwalist = new List<BrokerwiseAccount>();
            int count = bd.Count();
            foreach (var item in bd.ToList())
            {
                tb.Add(new Tempbond { bondid = item.bondid });
            }

            foreach (var t in tb.ToList())
            {
                var bondid = t.bondid;
                var applist = db.appltabs.Single(a => a.bondid == bondid);
                var totalinstall = (from io in db.Installmenttabs where io.bondid == bondid select io).Count();
                var maxinstall = (from u in db.Installmenttabs where u.bondid == bondid && u.payamount != 0 select u.installmentno).DefaultIfEmpty(0).Max();
                string nextinstall = (maxinstall + 1).ToString();
                var row = db.Installmenttabs.Single(r => r.bondid == bondid && r.installmentno == maxinstall + 1);
                string dt = (row.prevexpirydate).ToString("dd/MM/yyyy");
                if (applist.plantype == plan4.shortcutname)
                {
                    bwalist.Add(new BrokerwiseAccount { newintroducerid = NewAgentid, intname = ar.name, newbondid = applist.newbondid, issuedate = applist.formdate, name = applist.name, planname = applist.planname, term = applist.term, mode = applist.mode, maturitydate = applist.expirydate, totalinstall = totalinstall, nextinstall = nextinstall, amount = row.amount, nextdate = dt, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice, sdate = stdate, edate = enddate });
                }
                else if (applist.plantype == "FD")
                {
                    bwalist.Add(new BrokerwiseAccount { newintroducerid = NewAgentid, intname = ar.name, newbondid = applist.newbondid, issuedate = applist.formdate, name = applist.name, planname = applist.planname, term = applist.term, mode = applist.mode, maturitydate = applist.expirydate, totalinstall = totalinstall, nextinstall = "No Renewal", amount = row.amount, nextdate = "No Due", branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice, sdate = stdate, edate = enddate });
                }
                else
                {
                    bwalist.Add(new BrokerwiseAccount { newintroducerid = NewAgentid, intname = ar.name, newbondid = applist.newbondid, issuedate = applist.formdate, name = applist.name, planname = applist.planname, term = applist.term, mode = applist.mode, maturitydate = applist.expirydate, totalinstall = totalinstall, nextinstall = nextinstall, amount = row.amount, nextdate = dt, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice, sdate = stdate, edate = enddate });
                }
            }


            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "BrokerwiseAccount.rpt"));
            rd.SetDataSource(bwalist);

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

            return View(bwalist);


        }

        [HttpGet]
        public ActionResult BrokerSelfCollection()
        {
            List<TempSelfCollection> bsclist = new List<TempSelfCollection>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(bsclist);
            }
        }
        [HttpPost]
        public ActionResult BrokerSelfCollection(string newagent, DateTime sdate, DateTime edate)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(op => op.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);
                List<Tempbond> tb = new List<Tempbond>();
                List<TempSelfCollection> bsclist = new List<TempSelfCollection>();

                var agcount = db.AgentDetails.Where(hj => hj.NewAgentId == newagent && hj.BranchCode == br.BranchCode).Count();
                if (agcount > 0)
                {
                    var ag = db.AgentDetails.Single(hj => hj.NewAgentId == newagent);

                    var tbd = (from ap in db.appltabs where ap.newintroducerid == newagent select ap.bondid);
                    var bd = (from yt in db.Installmenttabs where yt.paymentdate >= sdate && yt.paymentdate <= edate && yt.branch == br.BranchCode && tbd.Contains(yt.bondid) select new { yt.bondid }).Distinct();
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
                                        where o.bondid == bondid && o.payamount != 0 && o.paymentdate >= sdate && o.paymentdate <= edate && o.branch == br.BranchCode
                                        orderby u.newbondid
                                        select new { o.payamount, o.installmentno, o.year, o.planname, o.newbondid, o.plantype }).ToList();
                            int i = 0;
                            foreach (var g in list.ToList())
                            {
                                i = i + 1;
                                string subplan = g.plantype;
                                if (subplan == "FD")
                                {
                                    sf.Add(new SelfCollection { sis = g.payamount });
                                }

                                if (subplan == "RD" && g.installmentno == 1 && g.year == 1)
                                {
                                    sf.Add(new SelfCollection { MISFresh = g.payamount });
                                }

                                if (subplan == "MIS" && i == 1)
                                {

                                    var ppssum = db.appltabs.Single(b => b.newbondid == g.newbondid);
                                    sf.Add(new SelfCollection { PPSFresh = ppssum.totalcon });
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

                                if (subplan == "RD" && g.installmentno > 1 && g.year == 7)
                                {
                                    sf.Add(new SelfCollection { MIS7a = g.payamount });
                                }

                            }

                            double sissum = 0, misfsum = 0, mis1sum = 0, mis2sum = 0, mis3sum = 0, mis4sum = 0, mis5sum = 0, mis6sum = 0, mis7sum = 0, ppsum = 0;

                            sissum = sf.Sum(tr => tr.sis);
                            ppsum = sf.Sum(tr => tr.PPSFresh);
                            misfsum = sf.Sum(tr => tr.MISFresh);
                            mis1sum = sf.Sum(tr => tr.MIS1);
                            mis2sum = sf.Sum(tr => tr.MIS2);
                            mis3sum = sf.Sum(tr => tr.MIS3);
                            mis4sum = sf.Sum(tr => tr.MIS4);
                            mis5sum = sf.Sum(tr => tr.MIS5);
                            mis6sum = sf.Sum(tr => tr.MIS6);
                            mis7sum = sf.Sum(tr => tr.MIS7a);

                            var brow = db.appltabs.Single(bg => bg.bondid == bondid);
                            bsclist.Add(new TempSelfCollection { Newbondid = brow.newbondid, Customername = brow.name, brokername = ag.name, brokercode = ag.NewAgentId, tsis = sissum, tMISFresh = misfsum + sissum + ppsum, tPPSFresh = ppsum, tMIS1 = mis1sum, tMIS2 = mis2sum, tMIS3 = mis3sum, tMIS4 = mis4sum, tMIS5 = mis5sum, tMIS6 = mis6sum, tMIS7a = mis7sum, companyname = cr.CompanyName, branchname = br.BranchName, branch = mr.branchname, bond = mr.custname, agent = mr.agentname, plan = brow.nolandunit.ToString(), sdate = sdate, edate = edate });

                        }


                    }

                    else
                    {
                        Response.Write("<script>alert('There is no " + mr.custname + " related to this " + mr.agentname + "')</script>");
                    }
                }

                else
                {
                    Response.Write("<script>alert('This " + mr.agentname + " is not Register with your " + mr.branchname + "')</script>");

                }

                NewAgentid = newagent;
                stdate = sdate;
                enddate = edate;

                return View(bsclist);

            }
        }
        public ActionResult PrintselfCollection()
        {

            var mr = db.Members.Single(m => m.Id == 1);
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var pr = db.Operators.Single(op => op.OperatorId == User.Identity.Name);
            var br = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);
            List<Tempbond> tb = new List<Tempbond>();
            List<TempSelfCollection> bsclist = new List<TempSelfCollection>();
            var agcount = db.AgentDetails.Where(hj => hj.NewAgentId == NewAgentid && hj.BranchCode == br.BranchCode).Count();
            if (agcount > 0)
            {
                var ag = db.AgentDetails.Single(hj => hj.NewAgentId == NewAgentid);
                var tbd = (from ap in db.appltabs where ap.newintroducerid == NewAgentid select ap.bondid);
                var bd = (from yt in db.Installmenttabs where yt.paymentdate >= stdate && yt.paymentdate <= enddate && yt.branch == br.BranchCode && tbd.Contains(yt.bondid) select new { yt.bondid }).Distinct();
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
                                    where o.bondid == bondid && o.payamount != 0 && o.paymentdate >= stdate && o.paymentdate <= enddate && o.branch == br.BranchCode
                                    orderby u.newbondid
                                    select new { o.payamount, o.installmentno, o.year, o.planname, o.newbondid, o.plantype }).ToList();
                        int i = 0;
                        foreach (var g in list.ToList())
                        {
                            i = i + 1;
                            string subplan = g.plantype;
                            if (subplan == "FD")
                            {
                                sf.Add(new SelfCollection { sis = g.payamount });
                            }

                            if (subplan == "RD" && g.installmentno == 1 && g.year == 1)
                            {
                                sf.Add(new SelfCollection { MISFresh = g.payamount });
                            }

                            if (subplan == "MIS" && i == 1)
                            {

                                var ppssum = db.appltabs.Single(b => b.newbondid == g.newbondid);
                                sf.Add(new SelfCollection { PPSFresh = ppssum.totalcon });
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

                            if (subplan == "RD" && g.installmentno > 1 && g.year == 7)
                            {
                                sf.Add(new SelfCollection { MIS7a = g.payamount });
                            }

                        }

                        double sissum = 0, misfsum = 0, mis1sum = 0, mis2sum = 0, mis3sum = 0, mis4sum = 0, mis5sum = 0, mis6sum = 0, mis7sum = 0, ppsum = 0;

                        sissum = sf.Sum(tr => tr.sis);
                        ppsum = sf.Sum(tr => tr.PPSFresh);
                        misfsum = sf.Sum(tr => tr.MISFresh);
                        mis1sum = sf.Sum(tr => tr.MIS1);
                        mis2sum = sf.Sum(tr => tr.MIS2);
                        mis3sum = sf.Sum(tr => tr.MIS3);
                        mis4sum = sf.Sum(tr => tr.MIS4);
                        mis5sum = sf.Sum(tr => tr.MIS5);
                        mis6sum = sf.Sum(tr => tr.MIS6);
                        mis7sum = sf.Sum(tr => tr.MIS7a);

                        var brow = db.appltabs.Single(bg => bg.bondid == bondid);
                        bsclist.Add(new TempSelfCollection { Newbondid = brow.newbondid, Customername = brow.name, brokername = ag.name, brokercode = ag.NewAgentId, tsis = sissum, tMISFresh = misfsum + sissum + ppsum, tPPSFresh = ppsum, tMIS1 = mis1sum, tMIS2 = mis2sum, tMIS3 = mis3sum, tMIS4 = mis4sum, tMIS5 = mis5sum, tMIS6 = mis6sum, tMIS7a = mis7sum, companyname = cr.CompanyName, branchname = br.BranchName, branch = mr.branchname, bond = mr.custname, agent = mr.agentname, plan = brow.nolandunit.ToString(), sdate = stdate, edate = enddate });

                    }


                }

                else
                {
                    Response.Write("<script>alert('There is no " + mr.custname + " related to this " + mr.agentname + "')</script>");
                }
            }

            else
            {
                Response.Write("<script>alert('This " + mr.agentname + " is not Register with your " + mr.branchname + "')</script>");

            }

            ReportDocument rdp = new ReportDocument();
            rdp.Load(Path.Combine(Server.MapPath("~/Reports/Branch"), "Brokerselfcollection.rpt"));
            rdp.SetDataSource(bsclist);

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
        public ActionResult CreditAccount()
        {
            List<SavingAccountInfo> sainfo = new List<SavingAccountInfo>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(sainfo);
            }
        }
        [HttpPost]
        public ActionResult CreditAccount(TransactionTab tob, string command, string accno)
        {

            List<SavingAccountInfo> sainfo = new List<SavingAccountInfo>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {


                switch (command)
                {
                    case "Search":

                        switch (accno)
                        {
                            case "":
                                Response.Write("<script>alert('Please Enter A/C No First')</script>");
                                break;
                            default:

                                sainfo = db.SavingAccountInfos.Where(c => c.accountno == accno).ToList();
                                var cc = db.SavingAccountInfos.Single(c => c.accountno == accno);
                                if (cc.actype != "Self")
                                {
                                    ViewData["Joint"] = "Yes";
                                }
                                return View(sainfo);
                        }

                        break;

                    case "Submit":

                        var acr = db.SavingAccountInfos.Single(a => a.accountno == tob.accountno);
                        var row1 = db.Operators.Single(c => c.OperatorId == User.Identity.Name);
                        var csrt = db.CompanyInfos.Single(a => a.AdminId == row1.companyid);
                        var creditsum = (from df in db.TransactionTabs where df.accountno == tob.accountno select df.credit).DefaultIfEmpty(0).Sum();
                        var debitsum = (from df in db.TransactionTabs where df.accountno == tob.accountno select df.debit).DefaultIfEmpty(0).Sum();
                        var ac = tob.accountno.Substring(tob.accountno.Count() - 4, 4);
                        var balance = (creditsum + tob.credit - debitsum);

                        string aa = gid();
                        TransactionTab ad = new TransactionTab();
                        var j = db.TransactionTabs.Where(s => s.accountno == tob.accountno).Count();
                        ad.transactionid = aa;
                        ad.branchcode = row1.BranchCode;
                        ad.accountno = tob.accountno;
                        ad.actype = tob.actype;
                        ad.acholdername = tob.acholdername;
                        ad.paymethod = tob.paymethod;
                        ad.pdate = tob.pdate;
                        ad.checkorddno = tob.checkorddno;
                        ad.drawon = tob.drawon;
                        ad.credit = tob.credit;
                        ad.debit = 0;
                        ad.balance = balance;
                        ad.remark = tob.remark;
                        ad.opid = User.Identity.Name;
                        ad.status = (j + 1);
                        ad.type = "Credit";
                        ad.Time = DateTime.Now.ToShortTimeString();

                        db.TransactionTabs.Add(ad);
                        db.SaveChanges();
                        MyClass.Sendmsg(acr.mobileno, "Your A/c xxxxxx" + ac + " credited with INR " + tob.credit + " on " + DateTime.Now.ToString("dd/MM/yyyy") + "- Remark-Avl Bal. INR " + balance + ", For more details Visit www.naclgroup.com");
                        if (acr.actype != "Saving")
                        {
                            MyClass.Sendmsg(acr.mobileno2, "Your A/c xxxxxx" + ac + " credited with INR " + tob.credit + " on " + DateTime.Now.ToString("dd/MM/yyyy") + "- Remark-Avl Bal. INR " + balance + ", For more details Visit www.naclgroup.com");

                        }
                        ViewBag.msg = " A/c " + tob.accountno + " credited with INR " + tob.credit + " on " + DateTime.Now.ToString("dd/MM/yyyy") + "- Remark-Avl Bal. INR " + balance + ", For more details Visit www.naclgroup.com";
                        ViewData["ok"] = "OK";
                        transactionid = aa;
                        break;
                }
            }
            return View(sainfo);
        }
        public ActionResult PrintCreditAccount()
        {
            List<TransactionTab> trlist = new List<TransactionTab>();
            trlist = (from tr in db.TransactionTabs where tr.transactionid == transactionid select tr).ToList();
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Credit.rpt"));
            rd.SetDataSource(trlist);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);

                return File(stream, "application/pdf", transactionid + "_Credit.pdf");
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
        public ActionResult DebitAccount()
        {
            List<SavingAccountInfo> sainfo = new List<SavingAccountInfo>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(sainfo);
            }
        }
        [HttpPost]
        public ActionResult DebitAccount(TransactionTab tob, string command, string accno)
        {

            List<SavingAccountInfo> sainfo = new List<SavingAccountInfo>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {


                switch (command)
                {
                    case "Search":

                        switch (accno)
                        {
                            case "":
                                Response.Write("<script>alert('Please Enter A/C No First')</script>");
                                break;
                            default:

                                sainfo = db.SavingAccountInfos.Where(c => c.accountno == accno).ToList();
                                var cc = db.SavingAccountInfos.Single(c => c.accountno == accno);
                                if (cc.actype != "Self")
                                {
                                    ViewData["Joint"] = "Yes";
                                }
                                return View(sainfo);
                        }

                        break;

                    case "Submit":

                        var acr = db.SavingAccountInfos.Single(a => a.accountno == tob.accountno);
                        var row1 = db.Operators.Single(c => c.OperatorId == User.Identity.Name);
                        var csrt = db.CompanyInfos.Single(a => a.AdminId == row1.companyid);
                        var creditsum = (from df in db.TransactionTabs where df.accountno == tob.accountno select df.credit).DefaultIfEmpty(0).Sum();
                        var debitsum = (from df in db.TransactionTabs where df.accountno == tob.accountno select df.debit).DefaultIfEmpty(0).Sum();
                        var ac = tob.accountno.Substring(tob.accountno.Count() - 4, 4);
                        var balance = (creditsum - tob.debit - debitsum);

                        string aa = gid();
                        TransactionTab ad = new TransactionTab();
                        var j = db.TransactionTabs.Where(s => s.accountno == tob.accountno).Count();
                        ad.transactionid = aa;
                        ad.branchcode = row1.BranchCode;
                        ad.accountno = tob.accountno;
                        ad.actype = tob.actype;
                        ad.acholdername = tob.acholdername;
                        ad.paymethod = tob.paymethod;
                        ad.pdate = tob.pdate;
                        ad.checkorddno = tob.checkorddno;
                        ad.drawon = tob.drawon;
                        ad.credit = 0;
                        ad.debit = tob.debit;
                        ad.balance = balance;
                        ad.remark = tob.remark;
                        ad.opid = User.Identity.Name;
                        ad.status = (j + 1);
                        ad.type = "Debit";
                        ad.Time = DateTime.Now.ToShortTimeString();

                        db.TransactionTabs.Add(ad);
                        db.SaveChanges();
                        MyClass.Sendmsg(acr.mobileno, "Your A/c xxxxxx" + ac + " debited with INR " + tob.debit + " on " + DateTime.Now.ToString("dd/MM/yyyy") + "- Remark-Avl Bal. INR " + balance + ", For more details Visit www.naclgroup.com");
                        ViewBag.msg = " A/c " + tob.accountno + " debited with INR " + tob.debit + " on " + DateTime.Now.ToString("dd/MM/yyyy") + "- Remark-Avl Bal. INR " + balance + ", For more details Visit www.naclgroup.com";
                        ViewData["ok"] = "OK";
                        transactionid = aa;
                        break;
                }
            }
            return View(sainfo);
        }
        public ActionResult PrintDebitAccount()
        {
            List<TransactionTab> trlist = new List<TransactionTab>();
            trlist = (from tr in db.TransactionTabs where tr.transactionid == transactionid select tr).ToList();
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Debit.rpt"));
            rd.SetDataSource(trlist);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);

                return File(stream, "application/pdf", transactionid + "_Debit.pdf");
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
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(quotalist);
            }
        }
        [HttpPost]
        public ActionResult QuotaCollection(string newagentid, DateTime sdate, DateTime edate)
        {
            List<QuotaCollectionList> quotalist = new List<QuotaCollectionList>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                List<InstallmentList> inslist = new List<InstallmentList>();
                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);
                var ar = db.AgentDetails.Single(a => a.NewAgentId == newagentid);

                var tbd = (from ap in db.appltabs where ap.newintroducerid == newagentid select ap.bondid);
                var bondlist = (from yt in db.Installmenttabs where yt.paymentdate >= sdate && yt.paymentdate <= edate && yt.branch == br.BranchCode && yt.term_plan > 4 && tbd.Contains(yt.bondid) select new { yt.newbondid }).Distinct();

                foreach (var bond in bondlist)
                {
                    var planlist = (from pl in db.Installmenttabs where pl.paymentdate >= sdate && pl.paymentdate <= edate && pl.newbondid == bond.newbondid && pl.branch == br.BranchCode select pl).ToList();
                    foreach (var plan in planlist)
                    {
                        inslist.Add(new InstallmentList { newbondid = plan.newbondid, installmentno = plan.installmentno, mode = plan.mode, year = plan.year, term = plan.term_plan, planname = plan.planname, amount = plan.payamount });
                    }

                }
                var pnlist = inslist.Select(i => new { i.planname }).Distinct();
                foreach (var pn in pnlist)
                {
                    var p = db.NewPlans.Single(i => i.Planname == pn.planname);
                    Double totalcoll = 0, quotaamount = 0, per = 0;
                    totalcoll = inslist.Where(tr => tr.planname == pn.planname).Sum(r => (Double)r.amount);

                    if (p.Term >= 5 && p.Type == "RD")
                    {
                        per = 100;
                    }
                    else if (p.Term == 5 || p.Term == 6 && p.Type == "FD")
                    {
                        per = 60;
                    }
                    else if (p.Term >= 7 && p.Type == "FD")
                    {
                        per = 70;
                    }
                    quotaamount = Math.Round(((totalcoll * per) / 100), 2);
                    quotalist.Add(new QuotaCollectionList { newagentid = newagentid, name = ar.name, rankname = ar.RankName, newintroducerid = ar.NewIntroducerId, planname = pn.planname, business = totalcoll, percentage = per, quotaamount = quotaamount, sdate = sdate, edate = edate, companyname = cr.CompanyName, branchname = br.BranchName, branch = mr.branchname, bond = mr.custname, plan = mr.planname, agent = mr.agentname });
                }
                NewAgentid = newagentid;
                stdate = sdate;
                enddate = edate;
                return View(quotalist);
            }
        }
        public ActionResult PrintQuotaCollection()
        {
            List<QuotaCollectionList> quotalist = new List<QuotaCollectionList>();
            List<InstallmentList> inslist = new List<InstallmentList>();
            var mr = db.Members.Single(m => m.Id == 1);
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var pr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
            var br = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);
            var ar = db.AgentDetails.Single(a => a.NewAgentId == NewAgentid);

            var tbd = (from ap in db.appltabs where ap.newintroducerid == NewAgentid select ap.bondid);
            var bondlist = (from yt in db.Installmenttabs where yt.paymentdate >= stdate && yt.paymentdate <= enddate && yt.branch == br.BranchCode && yt.term_plan > 4 && tbd.Contains(yt.bondid) select new { yt.newbondid }).Distinct();

            foreach (var bond in bondlist)
            {
                var planlist = (from pl in db.Installmenttabs where pl.paymentdate >= stdate && pl.paymentdate <= enddate && pl.newbondid == bond.newbondid && pl.branch == br.BranchCode select pl).ToList();
                foreach (var plan in planlist)
                {
                    inslist.Add(new InstallmentList { newbondid = plan.newbondid, installmentno = plan.installmentno, mode = plan.mode, year = plan.year, term = plan.term_plan, planname = plan.planname, amount = plan.payamount });
                }

            }
            var pnlist = inslist.Select(i => new { i.planname }).Distinct();
            foreach (var pn in pnlist)
            {
                var p = db.NewPlans.Single(i => i.Planname == pn.planname);
                Double totalcoll = 0, quotaamount = 0, per = 0;
                totalcoll = inslist.Where(tr => tr.planname == pn.planname).Sum(r => (Double)r.amount);

                if (p.Term >= 5 && p.Type == "RD")
                {
                    per = 100;
                }
                else if (p.Term == 5 || p.Term == 6 && p.Type == "FD")
                {
                    per = 60;
                }
                else if (p.Term >= 7 && p.Type == "FD")
                {
                    per = 70;
                }
                quotaamount = Math.Round(((totalcoll * per) / 100), 2);
                quotalist.Add(new QuotaCollectionList { newagentid = NewAgentid, name = ar.name, rankname = ar.RankName, newintroducerid = ar.NewIntroducerId, planname = pn.planname, business = totalcoll, percentage = per, quotaamount = quotaamount, sdate = stdate, edate = enddate, companyname = cr.CompanyName, branchname = br.BranchName, branch = mr.branchname, bond = mr.custname, plan = mr.planname, agent = mr.agentname });
            }


            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "QuotaCollection.rpt"));
            rd.SetDataSource(quotalist);

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
                return RedirectToAction("Logout", "Operator");
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
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                List<TempBrokerChainCollection> bcclist = new List<TempBrokerChainCollection>();
                MonthName mn = new MonthName();
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

                        var tbd = (from ap in db.appltabs where ap.newintroducerid == bc.newagentid select ap.bondid).ToList();
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

                var tbd = (from ap in db.appltabs where ap.newintroducerid == bc.newagentid select ap.bondid).ToList();
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
        public ActionResult BrokerSelfDueCollection()
        {
            List<SelfdueCollection> bsdclist = new List<SelfdueCollection>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(bsdclist);
            }
        }
        [HttpPost]
        public ActionResult BrokerSelfDueCollection(string newagentid)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(op => op.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);
                var ar = db.AgentDetails.Single(a => a.NewAgentId == newagentid);

                DateTime date = DateTime.Now.Date;
                List<SelfdueCollection> bsdclist = new List<SelfdueCollection>();
                var tb = (from n in db.appltabs where n.newintroducerid == newagentid && n.status == 1 select n);

                foreach (var t in tb.ToList())
                {
                    var bwdilist = (from i in db.Installmenttabs where i.bondid == t.bondid && i.payamount == 0 && i.prevexpirydate < date && i.branch == br.BranchCode select i).ToList();
                    foreach (var bwdi in bwdilist)
                    {
                        bsdclist.Add(new SelfdueCollection { newagentid = newagentid, name = ar.name, newbondid = bwdi.newbondid, cname = t.name, expirydate = bwdi.prevexpirydate, planname = bwdi.planname, term = bwdi.term_plan, mode = bwdi.mode, amount = bwdi.amount, installmentno = bwdi.installmentno, companyname = cr.CompanyName, branchname = br.BranchName, branch = mr.branchname, bond = mr.custname, agent = mr.agentname, plan = mr.planname, mobileno = t.mobileno });
                    }
                }

                int count = bsdclist.Count();
                if (count == 0)
                {
                    Response.Write("<script>alert('There is No due collection')</script>");
                }
                NewAgentid = newagentid;
                return View(bsdclist);

            }
        }
        public ActionResult PrintBrokerSelfDueReport()
        {
            DateTime date = DateTime.Now.Date;
            var mr = db.Members.Single(m => m.Id == 1);
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var pr = db.Operators.Single(op => op.OperatorId == User.Identity.Name);
            var br = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);
            var ar = db.AgentDetails.Single(a => a.NewAgentId == NewAgentid);

            List<SelfdueCollection> bsdclist = new List<SelfdueCollection>();
            var tb = (from n in db.appltabs where n.newintroducerid == NewAgentid && n.status == 1 select n);

            foreach (var t in tb.ToList())
            {
                var bwdilist = (from i in db.Installmenttabs where i.bondid == t.bondid && i.payamount == 0 && i.prevexpirydate < date && i.branch == br.BranchCode select i).ToList();
                foreach (var bwdi in bwdilist)
                {
                    bsdclist.Add(new SelfdueCollection { newagentid = NewAgentid, name = ar.name, newbondid = bwdi.newbondid, cname = t.name, expirydate = bwdi.prevexpirydate, planname = bwdi.planname, term = bwdi.term_plan, mode = bwdi.mode, amount = bwdi.amount, installmentno = bwdi.installmentno, companyname = cr.CompanyName, branchname = br.BranchName, branch = mr.branchname, bond = mr.custname, agent = mr.agentname, plan = mr.planname, mobileno = t.mobileno });
                }
            }

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports/Branch"), "BrokerSelfDueCollection.rpt"));
            rd.SetDataSource(bsdclist);

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
        public ActionResult BrokerChainDueCollection()
        {
            List<ChaindueCollection> bcdclist = new List<ChaindueCollection>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(bcdclist);
            }
        }
        [HttpPost]
        public ActionResult BrokerChainDueCollection(string newagentid)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                List<ChaindueCollection> bcdclist = new List<ChaindueCollection>();
                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(op => op.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);
                var ar = db.AgentDetails.Single(a => a.NewAgentId == newagentid);

                #region Select Broker Chain Start Here

                List<BrokerChain> bclist = new List<BrokerChain>();
                bclist.Add(new BrokerChain { newagentid = ar.NewAgentId, name = ar.name, rankcode = ar.RankCode, rankname = ar.RankName, introducerid = ar.NewIntroducerId, introname = ar.IntroName, branchname = br.BranchName });
                int rcount = 0;
                List<agents> dalist = new List<agents>();
                var alist = (from al in db.AgentDetails where al.NewIntroducerId == newagentid select new { al.AgencyCode }).Distinct();
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
                    bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = br.BranchName });

                }

                #endregion
                #region Select Due Collectin Start Here
                foreach (var bc in bclist)
                {

                    DateTime date = DateTime.Now.Date;
                    var tb = (from n in db.appltabs where n.newintroducerid == bc.newagentid && n.status == 1 select n);
                    foreach (var t in tb.ToList())
                    {
                        double amount = 0;
                        List<BondDueCollection> bsdclist = new List<BondDueCollection>();
                        var bwdilist = (from i in db.Installmenttabs where i.bondid == t.bondid && i.payamount == 0 && i.prevexpirydate < date && i.branch == br.BranchCode select i).ToList();
                        foreach (var bwdi in bwdilist)
                        {
                            bsdclist.Add(new BondDueCollection { amount = bwdi.amount });
                        }


                        amount = bsdclist.Sum(tr => tr.amount);
                        if (amount > 0)
                        {
                            bcdclist.Add(new ChaindueCollection { brokercode = newagentid, brokername = ar.name, newagentid = bc.newagentid, name = bc.name, newbondid = t.newbondid, cname = t.name, expirydate = t.expirydate, planname = t.planname, term = t.term, mode = t.mode, amount = amount, companyname = cr.CompanyName, branchname = br.BranchName, branch = mr.branchname, bond = mr.custname, agent = mr.agentname, plan = mr.planname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice, mobileno = t.mobileno });
                        }
                    }

                }
                #endregion

                NewAgentid = newagentid;
                return View(bcdclist);
            }
        }
        public ActionResult PrintBrokerChainDueReport()
        {
            List<ChaindueCollection> bcdclist = new List<ChaindueCollection>();
            var mr = db.Members.Single(m => m.Id == 1);
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var pr = db.Operators.Single(op => op.OperatorId == User.Identity.Name);
            var br = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);
            var ar = db.AgentDetails.Single(a => a.NewAgentId == NewAgentid);

            #region Select Broker Chain Start Here

            List<BrokerChain> bclist = new List<BrokerChain>();
            bclist.Add(new BrokerChain { newagentid = ar.NewAgentId, name = ar.name, rankcode = ar.RankCode, rankname = ar.RankName, introducerid = ar.NewIntroducerId, introname = ar.IntroName, branchname = br.BranchName });
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
                bclist.Add(new BrokerChain { newagentid = ad.NewAgentId, name = ad.name, rankcode = ad.RankCode, rankname = ad.RankName, introducerid = ad.NewIntroducerId, introname = ad.IntroName, branchname = br.BranchName });

            }

            #endregion
            #region Select Due Collectin Start Here
            foreach (var bc in bclist)
            {

                DateTime date = DateTime.Now.Date;
                var tb = (from n in db.appltabs where n.newintroducerid == bc.newagentid && n.status == 1 select n);
                foreach (var t in tb.ToList())
                {
                    double amount = 0;
                    List<BondDueCollection> bsdclist = new List<BondDueCollection>();
                    var bwdilist = (from i in db.Installmenttabs where i.bondid == t.bondid && i.payamount == 0 && i.prevexpirydate < date && i.branch == br.BranchCode select i).ToList();
                    foreach (var bwdi in bwdilist)
                    {
                        bsdclist.Add(new BondDueCollection { amount = bwdi.amount });
                    }


                    amount = bsdclist.Sum(tr => tr.amount);
                    if (amount > 0)
                    {
                        bcdclist.Add(new ChaindueCollection { brokercode = NewAgentid, brokername = ar.name, newagentid = bc.newagentid, name = bc.name, newbondid = t.newbondid, cname = t.name, expirydate = t.expirydate, planname = t.planname, term = t.term, mode = t.mode, amount = amount, companyname = cr.CompanyName, branchname = br.BranchName, branch = mr.branchname, bond = mr.custname, agent = mr.agentname, plan = mr.planname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice, mobileno = t.mobileno });
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
        public ActionResult AllSavingaccount()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var dn = db.SavingAccountInfos.Where(a => a.opid == User.Identity.Name).ToList();
                return View(dn);
            }

        }
        public ActionResult Printallsavingaccount()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var dn = db.SavingAccountInfos.Where(a => a.opid == User.Identity.Name).ToList();
                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "accountlist.rpt"));
                rd.SetDataSource(dn);

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
                return View(dn);

            }

        }

        [HttpGet]
        public ActionResult Datewisesaving()
        {
            List<SavingAccountInfo> savinglist = new List<SavingAccountInfo>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                return View(savinglist);
            }
        }
        [HttpPost]
        public ActionResult Datewisesaving(DateTime sdate, DateTime edate)
        {
            List<SavingAccountInfo> savinglist = new List<SavingAccountInfo>();

            //var bb = db.Branchtabs.Single(o => o.BranchCode == User.Identity.Name);
            savinglist = (from o in db.SavingAccountInfos where o.Doj <= edate && o.Doj >= sdate && o.opid == User.Identity.Name orderby o.accountno select o).ToList();
            edate1 = edate;
            sdate1 = sdate;
            return View(savinglist);

        }
        public ActionResult PrintDatewisesaving()
        {
            List<SavingAccountInfo> savinglist = new List<SavingAccountInfo>();

            //var bb = db.Branchtabs.Single(o => o.BranchCode == User.Identity.Name);
            savinglist = (from o in db.SavingAccountInfos where o.Doj <= edate1 && o.Doj >= sdate1 && o.opid == User.Identity.Name orderby o.accountno select o).ToList();
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "datewisesaving.rpt"));
            rd.SetDataSource(savinglist);

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
        public ActionResult AccStatementReport()
        {
            List<AcStatementReport> abrlist = new List<AcStatementReport>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(abrlist);
            }
        }
        [HttpPost]
        public ActionResult AccStatementReport(string accountid, DateTime sdate, DateTime edate)
        {
            List<AcStatementReport> abrlist = new List<AcStatementReport>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var opr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
                var alist = (from pl in db.TransactionTabs where pl.accountno == accountid && pl.pdate >= sdate && pl.pdate <= edate select pl).ToList();
                foreach (var a in alist)
                {
                    abrlist.Add(new AcStatementReport { actype = a.actype, accountno = a.accountno, acholdername = a.acholdername, transactionid = a.transactionid, remarks = a.remark, credit = a.credit, debit = a.debit, balance = a.balance, branchname = br.BranchName, companyname = cr.CompanyName, sdate = sdate, edate = edate, pdate = a.pdate, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });

                }

                newaccountid = accountid;
                stdate = sdate;
                enddate = edate;
                return View(abrlist);
            }
        }
        public ActionResult PrintAccStatementReport()
        {
            List<AcStatementReport> abrlist = new List<AcStatementReport>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var opr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
                var alist = (from pl in db.TransactionTabs where pl.accountno == newaccountid && pl.pdate >= stdate && pl.pdate <= enddate select pl).ToList();
                foreach (var a in alist)
                {
                    abrlist.Add(new AcStatementReport { actype = a.actype, accountno = a.accountno, acholdername = a.acholdername, transactionid = a.transactionid, remarks = a.remark, credit = a.credit, debit = a.debit, balance = a.balance, branchname = br.BranchName, companyname = cr.CompanyName, sdate = stdate, edate = enddate, pdate = a.pdate, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });

                }

                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "AccStatementReport.rpt"));
                rd.SetDataSource(abrlist);

                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();


                try
                {
                    Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    stream.Seek(0, SeekOrigin.Begin);
                    string dt = pdate.ToShortDateString();

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
        }

        #endregion

        #region Official Reports Start Here

        [HttpGet]
        public ActionResult DateWiseMember()
        {
            List<DWMemberList> memberlist = new List<DWMemberList>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                return View(memberlist);
            }
        }
        [HttpPost]
        public ActionResult DateWiseMember(DateTime sdate, DateTime edate)
        {
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            List<DWMemberList> memberlist = new List<DWMemberList>();
            var opr = db.Operators.Single(oo => oo.OperatorId == User.Identity.Name);
            var bb = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
            var mlist = (from o in db.Member_tabs where o.Cdate <= edate && o.Cdate >= sdate && o.BranchCode == opr.BranchCode orderby o.NewMemberId select o).ToList();
            foreach (var m in mlist)
            {
                memberlist.Add(new DWMemberList { newmemberid = m.NewMemberId, name = m.MemberName, dob = m.DOB, fathername = m.Father, maddress = m.Address, Mobileno = m.Mobile, branchname = bb.BranchName, formdate = m.Cdate, sdate = sdate, edate = edate, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
            }

            enddate = edate;
            stdate = sdate;
            return View(memberlist);

        }
        public ActionResult PrintDateWiseMember()
        {
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            List<DWMemberList> memberlist = new List<DWMemberList>();
            var opr = db.Operators.Single(oo => oo.OperatorId == User.Identity.Name);
            var bb = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
            var mlist = (from o in db.Member_tabs where o.Cdate <= enddate && o.Cdate >= stdate && o.BranchCode == opr.BranchCode orderby o.NewMemberId select o).ToList();
            foreach (var m in mlist)
            {
                memberlist.Add(new DWMemberList { newmemberid = m.NewMemberId, name = m.MemberName, dob = m.DOB, fathername = m.Father, maddress = m.Address, Mobileno = m.Mobile, branchname = bb.BranchName, formdate = m.Cdate, sdate = stdate, edate = enddate, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
            }

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "datewisemember.rpt"));
            rd.SetDataSource(memberlist);

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
        public ActionResult Customer(string newbondid, int n = 0)
        {
            List<DatewiseCustomerList> custlist = new List<DatewiseCustomerList>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                if (newbondid == "" || newbondid == null)
                {
                    return View(custlist);
                }
                else
                {
                    var dn = (from a in db.appltabs where a.newbondid == newbondid select a).ToList();
                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Customer.rpt"));
                    rd.SetDataSource(dn);

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

                    return View(custlist);
                }


            }
        }
        [HttpPost]
        public ActionResult Customer(int option)
        {
            List<DatewiseCustomerList> custlist = new List<DatewiseCustomerList>();
            List<appltab> cus = new List<appltab>();
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var mr = db.Members.Single(c => c.Id == 1);
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var pr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                var bb = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);

                if (option > 0)
                {
                    cus = (from s in db.appltabs where s.branchcode == bb.BranchCode && s.projectid == option && s.status == 1 orderby s.newbondid select s).ToList();
                }
                else
                {
                    cus = (from s in db.appltabs where s.branchcode == bb.BranchCode && s.status == 1 orderby s.newbondid select s).ToList();
                }
                foreach (var cust in cus)
                {
                    custlist.Add(new DatewiseCustomerList { newbondid = cust.newbondid, name = cust.name, newintroducerid = cust.newintroducerid, planname = cust.planname, mode = cust.mode, payment = cust.payment, bookingamount = cust.bookingamount, downpayment = cust.downpayment, plotsize = cust.nolandunit, Mobile = cust.mobileno, formdate = cust.formdate, branchname = User.Identity.Name, companyname = cr.CompanyName, branch = mr.branchname, bond = mr.custname, plan = mr.planname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                }

                PId = option;
                return View(custlist);

            }
        }
        public ActionResult PrintCustomer()
        {
            List<DatewiseCustomerList> custlist = new List<DatewiseCustomerList>();
            List<appltab> cus = new List<appltab>();
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var mr = db.Members.Single(c => c.Id == 1);
            var pr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
            var bb = db.Branchtabs.Single(o => o.BranchCode == pr.BranchCode);

            if (PId > 0)
            {
                cus = (from s in db.appltabs where s.branchcode == bb.BranchCode && s.projectid == PId && s.status == 1 orderby s.newbondid select s).ToList();
            }
            else
            {
                cus = (from s in db.appltabs where s.branchcode == bb.BranchCode && s.status == 1 orderby s.newbondid select s).ToList();
            }

            foreach (var cust in cus)
            {
                custlist.Add(new DatewiseCustomerList { newbondid = cust.newbondid, name = cust.name, newintroducerid = cust.newintroducerid, planname = cust.planname, mode = cust.mode, payment = cust.payment, bookingamount = cust.bookingamount, downpayment = cust.downpayment, plotsize = cust.nolandunit, Mobile = cust.mobileno, formdate = cust.formdate, branchname = bb.BranchCode, companyname = cr.CompanyName, branch = mr.branchname, bond = mr.custname, plan = mr.planname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
            }

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "CustomerList.rpt"));
            rd.SetDataSource(custlist);

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
        public ActionResult DatewiseCustomer()
        {
            List<DatewiseCustomerList> dwclist = new List<DatewiseCustomerList>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                return View(dwclist);
            }
        }
        [HttpPost]
        public ActionResult DatewiseCustomer(DateTime sdate, DateTime edate)
        {
            List<DatewiseCustomerList> clist = new List<DatewiseCustomerList>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);
                var tlist = (from cu in db.appltabs where cu.formdate >= sdate && cu.formdate <= edate && cu.branchcode == br.BranchCode && cu.status == 1 select cu).ToList();

                foreach (var i in tlist)
                {
                    clist.Add(new DatewiseCustomerList { newbondid = i.newbondid, name = i.name, Mobile = i.mobileno, formdate = i.formdate, newintroducerid = i.newintroducerid, planname = i.planname, mode = i.mode, payment = i.payment, bookingamount = i.bookingamount, downpayment = i.downpayment, plotsize = i.nolandunit, branch = mr.branchname, companyname = cr.CompanyName, branchname = br.BranchName, plan = mr.planname, bond = mr.custname, sdate = sdate, edate = edate });
                }

                stdate = sdate;
                enddate = edate;

                return View(clist);
            }
        }
        public ActionResult PrintDatewiseCustomer()
        {
            List<DatewiseCustomerList> clist = new List<DatewiseCustomerList>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);

                var tlist = (from cu in db.appltabs where cu.formdate >= stdate && cu.formdate <= enddate && cu.branchcode == br.BranchCode && cu.status == 1 select cu).ToList();
                foreach (var i in tlist)
                {
                    clist.Add(new DatewiseCustomerList { newbondid = i.newbondid, name = i.name, Mobile = i.mobileno, formdate = i.formdate, newintroducerid = i.newintroducerid, planname = i.planname, mode = i.mode, payment = i.payment, bookingamount = i.bookingamount, downpayment = i.downpayment, plotsize = i.nolandunit, branch = mr.branchname, companyname = cr.CompanyName, branchname = br.BranchName, plan = mr.planname, bond = mr.custname, sdate = stdate, edate = enddate });
                }

                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports/Branch"), "datewisecustomer.rpt"));
                rd.SetDataSource(clist);

                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();


                try
                {
                    Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    stream.Seek(0, SeekOrigin.Begin);
                    string dt = pdate.ToShortDateString();

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


                return View(clist);
            }
        }

        [HttpGet]
        public ActionResult MemberWiseAccountDetail()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Branch");
            }
            else
            {

                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);

                List<DWMemberList> memberlist = new List<DWMemberList>();
                var mlist = (from o in db.Member_tabs where o.BranchCode == br.BranchCode orderby o.NewMemberId select o).ToList();
                foreach (var m in mlist)
                {
                    memberlist.Add(new DWMemberList { newmemberid = m.NewMemberId, name = m.MemberName, dob = m.DOB, fathername = m.Father, maddress = m.Address, Mobileno = m.Mobile, branchname = br.BranchName, formdate = m.Cdate, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                }
                return View(memberlist);
            }


        }
        public ActionResult PrintMemberList()
        {

            var mr = db.Members.Single(m => m.Id == 1);
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var pr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
            var br = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);
            List<DWMemberList> memberlist = new List<DWMemberList>();
            var mlist = (from o in db.Member_tabs where o.BranchCode == br.BranchCode orderby o.NewMemberId select o).ToList();
            foreach (var m in mlist)
            {
                memberlist.Add(new DWMemberList { newmemberid = m.NewMemberId, name = m.MemberName, dob = m.DOB, fathername = m.Father, maddress = m.Address, Mobileno = m.Mobile, branchname = br.BranchName, formdate = m.Cdate, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
            }
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "AllMemberList.rpt"));
            rd.SetDataSource(memberlist);

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
        public ActionResult MemberAccountDetail(string newmemberid)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Branch");
            }
            else
            {
                List<BrokerAccount> balist = new List<BrokerAccount>();
                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);
                var ar = db.Member_tabs.Single(n => n.NewMemberId == newmemberid);
                var alist = (from a in db.appltabs where a.newmemberid == newmemberid && a.status == 1 orderby a.newbondid select a).ToList();
                foreach (var b in alist)
                {
                    var amountsum = (from ii in db.Installmenttabs where ii.newbondid == b.newbondid select ii.payamount).DefaultIfEmpty(0).Sum();
                    balist.Add(new BrokerAccount { newintroducerid = newmemberid, name = ar.MemberName, newbondid = b.newbondid, cname = b.name, amount = amountsum, planname = b.planname, term = b.term, mode = b.mode, expirydate = b.expirydate, formdate = b.formdate, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                }
                NewMemberid = newmemberid;
                return View(balist);
            }


        }
        public ActionResult PrintAccountDetail()
        {

            List<BrokerAccount> balist = new List<BrokerAccount>();
            var mr = db.Members.Single(m => m.Id == 1);
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var pr = db.Operators.Single(b => b.OperatorId == User.Identity.Name);
            var br = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);
            var ar = db.Member_tabs.Single(n => n.NewMemberId == NewMemberid);
            var alist = (from a in db.appltabs where a.newmemberid == NewMemberid && a.status == 1 orderby a.newbondid select a).ToList();
            foreach (var b in alist)
            {
                var amountsum = (from ii in db.Installmenttabs where ii.newbondid == b.newbondid select ii.payamount).DefaultIfEmpty(0).Sum();
                balist.Add(new BrokerAccount { newintroducerid = NewMemberid, name = ar.MemberName, newbondid = b.newbondid, cname = b.name, amount = amountsum, planname = b.planname, term = b.term, mode = b.mode, expirydate = b.expirydate, formdate = b.formdate, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
            }
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "MemberAccountList.rpt"));
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
        public ActionResult RdPlanList()
        {
            List<Plan_Tab> custreg = new List<Plan_Tab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                custreg = (from aaa in db.Plan_Tabs select aaa).ToList();
                return View(custreg);

            }
        }
        public ActionResult PrintRdPlanReport()
        {

            List<Plan_Tab> bond = new List<Plan_Tab>();
            bond = db.Plan_Tabs.ToList();
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "rdplan.rpt"));
            rd.SetDataSource(bond);

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
        public ActionResult FDPlanList()
        {
            List<Fixed_Tab> fd = new List<Fixed_Tab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                fd = (from aaa in db.Fixed_Tabs select aaa).ToList();
                return View(fd);

            }
        }
        public ActionResult PrintFdPlanReport()
        {

            List<Fixed_Tab> bond = new List<Fixed_Tab>();
            bond = db.Fixed_Tabs.ToList();
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Fdplan.rpt"));
            rd.SetDataSource(bond);

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
        public ActionResult NFPlanList()
        {
            List<MIPP_tab> fd = new List<MIPP_tab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                List<NfpList> rlist = new List<NfpList>();
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var plist = (from aaa in db.MIPP_tabs select aaa).ToList();
                foreach (var p in plist)
                {
                    rlist.Add(new NfpList { Planname = p.planname, Term = p.term, Amount = p.amount, EstimatedValue = p.FiveERV, Yearlyamount = p.Yearly, companyname = cr.CompanyName, branchname = User.Identity.Name, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                }
                return View(rlist);

            }
        }
        public ActionResult PrintNFPlanReport()
        {

            List<NfpList> rlist = new List<NfpList>();
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var plist = (from aaa in db.MIPP_tabs select aaa).ToList();
            foreach (var p in plist)
            {
                rlist.Add(new NfpList { Planname = p.planname, Term = p.term, Amount = p.amount, EstimatedValue = p.FiveERV, Yearlyamount = p.Yearly, companyname = cr.CompanyName, branchname = User.Identity.Name, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
            }
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "NFPlan.rpt"));
            rd.SetDataSource(rlist);

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
        public ActionResult PPlanList()
        {
            List<Pension_Tab> fd = new List<Pension_Tab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                fd = (from aaa in db.Pension_Tabs select aaa).ToList();
                return View(fd);

            }
        }
        public ActionResult PrintPPlanReport()
        {

            List<Pension_Tab> fd = new List<Pension_Tab>();
            fd = (from aaa in db.Pension_Tabs select aaa).ToList();
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "PPlan.rpt"));
            rd.SetDataSource(fd);

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
        public ActionResult RankList()
        {
            List<Ranktab> custreg = new List<Ranktab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                custreg = (from aaa in db.Ranktabs where aaa.RankCode < 15 select aaa).ToList();
                return View(custreg);

            }
        }
        public ActionResult PrintRank()
        {
            List<Ranktab> bond = new List<Ranktab>();
            bond = db.Ranktabs.Where(s => s.RankCode < 15).ToList();
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Rank.rpt"));
            rd.SetDataSource(bond);

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
        public ActionResult CustumerInstallmentReport()
        {
            List<CustomerInstallment> ins = new List<CustomerInstallment>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(ins);
            }
        }
        [HttpPost]
        public ActionResult CustumerInstallmentReport(string newbondid)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Admin");
            }
            else
            {
                int days = 0;
                List<CustomerInstallment> ins = new List<CustomerInstallment>();
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(p => p.OperatorId == User.Identity.Name);
                var bcount = (from vb in db.Installmenttabs where vb.newbondid == newbondid select vb).Count();

                if (bcount > 0)
                {
                    var inst = (from vb in db.Installmenttabs where vb.newbondid == newbondid select vb).ToList();
                    foreach (var item in inst.ToList())
                    {
                        if (item.payamount != 0)
                        {
                            if (item.paymentdate > item.prevexpirydate)
                            {
                                DateTime dt1 = Convert.ToDateTime(item.paymentdate);
                                DateTime dt2 = item.prevexpirydate;
                                TimeSpan ts = dt1.Subtract(dt2);
                                days = ts.Days;
                            }
                            else
                            {
                                days = 0;
                            }
                        }
                        else
                        {
                            days = 0;
                        }

                        var bb = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);
                        var brow = db.appltabs.Single(po => po.newbondid == item.newbondid);
                        var bro = db.AgentDetails.Single(bo => bo.NewAgentId == brow.newintroducerid);
                        ins.Add(new CustomerInstallment
                        {
                            downpayment = brow.downpayment,
                            latedays = days,
                            amount = item.amount,
                            payamount = item.payamount,
                            installmentno = item.installmentno,
                            year = item.year,
                            paymentdate = Convert.ToDateTime(item.paymentdate),
                            cssno = Convert.ToInt32(item.cssno),
                            receiptno = item.receiptno,
                            reliefrs = item.reliefrs,
                            newbondid = brow.newbondid,
                            cust_name = brow.name,
                            brokercode = brow.newintroducerid,
                            name = bro.name,
                            planname = brow.planname,
                            term_plan = brow.term,
                            mode = brow.mode,
                            branch = brow.branchcode,
                            mobile = brow.mobileno,
                            latefine = item.latefine,
                            age = brow.age,
                            address = brow.addr,
                            nomname = brow.nomineename,
                            nomage = brow.nomage,
                            nomrel = brow.nomrel,
                            bookingamount = brow.bookingamount,
                            totalcon = brow.totalcon,
                            expectedvalue = brow.expectedraisablevalue,
                            expirydate = item.prevexpirydate,
                            enddate = brow.expirydate,
                            Company = cr.CompanyName,
                            Branch_name = bb.BranchName,
                            caddress = cr.Address,
                            emailid = cr.Emailid,
                            contact = cr.Contact,
                            domainname = cr.HeadOffice,
                            branchname = bb.BranchName,

                            paymethod = item.paymethod,
                            bank = item.bank,
                            Account = item.Account,
                            chequeno = item.chequeno,
                            ACholdername = item.ACholdername,
                            Bbranch = item.Bbranch,
                            IFSCCode = item.IFSCCode,
                            ChequeAmount = item.ChequeAmount,
                            Chequedate = item.Chequedate.ToString(),
                            penality = item.Penality,
                            transactiontype = item.transactiontype
                        });

                        NewBondid = newbondid;

                    }
                }

                else
                {
                    Response.Write("<script>alert('Record Not Found for this Id')</script>");

                }
                return View(ins);
            }
        }
        public ActionResult PrintCustumerInstallment()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Admin");
            }
            else
            {
                int days = 0;
                string date1 = "01/01/0001";
                string date;
                //DateTime date = DateTime.Now.Date;
                List<CustomerInstallment> ins = new List<CustomerInstallment>();
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(p => p.OperatorId == User.Identity.Name);
                var inst = (from vb in db.Installmenttabs where vb.newbondid == NewBondid select vb).ToList();
                var paid_amt = inst.Select(d => d.payamount).DefaultIfEmpty(0).Sum();
                var brow = db.appltabs.Single(po => po.newbondid == NewBondid);
                var p1 = db.Plans.FirstOrDefault(d => d.Plancode == brow.projectid);
                string _property_type = p1 == null ? "Residential" : p1.type.Replace("Commercial", "Residential Cum Commercial");
                foreach (var item in inst.ToList())
                {
                    if (item.payamount == 0)
                    {
                        days = 0;
                        date = date1;
                    }
                    else
                    {
                        if (item.paymentdate > item.prevexpirydate)
                        {
                            DateTime dt1 = Convert.ToDateTime(item.paymentdate);
                            DateTime dt2 = item.prevexpirydate;
                            TimeSpan ts = dt1.Subtract(dt2);
                            days = ts.Days;
                            date = Convert.ToDateTime(item.paymentdate).ToString("dd/MM/yyyy");
                        }
                        else
                        {
                            days = 0;
                            date = Convert.ToDateTime(item.paymentdate).ToString("dd/MM/yyyy");

                        }

                    }
                    //var brow = db.appltabs.Single(po => po.newbondid == item.newbondid);
                    var bro = db.AgentDetails.Single(bo => bo.NewAgentId == brow.newintroducerid);
                    var bb = db.Branchtabs.Single(b => b.BranchCode == item.branch);
                    string Paymethod = "";
                    if (item.paymethod == "banktransaction")
                    {
                        Paymethod = item.transactiontype;
                    }
                    else
                    {
                        Paymethod = item.paymethod;
                    }
                    ins.Add(new CustomerInstallment
                    {
                        downpayment = brow.downpayment,
                        latedays = days,
                        latefine = item.latefine,
                        amount = item.amount,
                        payamount = item.payamount,
                        installmentno = item.installmentno,
                        year = item.year,
                        paydate = date,
                        cssno = Convert.ToInt32(item.cssno),
                        receiptno = item.receiptno,
                        reliefrs = item.reliefrs,
                        newbondid = brow.newbondid,
                        cust_name = brow.name,
                        brokercode = brow.newintroducerid,
                        name = bro.name,
                        planname = _property_type,
                        term_plan = brow.term,
                        mode = brow.mode,
                        branch = brow.branchcode,
                        age = brow.age,
                        address = brow.addr,
                        nomname = brow.nomineename,
                        nomage = brow.nomage,
                        nomrel = brow.nomrel,
                        bookingamount = brow.bookingamount,
                        totalcon = brow.totalcon,
                        expectedvalue = brow.totalcon - paid_amt,
                        expirydate = item.prevexpirydate,
                        enddate = brow.expirydate,
                        Company = cr.CompanyName,
                        Branch_name = bb.BranchName,
                        caddress = cr.Address,
                        emailid = cr.Emailid,
                        contact = cr.Contact,
                        domainname = cr.HeadOffice,
                        mobile = brow.mobileno,
                        branchname = bb.BranchName,

                        paymethod = Paymethod,
                        bank = item.bank,
                        Account = item.Account,
                        chequeno = item.chequeno,
                        ACholdername = item.ACholdername,
                        Bbranch = item.Bbranch,
                        IFSCCode = item.IFSCCode,
                        ChequeAmount = item.ChequeAmount,
                        Chequedate = item.Chequedate.ToString(),
                        penality = item.Penality,
                        transactiontype = item.transactiontype,
                        plotno = brow.plotno == 0 ? "NA" : brow.plotno.ToString(),
                        block = brow.block,
                        phase = brow.phase,
                        area = brow.nolandunit
                    });

                }

                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "CustomerInstallment.rpt"));
                rd.SetDataSource(ins);

                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();


                try
                {
                    Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    stream.Seek(0, SeekOrigin.Begin);
                    string dt = pdate.ToShortDateString();

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

                return View(ins);
            }
        }

        [HttpGet]
        public ActionResult DailyExpense()
        {
            List<Expense> ins = new List<Expense>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(ins);
            }
        }
        [HttpPost]
        public ActionResult DailyExpense(DateTime sdate, DateTime edate)
        {
            List<Expense> ins = new List<Expense>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var or = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                ins = (from dcl in db.Expenses where dcl.date_time >= sdate && dcl.date_time <= edate && dcl.branchcode == or.BranchCode select dcl).ToList();
                ab = sdate;
                b = edate;
                return View(ins);
            }
        }
        public ActionResult PrintExpanse()
        {
            var or = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
            var br = db.Branchtabs.Single(b => b.BranchCode == or.BranchCode);
            List<ExpenseList> elist = new List<ExpenseList>();
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var ins = (from dcl in db.Expenses where dcl.date_time >= ab && dcl.date_time <= b && dcl.branchcode == br.BranchCode select dcl).ToList();
            foreach (var i in ins)
            {
                elist.Add(new ExpenseList { head = i.head, Remark = i.Remark, amount = i.amount, date_time = i.date_time, sdate = ab, edate = b, branchcode = i.branchcode, opid = i.opid, branchname = br.BranchName, companyname = cr.CompanyName, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice, paymethod = i.paymethod, trans_id = i.chequeno });
            }
            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "DailyExpanse.rpt"));
            rd.SetDataSource(elist);

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
        public ActionResult CompleteDailyCollection()
        {
            List<MergeDailyCollection> mdc = new List<MergeDailyCollection>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                int count = (from n in db.NewLogins where n.UserName == User.Identity.Name select n.UserName).Count();
                if (count == 1)
                {
                    var log = db.NewLogins.Single(a => a.UserName == User.Identity.Name);
                    if (log.status == 1 && log.type == "Operator")
                    {

                        return View(mdc);
                    }
                    else
                    {
                        return RedirectToAction("Logout", "Operator");
                    }
                }
                else
                {
                    return RedirectToAction("Logout", "Operator");
                }
            }

        }
        [HttpPost]
        public ActionResult CompleteDailyCollection(DateTime sdate, DateTime edate, string type)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                List<MergeDailyCollection> mdc = new List<MergeDailyCollection>();
                List<Installment> inl = new List<Installment>();
                int sn = 1;
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(p => p.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);
                var tbd = (from ap in db.appltabs where ap.status == 1 select ap.bondid);

                Double formsum = 0, appsum = 0, totalexp = 0, memberfees = 0, acformfees = 0, totalvoucher = 0;
                var pst = db.Plans.Single(c => c.Plancode == 4);
                var dipositsaving = (from j in db.TransactionTabs where j.pdate >= sdate && j.pdate <= edate select j.credit).DefaultIfEmpty(0).Sum();
                var widthdrawsaving = (from j in db.TransactionTabs where j.pdate >= sdate && j.pdate <= edate select j.debit).DefaultIfEmpty(0).Sum();
                totalvoucher = (from ag in db.Voucher_Reports where ag.date >= sdate && ag.date <= edate select ag.netamount).DefaultIfEmpty(0).Sum();
                totalexp = (from ag in db.Expenses where ag.date_time >= sdate && ag.date_time <= edate select ag.amount).DefaultIfEmpty(0).Sum();
                appsum = (from b in db.appltabs where b.formdate >= sdate && b.formdate <= edate && b.status == 1 && b.paymethod == type select b.applicationcharge).DefaultIfEmpty(0).Sum();
                formsum = (from ag in db.AgentDetails where ag.Doj >= sdate && ag.Doj <= edate select ag.formfee).DefaultIfEmpty(0).Sum();
                memberfees = (from ag in db.Member_tabs where ag.Cdate >= sdate && ag.Cdate <= edate select ag.Fee).DefaultIfEmpty(0).Sum();
                acformfees = (from ag in db.SavingAccountInfos where ag.Doj >= sdate && ag.Doj <= edate select ag.formfee).DefaultIfEmpty(0).Sum();

                var bd = (from p in db.Installmenttabs where p.paymethod == type && p.paymentdate >= sdate && p.paymentdate <= edate && p.paymentno != 0 && tbd.Contains(p.bondid) select new { p.bondid }).Distinct();

                if (bd.Count() > 0)
                {
                    foreach (var t in bd.ToList())
                    {
                        var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                        var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                        var irow = (from im in db.Installmenttabs where im.paymethod == type && im.bondid == t.bondid && im.paymentdate >= sdate && im.paymentdate <= edate && im.payamount != 0 && im.type != "Adjustment" select im).ToList();
                        foreach (var cl in irow.ToList())
                        {
                            var finalamount = 0.0;
                            if (app.plantype == pst.shortcutname)
                            {
                                finalamount = app.totalcon;
                            }
                            else
                            {
                                finalamount = cl.payamount;
                            }
                            inl.Add(new Installment
                            {
                                paymentdate = Convert.ToDateTime(cl.paymentdate),
                                rcptno = cl.receiptno,
                                amount = finalamount,
                                latefee = cl.latefine,
                                relief = cl.reliefrs,
                                cssno = Convert.ToInt32(cl.cssno),
                                year = cl.year,
                                installno = cl.installmentno,
                                Account_Code = app.newbondid,
                                Name = app.name,
                                Introducer_Code = agt.NewAgentId,
                                Rank = agt.RankName,
                                Planname = app.planname,
                                Plantype = app.plantype,
                                Term = app.term,

                                paymethod = cl.paymethod,
                                bank = cl.bank,
                                Bbranch = cl.Bbranch,
                                Account = cl.Account,
                                ACholdername = cl.ACholdername,
                                IFSCCode = cl.IFSCCode,
                                ChequeAmount = cl.ChequeAmount,
                                chequeno = cl.chequeno,
                                Chequedate = (cl.Chequedate).ToString(),
                                transactiontype = cl.transactiontype
                            });
                        }

                    }

                    foreach (var i in inl.ToList())
                    {
                        string renew;
                        if (i.installno == 1)
                        {
                            renew = "New";
                        }
                        else
                        {
                            renew = "Renew";
                        }
                        if (renew == "Renew" && i.Plantype == pst.shortcutname)
                        {
                            ///-----------PPS Plan Amount Not Add 
                        }
                        else
                        {
                            mdc.Add(new MergeDailyCollection
                            {
                                trno = "tr" + sn,
                                no = "D" + sn,
                                cssno = i.cssno,
                                date = i.paymentdate,
                                newbondid = i.Account_Code,
                                name = i.Name,
                                rankname = i.Rank,
                                newintroducerid = i.Introducer_Code,
                                amount = i.amount,
                                latefine = i.latefee,
                                rebate = i.relief,
                                receiptno = i.rcptno,
                                planname = i.Planname,
                                term = i.Term,
                                newrenew = renew,
                                appcharge = appsum,
                                formfee = formsum,
                                memberfee = memberfees,
                                acformfee = acformfees,
                                totalexp = totalexp,
                                voucher = totalvoucher,
                                dipositsaving = dipositsaving,
                                widthdrawsaving = widthdrawsaving,
                                branchname = br.BranchName,
                                companyname = cr.CompanyName,
                                address = cr.Address,
                                emailid = cr.Emailid,
                                contact = cr.Contact,
                                domainname = cr.HeadOffice,
                                sdate = sdate,
                                edate = edate,

                                paymethod = i.paymethod,
                                bank = i.bank,
                                Bbranch = i.Bbranch,
                                Account = i.Account,
                                ACholdername = i.ACholdername,
                                IFSCCode = i.IFSCCode,
                                ChequeAmount = i.ChequeAmount,
                                chequeno = i.chequeno,
                                Chequedate = i.Chequedate,
                                transactiontype = i.transactiontype
                            });
                        }
                        sn = sn + 1;
                    }

                    //------Viewdata for display in page------------------------------------------

                    stdate = sdate;
                    enddate = edate;
                    typ = type;
                }
                else
                {
                    Response.Write("<script>alert('There is no collection in this date')</script>");
                }
                return View(mdc);
            }
        }
        public ActionResult PrintCompleteDailyCollection()
        {
            List<MergeDailyCollection> mdc = new List<MergeDailyCollection>();
            List<Installment> inl = new List<Installment>();
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var pr = db.Operators.Single(p => p.OperatorId == User.Identity.Name);
            var br = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);
            var tbd = (from ap in db.appltabs where ap.status == 1 select ap.bondid);
            var sn = 1;
            Double formsum = 0, appsum = 0, totalexp = 0, memberfees = 0, acformfees = 0, totalvoucher = 0;
            var pst = db.Plans.Single(c => c.Plancode == 4);
            var dipositsaving = (from j in db.TransactionTabs where j.pdate >= stdate && j.pdate <= enddate select j.credit).DefaultIfEmpty(0).Sum();
            var widthdrawsaving = (from j in db.TransactionTabs where j.pdate >= stdate && j.pdate <= enddate select j.debit).DefaultIfEmpty(0).Sum();
            totalvoucher = (from ag in db.Voucher_Reports where ag.date >= stdate && ag.date <= enddate select ag.netamount).DefaultIfEmpty(0).Sum();
            totalexp = (from ag in db.Expenses where ag.date_time >= stdate && ag.date_time <= enddate select ag.amount).DefaultIfEmpty(0).Sum();
            appsum = (from b in db.appltabs where b.formdate >= stdate && b.formdate <= enddate && b.status == 1 && b.paymethod == typ select b.applicationcharge).DefaultIfEmpty(0).Sum();
            formsum = (from ag in db.AgentDetails where ag.Doj >= stdate && ag.Doj <= enddate select ag.formfee).DefaultIfEmpty(0).Sum();
            memberfees = (from ag in db.Member_tabs where ag.Cdate >= stdate && ag.Cdate <= enddate select ag.Fee).DefaultIfEmpty(0).Sum();
            acformfees = (from ag in db.SavingAccountInfos where ag.Doj >= stdate && ag.Doj <= enddate select ag.formfee).DefaultIfEmpty(0).Sum();

            var bd = (from p in db.Installmenttabs where p.paymethod == typ && p.paymentdate >= stdate && p.paymentdate <= enddate && p.paymentno != 0 && tbd.Contains(p.bondid) select new { p.bondid }).Distinct();

            if (bd.Count() > 0)
            {
                foreach (var t in bd.ToList())
                {
                    var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                    var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                    var irow = (from im in db.Installmenttabs where im.paymethod == typ && im.bondid == t.bondid && im.paymentdate >= stdate && im.paymentdate <= enddate && im.payamount != 0 && im.type != "Adjustment" select im).ToList();
                    foreach (var cl in irow.ToList())
                    {
                        var finalamount = 0.0;
                        if (app.plantype == pst.shortcutname)
                        {
                            finalamount = app.totalcon;
                        }
                        else
                        {
                            finalamount = cl.payamount;
                        }
                        inl.Add(new Installment
                        {
                            paymentdate = Convert.ToDateTime(cl.paymentdate),
                            rcptno = cl.receiptno,
                            amount = finalamount,
                            latefee = cl.latefine,
                            relief = cl.reliefrs,
                            cssno = Convert.ToInt32(cl.cssno),
                            year = cl.year,
                            installno = cl.installmentno,
                            Account_Code = app.newbondid,
                            Name = app.name,
                            Introducer_Code = agt.NewAgentId,
                            Rank = agt.RankName,
                            Planname = app.planname,
                            Plantype = app.plantype,
                            Term = app.term,

                            paymethod = cl.paymethod,
                            bank = cl.bank,
                            Bbranch = cl.Bbranch,
                            Account = cl.Account,
                            ACholdername = cl.ACholdername,
                            IFSCCode = cl.IFSCCode,
                            ChequeAmount = cl.ChequeAmount,
                            chequeno = cl.chequeno,
                            Chequedate = (cl.Chequedate).ToString(),
                            transactiontype = cl.transactiontype
                        });
                    }

                }

                foreach (var i in inl.ToList())
                {
                    string renew;
                    if (i.installno == 1)
                    {
                        renew = "New";
                    }
                    else
                    {
                        renew = "Renew";
                    }
                    if (renew == "Renew" && i.Plantype == pst.shortcutname)
                    {
                        ///-----------PPS Plan Amount Not Add 
                    }
                    else
                    {
                        mdc.Add(new MergeDailyCollection
                        {
                            no = "D" + sn,
                            cssno = i.cssno,
                            date = i.paymentdate,
                            newbondid = i.Account_Code,
                            name = i.Name,
                            rankname = i.Rank,
                            newintroducerid = i.Introducer_Code,
                            amount = i.amount,
                            latefine = i.latefee,
                            rebate = i.relief,
                            receiptno = i.rcptno,
                            planname = i.Planname,
                            term = i.Term,
                            newrenew = renew,
                            appcharge = appsum,
                            formfee = formsum,
                            memberfee = memberfees,
                            acformfee = acformfees,
                            totalexp = totalexp,
                            voucher = totalvoucher,
                            dipositsaving = dipositsaving,
                            widthdrawsaving = widthdrawsaving,
                            branchname = br.BranchName,
                            companyname = cr.CompanyName,
                            address = cr.Address,
                            emailid = cr.Emailid,
                            contact = cr.Contact,
                            domainname = cr.HeadOffice,
                            sdate = stdate,
                            edate = enddate,

                            paymethod = i.paymethod,
                            bank = i.bank,
                            Bbranch = i.Bbranch,
                            Account = i.Account,
                            ACholdername = i.ACholdername,
                            IFSCCode = i.IFSCCode,
                            ChequeAmount = i.ChequeAmount,
                            chequeno = i.chequeno,
                            Chequedate = i.Chequedate,
                            newagenid = i.transactiontype
                        });
                    }
                    sn = sn + 1;
                }

                //------Viewdata for display in page------------------------------------------

            }
            else
            {
                Response.Write("<script>alert('There is no collection in this date')</script>");
            }

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "CDailycollection.rpt"));
            rd.SetDataSource(mdc);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();

            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                string dt = pdate.ToShortDateString();

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
        public ActionResult RPStatementReport()
        {

            List<BalanceReport> cbrlist = new List<BalanceReport>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(cbrlist);
            }
        }
        [HttpPost]
        public ActionResult RPStatementReport(DateTime sdate, DateTime edate)
        {
            List<BalanceReport> cbrlist = new List<BalanceReport>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var pr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(d => d.BranchCode == pr.BranchCode);
                Double totalsalary = 0;
                Double ytotalsalary = 0;
                var cr = db.CompanyInfos.Single(c => c.Id == 1);

                var plan1 = db.Plans.Single(p1 => p1.Plancode == 1);
                var plan2 = db.Plans.Single(p2 => p2.Plancode == 2);
                var plan3 = db.Plans.Single(p3 => p3.Plancode == 3);
                var tbd = (from ap in db.appltabs where ap.status == 1 select ap.bondid);
                /////Yesterday's Collection /////
                var todayfdcoll = (from df in db.Installmenttabs where df.paymentdate < sdate && df.mode == "Single" && df.type != "Adjustment" && df.branch == br.BranchCode && tbd.Contains(df.bondid) select df.payamount).DefaultIfEmpty(0).Sum();
                var todayRdcoll = (from df in db.Installmenttabs where df.paymentdate < sdate && df.mode != "Single" && df.type != "Adjustment" && df.branch == br.BranchCode && df.installmentno == 1 && df.planname == plan1.shortcutname && tbd.Contains(df.bondid) select df.payamount).DefaultIfEmpty(0).Sum();
                var bookingy = (from df in db.appltabs where df.formdate < sdate && df.type != "Adjustment" && tbd.Contains(df.bondid) && df.branchcode == br.BranchCode select df.bookingamount).DefaultIfEmpty(0).Sum();
                var advancey = (from df in db.appltabs where df.formdate < sdate && df.type != "Adjustment" && tbd.Contains(df.bondid) && df.branchcode == br.BranchCode select df.downpayment).DefaultIfEmpty(0).Sum();
                var todayNewBondcoll = todayfdcoll + todayRdcoll;
                var todayRenewalBondcoll = (from df in db.Installmenttabs where df.paymentdate < sdate && df.installmentno != 1 && df.type != "Adjustment" && df.branch == br.BranchCode && tbd.Contains(df.bondid) select df.payamount).DefaultIfEmpty(0).Sum();

                var yeslatefee = (from df in db.Installmenttabs where df.paymentdate < sdate && df.type != "Adjustment" && df.branch == br.BranchCode && tbd.Contains(df.bondid) select df.latefine).DefaultIfEmpty(0).Sum();
                var yesreleif = (from df in db.Installmenttabs where df.paymentdate < sdate && df.type != "Adjustment" && df.branch == br.BranchCode && tbd.Contains(df.bondid) select df.reliefrs).DefaultIfEmpty(0).Sum();
                var ylatefee = yeslatefee - yesreleif;

                var bformfeey = (from df in db.AgentDetails where df.Doj < sdate && df.BranchCode == br.BranchCode select df.formfee).DefaultIfEmpty(0).Sum();
                var cformfeey = (from df in db.appltabs where df.formdate < sdate && df.type != "Adjustment" && df.branchcode == br.BranchCode && tbd.Contains(df.bondid) select df.applicationcharge).DefaultIfEmpty(0).Sum();
                var mformfeey = (from df in db.Member_tabs where df.Cdate < sdate && df.BranchCode == br.BranchCode select df.Fee).DefaultIfEmpty(0).Sum();
                var acformfeey = (from df in db.SavingAccountInfos where df.Doj < sdate && df.branchcode == br.BranchCode select df.formfee).DefaultIfEmpty(0).Sum();
                var deposity = (from df in db.TransactionTabs where df.pdate < sdate && df.branchcode == br.BranchCode select df.credit).DefaultIfEmpty(0).Sum();
                var intotaly = Math.Round((todayRenewalBondcoll + bformfeey + cformfeey + mformfeey + acformfeey + deposity + ylatefee + bookingy + advancey), 2);

                var spotcommy = (from o in db.spotcommission_tabs where o.date < sdate && o.branchcode == br.BranchCode select o.commission).DefaultIfEmpty(0).Sum();
                var AdvPayment = (from df in db.AdvBrokerPaymentTabs where df.date < sdate && df.branchcode == br.BranchCode select df.amount).DefaultIfEmpty(0).Sum();
                var withdrawy = (from df in db.TransactionTabs where df.pdate < sdate && df.branchcode == br.BranchCode select df.debit).DefaultIfEmpty(0).Sum();
                var expansey = (from df in db.Expenses where df.date_time < sdate && df.branchcode == br.BranchCode select df.amount).DefaultIfEmpty(0).Sum();
                var todayVouchercoll = (from df in db.Voucher_Reports where df.date < sdate && df.branchcode == br.BranchCode select df.netamount).DefaultIfEmpty(0).Sum();
                var mat = (from df in db.MaturityTabs where df.maturitydate < sdate && df.branchcode == br.BranchCode select df.maturityamount).DefaultIfEmpty(0).Sum();
                var emplisty = (from se in db.Salary_ems where se.date < sdate select se).ToList();
                foreach (var el in emplisty)
                {
                    var erow = db.Emp_Regs.Single(er => er.empid == el.empid);
                    var branchcode = erow.branchcode;
                    if (branchcode == br.BranchCode)
                    {
                        var ysalary = (from df in db.Salary_ems where df.date < sdate && df.empid == el.empid select df.netsalary).DefaultIfEmpty(0).Sum();
                        ytotalsalary = totalsalary + ysalary;
                    }
                }
                var extotaly = Math.Round((expansey + todayVouchercoll + ytotalsalary + withdrawy + AdvPayment + mat + spotcommy), 2);
                var grandtotaly = intotaly - extotaly;

                /////Today's Collection /////
                var todayfdcoll1 = (from df in db.Installmenttabs where df.paymentdate >= sdate && df.paymentdate <= edate && df.mode == "Single" && df.type != "Adjustment" && df.branch == br.BranchCode && tbd.Contains(df.bondid) select df.payamount).DefaultIfEmpty(0).Sum();
                var todayRdcoll1 = (from df in db.Installmenttabs where df.paymentdate >= sdate && df.paymentdate <= edate && df.mode != "Single" && df.installmentno == 1 && df.type != "Adjustment" && df.branch == br.BranchCode && df.planname == plan1.shortcutname && tbd.Contains(df.bondid) select df.payamount).DefaultIfEmpty(0).Sum();
                var todaybooking = (from df in db.appltabs where df.formdate >= sdate && df.formdate <= edate && df.type != "Adjustment" && tbd.Contains(df.bondid) && df.branchcode == br.BranchCode select df.bookingamount).DefaultIfEmpty(0).Sum();
                var todayadvance = (from df in db.appltabs where df.formdate >= sdate && df.formdate <= edate && df.type != "Adjustment" && tbd.Contains(df.bondid) && df.branchcode == br.BranchCode select df.downpayment).DefaultIfEmpty(0).Sum();
                var todayNewBondcoll1 = todayfdcoll1 + todayRdcoll1;
                var todayRenewalBondcoll1 = (from df in db.Installmenttabs where df.paymentdate >= sdate && df.paymentdate <= edate && df.installmentno != 1 && df.branch == br.BranchCode && df.type != "Adjustment" && tbd.Contains(df.bondid) select df.payamount).DefaultIfEmpty(0).Sum();

                var todaylatefee = (from df in db.Installmenttabs where df.paymentdate >= sdate && df.paymentdate <= edate && df.type != "Adjustment" && df.branch == br.BranchCode && tbd.Contains(df.bondid) select df.latefine).DefaultIfEmpty(0).Sum();
                var todayreleif = (from df in db.Installmenttabs where df.paymentdate >= sdate && df.paymentdate <= edate && df.type != "Adjustment" && df.branch == br.BranchCode && tbd.Contains(df.bondid) select df.reliefrs).DefaultIfEmpty(0).Sum();
                var latefee = todaylatefee - todayreleif;

                var bformfee = (from df in db.AgentDetails where df.Doj >= sdate && df.Doj <= edate && df.BranchCode == br.BranchCode select df.formfee).DefaultIfEmpty(0).Sum();
                var cformfee = (from df in db.appltabs where df.formdate >= sdate && df.formdate <= edate && df.type != "Adjustment" && df.branchcode == br.BranchCode && tbd.Contains(df.bondid) select df.applicationcharge).DefaultIfEmpty(0).Sum();
                var mformfee = (from df in db.Member_tabs where df.Cdate >= sdate && df.Cdate <= edate && df.BranchCode == br.BranchCode select df.Fee).DefaultIfEmpty(0).Sum();
                var acformfee = (from df in db.SavingAccountInfos where df.Doj >= sdate && df.Doj <= edate && df.branchcode == br.BranchCode select df.formfee).DefaultIfEmpty(0).Sum();
                var deposit = (from df in db.TransactionTabs where df.pdate >= sdate && df.pdate <= edate && df.branchcode == br.BranchCode select df.credit).DefaultIfEmpty(0).Sum();
                var intotal = Math.Round((todayRenewalBondcoll1 + bformfee + cformfee + mformfee + acformfee + deposit + latefee + todaybooking + todayadvance), 2);

                var spotcomm = (from o in db.spotcommission_tabs where o.date >= sdate && o.date <= edate && o.branchcode == br.BranchCode select o.commission).DefaultIfEmpty(0).Sum();
                var AdvPayment1 = (from df in db.AdvBrokerPaymentTabs where df.date >= sdate && df.date <= edate && df.branchcode == br.BranchCode select df.amount).DefaultIfEmpty(0).Sum();
                var withdraw = (from df in db.TransactionTabs where df.pdate >= sdate && df.pdate <= edate && df.branchcode == br.BranchCode select df.debit).DefaultIfEmpty(0).Sum();
                var expanse = (from df in db.Expenses where df.date_time >= sdate && df.date_time <= edate && df.branchcode == br.BranchCode select df.amount).DefaultIfEmpty(0).Sum();
                var todayVouchercoll1 = (from df in db.Voucher_Reports where df.date >= sdate && df.date <= edate && df.branchcode == br.BranchCode select df.netamount).DefaultIfEmpty(0).Sum();
                var mat1 = (from df in db.MaturityTabs where df.maturitydate >= sdate && df.maturitydate <= edate && df.branchcode == br.BranchCode select df.maturityamount).DefaultIfEmpty(0).Sum();
                var emplist = (from se in db.Salary_ems where se.date >= sdate && se.date <= edate select se).ToList();
                foreach (var el in emplist)
                {

                    var salary = (from df in db.Salary_ems where df.date >= sdate && df.date <= edate && df.empid == el.empid select df.netsalary).DefaultIfEmpty(0).Sum();
                    totalsalary = totalsalary + salary;
                }

                var extotal = Math.Round((expanse + todayVouchercoll1 + totalsalary + withdraw + AdvPayment1 + mat1 + spotcomm), 2);
                var grandtotal = intotal - extotal;

                cbrlist.Add(new BalanceReport { bookingamount = todaybooking, downpayment = todayadvance, maturity = mat1, pps = 0, fpo = todayRdcoll1, dpo = todayfdcoll1, advpayment = AdvPayment1, newbond = todayNewBondcoll1, renewel = todayRenewalBondcoll1, latefee = latefee, bformfee = bformfee, cformfee = cformfee, mformfee = mformfee, acformfee = acformfee, deposit = deposit, withdraw = withdraw, intotal = intotal, expence = expanse, spotcomm = spotcomm, voucher = todayVouchercoll1, salary = totalsalary, extotal = extotal, grandtotal = grandtotal, branchname = br.BranchName, companyname = cr.CompanyName, openingbalance = grandtotaly, sdate = sdate, edate = edate, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });

                stdate = sdate;
                enddate = edate;
                return View(cbrlist);
            }
        }
        public ActionResult PrintClosingBalance()
        {
            List<BalanceReport> cbrlist = new List<BalanceReport>();

            var pr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
            var br = db.Branchtabs.Single(d => d.BranchCode == pr.BranchCode);
            Double totalsalary = 0;
            Double ytotalsalary = 0;
            var cr = db.CompanyInfos.Single(c => c.Id == 1);

            var plan1 = db.Plans.Single(p1 => p1.Plancode == 1);
            var plan2 = db.Plans.Single(p2 => p2.Plancode == 2);
            var plan3 = db.Plans.Single(p3 => p3.Plancode == 3);
            var tbd = (from ap in db.appltabs where ap.status == 1 select ap.bondid);
            /////Yesterday's Collection /////
            var todayfdcoll = (from df in db.Installmenttabs where df.paymentdate < stdate && df.mode == "Single" && df.type != "Adjustment" && df.branch == br.BranchCode && tbd.Contains(df.bondid) select df.payamount).DefaultIfEmpty(0).Sum();
            var todayRdcoll = (from df in db.Installmenttabs where df.paymentdate < stdate && df.mode != "Single" && df.type != "Adjustment" && df.branch == br.BranchCode && df.installmentno == 1 && df.planname == plan1.shortcutname && tbd.Contains(df.bondid) select df.payamount).DefaultIfEmpty(0).Sum();
            var bookingy = (from df in db.appltabs where df.formdate < stdate && df.type != "Adjustment" && tbd.Contains(df.bondid) && df.branchcode == br.BranchCode select df.bookingamount).DefaultIfEmpty(0).Sum();
            var advancey = (from df in db.appltabs where df.formdate < stdate && df.type != "Adjustment" && tbd.Contains(df.bondid) && df.branchcode == br.BranchCode select df.downpayment).DefaultIfEmpty(0).Sum();
            var todayNewBondcoll = todayfdcoll + todayRdcoll;
            var todayRenewalBondcoll = (from df in db.Installmenttabs where df.paymentdate < stdate && df.installmentno != 1 && df.type != "Adjustment" && df.branch == br.BranchCode && tbd.Contains(df.bondid) select df.payamount).DefaultIfEmpty(0).Sum();

            var yeslatefee = (from df in db.Installmenttabs where df.paymentdate < stdate && df.type != "Adjustment" && df.branch == br.BranchCode && tbd.Contains(df.bondid) select df.latefine).DefaultIfEmpty(0).Sum();
            var yesreleif = (from df in db.Installmenttabs where df.paymentdate < stdate && df.type != "Adjustment" && df.branch == br.BranchCode && tbd.Contains(df.bondid) select df.reliefrs).DefaultIfEmpty(0).Sum();
            var ylatefee = yeslatefee - yesreleif;

            var bformfeey = (from df in db.AgentDetails where df.Doj < stdate && df.BranchCode == br.BranchCode select df.formfee).DefaultIfEmpty(0).Sum();
            var cformfeey = (from df in db.appltabs where df.formdate < stdate && df.type != "Adjustment" && df.branchcode == br.BranchCode && tbd.Contains(df.bondid) select df.applicationcharge).DefaultIfEmpty(0).Sum();
            var mformfeey = (from df in db.Member_tabs where df.Cdate < stdate && df.BranchCode == br.BranchCode select df.Fee).DefaultIfEmpty(0).Sum();
            var acformfeey = (from df in db.SavingAccountInfos where df.Doj < stdate && df.branchcode == br.BranchCode select df.formfee).DefaultIfEmpty(0).Sum();
            var deposity = (from df in db.TransactionTabs where df.pdate < stdate && df.branchcode == br.BranchCode select df.credit).DefaultIfEmpty(0).Sum();
            var intotaly = Math.Round((todayRenewalBondcoll + bformfeey + cformfeey + mformfeey + acformfeey + deposity + ylatefee + bookingy + advancey), 2);

            var spotcommy = (from o in db.spotcommission_tabs where o.date < stdate && o.branchcode == br.BranchCode select o.commission).DefaultIfEmpty(0).Sum();
            var AdvPayment = (from df in db.AdvBrokerPaymentTabs where df.date < stdate && df.branchcode == br.BranchCode select df.amount).DefaultIfEmpty(0).Sum();
            var withdrawy = (from df in db.TransactionTabs where df.pdate < stdate && df.branchcode == br.BranchCode select df.debit).DefaultIfEmpty(0).Sum();
            var expansey = (from df in db.Expenses where df.date_time < stdate && df.branchcode == br.BranchCode select df.amount).DefaultIfEmpty(0).Sum();
            var todayVouchercoll = (from df in db.Voucher_Reports where df.date < stdate && df.branchcode == br.BranchCode select df.netamount).DefaultIfEmpty(0).Sum();
            var mat = (from df in db.MaturityTabs where df.maturitydate < stdate && df.branchcode == br.BranchCode select df.maturityamount).DefaultIfEmpty(0).Sum();
            var emplisty = (from se in db.Salary_ems where se.date < stdate select se).ToList();
            foreach (var el in emplisty)
            {
                var erow = db.Emp_Regs.Single(er => er.empid == el.empid);
                var branchcode = erow.branchcode;
                if (branchcode == br.BranchCode)
                {
                    var ysalary = (from df in db.Salary_ems where df.date < stdate && df.empid == el.empid select df.netsalary).DefaultIfEmpty(0).Sum();
                    ytotalsalary = totalsalary + ysalary;
                }
            }
            var extotaly = Math.Round((expansey + todayVouchercoll + ytotalsalary + withdrawy + AdvPayment + mat + spotcommy), 2);
            var grandtotaly = intotaly - extotaly;

            /////Today's Collection /////
            var todayfdcoll1 = (from df in db.Installmenttabs where df.paymentdate >= stdate && df.paymentdate <= enddate && df.mode == "Single" && df.type != "Adjustment" && df.branch == br.BranchCode && tbd.Contains(df.bondid) select df.payamount).DefaultIfEmpty(0).Sum();
            var todayRdcoll1 = (from df in db.Installmenttabs where df.paymentdate >= stdate && df.paymentdate <= enddate && df.mode != "Single" && df.installmentno == 1 && df.type != "Adjustment" && df.branch == br.BranchCode && df.planname == plan1.shortcutname && tbd.Contains(df.bondid) select df.payamount).DefaultIfEmpty(0).Sum();
            var todaybooking = (from df in db.appltabs where df.formdate >= stdate && df.formdate <= enddate && df.type != "Adjustment" && tbd.Contains(df.bondid) && df.branchcode == br.BranchCode select df.bookingamount).DefaultIfEmpty(0).Sum();
            var todayadvance = (from df in db.appltabs where df.formdate >= stdate && df.formdate <= enddate && df.type != "Adjustment" && tbd.Contains(df.bondid) && df.branchcode == br.BranchCode select df.downpayment).DefaultIfEmpty(0).Sum();
            var todayNewBondcoll1 = todayfdcoll1 + todayRdcoll1;
            var todayRenewalBondcoll1 = (from df in db.Installmenttabs where df.paymentdate >= stdate && df.paymentdate <= enddate && df.installmentno != 1 && df.branch == br.BranchCode && df.type != "Adjustment" && tbd.Contains(df.bondid) select df.payamount).DefaultIfEmpty(0).Sum();

            var todaylatefee = (from df in db.Installmenttabs where df.paymentdate >= stdate && df.paymentdate <= enddate && df.type != "Adjustment" && df.branch == br.BranchCode && tbd.Contains(df.bondid) select df.latefine).DefaultIfEmpty(0).Sum();
            var todayreleif = (from df in db.Installmenttabs where df.paymentdate >= stdate && df.paymentdate <= enddate && df.type != "Adjustment" && df.branch == br.BranchCode && tbd.Contains(df.bondid) select df.reliefrs).DefaultIfEmpty(0).Sum();
            var latefee = todaylatefee - todayreleif;

            var bformfee = (from df in db.AgentDetails where df.Doj >= stdate && df.Doj <= enddate && df.BranchCode == br.BranchCode select df.formfee).DefaultIfEmpty(0).Sum();
            var cformfee = (from df in db.appltabs where df.formdate >= stdate && df.formdate <= enddate && df.type != "Adjustment" && df.branchcode == br.BranchCode && tbd.Contains(df.bondid) select df.applicationcharge).DefaultIfEmpty(0).Sum();
            var mformfee = (from df in db.Member_tabs where df.Cdate >= stdate && df.Cdate <= enddate && df.BranchCode == br.BranchCode select df.Fee).DefaultIfEmpty(0).Sum();
            var acformfee = (from df in db.SavingAccountInfos where df.Doj >= stdate && df.Doj <= enddate && df.branchcode == br.BranchCode select df.formfee).DefaultIfEmpty(0).Sum();
            var deposit = (from df in db.TransactionTabs where df.pdate >= stdate && df.pdate <= enddate && df.branchcode == br.BranchCode select df.credit).DefaultIfEmpty(0).Sum();
            var intotal = Math.Round((todayRenewalBondcoll1 + bformfee + cformfee + mformfee + acformfee + deposit + latefee + todaybooking + todayadvance), 2);

            var spotcomm = (from o in db.spotcommission_tabs where o.date >= stdate && o.date <= enddate && o.branchcode == br.BranchCode select o.commission).DefaultIfEmpty(0).Sum();
            var AdvPayment1 = (from df in db.AdvBrokerPaymentTabs where df.date >= stdate && df.date <= enddate && df.branchcode == br.BranchCode select df.amount).DefaultIfEmpty(0).Sum();
            var withdraw = (from df in db.TransactionTabs where df.pdate >= stdate && df.pdate <= enddate && df.branchcode == br.BranchCode select df.debit).DefaultIfEmpty(0).Sum();
            var expanse = (from df in db.Expenses where df.date_time >= stdate && df.date_time <= enddate && df.branchcode == br.BranchCode select df.amount).DefaultIfEmpty(0).Sum();
            var todayVouchercoll1 = (from df in db.Voucher_Reports where df.date >= stdate && df.date <= enddate && df.branchcode == br.BranchCode select df.netamount).DefaultIfEmpty(0).Sum();
            var mat1 = (from df in db.MaturityTabs where df.maturitydate >= stdate && df.maturitydate <= enddate && df.branchcode == br.BranchCode select df.maturityamount).DefaultIfEmpty(0).Sum();
            var emplist = (from se in db.Salary_ems where se.date >= stdate && se.date <= enddate select se).ToList();
            foreach (var el in emplist)
            {

                var salary = (from df in db.Salary_ems where df.date >= stdate && df.date <= enddate && df.empid == el.empid select df.netsalary).DefaultIfEmpty(0).Sum();
                totalsalary = totalsalary + salary;
            }

            var extotal = Math.Round((expanse + todayVouchercoll1 + totalsalary + withdraw + AdvPayment1 + mat1 + spotcomm), 2);
            var grandtotal = intotal - extotal;

            cbrlist.Add(new BalanceReport { bookingamount = todaybooking, downpayment = todayadvance, maturity = mat1, pps = 0, fpo = todayRdcoll1, dpo = todayfdcoll1, advpayment = AdvPayment1, newbond = todayNewBondcoll1, renewel = todayRenewalBondcoll1, latefee = latefee, bformfee = bformfee, cformfee = cformfee, mformfee = mformfee, acformfee = acformfee, deposit = deposit, withdraw = withdraw, intotal = intotal, expence = expanse, spotcomm = spotcomm, voucher = todayVouchercoll1, salary = totalsalary, extotal = extotal, grandtotal = grandtotal, branchname = br.BranchName, companyname = cr.CompanyName, openingbalance = grandtotaly, sdate = stdate, edate = enddate, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports/Branch"), "ClosingBalanceReport.rpt"));
            rd.SetDataSource(cbrlist);
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


            return View(cbrlist);
        }

        [HttpGet]
        public ActionResult OperatorCashReport()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View();
            }
        }

        [HttpGet]
        public ActionResult DailyCollectionReport()
        {
            List<DailyCollection> dc = new List<DailyCollection>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                ViewData["date"] = string.Empty;
                ViewData["newagentid"] = string.Empty;
                return View(dc);
            }
        }
        [HttpPost]
        public ActionResult DailyCollectionReport(DateTime date, string newagentid, int filter, int year)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                ViewData["date"] = date;
                ViewData["newagentid"] = newagentid;
                var bbb = db.Branchtabs.Single(bc => bc.BranchCode == pr.BranchCode);
                ViewData["Branch"] = bbb.BranchCode;

                List<DailyCollection> dc = new List<DailyCollection>();
                List<Installment> inl = new List<Installment>();
                List<Memberfees> mfeeslist = new List<Memberfees>();
                Double formsum = 0, appsum = 0, totalexp = 0, memberfees = 0, acformfees = 0, totalvoucher = 0, dipositsaving = 0, widthdrawsaving = 0;
                Double formsumB = 0, appsumB = 0, memberfeesB = 0;
                var pst = db.Plans.Single(c => c.Plancode == 4);

                var ttbd = (from ap in db.appltabs where ap.status == 1 select ap.bondid);
                dipositsaving = (from j in db.TransactionTabs where j.pdate == date && j.opid == pr.OperatorId select j.credit).DefaultIfEmpty(0).Sum();
                widthdrawsaving = (from j in db.TransactionTabs where j.pdate == date && j.opid == pr.OperatorId select j.debit).DefaultIfEmpty(0).Sum();
                totalvoucher = (from ag in db.Voucher_Reports where ag.date == date && ag.Operatorid == pr.OperatorId select ag.netamount).DefaultIfEmpty(0).Sum();
                totalexp = (from ag in db.Expenses where ag.date_time == date && ag.opid == pr.OperatorId select ag.amount).DefaultIfEmpty(0).Sum();
                appsum = (from b in db.appltabs where b.formdate == date && b.opid == pr.OperatorId && b.status == 1 select b.applicationcharge).DefaultIfEmpty(0).Sum();
                formsum = (from ag in db.AgentDetails where ag.Doj == date && ag.operatorid == pr.OperatorId select ag.formfee).DefaultIfEmpty(0).Sum();
                memberfees = (from ag in db.Member_tabs where ag.Cdate == date && ag.Opid == pr.OperatorId select ag.Fee).DefaultIfEmpty(0).Sum();
                acformfees = (from ag in db.SavingAccountInfos where ag.Doj == date && ag.opid == pr.OperatorId select ag.formfee).DefaultIfEmpty(0).Sum();

                if (newagentid != null && newagentid != "")
                {
                    switch (filter)
                    {
                        case 1:
                            Double fee = 0;
                            var tbd = (from ap in db.appltabs where ap.newintroducerid == newagentid select ap.bondid);
                            var bd = (from p in db.Installmenttabs where p.paymentdate == date && p.paymentno != 0 && tbd.Contains(p.bondid) && p.opid == pr.OperatorId select new { p.bondid }).Distinct();
                            appsumB = (from ag in db.appltabs where ag.formdate == date && ag.newintroducerid == newagentid && ag.opid == pr.OperatorId && ag.status == 1 select ag.applicationcharge).DefaultIfEmpty(0).Sum();
                            formsumB = (from ag in db.AgentDetails where ag.Doj == date && ag.NewAgentId == newagentid && ag.operatorid == pr.OperatorId select ag.formfee).DefaultIfEmpty(0).Sum();
                            var agent = db.appltabs.Where(s => s.formdate == date && s.newintroducerid == newagentid && s.opid == pr.OperatorId && s.status == 1).ToList();
                            var agent1 = db.AgentDetails.Where(s => s.Doj == date && s.NewAgentId == newagentid && s.operatorid == pr.OperatorId).ToList();
                            foreach (var a in agent)
                            {

                                mfeeslist.Add(new Memberfees { newmemberid = a.newmemberid });
                            }
                            foreach (var b in agent1)
                            {
                                mfeeslist.Add(new Memberfees { newmemberid = b.newmemberid });
                            }
                            var result = (from m in mfeeslist select new { m.newmemberid }).Distinct().ToList();
                            foreach (var mf in result)
                            {
                                var mem1 = db.Member_tabs.Single(s => s.NewMemberId == mf.newmemberid);
                                if (mem1.Cdate == date)
                                {

                                    fee = mem1.Fee + fee;
                                }
                            }
                            memberfeesB = fee;
                            if (bd.Count() > 0)
                            {
                                foreach (var t in bd.ToList())
                                {

                                    var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                                    var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                                    var irow = db.Installmenttabs.Where(im => im.bondid == t.bondid && im.paymentdate == date && im.payamount != 0 && im.type != "Adjustment").ToList();
                                    foreach (var cl in irow.ToList())
                                    {
                                        var finalamount = 0.0;
                                        if (cl.planname.Substring(0, 3) == pst.shortcutname)
                                        {
                                            finalamount = app.totalcon;
                                        }
                                        else
                                        {
                                            finalamount = cl.payamount;
                                        }
                                        inl.Add(new Installment
                                        {
                                            rcptno = cl.receiptno,

                                            latefee = cl.latefine,
                                            relief = cl.reliefrs,
                                            cssno = Convert.ToInt32(cl.cssno),
                                            year = cl.year,
                                            installno = cl.installmentno,
                                            Account_Code = app.newbondid,
                                            Name = app.name,
                                            Introducer_Code = agt.NewAgentId,
                                            Rank = agt.RankName,
                                            amount = finalamount,
                                            Planname = app.planname,
                                            Term = app.term,
                                            branchcode = cl.branch

                                        });
                                    }

                                }

                                //-------Insertion in Daily Collection Temp Tab--------------------------
                                foreach (var i in inl.ToList())
                                {
                                    var bb = db.Branchtabs.Single(vv => vv.BranchCode == i.branchcode);
                                    string renew;
                                    if (i.installno == 1)
                                    {
                                        renew = "New";
                                    }
                                    else
                                    {
                                        renew = "Renew";
                                    }
                                    if (renew == "Renew" && i.Planname.Substring(0, 3) == pst.shortcutname)
                                    {

                                    }
                                    else
                                    {
                                        dc.Add(new DailyCollection
                                        {
                                            CSS_No = i.cssno,
                                            ondate = date,
                                            Account_Code = i.Account_Code,
                                            Name = i.Name,
                                            Rank = i.Rank,
                                            Unit_Code = i.Unit_Code,
                                            Introducer_Code = i.Introducer_Code,
                                            Amt_Deposit = i.amount,
                                            Late_Fee = i.latefee,
                                            Rebate = i.relief,
                                            Reciept_No = i.rcptno,
                                            Planname = i.Planname,
                                            Term = i.Term,
                                            New = renew,
                                            appcharge = appsumB,
                                            formfee = formsumB,
                                            memberfee = memberfeesB,
                                            acformfee = 0,
                                            totalexp = totalexp,
                                            Voucher = totalvoucher,
                                            branchcode = bbb.BranchCode,
                                            dipositsaving = 0,
                                            widthdrawsaving = 0,
                                            companyname = cr.CompanyName,
                                            address = cr.Address,
                                            emailid = cr.Emailid,
                                            contact = cr.Contact,
                                            domainname = cr.HeadOffice,
                                            branchname = bb.BranchName
                                        });
                                    }


                                }


                                //------Viewdata for display in page------------------------------------------


                            }
                            else if (dipositsaving > 0 || widthdrawsaving > 0)
                            {
                                dc.Add(new DailyCollection
                                {
                                    CSS_No = 0,
                                    ondate = date,
                                    Account_Code = "",
                                    Name = "",
                                    Rank = "",
                                    Unit_Code = "",
                                    Introducer_Code = "",
                                    Amt_Deposit = 0,
                                    Late_Fee = 0,
                                    Rebate = 0,
                                    Reciept_No = "",
                                    Planname = "",
                                    Term = 0,
                                    appcharge = appsum,
                                    formfee = formsum,
                                    totalexp = totalexp,
                                    acformfee = 0,
                                    Voucher = totalvoucher,
                                    branchcode = bbb.BranchCode,
                                    memberfee = 0,
                                    dipositsaving = 0,
                                    widthdrawsaving = 0,
                                    companyname = cr.CompanyName,
                                    address = cr.Address,
                                    emailid = cr.Emailid,
                                    contact = cr.Contact,
                                    domainname = cr.HeadOffice
                                });
                            }

                            else
                            {
                                Response.Write("<script>alert('There is no collection in this date')</script>");
                            }
                            break;

                        case 2:
                            Double fee1 = 0;
                            var tbd2 = (from ap in db.appltabs where ap.newintroducerid == newagentid select ap.bondid);
                            appsumB = (from ag in db.appltabs where ag.formdate == date && ag.newintroducerid == newagentid && ag.opid == pr.OperatorId && ag.status == 1 select ag.applicationcharge).DefaultIfEmpty(0).Sum();
                            formsumB = (from ag in db.AgentDetails where ag.Doj == date && ag.NewAgentId == newagentid && ag.operatorid == pr.OperatorId select ag.formfee).DefaultIfEmpty(0).Sum();
                            var agentB = db.appltabs.Where(s => s.formdate == date && s.newintroducerid == newagentid && s.opid == pr.OperatorId && s.status == 1).ToList();
                            var agentB1 = db.AgentDetails.Where(s => s.Doj == date && s.NewAgentId == newagentid && s.operatorid == pr.OperatorId).ToList();
                            foreach (var a in agentB)
                            {

                                mfeeslist.Add(new Memberfees { newmemberid = a.newmemberid });
                            }
                            foreach (var b in agentB1)
                            {
                                mfeeslist.Add(new Memberfees { newmemberid = b.newmemberid });
                            }
                            var resultB = (from m in mfeeslist select new { m.newmemberid }).Distinct().ToList();
                            foreach (var mf in resultB)
                            {
                                var mem1 = db.Member_tabs.Single(s => s.NewMemberId == mf.newmemberid);
                                if (mem1.Cdate == date)
                                {

                                    fee1 = mem1.Fee + fee1;
                                }
                            }
                            memberfeesB = fee1;
                            var bd2 = (from p in db.Installmenttabs where p.paymentdate == date && p.paymentno != 0 && tbd2.Contains(p.bondid) && p.opid == pr.OperatorId select new { p.bondid }).Distinct();
                            if (bd2.Count() > 0)
                            {
                                foreach (var t in bd2.ToList())
                                {

                                    var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                                    var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                                    var irow = db.Installmenttabs.Where(im => im.bondid == t.bondid && im.paymentdate == date && im.payamount != 0 && im.installmentno == 1 && im.type != "Adjustment").ToList();
                                    foreach (var cl in irow.ToList())
                                    {
                                        var finalamount = 0.0;
                                        if (cl.planname.Substring(0, 3) == pst.shortcutname)
                                        {
                                            finalamount = app.totalcon;
                                        }
                                        else
                                        {
                                            finalamount = cl.payamount;
                                        }
                                        inl.Add(new Installment
                                        {
                                            rcptno = cl.receiptno,
                                            latefee = cl.latefine,
                                            relief = cl.reliefrs,
                                            cssno = Convert.ToInt32(cl.cssno),
                                            year = cl.year,
                                            installno = cl.installmentno,
                                            Account_Code = app.newbondid,
                                            Name = app.name,
                                            Introducer_Code = agt.NewAgentId,
                                            Rank = agt.RankName,
                                            amount = finalamount,
                                            Planname = app.planname,
                                            Term = app.term,
                                            branchcode = cl.branch

                                        });
                                    }

                                }

                                foreach (var i in inl.ToList())
                                {
                                    var bb = db.Branchtabs.Single(vv => vv.BranchCode == i.branchcode);
                                    string renew;
                                    if (i.installno == 1)
                                    {
                                        renew = "New";
                                    }
                                    else
                                    {
                                        renew = "Renew";
                                    }
                                    if (renew == "Renew" && i.Planname.Substring(0, 3) == pst.shortcutname)
                                    {

                                    }
                                    else
                                    {
                                        dc.Add(new DailyCollection
                                        {
                                            CSS_No = i.cssno,
                                            ondate = date,
                                            Account_Code = i.Account_Code,
                                            Name = i.Name,
                                            Rank = i.Rank,
                                            Unit_Code = i.Unit_Code,
                                            Introducer_Code = i.Introducer_Code,
                                            Amt_Deposit = i.amount,
                                            Late_Fee = i.latefee,
                                            Rebate = i.relief,
                                            Reciept_No = i.rcptno,
                                            Planname = i.Planname,
                                            Term = i.Term,
                                            New = renew,
                                            appcharge = appsumB,
                                            formfee = formsumB,
                                            memberfee = memberfeesB,
                                            acformfee = 0,
                                            totalexp = totalexp,
                                            Voucher = totalvoucher,
                                            branchcode = bbb.BranchCode,
                                            dipositsaving = 0,
                                            widthdrawsaving = 0,
                                            companyname = cr.CompanyName,
                                            address = cr.Address,
                                            emailid = cr.Emailid,
                                            contact = cr.Contact,
                                            domainname = cr.HeadOffice,
                                            branchname = bb.BranchName
                                        });
                                    }


                                }

                            }
                            else
                            {
                                Response.Write("<script>alert('There is no collection in this date')</script>");
                            }
                            break;
                        case 3:
                            if (year > 0)
                            {
                                var tbd3 = (from ap in db.appltabs where ap.newintroducerid == newagentid select ap.bondid);
                                var bd3 = (from p in db.Installmenttabs where p.paymentdate == date && p.paymentno != 0 && tbd3.Contains(p.bondid) && p.opid == pr.OperatorId select new { p.bondid }).Distinct();
                                if (bd3.Count() > 0)
                                {
                                    foreach (var t in bd3.ToList())
                                    {

                                        var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                                        var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                                        var irow = db.Installmenttabs.Where(im => im.bondid == t.bondid && im.paymentdate == date && im.payamount != 0 && im.installmentno > 1 && im.year == year && im.type != "Adjustment").ToList();
                                        foreach (var cl in irow.ToList())
                                        {
                                            var finalamount = 0.0;
                                            if (cl.planname.Substring(0, 3) == pst.shortcutname)
                                            {
                                                finalamount = app.totalcon;
                                            }
                                            else
                                            {
                                                finalamount = cl.payamount;
                                            }
                                            inl.Add(new Installment
                                            {
                                                rcptno = cl.receiptno,

                                                latefee = cl.latefine,
                                                relief = cl.reliefrs,
                                                cssno = Convert.ToInt32(cl.cssno),
                                                year = cl.year,
                                                installno = cl.installmentno,
                                                Account_Code = app.newbondid,
                                                Name = app.name,
                                                Introducer_Code = agt.NewAgentId,
                                                Rank = agt.RankName,
                                                amount = finalamount,
                                                Planname = app.planname,
                                                Term = app.term,
                                                branchcode = cl.branch

                                            });
                                        }

                                    }

                                    //-------Insertion in Daily Collection Temp Tab--------------------------
                                    foreach (var i in inl.ToList())
                                    {
                                        var bb = db.Branchtabs.Single(vv => vv.BranchCode == i.branchcode);
                                        string renew;
                                        if (i.installno == 1)
                                        {
                                            renew = "New";
                                        }
                                        else
                                        {
                                            renew = "Renew";
                                        }
                                        if (renew == "Renew" && i.Planname.Substring(0, 3) == pst.shortcutname)
                                        {

                                        }
                                        else
                                        {
                                            dc.Add(new DailyCollection
                                            {
                                                CSS_No = i.cssno,
                                                ondate = date,
                                                Account_Code = i.Account_Code,
                                                Name = i.Name,
                                                Rank = i.Rank,
                                                Unit_Code = i.Unit_Code,
                                                Introducer_Code = i.Introducer_Code,
                                                Amt_Deposit = i.amount,
                                                Late_Fee = i.latefee,
                                                Rebate = i.relief,
                                                Reciept_No = i.rcptno,
                                                Planname = i.Planname,
                                                Term = i.Term,
                                                New = renew,
                                                appcharge = 0,
                                                formfee = 0,
                                                memberfee = 0,
                                                acformfee = 0,
                                                totalexp = 0,
                                                Voucher = 0,
                                                dipositsaving = 0,
                                                widthdrawsaving = 0,
                                                branchcode = bbb.BranchCode,
                                                companyname = cr.CompanyName,
                                                address = cr.Address,
                                                emailid = cr.Emailid,
                                                contact = cr.Contact,
                                                domainname = cr.HeadOffice,
                                                branchname = bb.BranchName
                                            });
                                        }


                                    }

                                }
                                else
                                {
                                    Response.Write("<script>alert('There is no collection in this date')</script>");
                                }
                            }
                            else
                            {
                                var tbd3 = (from ap in db.appltabs where ap.newintroducerid == newagentid select ap.bondid);
                                var bd3 = (from p in db.Installmenttabs where p.paymentdate == date && p.paymentno != 0 && tbd3.Contains(p.bondid) && p.opid == pr.OperatorId select new { p.bondid }).Distinct();
                                if (bd3.Count() > 0)
                                {
                                    foreach (var t in bd3.ToList())
                                    {

                                        var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                                        var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                                        var irow = db.Installmenttabs.Where(im => im.bondid == t.bondid && im.paymentdate == date && im.payamount != 0 && im.installmentno > 1 && im.type != "Adjustment").ToList();
                                        foreach (var cl in irow.ToList())
                                        {
                                            var finalamount = 0.0;
                                            if (cl.planname.Substring(0, 3) == pst.shortcutname)
                                            {
                                                finalamount = app.totalcon;
                                            }
                                            else
                                            {
                                                finalamount = cl.payamount;
                                            }
                                            inl.Add(new Installment
                                            {
                                                rcptno = cl.receiptno,

                                                latefee = cl.latefine,
                                                relief = cl.reliefrs,
                                                cssno = Convert.ToInt32(cl.cssno),
                                                year = cl.year,
                                                installno = cl.installmentno,
                                                Account_Code = app.newbondid,
                                                Name = app.name,
                                                Introducer_Code = agt.NewAgentId,
                                                Rank = agt.RankName,
                                                amount = finalamount,
                                                Planname = app.planname,
                                                Term = app.term,
                                                branchcode = cl.branch

                                            });
                                        }

                                    }

                                    //-------Insertion in Daily Collection Temp Tab--------------------------
                                    foreach (var i in inl.ToList())
                                    {
                                        var bb = db.Branchtabs.Single(vv => vv.BranchCode == i.branchcode);
                                        string renew;
                                        if (i.installno == 1)
                                        {
                                            renew = "New";
                                        }
                                        else
                                        {
                                            renew = "Renew";
                                        }
                                        if (renew == "Renew" && i.Planname.Substring(0, 3) == pst.shortcutname)
                                        {

                                        }
                                        else
                                        {
                                            dc.Add(new DailyCollection
                                            {
                                                CSS_No = i.cssno,
                                                ondate = date,
                                                Account_Code = i.Account_Code,
                                                Name = i.Name,
                                                Rank = i.Rank,
                                                Unit_Code = i.Unit_Code,
                                                Introducer_Code = i.Introducer_Code,
                                                Amt_Deposit = i.amount,
                                                Late_Fee = i.latefee,
                                                Rebate = i.relief,
                                                Reciept_No = i.rcptno,
                                                Planname = i.Planname,
                                                Term = i.Term,
                                                New = renew,
                                                appcharge = 0,
                                                formfee = 0,
                                                memberfee = 0,
                                                acformfee = 0,
                                                totalexp = 0,
                                                Voucher = 0,
                                                dipositsaving = 0,
                                                widthdrawsaving = 0,
                                                branchcode = bbb.BranchCode,
                                                companyname = cr.CompanyName,
                                                address = cr.Address,
                                                emailid = cr.Emailid,
                                                contact = cr.Contact,
                                                domainname = cr.HeadOffice,
                                                branchname = bb.BranchName
                                            });
                                        }


                                    }

                                }
                                else
                                {
                                    Response.Write("<script>alert('There is no collection in this date')</script>");
                                }
                            }
                            break;
                        case 4:

                            if (dipositsaving > 0 || widthdrawsaving > 0)
                            {
                                dc.Add(new DailyCollection
                                {
                                    CSS_No = 0,
                                    ondate = date,
                                    Account_Code = "",
                                    Name = "",
                                    Rank = "",
                                    Unit_Code = "",
                                    Introducer_Code = "",
                                    Amt_Deposit = 0,
                                    Late_Fee = 0,
                                    Rebate = 0,
                                    Reciept_No = "",
                                    Planname = "",
                                    Term = 0,
                                    appcharge = 0,
                                    formfee = 0,
                                    totalexp = 0,
                                    acformfee = 0,
                                    Voucher = 0,
                                    branchcode = bbb.BranchCode,
                                    memberfee = 0,
                                    dipositsaving = 0,
                                    widthdrawsaving = 0,
                                    companyname = cr.CompanyName,
                                    address = cr.Address,
                                    emailid = cr.Emailid,
                                    contact = cr.Contact,
                                    domainname = cr.HeadOffice
                                });
                            }

                            else
                            {
                                Response.Write("<script>alert('There is no collection in this date')</script>");
                            }

                            break;
                    }
                }
                else if (newagentid == null || newagentid == "")
                {
                    switch (filter)
                    {
                        case 1:

                            var bd = (from p in db.Installmenttabs where p.paymentdate == date && p.paymentno != 0 && p.opid == pr.OperatorId && ttbd.Contains(p.bondid) select new { p.bondid }).Distinct();
                            if (bd.Count() > 0)
                            {
                                foreach (var t in bd.ToList())
                                {

                                    var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                                    var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                                    var irow = db.Installmenttabs.Where(im => im.bondid == t.bondid && im.paymentdate == date && im.payamount != 0 && im.type != "Adjustment").ToList();
                                    foreach (var cl in irow.ToList())
                                    {
                                        var finalamount = 0.0;
                                        if (cl.planname.Substring(0, 3) == pst.shortcutname)
                                        {
                                            finalamount = app.totalcon;
                                        }
                                        else
                                        {
                                            finalamount = cl.payamount;
                                        }
                                        inl.Add(new Installment
                                        {
                                            rcptno = cl.receiptno,

                                            latefee = cl.latefine,
                                            relief = cl.reliefrs,
                                            cssno = Convert.ToInt32(cl.cssno),
                                            year = cl.year,
                                            installno = cl.installmentno,
                                            Account_Code = app.newbondid,
                                            Name = app.name,
                                            Introducer_Code = agt.NewAgentId,
                                            Rank = agt.RankName,
                                            amount = finalamount,
                                            Planname = app.planname,
                                            Term = app.term,
                                            branchcode = cl.branch

                                        });
                                    }

                                }

                                //-------Insertion in Daily Collection Temp Tab--------------------------
                                foreach (var i in inl.ToList())
                                {
                                    var bb = db.Branchtabs.Single(vv => vv.BranchCode == i.branchcode);
                                    string renew;
                                    if (i.installno == 1)
                                    {
                                        renew = "New";
                                    }
                                    else
                                    {
                                        renew = "Renew";
                                    }
                                    if (renew == "Renew" && i.Planname.Substring(0, 3) == pst.shortcutname)
                                    {

                                    }
                                    else
                                    {
                                        dc.Add(new DailyCollection
                                        {
                                            CSS_No = i.cssno,
                                            ondate = date,
                                            Account_Code = i.Account_Code,
                                            Name = i.Name,
                                            Rank = i.Rank,
                                            Unit_Code = i.Unit_Code,
                                            Introducer_Code = i.Introducer_Code,
                                            Amt_Deposit = i.amount,
                                            Late_Fee = i.latefee,
                                            Rebate = i.relief,
                                            Reciept_No = i.rcptno,
                                            Planname = i.Planname,
                                            Term = i.Term,
                                            New = renew,
                                            appcharge = appsum,
                                            formfee = formsum,
                                            memberfee = memberfees,
                                            acformfee = acformfees,
                                            totalexp = totalexp,
                                            Voucher = totalvoucher,
                                            branchcode = bbb.BranchCode,
                                            dipositsaving = dipositsaving,
                                            widthdrawsaving = widthdrawsaving,
                                            companyname = cr.CompanyName,
                                            address = cr.Address,
                                            emailid = cr.Emailid,
                                            contact = cr.Contact,
                                            domainname = cr.HeadOffice,
                                            branchname = bb.BranchName
                                        });
                                    }


                                }


                                //------Viewdata for display in page------------------------------------------


                            }
                            else if (dipositsaving > 0 || widthdrawsaving > 0)
                            {
                                dc.Add(new DailyCollection
                                {
                                    CSS_No = 0,
                                    ondate = date,
                                    Account_Code = "",
                                    Name = "",
                                    Rank = "",
                                    Unit_Code = "",
                                    Introducer_Code = "",
                                    Amt_Deposit = 0,
                                    Late_Fee = 0,
                                    Rebate = 0,
                                    Reciept_No = "",
                                    Planname = "",
                                    Term = 0,
                                    appcharge = appsum,
                                    formfee = formsum,
                                    totalexp = totalexp,
                                    acformfee = acformfees,
                                    Voucher = totalvoucher,
                                    branchcode = bbb.BranchCode,
                                    memberfee = 0,
                                    dipositsaving = dipositsaving,
                                    widthdrawsaving = widthdrawsaving,
                                    companyname = cr.CompanyName,
                                    address = cr.Address,
                                    emailid = cr.Emailid,
                                    contact = cr.Contact,
                                    domainname = cr.HeadOffice
                                });
                            }

                            else
                            {
                                Response.Write("<script>alert('There is no collection in this date')</script>");
                            }
                            break;

                        case 2:

                            var bd2 = (from p in db.Installmenttabs where p.paymentdate == date && p.paymentno != 0 && p.opid == pr.OperatorId && ttbd.Contains(p.bondid) select new { p.bondid }).Distinct();
                            if (bd2.Count() > 0)
                            {
                                foreach (var t in bd2.ToList())
                                {

                                    var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                                    var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                                    var irow = db.Installmenttabs.Where(im => im.bondid == t.bondid && im.paymentdate == date && im.payamount != 0 && im.installmentno == 1 && im.type != "Adjustment").ToList();
                                    foreach (var cl in irow.ToList())
                                    {
                                        var finalamount = 0.0;
                                        if (cl.planname.Substring(0, 3) == pst.shortcutname)
                                        {
                                            finalamount = app.totalcon;
                                        }
                                        else
                                        {
                                            finalamount = cl.payamount;
                                        }
                                        inl.Add(new Installment
                                        {
                                            rcptno = cl.receiptno,
                                            latefee = cl.latefine,
                                            relief = cl.reliefrs,
                                            cssno = Convert.ToInt32(cl.cssno),
                                            year = cl.year,
                                            installno = cl.installmentno,
                                            Account_Code = app.newbondid,
                                            Name = app.name,
                                            Introducer_Code = agt.NewAgentId,
                                            Rank = agt.RankName,
                                            amount = finalamount,
                                            Planname = app.planname,
                                            Term = app.term,
                                            branchcode = cl.branch

                                        });
                                    }

                                }

                                foreach (var i in inl.ToList())
                                {
                                    var bb = db.Branchtabs.Single(vv => vv.BranchCode == i.branchcode);
                                    string renew;
                                    if (i.installno == 1)
                                    {
                                        renew = "New";
                                    }
                                    else
                                    {
                                        renew = "Renew";
                                    }
                                    if (renew == "Renew" && i.Planname.Substring(0, 3) == pst.shortcutname)
                                    {

                                    }
                                    else
                                    {
                                        dc.Add(new DailyCollection
                                        {
                                            CSS_No = i.cssno,
                                            ondate = date,
                                            Account_Code = i.Account_Code,
                                            Name = i.Name,
                                            Rank = i.Rank,
                                            Unit_Code = i.Unit_Code,
                                            Introducer_Code = i.Introducer_Code,
                                            Amt_Deposit = i.amount,
                                            Late_Fee = i.latefee,
                                            Rebate = i.relief,
                                            Reciept_No = i.rcptno,
                                            Planname = i.Planname,
                                            Term = i.Term,
                                            New = renew,
                                            appcharge = appsum,
                                            formfee = formsum,
                                            memberfee = memberfees,
                                            acformfee = 0,
                                            totalexp = totalexp,
                                            Voucher = totalvoucher,
                                            branchcode = bbb.BranchCode,
                                            dipositsaving = 0,
                                            widthdrawsaving = 0,
                                            companyname = cr.CompanyName,
                                            address = cr.Address,
                                            emailid = cr.Emailid,
                                            contact = cr.Contact,
                                            domainname = cr.HeadOffice,
                                            branchname = bb.BranchName
                                        });
                                    }


                                }

                            }
                            else
                            {
                                Response.Write("<script>alert('There is no collection in this date')</script>");
                            }
                            break;
                        case 3:
                            if (year > 0)
                            {
                                var bd3 = (from p in db.Installmenttabs where p.paymentdate == date && p.paymentno != 0 && p.opid == pr.OperatorId && ttbd.Contains(p.bondid) select new { p.bondid }).Distinct();
                                if (bd3.Count() > 0)
                                {
                                    foreach (var t in bd3.ToList())
                                    {

                                        var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                                        var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                                        var irow = db.Installmenttabs.Where(im => im.bondid == t.bondid && im.paymentdate == date && im.payamount != 0 && im.installmentno > 1 && im.year == year && im.type != "Adjustment").ToList();
                                        foreach (var cl in irow.ToList())
                                        {
                                            var finalamount = 0.0;
                                            if (cl.planname.Substring(0, 3) == pst.shortcutname)
                                            {
                                                finalamount = app.totalcon;
                                            }
                                            else
                                            {
                                                finalamount = cl.payamount;
                                            }
                                            inl.Add(new Installment
                                            {
                                                rcptno = cl.receiptno,

                                                latefee = cl.latefine,
                                                relief = cl.reliefrs,
                                                cssno = Convert.ToInt32(cl.cssno),
                                                year = cl.year,
                                                installno = cl.installmentno,
                                                Account_Code = app.newbondid,
                                                Name = app.name,
                                                Introducer_Code = agt.NewAgentId,
                                                Rank = agt.RankName,
                                                amount = finalamount,
                                                Planname = app.planname,
                                                Term = app.term,
                                                branchcode = cl.branch

                                            });
                                        }

                                    }

                                    //-------Insertion in Daily Collection Temp Tab--------------------------
                                    foreach (var i in inl.ToList())
                                    {
                                        var bb = db.Branchtabs.Single(vv => vv.BranchCode == i.branchcode);
                                        string renew;
                                        if (i.installno == 1)
                                        {
                                            renew = "New";
                                        }
                                        else
                                        {
                                            renew = "Renew";
                                        }
                                        if (renew == "Renew" && i.Planname.Substring(0, 3) == pst.shortcutname)
                                        {

                                        }
                                        else
                                        {
                                            dc.Add(new DailyCollection
                                            {
                                                CSS_No = i.cssno,
                                                ondate = date,
                                                Account_Code = i.Account_Code,
                                                Name = i.Name,
                                                Rank = i.Rank,
                                                Unit_Code = i.Unit_Code,
                                                Introducer_Code = i.Introducer_Code,
                                                Amt_Deposit = i.amount,
                                                Late_Fee = i.latefee,
                                                Rebate = i.relief,
                                                Reciept_No = i.rcptno,
                                                Planname = i.Planname,
                                                Term = i.Term,
                                                New = renew,
                                                appcharge = 0,
                                                formfee = 0,
                                                memberfee = 0,
                                                acformfee = 0,
                                                totalexp = 0,
                                                Voucher = 0,
                                                dipositsaving = 0,
                                                widthdrawsaving = 0,
                                                branchcode = bbb.BranchCode,
                                                companyname = cr.CompanyName,
                                                address = cr.Address,
                                                emailid = cr.Emailid,
                                                contact = cr.Contact,
                                                domainname = cr.HeadOffice,
                                                branchname = bb.BranchName
                                            });
                                        }


                                    }

                                }
                                else
                                {
                                    Response.Write("<script>alert('There is no collection in this date')</script>");
                                }
                            }
                            else
                            {

                                var bd3 = (from p in db.Installmenttabs where p.paymentdate == date && p.paymentno != 0 && p.opid == pr.OperatorId && ttbd.Contains(p.bondid) select new { p.bondid }).Distinct();
                                if (bd3.Count() > 0)
                                {
                                    foreach (var t in bd3.ToList())
                                    {

                                        var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                                        var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                                        var irow = db.Installmenttabs.Where(im => im.bondid == t.bondid && im.paymentdate == date && im.payamount != 0 && im.installmentno > 1 && im.type != "Adjustment").ToList();
                                        foreach (var cl in irow.ToList())
                                        {
                                            var finalamount = 0.0;
                                            if (cl.planname.Substring(0, 3) == pst.shortcutname)
                                            {
                                                finalamount = app.totalcon;
                                            }
                                            else
                                            {
                                                finalamount = cl.payamount;
                                            }
                                            inl.Add(new Installment
                                            {
                                                rcptno = cl.receiptno,

                                                latefee = cl.latefine,
                                                relief = cl.reliefrs,
                                                cssno = Convert.ToInt32(cl.cssno),
                                                year = cl.year,
                                                installno = cl.installmentno,
                                                Account_Code = app.newbondid,
                                                Name = app.name,
                                                Introducer_Code = agt.NewAgentId,
                                                Rank = agt.RankName,
                                                amount = finalamount,
                                                Planname = app.planname,
                                                Term = app.term,
                                                branchcode = cl.branch

                                            });
                                        }

                                    }

                                    //-------Insertion in Daily Collection Temp Tab--------------------------
                                    foreach (var i in inl.ToList())
                                    {
                                        var bb = db.Branchtabs.Single(vv => vv.BranchCode == i.branchcode);
                                        string renew;
                                        if (i.installno == 1)
                                        {
                                            renew = "New";
                                        }
                                        else
                                        {
                                            renew = "Renew";
                                        }
                                        if (renew == "Renew" && i.Planname.Substring(0, 3) == pst.shortcutname)
                                        {

                                        }
                                        else
                                        {
                                            dc.Add(new DailyCollection
                                            {
                                                CSS_No = i.cssno,
                                                ondate = date,
                                                Account_Code = i.Account_Code,
                                                Name = i.Name,
                                                Rank = i.Rank,
                                                Unit_Code = i.Unit_Code,
                                                Introducer_Code = i.Introducer_Code,
                                                Amt_Deposit = i.amount,
                                                Late_Fee = i.latefee,
                                                Rebate = i.relief,
                                                Reciept_No = i.rcptno,
                                                Planname = i.Planname,
                                                Term = i.Term,
                                                New = renew,
                                                appcharge = 0,
                                                formfee = 0,
                                                memberfee = 0,
                                                acformfee = 0,
                                                totalexp = 0,
                                                Voucher = 0,
                                                dipositsaving = 0,
                                                widthdrawsaving = 0,
                                                branchcode = bbb.BranchCode,
                                                companyname = cr.CompanyName,
                                                address = cr.Address,
                                                emailid = cr.Emailid,
                                                contact = cr.Contact,
                                                domainname = cr.HeadOffice,
                                                branchname = bb.BranchName
                                            });
                                        }

                                    }

                                }
                                else
                                {
                                    Response.Write("<script>alert('There is no collection in this date')</script>");
                                }
                            }
                            break;
                        case 4:

                            if (dipositsaving > 0 || widthdrawsaving > 0)
                            {
                                dc.Add(new DailyCollection
                                {
                                    CSS_No = 0,
                                    ondate = date,
                                    Account_Code = "",
                                    Name = "",
                                    Rank = "",
                                    Unit_Code = "",
                                    Introducer_Code = "",
                                    Amt_Deposit = 0,
                                    Late_Fee = 0,
                                    Rebate = 0,
                                    Reciept_No = "",
                                    Planname = "",
                                    Term = 0,
                                    appcharge = 0,
                                    formfee = 0,
                                    totalexp = 0,
                                    acformfee = acformfees,
                                    Voucher = 0,
                                    branchcode = bbb.BranchCode,
                                    memberfee = 0,
                                    dipositsaving = dipositsaving,
                                    widthdrawsaving = widthdrawsaving,
                                    companyname = cr.CompanyName,
                                    address = cr.Address,
                                    emailid = cr.Emailid,
                                    contact = cr.Contact,
                                    domainname = cr.HeadOffice
                                });
                            }

                            else
                            {
                                Response.Write("<script>alert('There is no collection in this date')</script>");
                            }

                            break;
                    }
                }

                ViewData["ondate"] = date;
                pdate = date;
                NewAgentid = newagentid;
                vvyear = year;
                sfilter = filter;
                return View(dc);
            }
        }
        public ActionResult PrintDailyCollection()
        {
            var cr = db.CompanyInfos.Single(c => c.Id == 1);
            var pr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
            var bbb = db.Branchtabs.Single(bc => bc.BranchCode == pr.BranchCode);
            ViewData["Branch"] = bbb.BranchCode;
            List<DailyCollection> dc = new List<DailyCollection>();
            List<Installment> inl = new List<Installment>();
            List<Memberfees> mfeeslist = new List<Memberfees>();
            Double formsum = 0, appsum = 0, totalexp = 0, memberfees = 0, acformfees = 0, totalvoucher = 0, dipositsaving = 0, widthdrawsaving = 0;
            Double formsumB = 0, appsumB = 0, memberfeesB = 0;
            var pst = db.Plans.Single(c => c.Plancode == 4);

            var ttbd = (from ap in db.appltabs where ap.status == 1 select ap.bondid);
            dipositsaving = (from j in db.TransactionTabs where j.pdate == pdate && j.opid == pr.OperatorId select j.credit).DefaultIfEmpty(0).Sum();
            widthdrawsaving = (from j in db.TransactionTabs where j.pdate == pdate && j.opid == pr.OperatorId select j.debit).DefaultIfEmpty(0).Sum();
            totalvoucher = (from ag in db.Voucher_Reports where ag.date == pdate && ag.Operatorid == pr.OperatorId select ag.netamount).DefaultIfEmpty(0).Sum();
            totalexp = (from ag in db.Expenses where ag.date_time == pdate && ag.opid == pr.OperatorId select ag.amount).DefaultIfEmpty(0).Sum();
            appsum = (from b in db.appltabs where b.formdate == pdate && b.opid == pr.OperatorId && b.status == 1 select b.applicationcharge).DefaultIfEmpty(0).Sum();
            formsum = (from ag in db.AgentDetails where ag.Doj == pdate && ag.operatorid == pr.OperatorId select ag.formfee).DefaultIfEmpty(0).Sum();
            memberfees = (from ag in db.Member_tabs where ag.Cdate == pdate && ag.Opid == pr.OperatorId select ag.Fee).DefaultIfEmpty(0).Sum();
            acformfees = (from ag in db.SavingAccountInfos where ag.Doj == pdate && ag.opid == pr.OperatorId select ag.formfee).DefaultIfEmpty(0).Sum();

            if (NewAgentid != null && NewAgentid != "")
            {
                switch (sfilter)
                {
                    case 1:
                        Double fee = 0;
                        var tbd = (from ap in db.appltabs where ap.newintroducerid == NewAgentid select ap.bondid);
                        var bd = (from p in db.Installmenttabs where p.paymentdate == pdate && p.paymentno != 0 && tbd.Contains(p.bondid) && p.opid == pr.OperatorId select new { p.bondid }).Distinct();
                        appsumB = (from ag in db.appltabs where ag.formdate == pdate && ag.newintroducerid == NewAgentid && ag.opid == pr.OperatorId && ag.status == 1 select ag.applicationcharge).DefaultIfEmpty(0).Sum();
                        formsumB = (from ag in db.AgentDetails where ag.Doj == pdate && ag.NewAgentId == NewAgentid && ag.operatorid == pr.OperatorId select ag.formfee).DefaultIfEmpty(0).Sum();
                        var agent = db.appltabs.Where(s => s.formdate == pdate && s.newintroducerid == NewAgentid && s.opid == pr.OperatorId && s.status == 1).ToList();
                        var agent1 = db.AgentDetails.Where(s => s.Doj == pdate && s.NewAgentId == NewAgentid && s.operatorid == pr.OperatorId).ToList();
                        foreach (var a in agent)
                        {

                            mfeeslist.Add(new Memberfees { newmemberid = a.newmemberid });
                        }
                        foreach (var b in agent1)
                        {
                            mfeeslist.Add(new Memberfees { newmemberid = b.newmemberid });
                        }
                        var result = (from m in mfeeslist select new { m.newmemberid }).Distinct().ToList();
                        foreach (var mf in result)
                        {
                            var mem1 = db.Member_tabs.Single(s => s.NewMemberId == mf.newmemberid);
                            if (mem1.Cdate == pdate)
                            {

                                fee = mem1.Fee + fee;
                            }
                        }
                        memberfeesB = fee;
                        if (bd.Count() > 0)
                        {
                            foreach (var t in bd.ToList())
                            {

                                var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                                var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                                var irow = db.Installmenttabs.Where(im => im.bondid == t.bondid && im.paymentdate == pdate && im.payamount != 0 && im.type != "Adjustment").ToList();
                                foreach (var cl in irow.ToList())
                                {
                                    var finalamount = 0.0;
                                    if (cl.planname.Substring(0, 3) == pst.shortcutname)
                                    {
                                        finalamount = app.totalcon;
                                    }
                                    else
                                    {
                                        finalamount = cl.payamount;
                                    }
                                    inl.Add(new Installment
                                    {
                                        rcptno = cl.receiptno,

                                        latefee = cl.latefine,
                                        relief = cl.reliefrs,
                                        cssno = Convert.ToInt32(cl.cssno),
                                        year = cl.year,
                                        installno = cl.installmentno,
                                        Account_Code = app.newbondid,
                                        Name = app.name,
                                        Introducer_Code = agt.NewAgentId,
                                        Rank = agt.RankName,
                                        amount = finalamount,
                                        Planname = app.planname,
                                        Term = app.term,
                                        branchcode = cl.branch

                                    });
                                }

                            }

                            //-------Insertion in Daily Collection Temp Tab--------------------------
                            foreach (var i in inl.ToList())
                            {
                                var bb = db.Branchtabs.Single(vv => vv.BranchCode == i.branchcode);
                                string renew;
                                if (i.installno == 1)
                                {
                                    renew = "New";
                                }
                                else
                                {
                                    renew = "Renew";
                                }
                                if (renew == "Renew" && i.Planname.Substring(0, 3) == pst.shortcutname)
                                {

                                }
                                else
                                {
                                    dc.Add(new DailyCollection
                                    {
                                        CSS_No = i.cssno,
                                        ondate = pdate,
                                        Account_Code = i.Account_Code,
                                        Name = i.Name,
                                        Rank = i.Rank,
                                        Unit_Code = i.Unit_Code,
                                        Introducer_Code = i.Introducer_Code,
                                        Amt_Deposit = i.amount,
                                        Late_Fee = i.latefee,
                                        Rebate = i.relief,
                                        Reciept_No = i.rcptno,
                                        Planname = i.Planname,
                                        Term = i.Term,
                                        New = renew,
                                        appcharge = appsumB,
                                        formfee = formsumB,
                                        memberfee = memberfeesB,
                                        acformfee = 0,
                                        totalexp = totalexp,
                                        Voucher = totalvoucher,
                                        branchcode = bbb.BranchCode,
                                        dipositsaving = 0,
                                        widthdrawsaving = 0,
                                        companyname = cr.CompanyName,
                                        address = cr.Address,
                                        emailid = cr.Emailid,
                                        contact = cr.Contact,
                                        domainname = cr.HeadOffice,
                                        branchname = bb.BranchName
                                    });
                                }


                            }


                            //------Viewdata for display in page------------------------------------------


                        }
                        else if (dipositsaving > 0 || widthdrawsaving > 0)
                        {
                            dc.Add(new DailyCollection
                            {
                                CSS_No = 0,
                                ondate = pdate,
                                Account_Code = "",
                                Name = "",
                                Rank = "",
                                Unit_Code = "",
                                Introducer_Code = "",
                                Amt_Deposit = 0,
                                Late_Fee = 0,
                                Rebate = 0,
                                Reciept_No = "",
                                Planname = "",
                                Term = 0,
                                appcharge = appsum,
                                formfee = formsum,
                                totalexp = totalexp,
                                acformfee = 0,
                                Voucher = totalvoucher,
                                branchcode = bbb.BranchCode,
                                memberfee = 0,
                                dipositsaving = 0,
                                widthdrawsaving = 0,
                                companyname = cr.CompanyName,
                                address = cr.Address,
                                emailid = cr.Emailid,
                                contact = cr.Contact,
                                domainname = cr.HeadOffice
                            });
                        }

                        else
                        {
                            Response.Write("<script>alert('There is no collection in this date')</script>");
                        }
                        break;

                    case 2:
                        Double fee1 = 0;
                        var tbd2 = (from ap in db.appltabs where ap.newintroducerid == NewAgentid select ap.bondid);
                        appsumB = (from ag in db.appltabs where ag.formdate == pdate && ag.newintroducerid == NewAgentid && ag.opid == pr.OperatorId && ag.status == 1 select ag.applicationcharge).DefaultIfEmpty(0).Sum();
                        formsumB = (from ag in db.AgentDetails where ag.Doj == pdate && ag.NewAgentId == NewAgentid && ag.operatorid == pr.OperatorId select ag.formfee).DefaultIfEmpty(0).Sum();
                        var agentB = db.appltabs.Where(s => s.formdate == pdate && s.newintroducerid == NewAgentid && s.opid == pr.OperatorId && s.status == 1).ToList();
                        var agentB1 = db.AgentDetails.Where(s => s.Doj == pdate && s.NewAgentId == NewAgentid && s.operatorid == pr.OperatorId).ToList();
                        foreach (var a in agentB)
                        {

                            mfeeslist.Add(new Memberfees { newmemberid = a.newmemberid });
                        }
                        foreach (var b in agentB1)
                        {
                            mfeeslist.Add(new Memberfees { newmemberid = b.newmemberid });
                        }
                        var resultB = (from m in mfeeslist select new { m.newmemberid }).Distinct().ToList();
                        foreach (var mf in resultB)
                        {
                            var mem1 = db.Member_tabs.Single(s => s.NewMemberId == mf.newmemberid);
                            if (mem1.Cdate == pdate)
                            {

                                fee1 = mem1.Fee + fee1;
                            }
                        }
                        memberfeesB = fee1;
                        var bd2 = (from p in db.Installmenttabs where p.paymentdate == pdate && p.paymentno != 0 && tbd2.Contains(p.bondid) && p.opid == pr.OperatorId select new { p.bondid }).Distinct();
                        if (bd2.Count() > 0)
                        {
                            foreach (var t in bd2.ToList())
                            {

                                var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                                var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                                var irow = db.Installmenttabs.Where(im => im.bondid == t.bondid && im.paymentdate == pdate && im.payamount != 0 && im.installmentno == 1 && im.type != "Adjustment").ToList();
                                foreach (var cl in irow.ToList())
                                {
                                    var finalamount = 0.0;
                                    if (cl.planname.Substring(0, 3) == pst.shortcutname)
                                    {
                                        finalamount = app.totalcon;
                                    }
                                    else
                                    {
                                        finalamount = cl.payamount;
                                    }
                                    inl.Add(new Installment
                                    {
                                        rcptno = cl.receiptno,
                                        latefee = cl.latefine,
                                        relief = cl.reliefrs,
                                        cssno = Convert.ToInt32(cl.cssno),
                                        year = cl.year,
                                        installno = cl.installmentno,
                                        Account_Code = app.newbondid,
                                        Name = app.name,
                                        Introducer_Code = agt.NewAgentId,
                                        Rank = agt.RankName,
                                        amount = finalamount,
                                        Planname = app.planname,
                                        Term = app.term,
                                        branchcode = cl.branch

                                    });
                                }

                            }

                            foreach (var i in inl.ToList())
                            {
                                var bb = db.Branchtabs.Single(vv => vv.BranchCode == i.branchcode);
                                string renew;
                                if (i.installno == 1)
                                {
                                    renew = "New";
                                }
                                else
                                {
                                    renew = "Renew";
                                }
                                if (renew == "Renew" && i.Planname.Substring(0, 3) == pst.shortcutname)
                                {

                                }
                                else
                                {
                                    dc.Add(new DailyCollection
                                    {
                                        CSS_No = i.cssno,
                                        ondate = pdate,
                                        Account_Code = i.Account_Code,
                                        Name = i.Name,
                                        Rank = i.Rank,
                                        Unit_Code = i.Unit_Code,
                                        Introducer_Code = i.Introducer_Code,
                                        Amt_Deposit = i.amount,
                                        Late_Fee = i.latefee,
                                        Rebate = i.relief,
                                        Reciept_No = i.rcptno,
                                        Planname = i.Planname,
                                        Term = i.Term,
                                        New = renew,
                                        appcharge = appsumB,
                                        formfee = formsumB,
                                        memberfee = memberfeesB,
                                        acformfee = 0,
                                        totalexp = totalexp,
                                        Voucher = totalvoucher,
                                        branchcode = bbb.BranchCode,
                                        dipositsaving = 0,
                                        widthdrawsaving = 0,
                                        companyname = cr.CompanyName,
                                        address = cr.Address,
                                        emailid = cr.Emailid,
                                        contact = cr.Contact,
                                        domainname = cr.HeadOffice,
                                        branchname = bb.BranchName
                                    });
                                }


                            }

                        }
                        else
                        {
                            Response.Write("<script>alert('There is no collection in this date')</script>");
                        }
                        break;
                    case 3:
                        if (vvyear > 0)
                        {
                            var tbd3 = (from ap in db.appltabs where ap.newintroducerid == NewAgentid select ap.bondid);
                            var bd3 = (from p in db.Installmenttabs where p.paymentdate == pdate && p.paymentno != 0 && tbd3.Contains(p.bondid) && p.opid == pr.OperatorId select new { p.bondid }).Distinct();
                            if (bd3.Count() > 0)
                            {
                                foreach (var t in bd3.ToList())
                                {

                                    var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                                    var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                                    var irow = db.Installmenttabs.Where(im => im.bondid == t.bondid && im.paymentdate == pdate && im.payamount != 0 && im.installmentno > 1 && im.year == vvyear && im.type != "Adjustment").ToList();
                                    foreach (var cl in irow.ToList())
                                    {
                                        var finalamount = 0.0;
                                        if (cl.planname.Substring(0, 3) == pst.shortcutname)
                                        {
                                            finalamount = app.totalcon;
                                        }
                                        else
                                        {
                                            finalamount = cl.payamount;
                                        }
                                        inl.Add(new Installment
                                        {
                                            rcptno = cl.receiptno,

                                            latefee = cl.latefine,
                                            relief = cl.reliefrs,
                                            cssno = Convert.ToInt32(cl.cssno),
                                            year = cl.year,
                                            installno = cl.installmentno,
                                            Account_Code = app.newbondid,
                                            Name = app.name,
                                            Introducer_Code = agt.NewAgentId,
                                            Rank = agt.RankName,
                                            amount = finalamount,
                                            Planname = app.planname,
                                            Term = app.term,
                                            branchcode = cl.branch

                                        });
                                    }

                                }

                                //-------Insertion in Daily Collection Temp Tab--------------------------
                                foreach (var i in inl.ToList())
                                {
                                    var bb = db.Branchtabs.Single(vv => vv.BranchCode == i.branchcode);
                                    string renew;
                                    if (i.installno == 1)
                                    {
                                        renew = "New";
                                    }
                                    else
                                    {
                                        renew = "Renew";
                                    }
                                    if (renew == "Renew" && i.Planname.Substring(0, 3) == pst.shortcutname)
                                    {

                                    }
                                    else
                                    {
                                        dc.Add(new DailyCollection
                                        {
                                            CSS_No = i.cssno,
                                            ondate = pdate,
                                            Account_Code = i.Account_Code,
                                            Name = i.Name,
                                            Rank = i.Rank,
                                            Unit_Code = i.Unit_Code,
                                            Introducer_Code = i.Introducer_Code,
                                            Amt_Deposit = i.amount,
                                            Late_Fee = i.latefee,
                                            Rebate = i.relief,
                                            Reciept_No = i.rcptno,
                                            Planname = i.Planname,
                                            Term = i.Term,
                                            New = renew,
                                            appcharge = 0,
                                            formfee = 0,
                                            memberfee = 0,
                                            acformfee = 0,
                                            totalexp = 0,
                                            Voucher = 0,
                                            dipositsaving = 0,
                                            widthdrawsaving = 0,
                                            branchcode = bbb.BranchCode,
                                            companyname = cr.CompanyName,
                                            address = cr.Address,
                                            emailid = cr.Emailid,
                                            contact = cr.Contact,
                                            domainname = cr.HeadOffice,
                                            branchname = bb.BranchName
                                        });
                                    }


                                }

                            }
                            else
                            {
                                Response.Write("<script>alert('There is no collection in this date')</script>");
                            }
                        }
                        else
                        {
                            var tbd3 = (from ap in db.appltabs where ap.newintroducerid == NewAgentid select ap.bondid);
                            var bd3 = (from p in db.Installmenttabs where p.paymentdate == pdate && p.paymentno != 0 && tbd3.Contains(p.bondid) && p.opid == pr.OperatorId select new { p.bondid }).Distinct();
                            if (bd3.Count() > 0)
                            {
                                foreach (var t in bd3.ToList())
                                {

                                    var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                                    var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                                    var irow = db.Installmenttabs.Where(im => im.bondid == t.bondid && im.paymentdate == pdate && im.payamount != 0 && im.installmentno > 1 && im.type != "Adjustment").ToList();
                                    foreach (var cl in irow.ToList())
                                    {
                                        var finalamount = 0.0;
                                        if (cl.planname.Substring(0, 3) == pst.shortcutname)
                                        {
                                            finalamount = app.totalcon;
                                        }
                                        else
                                        {
                                            finalamount = cl.payamount;
                                        }
                                        inl.Add(new Installment
                                        {
                                            rcptno = cl.receiptno,

                                            latefee = cl.latefine,
                                            relief = cl.reliefrs,
                                            cssno = Convert.ToInt32(cl.cssno),
                                            year = cl.year,
                                            installno = cl.installmentno,
                                            Account_Code = app.newbondid,
                                            Name = app.name,
                                            Introducer_Code = agt.NewAgentId,
                                            Rank = agt.RankName,
                                            amount = finalamount,
                                            Planname = app.planname,
                                            Term = app.term,
                                            branchcode = cl.branch

                                        });
                                    }

                                }

                                //-------Insertion in Daily Collection Temp Tab--------------------------
                                foreach (var i in inl.ToList())
                                {
                                    var bb = db.Branchtabs.Single(vv => vv.BranchCode == i.branchcode);
                                    string renew;
                                    if (i.installno == 1)
                                    {
                                        renew = "New";
                                    }
                                    else
                                    {
                                        renew = "Renew";
                                    }
                                    if (renew == "Renew" && i.Planname.Substring(0, 3) == pst.shortcutname)
                                    {

                                    }
                                    else
                                    {
                                        dc.Add(new DailyCollection
                                        {
                                            CSS_No = i.cssno,
                                            ondate = pdate,
                                            Account_Code = i.Account_Code,
                                            Name = i.Name,
                                            Rank = i.Rank,
                                            Unit_Code = i.Unit_Code,
                                            Introducer_Code = i.Introducer_Code,
                                            Amt_Deposit = i.amount,
                                            Late_Fee = i.latefee,
                                            Rebate = i.relief,
                                            Reciept_No = i.rcptno,
                                            Planname = i.Planname,
                                            Term = i.Term,
                                            New = renew,
                                            appcharge = 0,
                                            formfee = 0,
                                            memberfee = 0,
                                            acformfee = 0,
                                            totalexp = 0,
                                            Voucher = 0,
                                            dipositsaving = 0,
                                            widthdrawsaving = 0,
                                            branchcode = bbb.BranchCode,
                                            companyname = cr.CompanyName,
                                            address = cr.Address,
                                            emailid = cr.Emailid,
                                            contact = cr.Contact,
                                            domainname = cr.HeadOffice,
                                            branchname = bb.BranchName
                                        });
                                    }


                                }

                            }
                            else
                            {
                                Response.Write("<script>alert('There is no collection in this date')</script>");
                            }
                        }
                        break;
                    case 4:

                        if (dipositsaving > 0 || widthdrawsaving > 0)
                        {
                            dc.Add(new DailyCollection
                            {
                                CSS_No = 0,
                                ondate = pdate,
                                Account_Code = "",
                                Name = "",
                                Rank = "",
                                Unit_Code = "",
                                Introducer_Code = "",
                                Amt_Deposit = 0,
                                Late_Fee = 0,
                                Rebate = 0,
                                Reciept_No = "",
                                Planname = "",
                                Term = 0,
                                appcharge = 0,
                                formfee = 0,
                                totalexp = 0,
                                acformfee = 0,
                                Voucher = 0,
                                branchcode = bbb.BranchCode,
                                memberfee = 0,
                                dipositsaving = 0,
                                widthdrawsaving = 0,
                                companyname = cr.CompanyName,
                                address = cr.Address,
                                emailid = cr.Emailid,
                                contact = cr.Contact,
                                domainname = cr.HeadOffice
                            });
                        }

                        else
                        {
                            Response.Write("<script>alert('There is no collection in this date')</script>");
                        }

                        break;
                }
            }
            else if (NewAgentid == null || NewAgentid == "")
            {
                switch (sfilter)
                {
                    case 1:

                        var bd = (from p in db.Installmenttabs where p.paymentdate == pdate && p.paymentno != 0 && p.opid == pr.OperatorId && ttbd.Contains(p.bondid) select new { p.bondid }).Distinct();
                        if (bd.Count() > 0)
                        {
                            foreach (var t in bd.ToList())
                            {

                                var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                                var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                                var irow = db.Installmenttabs.Where(im => im.bondid == t.bondid && im.paymentdate == pdate && im.payamount != 0 && im.type != "Adjustment").ToList();
                                foreach (var cl in irow.ToList())
                                {
                                    var finalamount = 0.0;
                                    if (cl.planname.Substring(0, 3) == pst.shortcutname)
                                    {
                                        finalamount = app.totalcon;
                                    }
                                    else
                                    {
                                        finalamount = cl.payamount;
                                    }
                                    inl.Add(new Installment
                                    {
                                        rcptno = cl.receiptno,

                                        latefee = cl.latefine,
                                        relief = cl.reliefrs,
                                        cssno = Convert.ToInt32(cl.cssno),
                                        year = cl.year,
                                        installno = cl.installmentno,
                                        Account_Code = app.newbondid,
                                        Name = app.name,
                                        Introducer_Code = agt.NewAgentId,
                                        Rank = agt.RankName,
                                        amount = finalamount,
                                        Planname = app.planname,
                                        Term = app.term,
                                        branchcode = cl.branch

                                    });
                                }

                            }

                            //-------Insertion in Daily Collection Temp Tab--------------------------
                            foreach (var i in inl.ToList())
                            {
                                var bb = db.Branchtabs.Single(vv => vv.BranchCode == i.branchcode);
                                string renew;
                                if (i.installno == 1)
                                {
                                    renew = "New";
                                }
                                else
                                {
                                    renew = "Renew";
                                }
                                if (renew == "Renew" && i.Planname.Substring(0, 3) == pst.shortcutname)
                                {

                                }
                                else
                                {
                                    dc.Add(new DailyCollection
                                    {
                                        CSS_No = i.cssno,
                                        ondate = pdate,
                                        Account_Code = i.Account_Code,
                                        Name = i.Name,
                                        Rank = i.Rank,
                                        Unit_Code = i.Unit_Code,
                                        Introducer_Code = i.Introducer_Code,
                                        Amt_Deposit = i.amount,
                                        Late_Fee = i.latefee,
                                        Rebate = i.relief,
                                        Reciept_No = i.rcptno,
                                        Planname = i.Planname,
                                        Term = i.Term,
                                        New = renew,
                                        appcharge = appsum,
                                        formfee = formsum,
                                        memberfee = memberfees,
                                        acformfee = acformfees,
                                        totalexp = totalexp,
                                        Voucher = totalvoucher,
                                        branchcode = bbb.BranchCode,
                                        dipositsaving = dipositsaving,
                                        widthdrawsaving = widthdrawsaving,
                                        companyname = cr.CompanyName,
                                        address = cr.Address,
                                        emailid = cr.Emailid,
                                        contact = cr.Contact,
                                        domainname = cr.HeadOffice,
                                        branchname = bb.BranchName
                                    });
                                }


                            }


                            //------Viewdata for display in page------------------------------------------


                        }
                        else if (dipositsaving > 0 || widthdrawsaving > 0)
                        {
                            dc.Add(new DailyCollection
                            {
                                CSS_No = 0,
                                ondate = pdate,
                                Account_Code = "",
                                Name = "",
                                Rank = "",
                                Unit_Code = "",
                                Introducer_Code = "",
                                Amt_Deposit = 0,
                                Late_Fee = 0,
                                Rebate = 0,
                                Reciept_No = "",
                                Planname = "",
                                Term = 0,
                                appcharge = appsum,
                                formfee = formsum,
                                totalexp = totalexp,
                                acformfee = acformfees,
                                Voucher = totalvoucher,
                                branchcode = bbb.BranchCode,
                                memberfee = 0,
                                dipositsaving = dipositsaving,
                                widthdrawsaving = widthdrawsaving,
                                companyname = cr.CompanyName,
                                address = cr.Address,
                                emailid = cr.Emailid,
                                contact = cr.Contact,
                                domainname = cr.HeadOffice
                            });
                        }

                        else
                        {
                            Response.Write("<script>alert('There is no collection in this date')</script>");
                        }
                        break;

                    case 2:

                        var bd2 = (from p in db.Installmenttabs where p.paymentdate == pdate && p.paymentno != 0 && p.opid == pr.OperatorId && ttbd.Contains(p.bondid) select new { p.bondid }).Distinct();
                        if (bd2.Count() > 0)
                        {
                            foreach (var t in bd2.ToList())
                            {

                                var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                                var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                                var irow = db.Installmenttabs.Where(im => im.bondid == t.bondid && im.paymentdate == pdate && im.payamount != 0 && im.installmentno == 1 && im.type != "Adjustment").ToList();
                                foreach (var cl in irow.ToList())
                                {
                                    var finalamount = 0.0;
                                    if (cl.planname.Substring(0, 3) == pst.shortcutname)
                                    {
                                        finalamount = app.totalcon;
                                    }
                                    else
                                    {
                                        finalamount = cl.payamount;
                                    }
                                    inl.Add(new Installment
                                    {
                                        rcptno = cl.receiptno,
                                        latefee = cl.latefine,
                                        relief = cl.reliefrs,
                                        cssno = Convert.ToInt32(cl.cssno),
                                        year = cl.year,
                                        installno = cl.installmentno,
                                        Account_Code = app.newbondid,
                                        Name = app.name,
                                        Introducer_Code = agt.NewAgentId,
                                        Rank = agt.RankName,
                                        amount = finalamount,
                                        Planname = app.planname,
                                        Term = app.term,
                                        branchcode = cl.branch

                                    });
                                }

                            }

                            foreach (var i in inl.ToList())
                            {
                                var bb = db.Branchtabs.Single(vv => vv.BranchCode == i.branchcode);
                                string renew;
                                if (i.installno == 1)
                                {
                                    renew = "New";
                                }
                                else
                                {
                                    renew = "Renew";
                                }
                                if (renew == "Renew" && i.Planname.Substring(0, 3) == pst.shortcutname)
                                {

                                }
                                else
                                {
                                    dc.Add(new DailyCollection
                                    {
                                        CSS_No = i.cssno,
                                        ondate = pdate,
                                        Account_Code = i.Account_Code,
                                        Name = i.Name,
                                        Rank = i.Rank,
                                        Unit_Code = i.Unit_Code,
                                        Introducer_Code = i.Introducer_Code,
                                        Amt_Deposit = i.amount,
                                        Late_Fee = i.latefee,
                                        Rebate = i.relief,
                                        Reciept_No = i.rcptno,
                                        Planname = i.Planname,
                                        Term = i.Term,
                                        New = renew,
                                        appcharge = appsum,
                                        formfee = formsum,
                                        memberfee = memberfees,
                                        acformfee = 0,
                                        totalexp = totalexp,
                                        Voucher = totalvoucher,
                                        branchcode = bbb.BranchCode,
                                        dipositsaving = 0,
                                        widthdrawsaving = 0,
                                        companyname = cr.CompanyName,
                                        address = cr.Address,
                                        emailid = cr.Emailid,
                                        contact = cr.Contact,
                                        domainname = cr.HeadOffice,
                                        branchname = bb.BranchName
                                    });
                                }


                            }

                        }
                        else
                        {
                            Response.Write("<script>alert('There is no collection in this date')</script>");
                        }
                        break;
                    case 3:
                        if (vvyear > 0)
                        {
                            var bd3 = (from p in db.Installmenttabs where p.paymentdate == pdate && p.paymentno != 0 && p.opid == pr.OperatorId && ttbd.Contains(p.bondid) select new { p.bondid }).Distinct();
                            if (bd3.Count() > 0)
                            {
                                foreach (var t in bd3.ToList())
                                {

                                    var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                                    var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                                    var irow = db.Installmenttabs.Where(im => im.bondid == t.bondid && im.paymentdate == pdate && im.payamount != 0 && im.installmentno > 1 && im.year == vvyear && im.type != "Adjustment").ToList();
                                    foreach (var cl in irow.ToList())
                                    {
                                        var finalamount = 0.0;
                                        if (cl.planname.Substring(0, 3) == pst.shortcutname)
                                        {
                                            finalamount = app.totalcon;
                                        }
                                        else
                                        {
                                            finalamount = cl.payamount;
                                        }
                                        inl.Add(new Installment
                                        {
                                            rcptno = cl.receiptno,

                                            latefee = cl.latefine,
                                            relief = cl.reliefrs,
                                            cssno = Convert.ToInt32(cl.cssno),
                                            year = cl.year,
                                            installno = cl.installmentno,
                                            Account_Code = app.newbondid,
                                            Name = app.name,
                                            Introducer_Code = agt.NewAgentId,
                                            Rank = agt.RankName,
                                            amount = finalamount,
                                            Planname = app.planname,
                                            Term = app.term,
                                            branchcode = cl.branch

                                        });
                                    }

                                }

                                //-------Insertion in Daily Collection Temp Tab--------------------------
                                foreach (var i in inl.ToList())
                                {
                                    var bb = db.Branchtabs.Single(vv => vv.BranchCode == i.branchcode);
                                    string renew;
                                    if (i.installno == 1)
                                    {
                                        renew = "New";
                                    }
                                    else
                                    {
                                        renew = "Renew";
                                    }
                                    if (renew == "Renew" && i.Planname.Substring(0, 3) == pst.shortcutname)
                                    {

                                    }
                                    else
                                    {
                                        dc.Add(new DailyCollection
                                        {
                                            CSS_No = i.cssno,
                                            ondate = pdate,
                                            Account_Code = i.Account_Code,
                                            Name = i.Name,
                                            Rank = i.Rank,
                                            Unit_Code = i.Unit_Code,
                                            Introducer_Code = i.Introducer_Code,
                                            Amt_Deposit = i.amount,
                                            Late_Fee = i.latefee,
                                            Rebate = i.relief,
                                            Reciept_No = i.rcptno,
                                            Planname = i.Planname,
                                            Term = i.Term,
                                            New = renew,
                                            appcharge = 0,
                                            formfee = 0,
                                            memberfee = 0,
                                            acformfee = 0,
                                            totalexp = 0,
                                            Voucher = 0,
                                            dipositsaving = 0,
                                            widthdrawsaving = 0,
                                            branchcode = bbb.BranchCode,
                                            companyname = cr.CompanyName,
                                            address = cr.Address,
                                            emailid = cr.Emailid,
                                            contact = cr.Contact,
                                            domainname = cr.HeadOffice,
                                            branchname = bb.BranchName
                                        });
                                    }


                                }

                            }
                            else
                            {
                                Response.Write("<script>alert('There is no collection in this date')</script>");
                            }
                        }
                        else
                        {

                            var bd3 = (from p in db.Installmenttabs where p.paymentdate == pdate && p.paymentno != 0 && p.opid == pr.OperatorId && ttbd.Contains(p.bondid) select new { p.bondid }).Distinct();
                            if (bd3.Count() > 0)
                            {
                                foreach (var t in bd3.ToList())
                                {

                                    var app = db.appltabs.Single(ap => ap.bondid == t.bondid);
                                    var agt = db.AgentDetails.Single(adt => adt.NewAgentId == app.newintroducerid);
                                    var irow = db.Installmenttabs.Where(im => im.bondid == t.bondid && im.paymentdate == pdate && im.payamount != 0 && im.installmentno > 1 && im.type != "Adjustment").ToList();
                                    foreach (var cl in irow.ToList())
                                    {
                                        var finalamount = 0.0;
                                        if (cl.planname.Substring(0, 3) == pst.shortcutname)
                                        {
                                            finalamount = app.totalcon;
                                        }
                                        else
                                        {
                                            finalamount = cl.payamount;
                                        }
                                        inl.Add(new Installment
                                        {
                                            rcptno = cl.receiptno,

                                            latefee = cl.latefine,
                                            relief = cl.reliefrs,
                                            cssno = Convert.ToInt32(cl.cssno),
                                            year = cl.year,
                                            installno = cl.installmentno,
                                            Account_Code = app.newbondid,
                                            Name = app.name,
                                            Introducer_Code = agt.NewAgentId,
                                            Rank = agt.RankName,
                                            amount = finalamount,
                                            Planname = app.planname,
                                            Term = app.term,
                                            branchcode = cl.branch

                                        });
                                    }

                                }

                                //-------Insertion in Daily Collection Temp Tab--------------------------
                                foreach (var i in inl.ToList())
                                {
                                    var bb = db.Branchtabs.Single(vv => vv.BranchCode == i.branchcode);
                                    string renew;
                                    if (i.installno == 1)
                                    {
                                        renew = "New";
                                    }
                                    else
                                    {
                                        renew = "Renew";
                                    }
                                    if (renew == "Renew" && i.Planname.Substring(0, 3) == pst.shortcutname)
                                    {

                                    }
                                    else
                                    {
                                        dc.Add(new DailyCollection
                                        {
                                            CSS_No = i.cssno,
                                            ondate = pdate,
                                            Account_Code = i.Account_Code,
                                            Name = i.Name,
                                            Rank = i.Rank,
                                            Unit_Code = i.Unit_Code,
                                            Introducer_Code = i.Introducer_Code,
                                            Amt_Deposit = i.amount,
                                            Late_Fee = i.latefee,
                                            Rebate = i.relief,
                                            Reciept_No = i.rcptno,
                                            Planname = i.Planname,
                                            Term = i.Term,
                                            New = renew,
                                            appcharge = 0,
                                            formfee = 0,
                                            memberfee = 0,
                                            acformfee = 0,
                                            totalexp = 0,
                                            Voucher = 0,
                                            dipositsaving = 0,
                                            widthdrawsaving = 0,
                                            branchcode = bbb.BranchCode,
                                            companyname = cr.CompanyName,
                                            address = cr.Address,
                                            emailid = cr.Emailid,
                                            contact = cr.Contact,
                                            domainname = cr.HeadOffice,
                                            branchname = bb.BranchName
                                        });
                                    }

                                }

                            }
                            else
                            {
                                Response.Write("<script>alert('There is no collection in this date')</script>");
                            }
                        }
                        break;
                    case 4:

                        if (dipositsaving > 0 || widthdrawsaving > 0)
                        {
                            dc.Add(new DailyCollection
                            {
                                CSS_No = 0,
                                ondate = pdate,
                                Account_Code = "",
                                Name = "",
                                Rank = "",
                                Unit_Code = "",
                                Introducer_Code = "",
                                Amt_Deposit = 0,
                                Late_Fee = 0,
                                Rebate = 0,
                                Reciept_No = "",
                                Planname = "",
                                Term = 0,
                                appcharge = 0,
                                formfee = 0,
                                totalexp = 0,
                                acformfee = acformfees,
                                Voucher = 0,
                                branchcode = bbb.BranchCode,
                                memberfee = 0,
                                dipositsaving = dipositsaving,
                                widthdrawsaving = widthdrawsaving,
                                companyname = cr.CompanyName,
                                address = cr.Address,
                                emailid = cr.Emailid,
                                contact = cr.Contact,
                                domainname = cr.HeadOffice
                            });
                        }

                        else
                        {
                            Response.Write("<script>alert('There is no collection in this date')</script>");
                        }

                        break;
                }
            }

            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports/Operator"), "OperatorDailycollection.rpt"));
            rd.SetDataSource(dc);

            Response.Buffer = false;
            Response.ClearContent();
            Response.ClearHeaders();


            try
            {
                Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                stream.Seek(0, SeekOrigin.Begin);
                string dt = pdate.ToShortDateString();

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



            return View(dc);

        }

        [HttpGet]
        public ActionResult DateWiseChequeCollection()
        {
            List<Chequecollection> model = new List<Chequecollection>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                ViewData["sdate"] = string.Empty;
                ViewData["edate"] = string.Empty;
                return View(model);
            }
        }
        [HttpPost]
        public ActionResult DateWiseChequeCollection(DateTime sdate, DateTime edate)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                ViewData["sdate"] = sdate;
                ViewData["edate"] = edate;
                List<Chequecollection> model = new List<Chequecollection>();
                SqlCommand cmd = new SqlCommand();
                con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                cmd.Connection = con;
                SqlDataReader sdr;
                try
                {

                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd = new SqlCommand("chequecollpro", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sdate", sdate);
                    cmd.Parameters.AddWithValue("@edate", edate);
                    cmd.Parameters.AddWithValue("@opid", User.Identity.Name);


                    con.Open();
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        var rs = new Chequecollection();
                        rs.newbondid = sdr["newbondid"].ToString();
                        rs.name = sdr["name"].ToString();
                        rs.bank = sdr["bank"].ToString();
                        rs.chequeno = sdr["chequeno"].ToString();
                        rs.pdate = Convert.ToDateTime(sdr["paydate"]);
                        rs.amount = Convert.ToDouble(sdr["amount"]);
                        rs.sdate = Convert.ToDateTime(sdr["sdate"]);
                        rs.edate = Convert.ToDateTime(sdr["edate"]);
                        rs.opid = sdr["opid"].ToString();
                        model.Add(rs);
                    }


                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "chequecoll.rpt"));
                    rd.SetDataSource(model);
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

                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('" + ex.Message + "')</script>");
                }
                finally
                {
                    con.Close();
                }
                return View();
            }
        }

        [HttpGet]
        public ActionResult DateWiseCollection()
        {
            List<OpDatewisecollection> dwclist = new List<OpDatewisecollection>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                ViewData["opid"] = string.Empty;
                ViewData["sdate"] = string.Empty;
                ViewData["edate"] = string.Empty;
                return View(dwclist);
            }
        }
        [HttpPost]
        public ActionResult DateWiseCollection(DateTime sdate, DateTime edate)
        {
            List<Memberfees> mfeeslist = new List<Memberfees>();
            List<OpDatewisecollection> dwclist = new List<OpDatewisecollection>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                ViewData["opid"] = User.Identity.Name; ;
                ViewData["sdate"] = sdate;
                ViewData["edate"] = edate;


                DateTime fsdate = sdate;
                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(p => p.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);
                var pst = db.Plans.Single(c => c.Plancode == 4);

                var tbd = (from ap in db.appltabs where ap.status == 1 select ap.bondid);
                Double tsis = 0, tMISFresh = 0, tMIS1 = 0, tMIS2 = 0, tMIS3 = 0, tMIS4 = 0, tMIS5 = 0, tMIS6 = 0, tMIS7a = 0, expense = 0, totalvoucher = 0, agfee = 0, appfee = 0, memberfee = 0, accfee = 0, deposit = 0, widthdraw = 0, pps = 0, late = 0, rebate = 0;
                while (sdate <= edate)
                {
                    tMISFresh = (from o in db.Installmenttabs where o.paymentdate == sdate && o.installmentno == 1 && o.mode != "Single" && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tsis = (from o in db.Installmenttabs where o.paymentdate == sdate && o.installmentno == 1 && o.mode == "Single" && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS1 = (from o in db.Installmenttabs where o.paymentdate == sdate && o.installmentno != 1 && o.year == 1 && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS2 = (from o in db.Installmenttabs where o.paymentdate == sdate && o.installmentno != 1 && o.year == 2 && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS3 = (from o in db.Installmenttabs where o.paymentdate == sdate && o.installmentno != 1 && o.year == 3 && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS4 = (from o in db.Installmenttabs where o.paymentdate == sdate && o.installmentno != 1 && o.year == 4 && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS5 = (from o in db.Installmenttabs where o.paymentdate == sdate && o.installmentno != 1 && o.year == 5 && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS6 = (from o in db.Installmenttabs where o.paymentdate == sdate && o.installmentno != 1 && o.year == 6 && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS7a = (from o in db.Installmenttabs where o.paymentdate == sdate && o.installmentno != 1 && o.year >= 7 && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    late = (from o in db.Installmenttabs where o.paymentdate == sdate && o.payamount != 0 && o.mode != "Single" && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.latefine).DefaultIfEmpty(0).Sum();
                    rebate = (from o in db.Installmenttabs where o.paymentdate == sdate && o.payamount != 0 && o.mode != "Single" && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.reliefrs).DefaultIfEmpty(0).Sum();

                    deposit = (from o in db.appltabs where o.pdate == sdate && o.branchcode == br.BranchCode && tbd.Contains(o.bondid) select o.downpayment).DefaultIfEmpty(0).Sum();
                    memberfee = (from ag in db.Member_tabs where ag.Cdate == sdate && ag.BranchCode == br.BranchCode select ag.Fee).DefaultIfEmpty(0).Sum();
                    appfee = (from ag in db.appltabs where ag.formdate == sdate && ag.status == 1 && ag.branchcode == br.BranchCode select ag.applicationcharge).DefaultIfEmpty(0).Sum();
                    agfee = (from ag in db.AgentDetails where ag.Doj == sdate && ag.BranchCode == br.BranchCode select ag.formfee).DefaultIfEmpty(0).Sum();
                    accfee = (from ag in db.SavingAccountInfos where ag.Doj == sdate && ag.branchcode == br.BranchCode select ag.formfee).DefaultIfEmpty(0).Sum();
                    widthdraw = (from o in db.spotcommission_tabs where o.date == sdate && o.branchcode == br.BranchCode select o.commission).DefaultIfEmpty(0).Sum();
                    totalvoucher = (from o in db.Voucher_Reports where o.date == sdate && o.branchcode == br.BranchCode select o.netamount).DefaultIfEmpty(0).Sum();
                    expense = (from ag in db.Expenses where ag.date_time == sdate && ag.branchcode == br.BranchCode select ag.amount).DefaultIfEmpty(0).Sum();

                    if (tsis + tMISFresh + tMIS1 + tMIS2 + tMIS3 + tMIS4 + tMIS5 + tMIS6 + tMIS7a + expense + totalvoucher + agfee + appfee + memberfee + accfee + deposit + widthdraw + pps + late + rebate > 0)
                    {
                        dwclist.Add(new OpDatewisecollection { date = sdate, tsis = tsis, tMISFresh = tMISFresh, tMIS1 = tMIS1, tMIS2 = tMIS2, tMIS3 = tMIS3, tMIS4 = tMIS4, tMIS5 = tMIS5, tMIS6 = tMIS6, tMIS7a = tMIS7a, Expense = expense, Voucher = totalvoucher, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, sdate = fsdate, edate = edate, Appcharge = appfee, Agencyfee = agfee, Accountfee = accfee, Memberfee = memberfee, deposit = deposit, widthdraw = widthdraw, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice, pps = pps, late = late, releaf = rebate });
                    }

                    sdate = sdate.AddDays(1);
                }

                stdate = fsdate;
                enddate = edate;
                return View(dwclist);
            }
        }
        public ActionResult PrintDatewiseCollection()
        {
            List<Memberfees> mfeeslist = new List<Memberfees>();
            List<OpDatewisecollection> dwclist = new List<OpDatewisecollection>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                DateTime fsdate = stdate;
                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(p => p.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);
                var pst = db.Plans.Single(c => c.Plancode == 4);

                var tbd = (from ap in db.appltabs where ap.status == 1 select ap.bondid);
                Double tsis = 0, tMISFresh = 0, tMIS1 = 0, tMIS2 = 0, tMIS3 = 0, tMIS4 = 0, tMIS5 = 0, tMIS6 = 0, tMIS7a = 0, expense = 0, totalvoucher = 0, agfee = 0, appfee = 0, memberfee = 0, accfee = 0, deposit = 0, widthdraw = 0, pps = 0, late = 0, rebate = 0;
                while (stdate <= enddate)
                {
                    tMISFresh = (from o in db.Installmenttabs where o.paymentdate == stdate && o.installmentno == 1 && o.mode != "Single" && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tsis = (from o in db.Installmenttabs where o.paymentdate == stdate && o.installmentno == 1 && o.mode == "Single" && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS1 = (from o in db.Installmenttabs where o.paymentdate == stdate && o.installmentno != 1 && o.year == 1 && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS2 = (from o in db.Installmenttabs where o.paymentdate == stdate && o.installmentno != 1 && o.year == 2 && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS3 = (from o in db.Installmenttabs where o.paymentdate == stdate && o.installmentno != 1 && o.year == 3 && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS4 = (from o in db.Installmenttabs where o.paymentdate == stdate && o.installmentno != 1 && o.year == 4 && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS5 = (from o in db.Installmenttabs where o.paymentdate == stdate && o.installmentno != 1 && o.year == 5 && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS6 = (from o in db.Installmenttabs where o.paymentdate == stdate && o.installmentno != 1 && o.year == 6 && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS7a = (from o in db.Installmenttabs where o.paymentdate == stdate && o.installmentno != 1 && o.year >= 7 && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    late = (from o in db.Installmenttabs where o.paymentdate == stdate && o.payamount != 0 && o.mode != "Single" && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.latefine).DefaultIfEmpty(0).Sum();
                    rebate = (from o in db.Installmenttabs where o.paymentdate == stdate && o.payamount != 0 && o.mode != "Single" && o.type != "Adjustment" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.reliefrs).DefaultIfEmpty(0).Sum();

                    deposit = (from o in db.appltabs where o.pdate == stdate && o.branchcode == br.BranchCode && tbd.Contains(o.bondid) select o.downpayment).DefaultIfEmpty(0).Sum();
                    memberfee = (from ag in db.Member_tabs where ag.Cdate == stdate && ag.BranchCode == br.BranchCode select ag.Fee).DefaultIfEmpty(0).Sum();
                    appfee = (from ag in db.appltabs where ag.formdate == stdate && ag.status == 1 && ag.branchcode == br.BranchCode select ag.applicationcharge).DefaultIfEmpty(0).Sum();
                    agfee = (from ag in db.AgentDetails where ag.Doj == stdate && ag.BranchCode == br.BranchCode select ag.formfee).DefaultIfEmpty(0).Sum();
                    accfee = (from ag in db.SavingAccountInfos where ag.Doj == stdate && ag.branchcode == br.BranchCode select ag.formfee).DefaultIfEmpty(0).Sum();
                    widthdraw = (from o in db.spotcommission_tabs where o.date == stdate && o.branchcode == br.BranchCode select o.commission).DefaultIfEmpty(0).Sum();
                    totalvoucher = (from o in db.Voucher_Reports where o.date == stdate && o.branchcode == br.BranchCode select o.netamount).DefaultIfEmpty(0).Sum();
                    expense = (from ag in db.Expenses where ag.date_time == stdate && ag.branchcode == br.BranchCode select ag.amount).DefaultIfEmpty(0).Sum();

                    if (tsis + tMISFresh + tMIS1 + tMIS2 + tMIS3 + tMIS4 + tMIS5 + tMIS6 + tMIS7a + expense + totalvoucher + agfee + appfee + memberfee + accfee + deposit + widthdraw + pps + late + rebate > 0)
                    {
                        dwclist.Add(new OpDatewisecollection { date = stdate, tsis = tsis, tMISFresh = tMISFresh, tMIS1 = tMIS1, tMIS2 = tMIS2, tMIS3 = tMIS3, tMIS4 = tMIS4, tMIS5 = tMIS5, tMIS6 = tMIS6, tMIS7a = tMIS7a, Expense = expense, Voucher = totalvoucher, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, sdate = fsdate, edate = enddate, Appcharge = appfee, Agencyfee = agfee, Accountfee = accfee, Memberfee = memberfee, deposit = deposit, widthdraw = widthdraw, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice, pps = pps, late = late, releaf = rebate });
                    }
                    stdate = stdate.AddDays(1);
                }

                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports/Operator"), "DatewiseCollection.rpt"));
                rd.SetDataSource(dwclist);

                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();


                try
                {
                    Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    stream.Seek(0, SeekOrigin.Begin);
                    string dt = pdate.ToShortDateString();

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
        }

        [HttpGet]
        public ActionResult CsswiseCollection()
        {
            List<CsswiseBusiness> csswlist = new List<CsswiseBusiness>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(csswlist);
            }
        }
        [HttpPost]
        public ActionResult CsswiseCollection(DateTime date, int mincss, int maxcss)
        {
            List<CsswiseBusiness> csswlist = new List<CsswiseBusiness>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(p => p.OperatorId == User.Identity.Name);
                var bb = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);
                var tbd = (from ap in db.appltabs where ap.status == 1 select ap.bondid);
                var inslist = (from i in db.Installmenttabs orderby i.cssno where i.paymentdate == date && i.cssno >= mincss && i.cssno <= maxcss && i.opid == pr.OperatorId && tbd.Contains(i.bondid) select i).ToList();
                foreach (var ins in inslist)
                {
                    var br = db.appltabs.Single(b => b.bondid == ins.bondid);
                    csswlist.Add(new CsswiseBusiness { paymethod = ins.paymethod, newbondid = ins.newbondid, name = br.name, cssno = Convert.ToInt32(ins.cssno), planname = ins.planname, term = ins.term_plan, mode = ins.mode, amount = ins.amount, latefine = ins.latefine, rebate = ins.reliefrs, payamount = ins.payamount, expirydate = ins.expirydate, branchname = bb.BranchName, companyname = cr.CompanyName, branch = mr.branchname, bond = mr.custname, plan = mr.planname, date = date });
                }
                stdate = date;
                scss = mincss;
                ecss = maxcss;
                return View(csswlist);
            }
        }
        public ActionResult PrintCsswiseBusiness()
        {
            List<CsswiseBusiness> csswlist = new List<CsswiseBusiness>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(p => p.OperatorId == User.Identity.Name);
                var bb = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);
                var tbd = (from ap in db.appltabs where ap.status == 1 select ap.bondid);
                var inslist = (from i in db.Installmenttabs orderby i.cssno where i.paymentdate == stdate && i.cssno >= scss && i.cssno <= ecss && i.opid == pr.OperatorId && tbd.Contains(i.bondid) select i).ToList();
                foreach (var ins in inslist)
                {
                    var br = db.appltabs.Single(b => b.bondid == ins.bondid);
                    csswlist.Add(new CsswiseBusiness { newbondid = ins.newbondid, name = br.name, cssno = Convert.ToInt32(ins.cssno), planname = ins.planname, term = ins.term_plan, mode = ins.mode, amount = ins.amount, latefine = ins.latefine, rebate = ins.reliefrs, payamount = ins.payamount, expirydate = ins.expirydate, branchname = bb.BranchName, companyname = cr.CompanyName, branch = mr.branchname, bond = mr.custname, plan = mr.planname, date = stdate });
                }

                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports/Operator"), "CSSwiseBusiness.rpt"));
                rd.SetDataSource(csswlist);

                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();


                try
                {
                    Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    stream.Seek(0, SeekOrigin.Begin);
                    string dt = pdate.ToShortDateString();

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
        }

        [HttpGet]
        public ActionResult FundMobilisation()
        {
            List<FundMobilization> pwclist = new List<FundMobilization>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(pwclist);
            }
        }
        [HttpPost]
        public ActionResult FundMobilisation(DateTime sdate, DateTime edate)
        {
            List<FundMobilization> pwclist = new List<FundMobilization>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(p => p.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);

                var tbd = (from ap in db.appltabs where ap.status == 1 select ap.bondid);
                var planlist = (from pl in db.Installmenttabs where pl.paymentdate >= sdate && pl.paymentdate <= edate && pl.branch == br.BranchCode select new { pl.planname }).ToList().Distinct();
                Double tsis = 0, tMISFresh = 0, tMIS1 = 0, tMIS2 = 0, tMIS3 = 0, tMIS4 = 0, tMIS5 = 0, tMIS6 = 0, tMIS7a = 0, total = 0, tpps = 0, tnfp = 0;
                foreach (var plan in planlist)
                {
                    tMISFresh = (from o in db.Installmenttabs where o.paymentdate >= sdate && o.paymentdate <= edate && o.planname == plan.planname && o.installmentno == 1 && o.mode != "Single" && o.plantype == "RD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tnfp = (from o in db.Installmenttabs where o.paymentdate >= sdate && o.paymentdate <= edate && o.planname == plan.planname && o.installmentno == 1 && o.mode != "Single" && o.plantype == "NFS" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tsis = (from o in db.Installmenttabs where o.paymentdate >= sdate && o.paymentdate <= edate && o.planname == plan.planname && o.installmentno == 1 && o.mode == "Single" && o.plantype == "FD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tpps = (from o in db.appltabs where o.formdate >= sdate && o.formdate <= edate && o.planname == plan.planname && o.mode != "Single" && o.plantype == "PPS" && o.branchcode == br.BranchCode && tbd.Contains(o.bondid) select o.totalcon).DefaultIfEmpty(0).Sum();
                    tMIS1 = (from o in db.Installmenttabs where o.paymentdate >= sdate && o.paymentdate <= edate && o.planname == plan.planname && o.installmentno != 1 && o.year == 1 && o.plantype == "RD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS2 = (from o in db.Installmenttabs where o.paymentdate >= sdate && o.paymentdate <= edate && o.planname == plan.planname && o.installmentno != 1 && o.year == 2 && o.plantype == "RD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS3 = (from o in db.Installmenttabs where o.paymentdate >= sdate && o.paymentdate <= edate && o.planname == plan.planname && o.installmentno != 1 && o.year == 3 && o.plantype == "RD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS4 = (from o in db.Installmenttabs where o.paymentdate >= sdate && o.paymentdate <= edate && o.planname == plan.planname && o.installmentno != 1 && o.year == 4 && o.plantype == "RD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS5 = (from o in db.Installmenttabs where o.paymentdate >= sdate && o.paymentdate <= edate && o.planname == plan.planname && o.installmentno != 1 && o.year == 5 && o.plantype == "RD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS6 = (from o in db.Installmenttabs where o.paymentdate >= sdate && o.paymentdate <= edate && o.planname == plan.planname && o.installmentno != 1 && o.year == 6 && o.plantype == "RD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS7a = (from o in db.Installmenttabs where o.paymentdate >= sdate && o.paymentdate <= edate && o.planname == plan.planname && o.installmentno != 1 && o.year >= 7 && o.plantype == "RD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    total = tMISFresh + tsis + tpps + tnfp + tMIS1 + tMIS2 + tMIS3 + tMIS4 + tMIS5 + tMIS6 + tMIS7a;
                    if (total > 0)
                    {
                        pwclist.Add(new FundMobilization { planname = plan.planname, tPPSFresh = tpps, tNFPFresh = tnfp, tsis = tsis, tMISFresh = tMISFresh, tMIS1 = tMIS1, tMIS2 = tMIS2, tMIS3 = tMIS3, tMIS4 = tMIS4, tMIS5 = tMIS5, tMIS6 = tMIS6, tMIS7a = tMIS7a, total = total, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, sdate = sdate, edate = edate, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                    }
                }

                stdate = sdate;
                enddate = edate;
                return View(pwclist);
            }
        }
        public ActionResult PrintFundMobilisation()
        {
            List<FundMobilization> pwclist = new List<FundMobilization>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(p => p.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);

                var tbd = (from ap in db.appltabs where ap.status == 1 select ap.bondid);
                var planlist = (from pl in db.Installmenttabs where pl.paymentdate >= stdate && pl.paymentdate <= enddate && pl.branch == br.BranchCode select new { pl.planname }).Distinct().ToList();
                Double tsis = 0, tMISFresh = 0, tMIS1 = 0, tMIS2 = 0, tMIS3 = 0, tMIS4 = 0, tMIS5 = 0, tMIS6 = 0, tMIS7a = 0, total = 0, tpps = 0, tnfp = 0;
                foreach (var plan in planlist)
                {
                    tMISFresh = (from o in db.Installmenttabs where o.paymentdate >= stdate && o.paymentdate <= enddate && o.planname == plan.planname && o.installmentno == 1 && o.mode != "Single" && o.plantype == "RD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tnfp = (from o in db.Installmenttabs where o.paymentdate >= stdate && o.paymentdate <= enddate && o.planname == plan.planname && o.installmentno == 1 && o.mode != "Single" && o.plantype == "NFS" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tsis = (from o in db.Installmenttabs where o.paymentdate >= stdate && o.paymentdate <= enddate && o.planname == plan.planname && o.installmentno == 1 && o.mode == "Single" && o.plantype == "FD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tpps = (from o in db.appltabs where o.formdate >= stdate && o.formdate <= enddate && o.planname == plan.planname && o.mode != "Single" && o.plantype == "PPS" && o.branchcode == br.BranchCode && tbd.Contains(o.bondid) select o.totalcon).DefaultIfEmpty(0).Sum();
                    tMIS1 = (from o in db.Installmenttabs where o.paymentdate >= stdate && o.paymentdate <= enddate && o.planname == plan.planname && o.installmentno != 1 && o.year == 1 && o.plantype == "RD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS2 = (from o in db.Installmenttabs where o.paymentdate >= stdate && o.paymentdate <= enddate && o.planname == plan.planname && o.installmentno != 1 && o.year == 2 && o.plantype == "RD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS3 = (from o in db.Installmenttabs where o.paymentdate >= stdate && o.paymentdate <= enddate && o.planname == plan.planname && o.installmentno != 1 && o.year == 3 && o.plantype == "RD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS4 = (from o in db.Installmenttabs where o.paymentdate >= stdate && o.paymentdate <= enddate && o.planname == plan.planname && o.installmentno != 1 && o.year == 4 && o.plantype == "RD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS5 = (from o in db.Installmenttabs where o.paymentdate >= stdate && o.paymentdate <= enddate && o.planname == plan.planname && o.installmentno != 1 && o.year == 5 && o.plantype == "RD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS6 = (from o in db.Installmenttabs where o.paymentdate >= stdate && o.paymentdate <= enddate && o.planname == plan.planname && o.installmentno != 1 && o.year == 6 && o.plantype == "RD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    tMIS7a = (from o in db.Installmenttabs where o.paymentdate >= stdate && o.paymentdate <= enddate && o.planname == plan.planname && o.installmentno != 1 && o.year >= 7 && o.plantype == "RD" && o.branch == br.BranchCode && tbd.Contains(o.bondid) select o.payamount).DefaultIfEmpty(0).Sum();
                    total = tMISFresh + tsis + tpps + tnfp + tMIS1 + tMIS2 + tMIS3 + tMIS4 + tMIS5 + tMIS6 + tMIS7a;
                    if (total > 0)
                    {
                        pwclist.Add(new FundMobilization { planname = plan.planname, tPPSFresh = tpps, tNFPFresh = tnfp, tsis = tsis, tMISFresh = tMISFresh, tMIS1 = tMIS1, tMIS2 = tMIS2, tMIS3 = tMIS3, tMIS4 = tMIS4, tMIS5 = tMIS5, tMIS6 = tMIS6, tMIS7a = tMIS7a, total = total, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, sdate = stdate, edate = enddate, address = cr.Address, emailid = cr.Emailid, contact = cr.Contact, domainname = cr.HeadOffice });
                    }
                }

                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports/Operator"), "FundMobilisation.rpt"));
                rd.SetDataSource(pwclist);

                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();


                try
                {
                    Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    stream.Seek(0, SeekOrigin.Begin);
                    string dt = pdate.ToShortDateString();

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
        }

        [HttpGet]
        public ActionResult MaturityPaidDetails()
        {
            List<MaturityPaid> mpdlist = new List<MaturityPaid>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(mpdlist);
            }
        }
        [HttpPost]
        public ActionResult MaturityPaidDetails(DateTime sdate, DateTime edate)
        {
            List<MaturityPaid> mpdlist = new List<MaturityPaid>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(p => p.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);

                var mlist = (from pl in db.MaturityTabs where pl.maturitydate >= sdate && pl.maturitydate <= edate && pl.opid == br.BranchCode select pl).ToList();

                foreach (var m in mlist)
                {
                    mpdlist.Add(new MaturityPaid { newbondid = m.newbondid, name = m.name, newintroducerid = m.newintroducerid, mobileno = m.mobileno, formdate = m.formdate, planname = m.planname, term = m.term, mode = m.mode, payment = m.payment, nolandunit = m.nolandunit, totalcon = m.totalcon, expectedraisablevalue = m.expectedraisablevalue, expirydate = m.expirydate, maturityamount = m.maturityamount, maturitydate = m.maturitydate, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, agent = mr.agentname, bond = mr.custname, plan = mr.planname, sdate = sdate, edate = edate });

                }

                stdate = sdate;
                enddate = edate;

                return View(mpdlist);
            }
        }
        public ActionResult PrintMaturityPaidDetails()
        {
            List<MaturityPaid> mpdlist = new List<MaturityPaid>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var mr = db.Members.Single(m => m.Id == 1);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var pr = db.Operators.Single(p => p.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == pr.BranchCode);

                var mlist = (from pl in db.MaturityTabs where pl.maturitydate >= stdate && pl.maturitydate <= enddate && pl.opid == br.BranchCode select pl).ToList();

                foreach (var m in mlist)
                {
                    mpdlist.Add(new MaturityPaid { newbondid = m.newbondid, name = m.name, newintroducerid = m.newintroducerid, mobileno = m.mobileno, formdate = m.formdate, planname = m.planname, term = m.term, mode = m.mode, payment = m.payment, nolandunit = m.nolandunit, totalcon = m.totalcon, expectedraisablevalue = m.expectedraisablevalue, expirydate = m.expirydate, maturityamount = m.maturityamount, maturitydate = m.maturitydate, branchname = br.BranchName, companyname = cr.CompanyName, branch = mr.branchname, agent = mr.agentname, bond = mr.custname, plan = mr.planname, sdate = stdate, edate = enddate });

                }

                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports/Operator"), "MaturityPaidDetails.rpt"));
                rd.SetDataSource(mpdlist);

                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();


                try
                {
                    Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    stream.Seek(0, SeekOrigin.Begin);
                    string dt = pdate.ToShortDateString();

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
        }

        [HttpGet]
        public ActionResult AccBalanceReport()
        {
            List<AcBalanceReport> abrlist = new List<AcBalanceReport>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(abrlist);
            }
        }
        [HttpPost]
        public ActionResult AccBalanceReport(DateTime sdate, DateTime edate)
        {
            List<AcBalanceReport> abrlist = new List<AcBalanceReport>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var opr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var alist = (from pl in db.SavingAccountInfos where pl.Doj >= sdate && pl.Doj <= edate select pl).ToList();
                foreach (var a in alist)
                {
                    var creditsum = (from df in db.TransactionTabs where df.accountno == a.accountno select df.credit).DefaultIfEmpty(0).Sum();
                    var debitsum = (from df in db.TransactionTabs where df.accountno == a.accountno select df.debit).DefaultIfEmpty(0).Sum();
                    var balance = (creditsum - debitsum);
                    abrlist.Add(new AcBalanceReport { actype = a.actype, accountno = a.accountno, acholdername = a.acholdername, credit = creditsum, debit = debitsum, balance = balance, branchname = br.BranchName, companyname = cr.CompanyName, sdate = sdate, edate = edate });

                }

                stdate = sdate;
                enddate = edate;

                return View(abrlist);
            }
        }
        public ActionResult PrintAccBalanceReport()
        {
            List<AcBalanceReport> abrlist = new List<AcBalanceReport>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var opr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var alist = (from pl in db.SavingAccountInfos where pl.Doj >= stdate && pl.Doj <= enddate select pl).ToList();
                foreach (var a in alist)
                {
                    var creditsum = (from df in db.TransactionTabs where df.accountno == a.accountno select df.credit).DefaultIfEmpty(0).Sum();
                    var debitsum = (from df in db.TransactionTabs where df.accountno == a.accountno select df.debit).DefaultIfEmpty(0).Sum();
                    var balance = (creditsum - debitsum);
                    abrlist.Add(new AcBalanceReport { actype = a.actype, accountno = a.accountno, acholdername = a.acholdername, credit = creditsum, debit = debitsum, balance = balance, branchname = br.BranchName, companyname = cr.CompanyName, sdate = stdate, edate = enddate });

                }

                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "AccBalanceReport.rpt"));
                rd.SetDataSource(abrlist);

                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();


                try
                {
                    Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    stream.Seek(0, SeekOrigin.Begin);
                    string dt = pdate.ToShortDateString();

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
        }

        [HttpGet]
        public ActionResult Advancepaymentreport()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                List<AdvPaymentTemp> ap = new List<AdvPaymentTemp>();
                return View(ap);
            }
        }
        [HttpPost]
        public ActionResult Advancepaymentreport(int month, int year)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                List<AdvPaymentTemp> ap = new List<AdvPaymentTemp>();
                string ayear = year.ToString();
                MonthName mn = new MonthName();
                var monthname = mn.numbertomonthname(month) + "," + year;
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var opr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
                var an = (from c in db.AdvBrokerPaymentTabs where c.date.Month == month && c.date.Year == year && c.branchcode == br.BranchCode select c).ToList();
                foreach (var item in an.ToList())
                {
                    Double recoveryamount = 0;
                    string rdate = "-";
                    var ar = db.AgentDetails.Single(a => a.NewAgentId == item.newagentid);
                    var count = (from vc in db.Voucher_Reports where vc.agentid == ar.AgencyCode && vc.month == month && vc.year == ayear select vc).Count();
                    if (count > 0)
                    {
                        var vr = db.Voucher_Reports.Single(v => v.agentid == ar.AgencyCode && v.month == month && v.year == ayear);
                        var comm = (from v in db.Voucher_Reports where v.agentid == ar.AgencyCode && v.month == month && v.year == ayear select v.commission).DefaultIfEmpty(0).Sum();
                        var tds = (from v in db.Voucher_Reports where v.agentid == ar.AgencyCode && v.month == month && v.year == ayear select v.tds).DefaultIfEmpty(0).Sum();
                        var netvoucher = (from v in db.Voucher_Reports where v.agentid == ar.AgencyCode && v.month == month && v.year == ayear select v.netamount).DefaultIfEmpty(0).Sum();
                        recoveryamount = comm - tds - netvoucher;
                        rdate = vr.date.ToString("dd/MM/yyy");
                        ap.Add(new AdvPaymentTemp { newagentid = item.newagentid, name = ar.name, amount = item.amount, recoveryamount = recoveryamount, date = item.date, recoverydate = rdate, Remark = item.Remark, opid = item.opid, branchcode = item.branchcode, companyname = cr.CompanyName, monthname = monthname });
                    }
                    else
                    {
                        ap.Add(new AdvPaymentTemp { newagentid = item.newagentid, name = ar.name, amount = item.amount, recoveryamount = recoveryamount, date = item.date, recoverydate = rdate, Remark = item.Remark, opid = item.opid, branchcode = item.branchcode, companyname = cr.CompanyName, monthname = monthname });
                    }
                }
                vvyear = year;
                vmonth = month;
                return View(ap);
            }
        }
        public ActionResult PrintAdvancepayment()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                List<AdvPaymentTemp> ap = new List<AdvPaymentTemp>();
                string ayear = vvyear.ToString();
                MonthName mn = new MonthName();
                var monthname = mn.numbertomonthname(vmonth) + "," + vvyear;
                var cr = db.CompanyInfos.Single(c => c.Id == 1);
                var opr = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                var br = db.Branchtabs.Single(b => b.BranchCode == opr.BranchCode);
                string rdate = "-";
                var an = (from c in db.AdvBrokerPaymentTabs where c.date.Month == vmonth && c.date.Year == vvyear && c.branchcode == br.BranchCode select c).ToList();
                foreach (var item in an.ToList())
                {
                    Double recoveryamount = 0;
                    var ar = db.AgentDetails.Single(a => a.NewAgentId == item.newagentid);
                    var count = (from vc in db.Voucher_Reports where vc.agentid == ar.AgencyCode && vc.month == vmonth && vc.year == ayear select vc).Count();
                    if (count > 0)
                    {
                        var vr = db.Voucher_Reports.Single(v => v.agentid == ar.AgencyCode && v.month == vmonth && v.year == ayear);
                        var comm = (from v in db.Voucher_Reports where v.agentid == ar.AgencyCode && v.month == vmonth && v.year == ayear select v.commission).DefaultIfEmpty(0).Sum();
                        var tds = (from v in db.Voucher_Reports where v.agentid == ar.AgencyCode && v.month == vmonth && v.year == ayear select v.tds).DefaultIfEmpty(0).Sum();
                        var netvoucher = (from v in db.Voucher_Reports where v.agentid == ar.AgencyCode && v.month == vmonth && v.year == ayear select v.netamount).DefaultIfEmpty(0).Sum();
                        recoveryamount = comm - tds - netvoucher;
                        rdate = vr.date.ToString("dd/MM/yyy");
                        ap.Add(new AdvPaymentTemp { newagentid = item.newagentid, name = ar.name, amount = item.amount, recoveryamount = recoveryamount, date = item.date, recoverydate = rdate, Remark = item.Remark, opid = item.opid, branchcode = item.branchcode, companyname = cr.CompanyName, monthname = monthname });
                    }
                    else
                    {
                        ap.Add(new AdvPaymentTemp { newagentid = item.newagentid, name = ar.name, amount = item.amount, recoveryamount = recoveryamount, date = item.date, recoverydate = rdate, Remark = item.Remark, opid = item.opid, branchcode = item.branchcode, companyname = cr.CompanyName, monthname = monthname });
                    }
                }

                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Advancepayment.rpt"));
                rd.SetDataSource(ap);

                Response.Buffer = false;
                Response.ClearContent();
                Response.ClearHeaders();


                try
                {
                    Stream stream = rd.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                    stream.Seek(0, SeekOrigin.Begin);
                    string dt = pdate.ToShortDateString();

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
        }

        #endregion

        #region Search Menu Start Here

        [HttpGet]
        public ActionResult SearchBroker()
        {
            List<AgentDetail> ad = new List<AgentDetail>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(ad);
            }
        }
        [HttpPost]
        public ActionResult SearchBroker(string NewAgentId, string command, string brokerid)
        {
            if (command == "Search")
            {
                List<AgentDetail> ad = new List<AgentDetail>();
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Logout", "Operator");
                }
                else
                {

                    ad = (from dcl in db.AgentDetails where dcl.NewAgentId == NewAgentId select dcl).ToList();
                    return View(ad);
                }
            }
            else if (command == "PDF")
            {

                var dn = (from a in db.AgentDetails where a.NewAgentId == brokerid select a).ToList();
                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Broker.rpt"));
                rd.SetDataSource(dn);

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

                return View(dn);
            }
            return View();

        }

        [HttpGet]
        public ActionResult SearchBond()
        {
            List<appltab> ad = new List<appltab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View(ad);
            }

        }
        [HttpPost]
        public ActionResult SearchBond(string NewBondId, string command, string newbondid)
        {
            if (command == "Search")
            {
                List<appltab> ad = new List<appltab>();
                if (!User.Identity.IsAuthenticated)
                {
                    return RedirectToAction("Logout", "Operator");
                }
                else
                {

                    ad = (from dcl in db.appltabs where dcl.newbondid == NewBondId select dcl).ToList();
                    return View(ad);
                }
            }
            else if (command == "PDF")
            {
                var dn = (from a in db.appltabs where a.newbondid == newbondid select a).ToList();
                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Customer.rpt"));
                rd.SetDataSource(dn);

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

                return View(dn);

            }
            return View();
        }

        #endregion

        #region Change Password Start Here

        [HttpGet]
        public ActionResult ChangePassword()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
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
                return RedirectToAction("Logout", "Operator");
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

        #endregion


        [HttpGet]
        public ActionResult VisitorEntry()
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult VisitorEntry(Visitortab model)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var br = db.Operators.Single(o => o.OperatorId == User.Identity.Name);
                Visitortab vt = new Visitortab();
                vt.name = model.name;
                vt.mobile = model.mobile;
                vt.intime = model.intime;
                vt.outtime = model.outtime;
                vt.date = model.date;
                vt.purpose = model.purpose;
                vt.status = 1;
                vt.opid = User.Identity.Name;
                vt.branchcode = br.BranchCode;
                vt.introducer = model.introducer;
                vt.intromobile = model.intromobile;
                db.Visitortabs.Add(vt);
                db.SaveChanges();
                return View();
            }
        }

        [HttpGet]
        public ActionResult Profile()
        {
            List<Operator> opr = new List<Operator>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                opr = db.Operators.Where(c => c.OperatorId == User.Identity.Name).ToList();

            }
            return View(opr);
        }
        public JsonResult EditOperator(Operator ob)
        {
            List<Operator> opr = new List<Operator>();
            Operator opn = db.Operators.Single(c => c.OperatorId == User.Identity.Name);
            opn.OperatorName = ob.OperatorName;
            opn.OperatorAddress = ob.OperatorAddress;
            opn.Operator_Mail = ob.Operator_Mail;
            opn.OperatorMobile = ob.OperatorMobile;

            //db.Entry(opn).State = EntityState.Modified;
            db.Entry<Operator>(opn).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            opr = db.Operators.Where(c => c.OperatorId == User.Identity.Name).ToList();
            return Json(opr, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Logout()
        {
            //int max = (from p in db.operatorlogin_details where p.iid == User.Identity.Name select p.Id).Max();
            //operatorlogin_detail ob = db.operatorlogin_details.Single(b => b.Id == max);
            //ob.outdatetime = DateTime.Now;
            //db.SaveChanges();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Home");
        }

        [HttpGet]
        public ActionResult ChequeDuplicateReceipt()
        {
            List<DupliReceipt> dplist = new List<DupliReceipt>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                return View();
            }
        }
        [HttpPost]
        public ActionResult ChequeDuplicateReceipt(string newbondid)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {


                var ta = db.tempappltabs.Single(t => t.newbondid == newbondid);
                if (ta.paymethod == "Cheque" || ta.paymethod == "banktransaction")
                {

                    List<tempcheque> bond = new List<tempcheque>();
                    var chequeamount = "";
                    var chequedate = "";
                    var chequeno = "";
                    if (ta.paymethod == "Cheque")
                    {
                        chequeamount = "Amount of Cheque (Rs.)";
                        chequedate = "Date of Cheque";
                        chequeno = "Cheque Number";
                    }
                    else if (ta.paymethod == "banktransaction")
                    {
                        chequeamount = "Amount of transaction (Rs.)";
                        chequedate = "Date of transaction";
                        chequeno = "Transaction Number";
                    }
                    bond.Add(new tempcheque { colChequedate = chequedate, colchequeno = chequeno, colChequeAmount = chequeamount, newbondid = ta.newbondid, name = ta.name, nolandunit = ta.nolandunit, opid = ta.opid, bookingamount = ta.bookingamount, bank = ta.bank, Account = ta.Account, chequeno = ta.chequeno, ACholdername = ta.ACholdername, Branch = ta.Branch, IFSCCode = ta.IFSCCode, ChequeAmount = ta.ChequeAmount, Chequedate = ta.Chequedate, Chequeimage = ta.Chequeimage });

                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "MasterAckReceipt.rpt"));
                    rd.SetDataSource(bond);

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
                else
                {
                    con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandText = "TempBond";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Connection = con;
                    cmd.Parameters.AddWithValue("@newbondid", newbondid);


                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }

                    catch (Exception e)
                    {
                        ViewBag.msg = e.Message;
                    }
                    finally
                    {
                        con.Close();
                    }

                    List<Bond_report> bond = new List<Bond_report>();
                    bond = db.Bond_reports.ToList();
                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "report.rpt"));
                    rd.SetDataSource(bond);

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
            }
        }

        [HttpGet]
        public ActionResult FundManagement()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                return View();
            }
        }

        [HttpGet]
        public ActionResult ChequeRegis()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                List<DatewiseCustomerList> custlist = new List<DatewiseCustomerList>();
                SqlCommand cmd = new SqlCommand();
                con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                cmd.Connection = con;
                SqlDataReader sdr;
                try
                {
                    DateTime sdate = DateTime.Now.Date.AddDays(-1);
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd = new SqlCommand("chequeregister", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sdate", sdate);
                    cmd.Parameters.AddWithValue("@type", "ChequerRegis");
                    cmd.Parameters.AddWithValue("@opid", User.Identity.Name);
                    con.Open();
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        var rs = new DatewiseCustomerList();
                        rs.newbondid = sdr["acholdername"].ToString();
                        rs.name = sdr["chequeno"].ToString();
                        rs.newintroducerid = sdr["bank"].ToString();
                        rs.payment = Convert.ToDouble(sdr["chequeamount"]);
                        rs.formdate = Convert.ToDateTime(sdr["chequedate"]);
                        custlist.Add(rs);
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
                return View(custlist);

            }
        }

        [HttpGet]
        public ActionResult PrintChequeRegis()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                List<DatewiseCustomerList> custlist = new List<DatewiseCustomerList>();
                SqlCommand cmd = new SqlCommand();
                con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                cmd.Connection = con;
                SqlDataReader sdr;
                try
                {
                    var com = db.CompanyInfos.Single(s => s.Id == 1);
                    DateTime sdate = DateTime.Now.Date.AddDays(-1);
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd = new SqlCommand("chequeregister", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sdate", sdate);
                    cmd.Parameters.AddWithValue("@type", "ChequerRegis");
                    cmd.Parameters.AddWithValue("@opid", User.Identity.Name);
                    con.Open();
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {

                        var rs = new DatewiseCustomerList();
                        rs.newbondid = sdr["acholdername"].ToString();
                        rs.name = sdr["chequeno"].ToString();
                        rs.newintroducerid = sdr["bank"].ToString();
                        rs.payment = Convert.ToDouble(sdr["chequeamount"]);
                        rs.formdate = Convert.ToDateTime(sdr["chequedate"]);
                        rs.address = com.Address;
                        rs.companyname = com.CompanyName;
                        rs.emailid = com.Emailid;
                        rs.domainname = com.HeadOffice;
                        custlist.Add(rs);
                    }


                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "ChequeRegisterList.rpt"));
                    rd.SetDataSource(custlist);
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
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('" + ex.Message + "')</script>");
                }
                finally
                {
                    con.Close();
                }
                return View();
            }
        }

        [HttpGet]
        public ActionResult BankRegis()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                List<DatewiseCustomerList> custlist = new List<DatewiseCustomerList>();
                return View(custlist);
            }
        }
        [HttpPost]
        public ActionResult BankRegis(DateTime sdate)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                List<DatewiseCustomerList> custlist = new List<DatewiseCustomerList>();
                SqlCommand cmd = new SqlCommand();
                con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                cmd.Connection = con;
                SqlDataReader sdr;
                try
                {
                    //DateTime sdate = DateTime.Now.Date.AddDays(-1);
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd = new SqlCommand("chequeregister", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sdate", sdate);
                    cmd.Parameters.AddWithValue("@type", "BankRegis");
                    cmd.Parameters.AddWithValue("@opid", User.Identity.Name);
                    con.Open();
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {
                        var rs = new DatewiseCustomerList();
                        rs.newintroducerid = sdr["bank"].ToString();
                        rs.newbondid = sdr["accountno"].ToString();
                        rs.branch = sdr["transactiontype"].ToString();
                        rs.name = sdr["chequeno"].ToString();
                        rs.payment = Convert.ToDouble(sdr["chequeamount"]);
                        rs.formdate = Convert.ToDateTime(sdr["chequedate"]);
                        rs.sdate = sdate;
                        custlist.Add(rs);
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
                stdate = sdate;
                return View(custlist);

            }
        }

        [HttpGet]
        public ActionResult PrintBankTransRegis()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                List<DatewiseCustomerList> custlist = new List<DatewiseCustomerList>();
                SqlCommand cmd = new SqlCommand();
                con.ConnectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                cmd.Connection = con;
                SqlDataReader sdr;
                try
                {
                    var com = db.CompanyInfos.Single(s => s.Id == 1);
                    DateTime sdate = DateTime.Now.Date.AddDays(-1);
                    SqlDataAdapter da = new SqlDataAdapter();
                    cmd = new SqlCommand("chequeregister", con);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@sdate", stdate);
                    cmd.Parameters.AddWithValue("@type", "BankRegis");
                    cmd.Parameters.AddWithValue("@opid", User.Identity.Name);
                    con.Open();
                    sdr = cmd.ExecuteReader();
                    while (sdr.Read())
                    {

                        var rs = new DatewiseCustomerList();
                        rs.newintroducerid = sdr["bank"].ToString();
                        rs.newbondid = sdr["accountno"].ToString();
                        rs.branch = sdr["transactiontype"].ToString();
                        rs.name = sdr["chequeno"].ToString();
                        rs.payment = Convert.ToDouble(sdr["chequeamount"]);
                        rs.formdate = Convert.ToDateTime(sdr["chequedate"]);
                        rs.sdate = sdate;
                        custlist.Add(rs);
                        rs.address = com.Address;
                        rs.companyname = com.CompanyName;
                        rs.emailid = com.Emailid;
                        rs.domainname = com.HeadOffice;
                        custlist.Add(rs);
                    }


                    ReportDocument rd = new ReportDocument();
                    rd.Load(Path.Combine(Server.MapPath("~/Reports"), "PrintbankRegisterList.rpt"));
                    rd.SetDataSource(custlist);
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
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('" + ex.Message + "')</script>");
                }
                finally
                {
                    con.Close();
                }
                return View();
            }
        }


        public JsonResult AgentIdAC(string term)
        {

            var list = db.AgentDetails.Where(d => d.NewAgentId.ToUpper().Contains(term.ToUpper())).Select(e => new { e.NewAgentId, e.name }).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        
        [HttpGet]
        public ActionResult AgentKYC()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View();
            }
            return RedirectToAction("logout");
        }
        [HttpPost]
        public ActionResult AgentKYC(AgentDetail model, string command)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (command == "Update")
                {
                    var agd = db.AgentDetails.Single(d => d.NewAgentId == model.NewAgentId);
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
                    //db.Entry(agd).State = EntityState.Modified;
                    db.Entry<AgentDetail>(agd).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();
                    ViewBag.message = "Agent Updated Successfully.";
                }
                else if (command == "Search")
                {
                    var list = db.AgentDetails.Single(s => s.NewAgentId == model.NewAgentId);
                    return View(list);
                }
                return View(model);
            }
            return RedirectToAction("Logout");
        }

        public ActionResult UpdateMenu()
        {
            if (User.Identity.IsAuthenticated)
            {
                return View();
            }
            return RedirectToAction("logout");
        }
       
        [HttpGet]
        public ActionResult DailyClosing()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult DailyClosing(DailyDepositTab ddt, string date, string remark1, string remark2, Double transferamount1 = 0, Double transferamount2 = 0, Double Grandtotalcash = 0, Double bal = 0)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Branch");
            }
            else
            {
                DateTime sdate = DateTime.ParseExact(date, "dd/MM/yyyy", null);
                int count = (from a in db.DailyDepositTabs where a.date == sdate && a.opid == User.Identity.Name && a.closingtype == "New" select a).Count();
                int tempcount = (from a in db.TempDailyDepositTabs where a.date == sdate && a.opid == User.Identity.Name && a.closingtype == "New" select a).Count();
                if (tempcount == 0 && count == 0)
                {
                    var op = db.Operators.Single(a => a.OperatorId == User.Identity.Name);
                    Double chequebookpen = (from a in db.tempappltabs where a.paymethod == "Cheque" && a.opid == op.OperatorId && a.status == 1 && a.formdate == sdate select a.bookingamount).DefaultIfEmpty(0).Sum();
                    Double chequerenpen = (from a in db.TempInstallmenttabs where a.paymethod == "Cheque" && a.opid == op.OperatorId && a.status == 1 && a.paymentdate == sdate select a.payamount).DefaultIfEmpty(0).Sum();
                    Double remailning = 0;
                    if (ddt.transfertype == "Bank")
                    {
                        remailning = bal - transferamount1;
                    }
                    else if (ddt.transfertype == "Cash")
                    {
                        remailning = bal - transferamount2;
                    }

                    if (ddt.transfertype != "select")
                    {
                        int trid = (from a in db.DailyDepositTabs where a.opid == User.Identity.Name select a.trid).DefaultIfEmpty(0).Max() + 1;
                        DailyDepositTab dt = new DailyDepositTab();
                        dt.date = sdate;
                        dt.twothousand = ddt.twothousand;
                        dt.fivehundred = ddt.fivehundred;
                        dt.twohundred = ddt.twohundred;
                        dt.hundred = ddt.hundred;
                        dt.fifty = ddt.fifty;
                        dt.twenty = ddt.twenty;
                        dt.ten = ddt.ten;
                        dt.five = ddt.five;
                        dt.two = ddt.two;
                        dt.one = ddt.one;

                        //-----Cashrecieved----
                        dt.RecieveCashBookingApp = ddt.RecieveCashBookingApp;
                        dt.RecieveBankBookingApp = ddt.RecieveBankBookingApp;
                        dt.RecieveChequeBookingApp = ddt.RecieveChequeBookingApp;
                        dt.RecieveCashRenApp = ddt.RecieveCashRenApp;
                        dt.RecieveBankRenApp = ddt.RecieveBankRenApp;
                        dt.RecieveChequeRenApp = ddt.RecieveChequeRenApp;
                        //-----Pendingrecieved----
                        dt.RecieveBankBookingPending = ddt.RecieveBankBookingPending;
                        dt.RecieveChequeBookingPending = chequebookpen;
                        dt.RecieveBankRenPending = ddt.RecieveBankRenPending;
                        dt.RecieveChequeRenPending = chequerenpen;
                        //-------Return-------------
                        dt.ReturnCashVoucher = ddt.ReturnCashVoucher;
                        dt.ReturnBanVoucher = ddt.ReturnBanVoucher;
                        dt.ReturnChequeVoucher = ddt.ReturnChequeVoucher;
                        dt.ReturnCashSpot = ddt.ReturnCashSpot;
                        dt.ReturnBankSpot = ddt.ReturnBankSpot;
                        dt.ReturnCashExp = ddt.ReturnCashExp;
                        dt.ReturnChequeExp = ddt.ReturnChequeExp;
                        dt.ReturnBankExp = ddt.ReturnBankExp;
                        //----------Other-----------
                        dt.Latefine = ddt.Latefine;
                        dt.Relief = ddt.Relief;
                        dt.total = ddt.total;
                        dt.opid = User.Identity.Name;
                        dt.branchcode = op.BranchCode;
                        dt.status = 0;
                        dt.bankappdate = DateTime.Now.Date;
                        dt.RejectionReason = ddt.RejectionReason;
                        dt.TotalCashamount = Grandtotalcash;
                        dt.TotalChequeamount = ddt.TotalChequeamount;
                        dt.TotalBankamount = ddt.TotalBankamount;
                        dt.Depositedamount = bal;
                        dt.bankname = ddt.bankname;
                        dt.transfertype = ddt.transfertype;
                        dt.trid = trid;
                        if (ddt.transfertype == "Bank")
                        {
                            dt.transferamount = transferamount1;
                            dt.remainingamount = bal - transferamount1;
                            dt.Remark = remark1;
                            dt.bankapp = 0;
                        }
                        else if (ddt.transfertype == "Cash")
                        {
                            dt.transferamount = transferamount2;
                            dt.remainingamount = bal - transferamount2;
                            dt.Remark = remark2;
                            dt.bankapp = 1;
                        }
                        dt.closingtype = "New";
                        dt.matchingdate = sdate;
                        db.DailyDepositTabs.Add(dt);
                        db.SaveChanges();
                        var clcount = (from c in db.ClosingAmount_Tabs where c.opid == User.Identity.Name && c.status == 0 select c).Count();
                        double beforedue = (from c in db.ClosingAmount_Tabs where c.opid == User.Identity.Name && c.status == 0 select c.remaininamount).DefaultIfEmpty().Sum();
                        if (clcount == 1)
                        {
                            var cl = db.ClosingAmount_Tabs.Single(c => c.opid == User.Identity.Name && c.status == 0);
                            cl.status = 1;
                            db.SaveChanges();
                        }
                        if (remailning > 0)
                        {
                            var max = (from a in db.ClosingAmount_Tabs where a.opid == User.Identity.Name select a.Trid).DefaultIfEmpty(0).Max() + 1;
                            ClosingAmount_Tab ct = new ClosingAmount_Tab();
                            ct.remaininamount = remailning;
                            ct.Amount = bal;
                            ct.depositamount = ddt.total;
                            ct.Trid = max;
                            ct.status = 0;
                            ct.date = sdate;
                            ct.opid = User.Identity.Name;
                            db.ClosingAmount_Tabs.Add(ct);
                            db.SaveChanges();
                        }
                        Response.Write("<script>alert('Date Closed Successfully')</script>");
                    }
                    else if (ddt.transfertype == "select")
                    {

                        int trid = (from a in db.DailyDepositTabs where a.opid == User.Identity.Name select a.trid).DefaultIfEmpty(0).Max() + 1;
                        TempDailyDepositTab dt = new TempDailyDepositTab();
                        dt.date = sdate;
                        dt.twothousand = ddt.twothousand;
                        dt.fivehundred = ddt.fivehundred;
                        dt.twohundred = ddt.twohundred;
                        dt.hundred = ddt.hundred;
                        dt.fifty = ddt.fifty;
                        dt.twenty = ddt.twenty;
                        dt.ten = ddt.ten;
                        dt.five = ddt.five;
                        dt.two = ddt.two;
                        dt.one = ddt.one;

                        //-----Cashrecieved----
                        dt.RecieveCashBookingApp = ddt.RecieveCashBookingApp;
                        dt.RecieveBankBookingApp = ddt.RecieveBankBookingApp;
                        dt.RecieveChequeBookingApp = ddt.RecieveChequeBookingApp;
                        dt.RecieveCashRenApp = ddt.RecieveCashRenApp;
                        dt.RecieveBankRenApp = ddt.RecieveBankRenApp;
                        dt.RecieveChequeRenApp = ddt.RecieveChequeRenApp;
                        //-----Pendingrecieved----
                        dt.RecieveBankBookingPending = ddt.RecieveBankBookingPending;
                        dt.RecieveChequeBookingPending = chequebookpen;
                        dt.RecieveBankRenPending = ddt.RecieveBankRenPending;
                        dt.RecieveChequeRenPending = chequerenpen;
                        //-------Return-------------
                        dt.ReturnCashVoucher = ddt.ReturnCashVoucher;
                        dt.ReturnBanVoucher = ddt.ReturnBanVoucher;
                        dt.ReturnChequeVoucher = ddt.ReturnChequeVoucher;
                        dt.ReturnCashSpot = ddt.ReturnCashSpot;
                        dt.ReturnBankSpot = ddt.ReturnBankSpot;
                        dt.ReturnCashExp = ddt.ReturnCashExp;
                        dt.ReturnChequeExp = ddt.ReturnChequeExp;
                        dt.ReturnBankExp = ddt.ReturnBankExp;
                        //----------Other-----------
                        dt.Latefine = ddt.Latefine;
                        dt.Relief = ddt.Relief;
                        dt.total = ddt.total;
                        dt.opid = User.Identity.Name;
                        dt.branchcode = op.BranchCode;
                        dt.status = 0;
                        dt.bankappdate = DateTime.Now.Date;
                        dt.RejectionReason = ddt.RejectionReason;
                        dt.TotalCashamount = Grandtotalcash;
                        dt.TotalChequeamount = ddt.TotalChequeamount;
                        dt.TotalBankamount = ddt.TotalBankamount;
                        dt.Depositedamount = bal;
                        dt.bankname = ddt.bankname;
                        dt.transfertype = ddt.transfertype;
                        dt.trid = trid;
                        if (ddt.transfertype == "Bank")
                        {
                            dt.transferamount = transferamount1;
                            dt.remainingamount = bal - transferamount1;
                            dt.Remark = remark1;
                            dt.bankapp = 0;
                        }
                        else if (ddt.transfertype == "Cash")
                        {
                            dt.transferamount = transferamount2;
                            dt.remainingamount = bal - transferamount2;
                            dt.Remark = remark2;
                            dt.bankapp = 1;
                        }
                        dt.OpeningBalance = ddt.OpeningBalance;
                        dt.ClosingBalance = ddt.ClosingBalance;
                        dt.closingtype = "New";
                        dt.matchingdate = sdate;
                        db.TempDailyDepositTabs.Add(dt);
                        db.SaveChanges();
                        var clcount = (from c in db.ClosingAmount_Tabs where c.opid == User.Identity.Name && c.status == 0 select c).Count();
                        double beforedue = (from c in db.ClosingAmount_Tabs where c.opid == User.Identity.Name && c.status == 0 select c.remaininamount).DefaultIfEmpty().Sum();
                        if (clcount > 0)
                        {
                            var cl = db.ClosingAmount_Tabs.Single(c => c.opid == User.Identity.Name && c.status == 0);
                            cl.status = 1;
                            db.SaveChanges();
                        }
                        var max = (from a in db.ClosingAmount_Tabs where a.opid == User.Identity.Name select a.Trid).DefaultIfEmpty(0).Max() + 1;
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
                        Response.Write("<script>alert('Date Closed Successfully')</script>");
                    }

                }
                else
                {
                    Response.Write("<script>alert('This date is already closed')</script>");
                }
                return View();
            }
        }

        [HttpGet]
        public ActionResult DailyClosingReport()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                List<DailyDepositTab> daily = new List<DailyDepositTab>();
                return View(daily);
            }
        }
        [HttpPost]
        public ActionResult DailyClosingReport(DateTime sdate, DateTime edate)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                List<DailyDepositTab> daily = (from a in db.DailyDepositTabs where a.closingtype == "New" && a.date >= sdate && a.date <= edate && a.opid == User.Identity.Name select a).ToList();
                return View(daily);
            }
        }

        [HttpGet]
        public ActionResult MatchingDailyClosing()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "operator");
            }
            else
            {
                ViewData["pagename"] = TempData["pagename"];
                return View();
            }
        }
        [HttpPost]
        public ActionResult MatchingDailyClosing(DailyDepositTab ddt, string opid, string pagename)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {


                DateTime maxdate = (from a in db.DailyDepositTabs where a.opid == User.Identity.Name where a.status == 1 select a.date).DefaultIfEmpty().Max();
                int maxtrid = (from a in db.DailyDepositTabs where a.opid == User.Identity.Name where a.status == 1 && a.date == maxdate select a.trid).DefaultIfEmpty().Max();
                DailyDepositTab dt = db.DailyDepositTabs.Single(a => a.opid == User.Identity.Name && a.date == maxdate && a.trid == maxtrid);
                var dcount = (from a in db.DailyDepositTabs where a.opid == User.Identity.Name && a.date == maxdate select a).Count();
                var clcount = (from c in db.ClosingMatchingTabs where c.date == maxdate && c.opid == User.Identity.Name && c.pagename == pagename select c).Count();
                if ((clcount == 0) && (dt.twothousand != ddt.twothousand || dt.fivehundred != ddt.fivehundred || dt.twohundred != ddt.twohundred || dt.hundred != ddt.hundred || dt.fifty != ddt.fifty || dt.twenty != ddt.twenty || dt.ten != ddt.ten || dt.five != ddt.five || dt.two != ddt.two || dt.one != ddt.one))
                {
                    Response.Write("<script>alert('field does not match')</script>");
                    return RedirectToAction(pagename);
                }
                else
                {
                    ClosingMatchingTab ct = new ClosingMatchingTab();
                    ct.date = maxdate;
                    ct.opid = User.Identity.Name;
                    ct.pagename = pagename;
                    ct.remark = "Verified";
                    ct.status = 1;
                    db.ClosingMatchingTabs.Add(ct);
                    db.SaveChanges();
                    return RedirectToAction(pagename);
                }
                return View();
            }
        }

        #region Daily Visitor Detail -> Vishal

        [HttpGet]
        public ActionResult DailyVisitorDetail()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }

            return View();
        }
        [HttpPost]
        public ActionResult DailyVisitorDetail(DailyVisitorDetail model, string whom, string recogniser = "first")
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                if (db.DailyVisitorDetails.Count(o => o.VisitorId == model.VisitorId) > 0)
                {
                    ViewBag.msg = "Awww ! So Sorry You Are Re-Entring The Same Data,Your Request Has Been Denied By Admin..Try Again ";
                    return View();
                }
                else
                {
                    int a = 0;
                    if (recogniser == "first")
                    {
                        DailyVisitorDetail dvd = new DailyVisitorDetail();
                        //var cnt = db.DailyVisitorDetails.Count();
                        //if (cnt == 0)
                        //{
                        //    dvd.uniqueid = 1001;
                        //}
                        //else
                        //{
                        //    //var lastid = db.DailyVisitorDetails.LastOrDefault();
                        //    dvd.uniqueid = lastid.uniqueid + 1;
                        //}
                        dvd.VisitorId = model.VisitorId;
                        dvd.VisitorName = model.VisitorName;
                        dvd.MeetToWhom = whom;
                        dvd.Address = model.Address;
                        dvd.Refference = model.Refference;
                        dvd.Id = model.Id;
                        dvd.MobileNo = model.MobileNo;
                        dvd.ReceptionistId = db.DailyVisitors.Single(o => o.UserName == User.Identity.Name).UserName;
                        dvd.OfficeCode = db.DailyVisitors.Single(o => o.UserName == User.Identity.Name).OfficeCode;
                        dvd.add = model.add;
                        dvd.Status = 1;
                        dvd.TodaysDate = DateTime.Now.Date;
                        dvd.feedbackDate = DateTime.Now.Date;
                        dvd.InTime = DateTime.Now.ToShortTimeString();
                        dvd.Type = "Visitor";
                        dvd.EmailId = model.EmailId;
                        dvd.Remark = model.Remark;
                        dvd.feedback = model.feedback;
                        dvd.feedbackTime = model.feedbackTime;

                        db.DailyVisitorDetails.Add(dvd);
                        a = db.SaveChanges();
                        if (a == 1)
                        {
                            ViewBag.msg = "Successfully visitor has been now created.Thank You !! ";
                            MyClass.Sendmsg(model.MobileNo, "Thanks for Showing interest in our Company. Regards ODH DEVELOPERS PVT. LTD.");
                        }
                    }

                }
                return View();
            }
        }

        [HttpPost]
        public ActionResult CheckOutRemark(string visitorid, string remark)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var item = db.DailyVisitorDetails.Single(o => o.VisitorId == visitorid);
                item.OutTime = DateTime.Now.ToShortTimeString();
                item.Remark = remark;
                item.Status = 2;
                //db.Entry(item).State = EntityState.Modified;
                db.Entry<DailyVisitorDetail>(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("DailyVisitorDetail", "Operator");
            }
        }
        [HttpPost]
        public ActionResult AgentCheckOutRemark(string visitorid, string remark)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var item = db.DailyVisitorDetails.Single(o => o.VisitorId == visitorid);
                item.OutTime = DateTime.Now.ToShortTimeString();
                item.Remark = remark;
                item.Status = 2;
                //db.Entry(item).State = EntityState.Modified;
                db.Entry<DailyVisitorDetail>(item).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

                return RedirectToAction("agentcoming", "Operator");
            }



        #endregion


        }

        public ActionResult TodayList()
        {

            return View();
        }

        public JsonResult JsonFunction(string VisitorId, string Recogniser, string Remark)
        {
            if (Recogniser == "detail")
            {
                return Json(JsonConvert.SerializeObject(db.DailyVisitorDetails.Single(o => o.VisitorId == VisitorId)), 0);
            }
            if (Recogniser == "Remark")
            {
                var visitor = db.DailyVisitorDetails.Single(o => o.VisitorId == VisitorId);
                visitor.feedback = Remark;
                visitor.feedbackDate = DateTime.Now.Date;
                visitor.feedbackTime = DateTime.Now.ToShortTimeString();
                visitor.Status = 5;
                //db.Entry(visitor).State = EntityState.Modified;
                db.Entry<DailyVisitorDetail>(visitor).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();

            }
            return Json(0, 0);
        }

        [HttpGet]
        public ActionResult FindVisitor()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            { }
            return View();
        }
        [HttpPost]
        public ActionResult FindVisitor(string Visitorid, DateTime? Sdate, DateTime? Edate, string command)
        {
            List<DailyVisitorDetail> list = new List<DailyVisitorDetail>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                if (command == "Id")
                {
                    list = db.DailyVisitorDetails.Where(o => o.VisitorId == Visitorid).ToList();
                }
                else
                {

                    list = db.DailyVisitorDetails.Where(o => o.TodaysDate >= Sdate && o.TodaysDate <= Edate).ToList();

                }

            }

            return View(list);
        }

        public ActionResult VisitorFeedbackGiven(string VisitorId, string Remark, string Recogniser)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            { }

            return View();

        }

        [HttpGet]
        public ActionResult GetFullVisitorDetail(string mobile, string visitorname, string feedback, string remark, string address, string emailid, string recogniser, string visitorid)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                if (recogniser == "update" && visitorid != "")
                {
                    var item = db.DailyVisitorDetails.Single(o => o.VisitorId == visitorid);
                    item.VisitorName = visitorname;
                    item.MobileNo = mobile;
                    item.EmailId = emailid;
                    item.Remark = remark;
                    item.feedback = feedback;
                    item.Address = address;
                    item.feedbackDate = DateTime.Now.Date;
                    item.feedbackTime = DateTime.Now.ToShortTimeString();
                    //db.Entry(item).State = EntityState.Modified;
                    db.Entry<DailyVisitorDetail>(item).State = System.Data.Entity.EntityState.Modified;
                    db.SaveChanges();

                }

                ViewData["fff"] = TempData["gg"];
                return View();

            }

        }
        [HttpPost]
        public ActionResult GetFullVisitorDetail(string VisitorId)
        {
            List<DailyVisitorDetail> list = new List<DailyVisitorDetail>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {

                ViewData["fff"] = VisitorId;
                return View();
                //list= db.DailyVisitorDetails.Where(o => o.VisitorId == VisitorId).ToList();
            }


        }

        public ActionResult RecentVisitorDetail()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
            }

            return View();
        }

        [HttpGet]
        public ActionResult ChangeCredential()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
            }

            return View();
        }
        [HttpPost]
        public ActionResult ChangeCredential(string last, string newpass, string confirm)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Operator");
            }
            else
            {
                var a = db.NewLogins.Where(o => o.UserName == User.Identity.Name && o.Password == last).Count();
                if (a == 1)
                {
                    var b = db.NewLogins.Single(o => o.UserName == User.Identity.Name && o.Password == last);
                    if (newpass == confirm)
                    {
                        var rec = db.DailyVisitors.Single(o => o.UserName == User.Identity.Name && o.Password == last);
                        rec.Password = newpass;
                        //db.Entry(rec).State = EntityState.Modified;
                        db.Entry<DailyVisitor>(rec).State = System.Data.Entity.EntityState.Modified;
                        b.Password = newpass;
                        //db.Entry(b).State = EntityState.Modified;
                        db.Entry<NewLogin>(b).State = System.Data.Entity.EntityState.Modified;

                        if (1 == db.SaveChanges())
                        {
                            ViewBag.msg = "Your password has been successfully changed !!";
                            return View();

                        }
                    }
                }
                ViewBag.msg = "Warning : Unfortunately your password been not changed !!";
            }

            return View();
        }

        public JsonResult getotp(string mobile)
        {
            var list = 0;
            if (mobile != null)
            {
                var otp = gid1(4);
                list = Convert.ToInt32(otp);
                MyClass.Sendmsg(mobile, "You need an OTP for Verify Mobile No DO NOT SHARE IT WITH ANYONE. " + otp + " .Please Verify Soon");
                return Json(list, JsonRequestBehavior.AllowGet);
            }
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public string gid1(int len)
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

        [HttpGet]
        public ActionResult agentcoming()
        {
            return View();
        }
        [HttpPost]
        public ActionResult agentcoming(DailyVisitorDetail nt)
        {
            DailyVisitorDetail at = new DailyVisitorDetail();
            at.VisitorId = nt.VisitorId;
            at.VisitorName = nt.VisitorName;
            at.MobileNo = nt.MobileNo;
            at.EmailId = nt.EmailId;
            at.MeetToWhom = nt.MeetToWhom;
            at.add = nt.add;
            at.Type = "Agent";
            at.Status = 1;
            at.Address = nt.Address;
            at.Refference = nt.Refference;
            at.TodaysDate = nt.TodaysDate;
            at.feedbackTime = nt.feedbackTime;
            at.TodaysDate = DateTime.Now.Date;
            at.feedbackDate = DateTime.Now.Date;
            at.InTime = DateTime.Now.ToShortTimeString();
            db.DailyVisitorDetails.Add(at);
            Response.Write("<script>alert('Sucessfully Submitted')</script>");
            db.SaveChanges();
            return View();
        }

        public JsonResult AutoComplete(string term)
        {
            var list = (from r in db.AgentDetails where r.NewAgentId.ToLower().Contains(term.ToLower()) || r.name.ToUpper().Contains(term.ToUpper()) select new { r.NewAgentId, r.name }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);

        }
        public JsonResult selectdetail(string visitorid)
        {

            List<newtab> srlist = new List<newtab>();
            var appinfo = db.AgentDetails.Single(s => s.NewAgentId == visitorid);
            srlist.Add(new newtab { VisitorName = appinfo.name, mobileno = appinfo.Mobileno, email = appinfo.Email, address = appinfo.Address });
            return Json(srlist, JsonRequestBehavior.AllowGet);
        }

    }
}