using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class GrupniTrening
    {
        public int GrupniTreningID { get; set; }
        public string FitnesCentarNaziv { get; set; }
        public TipTreninga TipTreninga { get; set; }
        public string FitnesCentar { get; set; }
        public int Trajanje { get; set; }
        public DateTime? DatumOdrzavanja { get; set; }
        public int MaxPosetilaca { get; set; }
        public string Trener { get; set; }
        public bool Odrzan { get; set; }
    }

    public enum TipTreninga
    {
        Yoga=1,
        LesMillsTone=2,
        BodyPump=3
    }
}