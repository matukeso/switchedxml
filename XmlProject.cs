using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Switchedxml
{
    public class ClipItem
    {
        public string id;
        public string name;
        public int duration;
        public int start;
        public int end;
        public bool enabled;
        public int inf;
        public int outf;
        public string fileref;

        public ClipItem()
        {
        }
        public ClipItem(XElement xe)
        {
            id = xe.Attribute("id").Value;
            duration = int.Parse(xe.Element("duration").Value);
            start = int.Parse(xe.Element("start").Value);
            end = int.Parse(xe.Element("end").Value);
            enabled = xe.Element("enabled").Value == "true";
            inf = int.Parse(xe.Element("in").Value);
            outf = int.Parse(xe.Element("in").Value);
            fileref = xe.Element("file").Attribute("id").Value;

        }
        public override string ToString()
        {
            return string.Format("{0}:{1}-{2}", fileref, start, end);
        }
        public void AppendNode(XElement video, FileElement fe)
        {
            XElement clip = new XElement("clipitem");
            clip.SetAttributeValue("id", id);
            //<clipitem id="00157.MTS 3">
            //    <name>00157.MTS</name>
            //    <duration>975</duration>
            //    <rate>
            //        <timebase>30</timebase>
            //        <ntsc>true</ntsc>
            //    </rate>
            //    <start>126</start>
            //    <end>377</end>
            //    <enabled>false</enabled>
            //    <in>126</in>
            //    <out>377</out>
            //    <file id="00157.MTS 2"/>
            //    <compositemode>normal</compositemode>
            //</clipitem>

            clip.Add(
                 new XElement("name", name)
                ,new XElement("duration", duration)
                ,FileElement.GenRate30ntsc()
                ,new XElement("start", start)
                ,new XElement("end", end)
                ,new XElement("enabled", enabled ? "true" : "false")
                ,new XElement("in", inf)
                ,new XElement("out", outf)
             );
            if (fe == null)
            {
                XElement xe = new XElement("file");
                xe.SetAttributeValue("id", fileref);
                clip.Add(xe);
            }
            else
            {
                fe.AppendNode(clip);
            }
            clip.Add(new XElement("compositemode", "normal"));


            video.Add(clip);
        }

    }
    public class Track : List<ClipItem>
    {
        string fileref;
        public Track(XElement elem)
        {
            foreach (XElement xe in elem.Elements("clipitem"))
            {
                this.Add(new ClipItem(xe));
            }
            if (this.Count > 0)
            {
                fileref = this[0].fileref;
            }
        }
        static public XElement CreateTrackNode( List<KeyValuePair<FileElement, int>> lengths, FileElement fe)
        {
            XElement track = new XElement("track");
            int frame = 0;
            foreach (var kv in lengths)
            {
                ClipItem v = new ClipItem();
                if (fe != null)
                {
                    v.id = fe.name + kv.Value.ToString();
                    v.fileref = fe.id;
                    v.duration = fe.duration;
                    v.name = fe.name;
                }
                else
                {
                    v.id = "trM" + kv.Value.ToString();
                    v.fileref = kv.Key.id;
                    v.duration = kv.Key.duration;
                    v.name = kv.Key.name;
                }
                v.inf = frame;
                v.outf = frame + kv.Value;
                v.start = frame;
                v.end = frame + kv.Value;
                v.enabled = kv.Key == fe;

                v.AppendNode(track, fe);

                frame += kv.Value;
            }
            track.Add(new XElement("enabled", "true"));
            track.Add(new XElement("locked", "false"));
            return track;
        }
    }

    public class XmlProject
    {
        XDocument xd;

        public string title { set; get; }
        public List<FileElement> files = new List<FileElement>();
        public List<Track> tracks = new List<Track>();
        public List<KeyValuePair<FileElement, int>> lengths = new List<KeyValuePair<FileElement, int>>();

        public XmlProject()
        {
            xd = XDocument.Parse(System.IO.File.ReadAllText(@"C:\Users\matuken\Documents\recved\21test_nofilter_pn.xml"));
            //            XElement first_track = xd.XPathSelectElement(/)
            foreach (XElement xe in xd.XPathSelectElements("//video/track"))
            {
                tracks.Add(new Track(xe));
            }
            foreach (XElement xe in xd.XPathSelectElements("//file[pathurl]"))
            {
                files.Add(new FileElement(xe));
            }
            foreach (ClipItem c in tracks.Last())
            {
                FileElement fe = files.Find(f => f.id == c.fileref);

                lengths.Add(new KeyValuePair<FileElement, int>(fe, c.end - c.start));

            }
        }


        public void RebuildByLength()
        {
            int totla_length = lengths.Sum(s => s.Value);
            xd.XPathSelectElement("//sequence/duration").SetValue(totla_length);

            XElement video = xd.XPathSelectElement("//video");
            video.RemoveNodes();
            foreach (FileElement fe in files)
            {
                fe.m_bfirst = true;
                video.Add( Track.CreateTrackNode(lengths, fe) );
            }
            video.Add(Track.CreateTrackNode(lengths, null));
            video.Add(Format());

            xd.Save("test.xml");
        }
        XElement Format()
        {
            return new XElement("format",
                        new XElement("samplecharacteristics",
                            new XElement("width", 1920),
                            new XElement("height", 1080),
                            new XElement("anamorphic", "false"),
                            new XElement("pixelaspectratio", "square"),
                            new XElement("fielddominance", "upper"),
                            new XElement("rate",
                                new XElement("timebase", 30),
                                new XElement("ntsc", "true")
                            ),
                            new XElement("colordepth", 24),
                            new XElement("codec",
                                new XElement("name", "Apple ProRes 422"),
                                new XElement("appspecificdata",
                                    new XElement("appname", "Final Cut Pro"),
                                    new XElement("appmanufacturer", "Apple Inc."),
                                    new XElement("appversion", "7.0"),
                                    new XElement("data",
                                        new XElement("qtcodec",
                                            new XElement("codecname", "Apple ProRes 422"),
                                            new XElement("codectypename", "Apple ProRes 422"),
                                            new XElement("codectypecode", "apcn"),
                                            new XElement("codecvendorcode", "appl"),
                                            new XElement("spatialquality", "1024"),
                                            new XElement("temporalquality", "0"),
                                            new XElement("keyframerate", "0"),
                                            new XElement("datarate", "0")

                                        )
                                    )
                                )
                            )
                       )
               );
        }
    }
}
