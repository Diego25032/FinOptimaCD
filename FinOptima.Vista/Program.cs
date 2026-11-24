using System;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;

namespace FinOptima.Vista
{
    internal static class Program
    {
        [STAThread]
        internal static void Main()
        {
            ApplicationConfiguration.Initialize();

            using (FormBienvenida bienvenida = new FormBienvenida())
            {
                Application.Run(bienvenida);
            }

            FabricaServicios fabrica = new FabricaServicios();

            using (FormLogin login = new FormLogin(fabrica))
            {
                Application.Run(login);
            }
        }
    }
}
