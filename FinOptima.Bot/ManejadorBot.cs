using System;
using System.Collections.Generic;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Composicion;
using FinOptima.Logica.Modelos;

namespace FinOptima.Bot
{
    /// <summary>
    /// Atiende los mensajes y botones de Telegram. Todas las acciones se hacen con botones:
    /// el teclado fijo abre cada flujo y los botones de cada mensaje guian paso a paso
    /// (sobre, monto, descripcion y confirmacion). Escribir en lenguaje natural sigue
    /// funcionando como atajo.
    /// </summary>
    public class ManejadorBot
    {
        private const int MinutosVigencia = 10;

        // Datos de los botones inline (maximo 64 bytes cada uno).
        private const string DatoMenuResumen = "menu:resumen";
        private const string DatoMenuSobres = "menu:sobres";
        private const string DatoMenuAlertas = "menu:alertas";
        private const string DatoMenuGasto = "menu:gasto";
        private const string DatoMenuIngreso = "menu:ingreso";
        private const string DatoIngresoSueldo = "ing:sueldo";
        private const string DatoIngresoOtro = "ing:otro";
        private const string PrefijoSobre = "sob:";
        private const string PrefijoMonto = "mon:";
        private const string DatoMontoOtro = "mon:otro";
        private const string DatoSinDescripcion = "desc:no";
        private const string DatoCambiarSobre = "cambiar";
        private const string DatoConfirmar = "confirmar";
        private const string DatoCancelar = "cancelar";

        private static readonly decimal[] MontosGasto = { 10000, 20000, 50000, 100000, 200000, 500000 };
        private static readonly decimal[] MontosIngreso = { 100000, 500000, 1000000, 2000000, 3000000, 5000000 };

        private ICanalTelegram cliente;
        private FabricaServicios servicios;
        private long chatAutorizado;
        private Dictionary<long, OperacionPendiente> pendientes;

        public ManejadorBot(ICanalTelegram clienteTelegram, FabricaServicios fabrica, long chatPermitido)
        {
            cliente = clienteTelegram;
            servicios = fabrica;
            chatAutorizado = chatPermitido;
            pendientes = new Dictionary<long, OperacionPendiente>();
        }

        public void Procesar(ActualizacionTelegram actualizacion)
        {
            if (actualizacion.ChatId != chatAutorizado)
            {
                if (actualizacion.EsCallback)
                {
                    cliente.ResponderCallback(actualizacion.CallbackId, "Chat no autorizado.");
                }
                else
                {
                    cliente.EnviarMensaje(actualizacion.ChatId, "Este bot es privado de FinOptima.", null);
                }

                return;
            }

            if (actualizacion.EsCallback)
            {
                ProcesarCallback(actualizacion);
            }
            else
            {
                ProcesarTexto(actualizacion);
            }
        }

        // ---------- Teclados ----------

        public static TecladoTelegram TecladoPrincipal()
        {
            return TecladoTelegram.Respuesta()
                .Boton(RespuestasBot.BotonGasto).Boton(RespuestasBot.BotonIngreso)
                .Fila().Boton(RespuestasBot.BotonResumen).Boton(RespuestasBot.BotonSobres).Boton(RespuestasBot.BotonAlertas)
                .Fila().Boton(RespuestasBot.BotonAyuda);
        }

        private static TecladoTelegram TecladoAccionesRapidas()
        {
            return TecladoTelegram.Inline()
                .Boton("➖ Gasto", DatoMenuGasto).Boton("➕ Ingreso", DatoMenuIngreso)
                .Fila().Boton("📊 Resumen", DatoMenuResumen).Boton("✉️ Sobres", DatoMenuSobres).Boton("🔔 Alertas", DatoMenuAlertas);
        }

        private TecladoTelegram TecladoSobres(string prefijo, bool permitirVolver)
        {
            TecladoTelegram teclado = TecladoTelegram.Inline();
            int enFila = 0;

            foreach (Sobre sobre in servicios.Sobres.Listar())
            {
                if (!sobre.Activo)
                {
                    continue;
                }

                if (enFila == 2)
                {
                    teclado.Fila();
                    enFila = 0;
                }

                teclado.Boton(sobre.Nombre + " · " + RespuestasBot.Moneda(sobre.Saldo), prefijo + sobre.Id);
                enFila = enFila + 1;
            }

            teclado.Fila();

            if (permitirVolver)
            {
                teclado.Boton("← Volver", DatoConfirmar + ":ver");
            }

            teclado.Boton("✖ Cancelar", DatoCancelar);
            return teclado;
        }

        private static TecladoTelegram TecladoMontos(OperacionPendiente operacion)
        {
            decimal[] montos = operacion.Tipo == TipoMovimiento.Ingreso ? MontosIngreso : MontosGasto;
            TecladoTelegram teclado = TecladoTelegram.Inline();

            for (int indice = 0; indice < montos.Length; indice = indice + 1)
            {
                if (indice == 3)
                {
                    teclado.Fila();
                }

                teclado.Boton(MontoCorto(montos[indice]), PrefijoMonto + montos[indice].ToString("0"));
            }

            teclado.Fila().Boton("✏️ Otro monto", DatoMontoOtro).Boton("✖ Cancelar", DatoCancelar);
            return teclado;
        }

        private static TecladoTelegram TecladoConfirmacion(OperacionPendiente operacion)
        {
            TecladoTelegram teclado = TecladoTelegram.Inline()
                .Boton("✅ Confirmar", DatoConfirmar)
                .Boton("✖ Cancelar", DatoCancelar);

            if (operacion.Asignaciones.Count <= 1)
            {
                teclado.Fila().Boton("✉️ Cambiar sobre", DatoCambiarSobre);
            }

            return teclado;
        }

        // ---------- Texto recibido ----------

        private void ProcesarTexto(ActualizacionTelegram actualizacion)
        {
            Interpretacion interpretacion = InterpretadorMensaje.Interpretar(actualizacion.Texto);
            OperacionPendiente enCurso = ObtenerVigente(actualizacion.ChatId);

            bool esBotonMenu = interpretacion.Tipo == TipoInterpretacion.Ayuda
                || interpretacion.Tipo == TipoInterpretacion.Saldo
                || interpretacion.Tipo == TipoInterpretacion.Sobres
                || interpretacion.Tipo == TipoInterpretacion.Alertas
                || interpretacion.Tipo == TipoInterpretacion.IniciarGasto
                || interpretacion.Tipo == TipoInterpretacion.IniciarIngreso;

            if (!esBotonMenu && enCurso != null && enCurso.Estado == EstadoConversacion.EsperandoMonto)
            {
                RecibirMontoEscrito(actualizacion.ChatId, enCurso, actualizacion.Texto);
                return;
            }

            if (!esBotonMenu && enCurso != null && enCurso.Estado == EstadoConversacion.EsperandoDescripcion)
            {
                RecibirDescripcion(actualizacion.ChatId, enCurso, actualizacion.Texto);
                return;
            }

            if (esBotonMenu)
            {
                pendientes.Remove(actualizacion.ChatId);
            }

            switch (interpretacion.Tipo)
            {
                case TipoInterpretacion.Ayuda:
                    cliente.EnviarMensaje(actualizacion.ChatId, RespuestasBot.Ayuda(), TecladoPrincipal());
                    break;
                case TipoInterpretacion.Saldo:
                    MostrarResumen(actualizacion.ChatId);
                    break;
                case TipoInterpretacion.Sobres:
                    MostrarSobres(actualizacion.ChatId);
                    break;
                case TipoInterpretacion.Alertas:
                    MostrarAlertas(actualizacion.ChatId);
                    break;
                case TipoInterpretacion.IniciarGasto:
                    IniciarGasto(actualizacion.ChatId, 0);
                    break;
                case TipoInterpretacion.IniciarIngreso:
                    IniciarIngreso(actualizacion.ChatId, 0);
                    break;
                case TipoInterpretacion.Ingreso:
                case TipoInterpretacion.Gasto:
                    Proponer(actualizacion.ChatId, interpretacion);
                    break;
                default:
                    cliente.EnviarMensaje(actualizacion.ChatId, RespuestasBot.AyudaCorta(), TecladoPrincipal());
                    break;
            }
        }

        private void MostrarResumen(long chatId)
        {
            cliente.EnviarMensaje(chatId, RespuestasBot.Resumen(servicios.Dashboard.Obtener()), TecladoAccionesRapidas());
        }

        private void MostrarSobres(long chatId)
        {
            cliente.EnviarMensaje(chatId, RespuestasBot.ListaSobres(servicios.Sobres.Listar()), TecladoAccionesRapidas());
        }

        private void MostrarAlertas(long chatId)
        {
            cliente.EnviarMensaje(chatId, RespuestasBot.ListaAlertas(servicios.Alertas.Listar()), TecladoAccionesRapidas());
        }

        // ---------- Flujos guiados ----------

        private void IniciarGasto(long chatId, int mensajeId)
        {
            OperacionPendiente operacion = new OperacionPendiente();
            operacion.Tipo = TipoMovimiento.Gasto;
            operacion.Categoria = "General";
            operacion.Estado = EstadoConversacion.EligiendoSobre;
            operacion.MensajeId = mensajeId;
            pendientes[chatId] = operacion;

            if (!HaySobresActivos())
            {
                pendientes.Remove(chatId);
                Mostrar(chatId, mensajeId, RespuestasBot.SinSobres(), null);
                return;
            }

            Mostrar(chatId, mensajeId, RespuestasBot.PreguntaSobre(operacion), TecladoSobres(PrefijoSobre, false));
        }

        private void IniciarIngreso(long chatId, int mensajeId)
        {
            OperacionPendiente operacion = new OperacionPendiente();
            operacion.Tipo = TipoMovimiento.Ingreso;
            operacion.Categoria = "Ingreso";
            operacion.Estado = EstadoConversacion.Ninguno;
            operacion.MensajeId = mensajeId;
            pendientes[chatId] = operacion;

            TecladoTelegram teclado = TecladoTelegram.Inline()
                .Boton("💼 Sueldo (repartir en sobres)", DatoIngresoSueldo)
                .Fila().Boton("💵 Otro ingreso", DatoIngresoOtro)
                .Fila().Boton("✖ Cancelar", DatoCancelar);

            Mostrar(chatId, mensajeId, RespuestasBot.PreguntaTipoIngreso(), teclado);
        }

        private void PedirMonto(long chatId, OperacionPendiente operacion, int mensajeId)
        {
            operacion.Estado = EstadoConversacion.EsperandoMonto;
            Mostrar(chatId, mensajeId, RespuestasBot.PreguntaMonto(operacion), TecladoMontos(operacion));
        }

        private void RecibirMontoEscrito(long chatId, OperacionPendiente operacion, string texto)
        {
            decimal monto = InterpretadorMensaje.ExtraerMonto(texto);

            if (monto <= 0)
            {
                cliente.EnviarMensaje(chatId, RespuestasBot.MontoNoEntendido(), TecladoMontos(operacion));
                return;
            }

            AplicarMonto(chatId, operacion, monto, 0);
        }

        private void AplicarMonto(long chatId, OperacionPendiente operacion, decimal monto, int mensajeId)
        {
            operacion.Monto = monto;

            if (operacion.Tipo == TipoMovimiento.Ingreso && operacion.EsSueldo)
            {
                operacion.Categoria = "Sueldo";
                operacion.Descripcion = "Sueldo";
                operacion.Asignaciones = ConstruirDistribucion(monto);

                if (operacion.Asignaciones.Count == 0)
                {
                    operacion.Asignaciones = AsignacionPorDefecto(operacion, monto);
                }

                MostrarPropuesta(chatId, operacion, mensajeId);
                return;
            }

            if (operacion.Descripcion.Length > 0)
            {
                ActualizarMontoAsignacion(operacion);
                MostrarPropuesta(chatId, operacion, mensajeId);
                return;
            }

            operacion.Estado = EstadoConversacion.EsperandoDescripcion;
            TecladoTelegram teclado = TecladoTelegram.Inline()
                .Boton("Sin descripción", DatoSinDescripcion)
                .Boton("✖ Cancelar", DatoCancelar);

            Mostrar(chatId, mensajeId, RespuestasBot.PreguntaDescripcion(operacion), teclado);
        }

        private void RecibirDescripcion(long chatId, OperacionPendiente operacion, string texto)
        {
            string descripcion = texto == null ? "" : texto.Trim();

            if (descripcion.Length > 200)
            {
                descripcion = descripcion.Substring(0, 200);
            }

            AplicarDescripcion(chatId, operacion, descripcion, 0);
        }

        private void AplicarDescripcion(long chatId, OperacionPendiente operacion, string descripcion, int mensajeId)
        {
            if (descripcion.Length == 0)
            {
                descripcion = operacion.Tipo == TipoMovimiento.Ingreso ? "Ingreso registrado desde Telegram" : "Gasto registrado desde Telegram";
            }
            else
            {
                string categoria;
                ClasificadorSobres.Clasificar(InterpretadorMensaje.Normalizar(descripcion), out categoria);

                if (categoria.Length > 0)
                {
                    operacion.Categoria = char.ToUpper(categoria[0]) + categoria.Substring(1);
                }
            }

            operacion.Descripcion = descripcion;

            if (operacion.Asignaciones.Count == 0)
            {
                operacion.Asignaciones = AsignacionPorDefecto(operacion, operacion.Monto);
            }

            ActualizarMontoAsignacion(operacion);
            MostrarPropuesta(chatId, operacion, mensajeId);
        }

        private void MostrarPropuesta(long chatId, OperacionPendiente operacion, int mensajeId)
        {
            operacion.Estado = EstadoConversacion.Confirmando;
            operacion.Creada = DateTime.Now;
            Mostrar(chatId, mensajeId, RespuestasBot.Propuesta(operacion), TecladoConfirmacion(operacion));
        }

        /// <summary>
        /// Atajo: el usuario escribio el movimiento en lenguaje natural.
        /// </summary>
        private void Proponer(long chatId, Interpretacion interpretacion)
        {
            OperacionPendiente operacion = new OperacionPendiente();
            operacion.Tipo = interpretacion.Tipo == TipoInterpretacion.Ingreso ? TipoMovimiento.Ingreso : TipoMovimiento.Gasto;
            operacion.Categoria = interpretacion.Categoria;
            operacion.Descripcion = interpretacion.Descripcion;
            operacion.EsSueldo = operacion.Tipo == TipoMovimiento.Ingreso && interpretacion.EsSueldo;
            operacion.Monto = interpretacion.Monto;
            pendientes[chatId] = operacion;

            if (!operacion.EsSueldo)
            {
                Sobre sobre = ElegirSobre(servicios.Sobres.Listar(), interpretacion.SobreSugerido, interpretacion.Descripcion);
                operacion.Asignaciones = new List<AsignacionSobre>();
                operacion.Asignaciones.Add(CrearAsignacion(sobre, interpretacion.Monto));
            }

            if (interpretacion.Monto <= 0)
            {
                // Falta el monto: se pregunta con botones en lugar de mostrar un error.
                PedirMonto(chatId, operacion, 0);
                return;
            }

            AplicarMonto(chatId, operacion, interpretacion.Monto, 0);
        }

        // ---------- Botones ----------

        private void ProcesarCallback(ActualizacionTelegram actualizacion)
        {
            string dato = actualizacion.DatosCallback == null ? "" : actualizacion.DatosCallback;
            long chatId = actualizacion.ChatId;
            int mensajeId = actualizacion.MensajeId;

            if (dato.StartsWith("menu:", StringComparison.Ordinal))
            {
                cliente.ResponderCallback(actualizacion.CallbackId, "");
                ProcesarMenu(chatId, dato);
                return;
            }

            if (dato == DatoCancelar)
            {
                pendientes.Remove(chatId);
                cliente.ResponderCallback(actualizacion.CallbackId, "Cancelado");
                Mostrar(chatId, mensajeId, RespuestasBot.Cancelada(), TecladoAccionesRapidas());
                return;
            }

            OperacionPendiente operacion = ObtenerVigente(chatId);

            if (operacion == null)
            {
                cliente.ResponderCallback(actualizacion.CallbackId, "La operación expiró");
                Mostrar(chatId, mensajeId, RespuestasBot.Expirada(), TecladoAccionesRapidas());
                return;
            }

            cliente.ResponderCallback(actualizacion.CallbackId, "");

            if (dato == DatoIngresoSueldo || dato == DatoIngresoOtro)
            {
                operacion.EsSueldo = dato == DatoIngresoSueldo;
                PedirMonto(chatId, operacion, mensajeId);
                return;
            }

            if (dato.StartsWith(PrefijoSobre, StringComparison.Ordinal))
            {
                ElegirSobrePorBoton(chatId, operacion, dato.Substring(PrefijoSobre.Length), mensajeId);
                return;
            }

            if (dato == DatoMontoOtro)
            {
                operacion.Estado = EstadoConversacion.EsperandoMonto;
                Mostrar(chatId, mensajeId, "✏️ Escribe el monto en el chat (ej: 45000 o 45 mil).", TecladoTelegram.Inline().Boton("✖ Cancelar", DatoCancelar));
                return;
            }

            if (dato.StartsWith(PrefijoMonto, StringComparison.Ordinal))
            {
                decimal monto;

                if (decimal.TryParse(dato.Substring(PrefijoMonto.Length), out monto) && monto > 0)
                {
                    AplicarMonto(chatId, operacion, monto, mensajeId);
                }

                return;
            }

            if (dato == DatoSinDescripcion)
            {
                AplicarDescripcion(chatId, operacion, "", mensajeId);
                return;
            }

            if (dato == DatoCambiarSobre)
            {
                operacion.Estado = EstadoConversacion.EligiendoSobre;
                Mostrar(chatId, mensajeId, RespuestasBot.PreguntaSobre(operacion), TecladoSobres(PrefijoSobre, true));
                return;
            }

            if (dato == DatoConfirmar + ":ver")
            {
                MostrarPropuesta(chatId, operacion, mensajeId);
                return;
            }

            if (dato == DatoConfirmar)
            {
                Confirmar(chatId, operacion, mensajeId);
                return;
            }
        }

        private void ProcesarMenu(long chatId, string dato)
        {
            pendientes.Remove(chatId);

            if (dato == DatoMenuResumen)
            {
                MostrarResumen(chatId);
            }
            else if (dato == DatoMenuSobres)
            {
                MostrarSobres(chatId);
            }
            else if (dato == DatoMenuAlertas)
            {
                MostrarAlertas(chatId);
            }
            else if (dato == DatoMenuGasto)
            {
                IniciarGasto(chatId, 0);
            }
            else if (dato == DatoMenuIngreso)
            {
                IniciarIngreso(chatId, 0);
            }
        }

        private void ElegirSobrePorBoton(long chatId, OperacionPendiente operacion, string textoId, int mensajeId)
        {
            int sobreId;

            if (!int.TryParse(textoId, out sobreId))
            {
                return;
            }

            Sobre elegido = null;

            foreach (Sobre sobre in servicios.Sobres.Listar())
            {
                if (sobre.Id == sobreId && sobre.Activo)
                {
                    elegido = sobre;
                }
            }

            if (elegido == null)
            {
                Mostrar(chatId, mensajeId, "Ese sobre ya no está disponible. Elige otro.", TecladoSobres(PrefijoSobre, false));
                return;
            }

            operacion.SobreElegidoId = elegido.Id;
            operacion.Asignaciones = new List<AsignacionSobre>();
            operacion.Asignaciones.Add(CrearAsignacion(elegido, operacion.Monto));

            if (operacion.Tipo == TipoMovimiento.Gasto && (operacion.Categoria.Length == 0 || operacion.Categoria == "General"))
            {
                operacion.Categoria = elegido.Nombre;
            }

            if (operacion.Monto > 0 && operacion.Descripcion.Length > 0)
            {
                MostrarPropuesta(chatId, operacion, mensajeId);
                return;
            }

            PedirMonto(chatId, operacion, mensajeId);
        }

        private void Confirmar(long chatId, OperacionPendiente operacion, int mensajeId)
        {
            pendientes.Remove(chatId);

            List<AsignacionSobre> guardadas = new List<AsignacionSobre>();
            string errores = "";

            foreach (AsignacionSobre asignacion in operacion.Asignaciones)
            {
                Resultado resultado = GuardarMovimiento(operacion, asignacion);

                if (resultado.Exito)
                {
                    guardadas.Add(asignacion);
                }
                else
                {
                    errores = errores + "• " + asignacion.SobreNombre + ": " + resultado.Mensaje + "\n";
                }
            }

            if (guardadas.Count == 0)
            {
                Mostrar(chatId, mensajeId, "No se pudo registrar el movimiento.\n" + errores, TecladoAccionesRapidas());
                return;
            }

            string texto = RespuestasBot.Confirmada(guardadas);

            if (errores.Length > 0)
            {
                texto = texto + "\nNo se pudieron guardar:\n" + errores;
            }

            Mostrar(chatId, mensajeId, texto, TecladoAccionesRapidas());
        }

        // ---------- Apoyo ----------

        /// <summary>
        /// Edita el mensaje del boton tocado; si no hay mensaje o Telegram no lo permite, envia uno nuevo.
        /// </summary>
        private void Mostrar(long chatId, int mensajeId, string texto, TecladoTelegram teclado)
        {
            if (mensajeId > 0)
            {
                try
                {
                    cliente.EditarMensaje(chatId, mensajeId, texto, teclado);
                    return;
                }
                catch (Exception)
                {
                    // Se envia como mensaje nuevo.
                }
            }

            cliente.EnviarMensaje(chatId, texto, teclado);
        }

        private OperacionPendiente ObtenerVigente(long chatId)
        {
            OperacionPendiente operacion;

            if (!pendientes.TryGetValue(chatId, out operacion))
            {
                return null;
            }

            if (Vencida(operacion))
            {
                pendientes.Remove(chatId);
                return null;
            }

            return operacion;
        }

        private bool HaySobresActivos()
        {
            foreach (Sobre sobre in servicios.Sobres.Listar())
            {
                if (sobre.Activo)
                {
                    return true;
                }
            }

            return false;
        }

        private List<AsignacionSobre> AsignacionPorDefecto(OperacionPendiente operacion, decimal monto)
        {
            TipoSobre tipo = operacion.Tipo == TipoMovimiento.Ingreso ? TipoSobre.Ahorro : TipoSobre.Necesidades;
            string categoria;
            string texto = InterpretadorMensaje.Normalizar(operacion.Descripcion);

            if (texto.Length > 0)
            {
                tipo = ClasificadorSobres.Clasificar(texto, out categoria);
            }

            List<AsignacionSobre> asignaciones = new List<AsignacionSobre>();
            asignaciones.Add(CrearAsignacion(ElegirSobre(servicios.Sobres.Listar(), tipo, operacion.Descripcion), monto));
            return asignaciones;
        }

        private static void ActualizarMontoAsignacion(OperacionPendiente operacion)
        {
            if (operacion.Asignaciones.Count == 1)
            {
                operacion.Asignaciones[0].Monto = operacion.Monto;
            }
        }

        private List<AsignacionSobre> ConstruirDistribucion(decimal monto)
        {
            List<Sobre> activos = new List<Sobre>();
            decimal totalPorcentaje = 0;
            List<AsignacionSobre> asignaciones = new List<AsignacionSobre>();

            foreach (Sobre sobre in servicios.Sobres.Listar())
            {
                if (sobre.Activo && sobre.PorcentajeObjetivo > 0)
                {
                    activos.Add(sobre);
                    totalPorcentaje = totalPorcentaje + sobre.PorcentajeObjetivo;
                }
            }

            if (activos.Count == 0 || totalPorcentaje <= 0)
            {
                return asignaciones;
            }

            decimal acumulado = 0;

            for (int posicion = 0; posicion < activos.Count; posicion = posicion + 1)
            {
                Sobre sobre = activos[posicion];
                decimal parte;

                if (posicion == activos.Count - 1)
                {
                    parte = monto - acumulado;
                }
                else
                {
                    parte = decimal.Round(monto * sobre.PorcentajeObjetivo / totalPorcentaje, 2, MidpointRounding.AwayFromZero);
                    acumulado = acumulado + parte;
                }

                if (parte <= 0)
                {
                    continue;
                }

                AsignacionSobre asignacion = CrearAsignacion(sobre, parte);
                asignacion.Porcentaje = sobre.PorcentajeObjetivo;
                asignaciones.Add(asignacion);
            }

            return asignaciones;
        }

        private Sobre ElegirSobre(List<Sobre> sobres, TipoSobre tipo, string descripcion)
        {
            string texto = InterpretadorMensaje.Normalizar(descripcion);

            foreach (Sobre sobre in sobres)
            {
                if (!sobre.Activo || sobre.Nombre.Length == 0)
                {
                    continue;
                }

                string nombre = InterpretadorMensaje.Normalizar(sobre.Nombre);

                if (InterpretadorMensaje.ContieneFrase(texto, nombre))
                {
                    return sobre;
                }
            }

            foreach (Sobre sobre in sobres)
            {
                if (sobre.Activo && sobre.Tipo == tipo)
                {
                    return sobre;
                }
            }

            foreach (Sobre sobre in sobres)
            {
                if (sobre.Activo)
                {
                    return sobre;
                }
            }

            return null;
        }

        private Resultado GuardarMovimiento(OperacionPendiente operacion, AsignacionSobre asignacion)
        {
            Movimiento movimiento = new Movimiento();
            movimiento.Tipo = operacion.Tipo;
            movimiento.Categoria = operacion.Categoria;
            movimiento.Descripcion = operacion.Descripcion;
            movimiento.Monto = asignacion.Monto;
            movimiento.SobreId = asignacion.SobreId;
            movimiento.Fecha = DateTime.Today;

            return servicios.Movimientos.Guardar(movimiento);
        }

        private static bool Vencida(OperacionPendiente operacion)
        {
            TimeSpan transcurrido = DateTime.Now - operacion.Creada;
            return transcurrido.TotalMinutes > MinutosVigencia;
        }

        private static AsignacionSobre CrearAsignacion(Sobre sobre, decimal monto)
        {
            AsignacionSobre asignacion = new AsignacionSobre();
            asignacion.Monto = monto;

            if (sobre != null)
            {
                asignacion.SobreId = sobre.Id;
                asignacion.SobreNombre = sobre.Nombre;
                asignacion.Porcentaje = sobre.PorcentajeObjetivo;
            }

            return asignacion;
        }

        public static string MontoCorto(decimal monto)
        {
            if (monto >= 1000000)
            {
                return "$" + (monto / 1000000).ToString("0.#") + " M";
            }

            if (monto >= 1000)
            {
                return "$" + (monto / 1000).ToString("0") + " mil";
            }

            return "$" + monto.ToString("0");
        }
    }
}
