using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ODHDEVELOPERS.Models
{
    public class GetLogged
    {
        public static string logId { get; set; }
        public static string logType { get; set; }
    }

    public class LogNames {
        public readonly static string
            Company = "Company",
            Admin = "Admin",
            Branch = "Branch",
            Operator = "Operator",
            Agent = "Agent",
            HR = "HR",
            Customer = "Customer";
    }
}