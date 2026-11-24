using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FinOptima.Vista.Utilidades
{
    /// <summary>
    /// Hace que un monto en una etiqueta suba (o baje) poco a poco hasta su valor final.
    /// </summary>
    public sealed class ContadorMoneda
    {
        private static readonly Dictionary<Label, ContadorMoneda> activos = new Dictionary<Label, ContadorMoneda>();
        private const int Cuadros = 45;

        private readonly Label etiqueta;
        private readonly Timer temporizador;
        private decimal desde;
        private decimal hasta;
        private decimal actual;
        private int cuadro;

        private ContadorMoneda(Label etiqueta)
        {
            this.etiqueta = etiqueta;
            temporizador = new Timer();
            temporizador.Interval = 16;
            temporizador.Tick += new EventHandler(AlAvanzar);
            etiqueta.Disposed += new EventHandler(AlLiberarEtiqueta);
        }

        public static void Animar(Label etiqueta, decimal valorFinal)
        {
            ContadorMoneda contador;

            if (!activos.TryGetValue(etiqueta, out contador))
            {
                contador = new ContadorMoneda(etiqueta);
                activos.Add(etiqueta, contador);
            }

            contador.desde = contador.actual;
            contador.hasta = valorFinal;
            contador.cuadro = 0;
            contador.temporizador.Start();
        }

        private void AlAvanzar(object remitente, EventArgs argumentos)
        {
            cuadro++;
            double t = (double)cuadro / Cuadros;

            if (t >= 1)
            {
                t = 1;
                temporizador.Stop();
            }

            double inverso = 1 - t;
            double suave = 1 - inverso * inverso * inverso;
            actual = desde + (hasta - desde) * (decimal)suave;

            if (t >= 1)
            {
                actual = hasta;
            }

            etiqueta.Text = UtilidadesVista.Moneda(Math.Round(actual, 0));
        }

        private void AlLiberarEtiqueta(object remitente, EventArgs argumentos)
        {
            temporizador.Stop();
            temporizador.Dispose();
            activos.Remove(etiqueta);
        }
    }
}
