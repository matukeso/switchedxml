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

        TCLog1 m_tclog = new TCLog1();
        XmlProject m_xp = new XmlProject();

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            string []files = drgevent.Data.GetData(DataFormats.FileDrop) as string[];
            foreach( String f in files)
            {
                string fu = f.ToUpper();
                if (fu.EndsWith(".XML"))
                {
                    LoadProject(f);
                }
                if(fu.EndsWith(".EZP"))
                {
                    LoadEdiusProject(f);

                }
                if (fu.EndsWith(".TXT"))
                {
                    LoadLog(f);
                }
                if (fu.EndsWith(".LOG"))
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
            m_tclog.Read(f);


            var logtexts = from p in m_tclog
                           select string.Format("{0}={1} - {2}",
                                p.ch, TCUtility.DfFrameToDate(p.start_tc), p.length);
            listBox1.Items.Clear();
            listBox1.Items.AddRange(logtexts.ToArray<string>());


        }

        void LoadProject(string f)
        {
            m_xp.Read(f);

            RefreshXPListbox();
        }

        private void LoadEdiusProject(string f)
        {
            EdiusProject ep = new Switchedxml.EdiusProject();
            ep.ReadProject(f);

            m_xp.Read(ep);
            RefreshXPListbox();

            this.lblProjectTC.Text = String.Format("{0} = {1}f",
                TCUtility.DfFrameToDate(ep.ProjectTC), ep.ProjectTC.ToString());
            this.Text = "switchedxml - " + m_xp.title;

        }

        private void RefreshXPListbox()
        {
            ListBox[] lists = new ListBox[] { listBox2, listBox3, listBox4, listBox5 };


            int ii = 0;
            foreach (Track track in m_xp.tracks)
            {
                var tracklog = from t in track.files
                               select string.Format("{1}, {2}, {0}",
                                    t.fe.name, TCUtility.DfFrameToDate(t.f_start_tc), t.f_length);

                string[] logs = tracklog.ToArray<string>();
                if (ii < lists.Length)
                {
                    lists[ii].Items.Clear();
                    lists[ii].Items.AddRange(logs);
                }
                if (logs.Length > 0)
                {
                    ii++;
                }
                //                t.files
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            SaveWindowPosition();

            foreach ( FileElement f  in m_xp.all_files)
            {
                f.m_bfirst = true;
            }
            Track.uniqueid = 0;

            m_xp.RebuildByLength(m_tclog);   
        private void SaveWindowPosition()
        {
            using (Microsoft.Win32.RegistryKey regkey =
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\ACC\SwitchedXML"))
            {
                Rectangle rect = (WindowState == FormWindowState.Normal) ? DesktopBounds : RestoreBounds;
                regkey.SetValue("WindowPosition", String.Format("{0},{1},{2},{3},{4}",
                    (int)this.WindowState,
                    rect.Left, rect.Top, rect.Width, rect.Height));
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
