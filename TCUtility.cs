using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Switchedxml
{
    public static class TCUtility
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="dat">00:11:22;ff</param>
        /// <returns></returns>
        public static int dateDfToFrame(string dat)
        {
            string[] s = dat.Split(';', ':');
            if (s.Length == 4)
            {
                int[] hhmmssff = Array.ConvertAll(s, int.Parse);
                int f = 0;
                f += hhmmssff[0] * 107892;
                f += hhmmssff[1] * 60 * 30;
                f -= 2 * (hhmmssff[1] - hhmmssff[1] / 10); ; //drop effect. maybe.

                f += hhmmssff[2] * 30;
                f += hhmmssff[3];
                return f;
            }
            return 0;

        }

        const int dfmin10 = 17982;
        const int dfmin1 = 1798;
        const int hourf = 3600 * 30;
        const int minf = 60 * 30;
        const int secf = 30;
        public static string DfFrameToDate(int f)
        {
            int nmin10 = f / dfmin10;
            int rem10 = (f % dfmin10 - 2) / nmin10;

            int df = f + 2 * (9 * nmin10 + rem10);

            int hh = df / hourf; df %= hourf;
            int mm = df / minf; df %= minf;
            int ss = df / secf; df %= secf;
            return string.Format("{0:d2}:{1:d2}:{2:d2}:{3:d2}", hh, mm, ss, df);

        }
    }
}
