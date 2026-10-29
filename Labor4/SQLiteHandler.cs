using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labor4
{
    // cerinta 3 - clasa noua SQLiteHandler
    public class SQLiteHandler
    {
        private SQLiteConnection conn;

        // true if the connection is open
        public bool IsConnected
        {
            get { return conn != null && conn.State == System.Data.ConnectionState.Open; }
        }

        // cerinta 3 - ConnectToDb(): conectare, baza de date si tabelul Keywords se creeaza automat daca nu exista
        public void ConnectToDb()
        {
            // already connected, nothing to do
            if (IsConnected) return;

            // the db file gets created automatically if it doesnt exist
            conn = new SQLiteConnection("Data Source=keywords.db;Version=3;");
            conn.Open();

            // table Keywords: id integer primary key autoincrement, keyword text
            string sql = "CREATE TABLE IF NOT EXISTS Keywords " +
                         "(id INTEGER PRIMARY KEY AUTOINCREMENT, keyword TEXT)";
            SQLiteCommand cmd = new SQLiteCommand(sql, conn);
            cmd.ExecuteNonQuery();
        }

        // cerinta 3 - DisconnectFromDb(): inchiderea conexiunii
        public void DisconnectFromDb()
        {
            if (IsConnected)
                conn.Close();
        }

        // cerinta 3 - InsertKeyword(string k): adaugarea unui cuvant cheie
        public void InsertKeyword(string k)
        {
            // @k is a parameter, safer than putting the text in the sql directly
            string sql = "INSERT INTO Keywords (keyword) VALUES (@k)";
            SQLiteCommand cmd = new SQLiteCommand(sql, conn);
            cmd.Parameters.AddWithValue("@k", k);
            cmd.ExecuteNonQuery();
        }

        // cerinta 3 - GetAllKeywords(): lista de string-uri cu toate cuvintele cheie
        public List<string> GetAllKeywords()
        {
            List<string> list = new List<string>();

            string sql = "SELECT keyword FROM Keywords";
            SQLiteCommand cmd = new SQLiteCommand(sql, conn);
            SQLiteDataReader reader = cmd.ExecuteReader();

            // read row by row
            while (reader.Read())
                list.Add(reader.GetString(0));

            reader.Close();
            return list;
        }
    }
}
