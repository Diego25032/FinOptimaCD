using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloReportes : ModuloBase
    {
        private Label nota;
        private Label notaFinal;
        private Panel tarjetaExcel;
        private Panel tarjetaPdf;
        private Label detalleExcel;
        private Label detallePdf;
        private Button excel;
        private Button pdf;

        public ModuloReportes(FabricaServicios servicios) : base(servicios, "Reportes")
        {
            ConstruirInterfaz();
            Distribuir();
        }

        private void ConstruirInterfaz()
        {
            nota = UtilidadesVista.CrearNota("Genera un resumen de tus finanzas en PDF o Excel.", Margen, InicioContenido, 880);
            Controls.Add(nota);

            tarjetaExcel = UtilidadesVista.CrearTarjeta("Reporte de Excel", Margen, 100, 380, 190, TemaActual.Actual.Exito);
            Controls.Add(tarjetaExcel);

            detalleExcel = new Label();
            detalleExcel.Text = "Hoja de resumen con indicadores y tablas con filtros y totales de sobres, movimientos, metas y deudas.";
            detalleExcel.Location = new Point(12, 65);
            detalleExcel.AutoSize = true;
            detalleExcel.MaximumSize = new Size(350, 0);
            detalleExcel.ForeColor = TemaActual.Actual.TextoSecundario;
            tarjetaExcel.Controls.Add(detalleExcel);

            excel = UtilidadesVista.CrearBoton("Exportar Excel", 12, 138, 180, 34, TemaActual.Actual.Exito);
            excel.Click += AlExportarExcel;
            tarjetaExcel.Controls.Add(excel);

            tarjetaPdf = UtilidadesVista.CrearTarjeta("Reporte PDF", Margen + 410, 100, 380, 190, TemaActual.Actual.Peligro);
            Controls.Add(tarjetaPdf);

            detallePdf = new Label();
            detallePdf.Text = "Informe con portada, indicadores, gráficos de gastos, metas, deudas y últimos movimientos.";
            detallePdf.Location = new Point(12, 65);
            detallePdf.AutoSize = true;
            detallePdf.MaximumSize = new Size(350, 0);
            detallePdf.ForeColor = TemaActual.Actual.TextoSecundario;
            tarjetaPdf.Controls.Add(detallePdf);

            pdf = UtilidadesVista.CrearBoton("Exportar PDF", 12, 138, 180, 34, TemaActual.Actual.Peligro);
            pdf.Click += AlExportarPdf;
            tarjetaPdf.Controls.Add(pdf);

            notaFinal = UtilidadesVista.CrearNota("Los reportes se guardan solo en el equipo donde estas usando la aplicacion.", Margen, 310, 880);
            Controls.Add(notaFinal);
        }

        protected override void OnResize(EventArgs argumentos)
        {
            base.OnResize(argumentos);
            Distribuir();
        }

        private void Distribuir()
        {
            if (tarjetaExcel == null || Recalculando)
            {
                return;
            }

            Recalculando = true;
            SuspendLayout();

            try
            {
                int ancho = ClientSize.Width - Margen * 2;
                int alto = DistribuirEnAncho(ancho);

                if (alto > ClientSize.Height && !VerticalScroll.Visible)
                {
                    ancho = ClientSize.Width - Margen * 2 - SystemInformation.VerticalScrollBarWidth;
                    alto = DistribuirEnAncho(ancho);
                }

                AutoScrollMinSize = new Size(
                    Math.Max(ClientSize.Width, ancho + Margen * 2),
                    Math.Max(ClientSize.Height, alto));
            }
            finally
            {
                ResumeLayout(false);
                Recalculando = false;
            }
        }

        private int DistribuirEnAncho(int ancho)
        {
            int separacion = Escalar(30);
            ancho = Math.Max(Escalar(320), ancho);
            int x = Margen + AutoScrollPosition.X;
            bool enColumnas = ancho >= Escalar(800);
            int anchoTarjeta = enColumnas ? (ancho - separacion) / 2 : ancho;

            UtilidadesVista.AjustarEtiqueta(nota, ancho);
            int y = nota.Bottom + 18;

            int altoExcel = AjustarTarjetaReporte(tarjetaExcel, detalleExcel, excel, anchoTarjeta);
            int altoPdf = AjustarTarjetaReporte(tarjetaPdf, detallePdf, pdf, anchoTarjeta);
            tarjetaExcel.Location = new Point(x, y);
            tarjetaPdf.Location = new Point(
                enColumnas ? x + anchoTarjeta + separacion : x,
                enColumnas ? y : tarjetaExcel.Bottom + separacion);

            notaFinal.Location = new Point(x, Math.Max(tarjetaExcel.Bottom, tarjetaPdf.Bottom) + 20);
            UtilidadesVista.AjustarEtiqueta(notaFinal, ancho);

            return notaFinal.Bottom - AutoScrollPosition.Y + Margen;
        }

        private int AjustarTarjetaReporte(Panel tarjeta, Label detalle, Button boton, int ancho)
        {
            UtilidadesVista.AjustarEtiqueta(detalle, ancho - 24);
            int alto = Math.Max(Escalar(190), detalle.Bottom + Escalar(60));
            UtilidadesVista.AjustarTarjeta(tarjeta, ancho, alto);
            boton.SetBounds(12, alto - boton.Height - 16, Math.Min(Escalar(180), ancho - 24), boton.Height);
            return alto;
        }

        private void AlExportarExcel(object remitente, EventArgs argumentos)
        {
            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Title = "Guardar reporte de Excel";
            dialogo.Filter = "Archivo de Excel (*.xlsx)|*.xlsx";
            dialogo.FileName = "FinOptima-reporte-" + DateTime.Now.ToString("yyyy-MM-dd") + ".xlsx";

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Resultado resultado = Servicios.Reportes.ExportarExcel(dialogo.FileName);
                MostrarResultadoYAbrir(resultado, dialogo.FileName);
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlExportarPdf(object remitente, EventArgs argumentos)
        {
            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Title = "Guardar reporte PDF";
            dialogo.Filter = "Archivo PDF (*.pdf)|*.pdf";
            dialogo.FileName = "FinOptima-reporte-" + DateTime.Now.ToString("yyyy-MM-dd") + ".pdf";

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                Resultado resultado = Servicios.Reportes.ExportarPdf(dialogo.FileName);
                MostrarResultadoYAbrir(resultado, dialogo.FileName);
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void MostrarResultadoYAbrir(Resultado resultado, string ruta)
        {
            if (!resultado.Exito)
            {
                MostrarResultado(resultado);
                return;
            }

            if (!UtilidadesVista.Confirmar(resultado.Mensaje + "\n\n¿Deseas abrirlo ahora?"))
            {
                return;
            }

            try
            {
                System.Diagnostics.ProcessStartInfo inicio = new System.Diagnostics.ProcessStartInfo(ruta);
                inicio.UseShellExecute = true;
                System.Diagnostics.Process.Start(inicio);
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }
    }
}
