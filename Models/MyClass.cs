using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Web;

namespace ODHDEVELOPERS.Models
{

    public class MyClass
    {
        public static void Sendmsg(string receiver, string msg)
        {
            string url = @"http://sms.technomate.mobi/api/swsend.asp?username=t1welltoss&password=carewe078066&sender=XPRTGP&entityID=1701160639054261180&sendto=" + receiver + "&message=" + msg + "";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
           //string url = @"https://login.bulksmsgateway.in/sendmessage.php?user=Odhdpl&password=odh@87612&mobile=" + receiver + "&message="+msg+"&sender=ODHDPL&type=3&template_id=123";
            //string url = @"http://sms.escroll.in/api/mt/SendSMS?user=Odhdpl&password=ODHDPL&senderid=ODHDPL&channel=TRANS&DCS=0&flashsms=0&number=" + receiver + "&text=" + msg + "&route=23";
            //string url = @"http://sms.technomate.mobi/api/swsend.asp?username=t1welltoss&password=34937074&sender=ODHDPL&sendto=" + receiver + "&message=" + msg + "";
            //string url = @"http://alotsolutions.in/API/WebSMS/Http/v1.0a/index.php?username=WELLTOSS&password=0VseYzvE&sender=ODHDPL&to=" + receiver + "&message=" + msg + "&reqid=1";
            //string url = @"http://sms.xpertgroup.in/api/pushsms.php?username=ODHDPL&password=2425&sender=ODHDPL&message=" + msg + "&numbers=" + receiver + "&unicode=false";
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                string aaa = resp.ResponseUri.ToString();
                if (aaa.StartsWith("Wrong UserName or Password"))
                {
                    msg = "Message not Sent";
                }
                else
                {
                    msg = "Message Sent";
                }
            }
            catch (Exception)
            {

            }

        }

        public static void Sendmsg_hindi(string receiver, string msg)
        {
            string url = @"http://sms.escroll.in/api/mt/SendSMS?user=ODHDPL&password=ODHDPL&senderid=ODHDPL&channel=TRANS&DCS=8&flashsms=0&number=" + receiver + "&text=" + msg + "&route=23";
            //string url = @"http://sms.technomate.mobi/api/swsendUni2.asp?username=t1welltoss&password=34937074&sender=ODHDPL&sendto=" + receiver + "&message=" + msg;
            //string url = @"http://alotsolutions.in/API/WebSMS/Http/v1.0a/index.php?username=WELLTOSS&password=0VseYzvE&sender=FRIZBY&to=" + receiver + "&message=" + msg + "&reqid=1";
            //string url = @"http://sms.xpertgroup.in/api/pushsms.php?username=XPRTGPRS&password=XPRTGPRS&sender=ODHDPL&message=" + msg + "&numbers=" + receiver + "&unicode=true";
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                string aaa = resp.ResponseUri.ToString();
                if (aaa.StartsWith("Wrong UserName or Password"))
                {
                    msg = "Message not Sent";
                }
                else
                {
                    msg = "Message Sent";
                }
            }
            catch (Exception)
            {

            }

        }
    }

    public class MonthName
    {
        public String numbertomonthname(int month)
        {

            String name = "";
            switch (month)
            {
                case 1:
                    name = "January";
                    break;
                case 2:
                    name = "February";
                    break;
                case 3:
                    name = "March";
                    break;
                case 4:
                    name = "April";
                    break;
                case 5:
                    name = "May";
                    break;
                case 6:
                    name = "June";
                    break;
                case 7:
                    name = "July";
                    break;
                case 8:
                    name = "August";
                    break;
                case 9:
                    name = "September";
                    break;
                case 10:
                    name = "October";
                    break;
                case 11:
                    name = "November";
                    break;
                case 12:
                    name = "December";
                    break;
            }
            return name;
        }
    }

    public class NumberToEnglish
    {
        public String changeToWords(String numb, bool isCurrency)
        {
            String val = "", wholeNo = numb, points = "", andStr = "", pointStr = "";
            String endStr = (isCurrency) ? ("Only") : ("");
            try
            {
                int decimalPlace = numb.IndexOf(".");
                if (decimalPlace > 0)
                {
                    wholeNo = numb.Substring(0, decimalPlace);
                    points = numb.Substring(decimalPlace + 1);
                    if (Convert.ToInt32(points) > 0)
                    {
                        andStr = (isCurrency) ? ("") : ("rupees");// just to separate whole numbers from points/cents

                        endStr = (isCurrency) ? ("paise " + endStr) : ("");
                        if (points.Length > 2)
                        {
                            points = points.Substring(0, 2);
                        }
                        pointStr = translateCents(points);
                    }
                }
                val = String.Format("{0} {1}{2} {3}", translateWholeNumber(wholeNo).Trim() + " Rupees", andStr + " ", pointStr, endStr);
            }
            catch { ;}
            return val;
        }
        private String translateWholeNumber(String number)
        {
            string word = "";
            try
            {
                bool beginsZero = false;//tests for 0XX

                double dblAmt = (Convert.ToDouble(number));
                //if ((dblAmt > 0) && number.StartsWith("0"))
                if (dblAmt > 0)
                {//test for zero or digit zero in a nuemric
                    beginsZero = number.StartsWith("0");
                    int numDigits = number.Length;
                    switch (numDigits)
                    {
                        case 1://ones' range
                            word = ones(number);
                            break;
                        case 2://tens' range
                            word = tens(number);
                            break;
                        case 3://hundreds' range
                            word = ones(number.Substring(0, 1)) + " Hundred  " + tens(number.Substring(1, 2));
                            break;
                        case 4://thousands' range
                            word = ones(number.Substring(0, 1)) + " Thousand ";
                            if (number.Substring(1, 1) != "0")
                            {
                                word += ones(number.Substring(1, 1)) + " Hundred  ";
                            }
                            if (number.Substring(2, 2) != "0")
                            {
                                word += tens(number.Substring(2, 2));
                            }
                            break;
                        case 5://ten thousand's range      
                            word = tens(number.Substring(0, 2)) + " Thousand ";
                            if (number.Substring(2, 1) != "0")
                            {
                                word += ones(number.Substring(2, 1)) + " Hundred  ";
                            }
                            if (number.Substring(3, 2) != "0")
                            {
                                word += tens(number.Substring(3, 2));
                            }
                            break;
                        case 6://lakh' range
                            word = ones(number.Substring(0, 1)) + " Lakh ";
                            if (number.Substring(1, 2) != "00")
                            {
                                word += tens(number.Substring(1, 2)) + " Thousand ";
                            }

                            if (number.Substring(3, 1) != "0")
                            {
                                word += ones(number.Substring(3, 1)) + " Hundred  ";
                            }
                            if (number.Substring(4, 2) != "00")
                            {
                                word += tens(number.Substring(4, 2));
                            }
                            break;
                        case 7://ten lakh's
                            word = tens(number.Substring(0, 2)) + " Lakh ";
                            if (number.Substring(2, 2) != "00")
                            {
                                word += tens(number.Substring(2, 2)) + " Thousand ";
                            }
                            if (number.Substring(4, 1) != "0")
                            {
                                word += ones(number.Substring(4, 1)) + " Hundred  ";
                            }
                            if (number.Substring(5, 2) != "00")
                            {
                                word += tens(number.Substring(5, 2));
                            }
                            break;
                        case 8://crore's range
                            word = ones(number.Substring(0, 1)) + " Crore ";
                            if (number.Substring(1, 2) != "00")
                            {
                                word += tens(number.Substring(1, 2)) + " Lakh ";
                            }
                            if (number.Substring(3, 2) != "00")
                            {
                                word += tens(number.Substring(3, 2)) + " Thousand ";
                            }
                            if (number.Substring(5, 1) != "0")
                            {
                                word += ones(number.Substring(5, 1)) + " Hundred  ";
                            }
                            if (number.Substring(6, 2) != "00")
                            {
                                word += tens(number.Substring(6, 2));
                            }
                            break;
                        case 9://ten crore's range
                            word = tens(number.Substring(0, 2)) + " Crore ";
                            if (number.Substring(2, 2) != "00")
                            {
                                word += tens(number.Substring(2, 2)) + " Lakh ";
                            }
                            if (number.Substring(4, 2) != "00")
                            {
                                word += tens(number.Substring(4, 2)) + " Thousand ";
                            }
                            if (number.Substring(6, 1) != "0")
                            {
                                word += ones(number.Substring(6, 1)) + " Hundred  ";
                            }
                            if (number.Substring(7, 2) != "00")
                            {
                                word += tens(number.Substring(7, 2));
                            }
                            break;
                        default:
                            word = "Zero";
                            break;
                    }

                }
                else
                {
                    word = "Zero";
                }
            }
            catch
            {

            }
            return word.Trim();
        }
        private String tens(String digit)
        {
            int digt = Convert.ToInt32(digit);
            String name = null;
            switch (digt)
            {
                case 10:
                    name = "Ten";
                    break;
                case 11:
                    name = "Eleven";
                    break;
                case 12:
                    name = "Twelve";
                    break;
                case 13:
                    name = "Thirteen";
                    break;
                case 14:
                    name = "Fourteen";
                    break;
                case 15:
                    name = "Fifteen";
                    break;
                case 16:
                    name = "Sixteen";
                    break;
                case 17:
                    name = "Seventeen";
                    break;
                case 18:
                    name = "Eighteen";
                    break;
                case 19:
                    name = "Nineteen";
                    break;
                case 20:
                    name = "Twenty";
                    break;
                case 30:
                    name = "Thirty";
                    break;
                case 40:
                    name = "Fourty";
                    break;
                case 50:
                    name = "Fifty";
                    break;
                case 60:
                    name = "Sixty";
                    break;
                case 70:
                    name = "Seventy";
                    break;
                case 80:
                    name = "Eighty";
                    break;
                case 90:
                    name = "Ninety";
                    break;
                default:
                    if (digt > 0)
                    {
                        name = tens(digit.Substring(0, 1) + "0") + " " + ones(digit.Substring(1));
                    }
                    break;
            }
            return name;
        }
        private String ones(String digit)
        {
            int digt = Convert.ToInt32(digit);
            String name = "";
            switch (digt)
            {
                case 1:
                    name = "One";
                    break;
                case 2:
                    name = "Two";
                    break;
                case 3:
                    name = "Three";
                    break;
                case 4:
                    name = "Four";
                    break;
                case 5:
                    name = "Five";
                    break;
                case 6:
                    name = "Six";
                    break;
                case 7:
                    name = "Seven";
                    break;
                case 8:
                    name = "Eight";
                    break;
                case 9:
                    name = "Nine";
                    break;
            }
            return name;
        }
        private String translateCents(String cents)
        {

            String cts = "";
            if (cents.Length == 2 && cents.Substring(0, 1) == "0")
            {
                cts = ones(cents.Substring(1, 1));
            }
            else if (cents.Length == 2)
            {
                if (Convert.ToInt32(cents) > 10 && Convert.ToInt32(cents) < 20)
                {
                    cts = tens(cents);
                }
                else
                {
                    cts = tens(cents.Substring(0, 1) + "0") + " " + ones(cents.Substring(1, 1));
                }
            }
            else
            {
                if (cents.Length == 1)
                {
                    cents += "0";
                }
                cts = tens(cents);
            }
            return cts;
        }
        public class Q
        {

            DataSet d = new DataSet();
            SqlConnection con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString);
            SqlCommand cmd = new SqlCommand();
            public DataSet ExQry(SqlCommand cmd, string commandTexrOrProcedurename)
            {
                cmd.Connection = con;
                cmd.CommandText = commandTexrOrProcedurename;
                using (SqlDataAdapter rdr = new SqlDataAdapter(cmd))
                {
                    rdr.Fill(d);
                }
                return d;
            }

            public string ExeQry(SqlCommand cmd, string commandTexrOrProcedurename)
            {
                string r;
                try
                {
                    cmd.Connection = con;
                    cmd.CommandText = commandTexrOrProcedurename;
                    con.Open();
                    r = cmd.ExecuteNonQuery().ToString();
                }
                catch (Exception ex) { r = "Error: " + ex.Message.Replace("'", "|"); }
                finally { con.Close(); }
                return r;
            }

        }
    }
}