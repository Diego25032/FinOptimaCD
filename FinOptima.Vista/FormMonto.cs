using System;
using System.Drawing;
using System.Windows.Forms;
using FinOptima.Vista.Temas;
using FinOptima.Vista.Utilidades;

namespace FinOptima.Vista
{
    public class FormMonto : Form
    {
        private NumericUpDown campoMonto;

        public decimal Monto
        {
            get { return campoMonto.Value; }
        }

        public FormMonto(string titulo, string descripcion, decimal maximo)
        {
            Text = titulo;
            AutoScaleMode = AutoScaleMode.Dpi;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;

            Label rotulo = UtilidadesVista.CrearNota(descripcion, 24, 20, 350);
            Controls.Add(rotulo);

            campoMonto = new NumericUpDown();
            campoMonto.Location = new Point(24, rotulo.Bottom + 12);
            campoMonto.Width = 350;
            campoMonto.DecimalPlaces = 2;
            campoMonto.Maximum = maximo;
            campoMonto.Increment = 10000;
            Controls.Add(campoMonto);

            int yBotones = campoMonto.Bottom + 20;
            Button aceptar = UtilidadesVista.CrearBoton("Aceptar", 24, yBotones, 170, 38, TemaActual.Acento);
            aceptar.Click += AlAceptar;
            Controls.Add(aceptar);

            Button cancelar = UtilidadesVista.CrearBotonSecundario("Cancelar", 204, yBotones, 170, 38);
            cancelar.Click += AlCancelar;
            Controls.Add(cancelar);

            AcceptButton = aceptar;
            CancelButton = cancelar;
            ClientSize = new Size(400, Math.Max(210, cancelar.Bottom + 24));

            UtilidadesVista.PrepararFormulario(this);
            UtilidadesVista.EstilizarCampos(this);
        }

        private void AlAceptar(object remitente, EventArgs argumentos)
        {
            if (campoMonto.Value <= 0)
            {
                UtilidadesVista.AvisarError("El monto debe ser mayor que cero.");
                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private void AlCancelar(object remitente, EventArgs argumentos)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
