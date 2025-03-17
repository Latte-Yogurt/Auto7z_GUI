using System;
using System.Windows.Forms;

namespace Auto7z_GUI
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(args));
            }

            catch (Exception)
            {
                Environment.Exit(0);
            }
        }
    }
}
