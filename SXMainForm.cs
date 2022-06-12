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
        string original_project_filename;

        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            string []files = drgevent.Data.GetData(DataFormats.FileDrop) as string[];
            foreach( String f in files)
            {
                string fu = f.ToUpper();
                if (fu.EndsWith(".XML"))
                {
                    LoadProject(f);
                    original_project_filename = f;
                }
                if(fu.EndsWith(".EZP"))
                {
                    LoadEdiusProject(f);
                    original_project_filename = f;
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

        PureEdiusProject edius_prj;
        private void LoadEdiusProject(string f)
        {
            EdiusProject ep = new Switchedxml.EdiusProject();
            ep.ReadProject(f);
            edius_prj = ep.prj;

            comboBox1.Items.Clear();
            foreach (var v in edius_prj.timelines)
            {
                comboBox1.Items.Add(v);
                button2.Enabled = true;
            }

            m_xp.Read(ep);
            RefreshXPListbox();

            this.lblProjectTC.Text = String.Format("{0} = {1}f",
                TCUtility.DfFrameToDate(ep.ProjectTC), ep.ProjectTC.ToString());
            this.Text = "switchedxml - " + m_xp.title;

        }
        private void button2_Click(object sender, EventArgs e)
        {
            EdiusTimeline t = (EdiusTimeline)(comboBox1.SelectedItem);

            EdiusProject ep = new Switchedxml.EdiusProject();
            ep.ReadProject(original_project_filename, t.guid);

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
        System.Media.SoundPlayer wav = new System.Media.SoundPlayer(@"c:\Windows\Media\Speech On.wav");


        private void button1_Click(object sender, EventArgs e)
        {
            SaveWindowPosition();

            foreach ( FileElement f  in m_xp.all_files)
            {
                f.m_bfirst = true;
            }
            Track.uniqueid = 0;

            int dif_tc_60 = (int)numericUpDown1.Value;

            TCLog1 newlog = new TCLog1();
            for( int i=0; i<m_tclog.Count; i++)
            {
                TCLogElement le = m_tclog[i];
                le.start_tc += dif_tc_60;
                newlog.Add(le);
            }

            System.Xml.Linq.XDocument doc =m_xp.RebuildByLength(newlog);
            string filedir = System.IO.Path.GetDirectoryName(original_project_filename);
            string filebase = System.IO.Path.GetFileNameWithoutExtension(original_project_filename);
            try
            {
                EdiusTimeline t = (EdiusTimeline)(comboBox1.SelectedItem);
                filebase += "_" + t.name;
            }
            catch (Exception ex) { }
            string tcxml = System.IO.Path.Combine(filedir, filebase + "_tc.xml");
            doc.Save(tcxml);

            wav.Play();
        }
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

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                int frame = TCUtility.dateDfToFrame30((sender as Control).Text);
                label1.Text = $"{frame.ToString()} ";

                int tc = m_xp.project_tc.Frame;
                label1.Text = $"{frame.ToString()} {frame-tc}";

            }
            catch (Exception ex)
            { }
        }


    }
}
