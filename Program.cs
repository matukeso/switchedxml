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

            SXMainForm form = new SXMainForm();
            try
            {
                using (Microsoft.Win32.RegistryKey regkey =
                Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\ACC\SwitchedXML"))
                {
                    string v = regkey.GetValue("WindowPosition") as string;
                    RestoreWindowPosition(form, v);
                }
            }
            catch { /* Just leave current position if error */ }

            Application.Run(form);
        }

        static void RestoreWindowPosition(Form f, string s)
        {

            List<int> settings = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(v => int.Parse(v)).ToList();
            if (settings.Count == 5)
            {
                f.StartPosition = FormStartPosition.Manual;
                f.SetBounds(settings[1], settings[2], settings[3], settings[4]);
                f.WindowState = (FormWindowState)settings[0];
            }

        }

    }
}
