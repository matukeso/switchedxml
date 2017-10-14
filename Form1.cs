using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Switchedxml
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            TCLog1 log = new TCLog1();
            log.Read(@"C:\Users\matuken\Documents\recved\広島フラ1日目.txt");

            XmlProject xp = new XmlProject();
            xp.Read(@"C:\Users\matuken\Documents\recved\4GB分割(Resolve).xml");


            foreach( Track t in xp.tracks)
            {
//                t.files
            }

            xp.RebuildByLength(log);   
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
