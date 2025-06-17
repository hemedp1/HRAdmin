using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HRAdmin
{
    internal static class Program
    {
        /// <summary> syazwan
        /// The main entry point for the application.
        /// </summary> callmewan
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form_Login());
        }
    }
}
