using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Switchedxml
{
    public class TimeCode
    {
        string tcstr;
        int tcframe;
        public override string ToString()
        {
            return TCUtility.DfFrameToDate(tcframe);
        }
        public int Frame {  get { return tcframe; } }
        public TimeCode(XElement xe)
        {
            tcstr = xe.Element("string").Value.ToString();

            XElement xeframe = xe.Element("frame");
            if (xeframe != null)
            {
                int.TryParse(xeframe.Value.ToString(), out tcframe);

                int timebase = 30;
                int.TryParse(xe.XPathSelectElement("rate/timebase").Value.ToString(), out timebase);
                if (timebase != 0)
                {
                    tcframe = tcframe * 30 / timebase;
                }
            }
            else
            {
                tcframe = TCUtility.dateDfToFrame(tcstr);
            }
        }
    }

    public class FileElement
    {

        public string id { set; get; }
        public string pathurl { set; get; }

        public string name { get; set; }

        public int duration { get; set; }

        public string timecode { get; set; }

        TimeCode filetc;

        public int tc {  get { return TCUtility.dateDfToFrame(timecode); } }
        public FileElement(XElement elem)
        {
            id = elem.Attribute("id").Value;
            pathurl = elem.Element("pathurl").Value;
            name = elem.Element("name").Value;
            duration = int.Parse(elem.Element("duration").Value);
            timecode = elem.XPathSelectElement("timecode/string").Value;
            filetc = new TimeCode(elem.XPathSelectElement("timecode"));
        }

        public string RealPath()
        {
            string src = pathurl.Replace("file://localhost/", "");
            byte[] barr = new byte[src.Length];

            int di = 0;
            int percent = -1;
            for (int si = 0; si < src.Length; si++)
            {
                char ch = src[si];
                if (percent == 1)
                {
                    barr[di] = (byte)(byte.Parse(ch.ToString(), System.Globalization.NumberStyles.HexNumber) << (4));
                    percent--;
                    continue;
                }
                if (percent == 0)
                {
                    barr[di] |= (byte)(byte.Parse(ch.ToString(), System.Globalization.NumberStyles.HexNumber));
                    percent--;
                    di++;
                    continue;
                }
                if (src[si] == '%')
                {
                    percent = 1;
                    continue;
                }
                barr[di++] = (byte)ch;
            }
            return Encoding.UTF8.GetString(barr, 0, di - 1);
        }

        public Boolean m_bfirst = false;
        public void AppendNode(XElement clip)
        {
            //<file id="00157.MTS 2">
            //    <duration>975</duration>
            //    <rate>
            //        <timebase>30</timebase>
            //        <ntsc>true</ntsc>
            //    </rate>
            //    <name>00157.MTS</name>
            //    <pathurl>file://localhost/W:/%E9%87%91%E5%B1%B1%E3%83%95%E3%83%A9201707/G20/Cards/2017-07-19/0001/PRIVATE/AVCHD/BDMV/STREAM/00157.MTS</pathurl>
            //    <timecode>
            //        <string>00:00:00:00</string>
            //        <displayformat>DF</displayformat>
            //        <rate>
            //            <timebase>30</timebase>
            //            <ntsc>true</ntsc>
            //        </rate>
            //    </timecode>
            //    <media>
            //        <video>
            //            <duration>975</duration>
            //            <samplecharacteristics>
            //                <width>1920</width>
            //                <height>1080</height>
            //            </samplecharacteristics>
            //        </video>
            //    </media>
            //</file>
            XElement xe = new XElement("file");
            xe.SetAttributeValue("id", id);
            if (m_bfirst)
            {
                xe.Add(
                    new XElement("duration", duration),
                        GenRate30ntsc(),
                        new XElement("name", name),
                        new XElement("pathurl", pathurl),
                        new XElement("timecode",
                            new XElement("string", timecode),
                            new XElement("displayformat", "DF"),
                            GenRate30ntsc()
                            ),
                        new XElement("media",
                            new XElement("video",
                                new XElement("duration", duration),
                                new XElement("samplecharacteristics",
                                    new XElement("width", 1920),
                                    new XElement("height", 1080)
                                )
                            )
                        )
                   );
                m_bfirst = false;
            }
            clip.Add(xe);
        }

        public static XElement GenRate30ntsc()
        {
            return new XElement("rate", 
                new XElement("timebase", 30),
                new XElement("ntsc", "true")
            );
        }
    }

    public class FileElements : List<FileElement>
    {
        public FileElement FindById(string fileref)
        {
            return this.Find(f => f.id == fileref);
        }

    }
}
