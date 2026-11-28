using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista.Modulos
{
    public class ModuloRespaldos : ModuloBase
    {
        private Label nota;
        private Label resultado;
        private Label aviso;
        private Panel tarjetaExportar;
        private Panel tarjetaImportar;
        private Label detalleExportar;
        private Label detalleImportar;
        private Button exportar;
        private Button importar;

        public ModuloRespaldos(FabricaServicios servicios) : base(servicios, "Respaldos")
        {
            ConstruirInterfaz();
            resultado.TextChanged += delegate(object remitente, EventArgs argumentos)
            {
                Distribuir();
            };
            Distribuir();
        }

        private void ConstruirInterfaz()
        {
            nota = UtilidadesVista.CrearNota("Guarda o restaura tus sobres, movimientos, metas y deudas en un archivo JSON.", Margen, InicioContenido, 880);
            Controls.Add(nota);

            tarjetaExportar = UtilidadesVista.CrearTarjeta("Exportar copia de seguridad", Margen, 100, 400, 200, Acento);
            Controls.Add(tarjetaExportar);

            detalleExportar = new Label();
            detalleExportar.Text = "Crea un archivo con toda tu informacion financiera. Guardalo en un lugar seguro.";
            detalleExportar.Location = new Point(12, 65);
            detalleExportar.AutoSize = true;
            detalleExportar.MaximumSize = new Size(370, 0);
            detalleExportar.ForeColor = TemaActual.Actual.TextoSecundario;
            tarjetaExportar.Controls.Add(detalleExportar);

            exportar = UtilidadesVista.CrearBoton("Exportar JSON", 12, 148, 180, 34, Acento);
            exportar.Click += AlExportar;
            tarjetaExportar.Controls.Add(exportar);

            tarjetaImportar = UtilidadesVista.CrearTarjeta("Restaurar copia de seguridad", Margen + 430, 100, 400, 200, TemaActual.Actual.Peligro);
            Controls.Add(tarjetaImportar);

            detalleImportar = new Label();
            detalleImportar.Text = "Reemplaza tus sobres, movimientos, metas y deudas por los del archivo.";
            detalleImportar.Location = new Point(12, 65);
            detalleImportar.AutoSize = true;
            detalleImportar.MaximumSize = new Size(370, 0);
            detalleImportar.ForeColor = TemaActual.Actual.TextoSecundario;
            tarjetaImportar.Controls.Add(detalleImportar);

            importar = UtilidadesVista.CrearBoton("Restaurar JSON", 12, 148, 180, 34, TemaActual.Actual.Peligro);
            importar.Click += AlImportar;
            tarjetaImportar.Controls.Add(importar);

            resultado = new Label();
            resultado.Location = new Point(Margen, 325);
            resultado.AutoSize = true;
            resultado.MaximumSize = new Size(860, 0);
            resultado.ForeColor = TemaActual.Actual.Texto;
            Controls.Add(resultado);

            aviso = new Label();
            aviso.Text = "Importante: el archivo contiene datos personales. No lo compartas.";
            aviso.Location = new Point(Margen, 420);
            aviso.AutoSize = true;
            aviso.MaximumSize = new Size(860, 0);
            aviso.ForeColor = TemaActual.Actual.Peligro;
            Controls.Add(aviso);
        }

        protected override void OnResize(EventArgs argumentos)
        {
            base.OnResize(argumentos);
            Distribuir();
        }

        private void Distribuir()
        {
            if (tarjetaExportar == null || Recalculando)
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
            bool enColumnas = ancho >= Escalar(850);
            int anchoTarjeta = enColumnas ? (ancho - separacion) / 2 : ancho;

            UtilidadesVista.AjustarEtiqueta(nota, ancho);
            int y = nota.Bottom + 18;

            int altoExportar = AjustarTarjetaRespaldo(tarjetaExportar, detalleExportar, exportar, anchoTarjeta);
            int altoImportar = AjustarTarjetaRespaldo(tarjetaImportar, detalleImportar, importar, anchoTarjeta);
            tarjetaExportar.Location = new Point(x, y);
            tarjetaImportar.Location = new Point(
                enColumnas ? x + anchoTarjeta + separacion : x,
                enColumnas ? y : tarjetaExportar.Bottom + separacion);

            resultado.Location = new Point(x, Math.Max(tarjetaExportar.Bottom, tarjetaImportar.Bottom) + 20);
            UtilidadesVista.AjustarEtiqueta(resultado, ancho);
            aviso.Location = new Point(x, resultado.Bottom + 16);
            UtilidadesVista.AjustarEtiqueta(aviso, ancho);

            return aviso.Bottom - AutoScrollPosition.Y + Margen;
        }

        private int AjustarTarjetaRespaldo(Panel tarjeta, Label detalle, Button boton, int ancho)
        {
            UtilidadesVista.AjustarEtiqueta(detalle, ancho - 24);
            int alto = Math.Max(Escalar(200), detalle.Bottom + Escalar(60));
            UtilidadesVista.AjustarTarjeta(tarjeta, ancho, alto);
            boton.SetBounds(12, alto - boton.Height - 16, Math.Min(Escalar(180), ancho - 24), boton.Height);
            return alto;
        }

        private void AlExportar(object remitente, EventArgs argumentos)
        {
            SaveFileDialog dialogo = new SaveFileDialog();
            dialogo.Title = "Guardar copia de seguridad";
            dialogo.Filter = "Copia de seguridad (*.json)|*.json";
            dialogo.FileName = "FinOptima-respaldo.json";

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                ResumenRespaldo resumen = Servicios.Respaldo.Exportar(dialogo.FileName);

                resultado.Text = "Copia guardada correctamente." + Environment.NewLine +
                                 "Sobres: " + resumen.Sobres + ". Movimientos: " + resumen.Movimientos + "." + Environment.NewLine +
                                 "Metas: " + resumen.Metas + ". Deudas: " + resumen.Deudas + ".";
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }

        private void AlImportar(object remitente, EventArgs argumentos)
        {
            OpenFileDialog dialogo = new OpenFileDialog();
            dialogo.Title = "Seleccionar copia de seguridad";
            dialogo.Filter = "Copia de seguridad (*.json)|*.json";

            if (dialogo.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            bool continuar = UtilidadesVista.Confirmar("La restauracion reemplaza tus datos financieros actuales. Deseas continuar?");

            if (!continuar)
            {
                return;
            }

            try
            {
                ResumenRespaldo resumen = Servicios.Respaldo.Importar(dialogo.FileName);

                resultado.Text = "Copia restaurada correctamente." + Environment.NewLine +
                                 "Sobres: " + resumen.Sobres + ". Movimientos: " + resumen.Movimientos + "." + Environment.NewLine +
                                 "Metas: " + resumen.Metas + ". Deudas: " + resumen.Deudas + ".";
            }
            catch (Exception error)
            {
                MostrarError(error);
            }
        }
    }
}
