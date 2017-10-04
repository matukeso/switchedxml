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
            XmlProject xp = new XmlProject();

            int i = 0;
            foreach ( var f in xp.files )
            {
                listBox1.Items.Add( string.Format("Tr{0}={1}\t{2}", ++i, f.duration, f.RealPath()));
            }

            xp.lengths.Add( new  KeyValuePair<FileElement, int>( xp.lengths[0].Key, 100));
            xp.RebuildByLength();   
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
