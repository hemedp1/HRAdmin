﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HRAdmin.Components
{
    public static class UserSession
    {
        public static string LoggedInUser { get; set; }
        public static string loggedInDepart { get; set; }
        public static string loggedInIndex { get; set; }
        public static string loggedInfullName { get; set; }
        public static string loggedInName { get; set; }
        public static string LoggedInBank { get; set; }
        public static string LoggedInAccNo { get; set; }
        public static string EventDetails { get; set; }
        public static DateTime? EventTime { get; set; }
        public static DateTime? DeliveryTime { get; set; }
        public static string OccasionType { get; set; }
        public static string pdfFilePath { get; set; }
        public static string fromDate { get; set; }
        public static string toDate { get; set; }
        public static DateTime? StartDate { get; set; }
        public static DateTime? EndDate { get; set; }

    }
}
