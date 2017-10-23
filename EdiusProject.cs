﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Switchedxml
{
    public class EdiusProject
    {
        Dictionary<string, FileElement> files = new Dictionary<string, FileElement>();
        public void ReadProject(string zipfilename)
        {
            using (ZipArchive zip = ZipFile.OpenRead(zipfilename))
            {
                foreach (var a in zip.Entries)
                {
                    if (a.Name.ToLower().EndsWith(".ews"))
                    {
                        XDocument xd = OnReadProjectFile(a);
                        Project(xd);
                    }
                }
                foreach (var a in zip.Entries)
                {
                    if (a.Name.ToUpper().Contains(ActiveTimelineGUID))
                    {
                        XDocument xd = OnReadProjectFile(a);
                        TimeLine(a.Name, xd);
                    }
                }
            }
        }

        XDocument OnReadProjectFile(ZipArchiveEntry a)
        {
            EdiusReadAsXML xmlr = new EdiusReadAsXML();
            StringBuilder sb = new StringBuilder();

            byte[] rawfile = new byte[a.Length];
            using (Stream s = a.Open())
            {
                s.Read(rawfile, 0, (int)a.Length);
            }

            xmlr.ReadItem(sb, new MemoryStream(rawfile));
            return XDocument.Parse(sb.ToString());
        }

        string ActiveTimelineGUID;
        void Project(XDocument xd)
        {
            //string barename = System.IO.Path.GetFileNameWithoutExtension(a.Name);
            //File.WriteAllText(barename + ".xml", sb.ToString(), System.Text.UTF8Encoding.UTF8);
            ActiveTimelineGUID = xd.XPathSelectElement("/EdiusProjectFile/ProjectInformations/ActiveTimelineSequenceGUID").Value.ToUpper();

            foreach (var sourcelist in xd.XPathSelectElements("/EdiusProjectFile/Media/SourceList/Source"))
            {
                string strid = sourcelist.Element("Id").Value;
                foreach (var src in sourcelist.XPathSelectElements(".//Src"))
                {
                    string inV = src.Element("InV").Value;
                    string durV = src.Element("DurV").Value;

                    var imp = src.XPathSelectElement("ImpList/Imp");
                    if (imp == null)
                        continue;
                    string filename = imp.Element("FileName").Value;
                    if (filename == "null")
                        continue;
                    FileElement fe = new FileElement();
                    fe.id = strid;
                    fe.pathurl = filename;
                    fe.name = filename;
                    fe.duration = int.Parse(durV);
                    files[strid] = fe;

                }
            }
        }

        List<Track> tracks = new List<Track>();
        void TimeLine(string filename, XDocument xd)
        {
            foreach (var tracklist in xd.XPathSelectElements("//TrackList"))
            {
                string ty = tracklist.Element("Type").Value;
                if (ty == "0")
                {
                    foreach (var track in tracklist.XPathSelectElements("./List/Track"))
                    {
                        Track ttrack = new Track(filename);
                        foreach (var clip in track.XPathSelectElements(".//Clip/BODY"))
                        {
                            string name = clip.Element("Name")?.Value;
                            if (name == null) name = clip.Element("SourceName")?.Value;
                            string id = clip.Element("SourceId").Value;
                            string fStart = clip.XPathSelectElement("Video/Start").Value;
                            string fEnd = clip.XPathSelectElement("Video/End").Value;
                            string inf = clip.Element("In").Value;
                            string dur = clip.Element("Duration").Value;

                            ClipItem ci = new ClipItem();
                            ci.id = id;
                            ci.name = name;
                            ci.duration = int.Parse(dur);
                            ci.inf = int.Parse(fStart) >> 16;
                            ci.outf = int.Parse(fEnd) >> 16;
                            ci.start = int.Parse(inf);
                            ci.end  = ci.start + ci.duration;
                            ci.fileref = id;
                            ttrack.Add(ci);
                        }
                        if (ttrack.Count > 0)
                        {
                            tracks.Add(ttrack);
                        }
                    }

                }


            }


        }
    }
}
