using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FinOptima.Bot
{
    public static class InterpretadorMensaje
    {
        private static readonly Regex PatronMonto = new Regex(
            @"(\d{1,3}(?:[.,]\d{3})+|\d+(?:[.,]\d+)?)(?:\s*(mil|millones|millon|k|m)\b)?",
            RegexOptions.Compiled);

        private static readonly string[] PalabrasSueldo = { "sueldo", "salario", "nomina", "quincena" };
        private static readonly string[] PalabrasIngreso = { "ingreso", "ingresaron", "recibi", "recibimos", "sueldo", "salario", "nomina", "quincena", "ganancia", "venta" };
        private static readonly string[] PalabrasGasto = { "gaste", "gastamos", "gasto", "pague", "pago", "compre", "compra", "consumi" };

        public static Interpretacion Interpretar(string texto)
        {
            Interpretacion interpretacion = new Interpretacion();

            if (texto == null)
            {
                return interpretacion;
            }

            string limpio = texto.Trim();

            if (limpio.Length == 0)
            {
                return interpretacion;
            }

            string normalizado = Normalizar(limpio);
            TipoInterpretacion boton = InterpretarBoton(normalizado);

            if (boton != TipoInterpretacion.Desconocido)
            {
                interpretacion.Tipo = boton;
                return interpretacion;
            }

            if (EsComando(normalizado, "/start") || EsComando(normalizado, "/ayuda") || EsComando(normalizado, "/help"))
            {
                interpretacion.Tipo = TipoInterpretacion.Ayuda;
                return interpretacion;
            }

            if (EsComando(normalizado, "/saldo"))
            {
                interpretacion.Tipo = TipoInterpretacion.Saldo;
                return interpretacion;
            }

            if (EsComando(normalizado, "/sobres"))
            {
                interpretacion.Tipo = TipoInterpretacion.Sobres;
                return interpretacion;
            }

            if (EsComando(normalizado, "/alertas"))
            {
                interpretacion.Tipo = TipoInterpretacion.Alertas;
                return interpretacion;
            }

            bool comandoIngreso = EsComando(normalizado, "/ingreso");
            bool comandoGasto = EsComando(normalizado, "/gasto");

            bool esIngreso = comandoIngreso
                || ContieneAlguna(normalizado, PalabrasIngreso)
                || ContieneFrase(normalizado, "me llego")
                || ContieneFrase(normalizado, "me llegaron")
                || ContieneFrase(normalizado, "me pagaron");

            bool esGasto = comandoGasto || ContieneAlguna(normalizado, PalabrasGasto);

            if (comandoIngreso)
            {
                interpretacion.Tipo = TipoInterpretacion.Ingreso;
            }
            else if (comandoGasto)
            {
                interpretacion.Tipo = TipoInterpretacion.Gasto;
            }
            else if (esIngreso)
            {
                interpretacion.Tipo = TipoInterpretacion.Ingreso;
            }
            else if (esGasto)
            {
                interpretacion.Tipo = TipoInterpretacion.Gasto;
            }
            else
            {
                return interpretacion;
            }

            Match coincidencia;
            interpretacion.Monto = BuscarMonto(normalizado, out coincidencia);
            interpretacion.EsSueldo = ContieneAlguna(normalizado, PalabrasSueldo);

            string categoria;
            interpretacion.SobreSugerido = ClasificadorSobres.Clasificar(normalizado, out categoria);

            if (interpretacion.EsSueldo)
            {
                interpretacion.Categoria = "Sueldo";
            }
            else if (categoria.Length > 0)
            {
                interpretacion.Categoria = categoria;
            }
            else if (interpretacion.Tipo == TipoInterpretacion.Ingreso)
            {
                interpretacion.Categoria = "Ingreso";
            }
            else
            {
                interpretacion.Categoria = "General";
            }

            interpretacion.Descripcion = LimpiarDescripcion(limpio, comandoIngreso || comandoGasto, coincidencia);
            return interpretacion;
        }

        /// <summary>
        /// Reconoce el texto de los botones del teclado fijo (por ejemplo "➖ Registrar gasto").
        /// Solo coincide con la frase completa, para no confundir un mensaje como
        /// "gaste 5000 en sobres de cartas" con el boton "Sobres".
        /// </summary>
        public static TipoInterpretacion InterpretarBoton(string normalizado)
        {
            string frase = LimpiarSignos(normalizado);

            switch (frase)
            {
                case "registrar gasto":
                case "nuevo gasto":
                case "gasto":
                case "gastos":
                    return TipoInterpretacion.IniciarGasto;
                case "registrar ingreso":
                case "nuevo ingreso":
                case "ingreso":
                case "ingresos":
                    return TipoInterpretacion.IniciarIngreso;
                case "resumen":
                case "ver resumen":
                case "saldo":
                case "mi saldo":
                    return TipoInterpretacion.Saldo;
                case "sobres":
                case "mis sobres":
                case "ver sobres":
                    return TipoInterpretacion.Sobres;
                case "alertas":
                case "ver alertas":
                    return TipoInterpretacion.Alertas;
                case "ayuda":
                case "menu":
                case "inicio":
                case "start":
                case "help":
                case "hola":
                    return TipoInterpretacion.Ayuda;
                default:
                    return TipoInterpretacion.Desconocido;
            }
        }

        /// <summary>
        /// Lee el monto de un texto libre ("45000", "45.000", "50 mil", "1,5 millones").
        /// </summary>
        public static decimal ExtraerMonto(string texto)
        {
            Match coincidencia;
            return BuscarMonto(Normalizar(texto), out coincidencia);
        }

        public static string Normalizar(string texto)
        {
            if (texto == null)
            {
                return "";
            }

            string minusculas = texto.ToLowerInvariant();
            StringBuilder resultado = new StringBuilder();

            foreach (char letra in minusculas)
            {
                switch (letra)
                {
                    case 'á':
                        resultado.Append('a');
                        break;
                    case 'é':
                        resultado.Append('e');
                        break;
                    case 'í':
                    case 'ï':
                        resultado.Append('i');
                        break;
                    case 'ó':
                        resultado.Append('o');
                        break;
                    case 'ú':
                    case 'ü':
                        resultado.Append('u');
                        break;
                    case 'ñ':
                        resultado.Append('n');
                        break;
                    default:
                        resultado.Append(letra);
                        break;
                }
            }

            return resultado.ToString();
        }

        public static bool ContienePalabra(string texto, string palabra)
        {
            string acotado = " " + LimpiarSignos(texto) + " ";
            string buscada = " " + palabra + " ";
            return acotado.IndexOf(buscada, StringComparison.Ordinal) >= 0;
        }

        private static bool ContieneAlguna(string texto, string[] palabras)
        {
            foreach (string palabra in palabras)
            {
                if (ContienePalabra(texto, palabra))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContieneFrase(string texto, string frase)
        {
            string acotado = " " + LimpiarSignos(texto) + " ";
            string buscada = " " + frase + " ";
            return acotado.IndexOf(buscada, StringComparison.Ordinal) >= 0;
        }

        private static bool EsComando(string texto, string comando)
        {
            string primero = PrimeraPalabra(texto);

            if (primero == comando)
            {
                return true;
            }

            if (primero.Length > comando.Length && primero.StartsWith(comando + "@", StringComparison.Ordinal))
            {
                return true;
            }

            return false;
        }

        private static string PrimeraPalabra(string texto)
        {
            int indice = texto.IndexOf(' ');

            if (indice < 0)
            {
                return texto;
            }

            return texto.Substring(0, indice);
        }

        private static string LimpiarSignos(string texto)
        {
            StringBuilder resultado = new StringBuilder();

            foreach (char letra in texto)
            {
                if (char.IsLetterOrDigit(letra))
                {
                    resultado.Append(letra);
                }
                else if (resultado.Length > 0 && resultado[resultado.Length - 1] != ' ')
                {
                    resultado.Append(' ');
                }
            }

            return resultado.ToString().Trim();
        }

        private static string UnirEspacios(string texto)
        {
            StringBuilder resultado = new StringBuilder();
            bool anteriorEspacio = false;

            foreach (char letra in texto)
            {
                if (letra == ' ' || letra == '\t' || letra == '\r' || letra == '\n')
                {
                    if (!anteriorEspacio && resultado.Length > 0)
                    {
                        resultado.Append(' ');
                    }

                    anteriorEspacio = true;
                }
                else
                {
                    resultado.Append(letra);
                    anteriorEspacio = false;
                }
            }

            return resultado.ToString().Trim();
        }

        private static string LimpiarDescripcion(string limpio, bool quitarComando, Match monto)
        {
            string resultado = limpio;

            if (monto != null && monto.Index >= 0 && monto.Index + monto.Length <= resultado.Length)
            {
                resultado = resultado.Remove(monto.Index, monto.Length);
            }

            resultado = resultado.Trim();

            if (quitarComando)
            {
                int espacio = resultado.IndexOf(' ');

                if (espacio < 0)
                {
                    resultado = "";
                }
                else
                {
                    resultado = resultado.Substring(espacio + 1).Trim();
                }
            }

            resultado = UnirEspacios(resultado);

            if (resultado.Length == 0)
            {
                resultado = "Registrado desde Telegram";
            }

            if (resultado.Length > 200)
            {
                resultado = resultado.Substring(0, 200);
            }

            return resultado;
        }

        private static decimal BuscarMonto(string texto, out Match elegido)
        {
            elegido = null;
            decimal mayor = 0;
            Match mayorCoincidencia = null;

            foreach (Match coincidencia in PatronMonto.Matches(texto))
            {
                decimal valor = CalcularMonto(coincidencia);

                if (valor <= 0)
                {
                    continue;
                }

                if (coincidencia.Groups[2].Value.Length > 0)
                {
                    elegido = coincidencia;
                    return valor;
                }

                if (valor > mayor)
                {
                    mayor = valor;
                    mayorCoincidencia = coincidencia;
                }
            }

            elegido = mayorCoincidencia;
            return mayor;
        }

        private static decimal CalcularMonto(Match coincidencia)
        {
            decimal valor = ConvertirNumero(coincidencia.Groups[1].Value);
            string multiplicador = coincidencia.Groups[2].Value;

            if (multiplicador == "mil" || multiplicador == "k")
            {
                valor = valor * 1000;
            }
            else if (multiplicador == "millon" || multiplicador == "millones" || multiplicador == "m")
            {
                valor = valor * 1000000;
            }

            if (valor > 1000000000)
            {
                return 0;
            }

            return valor;
        }

        private static decimal ConvertirNumero(string numero)
        {
            bool tienePunto = numero.IndexOf('.') >= 0;
            bool tieneComa = numero.IndexOf(',') >= 0;
            string limpio = numero;

            if (tienePunto && tieneComa)
            {
                bool ultimoEsPunto = numero.LastIndexOf('.') > numero.LastIndexOf(',');

                if (ultimoEsPunto)
                {
                    limpio = numero.Replace(",", "");
                }
                else
                {
                    limpio = numero.Replace(".", "").Replace(',', '.');
                }
            }
            else if (tieneComa)
            {
                if (SeparadorEsMiles(numero, ','))
                {
                    limpio = numero.Replace(",", "");
                }
                else
                {
                    limpio = numero.Replace(',', '.');
                }
            }
            else if (tienePunto)
            {
                if (SeparadorEsMiles(numero, '.'))
                {
                    limpio = numero.Replace(".", "");
                }
            }

            decimal valor;
            bool correcto = decimal.TryParse(limpio, NumberStyles.Number, CultureInfo.InvariantCulture, out valor);

            if (!correcto)
            {
                return 0;
            }

            return valor;
        }

        private static bool SeparadorEsMiles(string numero, char separador)
        {
            string[] partes = numero.Split(separador);

            for (int posicion = 1; posicion < partes.Length; posicion = posicion + 1)
            {
                if (partes[posicion].Length != 3)
                {
                    return false;
                }
            }

            return partes.Length > 1;
        }
    }
}
