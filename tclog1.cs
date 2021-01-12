using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Switchedxml
{
    public struct TCLogElement
    {
        public int start_tc;
        public int length;
        public int ch;

        public TCLogElement(int start, int len , int ch)
        {
            this.start_tc = start;
            this.length = len;
            this.ch = ch;
        }
        public override string ToString()
        {
            return string.Format("{0}+{1}={2}", TCUtility.DfFrameToDate(start_tc),  length, ch);
        }
    }


    public class TCLog1 : List<TCLogElement>
    {
        public TCLog1() { }


        static Tuple<int , int> ParseLine(string s)
        {
            string s_without_s = s.Replace("S", "");
            string[] sv = s_without_s.Split('=', ',');
            if (sv.Length != 9)
            {
                sv = s_without_s.Split(' ', ',');
                if (sv.Length != 3)
                    return new Tuple<int, int>(0,0);
            }
            string svtext = sv[1].Replace(" QPL:", "").Replace("QPL:", "");
            int pgm = -1;
            int.TryParse(svtext, out pgm);
            return new Tuple<int, int>(TCUtility.dateDfToFrame30(sv[0]), pgm);
        }


        public void Read( string path)
        {
            this.Clear();
            using (StreamReader tr = new StreamReader(path, Encoding.ASCII))
            {
                int last_switch = -1;
                int prev_f = -1;
                while (true)
                {
                    string s = tr.ReadLine();
                    if (s == null)
                        break;

                    int frame;
                    int pgm;
                    var p = ParseLine(s);
                    frame = p.Item1;
                    pgm = p.Item2;

                    if( pgm >= 0 && pgm != last_switch)
                    {
                        if (prev_f != -1)
                        {
                            this.Add(new TCLogElement(prev_f, frame - prev_f, last_switch));
                        }
                        last_switch = pgm;
                        prev_f = frame;
                    }
                }
                if (prev_f > 0 && this.Count > 0)
                {
                    this.Add(new TCLogElement(prev_f, -1, last_switch));
                }
            }


        }


    }
}
