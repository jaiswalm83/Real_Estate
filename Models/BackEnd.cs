using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ODHDEVELOPERS.Models
{
    public class BackEnd
    {
        UsersContext db = new UsersContext();
        CompanyInfo cg = new CompanyInfo();
        Member mrg = new Member();
        public BackEnd()
        {
            cg = db.CompanyInfos.Single(d => d.Id == 1);
            mrg = db.Members.Single(d => d.Id == 1);
        }
        #region 20-May-2019
        public List<AdvisorCollection> AdvisorCollection(string newintroducerid, DateTime sdate, DateTime edate)
        {
            var coll = new List<AllCollection>();
            var allcoll = new List<AdvisorCollection>();
            var agt = db.AgentDetails.FirstOrDefault(d => d.NewAgentId == newintroducerid);
            sdate = sdate.Date; edate = edate.Date;
            string c = "a.newbondid,a.name,a.plancode,a.plantype,a.planname,i.payamount,i.year,i.installmentno";
            string q = "select " + c + " from installmenttab i join appltab a on i.newbondid=a.newbondid where i.payamount>0 and " +
                       "a.newintroducerid='" + newintroducerid + "' and  i.paymentdate>='" + sdate.ToString("yyyy-MM-dd") + "' and i.paymentdate<='" + edate.ToString("yyyy-MM-dd") + "'";
            if (agt != null)
            {
                var br = db.Branchtabs.Single(d => d.BranchCode == agt.BranchCode);
                coll = db.Database.SqlQuery<AllCollection>(q).ToList();
                var bonds = coll.Select(d => d.newbondid).Distinct().ToList();
                foreach (var i in bonds)
                {
                    var single = coll.FirstOrDefault(d => d.newbondid == i);
                    var total = coll.Where(d => d.newbondid == i).Select(d => d.payamount).DefaultIfEmpty(0).Sum();
                    allcoll.Add(new AdvisorCollection()
                    {
                        companyname = cg.CompanyName,
                        companyaddress = cg.Address,
                        companyemail = cg.Emailid,
                        companycontact = cg.Contact,
                        companydomain = cg.HeadOffice,
                        opid = newintroducerid,
                        reportname = mrg.agentname + "Advisor Self Collection",
                        reportduration = "Duration: From " + sdate.ToString("dd-MMM-yyyy") + " To " + edate.ToString("dd-MMM-yyyy"),
                        branch = br.BranchName + " (" + br.BranchCode + ")",
                        advisorid = agt.NewAgentId,
                        advisorname = agt.name,
                        custid = single.newbondid,
                        custname = single.name,
                        planname = single.planname,
                        freshamount = coll.Where(d => d.newbondid == i && d.installmentno == 1).Select(d => d.payamount).Sum(),
                        first = coll.Where(d => d.newbondid == i && d.year == 1 && d.installmentno > 1).Select(d => d.payamount).DefaultIfEmpty(0).Sum(),
                        second = coll.Where(d => d.newbondid == i && d.year == 2).Select(d => d.payamount).DefaultIfEmpty(0).Sum(),
                        third = coll.Where(d => d.newbondid == i && d.year == 3).Select(d => d.payamount).DefaultIfEmpty(0).Sum(),
                        fourth = coll.Where(d => d.newbondid == i && d.year == 4).Select(d => d.payamount).DefaultIfEmpty(0).Sum(),
                        fifth = coll.Where(d => d.newbondid == i && d.year == 5).Select(d => d.payamount).DefaultIfEmpty(0).Sum(),
                        sixth = coll.Where(d => d.newbondid == i && d.year == 6).Select(d => d.payamount).DefaultIfEmpty(0).Sum(),
                        seventh = coll.Where(d => d.newbondid == i && d.year == 7).Select(d => d.payamount).DefaultIfEmpty(0).Sum(),
                        businessvalue = total,
                        total = total
                    });
                }
            }
            return allcoll;
        }
        public List<AdvisorCollection> AdvisorChainCollection(string newintroducerid, DateTime sdate, DateTime edate)
        {
            var coll = new List<AllCollection>();
            var allcoll = new List<AdvisorCollection>();
            var agt = db.AgentDetails.FirstOrDefault(d => d.NewAgentId == newintroducerid);
            sdate = sdate.Date; edate = edate.Date;
            if (agt != null)
            {
                var br = db.Branchtabs.Single(d => d.BranchCode == agt.BranchCode);
                SqlParameter[] param = new SqlParameter[] { 
                new SqlParameter("@newagentid",newintroducerid),
                new SqlParameter("@sdate",sdate.ToString("yyyy-MM-dd")),
                new SqlParameter("@edate",edate.ToString("yyyy-MM-dd"))
                };

                allcoll = db.Database.SqlQuery<AdvisorCollection>("exec advisorchaincollection @newagentid,@sdate,@edate", param).ToList();
                var l = allcoll.Count();
                if (l > 0) {
                    allcoll[0].opid = newintroducerid;
                    allcoll[0].companyname = cg.CompanyName;
                    allcoll[0].companyemail = cg.Emailid;
                    allcoll[0].companycontact = cg.Contact;
                    allcoll[0].companydomain = cg.HeadOffice;
                    allcoll[0].companyaddress = cg.Address;
                    allcoll[0].branch = br.BranchName + " (" + br.BranchCode + ")";
                    allcoll[0].reportname = mrg.agentname + " Chain Collection";
                    allcoll[0].reportduration = "Duration: From " + sdate.ToString("dd-MMM-yyyy") + " To " + edate.ToString("dd-MMM-yyyy");
                    //
                    l = l - 1;
                    allcoll[l].opid = newintroducerid;
                    allcoll[l].companyname = cg.CompanyName;
                    allcoll[l].companyemail = cg.Emailid;
                    allcoll[l].companycontact = cg.Contact;
                    allcoll[l].companydomain = cg.HeadOffice;
                    allcoll[l].companyaddress = cg.Address;
                    allcoll[l].branch = br.BranchName + " (" + br.BranchCode + ")";
                    allcoll[l].reportname = mrg.agentname + " Chain Collection";
                    allcoll[l].reportduration = "Duration: From " + sdate.ToString("dd-MMM-yyyy") + " To " + edate.ToString("dd-MMM-yyyy");
                }
            }
            return allcoll;
        }
        #endregion 20-May-2019

    }
}