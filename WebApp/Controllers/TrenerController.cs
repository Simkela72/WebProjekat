using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MySql.Data.MySqlClient;
using WebApp.Models;

namespace WebApp.Controllers
{
    public class TrenerController : Controller
    {
        private string connectionString = "Server=localhost;Port=3306;Database=sys;Uid=root;Pwd=spajk2002;";

        
        public ActionResult Index()
        {
            string trener = Session["Username"] as string;
            ViewBag.trener = trener;
            string fitnes_centar = GetFitnesCentarName(trener);
            ViewBag.fitnes_centar = fitnes_centar;
            List<GrupniTrening> odrzani_treninzi = GetDoneTrainingSessions(trener);
            ViewBag.odrzani = odrzani_treninzi;
            List<GrupniTrening> neodrzani_Treninzi = GetPendingTrainingSessions(trener);
            ViewBag.neodrzani = neodrzani_Treninzi;

            return View();
        }

        [HttpPost]
        public ActionResult AddTrainingSession(string FitCen, TipTreninga tip, int Trajanje, DateTime Dat, int Max, string Tren)
        {
            if (Dat <= DateTime.Today.AddDays(3))
            {
                return RedirectToAction("Index");
            }
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO grupni_trening (FitnesCentarNaziv, TipTreninga, Trajanje, DatumOdrzavanja, MaxPosetilaca, Trener, Odrzan) VALUES (@FitnesCentarNaziv, @TipTreninga, @Trajanje, @DatumOdrzavanja, @MaxPosetilaca, @Trener, 0)";

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@FitnesCentarNaziv", FitCen);
                    cmd.Parameters.AddWithValue("@TipTreninga", tip.ToString());
                    cmd.Parameters.AddWithValue("@Trajanje", Trajanje);
                    cmd.Parameters.AddWithValue("@DatumOdrzavanja", Dat);
                    cmd.Parameters.AddWithValue("@MaxPosetilaca", Max);
                    cmd.Parameters.AddWithValue("@Trener", Tren);
                    cmd.ExecuteNonQuery();
                }
            }
            return RedirectToAction("Index");
        }

        private string GetFitnesCentarName(string trener)
        {
            string fitnes_centar = "";
            using(MySqlConnection connection=new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM trener_centar WHERE Trener=@trener";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@trener", trener);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        fitnes_centar = reader["Fitnes_Centar"].ToString();
                    }
                }
            }
            return fitnes_centar;
        }

        private List<GrupniTrening> GetPendingTrainingSessions(string trener)
        {
            List<GrupniTrening> treninzi = new List<GrupniTrening>();
            using(MySqlConnection connection=new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM grupni_trening WHERE Odrzan=0 AND Trener=@tre";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@tre",trener);
                using(MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string statusString = reader["TipTreninga"].ToString();
                        Enum.TryParse<TipTreninga>(statusString, out TipTreninga tip);
                        GrupniTrening trening = new GrupniTrening()
                        {
                            GrupniTreningID = reader.GetInt32("GrupniTreningID"),
                            FitnesCentarNaziv = reader.GetString("FitnesCentarNaziv"),
                            TipTreninga = tip,
                            Trajanje = reader.GetInt32("Trajanje"),
                            DatumOdrzavanja = reader.IsDBNull(reader.GetOrdinal("DatumOdrzavanja")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("DatumOdrzavanja")),
                            MaxPosetilaca = reader.GetInt32("MaxPosetilaca"),
                            Trener = reader.GetString("Trener"),
                            Odrzan = reader.GetBoolean("Odrzan")
                        };
                        treninzi.Add(trening);
                    }
                }
            }
            return treninzi;
        }

        private List<GrupniTrening> GetDoneTrainingSessions(string trener)
        {
            List<GrupniTrening> treninzi = new List<GrupniTrening>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM grupni_trening WHERE Odrzan=1 AND Trener=@tre";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@tre", trener);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string statusString = reader["TipTreninga"].ToString();
                        Enum.TryParse<TipTreninga>(statusString, out TipTreninga tip);
                        GrupniTrening trening = new GrupniTrening()
                        {
                            GrupniTreningID = reader.GetInt32("GrupniTreningID"),
                            FitnesCentarNaziv = reader.GetString("FitnesCentarNaziv"),
                            TipTreninga = tip,
                            Trajanje = reader.GetInt32("Trajanje"),
                            DatumOdrzavanja = reader.IsDBNull(reader.GetOrdinal("DatumOdrzavanja")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("DatumOdrzavanja")),
                            MaxPosetilaca = reader.GetInt32("MaxPosetilaca"),
                            Trener = reader.GetString("Trener"),
                            Odrzan = reader.GetBoolean("Odrzan")
                        };
                        treninzi.Add(trening);
                    }
                }
            }
            return treninzi;
        }

        [HttpPost]
        public ActionResult DeleteSession(int TrainingId)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM grupni_trening WHERE GrupniTreningID=@gr_tr";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@gr_tr", TrainingId);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        public ActionResult ModifySessionView(int TrainingId)
        {
            GrupniTrening grupniTrening = new GrupniTrening();
            using(MySqlConnection connection=new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM grupni_trening WHERE GrupniTreningID=@id";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", TrainingId);
                using(MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        grupniTrening = new GrupniTrening
                        {
                            GrupniTreningID = reader.GetInt32("GrupniTreningID"),
                            FitnesCentarNaziv = reader.GetString("FitnesCentarNaziv"),
                            TipTreninga = (TipTreninga)Enum.Parse(typeof(TipTreninga), reader.GetString("TipTreninga")),
                            Trajanje = reader.GetInt32("Trajanje"),
                            DatumOdrzavanja = reader.GetDateTime("DatumOdrzavanja"),
                            MaxPosetilaca = reader.GetInt32("MaxPosetilaca"),
                            Trener = reader.GetString("Trener"),
                            Odrzan = reader.GetBoolean("Odrzan")
                        };
                    }
                }

                
            }
            return View(grupniTrening);
        }

        [HttpPost]
        public ActionResult ModifySession(GrupniTrening grupni_trening)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "UPDATE grupni_trening " +
                               "SET TipTreninga = @TipTreninga, " +
                               "    Trajanje = @Trajanje, " +
                               "    DatumOdrzavanja = @DatumOdrzavanja, " +
                               "    MaxPosetilaca = @MaxPosetilaca " +
                               "WHERE GrupniTreningID = @GrupniTreningID";

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@TipTreninga", grupni_trening.TipTreninga);
                    cmd.Parameters.AddWithValue("@Trajanje", grupni_trening.Trajanje);
                    cmd.Parameters.AddWithValue("@DatumOdrzavanja", grupni_trening.DatumOdrzavanja);
                    cmd.Parameters.AddWithValue("@MaxPosetilaca", grupni_trening.MaxPosetilaca);
                    cmd.Parameters.AddWithValue("@GrupniTreningID", grupni_trening.GrupniTreningID);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult SeeAllSubscribers(string trainingId)
        {
            List<string> pretplatnici = new List<string>();
            using(MySqlConnection connection= new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM posetilac_trening WHERE Grupni_Trening=@GrTr";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@GrTr", trainingId);
                using(MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pretplatnici.Add(reader["Posetilac"].ToString());
                    }
                }

            }
            ViewBag.Pretplatnici = pretplatnici;
            return View();
        }

    }
}