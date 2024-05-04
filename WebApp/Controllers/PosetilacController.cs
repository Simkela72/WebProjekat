using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using MySql.Data.MySqlClient;

namespace WebApp.Controllers
{
    public class PosetilacController : Controller
    {
        private string connectionString = "Server=localhost;Port=3306;Database=sys;Uid=root;Pwd=spajk2002;";
        public ActionResult Index()
        {
            List<FitnesCentar> fc = GetFitnesCentars();
            ViewBag.FitnesCentri = fc;
            return View();
        }

        private List<FitnesCentar> GetFitnesCentars()
        {
            List<FitnesCentar> fitnesCentars = new List<FitnesCentar>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM fitnes_centar";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        FitnesCentar fitnes_centar = new FitnesCentar()
                        {
                            Naziv = reader["Naziv"].ToString(),
                            Adresa = reader["Adresa"].ToString(),
                            GodinaOtvaranja = Convert.ToDecimal(reader["GodinaOtvaranja"]),
                            CenaMesec = Convert.ToDecimal(reader["Cena_mesec"]),
                            CenaGodina = Convert.ToDecimal(reader["Cena_godina"]),
                            JedanTrening = Convert.ToDecimal(reader["Jedan_trening"]),
                            GrupniTrening = Convert.ToDecimal(reader["Grupni_trening"]),
                            CenaPersonalTrener = Convert.ToDecimal(reader["Cena_personal_trener"]),
                            Vlasnik = reader["Vlasnik"].ToString()
                        };
                        fitnesCentars.Add(fitnes_centar);
                    }
                }
            }
            return fitnesCentars;
        }

        public ActionResult GroupSessionView(string fitnesCentarNaziv)
        {
            List<GrupniTrening> pendingTrainingSessions = GetPendingTrainingSessions(fitnesCentarNaziv);
            ViewBag.PendingTrainingSessions = pendingTrainingSessions;

            return View();
        }

        private List<GrupniTrening> GetPendingTrainingSessions(string fitnesCentarNaziv)
        {
            List<GrupniTrening> gt = new List<GrupniTrening>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM grupni_trening WHERE FitnesCentarNaziv = @fitCen AND Odrzan = 0";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@fitCen", fitnesCentarNaziv);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        GrupniTrening grupniTrening = new GrupniTrening()
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
                        gt.Add(grupniTrening);
                    }
                }
            }
            return gt;
        }

        public ActionResult GroupSession(int grupniTreningId)
        {
            string currentVisitor = GetCurrentVisitorUsername();
            string fitnesCentarNaziv = GetFitnessCenterName(grupniTreningId);


            bool alreadyJoined = CheckIfAlreadyJoined(grupniTreningId, currentVisitor);
            FitnesCentar fitnes_centar = new FitnesCentar();

            if (!alreadyJoined)
            {
                AddVisitorToSession(grupniTreningId, currentVisitor);
            }
            else
            {
                return RedirectToAction("GroupSessionView", new { fitnesCentarNaziv = fitnesCentarNaziv });

            }


            return RedirectToAction("GroupSessionView", new { fitnesCentarNaziv = fitnesCentarNaziv });

        }

        private bool CheckIfAlreadyJoined(int grupniTreningId, string currentVisitor)
        {

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM posetilac_trening WHERE Grupni_Trening = @grupniTreningId AND Posetilac = @currentVisitor";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@grupniTreningId", grupniTreningId);
                cmd.Parameters.AddWithValue("@currentVisitor", currentVisitor);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count > 0;
            }
        }

        private void AddVisitorToSession(int grupniTreningId, string currentVisitor)
        {

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO posetilac_trening (Grupni_Trening, Posetilac) VALUES (@grupniTreningId, @currentVisitor)";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@grupniTreningId", grupniTreningId);
                cmd.Parameters.AddWithValue("@currentVisitor", currentVisitor);
                cmd.ExecuteNonQuery();
            }
        }

        private string GetCurrentVisitorUsername()
        {
            string currentVisitor = Session["Username"] as string;
            return currentVisitor;
        }

        private string GetFitnessCenterName(int grupniTreningId)
        {
            string fitnesCentarNaziv = null;

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT FitnesCentarNaziv FROM grupni_trening WHERE GrupniTreningID = @grupniTreningId";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@grupniTreningId", grupniTreningId);

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        fitnesCentarNaziv = reader.GetString("FitnesCentarNaziv");
                    }
                }
            }

            return fitnesCentarNaziv;
        }

        public ActionResult CommentView(string fitnesCentarNaziv)
        {
            ViewBag.fitnesCentarNaziv = fitnesCentarNaziv;
            return View();
        }

        public ActionResult Comment(string comment_content, string FitnesCentar, int ocena)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO komentar (comment_content, FitnesCentar, odobren, ocena) VALUES (@comment_content, @FitnesCentar, @odobren, @ocena)";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@comment_content", comment_content);
                command.Parameters.AddWithValue("@FitnesCentar", FitnesCentar);
                command.Parameters.AddWithValue("@odobren", false);
                command.Parameters.AddWithValue("@ocena", ocena);
                command.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        public ActionResult CommentFitnesCenterView(string fitnesCentarNaziv)
        {
            List<Komentar> komentari = GetKomentarFitnesCentar(fitnesCentarNaziv);
            ViewBag.Komentari = komentari;
            return View();
        }

        private List<Komentar> GetKomentarFitnesCentar(string fitnesCentar)
        {
            List<Komentar> komentari = new List<Komentar>();
            using(MySqlConnection connection=new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM komentar WHERE FitnesCentar=@fc AND odobren=1";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@fc", fitnesCentar);
                using(MySqlDataReader reader = cmd.ExecuteReader()) 
                {
                    while (reader.Read())
                    {
                        Komentar komentar = new Komentar()
                        {
                            ID = reader.GetInt32("ID"),
                            comment_content=reader.GetString("comment_content"),
                            FitnesCentar=reader.GetString("FitnesCentar"),
                            odobren=reader.GetBoolean("odobren"),
                            ocena=reader.GetInt32("ocena")
                        };
                        komentari.Add(komentar);
                    }
                }
            }
            return komentari;
        }

        public ActionResult Logout()
        {

            Session.Clear();
            Session.Abandon();
            return RedirectToAction("Login", "Auth");
        }


    }
}