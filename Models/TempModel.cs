using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;

namespace ODHDEVELOPERS.Models
{

    #region 20-May-2019

    public class AdvisorCollection
    {

        public string companyname { get; set; }
        public string companyaddress { get; set; }
        public string companyemail { get; set; }
        public string companydomain { get; set; }
        public string companycontact { get; set; }
        public string opid { get; set; }
        public string reportname { get; set; }
        public string reportduration { get; set; }
        public string branch { get; set; }
        public string advisorid { get; set; }
        public string advisorname { get; set; }
        public string custid { get; set; }
        public string custname { get; set; }
        public string planname { get; set; }
        public double freshamount { get; set; }
        public double first { get; set; }
        public double second { get; set; }
        public double third { get; set; }
        public double fourth { get; set; }
        public double fifth { get; set; }
        public double sixth { get; set; }
        public double seventh { get; set; }
        public double businessvalue { get; set; }
        public double total { get; set; }
        public string type { get; set; }
    }

    public class AllCollection
    {
        public int year { get; set; }
        public int plancode { get; set; }
        public int installmentno { get; set; }
        public double payamount { get; set; }
        public string plantype { get; set; }
        public string planname { get; set; }
        public string newbondid { get; set; }
        public string name { get; set; }
    }

    public class AutoCompleteAgent
    {
        public string NewAgentId { get; set; }
        public string name { get; set; }
    }

    #endregion 20-May-2019
    public class newtab
    {
        public string VisitorName { get; set; }
        public string mobileno { get; set; }
        public string email { get; set; }
        public string  address { get; set; }
    }
    public class Dashboard
    {
        public double todaybooking { get; set; }
        public double todayRenewalcoll { get; set; }
        public double todayadvance { get; set; }
        public double TodayLateFee { get; set; }
        public double TodayReleaf { get; set; }
        public double todayappfee { get; set; }
        public double todayagfee { get; set; }
        public double todaymemberfee { get; set; }
        public double todayspot { get; set; }
        public double todayvoucher { get; set; }
        public double todayexpense { get; set; }
        public double todaybalance { get; set; }
        public double yesterdaybooking { get; set; }
        public double yesterdayRenewalcoll { get; set; }
        public double yesterdayadvance { get; set; }
        public double yesterdayLateFee { get; set; }
        public double yesterdayReleaf { get; set; }
        public double yesterdayappfee { get; set; }
        public double yesterdayagfee { get; set; }
        public double yesterdaymemberfee { get; set; }
        public double yesterdayspot { get; set; }
        public double yesterdayvoucher { get; set; }
        public double yesterdayexpense { get; set; }
        public double yesterdaybalance { get; set; }
        public double monthlybooking { get; set; }
        public double monthlyRenewalcoll { get; set; }
        public double monthlyadvance { get; set; }
        public double monthlyLateFee { get; set; }
        public double monthlyReleaf { get; set; }
        public double monthlyappfee { get; set; }
        public double monthlyagfee { get; set; }
        public double monthlymemberfee { get; set; }
        public double monthlyspot { get; set; }
        public double monthlyvoucher { get; set; }
        public double monthlyexpense { get; set; }
        public double monthlybalance { get; set; }
        public double totalbooking { get; set; }
        public double totalRenewalcoll { get; set; }
        public double totaladvance { get; set; }
        public double totalLateFee { get; set; }
        public double totalReleaf { get; set; }
        public double totalappfee { get; set; }
        public double totalagfee { get; set; }
        public double totalmemberfee { get; set; }
        public double totalspot { get; set; }
        public double totalvoucher { get; set; }
        public double totalexpense { get; set; }
        public double totalbalance { get; set; }
    }

    public class birthdaywishes
    {
        public string sms { get; set; }
    }
    public class temp_empatten
    {
        public string empid { get; set; }
        public string name { get; set; }
    }
    public class temp_aggrement_of_sell_EMI
    {
        public DateTime aggrementdate { get; set; }
        public string compnay_name   { get; set; }
        public string compnay_add { get; set; }
        public string comp_contact { get; set; }
        public string comp_url { get; set; }
        public string customer { get; set; }
        public string rel { get; set; }
        public string relation { get; set; }
        public string address { get; set; }
        public string allot_address { get; set; }
        public double summof_rs { get; set; }
        public string amountinword { get; set; }
        public double bookingamnt { get; set; }
        public double chequeamount { get; set; }
        public double nolandunit { get; set; }
        public double plotvalue { get; set; }
        public string paymethod { get; set; }
        public string chequeno { get; set; }
        public string bankname { get; set; }
        public DateTime chequedate { get; set; }
        public string yearterm { get; set; }
        public int term { get; set; }
        public string plotarea { get; set; }
        public string projectname { get; set; }
        public int plotno { get; set; }
        public string phase { get; set; }
        public string plottype { get; set; }
        public string residence { get; set; }
        public string comercial { get; set; }
        public string blockno { get; set; }
        public double pefrenc_loctaion { get; set; }
        public double dev_charge { get; set; }
        public double discount { get; set; }
        public double totalplotvalue { get; set; }
        public string plotdimensions { get; set; }
        public string pymethodss { get; set; }
        public string plotdescription { get; set; }
        public string plotdiamen1 { get; set; }
        public string plotdiamen2 { get; set; }
        public string plotdiamen3 { get; set; }
        public string Preferenceplot { get; set; }
        public string Normal { get; set; }
        public string Corner { get; set; }
        public string ParkFacing { get; set; }
        public string WideRoad1 { get; set; }
        public string WideRoad2 { get; set; }
        public string WideRoad3 { get; set; }
        public DateTime dateofbooking { get; set; }
        public string mode { get; set; }
        public string transetionno { get; set; }
        public DateTime transectiondate { get; set; }
        public string bookinrecipt { get; set; }
        public string fullpay { get; set; }
        public string partpay { get; set; }
        public double Outstanding { get; set; }
        public string PDCCheq1 { get; set; }
        public string PDCCheq2 { get; set; }
        public string PDCCheq3 { get; set; }
        public string PDCCheq4 { get; set; }

        public DateTime PDCCheq_1 { get; set; }
        public DateTime PDCCheq_2 { get; set; }
        public DateTime PDCCheq_3 { get; set; }
        public DateTime PDCCheq_4 { get; set; }

        public string Tenure { get; set; }
        public double EMIPerMonth  { get; set; }
        public string EMIStartDate { get; set; }
        public string EMIEndDate { get; set; }

        public string SOUTHEAST  { get; set; }
        public string NORTHWEST  { get; set; }
        public string NORTHEAST { get; set; }
        public string SOUTHWEST { get; set; }

        public double saleprice { get; set; }
        public double empty2{ get; set; }
        public double empty3 { get; set; }
        public double empty4 { get; set; }

        public int empty5 { get; set; }
        public int empty6 { get; set; }
        public int empty7 { get; set; }
        public int empty8 { get; set; }

        public string empty9 { get; set; }
        public string empty10 { get; set; }
        public string empty11 { get; set; }
        public string empty12 { get; set; }
        public string complotype { get; set; }
        public string blocktype { get; set; }
        public string plocorner { get; set; }
        public string normaltype { get; set; }
        public string parkingfashe { get; set; }
        public string terty_ft { get; set; }
        public string fourty_ft { get; set; }
        public string sixty_ft { get; set; }

        public double basiccost { get; set; }

        public string pymethods1 { get; set; }
        public string pymethods2 { get; set; }
        public string pymethods3 { get; set; }

        public string pymethods11 { get; set; }
        public string chquedate1 { get; set; }
        public string chequeno1 { get; set; }
        public string bankname1 { get; set; }
        public double chqamount1 { get; set; }
        public string bookingdate1 { get; set; }
        public string agreementdate1 { get; set; }

        public string pymethods22 { get; set; }
        public string chquedate2 { get; set; }
        public string chequeno2 { get; set; }
        public string bankname2 { get; set; }
        public double chqamount2 { get; set; }
        public string bookingdate2 { get; set; }
        public string agreementdate2 { get; set; }

        public string pymethods33 { get; set; }
        public string chquedate3 { get; set; }
        public string chequeno3 { get; set; }
        public string bankname3 { get; set; }
        public double chqamount3 { get; set; }
        public string bookingdate3 { get; set; }
        public string agreementdate3 { get; set; }

        public string transectdate2 { get; set; }
        public string transectdate22 { get; set; }
        public double outstanding1 { get; set; }

        public string receptno1 { get; set; }
        public string receptno2 { get; set; }
        public string receptno4 { get; set; }

      }

    public class bithdaylist
    {

        public string memberid { get; set; }
        public string membername { get; set; }
        public string mobile { get; set; }
        [DataType(DataType.Date)]
        public DateTime dob { get; set; }
        public string rankname { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
          
            

    }
    public class TempGullak
    {
        public string newbondid { get; set; }
        public string custname { get; set; }
        public string mobile { get; set; }
        public string age { get; set; }
        public string address { get; set; }
        public string nomname { get; set; }
        public string nomage { get; set; }
        public string nomrel { get; set; }
        public string transactionid { get; set; }
        public int installmentno { get; set; }
        [DataType(DataType.Date)]
        public DateTime pdate { get; set; }
        public string paymethod { get; set; }
        public string checkorddno { get; set; }
        public string drawon { get; set; }
        public Double credit { get; set; }
        public Double debit { get; set; }
        public Double balance { get; set; }
        public string remark { get; set; }
        public string type { get; set; }
        public string companyname { get; set; }
        public string branchname { get; set; }
        public string planname { get; set; }
        public Double term { get; set; }
        public string mode { get; set; }
        public Double totalcon { get; set; }
        public Double expectedvalue { get; set; }
        [DataType(DataType.Date)]
        public DateTime formdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime maturitydate { get; set; }
    }

    public class RemainingCashCollection
    {

        [DataType(DataType.Date)]
        public DateTime date { get; set; }
        public string branchname { get; set; }
        public Double collection { get; set; }
        public Double transferamount { get; set; }
        public string companyname { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }

    }
    public class fullwellcome
    {
        public string refno { get; set; }
        public string name { get; set; }
        public string addr { get; set; }
        public string mobileno { get; set; }
        public string newintroducerid { get; set; }
        [DataType(DataType.Date)]
        public DateTime formdate { get; set; }
        public Double totalcon { get; set; }
        public string newmemberid { get; set; }
        public int plotno { get; set; }
        public string district { get; set; }
        public string state { get; set; }
        public string pin { get; set; }
        public string phase { get; set; }
        public string block { get; set; }
        public string newbondid { get; set; }
        public string loanid { get; set; }       
        public Double bookingamount { get; set; }
        public Double payment { get; set; }
        public string day { get; set; }
        public string paymenttype { get; set; }       

    }

    public class Chequecollection
    {
        public string newbondid { get; set; }
        public string name { get; set; }
        public string bank { get; set; }
        public string chequeno { get; set; }
        public Double amount { get; set; }
        [DataType(DataType.Date)]
        public DateTime pdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string opid { get; set; }
    }
    
    public class tempcheque
    {
        public string newbondid { get; set; }
        public string name { get; set; }
        public Double nolandunit { get; set; }
        public string opid { get; set; }
        public Double bookingamount { get; set; }
        public string bank { get; set; }
        public string Account { get; set; }
        public string chequeno { get; set; }
        public string ACholdername { get; set; }
        public string Branch { get; set; }
        public string IFSCCode { get; set; }
        public string ChequeAmount { get; set; }
        [DataType(DataType.Date)]
        public DateTime Chequedate { get; set; }
        public string Chequeimage { get; set; }
        public string colChequeAmount { get; set; }
        public string colchequeno { get; set; }
        public string colChequedate { get; set; }
    }
    public class PlotList2
    {
        public int id { get; set; }
        public string availablity { get; set; }
        public int status { get; set; }
        public int projectid { get; set; }
        public int blockid { get; set; }
        public string projectname { get; set; }
        public string blockname { get; set; }
        public int plotno { get; set; }

    }

    public class plotlist
    {
        public int plotno { get; set; }
        public string block { get; set; }
        public string project { get; set; }
        public int phaseid { get; set; }
    }

    public class blocklist
    {
        public string block { get; set; }
    }

    public class SpotCommList
    {
        public string newagentid { get; set; }
        public string agentname { get; set; }
        public string newbondid { get; set; }
        public string name { get; set; }
        public Double Business { get; set; }
        public Double Commission { get; set; }
        [DataType(DataType.Date)]
        public DateTime date { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }

    }

    public class booking
    {
        public Double bookingamount { get; set; }
        public string amountword { get; set; }
    }

    public class printcheckbook
    {
        public string name { get; set; }
        public string amountword { get; set; }
        public string amount { get; set; }
        public string D1 { get; set; }
        public string D2 { get; set; }
        public string M1 { get; set; }
        public string M2 { get; set; }
        public string Y1 { get; set; }
        public string Y2 { get; set; }
        public string Y3 { get; set; }
        public string Y4 { get; set; }
    }

    public class MaturityDetails
    {
        public string name { get; set; }
        public string newbondid { get; set; }
        public string newintroducerid { get; set; }
        public Double totalcon { get; set; }
        public Double mamount { get; set; }
        public int type { get; set; }
    }

    public class CustomerDetails
    {
        public string newbondid { get; set; }
        public Double totalcon { get; set; }
        public Double commamount { get; set; }
        public Double pendingamount { get; set; }
        public Double extraamount { get; set; }

    }

    public class VoucherDetail
    {
        public string comname { get; set; }
        public string comaddress { get; set; }
        public string Branch { get; set; }
        public string voucher { get; set; }
        public string bondid { get; set; }
        public string name { get; set; }
        public string amountinword { get; set; }
        public Double matamt { get; set; }
        public string paymethod { get; set; }
        [DataType(DataType.Date)]
        public DateTime pdate { get; set; }
        public string checkorddno { get; set; }
        public string drawno { get; set; }
        public string branchpay { get; set; }
    }

    public class MaturityFormDetail
    {
        public string branchcode { get; set; }
        public string newbondid { get; set; }
        public string name { get; set; }
        public string RelationOf { get; set; }
        public string fathername { get; set; }
        public string addr { get; set; }
        public string mobileno { get; set; }
        public string newintroducerid { get; set; }
        public int intrankcode { get; set; }
        public string intrankname { get; set; }
        [DataType(DataType.Date)]
        public DateTime formdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime dob { get; set; }
        public string age { get; set; }
        public string nationality { get; set; }

        public string guardianname { get; set; }
        public string gurage { get; set; }
        public string gurrel { get; set; }
        public string guraddr { get; set; }
        public string photo { get; set; }

        public string panno { get; set; }
        public string bankname { get; set; }
        public string accountno { get; set; }

        public string plantype { get; set; }
        public string planname { get; set; }
        public Double term { get; set; }
        public string mode { get; set; }
        public Double payment { get; set; }
        public Double nolandunit { get; set; }
        public Double totalcon { get; set; }
        public Double expectedraisablevalue { get; set; }
        public Double applicationcharge { get; set; }
        public Double bonusper { get; set; }
        [DataType(DataType.Date)]
        public DateTime expirydate { get; set; }

        public string nomineename { get; set; }
        public string nomage { get; set; }
        public string nomrel { get; set; }
        public string nomaddr { get; set; }

        public string paymethod { get; set; }
        [DataType(DataType.Date)]
        public DateTime pdate { get; set; }
        public string checkorddno { get; set; }
        public string drawno { get; set; }
        public string branchpay { get; set; }
        public string amountword { get; set; }
        public string opid { get; set; }
        public string maturitytype { get; set; }
        public string type { get; set; }
        public string Macaddress { get; set; }
        public string Time { get; set; }
        public Double maturityamount { get; set; }
        public Double pendingamount { get; set; }
        public Double extraamount { get; set; }
        public Double commamount { get; set; }
        [DataType(DataType.Date)]
        public DateTime maturitydate { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string regno { get; set; }
    }

    public class Printmatdetail
    {
        public string Companyname { get; set; }
        public string mobileno { get; set; }
        public string address { get; set; }
        [DataType(DataType.Date)]
        public DateTime recdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime matdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime exptdate { get; set; }
        public Double depamount { get; set; }
        public Double matamount { get; set; }
        public string name { get; set; }
        public string newbondid { get; set; }
        public string plan { get; set; }
        public string nomineename { get; set; }
        public string nomineerel { get; set; }
        [DataType(DataType.Date)]
        public DateTime appdate { get; set; }
        public int latedays { get; set; }
        public Double amount { get; set; }
        public Double payamount { get; set; }
        public int installmentno { get; set; }
        public int year { get; set; }
        public int cssno { get; set; }
        [DataType(DataType.Date)]
        public DateTime paymentdate { get; set; }
        public string receiptno { get; set; }
        public Double reliefrs { get; set; }
        public Double latefine { get; set; }
        [DataType(DataType.Date)]
        public DateTime expirydate { get; set; }
        public string age { get; set; }
        public string custaddress { get; set; }
        public Double term { get; set; }
        public string mode { get; set; }
        public string branchcode { get; set; }
        public string nomname { get; set; }
        public string nomage { get; set; }
        public string rel { get; set; }
        public string introid { get; set; }
        public string intoname { get; set; }
    }

    public class ranklist
    {
        public int rankcode { get; set; }
        public string rankname { get; set; }
    }

    public class advbalance
    {
        public Double balanceamount { get; set; }
    }

    public class AdvPaymentTemp
    {
        public string newagentid { get; set; }
        public string name { get; set; }
        public Double amount { get; set; }
        public Double recoveryamount { get; set; }
        public string Remark { get; set; }
        public string branchcode { get; set; }
        public string opid { get; set; }
        public string companyname { get; set; }
        [DataType(DataType.Date)]
        public DateTime date { get; set; }
        public string recoverydate { get; set; }
        public string monthname { get; set; }     
    }

    public class Memberfees
    {
        public string newmemberid { get; set; }
       
        
    }

    public class CustomerDeactiveList
    {
        public string newbondid { get; set; }
        public string name { get; set; }
        public string newintroducerid { get; set; }
        public string planname { get; set; }
        public string mode { get; set; }
        public Double payment { get; set; }
        public string mobileno { get; set; }
        [DataType(DataType.Date)]
        public DateTime formdate { get; set; }
        public int status { get; set; }
        public Double latefine { get; set; }
        public Double totalsum { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class Broker
    {
        public string NewAgentId { get; set; }
        public string name { get; set; }
        public string Mobile { get; set; }
        public string Panno { get; set; }
    }

    public class BrokerList
    {

        public string newagentid { get; set; }
        public string name { get; set; }
        public string rankname { get; set; }
        public string introducerid { get; set; }
        public string mobile { get; set; }
        [DataType(DataType.Date)]
        public DateTime doj { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
        public string password { get; set; }

    }

    public class Printcheckbook
    {
        public string Tranid { get; set; }
        public string Name { get; set; }
        public string AccountNo { get; set; }
        public double Amount { get; set; }
        public string Amountword { get; set; }
        public string Planname { get; set; }
        public double term { get; set; }
        public string date { get; set; }
    }

    public class BrokerChainList
    {
        public string newagentid { get; set; }
        public int level { get; set; }
        public int rankcode { get; set; }
        public string introducerid { get; set; }
        public string name { get; set; }
        public string introname { get; set; }

    }

    public class BrokerChain
    {
        public int sr { get; set; }
        public int agentid { get; set; }
        public string newagentid { get; set; }
        public string name { get; set; }
        public int rankcode { get; set; }
        public string rankname { get; set; }
        public string introducerid { get; set; }
        public string introname { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }

    }

    public class BrokerPayment
    {

        public string newagentid { get; set; }
        public string name { get; set; }
        public Double Business { get; set; }
        public Double commission { get; set; }
        public Double tds { get; set; }
        public Double netamount { get; set; }
        [DataType(DataType.Date)]
        public DateTime pdate { get; set; }
        public string month { get; set; }
        public string year { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string branch { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
        public string status { get; set; }

    }

    public class BrokerAccount
    {

        public string newintroducerid { get; set; }
        public string name { get; set; }
        public string newbondid { get; set; }
        public string cname { get; set; }
        public Double amount { get; set; }
        public string planname { get; set; }
        public Double term { get; set; }
        public string mode { get; set; }
        [DataType(DataType.Date)]
        public DateTime expirydate { get; set; }
        [DataType(DataType.Date)]
        public DateTime formdate { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string branch { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
        public string monthname { get; set; }

    }

    public class BrokerwiseAccount
    {
        public string newintroducerid { get; set; }
        public string intname { get; set; }
        public string newbondid { get; set; }
        [DataType(DataType.Date)]
        public DateTime issuedate { get; set; }
        public string name { get; set; }
        public string planname { get; set; }
        public Double term { get; set; }
        public string mode { get; set; }
        [DataType(DataType.Date)]
        public DateTime maturitydate { get; set; }
        public int totalinstall { get; set; }
        public string nextinstall { get; set; }
        public Double amount { get; set; }
        public string nextdate { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string branch { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class InputReceipt
    {

        public string newbondid { get; set; }
        public int paymentno { get; set; }
        public Double payamount { get; set; }
        public int from { get; set; }
        public int to { get; set; }

    }

    public class DupliReceipt
    {

        public string newbondid { get; set; }
        public int paymentno { get; set; }
        public Double payamount { get; set; }
    }

    public class selectrenewaldetail
    {
        public string planname { get; set; }
        public Double term { get; set; }
        public string mode { get; set; }
        [DataType(DataType.Date)]
        public DateTime? prevexpirydate { get; set; }
        [DataType(DataType.Date)]
        public DateTime? expirydate { get; set; }
        public int installmentno { get; set; }
        public int from { get; set; }
        public Double latefine { get; set; }
        public Double relief { get; set; }
        public Double payamount { get; set; }
        public string amountinwords { get; set; }
        public string bondname { get; set; }
        public string introname { get; set; }
        public Double penality { get; set; }
        public Double totalplotcost { get; set; }
        public Double paidamount { get; set; }
        public Double dueamount { get; set; }
    }

    public class ResetRenewel
    {
        [DataType(DataType.Date)]
        public DateTime? expirydate { get; set; }
        public Double latefine { get; set; }
        public Double payamount { get; set; }
        public string amountinwords { get; set; }
    }

    public class CheckRelief
    {
        public Double relief { get; set; }
        public Double latefine { get; set; }
        public Double payamount { get; set; }
        public string amountinwords { get; set; }
        public string msg { get; set; }
        public int type { get; set; }
    }

    public class ModeTerm
    {
        public string mode { get; set; }
        public Double term { get; set; }
        [DataType(DataType.Date)]
        public DateTime expirydate { get; set; }
    }

    public class PaymentList
    {
        public Double payment { get; set; }
        public Double totalcon { get; set; }
        public Double maturityamount { get; set; }
        public string amountinwords { get; set; }
        [DataType(DataType.Date)]
        public DateTime expirydate { get; set; }
    }

    public class CalculateAmount
    {
        public string amountinwords { get; set; }
        public Double totalconsideration { get; set; }
        public Double maturityamount { get; set; }
        public Double plotsize { get; set; }
        public Double bookingamount { get; set; }
        public DateTime expirydate { get; set; }

        public double payment { get; set; }
    }

    public class DisplayMsg
    {

        public string msg { get; set; }
        public int type { get; set; }

    }

    public class RenewalMsg
    {
        public string newbondid { get; set; }
        public string msg { get; set; }
        public int type { get; set; }
    }

    public class RenewalDetails
    {
        public string name { get; set; }
        public string bondname { get; set; }
        public string introname { get; set; }
        public string newbondid { get; set; }
        public string newintroducerid { get; set; }
        public string mobileno { get; set; }
        public int previnstall { get; set; }
        public int type { get; set; }

    }

    public class Commission
    {
        public int rankcode { get; set; }
        public int plancode { get; set; }
        public Double commyear { get; set; }
        public Double commyear2 { get; set; }
        public Double commyear3 { get; set; }
        public Double commyear4 { get; set; }
        public Double commyear5 { get; set; }
    }

    public class Salary
    {
        public int rankcode { get; set; }
        public int planyear { get; set; }
        public int plancode { get; set; }
        public int BasicSalary { get; set; }
        public Double Bonuspercent { get; set; }
        public int insurance { get; set; }
    }

    public class DaillyExpense
    {
        [DataType(DataType.Date)]
        public DateTime date { get; set; }
        public Double amount { get; set; }
    }

    public class Tempbond
    {
        [Key]
        public int sn { get; set; }
        public int bondid { get; set; }
    }

    public class ReceiptList
    {
        public int installmentno { get; set; }
        public string newbondid { get; set; }
        public Double amount { get; set; }
        public string planname { get; set; }
        public string receiptno { get; set; }
        public Double term { get; set; }
        public string mode { get; set; }
        [DataType(DataType.Date)]
        public DateTime paymentdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime maturitydate { get; set; }
        public string branchcode { get; set; }
      
    }

    public class SelfdueCollection
    {
        public string newagentid { get; set; }
        public string name { get; set; }
        public string newbondid { get; set; }
        public string cname { get; set; }
        public string mobileno { get; set; }
        [DataType(DataType.Date)]
        public DateTime expirydate { get; set; }
        public string planname { get; set; }
        public Double term { get; set; }
        public string mode { get; set; }
        public Double amount { get; set; }
        public int installmentno { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string branch { get; set; }
        public string agent { get; set; }
        public string bond { get; set; }
        public string plan { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class BondDueCollection
    {
        public Double amount { get; set; }
    }

    public class ChaindueCollection
    {
        public string brokercode { get; set; }
        public string brokername { get; set; }
        public string newagentid { get; set; }
        public string name { get; set; }
        public string newbondid { get; set; }
        public string cname { get; set; }
        public string mobileno { get; set; }
        [DataType(DataType.Date)]
        public DateTime expirydate { get; set; }
        public string planname { get; set; }
        public Double term { get; set; }
        public string mode { get; set; }
        public Double amount { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string branch { get; set; }
        public string agent { get; set; }
        public string bond { get; set; }
        public string plan { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class SelfCollection
    {
        public string Newbondid { get; set; }
        public Double sis { get; set; }
        public Double MISFresh { get; set; }
        public Double NFPFresh { get; set; }
        public Double PPSFresh { get; set; }
        public Double MIS1 { get; set; }
        public Double MIS2 { get; set; }
        public Double MIS3 { get; set; }
        public Double MIS4 { get; set; }
        public Double MIS5 { get; set; }
        public Double MIS6 { get; set; }
        public Double MIS7a { get; set; }  
    }

    public class OpDatewisecollection
    {
        [DataType(DataType.Date)]
        public DateTime date { get; set; }
        public Double tsis { get; set; }
        public Double tMISFresh { get; set; }
        public Double tMIS1 { get; set; }
        public Double tMIS2 { get; set; }
        public Double tMIS3 { get; set; }
        public Double tMIS4 { get; set; }
        public Double tMIS5 { get; set; }
        public Double tMIS6 { get; set; }
        public Double tMIS7a { get; set; }
        public Double pps { get; set; }
        public Double Expense { get; set; }
        public Double Voucher { get; set; }
        public Double Salary { get; set; }
        public Double Agencyfee { get; set; }
        public Double Appcharge { get; set; }
        public Double Memberfee { get; set; }
        public Double Accountfee { get; set; }
        public Double deposit { get; set; }
        public Double late { get; set; }
        public Double releaf { get; set; }
        public Double widthdraw { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string branch { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }

    }

    public class FundMobilization
    {
        public string planname { get; set; }
        public Double tsis { get; set; }
        public Double tMISFresh { get; set; }
        public Double tPPSFresh { get; set; }
        public Double tNFPFresh { get; set; }
        public Double tMIS1 { get; set; }
        public Double tMIS2 { get; set; }
        public Double tMIS3 { get; set; }
        public Double tMIS4 { get; set; }
        public Double tMIS5 { get; set; }
        public Double tMIS6 { get; set; }
        public Double tMIS7a { get; set; }
        public Double total { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string branch { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }

    }

    public class CsswiseBusiness
    {
       
        public string newbondid { get; set; }
        public string name { get; set; }
        public int cssno { get; set; }
        public string planname { get; set; }
        public string paymethod { get; set; }
        public Double term { get; set; }
        public string mode { get; set; }
        public Double amount { get; set; }
        public Double latefine { get; set; }
        public Double rebate { get; set; }
        public Double payamount { get; set; }
        [DataType(DataType.Date)]
        public DateTime expirydate { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string branch { get; set; }
        public string bond { get; set; }
        public string plan { get; set; }
        [DataType(DataType.Date)]
        public DateTime date { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class TempSelfCollection
    {
       
        public string Newbondid { get; set; }
        public string Customername { get; set; }
        public string brokercode{ get; set; }
        public string brokername { get; set; }
        public Double tsis { get; set; }
        public Double tMISFresh { get; set; }
        public Double tNFPFresh { get; set; }
        public Double tPPSFresh { get; set; }
        public Double tMIS1 { get; set; }
        public Double tMIS2 { get; set; }
        public Double tMIS3 { get; set; }
        public Double tMIS4 { get; set; }
        public Double tMIS5 { get; set; }
        public Double tMIS6 { get; set; }
        public Double tMIS7a { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string branch { get; set; }
        public string agent { get; set; }
        public string bond { get; set; }
        public string plan { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }

    }

    public class TempBrokerChainCollection
    {
        public string brokercode { get; set; }
        public string brokername { get; set; }
        public string newagentid { get; set; }
        public string name { get; set; }
        public Double tsis { get; set; }
        public Double tMISFresh { get; set; }
        public Double tppp { get; set; }
        public Double tMIP { get; set; }
        public Double tMIS1 { get; set; }
        public Double tMIS2 { get; set; }
        public Double tMIS3 { get; set; }
        public Double tMIS4 { get; set; }
        public Double tMIS5 { get; set; }
        public Double tMIS6 { get; set; }
        public Double tMIS7a { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string branch { get; set; }
        public string agent { get; set; }
        public string bond { get; set; }
        public string plan { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }

    }

    public class DailyCollection
    {

        public int SN { get; set; }
        public string branchcode { get; set; }
        public string Name { get; set; }
        [DataType(DataType.Date)]
        public DateTime ondate { get; set; }
        public int CSS_No { get; set; }
        public string Account_Code { get; set; }
        public string Rank { get; set; }
        public string Broker_Code { get; set; }
        public string Unit_Code { get; set; }
        public string Introducer_Code { get; set; }
        public Double Amt_Deposit { get; set; }
        public Double Late_Fee { get; set; }
        public Double Rebate { get; set; }
        public string Reciept_No { get; set; }
        public string Planname { get; set; }
        public Double Term { get; set; }
        public Double formfee { get; set; }
        public Double appcharge { get; set; }
        public Double memberfee { get; set; }
        public Double acformfee { get; set; }
        public Double dipositsaving { get; set; }
        public Double widthdrawsaving { get; set; }
        public Double totalexp { get; set; }
        public Double Salary { get; set; }
        public Double Voucher { get; set; }
        public string New { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
        public string branchname { get; set; }

    }

    public class Installment
    {
        
        public int SN { get; set; }
        public string Account_Code { get; set; }
        public string Name { get; set; }
        [DataType(DataType.Date)]
        public DateTime paymentdate { get; set; } 
        public string Rank { get; set; }
        public string Broker_Code { get; set; }
        public string Unit_Code { get; set; }
        public string Introducer_Code { get; set; }
        public string Planname { get; set; }
        public string Plantype { get; set; }
        public string mode { get; set; }
        public Double Term { get; set; }
        public string rcptno { get; set; }
        public Double amount { get; set; }
        public Double formfee { get; set; }
        public Double appcharge { get; set; }
        public Double latefee { get; set; }
        public Double relief { get; set; }
        public int cssno { get; set; }
        public int installno { get; set; }
        public Double year { get; set; }
        public string branchcode { get; set; }

        public string paymethod { get; set; }
        public string bank { get; set; }
        public string Account { get; set; }
        public string chequeno { get; set; }
        public string ACholdername { get; set; }
        public string Bbranch { get; set; }
        public string IFSCCode { get; set; }
        public string ChequeAmount { get; set; }
        public string Chequedate { get; set; }
        public Double penality { get; set; }
        public string transactiontype { get; set; }
    }

    public class CustomerInstallment
    {
       
        public int Id { get; set; }
        public Double amount { get; set; }
        public Double payamount { get; set; }
        public int bondid { get; set; }
        public Double latefine { get; set; }
        [DataType(DataType.Date)]
        public DateTime prevexpirydate { get; set; }
        [DataType(DataType.Date)]
        public DateTime expirydate { get; set; }
        [DataType(DataType.Date)]
        public DateTime paymentdate { get; set; }
        public Double reliefrs { get; set; }
        public int year { get; set; }
        public int cssno { get; set; }
        public int installmentno { get; set; }
        public string receiptno { get; set; }
        public string planname { get; set; }
        public Double term_plan { get; set; }
        public string mode { get; set; }
        public string paymethod { get; set; }
        public string chekddno { get; set; }
        public string drawno { get; set; }
        public string branch { get; set; }
        public string amountinword { get; set; }
        public string newbondid { get; set; }
        public string opid { get; set; }
        public int paymentno { get; set; }
        public string brokercode { get; set; }
        public string name { get; set; }
        public string age { get; set; }
        public string address { get; set; }
        public string nomname { get; set; }
        public string nomage { get; set; }
        public string nomrel { get; set; }
        public string mobile { get; set; }
        public Double amountrs { get; set; }
        [DataType(DataType.Date)]
        public DateTime pdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime enddate { get; set; }
        public Double bookingamount { get; set; }
        public Double downpayment { get; set; }
        public Double totalcon { get; set; }
        public Double expectedvalue { get; set; }
        public string cust_name { get; set; }
        public string Company { get; set; }
        public string Branch_name { get; set; }
        public string plan_name { get; set; }
        public string caddress { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
        public string branchname { get; set; }
        public string paydate { get; set; }
        public int latedays { get; set; }

        public string bank { get; set; }
        public string Account { get; set; }
        public string chequeno { get; set; }
        public string ACholdername { get; set; }
        public string Bbranch { get; set; }
        public string IFSCCode { get; set; }
        public string ChequeAmount { get; set; }
        public string Chequedate { get; set; }
        public Double penality { get; set; }

        public string Rbank { get; set; }
        public string RAccount { get; set; }
        public string Rchequeno { get; set; }
        public string RACholdername { get; set; }
        public string RBbranch { get; set; }
        public string RIFSCCode { get; set; }
        public string RChequeAmount { get; set; }
        public string RChequedate { get; set; }
        public Double Rpenality { get; set; }
        public string transactiontype { get; set; }
        public string plotno { get; set; }
        public string block { get; set; }
        public string phase { get; set; }
        public double area { get; set; }
    }

    public class Customer
    {
        public string newbondid { get; set; }
        public string name { get; set; }
        public string Mobile { get; set; }
    }

    public class BalanceReport
    {
        public Double fpo { get; set; }
        public Double dpo { get; set; }
        public Double pps { get; set; }
        public Double bookingamount { get; set; }
        public Double downpayment { get; set; }
        public Double newbond { get; set; }
        public Double renewel { get; set; }
        public Double bformfee { get; set; }
        public Double cformfee { get; set; }
        public Double mformfee { get; set; }
        public Double acformfee { get; set; }
        public Double intotal { get; set; }
        public Double expence { get; set; }
        public Double voucher { get; set; }
        public Double advpayment { get; set; }
        public Double salary { get; set; }
        public Double deposit { get; set; }
        public Double withdraw { get; set; }
        public Double spotcomm { get; set; }
        public Double extotal { get; set; }
        public Double grandtotal { get; set; }
        public Double latefee { get; set; }
        public string branchname { get; set; }
        public Double openingbalance { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
        public Double maturity { get; set; }
      


    }

    public class MaturityPaid
    {
        public string newbondid { get; set; }
        public string name { get; set; }
        public string newintroducerid { get; set; }
        public string mobileno { get; set; }
        [DataType(DataType.Date)]
        public DateTime formdate { get; set; }
        public string planname { get; set; }
        public Double term { get; set; }
        public string mode { get; set; }
        public Double payment { get; set; }
        public Double nolandunit { get; set; }
        public Double totalcon { get; set; }
        public Double expectedraisablevalue { get; set; }
        [DataType(DataType.Date)]
        public DateTime expirydate { get; set; }
        public Double maturityamount { get; set; }
        [DataType(DataType.Date)]
        public DateTime maturitydate { get; set; }
        public string branchname { get; set; }
        public string chequedate { get; set; }
        public string companyname { get; set; }
        public string branch { get; set; }
        public string agent { get; set; }
        public string bond { get; set; }
        public string plan { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
        [DataType(DataType.Date)]
        public DateTime exptdate { get; set; }
        public string paymethod { get; set; }
        [DataType(DataType.Date)]
        public DateTime paytdate { get; set; }
        public string Draw { get; set; }
        public string branchpay { get; set; }
        public string checkdd { get; set; }
        public int plotno { get; set; }
        public string block { get; set; }
    }

    public class plan
    {
        
        public int sr { get; set; }
        public int plancode { get; set; }
        public string planname { get; set; }
    }

    public class agents
    {
        
        public int sr { get; set; }
        public int agentcode { get; set; }

    }

    public class teamagents
    {
       
        public int sr { get; set; }
        public int tagentcode { get; set; }

    }

    public class require
    {
        public int agentid { get; set; }
        public string newagentid { get; set; }
        public string name { get; set; }
        public string rankname { get; set; }
        public string planname { get; set; }
        public Double year { get; set; }
        public Double business { get; set; }
        public Double percentage { get; set; }
        public Double commission { get; set; }
        public Double tds { get; set; }
        public Double bonusamount { get; set; }
        public Double advancepayment { get; set; }
        public string brokerid { get; set; }
        public string brokername { get; set; }
        public string brokerrank { get; set; }
        public string panno { get; set; }
        public string newintroducerid { get; set; }
        public string month { get; set; }
        public int  voucherno { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
        public string commtype { get; set; }
        public string amountinword { get; set; }
        public string chequeno { get; set; }
        public DateTime Chequedate { get; set; }
        public string paymethod { get; set; }
        public double sumamount1 { get; set; }
        public double sumcommission1 { get; set; }
        public double advancepayment1 { get; set; }
        public double bonusamount1 { get; set; }
        public double lesstds1 { get; set; }
        public double netpayamount1 { get; set; }
        public string paystatus { get; set; }
        public string fathername { get; set; }
        public string city { get; set; }
        public string pincode { get; set; }
        public string introducername { get; set; }
        public double directincome { get; set; }
        public double teamincome { get; set; }
    }

    public class trequire
    {
        public string newagentid { get; set; }
        public string planname { get; set; }
        public int plancode { get; set; }
        public Double year { get; set; }
        public Double business { get; set; }
        public Double percentage { get; set; }
        public Double commission { get; set; }
        public string commtype { get; set; }

    }

    public class Icard
    {
        public string newagentid { get; set; }
        public string name { get; set; }
        public string newintroducerid { get; set; }
        public string introname { get; set; }
        public string fname { get; set; }
        public string mobileno { get; set; }
        public string address { get; set; }
        public string dist { get; set; }
        public string state { get; set; }
        public string pin { get; set; }
        public string rank { get; set; }
        public string branch { get; set; }
        public string comcontact { get; set; }
        public string companyname { get; set; }
        public string comaddress { get; set; }
        public string domainname { get; set; }
        public string emailid { get; set; }
        [DataType(DataType.Date)]
        public DateTime dob { get; set; }
        [DataType(DataType.Date)]
        public DateTime issuedate { get; set; }
        [DataType(DataType.Date)]
        public DateTime validdate { get; set; }
        public string btype { get; set; }
        public byte[] photo { get; set; }
        public int age { get; set; }
    
    }

    public class Voucher
    {
        [Key]
        public int sr { get; set; }
        public String newagentid { get; set; }
        public string planname { get; set; }
        public double year { get; set; }
        public String newbondid { get; set; }
        public String name { get; set; }
        public Double business { get; set; }
        public Double commission { get; set; }
        public Double tds { get; set; }
        public Double netamount { get; set; }
        public string branch { get; set; }
        [DataType(DataType.Date)]
        public DateTime date { get; set; }

    }

    public class VoucherList
    {
        public string id { get; set; }
        public string Branchname { get; set; }
        public string newagentid { get; set; }
        public string name { get; set; }
        public string introducerid { get; set; }
        public Double amount { get; set; }
        public int status { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public string monthname { get; set; }
        public Double business { get; set; }
        public Double GrossAmount { get; set; }
        public Double TDSAmount { get; set; }
        public Double advancepayment { get; set; }
        public Double bounusamount { get; set; }
        public Double commission { get; set; }
        public Int64 OtherMobile { get; set; }
        public string Panno { get; set; }
        public string BankAccountno { get; set; }
        public string IFCCode { get; set; }
        public string CompanyName { get; set; }
        public string chequeno { get; set; }
        public DateTime Chequedate { get; set; }
        public string CompanyAddress { get; set; }
        //public string transaction_id { get; set; }
        public string paymethod { get; set; }
        //public DateTime cdate { get; set; }
    }
    
    public class BondwiseVoucherlist
    {
        public string newbondid { get; set; }
        public string bondname { get; set; }
        public string bondintroducerid { get; set; }
        public string planname { get; set; }
        public string mode { get; set; }
        public Double percentage { get; set; }
        public Double business { get; set; }
        public Double commission { get; set; }
        public string newrenew { get; set; }
        public Double tds { get; set; }
        public Double bonusamount { get; set; }
        public string brokerid { get; set; }
        public string brokername { get; set; }
        public string brokerrank { get; set; }
        public string panno { get; set; }
        public string newintroducerid { get; set; }
        public string month { get; set; }
        public int voucherno { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string branch { get; set; }
        public string agent { get; set; }
        public string bond { get; set; }
        public string plan { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
        public string commtype { get; set; }
    }

    public class PaidVoucher
    {
        public int Id { get; set; }
        public string Brokercode { get; set; }
        public string BrokerName { get; set; }
        public string panno { get; set; }
        public string branchcode { get; set; }
        public string Operatorid { get; set; }
        public int agentid { get; set; }
        public Double bussiness { get; set; }
        public Double commission { get; set; }
        public Double tds { get; set; }
        public Double advamount { get; set; }
        public Double bonusamount { get; set; }
        public Double netamount { get; set; }
        public string month { get; set; }
        public string year { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        [DataType(DataType.Date)]
        public DateTime date { get; set; }
        public string Companyname { get; set; }
        public string Broker_name { get; set; }
        public string Branch_name { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class InstallmentList
    {
        public string newbondid { get; set; }
        public int installmentno { get; set; }
        public string mode { get; set; }
        public int year { get; set; }
        public Double term { get; set; }
        public string planname { get; set; }
        public Double amount { get; set; }
        public int plancode { get; set; }
        public int paymentno { get; set; }
        public Double payamount { get; set; }
        public string name { get; set; }
        public string newintroducerid { get; set; }
        public string branchcode { get; set; }
        public string Bbranch { get; set; }
        public DateTime paymentdate { get; set; }
        public string paymethod { get; set; }
        public int Id { get; set; }
        public string IIDD { get; set; }
        public string id2 { get; set; }
        public string bank { get; set; }
        public string ACholdername { get; set; }
        public string chequeno { get; set; }
        public string Chequeamount { get; set; }
        public string IFSC { get; set; }
        public DateTime Chequedate { get; set; }
        public Double penality { get; set; }
        public Double latefine { get; set; }
        public Double reliefrs { get; set; }
    }

    public class QuotaCollectionList
    {
        public string brokerid { get; set; }
        public string brokername { get; set; }
        public string newagentid { get; set; }
        public string name { get; set; }
        public string rankname { get; set; }
        public string newintroducerid { get; set; }
        public string planname { get; set; }
        public Double DPPBusiness { get; set; }
        public Double IPPBusiness { get; set; }
        public Double business { get; set; }
        public Double percentage { get; set; }
        public Double quotaamount { get; set; }
        public Double PromotionTarget { get; set; }
        public Double Difference { get; set; }
        public int rankcount { get; set; }
        public int prankcount { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string branch { get; set; }
        public string agent { get; set; }
        public string bond { get; set; }
        public string plan { get; set; }
        public int rankcode { get; set; }
        public string status { get; set; }
    }

    public class QuotaAAchive
    {
        public string NewAgentid { get; set; }
        public string Name { get; set; }
        public string RankName { get; set; }
        public Double QuotaAmount { get; set; }
        public Double QuotaAchive { get; set; }
        public string Remark { get; set; }
        public string Month { get; set; }
        public string Year { get; set; }
    }

    public class DatewiseCustomerList
    {
        public string newbondid { get; set; }
        public string name { get; set; }
        public string newintroducerid { get; set; }
        public string planname { get; set; }
        public string mode { get; set; }
        public Double totalamount { get; set; }
        public Double payment { get; set; }
        public Double bookingamount { get; set; }
        public Double downpayment { get; set; }
        public Double plotsize { get; set; }
        public string Mobile { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string branch { get; set; }
        public string bond { get; set; }
        public string plan { get; set; }
        [DataType(DataType.Date)]
        public DateTime formdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
        public int plotno { get; set; }
        public string block { get; set; }
    }

   
    public class Newbondid
    {
        public string Nbond { get; set; }
    }

    public class Accountdetail
    {
        public string newbondid { get; set; }
        [DataType(DataType.Date)]
        public DateTime issuedate { get; set; }
        public string name { get; set; }
        public string planname { get; set; }
        public Double term { get; set; }
        public string mode { get; set; }
        [DataType(DataType.Date)]
        public DateTime maturitydate { get; set; }
        public int totalinstall { get; set; }
        public int nextinstall { get; set; }
        public Double amount { get; set; }
        [DataType(DataType.Date)]
        public DateTime nextdate { get; set; }

    }

    public class printpassbook
    {
        public string sr { get; set; }
        public string trandate { get; set; }
        public string tranid { get; set; }
        public string mode { get; set; }
        public string installno { get; set; }
        public string status { get; set; }
        public string widthdraw { get; set; }
        public string diposit { get; set; }
        public string balance { get; set; }
        public string latefine { get; set; }
        public double rebate { get; set; }
        public string lateday { get; set; }
        public string remark { get; set; }
    }

    public class AcBalanceReport
    {
        public string actype { get; set; }
        public string accountno { get; set; }
        public string acholdername { get; set; }
        public Double credit { get; set; }
        public Double debit { get; set; }
        public Double balance { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string companyname { get; set; }
        public string branchname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class AcStatementReport
    {
        public string actype { get; set; }
        public string accountno { get; set; }
        public string acholdername { get; set; }
        public string transactionid { get; set; }
        public string remarks { get; set; }
        public Double credit { get; set; }
        public Double debit { get; set; }
        public Double balance { get; set; }
        public Double prevbalance { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        [DataType(DataType.Date)]
        public DateTime pdate { get; set; }
        public string companyname { get; set; }
        public string branchname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class Logindetail
    {
        public string userid { get; set; }
        public string password { get; set; }
        [DataType(DataType.Date)]
        public DateTime intime { get; set; }
        [DataType(DataType.Date)]
        public DateTime outtime { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class Branchlist
    {
        public string BranchName { get; set; }
        public string BranchCode { get; set; }
        public string password { get; set; }
        public string BranchDistrict { get; set; }
        public string branchaddress { get; set; }
        public string mobile { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class Oplist
    {
        public string OperatorName { get; set; }
        public string OperatorId { get; set; }
        public string BranchCode { get; set; }
        public string OperatorMobile { get; set; }
        public string OperatorAddress { get; set; }
        public string Operator_Mail { get; set; }
        [DataType(DataType.Date)]
        public DateTime Cdate { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
        public string password { get; set; }
    }

    public class hrlist
    {
        public string HRName { get; set; }
        public string HRId { get; set; }
        public string password { get; set; }
        public string BranchCode { get; set; }
        public string Mobile { get; set; }
        public string email { get; set; }
        public string hAddress { get; set; }
        [DataType(DataType.Date)]
        public DateTime date { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class DWMemberList
    {
        public string newmemberid { get; set; }
        public string name { get; set; }
        [DataType(DataType.Date)]
        public DateTime dob { get; set; }
        public string fathername { get; set; }
        public string maddress { get; set; }
        public string Mobileno { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        [DataType(DataType.Date)]
        public DateTime formdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class DWAccList
    {
        public string accountno { get; set; }
        public string name { get; set; }
        public string fathername { get; set; }
        public string maddress { get; set; }
        public string Mobileno { get; set; }
        public string nomineename { get; set; }
        public string nomage { get; set; }
        public string nomrel { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        [DataType(DataType.Date)]
        public DateTime formdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class DWAccListCollection
    {
        public string accountno { get; set; }
        public string name { get; set; }
        public double credit { get; set; }
        public double debit { get; set; }
        public double balance { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
      
    }

    public class RdList
    {
        public string Planname { get; set; }
        public Double Term { get; set; }
        public Double Amount { get; set; }
        public int plotsize { get; set; }
        public Double Monthly { get; set; }
        public Double Quarterly { get; set; }
        public Double Halfyearly { get; set; }
        public Double Yearly { get; set; }
        public Double EstimatedValue { get; set; }
        public Double Accidental { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class FdList
    {
        public string Planname { get; set; }
        public Double Term { get; set; }
        public int plotsize { get; set; }
        public Double Amount { get; set; }
        public Double EstimatedValue { get; set; }
        public Double Accidental { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class NfpList
    {
        public string Planname { get; set; }
        public Double Term { get; set; }
        public Double Amount { get; set; }
        public Double EstimatedValue { get; set; }
        public Double Yearlyamount { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class PensionList
    {
        public string Planname { get; set; }
        public Double Term { get; set; }
        public Double Amount { get; set; }
        public Double Monthly { get; set; }
        public Double Quarterly { get; set; }
        public Double Halfyearly { get; set; }
        public Double Yearly { get; set; }
        public Double Profit { get; set; }
        public Double EstimatedValue { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class ranList
    {
        public string rankname { get; set; }
        public string rankshname { get; set; }
        public int rankcode { get; set; }
        public Double selfquota { get; set; }
        public Double teamquota { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }

    public class ExpenseList
    {
        public string head { get; set; }
        public string Remark { get; set; }
        public Double amount { get; set; }
        [DataType(DataType.Date)]
        public DateTime date_time { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string branchcode { get; set; }
        public string opid { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
        public string paymethod { get; set; }
        public string trans_id { get; set; }
    }

    public class HeadwiseExpense
    {
        public string head { get; set; }
        public string remark { get; set; }
        public Double amount { get; set; }
        [DataType(DataType.Date)]
        public DateTime date { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string companyname { get; set; }
        public string branchname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
        public string paymethod { get; set; }
        public string opid { get; set; }
    }

    public class MergeDailyCollection
    {

        public int SN { get; set; }
        public string no { get; set; }
        public string trno { get; set; }
        public string name { get; set; }
        [DataType(DataType.Date)]
        public DateTime date { get; set; }
        public int cssno { get; set; }
        public string newbondid { get; set; }
        public string rankname { get; set; }
        public string newagenid { get; set; }
        public string newintroducerid { get; set; }
        public Double amount { get; set; }
        public Double latefine { get; set; }
        public Double rebate { get; set; }
        public string receiptno { get; set; }
        public string planname { get; set; }
        public Double term { get; set; }
        public Double formfee { get; set; }
        public Double appcharge { get; set; }
        public Double memberfee { get; set; }
        public Double acformfee { get; set; }
        public Double dipositsaving { get; set; }
        public Double widthdrawsaving { get; set; }
        public Double totalexp { get; set; }
        public Double salary { get; set; }
        public Double voucher { get; set; }
        public string newrenew { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
        public string paymethod { get; set; }

        public string bank { get; set; }
        public string Account { get; set; }
        public string chequeno { get; set; }
        public string ACholdername { get; set; }
        public string Bbranch { get; set; }
        public string IFSCCode { get; set; }
        public string ChequeAmount { get; set; }
        public string Chequedate { get; set; }
        public string Chequeimage { get; set; }
        public string transactiontype { get; set; } 
    }

    public class DuplicateList
    {
      
        public string newbondid { get; set; }
        public Double payamount { get; set; }
        public int paymentno { get; set; }
        public string opid { get; set; }
        [DataType(DataType.Date)]
        public DateTime date { get; set; }
        public string macaddress { get; set; }
        public string type { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }

    }

    public class CancelRcptList
    {

        public string newbondid { get; set; }
        public Double payamount { get; set; }
        public int installmentno { get; set; }
        public string receiptno { get; set; }
        public string planname { get; set; }
        public string mode { get; set; }
        public Double term { get; set; }
        [DataType(DataType.Date)]
        public DateTime pdate { get; set; }
        public string cdate { get; set; }
        public string cancelby { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
        public string opid { get; set; }
        public string time { get; set; }
        public string customername { get; set; }
        public string brokername { get; set; }
    }

    public class TDSTemp
    {
        public string brokerid { get; set; }
        public string brokername { get; set; }
        public string address { get; set; }
        [DataType(DataType.Date)]
        public DateTime dob { get; set; }
        public string panno { get; set; }
        public double tds { get; set; }
        public string monthname { get; set; }
    }

    public class DatewiseDueCollection
    {
        public string newbondid { get; set; }
        public int bondid { get; set; }
        public string name { get; set; }
        public string newintroducerid { get; set; }
        public string IntroName { get; set; }
        [DataType(DataType.Date)]
        public DateTime formdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime date { get; set; }
        public string installment { get; set; }
        public Double amount { get; set; }
        public Double installamount { get; set; }
        public string branchname { get; set; }
        public string branchnamenew { get; set; }
        public string companyname { get; set; }
        public string monthname { get; set; }
        public string plan { get; set; }
        public string viewby { get; set; }
        public string mobileno { get; set; }
        public int plotno { get; set; }
        public string block { get; set; }
    }

    public class AgentCollection
    {
        public Double todayfdcoll1 { get; set; }
        public Double todayRdcoll1 { get; set; }
        public Double todayNewBondcoll1 { get; set; }
        public Double todayRenewalBondcoll1 { get; set; }
        public Double todayVouchercoll1 { get; set; }
        public Double todaycoll1 { get; set; }

        public Double PrevFivedayfdcoll1 { get; set; }
        public Double PrevFivedayRdcoll1 { get; set; }
        public Double PrevFivedayNewBondcoll1 { get; set; }
        public Double PrevFivedayRenewalBondcoll1 { get; set; }
        public Double PrevFivedayVouchercoll1 { get; set; }
        public Double PrevFivedaycoll1 { get; set; }

        public Double AfterFivefdcoll1 { get; set; }
        public Double AfterFiverdcoll1 { get; set; }
        public Double AfterFivenewbondcoll1 { get; set; }
        public Double AfterFiverenewalbondcoll1 { get; set; }
        public Double AfterFiveVouchercll1 { get; set; }
        public Double AfterFivecoll1 { get; set; }

        public Double totalfdcoll1 { get; set; }
        public Double totalRdcoll1 { get; set; }
        public Double totalNewBondcoll1 { get; set; }
        public Double totalRenewalBondcoll1 { get; set; }
        public Double totalVouchercoll1 { get; set; }
        public Double totalcoll1 { get; set; }
    }

    public class Liability
    {
        public string newbondid { get; set; }
        public string name { get; set; }
        public string newintroducerid { get; set; }
        [DataType(DataType.Date)]
        public DateTime Maturitydate { get; set; }
        [DataType(DataType.Date)]
        public DateTime doj { get; set; }
        [DataType(DataType.Date)]
        public DateTime date { get; set; }
        [DataType(DataType.Date)]
        public DateTime expdate { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public string monthname { get; set; }
        public string installment { get; set; }
        public Double amount { get; set; }
        public Double paidinstallment { get; set; }
        public Double unpaidinstallment { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string totalinstallment { get; set; }
        public string paidamount { get; set; }
        public string unpainamount { get; set; }
        public string plan { get; set; }
        public double planamount { get; set; }
        public string docstatus { get; set; }
    }

    public class DatewiseVoucherList
    {
        public string newagentid { get; set; }
        public string name { get; set; }
        public string newintroducerid { get; set; }
        public Double amount { get; set; }
        [DataType(DataType.Date)]
        public DateTime pdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }

    }

    public class DatewiseSVoucherList
    {
        public string newagentid { get; set; }
        public string name { get; set; }
        public Double amount { get; set; }
        public Double commission { get; set; }
        [DataType(DataType.Date)]
        public DateTime pdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string newbondid { get; set; }
        public string bondname { get; set; }
        public int agentid { get; set; }
    }
    public class plotReglist
    {
        public string newbondid { get; set; }
        public string name { get; set; }
        public int plotno { get; set; }
        public string block { get; set; }
        public double amount { get; set; }
        public double totalamount { get; set; }
        [DataType(DataType.Date)]
        public DateTime date { get; set; }

        [DataType(DataType.Date)]
        public DateTime sdate { get; set; }

        [DataType(DataType.Date)]
        public DateTime edate { get; set; }
        public double bookingamount { get; set; }
        public double Remainingamount { get; set;}
        public double deductamount { get; set; }
        public double commission { get; set;}
        public int status { get; set; }
        public string type { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
    }
    public class plotFree
    {
        public string newbondid { get; set; }
        public string name { get; set;}
        public string Introducer { get; set; }
        public Double plotcost { get; set; }
        public int plotno { get; set; }
        public string block { get; set; }
        public string city { get; set; }
        public int due { get; set; }
        public Double totalamount { get; set; }
        public Double paidamount { get; set; }
         [DataType(DataType.Date)]
        public DateTime date { get; set; }
    }
    public class Visitors
    {
        public string name { get; set; }
        public int mobile { get; set; }
        public string purpose { get; set; }
        public string intime { get; set; }
        public string outtime { get; set; }
        public DateTime date { get; set; }
    }
    public class promotiondetail
    {
        public string newagentid { get; set; }
        public string name { get; set; }
        public int rank { get; set; }
        public int selfjoining { get; set; }
        public Double business { get; set; }
        public Double joining { get; set; }
        public Double target { get; set; }
        public string status { get; set; }
        public DateTime sdate { get; set; }
        public DateTime edate { get; set; }
        public string branchname { get; set; }
        public string companyname { get; set; }
        public string address { get; set; }
        public string emailid { get; set; }
        public string contact { get; set; }
        public string domainname { get; set; }
        public int advjoining { get; set; }
    }

    public class Wallet_TransactionTemp
    {
        [Key]
        public int Sr_No { get; set; }
        public double Debit { get; set; }
        public double Credit { get; set; }
        public string agentid { get; set; }
        public string Type { get; set; }
        public string Remark { get; set; }
        public DateTime Date_Time { get; set; }
        public string Transaction_No { get; set; }
        public int status { get; set; }
        public double netamount { get; set; }
        public string Sender_Id { get; set; }
        public string Sender_Name { get; set; }
    }

    public class Appltab
    {

        public int status { get; set; }

        public string newbondid { get; set; }

        public string name { get; set; }

        public string newintroducerid { get; set; }

        public string planname { get; set; }

        public string mode { get; set; }

        public double payment { get; set; }

        public string mobileno { get; set; }

        public DateTime formdate { get; set; }

        public double dueamount { get; set; }

        public int dueinstall { get; set; }
    }


    /*--------------------------------------------------26-09-2018-----------------------------------------*/

    public class AllotmentLetter {


        public string bookingdate { get; set; }
        public string customerid { get; set; }
        public string customername { get; set; }
        public string customeraddress { get; set; }
        public int propertyid { get; set; }
        public int propertyno { get; set; }
        public string category { get; set; }
        public string incomegroup { get; set; }
        public double bookingamount { get; set; }
        public double propertycost { get; set; }
        public string projectname { get; set; }
        public string phase { get; set; }
        public string propertytype { get; set; }
        public double propertyarea { get; set; }
        public int installmentno { get; set; }
        public string paymentdate { get; set; }
        public double installmentamount { get; set; }
        public int paymentno { get; set; }
        public string plotdesp { get; set; }
        public string northwest { get; set; }
        public string northeast { get; set; }
        public string southwest { get; set; }
        public string southeast { get; set; }
        public string prppertyblock { get; set; }
        public string propertyaddress { get; set; }
        public string propertypreference { get; set; }
        public string amountinword { get; set; }
        public int totalinstallment { get; set; }
        public string refno { get; set; }
        public string Gender { get; set; }
        public double totalinstallmentamount { get; set; }
        //public string refno { get; set; }
        public string name { get; set; }
        public string addr { get; set; }
        public string mobileno { get; set; }
        public string newintroducerid { get; set; }
        [DataType(DataType.Date)]
        public DateTime formdate { get; set; }
        public Double totalcon { get; set; }
        public string newmemberid { get; set; }
        public int plotno { get; set; }
        public string district { get; set; }
        public string state { get; set; }
        public string pin { get; set; }
        //public string phase { get; set; }
        public string block { get; set; }
        public string newbondid { get; set; }
        public string loanid { get; set; }
        //public Double bookingamount { get; set; }
        public Double payment { get; set; }
        public string day { get; set; }
        public string paymenttype { get; set; }
        public string bankname { get; set; }
        public string chequeno { get; set; }
        public string planname { get; set; }
    }
    }
    #region

    /// <summary> Recharge & Wallet Model
    public class tempcircluser2
    {

        public string mobile { get; set; }
        public string bankname { get; set; }
        public double amount { get; set; }
        public int payment_status { get; set; }
    }

    public class rechargeresponce
    {
        public string payid { get; set; }
        public string operator_ref { get; set; }
        public string status { get; set; }
        public string txstatus_desc { get; set; }
        public string message { get; set; }
    }
    public class tempbhartitranchk_status
    {
        public string status { get; set; }
        public string responseCode { get; set; }
        public string approvedAmount { get; set; }
        public string transactionId { get; set; }
        public string message { get; set; }
    }
    public class temprechargebalance
    {
        public double balance { get; set; }
    }
    /// </summary>
    #endregion



    public class tempiciciiorderid
    {
        public int status { get; set; }
        public string response { get; set; }
    }

    public class tempopenconnet_ICICIgetway
    {
        public string amount { get; set; }
        public string currency { get; set; }
        public string mtx { get; set; }
        public string[] payment_instruments { get; set; }
        public string[] payment_instrument_types { get; set; }
        public string attempts { get; set; }
        public string id { get; set; }
        public string entity { get; set; }
        public string status { get; set; }
        public cutomer customer { get; set; }
    }
    public class cutomer
    {
        public string contact_number { get; set; }
        public string email_id { get; set; }
        public string id { get; set; }
        public string entity { get; set; }
    }

    public class payment_gateway
    {

        //public static string api_key = "74b053d0-a0c5-11ea-a81f-f997fa5740be";
        //public static string secret_key = "d8125f1abf29577afd7b32c01872a300a2e357e7";
        public static string api_key = "c5aaddc0-b2e0-11ea-83d6-73e546ea240e";
        public static string secret_key = "a78f996962224b8da1c74896347fdac028e2b1c3";
        public static string gateway_url = "https://sandbox-demo.open.money/callback";

    }

    public class PossessionReport
    {

        public int Id { get; set; }
        public int bondid { get; set; }
        public string branchcode { get; set; }
        public string newbondid { get; set; }
        public string name { get; set; }
        public string RelationOf { get; set; }
        public string fathername { get; set; }
        public string addr { get; set; }
        public string mobileno { get; set; }
        public int introducerid { get; set; }
        public string newintroducerid { get; set; }
        public int intrankcode { get; set; }
        public string intrankname { get; set; }
        [DataType(DataType.Date)]
        public DateTime formdate { get; set; }
        [DataType(DataType.Date)]
        public DateTime dob { get; set; }
        public string age { get; set; }
        public string nationality { get; set; }

        public string guardianname { get; set; }
        public string gurage { get; set; }
        public string gurrel { get; set; }
        public string guraddr { get; set; }
        public string photo { get; set; }

        public string panno { get; set; }
        public string bankname { get; set; }
        public string accountno { get; set; }

        public string plantype { get; set; }
        public int plancode { get; set; }
        public string planname { get; set; }
        public Double term { get; set; }
        public string mode { get; set; }
        public Double payment { get; set; }
        public Double nolandunit { get; set; }
        public Double totalcon { get; set; }
        public Double expectedraisablevalue { get; set; }
        public Double applicationcharge { get; set; }
        public Double bonusper { get; set; }
        [DataType(DataType.Date)]
        public DateTime expirydate { get; set; }

        public string nomineename { get; set; }
        public string nomage { get; set; }
        public string nomrel { get; set; }
        public string nomaddr { get; set; }

        public string paymethod { get; set; }
        [DataType(DataType.Date)]
        public DateTime pdate { get; set; }
        public string checkorddno { get; set; }
        public string drawno { get; set; }
        public string branchpay { get; set; }
        public string amountword { get; set; }
        public string opid { get; set; }
        public int status { get; set; }
        public string type { get; set; }
        public string Macaddress { get; set; }
        public string Time { get; set; }
        public int memberid { get; set; }
        public string newmemberid { get; set; }
        public string IFSC { get; set; }
        public Double discountper { get; set; }
        public Double bookingamount { get; set; }
        public Double downpayment { get; set; }
        public int plotno { get; set; }
        public string PYN { get; set; }
        public int projectid { get; set; }
        public string block { get; set; }
        public int phaseid { get; set; }
        public int printstatus { get; set; }

        public string bank { get; set; }
        public string Account { get; set; }
        public string chequeno { get; set; }
        public string ACholdername { get; set; }
        public string Branch { get; set; }
        public string IFSCCode { get; set; }
        public string ChequeAmount { get; set; }
        [DataType(DataType.Date)]
        public DateTime Chequedate { get; set; }
        public string Chequeimage { get; set; }
        [DataType(DataType.Date)]
        public DateTime chequeappdate { get; set; }
        public string transactiontype { get; set; }

        [DataType(DataType.Date)]
        public DateTime PAN_ReqDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime PAN_AppDate { get; set; }
        public int PanStatus { get; set; }
        [DataType(DataType.Date)]
        public DateTime Aadhaar_ReqDate { get; set; }
        [DataType(DataType.Date)]
        public DateTime Aadhaar_AppDate { get; set; }
        public string Aadhaar_No { get; set; }
        public int Aadhaar_status { get; set; }
        public string paymenttype { get; set; }
        /*-----------------------------------------------------*/
        public string propertyaddress { get; set; }
        public string propertypreference { get; set; }
        public Double plccost { get; set; }
        public string refno { get; set; }
        public int propertyid { get; set; }
        public string loanid { get; set; }
        /*-----------------------------------------------------*/
        public string propertytype { get; set; }
        public string incomegroup { get; set; }
        public string phase { get; set; }
        public string plotdesp { get; set; }
        public string northwest { get; set; }
        public string northeast { get; set; }
        public string southwest { get; set; }
        public string southeast { get; set; }
        /*-----------------------------------------------------*/
        public string sapplicantname { get; set; }
        public string sfathername { get; set; }
        public string smothername { get; set; }
        public string scategory { get; set; }
        public string scorraddress { get; set; }
        public string spermanentaddress { get; set; }
        public string scity { get; set; }
        public string sstate { get; set; }
        public int spincode { get; set; }
        public string snationality { get; set; }
        public string squalification { get; set; }
        public string smobileno { get; set; }
        public string salternatemobileno { get; set; }
        public string semailid { get; set; }
        public string sdob { get; set; }
        public string spanno { get; set; }
        public string saadharno { get; set; }
        public string smaritialstatus { get; set; }
        public string sanniversarydate { get; set; }
        public string soccupation { get; set; }
        public string spassport { get; set; }
        public string sresidentialstatus { get; set; }
        public string sphoto { get; set; }
        public string saadharphoto { get; set; }
        public string spanphoto { get; set; }
        public string category { get; set; }
        public string pcity { get; set; }
        public string pstate { get; set; }
        public string ppincode { get; set; }
        public int bank_status { get; set; }

        public string email { get; set; }

        public string city { get; set; }
        public string state { get; set; }
        public string pincode { get; set; }
    }
