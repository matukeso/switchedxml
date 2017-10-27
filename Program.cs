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
            ///TCUtility.Test();
            //new EdiusProject().ReadProject(@"C:\Users\matuken\Documents\recved\ezp-Sample.ezp");
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SXMainForm());
        }
    }
}
