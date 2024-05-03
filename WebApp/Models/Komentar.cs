using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class Komentar
    {
        public int ID { get; set; }
        public string comment_content { get; set; }
        public string FitnesCentar { get; set; }
        public bool? odobren { get; set; }
        public int ocena { get; set; }
    }
}