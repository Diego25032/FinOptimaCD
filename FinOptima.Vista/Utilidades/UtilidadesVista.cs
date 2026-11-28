using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using FinOptima.Entidades.Enums;
using FinOptima.Vista.Controles;
using FinOptima.Vista.Temas;

namespace FinOptima.Vista.Utilidades
{
    public static class UtilidadesVista
    {
        public const int RadioBoton = 8;
        public const int RadioTarjeta = 12;

        private static Icon iconoAplicacion;

        /// <summary>
        /// Icono de FinOptima (el mismo del ejecutable) para las ventanas y la bandeja del sistema.
        /// </summary>
        public static Icon IconoAplicacion()
        {
            if (iconoAplicacion == null)
            {
                try
                {
                    iconoAplicacion = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                }
                catch (Exception)
                {
                    iconoAplicacion = null;
                }
            }

            return iconoAplicacion;
        }

        public static void AplicarIcono(Form formulario)
        {
            Icon icono = IconoAplicacion();

            if (icono != null)
            {
                formulario.Icon = icono;
            }
        }

        public static Color Mezclar(Color original, Color otro, int porcentaje)
        {
            return Tema.Mezclar(original, otro, porcentaje);
        }

        public static void Redondear(Control control, int radio)
        {
            if (control.Width <= 0 || control.Height <= 0)
            {
                return;
            }

            using (GraphicsPath ruta = CrearRutaRedonda(new Rectangle(0, 0, control.Width, control.Height), radio))
            {
                Region anterior = control.Region;
                control.Region = new Region(ruta);

                if (anterior != null)
                {
                    anterior.Dispose();
                }
            }
        }

        public static int CrearMarcaCabecera(Control contenedor, string subtitulo, Color acento)
        {
            Tema tema = TemaActual.Actual;
            Color acentoVisible = tema.AcentoVisible(acento);

            LogoFinOptima logo = new LogoFinOptima();
            logo.Location = new Point(22, 9);
            logo.Size = new Size(36, 36);
            logo.BackColor = contenedor.BackColor;
            logo.ColorAcento = acentoVisible;
            contenedor.Controls.Add(logo);

            Label titulo = new Label();
            titulo.Text = "FinOptima";
            titulo.AutoSize = true;
            titulo.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titulo.ForeColor = tema.CabeceraTexto;
            titulo.BackColor = Color.Transparent;
            titulo.Location = new Point(68, 5);
            contenedor.Controls.Add(titulo);

            Label nota = new Label();
            nota.Text = subtitulo;
            nota.AutoSize = true;
            nota.Font = new Font("Segoe UI", 8);
            nota.ForeColor = tema.CabeceraSuave;
            nota.BackColor = Color.Transparent;
            nota.Location = new Point(70, titulo.Bottom);
            contenedor.Controls.Add(nota);

            return nota.Bottom + 9;
        }

        public static void EstilizarBotonMenu(Button boton, bool activo)
        {
            Tema tema = TemaActual.Actual;
            Color acento = tema.AcentoVisible(TemaActual.Acento);

            boton.AutoSize = false;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.TextAlign = ContentAlignment.MiddleLeft;
            boton.Padding = new Padding(12, 0, 0, 0);
            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;
            boton.Font = new Font("Segoe UI", 10, activo ? FontStyle.Bold : FontStyle.Regular);
            boton.BackColor = activo ? Tema.Mezclar(tema.Menu, acento, 30) : tema.Menu;
            boton.ForeColor = activo ? Color.White : tema.MenuTexto;
            boton.FlatAppearance.MouseOverBackColor = Tema.Mezclar(tema.Menu, acento, 18);
            boton.FlatAppearance.MouseDownBackColor = Tema.Mezclar(tema.Menu, acento, 38);
        }

        public static void EstilizarBotonNav(Button boton, bool activo, Color acento)
        {
            Tema tema = TemaActual.Actual;
            Color acentoVisible = tema.AcentoVisible(acento);
            Color fondoActivo = Tema.Mezclar(tema.Cabecera, acentoVisible, 18);

            boton.AutoSize = false;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.TextAlign = ContentAlignment.MiddleCenter;
            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;
            boton.Font = new Font("Segoe UI", 9.5f, activo ? FontStyle.Bold : FontStyle.Regular);
            boton.BackColor = activo ? fondoActivo : tema.Cabecera;
            boton.ForeColor = activo ? acentoVisible : tema.CabeceraSuave;
            boton.FlatAppearance.MouseOverBackColor = activo ? fondoActivo : Tema.Mezclar(tema.Cabecera, acentoVisible, 8);
            boton.FlatAppearance.MouseDownBackColor = Tema.Mezclar(tema.Cabecera, acentoVisible, 26);
            Redondear(boton, 19);
        }

        /// <summary>
        /// Estilo de los botones de la barra "Mas" (segundo nivel de navegacion).
        /// </summary>
        public static void EstilizarBotonSubNav(Button boton, bool activo, Color acento, Color fondo)
        {
            Tema tema = TemaActual.Actual;
            Color acentoVisible = tema.AcentoVisible(acento);
            Color fondoActivo = Tema.Mezclar(fondo, acentoVisible, 20);

            boton.AutoSize = false;
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.TextAlign = ContentAlignment.MiddleCenter;
            boton.Cursor = Cursors.Hand;
            boton.UseVisualStyleBackColor = false;
            boton.Font = new Font("Segoe UI", 9f, activo ? FontStyle.Bold : FontStyle.Regular);
            boton.BackColor = activo ? fondoActivo : fondo;
            boton.ForeColor = activo ? acentoVisible : tema.CabeceraTexto;
            boton.FlatAppearance.MouseOverBackColor = activo ? fondoActivo : Tema.Mezclar(fondo, acentoVisible, 10);
            boton.FlatAppearance.MouseDownBackColor = Tema.Mezclar(fondo, acentoVisible, 26);
            Redondear(boton, 17);
        }

        public static GraphicsPath CrearRutaRedonda(Rectangle area, int radio)
        {
            GraphicsPath ruta = new GraphicsPath();
            int diametro = radio * 2;

            if (diametro > area.Width)
            {
                diametro = area.Width;
            }

            if (diametro > area.Height)
            {
                diametro = area.Height;
            }

            if (diametro <= 0)
            {
                ruta.AddRectangle(area);
                return ruta;
            }

            ruta.AddArc(area.X, area.Y, diametro, diametro, 180, 90);
            ruta.AddArc(area.Right - diametro, area.Y, diametro, diametro, 270, 90);
            ruta.AddArc(area.Right - diametro, area.Bottom - diametro, diametro, diametro, 0, 90);
            ruta.AddArc(area.X, area.Bottom - diametro, diametro, diametro, 90, 90);
            ruta.CloseFigure();
            return ruta;
        }

        public static void PrepararGrilla(DataGridView grilla)
        {
            Tema tema = TemaActual.Actual;
            Color acento = tema.AcentoVisible(TemaActual.Acento);

            grilla.ReadOnly = true;
            grilla.AllowUserToAddRows = false;
            grilla.AllowUserToDeleteRows = false;
            grilla.AllowUserToResizeRows = false;
            grilla.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grilla.MultiSelect = false;
            grilla.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grilla.BackgroundColor = tema.Superficie;
            grilla.GridColor = tema.BordeSuave;
            grilla.BorderStyle = BorderStyle.None;
            grilla.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            grilla.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            grilla.RowHeadersVisible = false;
            grilla.EnableHeadersVisualStyles = false;
            grilla.ScrollBars = ScrollBars.Both;
            grilla.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.None;
            grilla.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grilla.ColumnHeadersDefaultCellStyle.BackColor = tema.Cabecera;
            grilla.ColumnHeadersDefaultCellStyle.ForeColor = tema.CabeceraTexto;
            grilla.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            grilla.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
            grilla.ColumnHeadersDefaultCellStyle.Padding = new Padding(14, 8, 14, 8);
            grilla.DefaultCellStyle.BackColor = tema.Superficie;
            grilla.DefaultCellStyle.ForeColor = tema.Texto;
            grilla.DefaultCellStyle.Font = new Font("Segoe UI", 9.5f);
            grilla.DefaultCellStyle.SelectionBackColor = acento;
            grilla.DefaultCellStyle.SelectionForeColor = ColorContraste(acento);
            grilla.DefaultCellStyle.WrapMode = DataGridViewTriState.False;
            grilla.DefaultCellStyle.Padding = new Padding(14, 6, 14, 6);
            grilla.AlternatingRowsDefaultCellStyle.BackColor = tema.SuperficieAlterna;
            grilla.AlternatingRowsDefaultCellStyle.ForeColor = tema.Texto;
            grilla.AlternatingRowsDefaultCellStyle.SelectionBackColor = acento;
            grilla.AlternatingRowsDefaultCellStyle.SelectionForeColor = ColorContraste(acento);
            grilla.RowTemplate.Height = 38;
            grilla.MinimumSize = new Size(320, 180);

            grilla.Paint += AlPintarBordeGrilla;

            if (grilla.Dock == DockStyle.None && (grilla.Anchor & AnchorStyles.Right) == AnchorStyles.Right)
            {
                grilla.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                grilla.ParentChanged += AlCambiarPadreGrilla;
            }
        }

        private static void AlPintarBordeGrilla(object remitente, PaintEventArgs argumentos)
        {
            DataGridView grilla = (DataGridView)remitente;

            if (grilla.Width <= 2 || grilla.Height <= 2)
            {
                return;
            }

            using (GraphicsPath ruta = CrearRutaRedonda(new Rectangle(0, 0, grilla.Width - 1, grilla.Height - 1), RadioBoton))
            {
                using (Pen lapiz = new Pen(TemaActual.Actual.BordeSuave))
                {
                    argumentos.Graphics.DrawPath(lapiz, ruta);
                }
            }
        }

        private static void AlCambiarPadreGrilla(object remitente, EventArgs argumentos)
        {
            DataGridView grilla = remitente as DataGridView;

            if (grilla == null || grilla.Parent == null || grilla.Dock != DockStyle.None)
            {
                return;
            }

            AjustarAnchoGrilla(grilla);
            grilla.Parent.ClientSizeChanged += delegate(object remitentePadre, EventArgs argumentosPadre)
            {
                AjustarAnchoGrilla(grilla);
            };
        }

        private static void AjustarAnchoGrilla(DataGridView grilla)
        {
            if (grilla.Parent == null || (grilla.Anchor & AnchorStyles.Right) == AnchorStyles.Right)
            {
                return;
            }

            int disponible = grilla.Parent.ClientSize.Width - grilla.Left * 2;

            if (disponible < 320)
            {
                disponible = 320;
            }

            grilla.Width = disponible;
        }

        public static Label CrearEtiqueta(string texto, int x, int y)
        {
            Label etiqueta = new Label();
            etiqueta.Text = texto;
            etiqueta.Location = new Point(x, y);
            etiqueta.AutoSize = true;
            etiqueta.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            etiqueta.ForeColor = TemaActual.Actual.Texto;
            etiqueta.BackColor = Color.Transparent;
            return etiqueta;
        }

        public static Label CrearNota(string texto, int x, int y, int ancho)
        {
            Label nota = new Label();
            nota.Text = texto;
            nota.Location = new Point(x, y);
            nota.AutoSize = true;
            nota.MaximumSize = new Size(ancho, 0);
            nota.Font = new Font("Segoe UI", 9);
            nota.ForeColor = TemaActual.Actual.TextoSecundario;
            nota.BackColor = Color.Transparent;
            return nota;
        }

        public static void AjustarEtiqueta(Label etiqueta, int ancho)
        {
            if (etiqueta == null)
            {
                return;
            }

            int maximo = Math.Max(1, ancho);
            etiqueta.AutoSize = true;
            etiqueta.MaximumSize = new Size(maximo, 0);
            etiqueta.Size = new Size(Math.Min(maximo, etiqueta.PreferredWidth), etiqueta.PreferredHeight);
        }

        public static Label CrearEtiquetaSuave(string texto, int x, int y)
        {
            Label etiqueta = new Label();
            etiqueta.Text = texto;
            etiqueta.Location = new Point(x, y);
            etiqueta.AutoSize = true;
            etiqueta.Font = new Font("Segoe UI", 8.5f);
            etiqueta.ForeColor = TemaActual.Actual.TextoSecundario;
            etiqueta.BackColor = Color.Transparent;
            return etiqueta;
        }

        public static Label CrearTitulo(string texto, int x, int y)
        {
            Label titulo = new Label();
            titulo.Text = texto;
            titulo.Location = new Point(x, y);
            titulo.AutoSize = true;
            titulo.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            titulo.ForeColor = TemaActual.Actual.Texto;
            titulo.BackColor = Color.Transparent;
            return titulo;
        }

        public static Label CrearValor(string texto, int x, int y)
        {
            Label etiqueta = new Label();
            etiqueta.Text = texto;
            etiqueta.Location = new Point(x, y);
            etiqueta.AutoSize = true;
            etiqueta.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            etiqueta.ForeColor = TemaActual.Actual.Texto;
            etiqueta.BackColor = Color.Transparent;
            return etiqueta;
        }

        public static TextBox CrearCampo(int x, int y, int ancho)
        {
            TextBox campo = new TextBox();
            campo.Location = new Point(x, y);
            campo.Width = ancho;
            campo.Font = new Font("Segoe UI", 10);
            AplicarEstiloCampo(campo);
            return campo;
        }

        public static ComboBox CrearLista(int x, int y, int ancho)
        {
            ComboBox lista = new ComboBox();
            lista.Location = new Point(x, y);
            lista.Width = ancho;
            lista.Font = new Font("Segoe UI", 10);
            lista.DropDownStyle = ComboBoxStyle.DropDownList;
            lista.FlatStyle = FlatStyle.Flat;
            lista.DrawMode = DrawMode.OwnerDrawFixed;
            lista.ItemHeight = 24;
            lista.DrawItem += AlDibujarElementoLista;
            AplicarEstiloCampo(lista);
            return lista;
        }

        private static void AlDibujarElementoLista(object remitente, DrawItemEventArgs argumentos)
        {
            ComboBox lista = (ComboBox)remitente;
            Tema tema = TemaActual.Actual;
            bool marcado = (argumentos.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color fondo = marcado ? tema.AcentoVisible(TemaActual.Acento) : tema.Superficie;
            Color texto = marcado ? ColorContraste(fondo) : tema.Texto;

            using (SolidBrush pincel = new SolidBrush(fondo))
            {
                argumentos.Graphics.FillRectangle(pincel, argumentos.Bounds);
            }

            int indice = argumentos.Index;

            if (indice < 0)
            {
                indice = lista.SelectedIndex;
            }

            if (indice < 0 || indice >= lista.Items.Count)
            {
                return;
            }

            string valor = lista.GetItemText(lista.Items[indice]);
            Rectangle zona = new Rectangle(
                argumentos.Bounds.X + 6,
                argumentos.Bounds.Y,
                argumentos.Bounds.Width - 28,
                argumentos.Bounds.Height);

            TextRenderer.DrawText(
                argumentos.Graphics,
                valor,
                lista.Font,
                zona,
                texto,
                TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        public static Button CrearBoton(string texto, int x, int y, int ancho, int alto, Color color)
        {
            Button boton = new Button();
            boton.Text = texto;
            boton.Location = new Point(x, y);
            boton.Width = ancho;
            boton.Height = alto;
            boton.MinimumSize = new Size(90, 32);
            boton.BackColor = color;
            boton.ForeColor = ColorContraste(color);
            boton.FlatStyle = FlatStyle.Flat;
            boton.FlatAppearance.BorderSize = 0;
            boton.FlatAppearance.MouseOverBackColor = Tema.Mezclar(color, Color.White, 14);
            boton.FlatAppearance.MouseDownBackColor = Tema.Mezclar(color, Color.Black, 12);
            boton.Cursor = Cursors.Hand;
            boton.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            boton.Padding = new Padding(6, 0, 6, 0);
            boton.UseVisualStyleBackColor = false;
            Redondear(boton, RadioBoton);
            return boton;
        }

        public static Button CrearBotonSecundario(string texto, int x, int y, int ancho, int alto)
        {
            Tema tema = TemaActual.Actual;
            Button boton = CrearBoton(texto, x, y, ancho, alto, tema.Superficie);
            boton.ForeColor = tema.Texto;
            boton.FlatAppearance.BorderSize = 1;
            boton.FlatAppearance.BorderColor = tema.Borde;
            boton.FlatAppearance.MouseOverBackColor = tema.SuperficieAlterna;
            boton.FlatAppearance.MouseDownBackColor = tema.Borde;
            return boton;
        }

        public static Button CrearBotonPeligro(string texto, int x, int y, int ancho, int alto)
        {
            return CrearBoton(texto, x, y, ancho, alto, TemaActual.Actual.Peligro);
        }

        public static Panel CrearTarjeta(string titulo, int x, int y, int ancho, int alto, Color acento)
        {
            Tema tema = TemaActual.Actual;
            Color acentoVisible = tema.AcentoVisible(acento);

            Panel tarjeta = new Panel();
            tarjeta.Location = new Point(x, y);
            tarjeta.Width = ancho;
            tarjeta.Height = alto;
            tarjeta.BackColor = tema.Superficie;
            tarjeta.BorderStyle = BorderStyle.None;
            tarjeta.Paint += AlPintarTarjeta;

            Panel franja = new Panel();
            franja.Name = "Franja";
            franja.Location = new Point(0, 14);
            franja.Width = 4;
            franja.Height = alto - 28;
            franja.BackColor = acentoVisible;
            tarjeta.Controls.Add(franja);

            Label rotulo = new Label();
            rotulo.Name = "Titulo";
            rotulo.Text = titulo;
            rotulo.Location = new Point(18, 12);
            rotulo.AutoSize = true;
            rotulo.MaximumSize = new Size(ancho - 36, 0);
            rotulo.Font = new Font("Segoe UI", 9);
            rotulo.ForeColor = tema.TextoSecundario;
            rotulo.BackColor = Color.Transparent;
            tarjeta.Controls.Add(rotulo);

            Label valor = new Label();
            valor.Name = "Valor";
            valor.Text = "";
            valor.Location = new Point(18, 34);
            valor.Width = ancho - 36;
            valor.Height = 34;
            valor.AutoSize = false;
            valor.AutoEllipsis = true;
            valor.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            valor.ForeColor = tema.Texto;
            valor.BackColor = Color.Transparent;
            tarjeta.Controls.Add(valor);

            Redondear(tarjeta, RadioTarjeta);
            return tarjeta;
        }

        public static void AjustarTarjeta(Panel tarjeta, int ancho, int alto)
        {
            if (tarjeta == null)
            {
                return;
            }

            tarjeta.Size = new Size(Math.Max(1, ancho), Math.Max(1, alto));

            foreach (Control control in tarjeta.Controls)
            {
                Label etiqueta = control as Label;

                if (control.Name == "Franja")
                {
                    control.Height = Math.Max(1, alto - 28);
                }
                else if (control.Name == "Titulo" && etiqueta != null)
                {
                    etiqueta.MaximumSize = new Size(Math.Max(1, ancho - 36), 0);
                    etiqueta.Width = Math.Min(etiqueta.MaximumSize.Width, etiqueta.PreferredWidth);
                    etiqueta.Height = etiqueta.PreferredHeight;
                }
                else if (control.Name == "Valor" && etiqueta != null)
                {
                    etiqueta.SetBounds(18, 34, Math.Max(1, ancho - 36), Math.Max(etiqueta.PreferredHeight, 34));
                }
            }

            Redondear(tarjeta, RadioTarjeta);
        }

        private static void AlPintarTarjeta(object remitente, PaintEventArgs argumentos)
        {
            Panel tarjeta = (Panel)remitente;

            using (GraphicsPath ruta = CrearRutaRedonda(new Rectangle(0, 0, tarjeta.Width - 1, tarjeta.Height - 1), RadioTarjeta))
            {
                using (Pen lapiz = new Pen(TemaActual.Actual.Borde))
                {
                    argumentos.Graphics.DrawPath(lapiz, ruta);
                }
            }
        }

        public static void PonerValor(Panel tarjeta, string texto)
        {
            foreach (Control control in tarjeta.Controls)
            {
                if (control.Name == "Valor")
                {
                    control.Text = texto;
                }
            }
        }

        public static void PrepararFormulario(Form formulario)
        {
            Tema tema = TemaActual.Actual;
            Color acento = tema.AcentoVisible(TemaActual.Acento);

            formulario.AutoScaleMode = AutoScaleMode.Dpi;
            formulario.AutoScaleDimensions = new SizeF(96, 96);
            formulario.BackColor = tema.Fondo;
            formulario.ForeColor = tema.Texto;
            formulario.Font = new Font("Segoe UI", 10);
            formulario.AutoScroll = true;
            formulario.StartPosition = FormStartPosition.CenterParent;
            AjustarVentanaAPantalla(formulario);
            formulario.Paint += delegate(object remitente, PaintEventArgs argumentos)
            {
                using (SolidBrush pincel = new SolidBrush(acento))
                {
                    argumentos.Graphics.FillRectangle(pincel, 0, 0, formulario.ClientSize.Width, 4);
                }
            };
        }

        public static Size TamanoMaximoVentana(Form formulario)
        {
            Screen pantalla = formulario.Owner != null
                ? Screen.FromControl(formulario.Owner)
                : Screen.FromPoint(Cursor.Position);

            if (pantalla == null)
            {
                pantalla = Screen.PrimaryScreen;
            }

            Rectangle area = pantalla.WorkingArea;
            return new Size(Math.Max(320, area.Width - 32), Math.Max(240, area.Height - 64));
        }

        public static void AjustarVentanaAPantalla(Form formulario)
        {
            Size maximo = TamanoMaximoVentana(formulario);
            Size deseado = formulario.ClientSize;
            formulario.ClientSize = new Size(
                Math.Min(deseado.Width, maximo.Width),
                Math.Min(deseado.Height, maximo.Height));
            formulario.MinimumSize = new Size(
                Math.Min(formulario.MinimumSize.Width, maximo.Width),
                Math.Min(formulario.MinimumSize.Height, maximo.Height));
        }

        public static void EstilizarCampos(Control contenedor)
        {
            foreach (Control control in contenedor.Controls)
            {
                EstilizarCampo(control);
                EstilizarCampos(control);
            }
        }

        public static void EstilizarCampo(Control control)
        {
            Tema tema = TemaActual.Actual;

            if (control is TextBox || control is ComboBox || control is NumericUpDown || control is DateTimePicker)
            {
                AplicarEstiloCampo(control);
            }
            else if (control is CheckBox || control is RadioButton)
            {
                control.BackColor = Color.Transparent;
                control.ForeColor = tema.Texto;
            }
            else if (control is GroupBox)
            {
                control.BackColor = Color.Transparent;
                control.ForeColor = tema.Texto;
            }
            else if (control is Label)
            {
                if (control.ForeColor != tema.AcentoVisible(TemaActual.Acento) &&
                    control.ForeColor != tema.TextoSecundario &&
                    control.ForeColor != tema.CabeceraSuave &&
                    control.ForeColor != tema.Peligro &&
                    control.ForeColor != tema.Exito)
                {
                    control.ForeColor = tema.Texto;
                }

                control.BackColor = Color.Transparent;
            }
            else if (control is Panel)
            {
                if (control.BackColor == Color.White || control.BackColor == Color.FromArgb(244, 246, 249))
                {
                    control.BackColor = tema.Superficie;
                }
            }
            else if (control is Button)
            {
                control.ForeColor = ColorContraste(control.BackColor);
            }
        }

        public static void AplicarEstiloCampo(Control campo)
        {
            Tema tema = TemaActual.Actual;

            campo.BackColor = tema.Superficie;
            campo.ForeColor = tema.Texto;
            campo.Font = new Font("Segoe UI", 10);

            if (campo is TextBox)
            {
                TextBox caja = (TextBox)campo;
                caja.BorderStyle = BorderStyle.FixedSingle;
            }
        }

        public static Color ColorContraste(Color fondo)
        {
            double luminancia = 0.2126 * CanalLineal(fondo.R) + 0.7152 * CanalLineal(fondo.G) + 0.0722 * CanalLineal(fondo.B);

            if (luminancia > 0.38)
            {
                return Color.FromArgb(12, 18, 16);
            }

            return Color.White;
        }

        private static double CanalLineal(int canal)
        {
            double valor = canal / 255.0;

            if (valor <= 0.03928)
            {
                return valor / 12.92;
            }

            return Math.Pow((valor + 0.055) / 1.055, 2.4);
        }

        public static Color AcentoDeUsuario(string texto)
        {
            Color color = ColorDesdeTexto(texto);
            Color predeterminadoAnterior = Color.FromArgb(47, 111, 237);

            if (color.ToArgb() == predeterminadoAnterior.ToArgb() && TemaActual.Actual.Visual == TemaVisual.Oscuro)
            {
                return Color.FromArgb(23, 201, 100);
            }

            return color;
        }

        public static Color ColorDesdeTexto(string texto)
        {
            try
            {
                return ColorTranslator.FromHtml(texto);
            }
            catch
            {
                return Color.FromArgb(47, 111, 237);
            }
        }

        public static int TamanoFuenteSeguro(int tamano)
        {
            if (tamano < 8)
            {
                return 8;
            }

            if (tamano > 14)
            {
                return 14;
            }

            return tamano;
        }

        public static string Moneda(decimal monto)
        {
            return monto.ToString("C2");
        }

        public static string MonedaCorta(decimal monto)
        {
            return monto.ToString("C0");
        }

        public static string Porcentaje(decimal valor)
        {
            return valor.ToString("0.##") + "%";
        }

        public static void Informar(string mensaje)
        {
            MessageBox.Show(mensaje, "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static void AvisarError(string mensaje)
        {
            MessageBox.Show(mensaje, "FinOptima", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        public static bool Confirmar(string mensaje)
        {
            DialogResult respuesta = MessageBox.Show(mensaje, "FinOptima", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            return respuesta == DialogResult.Yes;
        }

        public static string NombreTipoSobre(int tipo)
        {
            if (tipo == 1)
            {
                return "Necesidades";
            }

            if (tipo == 2)
            {
                return "Ahorro";
            }

            if (tipo == 3)
            {
                return "Inversion";
            }

            if (tipo == 4)
            {
                return "Educacion";
            }

            if (tipo == 5)
            {
                return "Ocio";
            }

            if (tipo == 6)
            {
                return "Generosidad";
            }

            return "Metas";
        }

        public static string NombreTipoMovimiento(int tipo)
        {
            if (tipo == 1)
            {
                return "Ingreso";
            }

            return "Gasto";
        }

        public static string NombrePrioridad(int prioridad)
        {
            if (prioridad == 3)
            {
                return "Alta";
            }

            if (prioridad == 1)
            {
                return "Baja";
            }

            return "Media";
        }
    }
}
