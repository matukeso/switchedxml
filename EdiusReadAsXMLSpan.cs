using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Buffers.Binary;
namespace Switchedxml
{


    public class EdiusReadAsXMLSpan
    {
        static int read_length(ref ReadOnlySpan<byte> sr)
        {
            int len = sr[0];
            if (len < 0x80)
            {
                sr = sr.Slice(1);
                return len;
            }
            else
            {
                len &= 0x7f;


                for (int i = 1; i < 5; i++)
                {
                    int lp = sr[i];
                    len += (lp & 0x7f) << (7 * i);
                    if (lp < 0x80)
                    {
                        sr = sr.Slice(i+1);
                        return len;
                    }
                }
                throw new InvalidDataException("too long lengt");
            }
        }

        static double read_float(ref ReadOnlySpan<byte> sr)
        {
            ReadOnlySpan<byte> data = read_as_byte(ref sr);

            if (data.Length == 4)
                return BitConverter.ToSingle(data.Slice(0,4).ToArray(), 0);
            if (data.Length == 8)
                return BitConverter.Int64BitsToDouble( BinaryPrimitives.ReadInt64LittleEndian(data));
            throw new InvalidDataException("invalid as integer");

        }
        static ReadOnlySpan<byte> read_as_byte(ref ReadOnlySpan<byte> sr)
        {
            int len = read_length(ref sr);
            var r = sr.Slice(0, len);
            sr = sr.Slice(len);
            return r;
        }
        static string read_string_utf8(ref ReadOnlySpan<byte> sr)
        {
            ReadOnlySpan<byte> data = read_as_byte(ref sr);
            return System.Text.UTF8Encoding.UTF8.GetString(data.ToArray()).TrimEnd('\0');
        }
        static string read_string_utf16(ref ReadOnlySpan<byte> sr)
        {
            ReadOnlySpan<byte> data = read_as_byte(ref sr);
            return System.Text.Encoding.Unicode.GetString(data.ToArray());
        }
        static long read_int(ref ReadOnlySpan<byte> sr)
        {
            ReadOnlySpan<byte> data = read_as_byte(ref sr);

            switch (data.Length)
            {
                case 1: return data[0];
                case 2: return BinaryPrimitives.ReadInt16LittleEndian(data);
                case 4: return BinaryPrimitives.ReadInt32LittleEndian(data);
                case 8: return BinaryPrimitives.ReadInt64LittleEndian(data);
                default:
                    throw new InvalidDataException("invalid as integer");
            }

        }

        public ReadOnlySpan<byte >ReadItem(StringBuilder xml,  ReadOnlySpan<byte> sr)
        {
            while (sr.Length > 0)
            {
                int chType = sr[0];
                sr = sr.Slice(1);
                switch (chType)
                {
                    case -1:
                        return sr;

                    case 0x80:
                    case 0xc0:
                        {
                            string s = read_string_utf8(ref sr);
                            ReadOnlySpan<byte> data = read_as_byte(ref sr);
                            //if( string.IsNullOrEmpty(s))
                            //{
                            //    System.Diagnostics.Debugger.Break();
                            //}
                            StringBuilder sbnew = new StringBuilder();
                            ReadItem(sbnew, data);
                            if (!string.IsNullOrEmpty(s))
                            {
                                xml.AppendFormat("<{0}>", s);
                            }
                            xml.AppendLine("");
                            xml.Append(sbnew.ToString());
                            //if (sbnew.ToString() == "<>")
                            //{
                            //    System.Diagnostics.Debugger.Break();
                            //}
                            if (!string.IsNullOrEmpty(s))
                            {
                                xml.AppendFormat("</{0}>", s);
                            }
                            xml.AppendLine("");
                        }
                        break;
                    case 0x5a:
                        {
                            string s = read_string_utf8(ref sr);
                            string v = read_string_utf8(ref sr);
                            xml.AppendXML(s, v);
                        }
                        break;
                    case 0x5c:
                        {
                            string s = read_string_utf8 (ref sr);
                            string v = read_string_utf16(ref sr);
                            xml.AppendXML(s, v);
                        }
                        break;
                    case 0x68:
                        {
                            string s = read_string_utf8(ref sr);
                            double v = read_float(ref sr);
                            xml.AppendXML(s, v.ToString());
                        }
                        break;
                    case 0x78:
                        {
                            string s = read_string_utf8(ref sr);
                            long v = read_int(ref sr);
                            xml.AppendXML(s, v.ToString());
                        }
                        break;
                    case 0x70:
                        {
                            //                            xml.AppendFormat("<list>");

                            var start = sr;
                            while (true)
                            {
                                string s = read_string_utf8(ref sr);
                                ReadOnlySpan< byte> v = read_as_byte(ref sr);
                                AppendAsDump(xml, s, v);
                                if (sr.Length == 0) break;

                                int ch2 = sr[0];
                                if (ch2 == 0)
                                {
                                    sr = sr.Slice(1);
                                    continue;
                                }
                                if (ch2 == 0xff)
                                {
                                    sr = sr.Slice(1);
                                }
                                break;
                            }
//                            xml.AppendFormat("</list>");
                            xml.AppendLine("");
                            //                            Console.WriteLine("{0}]", indent);
                        }
                        break;


                    case 0:
                        {
                            string s = read_string_utf8(ref sr);
                            ReadOnlySpan<byte> v = read_as_byte(ref sr);

                            AppendAsDump(xml, s, v);
                            xml.AppendLine("");
                        }
                        break;
                }
            }
            return sr;

        }

        private static void AppendAsDump(StringBuilder xml, string s, ReadOnlySpan<byte> v)
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
                Guid guid = new Guid(v.ToArray());
                dump.Append(guid.ToString());
            }
            else if ( s == "ID" && v.Length == 8)
            {
                long lv = BinaryPrimitives.ReadInt64LittleEndian(v);
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
