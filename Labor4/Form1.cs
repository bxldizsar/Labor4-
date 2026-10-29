using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;


namespace Labor4
{
    // cerinta 2 - aplicatia din laboratorul anterior (lab 3, web browser) + meniul nou
    public partial class Form1 : Form
    {
        private List<string> blockedKeywords = new List<string>
        {
            "facebook",
            "tiktok",
            "casino"
        };

        // cerinta 4 - obiectul clasei noi, folosit din meniu
        private SQLiteHandler db = new SQLiteHandler();

        public Form1()
        {
            InitializeComponent();
        }

        // ---------------- lab 3: navigare ----------------

        private void NavigateToUrl()
        {
            string url = txtUrl.Text.Trim();
            if (url == "") return;

            // if they only type "google.com" we add the https:// part
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                url = "https://" + url;

            webBrowser1.Navigate(url);
        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            NavigateToUrl();
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (webBrowser1.CanGoBack)
                webBrowser1.GoBack();
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            if (webBrowser1.CanGoForward)
                webBrowser1.GoForward();
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            webBrowser1.GoHome();
        }

        private void txtUrl_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;   // no "ding" sound
                NavigateToUrl();
            }
        }

        // ---------------- lab 3: blocare url ----------------

        private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            string url = e.Url.ToString();

            bool blocked = IsBlockedAsync(url).Result;   // we wait for the task result here

            if (blocked)
            {
                e.Cancel = true;
                MessageBox.Show("Aceasta pagina este blocata!\n" + url);
            }
        }

        // linq + task: is there any keyword inside the url?
        private Task<bool> IsBlockedAsync(string url)
        {
            List<string> copy = blockedKeywords.ToList();   // copy so it doesnt change meanwhile
            string lowerUrl = url.ToLower();

            return Task.Run(() => copy.Any(k => lowerUrl.Contains(k.ToLower())));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            Trace.Listeners.Add(new TextWriterTraceListener("log.txt"));
            Trace.AutoFlush = true;

            Log("Aplicatia a pornit");
            RefreshCombo();
            webBrowser1.GoHome();
        }

        private void Log(string message)
        {
            string line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + " | " + message;
            Task.Run(() => Trace.WriteLine(line));   // not awaited
        }

        private async void btnAddKeyword_Click(object sender, EventArgs e)
        {
            await AddKeyword();
        }

        private async void txtKeyword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await AddKeyword();
            }
        }

        private async Task AddKeyword()
        {
            string keyword = txtKeyword.Text.Trim().ToLower();
            if (keyword == "")
            {
                MessageBox.Show("Introduceți un cuvânt cheie!");
                return;
            }

            // linq: check if its already in the list
            List<string> copy = blockedKeywords.ToList();
            bool exists = await Task.Run(() =>
                copy.Any(k => k.Equals(keyword, StringComparison.OrdinalIgnoreCase)));

            if (exists)
            {
                MessageBox.Show("Cuvântul \"" + keyword + "\" există deja în listă!");
                return;
            }

            blockedKeywords.Add(keyword);
            RefreshCombo();
            cmbKeywords.SelectedItem = keyword;
            txtKeyword.Clear();
            Log("Cuvânt cheie adăugat: " + keyword);
        }

        private void RefreshCombo()
        {
            cmbKeywords.Items.Clear();
            foreach (string k in blockedKeywords.OrderBy(k => k))   // linq: abc order
                cmbKeywords.Items.Add(k);

            if (cmbKeywords.Items.Count > 0)
                cmbKeywords.SelectedIndex = 0;
        }

        // ---------------- lab 4 ----------------

        // cerinta 2 + 4 - Database -> Connect: apeleaza ConnectToDb()
        private void connectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                db.ConnectToDb();
                Log("Conectat la baza de date");

                // the words from the db also go into the block list, so they actually block pages
                foreach (string k in db.GetAllKeywords())
                {
                    if (!blockedKeywords.Contains(k))
                        blockedKeywords.Add(k);
                }
                RefreshCombo();

                MessageBox.Show("Conectat la baza de date!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Eroare: " + ex.Message);
            }
        }

        // cerinta 2 + 4 - Database -> Disconnect: apeleaza DisconnectFromDb()
        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            db.DisconnectFromDb();
            Log("Deconectat de la baza de date");
            MessageBox.Show("Deconectat!");
        }

        // cerinta 2 + 4 - Database -> AddKeyword: form nou cu ShowDialog(), salvare doar la OK
        private void addKeywordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // cant add anything if were not connected
            if (!db.IsConnected)
            {
                MessageBox.Show("Mai intai conecteaza-te la baza de date!");
                return;
            }

            KeywordInput kwdForm = new KeywordInput();

            // cerinta 4 - verificam ce returneaza ShowDialog(), doar la OK salvam
            if (kwdForm.ShowDialog() == DialogResult.OK)
            {
                string kwd = kwdForm.getInputText().Trim().ToLower();

                if (kwd == "")
                {
                    MessageBox.Show("Introduceti un cuvant cheie!");
                    return;
                }

                db.InsertKeyword(kwd);
                Log("Cuvant cheie salvat in DB: " + kwd);

                // also put it in the block list
                if (!blockedKeywords.Contains(kwd))
                {
                    blockedKeywords.Add(kwd);
                    RefreshCombo();
                }

                MessageBox.Show(string.Format("S-a introdus: {0}", kwd));
            }
        }

        // cerinta 2 + 4 - Database -> ViewKeywords: apeleaza GetAllKeywords()
        private void viewKeywordsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!db.IsConnected)
            {
                MessageBox.Show("Mai intai conecteaza-te la baza de date!");
                return;
            }

            List<string> keywords = db.GetAllKeywords();

            if (keywords.Count == 0)
            {
                MessageBox.Show("Nu exista cuvinte cheie in baza de date.");
                return;
            }

            // every keyword goes on a new line
            MessageBox.Show(string.Join("\n", keywords));
        }

        // cerinta 2 - Help -> About
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Web browser cu blocare de cuvinte cheie\nLaborator 4 - MIP (ADO.NET + SQLite)");
        }
    }

}
