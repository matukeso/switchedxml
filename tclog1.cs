using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
                    string []sv = s.Split('=',',');
                    if (sv.Length != 9)
                        continue;
                    string svtext = sv[1].Replace(" QPL:", "");
                    int pgm=-1;
                    int.TryParse(svtext, out pgm);
                    if( pgm >= 0 && pgm != last_switch)
                    {
                        int frame = TCUtility.dateDfToFrame(sv[0]);
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
