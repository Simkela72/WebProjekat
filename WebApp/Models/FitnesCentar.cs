using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class FitnesCentar
    {
        public string Naziv { get; set; }
        public string Adresa { get; set; }
        public decimal? GodinaOtvaranja { get; set; }
        public decimal? CenaMesec { get; set; }
        public decimal? CenaGodina { get; set; }
        public decimal? JedanTrening { get; set; }
        public decimal? GrupniTrening { get; set; }
        public decimal? CenaPersonalTrener { get; set; }
        public string Vlasnik { get; set; }
    }
}