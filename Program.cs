using System;
using System.Windows.Forms;
using CliniqueVeterinaire.Forms;

namespace CliniqueVeterinaire
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FrmConnexion());
        }
    }
}