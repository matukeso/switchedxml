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

        FileElement refFe;
        public ClipItem()
        {
        }
        public ClipItem(FileElement fe)
        {
            refFe = fe;
            fileref = fe.id;
            name =fe.name;
        }
        public ClipItem(XElement xe)
        {
            int rate = int.Parse(xe.XPathSelectElement("rate/timebase").Value);

            id = xe.Attribute("id").Value;
            duration = int.Parse(xe.Element("duration").Value) * 60 /rate; 
            start = int.Parse(xe.Element("start").Value) * 60 / rate;
            end = int.Parse(xe.Element("end").Value) * 60 / rate;
            enabled = xe.Element("enabled").Value == "true";
            inf = int.Parse(xe.Element("in").Value) * 60 / rate;
            outf = int.Parse(xe.Element("out").Value) * 60 / rate;
            fileref = xe.Element("file").Attribute("id").Value;

        }
        public override string ToString()
        {
            return string.Format("{0} : {1}-{2}", fileref, start, end);
        }
        public void AppendNode(XElement video)
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
                ,FileElement.GenRate60ntsc()
                ,new XElement("start", start)
                ,new XElement("end", end)
                ,new XElement("enabled", enabled ? "true" : "false")
                ,new XElement("in", inf)
                ,new XElement("out", outf)
             );
            if (refFe == null)
            {
                XElement xe = new XElement("file");
                xe.SetAttributeValue("id", fileref);
                clip.Add(xe);
            }
            else
            {
                refFe.AppendNode(clip);
            }
            clip.Add(new XElement("compositemode", "normal"));


            video.Add(clip);
        }

    }
    public class Track : List<ClipItem>
    {
        string fileref_first;
        public Track(XElement elem)
        {
            foreach (XElement xe in elem.Elements("clipitem"))
            {
                this.Add(new ClipItem(xe));
            }
            if (this.Count > 0)
            {
                fileref_first = this[0].fileref;
            }
        }

        public XElement CreateTrackNode( int projecttc, TCLog1 log, int mytrackno)
        {
            XElement track = new XElement("track");

            int log_first_tc = log[0].start_tc;

            for (int ii = 0; ii < Count; ii++)
            {
                TrackFile ciref = files[ii];
                if(ciref.f_start_tc  < log_first_tc)
                {
                    int start = ciref.f_start_tc;
                    int len = Math.Min(ciref.f_length, log_first_tc - start);
                    if (start < projecttc)
                    {
                        files[0].inOffset = 0;
                        len -= (projecttc- ciref.f_start_tc );
                        start = projecttc;
                    }

                    if (len < 0)
                        continue;
                    List<ClipItem> clips = GonvertTcToClips(projecttc, start, len, false);

                    foreach (ClipItem ci in clips)
                    {
                        ci.AppendNode(track);
                    }

                }
                else
                    break;

            }

            foreach (TCLogElement le in log)
            {
                List<ClipItem> clips = GonvertTcToClips(projecttc,le.start_tc, le.length, mytrackno == le.ch);
                foreach( ClipItem ci in clips)
                {
                    ci.AppendNode(track);
                }
            }
            track.Add(new XElement("enabled", "true"));
            track.Add(new XElement("locked", "false"));
            return track;
        }
        public static XElement CreateVirtualTrackNode( int project_tc, TCLog1 log, List<Track> tracks)
        {
            XElement track = new XElement("track");
            foreach (TCLogElement le in log)
            {
                if (le.ch < tracks.Count)
                {
                    List<ClipItem> clips = tracks[le.ch].GonvertTcToClips(project_tc, le.start_tc, le.length, true);
                    foreach (ClipItem ci in clips)
                    {
                        ci.AppendNode(track);
                    }
                }
            }
            track.Add(new XElement("enabled", "true"));
            track.Add(new XElement("locked", "false"));
            return track;

        }
        public static int uniqueid = 0;

        private List<ClipItem> GonvertTcToClips(int project_tc, int start_tc,  int frame_len, bool bActiveCh)
        {
            List<ClipItem> clips = new List<ClipItem>();
            int nFileIndex = 0;
            TrackFile fe = files[nFileIndex];
            do
            {
                //tclog << project_start. skip.
                if (start_tc < fe.f_start_tc)
                {
                    break;
                }

                int inf = start_tc - fe.f_start_tc;
                int cliplen = Math.Min(fe.f_length - inf , frame_len);

                // log start tc < splitted file . so move next file.
                if (cliplen <= 0)
                {
                    nFileIndex++;
                    if (nFileIndex < files.Count)
                    {
                        fe = files[nFileIndex];
                        continue;
                    }

                    // no more file. bye
                    break;
                }



                ClipItem v = new ClipItem(fe.fe);
                v.id = fe.fe.name + "_" + (uniqueid++).ToString();

                v.duration = cliplen;
                v.inf = inf + fe.inOffset ;
                v.outf = inf + cliplen + fe.inOffset ;
                v.start = start_tc  - project_tc + fe.inOffset;
                v.end = start_tc + cliplen - project_tc + fe.inOffset;
                v.enabled = bActiveCh;

                clips.Add(v);

                start_tc += cliplen;
                frame_len -= cliplen;
            } while (frame_len > 0);

            return clips;
        }

        public class TrackFile
        {
            public int f_start_tc;
            public int f_length;
            public int inOffset;
            public FileElement fe;

            public TrackFile( int st, int len, int inp, FileElement fe)
            {
                this.f_start_tc = st;
                this.f_length = len;
                this.inOffset = inp;
                this.fe = fe;
            }
            public override string ToString()
            {
                return string.Format("{0} - {1} : {2}", f_start_tc, f_length, fe.id);
            }
        }

        public List<TrackFile> files = new List<TrackFile>();
        int start_tc;
        internal void MakeRelation(int projecttc, FileElements all_files)
        {
            if (this.Count > 0)
            {
                FileElement fe = all_files.FindById(fileref_first);
                start_tc = projecttc - this[0].inf + this[0].start;
            }

            foreach(ClipItem ci in this)
            {
                FileElement fe = all_files.FindById(ci.fileref);
                int tc = start_tc + ci.start;
                files.Add(new TrackFile(tc, ci.duration, ci.inf, fe));
            }
        }
    }

    public class XmlProject
    {
        XDocument xd;

        public string title { set; get; }
        public FileElements all_files = new FileElements();
        public List<Track> tracks = new List<Track>();
        public int duration;

        TimeCode project_tc; 

        public XmlProject()
        {
        }

        public void Read(string path)
        {
            all_files.Clear();
            tracks.Clear();

            xd = XDocument.Parse(System.IO.File.ReadAllText(path));

            project_tc = new TimeCode(xd.XPathSelectElement("/xmeml/sequence/timecode"));

            //            XElement first_track = xd.XPathSelectElement(/)
            foreach (XElement xe in xd.XPathSelectElements("//file[pathurl]"))
            {
                all_files.Add(new FileElement(xe));
            }
            foreach (XElement xe in xd.XPathSelectElements("//video/track"))
            {
                Track t = new Track(xe);
                t.MakeRelation(project_tc.Frame, all_files);
                tracks.Add(t);
            }
        }


        public void RebuildByLength(TCLog1 log)
        {
            //int totla_length = lengths.Sum(s => s.Value);
            //xd.XPathSelectElement("//sequence/duration").SetValue(totla_length);

            XElement video = xd.XPathSelectElement("//video");
            video.RemoveNodes();
            int trkno=0;
            foreach (Track t in tracks)
            {
                video.Add(t.CreateTrackNode(project_tc.Frame, log, trkno ++) );
            }
            video.Add(Track.CreateVirtualTrackNode(project_tc.Frame, log, tracks));
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
                                new XElement("timebase", 60),
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
