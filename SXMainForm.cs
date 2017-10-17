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
    public partial class SXMainForm : Form
    {
        public SXMainForm()
        {
            InitializeComponent();
            this.AllowDrop = true;
        }

        TCLog1 log = new TCLog1();
        XmlProject xp = new XmlProject();

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            string []files = drgevent.Data.GetData(DataFormats.FileDrop) as string[];
            foreach( String f in files)
            {
                if (f.ToUpper().EndsWith(".XML"))
                {
                    LoadProject(f);
                }
                if (f.ToUpper().EndsWith(".TXT"))
                {
                    LoadLog(f);
                }
            }

        }
        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            drgevent.Effect = DragDropEffects.Link;
        }

        void LoadLog(string f)
        {
            //@"C:\Users\matuken\Documents\recved\広島フラ1日目.txt"
            log.Read(f);


            var logtexts = from p in log
                           select string.Format("{0}={1} - {2}",
                                p.ch, TCUtility.DfFrameToDate(p.start_tc), p.length);
            listBox1.Items.Clear();
            listBox1.Items.AddRange(logtexts.ToArray<string>());


        }

        void LoadProject(string f)
        {
            xp.Read(f);

            ListBox[] lists = new ListBox[] { listBox2, listBox3, listBox4, listBox5 };


            int ii = 0;
            foreach (Track track in xp.tracks)
            {
                var tracklog = from t in track.files
                               select string.Format("{1}, {2}, {0}",
                                    t.fe.name, TCUtility.DfFrameToDate(t.f_start_tc), t.f_length);
                lists[ii].Items.Clear();
                lists[ii].Items.AddRange(tracklog.ToArray<string>());
                ii++;
                //                t.files
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            foreach( FileElement f  in xp.all_files)
            {
                f.m_bfirst = true;
            }
            Track.uniqueid = 0;

            xp.RebuildByLength(log);   
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
