using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Labor4
{
    // cerinta 4 - form nou pt introducerea unui keyword: eticheta, textbox, butoane OK si Cancel
    // (OK are DialogResult = OK, Cancel are DialogResult = Cancel, MaximizeBox = false)
    public partial class KeywordInput : Form
    {
        public KeywordInput()
        {
            InitializeComponent();
        }

        // cerinta 4 - metoda de acces la textul introdus
        public string getInputText()
        {
            return textBox1.Text;
        }
    }
}
