using System;

namespace Switchedxml
{
    public static class TCUtility
    {
        const int POS_HOUR = 0;
        const int POS_MIN = 1;
        const int POS_SEC = 2;
        const int POS_FRAME = 3;

        public static void ltc_frame_increment(int[] frame)
        {
            frame[POS_FRAME]++;
            if (frame[POS_FRAME] == 60)
            {
                frame[POS_FRAME] = 0;
                frame[POS_SEC]++;
                if (frame[POS_SEC] == 60)
                {
                    frame[POS_SEC] = 0;
                    frame[POS_MIN]++;
                    if (frame[POS_MIN] == 60)
                    {
                        frame[POS_MIN] = 0;
                        frame[POS_HOUR]++;
                    }
                }
            }
            if ((frame[POS_MIN] % 10) != 0 &&
                frame[POS_SEC] == 0 &&
                frame[POS_FRAME] == 0)
            {
                frame[POS_FRAME] += 4;
            }
        }
        /// <summary>
        ///
        /// </summary>
        /// <param name="dat">00:11:22;ff</param>
        /// <returns></returns>
        public static int dateDfToFrame(string dat,bool f60)
        {
            string[] s = dat.Split(';', ':', '.');
            if (s.Length == 4)
            {
                int[] hhmmssff = Array.ConvertAll(s, int.Parse);
                int f = 0;
                f += hhmmssff[POS_HOUR] * 107892 * 2;
                f += hhmmssff[POS_MIN] * 60 * 30 * 2;
                f -= 4 * (hhmmssff[POS_MIN] - hhmmssff[POS_MIN] / 10); ; //drop effect. maybe.

                f += hhmmssff[POS_SEC] * 60;
                f += hhmmssff[POS_FRAME] * (f60 ? 1 : 2);
                return f;
            }
            return 0;
        }
        public static int dateDfToFrame60(string dat) { return dateDfToFrame(dat, true); }
        public static int dateDfToFrame30(string dat) { return dateDfToFrame(dat, false); }

        const int dfmin10 = 17982 * 2;
        const int dfmin1 = 1798 * 2;
        const int hourf = 3600 * 30 * 2;
        const int minf = 60 * 30 * 2;
        const int secf = 30 * 2;
        public static string DfFrameToDate(int f)
        {
            int nmin10 = f / dfmin10;
            int rem10 = (f % dfmin10 - 4) / dfmin1;

            int df = f + 4 * (9 * nmin10 + rem10);

            int hh = df / hourf; df %= hourf;
            int mm = df / minf; df %= minf;
            int ss = df / secf; df %= secf;
            return string.Format("{0:d2}:{1:d2}:{2:d2}:{3:d2}", hh, mm, ss, df);

        }

        public static bool Test()
        {
            int[] ltc = new int[4];
            int nframe = 0;
            while (nframe < 86400 * 30)
            {
                nframe++;
                ltc_frame_increment(ltc);

                string df = string.Format("{0:d2}:{1:d2}:{2:d2}:{3:d2}", ltc[0], ltc[1], ltc[2], ltc[3]);
                int f = dateDfToFrame60(df);
                string df2 = DfFrameToDate(nframe);

                if (f != nframe || df2 != df)
                {
                    Console.WriteLine("{0}", df);
                    return false;
                }
            }
            return true;
        }
    }
}
