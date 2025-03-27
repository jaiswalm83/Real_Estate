using ODHDEVELOPERS.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ODHDEVELOPERS.Controllers
{
    public class HRController : Controller
    {
        SqlConnection con = new SqlConnection();
        UsersContext db = new UsersContext();
     
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
        string gidd()
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
        //
        // GET: /HR/




        public JsonResult AutoCompleteEmpid(string term)
        {

            var list = (from r in db.Emp_Salarys where r.empid.ToLower().Contains(term.ToLower()) || r.empid.ToUpper().Contains(term.ToUpper()) select new { r.empid}).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
            
        }

        public JsonResult AutoCompleteSalaryMonth(string term)
        {

            var list = (from r in db.Salary_ems where r.month.ToLower().Contains(term.ToLower()) select new { r.month }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult AutoCompleteEmpDesignation(string term)
        {

            var list = (from r in db.Emp_Regs where r.designation.ToLower().Contains(term.ToLower()) select new { r.designation }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult AutoCompleteEmpDepartment(string term)
        {

            var list = (from r in db.Emp_Regs where r.department.ToLower().Contains(term.ToLower()) select new { r.department }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        public JsonResult AutoCompleteEmp_sal_month(string term)
        {

            var list = (from r in db.Emp_Salarys where r.month.ToLower().Contains(term.ToLower()) select new { r.month }).Distinct();
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
        public ActionResult EmployeeReg()
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
        public ActionResult EmployeeReg(Emp_Reg model, HttpPostedFileBase Photo, HttpPostedFileBase Photoimg)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {


                Emp_Reg ad = new Emp_Reg();
                ad.name = model.name;
                ad.father = model.father;
                ad.department = model.department;
                ad.designation = model.designation;
                ad.basicsalary = model.basicsalary;
                ad.dob = model.dob;
                ad.gender = model.gender;
                ad.mob = model.mob;
                ad.registerdate = DateTime.Now;
                ad.doj = model.doj;
                ad.email = model.email;
                ad.address = model.address;
                ad.branchcode = model.branchcode;
                ad.empid = model.empid;
                if (Photo != null)
                {
                    string imgname = gid();
                    ad.cv = "~/Photo/" + imgname + ".doc";
                    Photo.SaveAs(HttpContext.Server.MapPath("~/Photo/") + imgname + ".doc");


                }
                else
                {
                    ad.cv = "~/Photo/default.jpg";

                }



                if (Photoimg != null)
                {
                    string imgname = gid();
                    ad.img = "~/Photo/" + imgname + ".jpg";
                    Photoimg.SaveAs(HttpContext.Server.MapPath("~/Photo/") + imgname + ".jpg");


                }
                else
                {
                    ad.img = "~/Photo/default.jpg";

                }
                db.Emp_Regs.Add(ad);
                db.SaveChanges();
                Response.Write("<script>alert('Employee Registration SuccessFully.')</script>");

            }
            return View();
        }

     [HttpGet]
        public ActionResult EmployeeLeave()
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
        public ActionResult EmployeeLeave(Emp_leave model)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }

            else
            {
                if (ModelState.IsValid)
                {
                    var ad = db.Emp_Regs.Single(dcl => dcl.empid == model.empid);
                    Emp_leave el = new Emp_leave();
                    el.branchcode = ad.branchcode;
                    el.empid = model.empid;
                    el.leavetype = model.leavetype;
                    el.reason = model.reason;
                    el.sdate = model.sdate;
                    el.edate = model.edate;
                    db.Emp_leaves.Add(el);
                    db.SaveChanges();
                    ViewBag.msg = "Leave application successfully submited.";
                }
                return View();
            }
        }

        [HttpGet]
        public ActionResult UpdateEmployeeReg()
        {
            List<Emp_Reg> li = new List<Emp_Reg>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                return View(li);
            }

        }


        [HttpPost]
        public ActionResult UpdateEmployeeReg(Emp_Reg model, HttpPostedFileBase Photoimg, HttpPostedFileBase Photo, string empid = "", string command = "", string emppid = "")
        {
            List<Emp_Reg> li = new List<Emp_Reg>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                if (command == "search")
                {
                    TempData["empid"] = emppid;
                    li = db.Emp_Regs.Where(a => a.empid == emppid).ToList();
                }
                else if (command == "submit")
                {
                    var ad = db.Emp_Regs.Single(a => a.empid == empid);
                    ad.name = model.name;
                    ad.father = model.father;
                    ad.department = model.department;
                    ad.designation = model.designation;
                    ad.basicsalary = model.basicsalary;
                    ad.dob = model.dob;
                    ad.gender = model.gender;
                    ad.mob = model.mob;
                    ad.doj = model.doj;
                    ad.email = model.email;
                    ad.address = model.address;
                    ad.branchcode = model.branchcode;
                    ad.empid = model.empid;
                    ad.distict = model.distict;
                    ad.registerdate = model.registerdate;
                    ad.state = model.state;
                    if (Photo != null)
                    {
                        string imgname = gid();
                        var ext = Path.GetExtension(Photo.FileName);
                        ad.cv = "~/Other/" + imgname + ext;
                        Photo.SaveAs(HttpContext.Server.MapPath(ad.cv));


                    }
                    else
                    {
                        ad.cv = "~/Other/Upload_Project_Technical_Specification_Document.pdf";

                    }



                    if (Photoimg != null)
                    {
                        string imgname = gid();
                        ad.img = "~/Photo/" + imgname + ".jpg";
                        Photoimg.SaveAs(HttpContext.Server.MapPath("~/Photo/") + imgname + ".jpg");


                    }
                    else
                    {
                        ad.img = "~/Photo/default.jpg";

                    }
                    db.Entry<Emp_Reg>(ad).State = System.Data.Entity.EntityState.Modified;
                    
                    db.SaveChanges();
                    Response.Write("<script>alert('Employee Update SuccessFully.')</script>");
                }
                return View(li);
            }

            return View(li);
        }



        public JsonResult Autoempid(string term)
        {

            var list = (from r in db.Emp_Regs where r.email.ToLower().Contains(term.ToLower()) || r.name.ToLower().Contains(term.ToLower()) select new { r.empid, r.name }).Distinct();
            return Json(list, JsonRequestBehavior.AllowGet);
        }


        //This is update code


        [HttpGet]
        public ActionResult EmployeeAtten()
        {
            List<Emp_Reg> dn = new List<Emp_Reg>();
            List<Emp_atten> empatten = new List<Emp_atten>();
            DateTime date = DateTime.Now.Date;
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {

                //var or = db.HRTabs.Single(o => o.HRId == User.Identity.Name);
                //if (mark == null)
                //{

                //    empatten = (from ae in db.Emp_attens where ae.date == date select ae).ToList();
                //    dn = (from ob in db.Emp_Regs where ob.branchcode == or.BranchCode select ob).ToList();
                //    foreach (var ea in empatten)
                //    {
                //        dn.RemoveAll(x => x.empid == ea.empid);
                //    }
                //}
                //else
                //{
                //    var er = db.Emp_Regs.Single(o => o.empid == empid);
                //    Emp_atten en = new Emp_atten();
                //    en.empid = empid;
                //    en.name = er.name;
                //    en.date = DateTime.Now.Date;
                //    en.atten = mark;
                //    en.branchcode = or.BranchCode;

                //    db.Emp_attens.Add(en);
                //    db.SaveChanges();

                //    empatten = (from ae in db.Emp_attens where ae.date == date select ae).ToList();
                //    dn = (from ob in db.Emp_Regs where ob.branchcode == or.BranchCode select ob).ToList();
                //    foreach (var ea in empatten)
                //    {
                //        dn.RemoveAll(x => x.empid == ea.empid);
                //    }


                //}


                //return View(dn);

              

                return View();
            }
        }
        [HttpPost]
        public ActionResult EmployeeAtten(string command, string memberid, string[] att, string[] From, string[] To, string Attendencedate1, string date1 = "")
        {
            List<Emp_Reg> dn = new List<Emp_Reg>();
            List<Emp_atten> empatten = new List<Emp_atten>();
            DateTime date = DateTime.Now.Date;
            DateTime Attendencedate = date;
            DateTime.TryParse(Attendencedate1, out Attendencedate);
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                if (command == "Search")
                {
                    TempData["date1"] = date1;
                    if (command == "Search")
                    {
                        dn = db.Emp_Regs.ToList();

                        TempData["list"] = dn;
                    }
                }

                else if (command == "submit")
                {
                   var i=0;

                    foreach (var item in TempData["list"] as IEnumerable<Emp_Reg>)
                    {
                        var newattendate=Convert.ToDateTime(Attendencedate);
                        var countatten = db.Emp_attens.Where(aa => aa.empid == item.empid && aa.Attendencedate == newattendate).Count();
                        if (countatten == 0)
                        {
                            db.Emp_attens.Add(new Emp_atten { empid = item.empid, name = item.name, atten = att[i], timingfrom = From[i] ?? DateTime.Now.ToShortTimeString(), timingto = To[i] ?? DateTime.Now.ToShortTimeString(), Attendencedate = newattendate, date = newattendate, status = 1 });
                        }
                        var sgcount = db.Emp_attens.Where(a => a.empid == item.empid).Count();
                        if (sgcount > 0)
                        {
                            //var sg = db.Emp_leaves.Single(a => a.empid == item.empid);
                            //if (sg.edate == Attendencedate)
                            //{
                            //    sg.status = 1;
                            //}
                        }
                        i++;
                    
                    }
                    
                    db.SaveChanges();
                    Response.Write("<script>alert('Attendence Saved Successfully !!!!')</script>");
                }
                return View(dn);
            }
        }

        public ActionResult block_emp(int Id = 0, int status = 0)
        {
            
            var getstatus = db.Emp_Regs.Single(x => x.Id == Id);
            if (status != 0)
            {
                getstatus.status = 0;
            }
            else{
                getstatus.status = 1;
            }
               db.Entry<Emp_Reg>(getstatus).State = System.Data.Entity.EntityState.Modified;
                //db.Entry(getstatus).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("EmployeeAtten", "HR");
        }
        [HttpGet]
        public ActionResult updateatten()
        {
            List<Emp_Reg> dn = new List<Emp_Reg>();
            List<Emp_atten> empatten = new List<Emp_atten>();
            DateTime date = DateTime.Now.Date;
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
        public ActionResult updateatten(string command, string memberid, string[] att, string[] From, string[] To, string Attendencedate1, string date1 = "")
        {
            List<Emp_Reg> dn = new List<Emp_Reg>();
            List<Emp_atten> empatten = new List<Emp_atten>();
            DateTime date = DateTime.Now.Date;
            DateTime Attendencedate = date;
            DateTime.TryParse(Attendencedate1, out Attendencedate);
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                if (command == "Search")
                {
                    TempData["date1"] = date1;
                    if (command == "Search")
                    {
                        var newdate=DateTime.Parse(date1);
                        empatten = db.Emp_attens.Where(aa=>aa.Attendencedate==newdate).ToList();

                        TempData["list"] = empatten;
                    }
                }

                else if (command == "submit")
                {
                    var i = 0;

                    foreach (var item in TempData["list"] as IEnumerable<Emp_atten>)
                    {
  
                        var attendetail = db.Emp_attens.Single(aa => aa.Id == item.Id);
                        attendetail.timingfrom = From[i] ?? DateTime.Now.ToShortTimeString();
                        attendetail.timingto = To[i] ?? DateTime.Now.ToShortTimeString();
                        attendetail.Attendencedate = Convert.ToDateTime(Attendencedate);
                        attendetail.date = date = Convert.ToDateTime(Attendencedate);
                        attendetail.atten = att[i];
                        //db.Entry(attendetail).State = EntityState.Modified;
                        db.Entry<Emp_atten>(attendetail).State = System.Data.Entity.EntityState.Modified;
                        db.SaveChanges();

                        i++;

                    }

                    db.SaveChanges();
                    TempData["msg"]= "Attendence Saved Successfully !!!!";
                    return RedirectToAction("updateatten", "HR");
                }
                return View(empatten);
            }
        }

        public ActionResult monthlyatten()
        {
            return View();
        }


        //This is c# code

        [HttpGet]
        public ActionResult Empl_salary()
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
        public ActionResult Empl_salary(Emp_Salary model, string empid, int year, string month)
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                var count = (from b in db.Emp_Salarys where b.empid == empid && b.year == year && b.month == month select b).Count();
                var cnt = db.Emp_Salarys.Where(a => a.empid == a.empid).Count();
                //var cn = db.Emp_Salarys.Single(a => a.empid == empid);
                var mm = month;
                var yy = year;
                MonthNum mint = new MonthNum();
                DateTime startdate = new DateTime(year, 1, 1);
                int day = DateTime.DaysInMonth(year, 1);
                DateTime enddate = new DateTime(year, 1, day);
                var total_atten = db.Emp_attens.Where(a => a.atten == "P" && a.date >= startdate && a.date <= enddate && a.empid == empid).Count();
                var basic = model.basicsalary / 30;
                var totaksalary = basic * total_atten;
                var totalbasicsalary = model.basicsalary - totaksalary;
                if (count == 0)
                {
                    Emp_Salary ad = new Emp_Salary();
                    ad.empid = model.empid;
                    ad.branchcode = model.branchcode;
                    ad.opid = User.Identity.Name;
                    ad.HRA = model.HRA;
                    ad.DA = model.DA;
                    ad.CCA = model.CCA;
                    ad.TA = model.TA;
                    ad.medical = model.medical;
                    ad.advance_Pay = model.advance_Pay;
                    ad.professionaltax = model.professionaltax;
                    ad.loan = model.loan;
                    ad.provisional_fund = model.provisional_fund;
                    ad.year = model.year;
                    ad.month = model.month;
                    ad.basicsalary = totaksalary;
                    ad.doj = DateTime.Now.Date;
                    //ad.Perdaysalery = model.basicsalary;
                    //ad.deductAmt = model.Perdaysalery;
                    db.Emp_Salarys.Add(ad);
                    db.SaveChanges();
                    Response.Write("<script>alert('Employee Salary SuccessFully.')</script>");
                }
                else
                {
                    Response.Write("<script>alert('Please Fill Correct Empid or Month or Year')</script>");

                }



            }
            return View();

        }




        [HttpGet]
        public ActionResult Viewsalary()
        {
            List<Salary_em> emp = new List<Salary_em>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {

                return View(emp);
            }
        }

        [HttpPost]
        public ActionResult Viewsalary(Emp_Salary o, string empid, int month, int year)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {

                int days = DateTime.DaysInMonth(year, month);
                MonthName mm = new MonthName();
                var month1 = mm.numbertomonthname(month);
                var sdate = new DateTime(year, month, 1);
                var edate = sdate.AddMonths(1).AddDays(-1);
                List<Salary_em> list = new List<Salary_em>();
                var count = (from b in db.Salary_ems where b.empid == empid && b.year == year && b.month == month1 select b).Count();
                var cnt = db.Emp_attens.Where(a => a.empid == empid && a.Attendencedate >= sdate && a.Attendencedate <= edate && a.atten == "A").Count();
                var cntp = db.Emp_attens.Where(a => a.empid == empid && a.Attendencedate >= sdate && a.Attendencedate <= edate && a.atten == "P").Count();
                var ct = db.Emp_leaves.Where(a => a.empid == empid && a.sdate >= sdate && a.edate <= edate && a.leavetype == "Casual Leave").Count();
                var mt = db.Emp_leaves.Where(a => a.empid == empid && a.sdate >= sdate && a.edate <= edate && a.leavetype == a.leavetype).Count();
                if (count == 0)
                {
                    var br = db.Emp_Regs.Single(b => b.empid == empid);
                    var cc = db.Emp_Salarys.Single(b => b.empid == empid && b.year == year && b.month == month1);
                    //var ccw = db.Emp_Salarys.Single(b => b.empid == empid);
                    Salary_em tr = new Salary_em();
                    tr.year = cc.year;
                    tr.img = br.img;
                    tr.month = cc.month;
                    tr.empid = br.empid;
                    tr.name = br.name;
                    tr.designation = br.designation;
                    tr.department = br.department;
                    tr.doj = br.doj;
                    tr.date = DateTime.Now.Date;
                    tr.basicsalary = cc.basicsalary;
                    tr.HRA = cc.HRA;
                    tr.DA = cc.DA;
                    tr.CCA = cc.CCA;
                    tr.TA = cc.TA;
                    tr.medical = cc.medical;
                    var incentivesum = cc.HRA + cc.DA + cc.CCA + cc.TA + cc.medical;
                    tr.grossincentive = incentivesum;
                    tr.advance_Pay = cc.advance_Pay;
                    tr.professionaltax = cc.professionaltax;
                    tr.loan = cc.loan;
                    tr.provisional_fund = cc.provisional_fund;
                    var deductionamount = cc.advance_Pay + cc.professionaltax + cc.provisional_fund + cc.loan;
                    tr.deductionamount = deductionamount;
                    var perday_salary = br.basicsalary / 30;
                    var salamt = perday_salary * cntp;
                    //var asr = mt - ct;
                    //var asb = asr * perday_salary;
                    var countleave = cnt - ct;
                    var absent = perday_salary * countleave;
                    var netsalary = (salamt + tr.grossincentive) - (deductionamount + absent);
                    if (netsalary > 0)
                    {
                        tr.netsalary = netsalary;
                    }
                    else
                    {
                        tr.netsalary = 0;
                    }
                    db.Salary_ems.Add(tr);
                    db.SaveChanges();
                    list = db.Salary_ems.Where(b => b.empid == empid && b.year == year && b.month == month1).ToList();
                }

                else
                {
                    Response.Write("<script>alert('Please Fill Correct Empid or Month or Year')</script>");

                }


                return View(list);
            }
        }



        [HttpGet]
        public ActionResult DuplicateEmpSalary()
        {

            List<Salary_em> emp = new List<Salary_em>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {

                return View(emp);
            }
        }

        [HttpPost]
        public ActionResult DuplicateEmpSalary(Emp_Salary o, string empid, string month, int year)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                var count = (from b in db.Salary_ems where b.empid == empid && b.year == year && b.month == month select b).Count();
                if (count == 0)
                {

                    var br = db.Emp_Regs.Single(b => b.empid == empid);
                    var cc = db.Emp_Salarys.Single(b => b.empid == empid && b.year == year && b.month == month);
                    // var cc = db.Emp_Salarys.Single(b => b.empid == empid);

                    Salary_em tr = new Salary_em();
                    tr.year = cc.year;
                    tr.img = br.img;
                    tr.month = cc.month;
                    tr.empid = br.empid;
                    tr.name = br.name;
                    tr.designation = br.designation;
                    tr.department = br.department;
                    tr.doj = br.doj;
                    tr.date = DateTime.Now.Date;
                    tr.basicsalary = br.basicsalary;

                    tr.HRA = cc.HRA;
                    tr.DA = cc.DA;
                    tr.CCA = cc.CCA;
                    tr.TA = cc.TA;
                    tr.medical = cc.medical;
                    var incentivesum = cc.HRA + cc.DA + cc.CCA + cc.TA + cc.medical;

                    tr.grossincentive = incentivesum;
                    tr.advance_Pay = cc.advance_Pay;
                    tr.professionaltax = cc.professionaltax;
                    tr.loan = cc.loan;
                    tr.provisional_fund = cc.provisional_fund;
                    var deductionamount = cc.advance_Pay + cc.professionaltax + cc.provisional_fund + cc.loan;

                    tr.deductionamount = deductionamount;
                    var netsalary = (br.basicsalary + incentivesum) - deductionamount;
                    tr.netsalary = netsalary;

                    db.Salary_ems.Add(tr);
                    db.SaveChanges();
                }
                if (count == 1)
                {
                    var dn = db.Salary_ems.Where(c => c.empid == empid);
                    return View(dn);
                }
                else
                {
                    Response.Write("<script>alert('Please Fill Correct Empid or Month or Year')</script>");
                    return View();
                }


            }
        }
        [HttpGet]
        public ActionResult EmployeeList()
        {
            var hr = db.HRTabs.Single(h => h.HRId == User.Identity.Name);
            List<Emp_Reg> emplist = new List<Emp_Reg>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                emplist = (from dcl in db.Emp_Regs where dcl.branchcode == hr.BranchCode select dcl).ToList();
                return View(emplist);

            }
        }

        public ActionResult MonthlySalary(Salary_em o)
        {

            var dn = db.Salary_ems.Where(c => c.month == o.month);
            ViewData["count"] = dn.Count();
            return View(dn);

        }
        
        public ActionResult YearlySalary(Salary_em o)
        {

            var dn = db.Salary_ems.Where(c => c.year == o.year);
            ViewData["count"] = dn.Count();
            return View(dn);

        }
        [HttpGet]
        public ActionResult BranchWiseSalary()
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
        public ActionResult DepartmentWiseSalary()
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
        public ActionResult SearchHR()
        {

            List<Emp_Reg> dn = new List<Emp_Reg>();

            {
                dn = db.Emp_Regs.ToList();
            }
            return View(dn);

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
        public ActionResult empidwiselist(Emp_Reg o)
        {

            var dn = db.Emp_Regs.Where(c => c.empid == o.empid);

            ViewData["count"] = dn.Count();
            return View(dn);
        }
        public ActionResult empdepartmentwiselist(Emp_Reg o)
        {

            var dn = db.Emp_Regs.Where(c => c.department == o.department);
            ViewData["count"] = dn.Count();
            return View(dn);
        }
        public ActionResult empdesignationwiselist(Emp_Reg o)
        {

            var dn = db.Emp_Regs.Where(c => c.designation == o.designation);
            ViewData["count"] = dn.Count();
            return View(dn);
        }
        public ActionResult Logout()
        {
            int max = (from p in db.hrlogin_details where p.iid == User.Identity.Name select p.Id).Max();
            hrlogin_detail ob = db.hrlogin_details.Single(b => b.Id == max);
            ob.outdatetime = DateTime.Now;
            db.SaveChanges();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Home");
        }

        [HttpGet]
        public ActionResult display_attendance()
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
        public ActionResult display_attendance(int month=0,int year=0 )
        {

            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                var newdays = DateTime.DaysInMonth(year, month);
                DateTime firstofmonth = new DateTime(year, month, 1);
                DateTime lastofmonth = new DateTime(year,month, newdays);
                ViewData["sdate"] = firstofmonth;
                ViewData["edate"] = lastofmonth;
                var list = db.Database.SqlQuery<temp_empatten>("select empid,name from Emp_atten where Attendencedate>='" + firstofmonth + "' and Attendencedate<='" + lastofmonth + "'   group by empid,name").ToList();
                return View(list);
            }
        }
        public ActionResult showattendence(string empid, DateTime firstofmonth, DateTime lastofmonth)
        {
            TempData["id"] = empid;
            TempData["idd"] = firstofmonth;
            TempData["ii"] = lastofmonth;
            return View();
        }

        public ActionResult deleteattendence(DateTime firstofmonth, DateTime lastofmonth, int id = 0, int status = 0)
        {
            var empid = "";
            if (id > 0)
            {
                var attendetail = db.Emp_attens.Single(aa => aa.Id == id && aa.status == status);
                attendetail.status = 3;
                //db.Entry(attendetail).State = EntityState.Modified;
                db.Entry<Emp_atten>(attendetail).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                TempData["msg"] = "Deleted Successfully..!!";
                empid = attendetail.empid;
                
            }
            return RedirectToAction("showattendence", "HR", new { empid = empid, firstofmonth = firstofmonth, lastofmonth = lastofmonth });
        }

        public ActionResult Totalleave(string empid, string atten, int status, DateTime firstofmonth, DateTime lastofmonth)
        {
            TempData["id"] = empid;
            TempData["idd"] = atten;
            TempData["ad"] = status;
            TempData["md"] = firstofmonth;
            TempData["dm"] = lastofmonth;
            return View();
        }

        public JsonResult getemployeelist(DateTime sdate, DateTime edate, string empid)
        {
            var list = db.Emp_attens.Where(x => x.Attendencedate >= sdate && x.Attendencedate <= edate && x.empid == empid).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult Career()
        {

            return View();
        }
        [HttpPost]
        public ActionResult Career(careertable1 nt1)
        {
           var nt = new careertable1();
            nt.jobtype = nt1.jobtype;
            nt.jobdiscription = nt1.jobdiscription;
            nt.experience = nt1.experience;
            nt.industry = nt1.industry;
            nt.role = nt1.role;
            nt.functionalarea = nt1.functionalarea;
            nt.salary = nt1.salary;
            nt.Title = nt1.Title;
            nt.Keyword = "NA";
            nt.joblocationstate = nt1.joblocationstate;
            nt.city = nt1.city;
            nt.jobid = "JOB_"+gidd();
            nt.status = 1;
            nt.companyemail = nt1.companyemail;
            nt.publisdate = DateTime.Now.Date;
            nt.Fromdate = nt1.Fromdate;
            nt.Todate = nt1.Todate;
            nt.contact = nt1.contact;
            db.careertable1s.Add(nt);
            db.SaveChanges();
            Response.Write("<script>alert('Add jobs Successfully...')</script>");
            return View();
        }
        public ActionResult Displayjobs()
        {
            return View();
        }[HttpGet]
        public ActionResult Updatejobs()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Updatejobs(careertable1 nt1, string jobsid)
        {
            
            var nps = db.careertable1s.Single(x => x.jobid == jobsid);
            nps.jobtype = nt1.jobtype;
            nps.jobdiscription = nt1.jobdiscription;
            nps.experience = nt1.experience;
             nps.industry = nt1.industry;
            nps.role  = nt1.role;
            nps.functionalarea = nt1.functionalarea;
            nps.salary = nt1.salary;
            nps.Title = nt1.Title;
            //nps.Keyword = nt1.Keyword;
            nps.joblocationstate = nt1.joblocationstate;
            nps.city = nt1.city;
            nps.contact = nt1.contact;
            nps.companyemail = nt1.companyemail;
            //db.Entry(nps).State = EntityState.Modified;
            db.Entry<careertable1>(nps).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();
            Response.Write("<script>alert('Update Successfully..')</script>");
            return View();
        }
        [HttpGet]
        public ActionResult blockunblockjobs()
        {
            return View();
        }
        [HttpGet]
        public ActionResult Viewjobs()
        {

            return View();
        }
        public JsonResult Pickdata(string jobid)
        {
            var list = db.careertable1s.Where(x => x.jobid == jobid).ToList();
            return Json(list, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Holiday_leave()
        {
            var hltab = db.holidayleavetabs.ToList();
            return View(hltab);
        }
        [HttpPost]
        public ActionResult Holiday_leave(holidayleavetab nt1)
        {
            holidayleavetab nt = new holidayleavetab();
            nt.ldate = nt1.ldate;
            nt.cdate = DateTime.Now;
            nt.status = 1;
            db.holidayleavetabs.Add(nt);
            db.SaveChanges();

            return RedirectToAction("Holiday_leave","HR");
        }
        public ActionResult blacklist(int id, int status = 0)
        {
                var dd=0;
            if(id>0)
            {
                var list = db.holidayleavetabs.Single(x => x.id == id);
                list.status = status;
                //db.Entry(list).State = EntityState.Modified;
                db.Entry<holidayleavetab>(list).State = System.Data.Entity.EntityState.Modified;
                db.SaveChanges();
                dd = 1;
            }
            return Json(dd, JsonRequestBehavior.AllowGet);
        }
       
        [HttpGet]
        public ActionResult Add_leavetype()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Add_leavetype(Add_leavetab nt1)
        {
            Add_leavetab nt = new Add_leavetab();
            nt.leavetype = nt1.leavetype;
            db.Add_leavetabs.Add(nt);
            db.SaveChanges();
            var cnt = db.Add_leavetabs.ToList();
            return View(cnt);
        }
        [HttpGet]
        public ActionResult leave_type(leavetab at)
        {
            leavetab ab = new leavetab();
            ab.name = at.name;
            ab.leavetype = at.leavetype;
            db.leavetabs.Add(ab);
            db.SaveChanges();
            return View();
        }

        //public JsonResult block_unblockjobs(string jobids, int status = 0)
        //{
        //    List<birthdaywishes> mssg=new List<birthdaywishes>();
        //    var countlist = db.careertable1s.Where(x => x.jobid == jobids).Count();
        //    if(countlist!=0)
        //    {
        //    var list = db.careertable1s.Single(x => x.jobid == jobids);
        //    list.status = status;
        //    db.Entry(list).State = EntityState.Modified;
        //    db.SaveChanges();
        //        if(status==0)
        //        {
        //         mssg.Add(new birthdaywishes{
        //        sms="Jobs Blocked Successfully."
        //        });
        //        }
        //         if(status==1)
        //        {
        //         mssg.Add(new birthdaywishes{
        //        sms="Jobs Unblocked Successfully."
        //        });
        //        }
        //    }
        //    else
        //    {
        //        mssg.Add(new birthdaywishes{
        //        sms="This jobid does not exist"
        //        });
        //    }
        //    return Json(mssg, JsonRequestBehavior.AllowGet);
        //}

        //Amit Singh 1-03-2021
        public ActionResult mediastatus1(int opid = 0, int status = 0)
        {
                var getstatus = db.careertable1s.Single(x => x.Id == opid && x.status == status);
                if (status == 1)
                {
                    getstatus.status = 0;
                }
                else
                {
                    getstatus.status = 1;
                }
            //db.Entry(getstatus).State = EntityState.Modified;
            db.Entry<careertable1>(getstatus).State = System.Data.Entity.EntityState.Modified;
            db.SaveChanges();

                return RedirectToAction("blockunblockjobs", "HR");

        }
      
    }
}

