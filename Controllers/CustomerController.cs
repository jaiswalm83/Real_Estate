using ODHDEVELOPERS.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.IO;
using System.Net.NetworkInformation;
using CrystalDecisions.CrystalReports.Engine;

namespace ODHDEVELOPERS.Controllers
{
    public class CustomerController : Controller
    {
        //
        // GET: /Customer/
        SqlConnection con = new SqlConnection();
        UsersContext db = new UsersContext();

        public string NewBondid;
        public static string bond, acno;
        public static int lstsr = 0, ltsr = 0;

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
            List<appltab> ap = new List<appltab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                ap = db.appltabs.Where(apl => apl.newbondid == User.Identity.Name).ToList();
                return View(ap);
            }
        }

        [HttpPost]
        public ActionResult Bond(int a = 0)
        {
            List<appltab> ap = new List<appltab>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                ap = db.appltabs.Where(apl => apl.newbondid == User.Identity.Name).ToList();
                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "Customer.rpt"));
                rd.SetDataSource(ap);

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
                return View(ap);
            }
        }

        [HttpGet]
        public ActionResult RenewalReceipt()
        {
            List<Installment> rnl = new List<Installment>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Home");
            }
            else
            {
                var row = (from rn in db.Installmenttabs where rn.newbondid == User.Identity.Name && rn.payamount != 0 select rn).ToList();

                foreach (var h in row)
                {
                    rnl.Add(new Installment
                    {
                        Account_Code = h.newbondid,
                        rcptno = h.receiptno,
                        paymentdate = Convert.ToDateTime( h.paymentdate),
                        amount = h.payamount,
                        installno = h.installmentno,
                        year = h.year,
                        Planname = h.planname,
                        Term = h.term_plan,
                        mode = h.mode
                    });
                
                }

                return View(rnl);
            }
        }

        public ActionResult PrintRenewalReceipt()
        {
            List<Installment> rnl = new List<Installment>();
           
                var row = (from rn in db.Installmenttabs where rn.newbondid == User.Identity.Name && rn.payamount != 0 select rn).ToList();

                foreach (var h in row)
                {
                    rnl.Add(new Installment
                    {
                        Account_Code = h.newbondid,
                        rcptno = h.receiptno,
                        paymentdate = Convert.ToDateTime(h.paymentdate),
                        amount = h.payamount,
                        installno = h.installmentno,
                        year = h.year,
                        Planname = h.planname,
                        Term = h.term_plan,
                        mode = h.mode
                    });

                }


                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "CustomerRenewalList.rpt"));
                rd.SetDataSource(rnl);

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

                return View(rnl);
            
        }

        [HttpGet]
        public ActionResult CustumerPaidInstallment()
        {
            List<CustomerInstallment> ins = new List<CustomerInstallment>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Branch");
            }
            else
            {
                var bcount = (from vb in db.Installmenttabs  where vb.newbondid == User.Identity.Name select vb).Count();

                if (bcount > 0)
                {
                    var inst = (from vb in db.Installmenttabs where vb.newbondid == User.Identity.Name && vb.payamount != 0 select vb).ToList();

                    foreach (var item in inst.ToList())
                    {
                        var brow = db.appltabs.Single(po => po.newbondid == item.newbondid);

                        ins.Add(new CustomerInstallment
                        {
                            downpayment = brow.downpayment,
                            amount = item.amount,
                            payamount = item.payamount,
                            installmentno = item.installmentno,
                            year = item.year,
                            paymentdate = Convert.ToDateTime(item.paymentdate),
                            cssno = Convert.ToInt32(item.cssno),
                            receiptno = item.receiptno,
                            reliefrs = item.reliefrs,
                            newbondid = item.newbondid,
                            planname = item.planname,
                            term_plan = item.term_plan,
                            mode = item.mode,
                            branch = item.branch,
                            name = brow.name,
                            age = brow.age,
                            address = brow.addr,
                            nomname = brow.nomineename,
                            nomage = brow.nomage,
                            nomrel = brow.nomrel,
                            totalcon = brow.totalcon,
                            expectedvalue = brow.expectedraisablevalue,
                            expirydate = item.expirydate,
                            enddate = brow.expirydate

                        });


                    }
                }

                else
                {
                    Response.Write("<script>alert('Record Not Found for this Id')</script>");

                }
                return View(ins);
            }
        }

        public ActionResult PrintPaidInstallment()
        {
            List<CustomerInstallment> ins = new List<CustomerInstallment>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Branch");
            }
            else
            {
                var inst = (from vb in db.Installmenttabs where vb.newbondid == NewBondid && vb.payamount != 0 select vb).ToList();

                foreach (var item in inst.ToList())
                {
                    var brow = db.appltabs.Single(po => po.newbondid == item.newbondid);

                    ins.Add(new CustomerInstallment
                    {
                        downpayment = brow.downpayment,
                        amount = item.amount,
                        payamount = item.payamount,
                        installmentno = item.installmentno,
                        year = item.year,
                        paymentdate = Convert.ToDateTime(item.paymentdate),
                        cssno = Convert.ToInt32(item.cssno),
                        receiptno = item.receiptno,
                        reliefrs = item.reliefrs,
                        newbondid = item.newbondid,
                        planname = item.planname,
                        term_plan = item.term_plan,
                        mode = item.mode,
                        branch = item.branch,
                        name = brow.name,
                        age = brow.age,
                        address = brow.addr,
                        nomname = brow.nomineename,
                        nomage = brow.nomage,
                        nomrel = brow.nomrel,
                        totalcon = brow.totalcon,
                        expectedvalue = brow.expectedraisablevalue,
                        expirydate = item.expirydate,
                        enddate = brow.expirydate

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
        public ActionResult CustumerUnpaidInstallment()
        {
            List<CustomerInstallment> ins = new List<CustomerInstallment>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Branch");
            }
            else
            {
                var bcount = (from vb in db.Installmenttabs where vb.newbondid == User.Identity.Name select vb).Count();

                if (bcount > 0)
                {
                    var inst = (from vb in db.Installmenttabs where vb.newbondid == User.Identity.Name && vb.payamount == 0 select vb).ToList();

                    foreach (var item in inst.ToList())
                    {
                        var brow = db.appltabs.Single(po => po.newbondid == item.newbondid);

                        ins.Add(new CustomerInstallment
                        {
                            downpayment = brow.downpayment,
                            amount = item.amount,
                            payamount = item.payamount,
                            installmentno = item.installmentno,
                            year = item.year,
                            paymentdate = Convert.ToDateTime(item.paymentdate),
                            cssno = Convert.ToInt32(item.cssno),
                            receiptno = item.receiptno,
                            reliefrs = item.reliefrs,
                            newbondid = item.newbondid,
                            planname = item.planname,
                            term_plan = item.term_plan,
                            mode = item.mode,
                            branch = item.branch,
                            name = brow.name,
                            age = brow.age,
                            address = brow.addr,
                            nomname = brow.nomineename,
                            nomage = brow.nomage,
                            nomrel = brow.nomrel,
                            totalcon = brow.totalcon,
                            expectedvalue = brow.expectedraisablevalue,
                            expirydate = item.expirydate,
                            enddate = brow.expirydate

                        });


                    }
                }

                else
                {
                    Response.Write("<script>alert('Record Not Found for this Id')</script>");

                }
                return View(ins);
            }
        }

        public ActionResult PrintCustumerUnpaidInstallment()
        {
            List<CustomerInstallment> ins = new List<CustomerInstallment>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Branch");
            }
            else
            {
                var inst = (from vb in db.Installmenttabs where vb.newbondid == NewBondid && vb.payamount == 0 select vb).ToList();

                foreach (var item in inst.ToList())
                {
                    var brow = db.appltabs.Single(po => po.newbondid == item.newbondid);

                    ins.Add(new CustomerInstallment
                    {
                        downpayment = brow.downpayment,
                        amount = item.amount,
                        payamount = item.payamount,
                        installmentno = item.installmentno,
                        year = item.year,
                        paymentdate = Convert.ToDateTime(item.paymentdate),
                        cssno = Convert.ToInt32(item.cssno),
                        receiptno = item.receiptno,
                        reliefrs = item.reliefrs,
                        newbondid = item.newbondid,
                        planname = item.planname,
                        term_plan = item.term_plan,
                        mode = item.mode,
                        branch = item.branch,
                        name = brow.name,
                        age = brow.age,
                        address = brow.addr,
                        nomname = brow.nomineename,
                        nomage = brow.nomage,
                        nomrel = brow.nomrel,
                        totalcon = brow.totalcon,
                        expectedvalue = brow.expectedraisablevalue,
                        expirydate = item.expirydate,
                        enddate = brow.expirydate

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

        public ActionResult PassBookMenu()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Branch");
            }
            else
            {
                return View();
            }
        }






        [HttpGet]
        public ActionResult PrintPassBookDetail()
        {
            //var dc = db.appltabs.Single(c => c.newbondid == User.Identity.Name);
            //var dcc = db.SavingAccountInfos.Where(c => c.newmemberid == dc.newmemberid).Count();
            List<appltab> pass = new List<appltab>();

            //if (dcc > 0)
            //{

                var count = db.appltabs.Where(c => c.newbondid == User.Identity.Name && c.plantype == "RD").Count();
                if (count == 1)
                {
                    var pass1 = db.appltabs.Where(b => b.newbondid == User.Identity.Name && b.plantype == "RD").ToList();
                    return View(pass1);
                }
                else
                {
                    ViewBag.msg = "This Account No. Does Not Exist Or Account FD";


                }
            //}
            //else
            //{

            //    Response.Write("<script>alert('Sorry You don't have saving account .')</script>");

            //}

            return View(pass);
        }
      

        public ActionResult PDFPrintPassBookDetail()
        {

            //var dc = db.appltabs.Single(c => c.newbondid == User.Identity.Name);
            //var dcc = db.SavingAccountInfos.Where(c => c.newmemberid == dc.newmemberid).Count();
            List<appltab> pass = new List<appltab>();
            var count = db.appltabs.Where(c => c.newbondid == User.Identity.Name && c.plantype == "RD").Count();
            if (count == 1)
            {
                var pass1 = db.appltabs.Where(b => b.newbondid == User.Identity.Name && b.plantype == "RD").ToList();
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
        public ActionResult PrintSavingPassBookDetail()
        {
           
            var dd = db.appltabs.Single(c => c.newbondid == User.Identity.Name);
            var dc = db.SavingAccountInfos.Where(c => c.newmemberid == dd.newmemberid).Count();
          
            List<SavingAccountInfo> pass = new List<SavingAccountInfo>();
            if (dc > 0)
            {
                var df = db.SavingAccountInfos.Single(c => c.newmemberid == dd.newmemberid);
                var count = db.SavingAccountInfos.Where(c => c.accountno == df.accountno).Count();
                if (count == 1)
                {
                    var pass1 = db.SavingAccountInfos.Where(b => b.accountno == df.accountno).ToList();
                    return View(pass1);
                }
                else
                {
                    ViewBag.msg = "This Account No. Does Not Exist Or Account FD";
                   

                }
            }
            else
            {
                Response.Write("<script>alert('Sorry You don't have saving account .')</script>");
            }
            return View(pass);
        }

        public ActionResult PDFPrintSavingPassBookDetail(string accno)
        {
            List<SavingAccountInfo> pass = new List<SavingAccountInfo>();
            var count = db.SavingAccountInfos.Where(c => c.accountno == accno).Count();
            if (count == 1)
            {
                var pass1 = db.SavingAccountInfos.Where(b => b.accountno == accno).ToList();
                ReportDocument rd = new ReportDocument();
                rd.Load(Path.Combine(Server.MapPath("~/Reports"), "savingdetail.rpt"));
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
                return RedirectToAction("Logout", "Customer");
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
                return RedirectToAction("Logout", "Branch");
            }

            return View(print);
        }
        [HttpPost]
        public ActionResult PrintPassBook( int lastsr)
        {
            List<printpassbook> print = new List<printpassbook>();

            var a = db.appltabs.Where(f => f.newbondid == User.Identity.Name && f.plantype == "RD").Count();
            if (a == 1)
            {
                var bb = db.appltabs.Single(p => p.newbondid == User.Identity.Name);

                var count = db.Installmenttabs.Where(c => c.newbondid == User.Identity.Name && c.payamount != 0 && c.installmentno <= lastsr).Count();
                while (count > 0)
                {
                    print.Add(new printpassbook { sr = null, trandate = null, tranid = null, mode = null, status = null, installno = null, widthdraw = null, diposit = null, balance = null });

                    count = count - 1;
                }
                var install = db.Installmenttabs.Where(b => b.newbondid == User.Identity.Name && b.payamount != 0 && b.installmentno > lastsr).ToList();
                foreach (var r in install)
                {
                    var check = db.Installmenttabs.Where(s => s.newbondid == User.Identity.Name && s.installmentno == r.installmentno && s.payamount != 0 && s.receiptno == r.receiptno).Count();
                    if (check == 1)
                    {
                        var balance = (from o in db.Installmenttabs where o.installmentno <= r.installmentno && o.payamount != 0 && o.newbondid == User.Identity.Name select o.payamount).Sum();
                        var detail = db.Installmenttabs.Single(p => p.receiptno == r.receiptno && p.newbondid == User.Identity.Name && p.installmentno == r.installmentno);
                        print.Add(new printpassbook { sr = detail.installmentno.ToString(), trandate = detail.paymentdate.Value.ToString("dd/MM/yyyy"), tranid = detail.receiptno, mode = detail.mode, status = "1", installno = detail.installmentno.ToString(), widthdraw = null, diposit = detail.payamount.ToString(), balance = balance.ToString() });
                    }
                }
                bond = User.Identity.Name;
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

            List<printpassbook> print = new List<printpassbook>();
            var bb = db.appltabs.Single(p => p.newbondid == bond);


            var count = db.Installmenttabs.Where(c => c.newbondid == bond && c.payamount != 0 && c.installmentno <= lstsr).Count();
            while (count > 0)
            {
                print.Add(new printpassbook { sr = null, trandate = null, tranid = null, mode = null, status = null, installno = null, widthdraw = null, diposit = null, balance = null });

                count = count - 1;
            }
            var install = db.Installmenttabs.Where(b => b.newbondid == bond && b.payamount != 0 && b.installmentno > lstsr).ToList();
            foreach (var r in install)
            {
                var check = db.Installmenttabs.Where(s => s.newbondid == bond && s.installmentno == r.installmentno && s.payamount != 0 && s.receiptno == r.receiptno).Count();
                if (check == 1)
                {
                    var balance = (from o in db.Installmenttabs where o.installmentno <= r.installmentno && o.payamount != 0 && o.newbondid == bond select o.payamount).Sum();
                    var detail = db.Installmenttabs.Single(p => p.receiptno == r.receiptno && p.newbondid == bond && p.installmentno == r.installmentno);
                    print.Add(new printpassbook { sr = detail.installmentno.ToString(), trandate = detail.paymentdate.Value.ToString("dd/MM/yyyy"), tranid = detail.receiptno, mode = detail.mode, status = "1", installno = detail.installmentno.ToString(), widthdraw = null, diposit = detail.payamount.ToString(), balance = balance.ToString() });
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











        [HttpGet]
        public ActionResult SavingTransiction()
        {
            List<printpassbook> print = new List<printpassbook>();
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Logout", "Branch");
            }

            return View(print);
        }
        [HttpPost]
        public ActionResult SavingTransiction(int lastsr)
        {
            List<printpassbook> print = new List<printpassbook>();
            var dd = db.appltabs.Single(c => c.newbondid == User.Identity.Name);

            var dcc = db.SavingAccountInfos.Where(c => c.newmemberid == dd.newmemberid).Count();

            if (dcc > 0)
            {
                var dc = db.SavingAccountInfos.Single(c => c.newmemberid == dd.newmemberid);



                var count = db.TransactionTabs.Where(c => c.accountno == dc.accountno && c.status <= lastsr).Count();
                while (count > 0)
                {
                    print.Add(new printpassbook { sr = null, trandate = null, tranid = null, mode = null, status = null, installno = null, widthdraw = null, diposit = null, balance = null });

                    count = count - 1;
                }
                var install = db.TransactionTabs.Where(b => b.accountno == dc.accountno && b.status > lastsr).ToList();
                foreach (var r in install)
                {
                    print.Add(new printpassbook { sr = r.status.ToString(), trandate = r.pdate.ToString("dd/MM/yyyy"), tranid = r.transactionid, mode = null, status = r.status.ToString(), installno = r.status.ToString(), widthdraw = r.debit.ToString(), diposit = r.credit.ToString(), balance = r.balance.ToString() });

                }

                acno = dc.accountno;
                ltsr = lastsr;

            }
            else
            {
                @ViewBag.msg = "Sorry You don't have Transaction .";
            }
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
                print.Add(new printpassbook { sr = r.status.ToString(), trandate = r.pdate.ToString("dd/MM/yyyy"), tranid = r.transactionid, mode = r.actype, status = "1", installno = r.status.ToString(), widthdraw = r.debit.ToString(), diposit = r.credit.ToString(), balance = r.balance.ToString() });

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
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login", "Home");
        }

        public ActionResult printemiaggreement(string newid)
        {
            List<temp_aggrement_of_sell_EMI> list = new List<temp_aggrement_of_sell_EMI>();

            appltab appls=new appltab();
            string properttypes = string.Empty;
            string pymethods = string.Empty;
            string propertprefernce = string.Empty;
            DateTime chquedate=DateTime.Now;
            double chqamount = 0;
            string bankname = string.Empty;
            string chequeno = string.Empty;
            string transectionno = string.Empty;

           
           
            string transectionno1 = string.Empty;
            string transect_types = string.Empty;
            string projectname = string.Empty;

            string comprotype = string.Empty;
            string blocktypes = string.Empty;

            string plocorner = string.Empty;
            string normaltype = string.Empty;
            string parkingfashe = string.Empty;
            string terty_ft = string.Empty;
            string fourty_ft = string.Empty;
            string sixty_ft = string.Empty;

            Double Basiccost = 0;
            Double plotcosts = 0;
            Double outsourcing = 0;
            Double totalcon = 0;
            Double dev_charge = 0;
            Double totalplot_val = 0;
            string transectdate2 = string.Empty;
            string transectdate22 = string.Empty;
       string pymethods1  = string.Empty;
       string pymethods2  = string.Empty;
       string pymethods3  = string.Empty;

       string pymethods11 = string.Empty;
       string chquedate1 = string.Empty;
       
       string chequeno1  = string.Empty;
       string bankname1 = string.Empty;
       double chqamount1=0;
       string bookingdate1 = string.Empty;
       string agreementdate1 = string.Empty;

       string pymethods22 = string.Empty;
       string chquedate2 = string.Empty;
       string chequeno2  = string.Empty;
       string bankname2 = string.Empty;
       double chqamount2 =0;
       string bookingdate2 = string.Empty;
       string agreementdate2 = string.Empty;

       string pymethods33 = string.Empty;
       string chquedate3 = string.Empty;
       string chequeno3 = string.Empty;
       string bankname3 = string.Empty;
       double chqamount3 = 0;
       string bookingdate3 = string.Empty;
       string nameprifix = string.Empty;
       string usernameprifix = string.Empty;
       DateTime EMIStartDate = DateTime.Now;
       DateTime EMIEndDate = DateTime.Now;
       double outstanding1 = 0;
       string receptno3 = string.Empty;
       string receptno1 = string.Empty;
       string receptno2 = string.Empty;
       string receptno4 = string.Empty;
       string sdatee = string.Empty;
       string edatee = string.Empty;
            var appllist = db.appltabs.Single(x => x.newbondid == newid);
            var projectnames = db.Plans.Single(x => x.Plancode == appllist.projectid);

            projectname = projectnames.Planname;
            var plotvalue = db.RateTabs.FirstOrDefault(t => t.projectid == appllist.projectid && t.plotsize == 50).plotcost;
            plotcosts = plotvalue;
            var rp = appllist.nolandunit / 50;
            totalcon = plotvalue * rp;
            dev_charge = (appllist.totalcon*100) / totalcon;

            outsourcing = appllist.totalcon - appllist.bookingamount;

            Basiccost = appllist.nolandunit;
            var startdate = db.Database.SqlQuery<DateTime>("select top 1 expirydate from Installmenttab where newbondid='" + newid + "'").FirstOrDefault();
            var enddate = db.Database.SqlQuery<DateTime>("select top 1 expirydate from Installmenttab where newbondid='"+newid+"' order by id desc").FirstOrDefault();
            var counttuner = db.Installmenttabs.Where(x => x.newbondid == newid).Count();
            var installment = db.Installmenttabs.FirstOrDefault(x => x.newbondid == newid && x.bank!=null && x.mode!=null && x.amount!=0);
            var gender=db.Member_tabs.Single(x=>x.MemberId==appllist.memberid).Gender;
            if (gender == "Male")
            {
                usernameprifix = "Mr.";
            }
            if (gender == "Female")
            {
                usernameprifix = "Mrs.";
            }
            if (appllist.propertytype == "Residential")
            {
                properttypes = "Y";
                comprotype = "N";
                pymethods11 = "Residential";
                
            }
            if (appllist.propertytype == "Commercial")
            {
                comprotype = "Y";
                properttypes = "N";
                
                pymethods11 = "Commercial";
            }
            if (appllist.propertypreference == "Corner")
            {
                plocorner = "Y";
            }                                                     
            if (appllist.propertypreference == "Normal")
            {
                normaltype = "Y";
            }
            if (appllist.propertypreference == "Park Facing")
            {
                parkingfashe = "Y";
            }
            if (appllist.propertypreference == "30ft Road")
            {
                terty_ft = "Y";
            }
            if (appllist.propertypreference == "40ft Road")
            {
                fourty_ft = "Y";
            }
            if (appllist.propertypreference == "60ft Road")
            {
                sixty_ft = "Y";
            }
            if (appllist.paymethod == "Cash")
            {
                transect_types = "Cash";
                bankname = "";
            }
            if (appllist.paymethod == "Cheque")
            {
                transect_types = "Cheque";
                chquedate = Convert.ToDateTime(installment.Chequedate);
                chequeno = installment.chequeno;
                bankname = "Bank Name " + installment.bank;
                chqamount = Convert.ToDouble(installment.ChequeAmount);
            }
            if (appllist.paymethod == "DD")
            {
                transect_types = "DD";
                chquedate = Convert.ToDateTime(installment.Chequedate);
                chequeno = installment.chekddno;
                bankname = "Bank Name " + installment.drawno;
                chqamount = installment.amount;
                
            }
            if (appllist.paymethod == "banktransaction")
            {
                transect_types = "Banktransaction";
                chquedate = Convert.ToDateTime(installment.Chequedate);
                chequeno =  installment.chequeno;
                bankname = "Bank Name " + installment.bank;
                chqamount = Convert.ToDouble(installment.ChequeAmount);
                transectdate2 = Convert.ToDateTime(installment.Chequedate).ToString("dd/MM/yyyy");
                transectdate22 = installment.chequeno; 

            }
            if (appllist.paymenttype == "FULL")
            {
                pymethods1 = "Y";
                chquedate1 = Convert.ToDateTime(installment.Chequedate).ToString("dd/MM/yyyy");
                chequeno1 = installment.chequeno;
                bankname1 = installment.bank;
                chqamount1 = Convert.ToDouble(installment.ChequeAmount);
                bookingdate1 = Convert.ToDateTime(appllist.pdate).ToString("dd/MM/yyyy");
                agreementdate1 = Convert.ToDateTime(appllist.formdate).ToString("dd/MM/yyyy");
                receptno1 = installment.receiptno;
            }
            if (appllist.paymenttype == "PART")
            {   
                pymethods2 = "Y";
                chquedate2 = Convert.ToDateTime(installment.Chequedate).ToString("yyyy/MM/dd");
                chequeno2 = installment.chequeno;
                bankname2 = installment.bank;
                chqamount2 = Convert.ToDouble(installment.ChequeAmount);
                bookingdate2 = Convert.ToDateTime(appllist.pdate).ToString("dd/MM/yyyy");
                agreementdate2 = Convert.ToDateTime(appllist.formdate).ToString("dd/MM/yyyy");
                receptno2 = installment.receiptno;
            }
            if (appllist.paymenttype == "EMI")
            {
                pymethods3 = "Y";
                chquedate3 = Convert.ToDateTime(installment.Chequedate).ToString("dd/MM/yyyy");
                chequeno3 = installment.chequeno;
                bankname3 = installment.bank;
                chqamount3 = Convert.ToDouble(installment.ChequeAmount);
                bookingdate3 = Convert.ToDateTime(appllist.pdate).ToString("dd/MM/yyyy");
                nameprifix = Convert.ToDateTime(appllist.formdate).ToString("dd/MM/yyyy");
                outstanding1 = outsourcing;
                receptno3=counttuner+" Months";
                sdatee = Convert.ToDateTime(startdate).ToString("dd/MM/yyyy");
                edatee = Convert.ToDateTime(enddate).ToString("dd/MM/yyyy");
                receptno4 = installment.receiptno;
            }

            totalplot_val = (appllist.totalcon + appllist.plccost + dev_charge)- appllist.discountper;
            var comp=db.CompanyInfos.Single(x=>x.Id==1);
            list.Add(new temp_aggrement_of_sell_EMI { 
                    aggrementdate=appllist.formdate,
                    compnay_name=comp.CompanyName,
                    compnay_add=comp.Address,
                    customer=appllist.name,
                    rel=appllist.RelationOf,
                    address=appllist.addr,
                    allot_address = appllist.propertyaddress,
                    summof_rs = appllist.totalcon,
                    bookingamnt=appllist.bookingamount,
                    paymethod=appllist.paymethod,
                    chequeno = chequeno,
                    bankname = bankname,
                    chequedate = chquedate,
                    chequeamount = chqamount,
                    nolandunit=appllist.nolandunit,
                    projectname = projectname,
                    plotno=appllist.plotno,
                    phase=appllist.phase,
                    plottype = properttypes,
                    blockno=appllist.block,
                    Preferenceplot = propertprefernce,
                    plotvalue = plotcosts,
                    pefrenc_loctaion = appllist.plccost,
                    dev_charge=dev_charge,
                    discount=appllist.discountper,
                    totalplotvalue = totalplot_val,
                    pymethodss= pymethods,
                    plotdescription=appllist.plotdesp,
                    SOUTHEAST=appllist.southeast,
                    SOUTHWEST=appllist.southwest,
                    NORTHEAST=appllist.northeast,
                    NORTHWEST=appllist.northwest,
                    Outstanding = outsourcing,
                    empty9 = transect_types,
                    complotype=comprotype,
                    normaltype=normaltype,
                    parkingfashe=parkingfashe,
                    terty_ft=terty_ft,
                    fourty_ft=fourty_ft,
                    sixty_ft=sixty_ft,
                    plocorner=plocorner,
                    saleprice = plotvalue,
                    basiccost=Basiccost,
                    pymethods1 =pymethods1,
                    chquedate1 =chquedate1,
                    chequeno1 = chequeno1,
                    bankname1 =  bankname1,
                    chqamount1 = chqamount1,
                    bookingdate1 = bookingdate1,
                    agreementdate1 = agreementdate1,
                    pymethods2 = pymethods2,
                    chquedate2 = chquedate2,
                    chequeno2 = chequeno2,
                    bankname2 = bankname2,
                    chqamount2 = chqamount2,
                    bookingdate2 = bookingdate2,
                    agreementdate2 = agreementdate2,

                    pymethods3 = pymethods3,
                    chquedate3 = chquedate3,
                    chequeno3 = chequeno3,
                    bankname3 = bankname3,
                    chqamount3 = chqamount3,
                    bookingdate3 = bookingdate3,
                    agreementdate3 = nameprifix,
                    transectdate2=transectdate2,
                    transectdate22=transectdate22,
                    Tenure = receptno3,
                    EMIPerMonth=appllist.payment,
                    EMIStartDate=sdatee,
                    EMIEndDate=edatee,
                    outstanding1=outstanding1,
                    empty10=usernameprifix,
                    bookinrecipt = installment.receiptno,
                    receptno1=receptno1,
                    receptno2=receptno2,
                    receptno4=receptno4

            });


            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "EMIAgriment.rpt"));
            rd.SetDataSource(list);

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


        public ActionResult printemiaggreementPART(string newid)
        {
            List<temp_aggrement_of_sell_EMI> list = new List<temp_aggrement_of_sell_EMI>();

            appltab appls = new appltab();
            string properttypes = string.Empty;
            string pymethods = string.Empty;
            string propertprefernce = string.Empty;
            DateTime chquedate = DateTime.Now;
            double chqamount = 0;
            string bankname = string.Empty;
            string chequeno = string.Empty;
            string transectionno = string.Empty;



            string transectionno1 = string.Empty;
            string transect_types = string.Empty;
            string projectname = string.Empty;

            string comprotype = string.Empty;
            string blocktypes = string.Empty;

            string plocorner = string.Empty;
            string normaltype = string.Empty;
            string parkingfashe = string.Empty;
            string terty_ft = string.Empty;
            string fourty_ft = string.Empty;
            string sixty_ft = string.Empty;

            Double Basiccost = 0;
            Double plotcosts = 0;
            Double outsourcing = 0;
            Double totalcon = 0;
            Double dev_charge = 0;
            Double totalplot_val = 0;
            Double outstanding_part = 0;
            string transectdate2 = string.Empty;
            string transectdate22 = string.Empty;
            string pymethods1 = string.Empty;
            string pymethods2 = string.Empty;
            string pymethods3 = string.Empty;

            string pymethods11 = string.Empty;
            string chquedate1 = string.Empty;

            string chequeno1 = string.Empty;
            string bankname1 = string.Empty;
            double chqamount1 = 0;
            string bookingdate1 = string.Empty;
            string agreementdate1 = string.Empty;

            string pymethods22 = string.Empty;
            string chquedate2 = string.Empty;
            string chequeno2 = string.Empty;
            string bankname2 = string.Empty;
            double chqamount2 = 0;
            string bookingdate2 = string.Empty;
            string agreementdate2 = string.Empty;

            string pymethods33 = string.Empty;
            string chquedate3 = string.Empty;
            string chequeno3 = string.Empty;
            string bankname3 = string.Empty;
            double chqamount3 = 0;
            string bookingdate3 = string.Empty;
            string agreementdate3 = string.Empty;
            DateTime EMIStartDate = DateTime.Now;
            DateTime EMIEndDate = DateTime.Now;
            double outstanding1 = 0;
            string tunner = string.Empty;
            string usernameprifix = string.Empty;
            string sdatee = string.Empty;
            string edatee = string.Empty;

            string receptno1 = string.Empty;
            string receptno2 = string.Empty;
            string receptno4 = string.Empty;
            var appllist = db.appltabs.Single(x => x.newbondid == newid);
            var projectnames = db.Plans.Single(x => x.Plancode == appllist.projectid);

            projectname = projectnames.Planname;
            var plotvalue = db.RateTabs.FirstOrDefault(t => t.projectid == appllist.projectid && t.plotsize == 50).plotcost;
            plotcosts = plotvalue;
            var rp = appllist.nolandunit / 50;
            totalcon = plotvalue * rp;
            dev_charge = (appllist.totalcon * 100) / totalcon;

            outsourcing = appllist.totalcon - appllist.bookingamount;
            outstanding_part = appllist.totalcon - appllist.bookingamount;
            Basiccost = appllist.nolandunit;
            var startdate = db.Database.SqlQuery<DateTime>("select top 1 expirydate from Installmenttab where newbondid='" + newid + "'").FirstOrDefault();
            var enddate = db.Database.SqlQuery<DateTime>("select top 1 expirydate from Installmenttab where newbondid='" + newid + "' order by id desc").FirstOrDefault();
            var counttuner = db.Installmenttabs.Where(x => x.newbondid == newid).Count();
            var installment = db.Installmenttabs.FirstOrDefault(x => x.newbondid == newid && x.bank != null && x.mode != null && x.amount != 0);
            var gender = db.Member_tabs.Single(x => x.MemberId == appllist.memberid).Gender;
            if (gender == "Male")
            {
                usernameprifix = "Mr.";
            }
            if (gender == "Female")
            {
                usernameprifix = "Mrs.";
            }
            if (appllist.propertytype == "Residential")
            {
                properttypes = "Y";
                comprotype = "N";
                pymethods11 = "Residential";

            }
            if (appllist.propertytype == "Commercial")
            {
                comprotype = "Y";
                properttypes = "N";

                pymethods11 = "Commercial";
            }
            if (appllist.propertypreference == "Corner")
            {
                plocorner = "Y";
            }
            if (appllist.propertypreference == "Normal")
            {
                normaltype = "Y";
            }
            if (appllist.propertypreference == "Park Facing")
            {
                parkingfashe = "Y";
            }
            if (appllist.propertypreference == "30ft Road")
            {
                terty_ft = "Y";
            }
            if (appllist.propertypreference == "40ft Road")
            {
                fourty_ft = "Y";
            }
            if (appllist.propertypreference == "60ft Road")
            {
                sixty_ft = "Y";
            }
            if (appllist.paymethod == "Cash" || appllist.paymethod == "cash" || appllist.paymethod == "CASH")
            {
                transect_types = "Cash";
                bankname = "";
            }
            if (appllist.paymethod == "Cheque")
            {
                transect_types = "Cheque No.";
                chquedate = Convert.ToDateTime(installment.Chequedate);
                chequeno = installment.chequeno;
                bankname = "Bank Name "+installment.bank;
                chqamount = Convert.ToDouble(installment.ChequeAmount);
            }
            if (appllist.paymethod == "DD")
            {
                transect_types = "DD No.";
                chquedate = Convert.ToDateTime(installment.Chequedate);
                chequeno = installment.chekddno;
                bankname = "Bank Name " + installment.drawno;
                chqamount = installment.amount;

            }
            if (appllist.paymethod == "banktransaction")
            {
                transect_types = "Banktransaction No.";
                chquedate = Convert.ToDateTime(installment.Chequedate);
                chequeno = installment.chequeno;
                bankname = "Bank Name " + installment.bank;
                chqamount = Convert.ToDouble(installment.ChequeAmount);
                transectdate2 = Convert.ToDateTime(installment.Chequedate).ToString("dd/MM/yyyy");
                transectdate22 = installment.chequeno;

            }
            if (appllist.paymenttype == "FULL")
            {
                pymethods1 = "Y";
                chquedate1 = Convert.ToDateTime(installment.Chequedate).ToString("dd/MM/yyyy");
                chequeno1 = installment.chequeno;
                bankname1 = installment.bank;
                chqamount1 = Convert.ToDouble(installment.ChequeAmount);
                bookingdate1 = Convert.ToDateTime(appllist.pdate).ToString("dd/MM/yyyy");
                agreementdate1 = Convert.ToDateTime(appllist.formdate).ToString("dd/MM/yyyy");
                receptno1 = installment.receiptno;
            }
            if (appllist.paymenttype == "PART")
            {
                pymethods2 = "Y";
                chquedate2 = Convert.ToDateTime(installment.Chequedate).ToString("dd/MM/yyyy");
                chequeno2 = installment.chequeno;
                bankname2 = installment.bank;
                chqamount2 = Convert.ToDouble(installment.ChequeAmount);
                bookingdate2 = Convert.ToDateTime(appllist.pdate).ToString("dd/MM/yyyy");
                agreementdate2 = Convert.ToDateTime(appllist.formdate).ToString("dd/MM/yyyy");
                receptno2 = installment.receiptno;
            }
            if (appllist.paymenttype == "EMI")
            {
                pymethods3 = "Y";
                chquedate3 = Convert.ToDateTime(installment.Chequedate).ToString("dd/MM/yyyy");
                chequeno3 = installment.chequeno;
                bankname3 = installment.bank;
                chqamount3 = Convert.ToDouble(installment.ChequeAmount);
                bookingdate3 = Convert.ToDateTime(appllist.pdate).ToString("dd/MM/yyyy");
                agreementdate3 = Convert.ToDateTime(appllist.formdate).ToString("dd/MM/yyyy");
                outstanding1 = outsourcing;
                tunner = counttuner + " Months";
                sdatee = Convert.ToDateTime(startdate).ToString("dd/MM/yyyy");
                edatee = Convert.ToDateTime(enddate).ToString("dd/MM/yyyy");
                receptno4 = installment.receiptno;
            }

            totalplot_val = (appllist.totalcon + appllist.plccost + dev_charge) - appllist.discountper;
            var comp = db.CompanyInfos.Single(x => x.Id == 1);
            list.Add(new temp_aggrement_of_sell_EMI
            {
                aggrementdate = appllist.formdate,
                compnay_name = comp.CompanyName,
                compnay_add = comp.Address,
                customer = appllist.name,
                rel = appllist.RelationOf,
                address = appllist.addr,
                allot_address = appllist.propertyaddress,
                summof_rs = appllist.totalcon,
                bookingamnt = appllist.bookingamount,
                paymethod = appllist.paymethod,
                chequeno = chequeno,
                bankname = bankname,
                chequedate = chquedate,
                chequeamount = chqamount,
                nolandunit = appllist.nolandunit,
                projectname = projectname,
                plotno = appllist.plotno,
                phase = appllist.phase,
                plottype = properttypes,
                blockno = appllist.block,
                Preferenceplot = propertprefernce,
                plotvalue = plotcosts,
                pefrenc_loctaion = appllist.plccost,
                dev_charge = dev_charge,
                discount = appllist.discountper,
                totalplotvalue = totalplot_val,
                pymethodss = pymethods,
                plotdescription = appllist.plotdesp,
                SOUTHEAST = appllist.southeast,
                SOUTHWEST = appllist.southwest,
                NORTHEAST = appllist.northeast,
                NORTHWEST = appllist.northwest,
                Outstanding = outsourcing,
                empty9 = transect_types,
                complotype = comprotype,
                normaltype = normaltype,
                parkingfashe = parkingfashe,
                terty_ft = terty_ft,
                fourty_ft = fourty_ft,
                sixty_ft = sixty_ft,
                plocorner = plocorner,
                saleprice = plotvalue,
                basiccost = Basiccost,
                pymethods1 = pymethods1,
                chquedate1 = chquedate1,
                chequeno1 = chequeno1,
                bankname1 = bankname1,
                chqamount1 = chqamount1,
                bookingdate1 = bookingdate1,
                agreementdate1 = agreementdate1,
                pymethods2 = pymethods2,
                chquedate2 = chquedate2,
                chequeno2 = chequeno2,
                bankname2 = bankname2,
                chqamount2 = chqamount2,
                bookingdate2 = bookingdate2,
                agreementdate2 = agreementdate2,

                pymethods3 = pymethods3,
                chquedate3 = chquedate3,
                chequeno3 = chequeno3,
                bankname3 = bankname3,
                chqamount3 = chqamount3,
                bookingdate3 = bookingdate3,
                agreementdate3 = agreementdate3,
                transectdate2 = transectdate2,
                transectdate22 = transectdate22,
                Tenure = tunner,
                EMIPerMonth = appllist.payment,
                EMIStartDate = sdatee,
                EMIEndDate = edatee,
                outstanding1 = outstanding1,
                dateofbooking=appllist.pdate,
                empty3 = outstanding_part,
                empty10=usernameprifix,
                bookinrecipt=installment.receiptno,
                receptno1 = receptno1,
                receptno2 = receptno2,
                receptno4 = receptno4
                

            });


            ReportDocument rd = new ReportDocument();
            rd.Load(Path.Combine(Server.MapPath("~/Reports"), "EMIAgriment_PART.rpt"));
            rd.SetDataSource(list);

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
