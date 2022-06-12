using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
namespace Switchedxml

{
    static class XMLSB
    {
public        static void AppendXML(this StringBuilder xml, string element, string value)
        {
            if (!string.IsNullOrEmpty(element))
            {
                xml.AppendFormat("<{0}>{1}</{0}>", element, value);
            }
            else
            {
                xml.Append(value);
            }
            xml.AppendLine("");
        }
    }


    public class EdiusReadAsXML
    {
        static int read_length(Stream sr)
        {
            int len = sr.ReadByte();
            if (len < 0x80)
            {
                return len;
            }
            else
            {
                len &= 0x7f;


                for (int i = 1; i < 5; i++)
                {
                    int lp = sr.ReadByte();
                    len += (lp & 0x7f) << (7 * i);
                    if (lp < 0x80)
                        return len;
                }
                throw new InvalidDataException("too long lengt");
            }
        }

        static double read_float(Stream sr)
        {
            byte[] data = read_as_byte(sr);

            if (data.Length == 4)
                return System.BitConverter.ToSingle(data, 0);
            if (data.Length == 8)
                return System.BitConverter.ToDouble(data, 0);
            throw new InvalidDataException("invalid as integer");

        }
        static byte[] read_as_byte(Stream sr)
        {
            int len = read_length(sr);
            byte[] data = new byte[len];
            sr.Read(data, 0, len);
            return data;
        }
        static string read_string_utf8(Stream sr)
        {
            byte[] data = read_as_byte(sr);
            return System.Text.UTF8Encoding.UTF8.GetString(data).TrimEnd('\0');
        }
        static string read_string_utf16(Stream sr)
        {
            byte[] data = read_as_byte(sr);
            return System.Text.Encoding.Unicode.GetString(data);
        }
        static long read_int(Stream sr)
        {
            byte[] data = read_as_byte(sr);

            switch (data.Length)
            {
                case 1: return data[0];
                case 2: return System.BitConverter.ToInt16(data, 0);
                case 4: return System.BitConverter.ToInt32(data, 0);
                case 8: return System.BitConverter.ToInt64(data, 0);
                default:
                    throw new InvalidDataException("invalid as integer");
            }

        }

        public int ReadItem(StringBuilder xml, Stream ms)
        {
            while (ms.CanRead)
            {
                int chType = ms.ReadByte();
                switch (chType)
                {
                    case -1:
                        return 0;

                    case 0x80:
                    case 0xc0:
                        {
                            long pos = ms.Position;
                            string s = read_string_utf8(ms);
                            byte[] data = read_as_byte(ms);

                            StringBuilder sbnew = new StringBuilder();
                            ReadItem(sbnew, new MemoryStream(data));

                            xml.AppendFormat("<{0}>", s);
                            xml.AppendLine("");
                            xml.Append(sbnew.ToString());
                            xml.AppendFormat("</{0}>", s);
                            xml.AppendLine("");
                        }
                        break;
                    case 0x5a:
                        {
                            long pos = ms.Position;
                            string s = read_string_utf8(ms);
                            string v = read_string_utf8(ms);
                            xml.AppendXML(s, v);
                        }
                        break;
                    case 0x5c:
                        {
                            string s = read_string_utf8(ms);
                            string v = read_string_utf16(ms);
                            xml.AppendXML(s, v);
                        }
                        break;
                    case 0x68:
                        {
                            string s = read_string_utf8(ms);
                            double v = read_float(ms);
                            xml.AppendXML(s, v.ToString());
                        }
                        break;
                    case 0x78:
                        {
                            string s = read_string_utf8(ms);
                            long v = read_int(ms);
                            xml.AppendXML(s, v.ToString());
                        }
                        break;
                    case 0x70:
                        {
//                            xml.AppendFormat("<list>");


                            while (true)
                            {
                                long pos = ms.Position;
                                string s = read_string_utf8(ms);
                                byte[] v = read_as_byte(ms);
                                AppendAsDump(xml, s, v);

                                int ch2 = ms.ReadByte();
                                if (ch2 == -1)
                                    break;
                                if (ch2 != 0)
                                {
                                    ms.Seek(ms.Position - 1, SeekOrigin.Begin);
                                    break;
                                }
                            }
//                            xml.AppendFormat("</list>");
                            xml.AppendLine("");
                            //                            Console.WriteLine("{0}]", indent);
                        }
                        break;


                    case 0:
                        {
                            long pos = ms.Position;
                            string s = read_string_utf8(ms);
                            byte[] v = read_as_byte(ms);

                            AppendAsDump(xml, s, v);
                            xml.AppendLine("");
                        }
                        break;
                }
            }
            return 0;

        }

        private static void AppendAsDump(StringBuilder xml, string s, byte[] v)
        {
            var dump = new StringBuilder();

            bool bGuid = v.Length == 16;
            if( bGuid)
            {
                if (s.Contains("GUID") ||
                    s == "CLASS" ||
                    s.EndsWith("Id"))
                {
                    ;
                }
                else
                    bGuid = false;
            }

            if (bGuid)
            {
                Guid guid = new Guid(v);
                dump.Append(guid.ToString());
            }
            else if ( s == "ID" && v.Length == 8)
            {
                long lv = BitConverter.ToInt64(v, 0);
                dump.Append(lv.ToString());
            }
            else
            {
                for (int ii = 0; ii < v.Length; ii++)
                {
                    string h = v[ii].ToString("X2");
                    dump.Append(h);
                    dump.Append("-");
                }
                if (dump.Length > 0)
                {
                    dump.Length = dump.Length - 1;
                }
            }

            if (String.IsNullOrEmpty(s))
            {
                xml.Append(dump.ToString());
            }
            else
            {
                xml.AppendFormat("<{0}>{1}</{0}>", s, dump.ToString());
            }
        }
    }
}
