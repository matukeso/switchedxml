using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Switchedxml
{
    static class Program
    {
        /// <summary>
        /// アプリケーションのメイン エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            EdiusProject ep = new Switchedxml.EdiusProject();
            ep.ReadProject(@"C:\Users\matuken\Documents\recved\ezp-Sample.ezp");
            ///TCUtility.Test();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SXMainForm());
        }
    }
}
