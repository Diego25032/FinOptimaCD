using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ClosedXML.Excel;
using FinOptima.Datos.Reportes;
using FinOptima.Bot;
using FinOptima.Entidades.Consultas;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Catalogos;
using FinOptima.Logica.Modelos;
using FinOptima.Logica.Seguridad;
using FinOptima.Logica.Servicios;
using FinOptima.Logica.Validaciones;

namespace FinOptima.Pruebas
{
    public static class Program
    {
        private static int aprobadas = 0;
        private static int fallidas = 0;

        public static int Main(string[] argumentos)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("==============================================");
            Console.WriteLine(" PRUEBAS DE FINOPTIMA - CAPA LOGICA");
            Console.WriteLine("==============================================");

            ProbarSeguridad();
            ProbarPoliticas();
            ProbarCatalogo();
            ProbarBot();
            ProbarAutenticacion();
            ProbarServiciosFinancieros();
            ProbarSimuladores();
            ProbarAdministracion();
            ProbarReportesReales();

            Console.WriteLine();
            Console.WriteLine("==============================================");
            Console.WriteLine(" Aprobadas: " + aprobadas);
            Console.WriteLine(" Fallidas:  " + fallidas);
            Console.WriteLine("==============================================");

            if (fallidas > 0)
            {
                return 1;
            }

            return 0;
        }

        private static void Verificar(string descripcion, bool condicion)
        {
            if (condicion)
            {
                aprobadas = aprobadas + 1;
                Console.WriteLine("[OK]    " + descripcion);
            }
            else
            {
                fallidas = fallidas + 1;
                Console.WriteLine("[FALLA] " + descripcion);
            }
        }

        private static void ProbarSeguridad()
        {
            Console.WriteLine();
            Console.WriteLine("-- Hash de contrasenas --");

            string sal = HashContrasena.CrearSal();
            string hash = HashContrasena.Calcular("Clave1234", sal);

            Verificar("La sal tiene el tamano esperado en Base64", sal.Length > 0);
            Verificar("La contrasena correcta se verifica", HashContrasena.Verificar("Clave1234", sal, hash));
            Verificar("Una contrasena incorrecta no se verifica", !HashContrasena.Verificar("Clave1235", sal, hash));

            string otraSal = HashContrasena.CrearSal();
            string otroHash = HashContrasena.Calcular("Clave1234", otraSal);

            Verificar("La misma contrasena con otra sal produce otro hash", hash != otroHash);

            string hashMalo = HashContrasena.Calcular("OtraClave1", sal);
            Verificar("Un hash de otra contrasena no coincide", !HashContrasena.Verificar("Clave1234", sal, hashMalo));
        }

        private static void ProbarPoliticas()
        {
            Console.WriteLine();
            Console.WriteLine("-- Politicas de contrasena y bloqueo --");

            Verificar("Rechaza contrasena corta", !PoliticaContrasena.Validar("Abc12").EsValido);
            Verificar("Rechaza contrasena sin numeros", !PoliticaContrasena.Validar("abcdefgh").EsValido);
            Verificar("Rechaza contrasena con espacios", !PoliticaContrasena.Validar("Clave 123").EsValido);
            Verificar("Acepta una contrasena valida", PoliticaContrasena.Validar("Clave1234").EsValido);

            DateTime ahora = new DateTime(2026, 1, 10, 9, 0, 0);
            Verificar("Cuatro intentos no bloquean la cuenta", PoliticaBloqueo.CalcularBloqueo(4, ahora) == DateTime.MinValue);

            DateTime bloqueo = PoliticaBloqueo.CalcularBloqueo(5, ahora);
            Verificar("Cinco intentos bloquean la cuenta", bloqueo == ahora.AddMinutes(15));
            Verificar("La cuenta bloqueada se detecta", PoliticaBloqueo.EstaBloqueada(bloqueo, ahora));
            Verificar("Quedan 15 minutos de bloqueo", PoliticaBloqueo.MinutosRestantes(bloqueo, ahora) == 15);
            Verificar("Un bloqueo vencido ya no aplica", !PoliticaBloqueo.EstaBloqueada(ahora.AddMinutes(-1), ahora));
        }

        private static void ProbarCatalogo()
        {
            Console.WriteLine();
            Console.WriteLine("-- Sobres predeterminados --");

            List<Sobre> sobres = CatalogoSobresPredeterminados.Crear(1);
            decimal sumaObjetivos = 0;
            decimal sumaMinimos = 0;
            decimal sumaMaximos = 0;

            foreach (Sobre sobre in sobres)
            {
                sumaObjetivos = sumaObjetivos + sobre.PorcentajeObjetivo;
                sumaMinimos = sumaMinimos + sobre.PorcentajeMinimo;
                sumaMaximos = sumaMaximos + sobre.PorcentajeMaximo;
            }

            Verificar("Se crean los 7 sobres del metodo", sobres.Count == 7);
            Verificar("Los objetivos suman 100%", sumaObjetivos == 100);
            Verificar("Los minimos no superan 100%", sumaMinimos <= 100);
            Verificar("Los maximos permiten llegar a 100%", sumaMaximos >= 100);
        }

        private static void ProbarBot()
        {
            Console.WriteLine();
            Console.WriteLine("-- Interpretador del bot de Telegram --");

            Interpretacion sueldo = InterpretadorMensaje.Interpretar("Me llego el sueldo de 3.500.000");
            Verificar("El bot detecta un ingreso de sueldo", sueldo.Tipo == TipoInterpretacion.Ingreso && sueldo.EsSueldo);
            Verificar("El bot lee el monto del sueldo", sueldo.Monto == 3500000m);
            Verificar("El bot marca la categoria del sueldo", sueldo.Categoria == "Sueldo");

            Interpretacion miles = InterpretadorMensaje.Interpretar("Gaste 50 mil en transporte");
            Verificar("El bot detecta un gasto", miles.Tipo == TipoInterpretacion.Gasto);
            Verificar("El bot entiende los miles", miles.Monto == 50000m);
            Verificar("El bot ubica el gasto en Necesidades", miles.SobreSugerido == TipoSobre.Necesidades);

            Interpretacion millones = InterpretadorMensaje.Interpretar("Me llego un ingreso de 3,5 millones por una venta");
            Verificar("El bot entiende los millones", millones.Monto == 3500000m);
            Verificar("El bot clasifica la venta como ingreso", millones.Tipo == TipoInterpretacion.Ingreso && !millones.EsSueldo);

            Interpretacion comando = InterpretadorMensaje.Interpretar("/gasto 80000 cine");
            Verificar("El comando /gasto se interpreta", comando.Tipo == TipoInterpretacion.Gasto && comando.Monto == 80000m);
            Verificar("El bot clasifica el cine como Ocio", comando.SobreSugerido == TipoSobre.Ocio);
            Verificar("El bot limpia la descripcion", comando.Descripcion == "cine");

            Interpretacion ayuda = InterpretadorMensaje.Interpretar("/ayuda");
            Verificar("El comando /ayuda responde con ayuda", ayuda.Tipo == TipoInterpretacion.Ayuda);

            Interpretacion saldo = InterpretadorMensaje.Interpretar("/saldo@FinOptimaBot");
            Verificar("Los comandos con @bot se reconocen", saldo.Tipo == TipoInterpretacion.Saldo);

            Interpretacion desconocido = InterpretadorMensaje.Interpretar("que tal el clima");
            Verificar("Un mensaje irreconocible no se registra", desconocido.Tipo == TipoInterpretacion.Desconocido);

            Verificar("El boton Registrar gasto abre el flujo de gasto", InterpretadorMensaje.Interpretar(RespuestasBot.BotonGasto).Tipo == TipoInterpretacion.IniciarGasto);
            Verificar("El boton Registrar ingreso abre el flujo de ingreso", InterpretadorMensaje.Interpretar(RespuestasBot.BotonIngreso).Tipo == TipoInterpretacion.IniciarIngreso);
            Verificar("El boton Resumen muestra el saldo", InterpretadorMensaje.Interpretar(RespuestasBot.BotonResumen).Tipo == TipoInterpretacion.Saldo);
            Verificar("El boton Sobres muestra los sobres", InterpretadorMensaje.Interpretar(RespuestasBot.BotonSobres).Tipo == TipoInterpretacion.Sobres);
            Verificar("El boton Alertas muestra las alertas", InterpretadorMensaje.Interpretar(RespuestasBot.BotonAlertas).Tipo == TipoInterpretacion.Alertas);
            Verificar("El boton Ayuda muestra el menu", InterpretadorMensaje.Interpretar(RespuestasBot.BotonAyuda).Tipo == TipoInterpretacion.Ayuda);
            Verificar("Saludar abre el menu", InterpretadorMensaje.Interpretar("Hola").Tipo == TipoInterpretacion.Ayuda);
            Verificar("Una frase con la palabra sobres no se toma como boton", InterpretadorMensaje.Interpretar("gaste 5000 en sobres de carta").Tipo == TipoInterpretacion.Gasto);
            Verificar("El monto escrito se lee con puntos de miles", InterpretadorMensaje.ExtraerMonto("45.000") == 45000m);
            Verificar("El monto escrito se lee con la palabra mil", InterpretadorMensaje.ExtraerMonto("45 mil") == 45000m);
            Verificar("Un texto sin numeros no tiene monto", InterpretadorMensaje.ExtraerMonto("almuerzo") == 0m);

            string tecladoFijo = ManejadorBot.TecladoPrincipal().ToJson();
            Verificar("El teclado principal es fijo y redimensionable", tecladoFijo.StartsWith("{\"keyboard\":") && tecladoFijo.Contains("\"is_persistent\":true"));
            Verificar("El teclado principal tiene los 6 botones", ManejadorBot.TecladoPrincipal().CantidadBotones == 6);

            string tecladoInline = TecladoTelegram.Inline().Boton("✅ Confirmar", "confirmar").Boton("✖ Cancelar", "cancelar").ToJson();
            Verificar("Los botones del mensaje envian un dato al bot", tecladoInline == "{\"inline_keyboard\":[[{\"text\":\"✅ Confirmar\",\"callback_data\":\"confirmar\"},{\"text\":\"✖ Cancelar\",\"callback_data\":\"cancelar\"}]]}");
            Verificar("Los montos rapidos en miles se muestran abreviados", ManejadorBot.MontoCorto(50000m) == "$50 mil");
            string millon = ManejadorBot.MontoCorto(1500000m);
            Verificar("Los montos rapidos en millones se muestran abreviados", millon == "$1.5 M" || millon == "$1,5 M");

            Verificar("La frase de sobre se reconoce", InterpretadorMensaje.ContieneFrase("en educacion", "educacion"));
            Verificar("La frase de sobre no coincide parcialmente", !InterpretadorMensaje.ContieneFrase("en educaciones", "educacion"));

            OperacionPendiente operacion = new OperacionPendiente();
            operacion.Tipo = TipoMovimiento.Ingreso;
            operacion.Categoria = "Sueldo";
            operacion.Descripcion = "Sueldo de prueba";

            AsignacionSobre asignacion = new AsignacionSobre();
            asignacion.SobreNombre = "Necesidades";
            asignacion.Monto = 1000;
            asignacion.Porcentaje = 50;
            operacion.Asignaciones.Add(asignacion);

            string propuesta = RespuestasBot.Propuesta(operacion);
            Verificar("La propuesta incluye el sobre", propuesta.Contains("Necesidades"));
            Verificar("La propuesta pide confirmacion", propuesta.Contains("¿Confirmas el registro?"));

            List<Alerta> alertasVacias = new List<Alerta>();
            Verificar("Sin alertas el bot responde que todo esta en orden", RespuestasBot.ListaAlertas(alertasVacias).Contains("Todo en orden"));
        }

        private static void ProbarReportesReales()
        {
            Console.WriteLine();
            Console.WriteLine("-- Reportes reales de Excel y PDF --");

            DatosReporte datos = new DatosReporte();
            datos.NombreUsuario = "Persona de prueba";

            Sobre sobre = new Sobre();
            sobre.Id = 1;
            sobre.Nombre = "Necesidades";
            sobre.PorcentajeObjetivo = 55;
            sobre.Saldo = 1250000;
            datos.Sobres.Add(sobre);

            Movimiento ingreso = new Movimiento();
            ingreso.Tipo = TipoMovimiento.Ingreso;
            ingreso.Categoria = "Sueldo";
            ingreso.Descripcion = "Sueldo";
            ingreso.Monto = 3500000;
            ingreso.SobreId = 1;
            datos.Movimientos.Add(ingreso);

            Movimiento gasto = new Movimiento();
            gasto.Tipo = TipoMovimiento.Gasto;
            gasto.Categoria = "Mercado";
            gasto.Descripcion = "Compra de mercado";
            gasto.Monto = 450000;
            gasto.SobreId = 1;
            datos.Movimientos.Add(gasto);

            string carpeta = Path.Combine(Path.GetTempPath(), "finoptima-pruebas");
            Directory.CreateDirectory(carpeta);
            string rutaExcel = Path.Combine(carpeta, "reporte.xlsx");
            string rutaPdf = Path.Combine(carpeta, "reporte.pdf");

            GeneradorReportes generador = new GeneradorReportes();
            bool excelCorrecto = true;

            try
            {
                generador.ExportarExcel(rutaExcel, datos);
            }
            catch (Exception error)
            {
                Console.WriteLine("        " + error.Message);
                excelCorrecto = false;
            }

            Verificar("El Excel real se genera sin errores", excelCorrecto && File.Exists(rutaExcel));

            if (excelCorrecto)
            {
                using (XLWorkbook libro = new XLWorkbook(rutaExcel))
                {
                    IXLWorksheet resumen = libro.Worksheet("Resumen");
                    IXLWorksheet movimientos = libro.Worksheet("Movimientos");
                    Verificar("El Excel trae la hoja de resumen con el nombre de la cuenta", resumen.Cell(3, 2).GetString().Contains("Persona de prueba"));
                    Verificar("Los ingresos del resumen suman 3.500.000", resumen.Cell(6, 3).GetValue<decimal>() == 3500000m);
                    Verificar("Las fechas del Excel usan dia/mes/ano", movimientos.Cell(2, 1).Style.NumberFormat.Format == "dd/mm/yyyy");
                    Verificar("La columna de ingresos es lo bastante ancha para millones", movimientos.Column(6).Width >= 14);
                    Verificar("Los movimientos quedan en una tabla con filtros", movimientos.Tables.Contains("TablaMovimientos"));
                }
            }

            bool pdfCorrecto = true;

            try
            {
                generador.ExportarPdf(rutaPdf, datos);
            }
            catch (Exception error)
            {
                Console.WriteLine("        " + error.Message);
                pdfCorrecto = false;
            }

            Verificar("El PDF real se genera sin errores", pdfCorrecto && File.Exists(rutaPdf));

            if (pdfCorrecto)
            {
                byte[] inicio = new byte[5];

                using (FileStream archivo = File.OpenRead(rutaPdf))
                {
                    archivo.ReadExactly(inicio, 0, 5);
                }

                Verificar("El archivo generado es un PDF de verdad (no HTML)", Encoding.ASCII.GetString(inicio) == "%PDF-");
            }
        }

        private static void ProbarAutenticacion()
        {
            Console.WriteLine();
            Console.WriteLine("-- Autenticacion y bloqueo --");

            RepositorioUsuariosFalso usuarios = new RepositorioUsuariosFalso();
            RepositorioSobresFalso sobres = new RepositorioSobresFalso();
            RepositorioActividadFalso actividad = new RepositorioActividadFalso();
            SesionActual sesion = new SesionActual();
            ServicioAutenticacion autenticacion = new ServicioAutenticacion(usuarios, sobres, actividad, sesion);

            Resultado primero = autenticacion.Registrar("Primera Persona", "primera@finoptima.com", "Clave1234", "Clave1234");
            Verificar("El primer registro se completa", primero.Exito);

            Usuario primera = usuarios.BuscarPorCorreo("primera@finoptima.com");
            Verificar("Las cuentas creadas son usuarios", primera.Rol == RolUsuario.Usuario);
            Verificar("La cuenta nueva recibe 7 sobres", sobres.ListarPorUsuario(primera.Id).Count == 7);

            Resultado segundo = autenticacion.Registrar("Usuario Secundario", "usuario@finoptima.com", "Clave1234", "Clave1234");
            Verificar("El segundo registro se completa", segundo.Exito);

            Usuario normal = usuarios.BuscarPorCorreo("usuario@finoptima.com");
            Verificar("El segundo registro tambien es usuario", normal.Rol == RolUsuario.Usuario);
            Verificar("El segundo registro recibe 7 sobres", sobres.ListarPorUsuario(normal.Id).Count == 7);

            Resultado duplicado = autenticacion.Registrar("Otra Persona", "primera@finoptima.com", "Clave1234", "Clave1234");
            Verificar("No se permite un correo repetido", !duplicado.Exito);

            ResultadoLogin correcto = autenticacion.IniciarSesion("usuario@finoptima.com", "Clave1234");
            Verificar("El inicio de sesion correcto funciona", correcto.Exito && sesion.HaySesion);

            for (int intento = 0; intento < 5; intento = intento + 1)
            {
                autenticacion.IniciarSesion("usuario@finoptima.com", "Mala1234");
            }

            Verificar("Cinco fallos bloquean la cuenta", usuarios.BuscarPorCorreo("usuario@finoptima.com").BloqueadoHasta > DateTime.Now);

            ResultadoLogin bloqueado = autenticacion.IniciarSesion("usuario@finoptima.com", "Clave1234");
            Verificar("Una cuenta bloqueada no puede entrar", !bloqueado.Exito && bloqueado.Mensaje.Contains("bloqueada"));
        }

        private static void ProbarServiciosFinancieros()
        {
            Console.WriteLine();
            Console.WriteLine("-- Servicios financieros --");

            RepositorioUsuariosFalso usuarios = new RepositorioUsuariosFalso();
            RepositorioSobresFalso sobres = new RepositorioSobresFalso();
            RepositorioMovimientosFalso movimientos = new RepositorioMovimientosFalso();
            RepositorioMetasFalso metas = new RepositorioMetasFalso();
            RepositorioDeudasFalso deudas = new RepositorioDeudasFalso();
            RepositorioPresupuestosFalso presupuestos = new RepositorioPresupuestosFalso();
            RepositorioActividadFalso actividad = new RepositorioActividadFalso();
            SesionActual sesion = new SesionActual();

            ServicioAutenticacion autenticacion = new ServicioAutenticacion(usuarios, sobres, actividad, sesion);
            autenticacion.Registrar("Administrador Principal", "admin@finoptima.com", "Clave1234", "Clave1234");
            autenticacion.Registrar("Usuario Secundario", "usuario@finoptima.com", "Clave1234", "Clave1234");
            autenticacion.IniciarSesion("admin@finoptima.com", "Clave1234");

            ServicioSobres servicioSobres = new ServicioSobres(sobres, sesion);
            ServicioMovimientos servicioMovimientos = new ServicioMovimientos(movimientos, sobres, sesion);
            ServicioMetas servicioMetas = new ServicioMetas(metas, sesion);
            ServicioDeudas servicioDeudas = new ServicioDeudas(deudas, sesion);
            ServicioPresupuestos servicioPresupuestos = new ServicioPresupuestos(presupuestos, movimientos, sesion);
            ServicioAlertas servicioAlertas = new ServicioAlertas(metas, deudas, movimientos, sobres, servicioPresupuestos, sesion);
            ServicioDashboard servicioDashboard = new ServicioDashboard(movimientos, sobres, metas, deudas, servicioAlertas, sesion);
            ServicioAnalisis servicioAnalisis = new ServicioAnalisis(movimientos, sesion);
            ServicioPerfil servicioPerfil = new ServicioPerfil(usuarios, sesion);

            List<Sobre> listaSobres = servicioSobres.Listar();
            Verificar("La cuenta ve sus 7 sobres", listaSobres.Count == 7);

            Sobre necesidades = BuscarSobrePorNombre(listaSobres, "Necesidades");
            Verificar("Existe el sobre Necesidades", necesidades != null);

            Movimiento gasto = new Movimiento();
            gasto.Tipo = TipoMovimiento.Gasto;
            gasto.Categoria = "Mercado";
            gasto.Descripcion = "Compra semanal";
            gasto.Monto = 100;
            gasto.SobreId = necesidades.Id;
            gasto.Fecha = DateTime.Today;

            Resultado guardarGasto = servicioMovimientos.Guardar(gasto);
            Verificar("Se registra un gasto", guardarGasto.Exito);
            Verificar("El gasto reduce el saldo del sobre", SaldoDeSobre(servicioSobres, necesidades.Id) == -100m);

            Movimiento ingreso = new Movimiento();
            ingreso.Tipo = TipoMovimiento.Ingreso;
            ingreso.Categoria = "Extra";
            ingreso.Descripcion = "Trabajo adicional";
            ingreso.Monto = 50;
            ingreso.SobreId = necesidades.Id;
            ingreso.Fecha = DateTime.Today;

            Resultado guardarIngreso = servicioMovimientos.Guardar(ingreso);
            Verificar("Se registra un ingreso", guardarIngreso.Exito);
            Verificar("El ingreso aumenta el saldo del sobre", SaldoDeSobre(servicioSobres, necesidades.Id) == -50m);

            Movimiento edicion = new Movimiento();
            edicion.Id = gasto.Id;
            edicion.Tipo = TipoMovimiento.Gasto;
            edicion.Categoria = "Mercado";
            edicion.Descripcion = "Compra semanal corregida";
            edicion.Monto = 30;
            edicion.SobreId = necesidades.Id;
            edicion.Fecha = DateTime.Today;

            Resultado editarGasto = servicioMovimientos.Guardar(edicion);
            Verificar("Se edita el gasto", editarGasto.Exito);
            Verificar("La edicion ajusta el saldo del sobre", SaldoDeSobre(servicioSobres, necesidades.Id) == 20m);

            servicioMovimientos.Eliminar(gasto.Id);
            Verificar("Eliminar el gasto devuelve el saldo", SaldoDeSobre(servicioSobres, necesidades.Id) == 50m);

            servicioMovimientos.Eliminar(ingreso.Id);
            Verificar("Eliminar el ingreso devuelve el saldo inicial", SaldoDeSobre(servicioSobres, necesidades.Id) == 0m);

            Movimiento ingresoMes = new Movimiento();
            ingresoMes.Tipo = TipoMovimiento.Ingreso;
            ingresoMes.Categoria = "Salario";
            ingresoMes.Descripcion = "Pago mensual";
            ingresoMes.Monto = 1000;
            ingresoMes.SobreId = 0;
            ingresoMes.Fecha = DateTime.Today;
            servicioMovimientos.Guardar(ingresoMes);

            Movimiento gastoMes = new Movimiento();
            gastoMes.Tipo = TipoMovimiento.Gasto;
            gastoMes.Categoria = "Comida";
            gastoMes.Descripcion = "Supermercado";
            gastoMes.Monto = 200;
            gastoMes.SobreId = 0;
            gastoMes.Fecha = DateTime.Today;
            servicioMovimientos.Guardar(gastoMes);

            FiltroMovimientos filtroGastos = new FiltroMovimientos();
            filtroGastos.Tipo = TipoMovimiento.Gasto;
            filtroGastos.Anio = DateTime.Today.Year;
            filtroGastos.Mes = DateTime.Today.Month;
            Verificar("El filtro devuelve solo los gastos del mes", servicioMovimientos.Listar(filtroGastos).Count == 1);

            FiltroMovimientos filtroCategoria = new FiltroMovimientos();
            filtroCategoria.Categoria = "comi";
            Verificar("El filtro por categoria ignora mayusculas", servicioMovimientos.Listar(filtroCategoria).Count == 1);

            FiltroMovimientos filtroTexto = new FiltroMovimientos();
            filtroTexto.Texto = "super";
            Verificar("El filtro por texto busca en la descripcion", servicioMovimientos.Listar(filtroTexto).Count == 1);

            int mesFiltroAnterior = DateTime.Today.Month == 1 ? 12 : DateTime.Today.Month - 1;
            int anioFiltroAnterior = DateTime.Today.Month == 1 ? DateTime.Today.Year - 1 : DateTime.Today.Year;
            FiltroMovimientos filtroOtroMes = new FiltroMovimientos();
            filtroOtroMes.Anio = anioFiltroAnterior;
            filtroOtroMes.Mes = mesFiltroAnterior;
            Verificar("El filtro por mes no trae movimientos de otros meses", servicioMovimientos.Listar(filtroOtroMes).Count == 0);

            FiltroMovimientos sinFiltros = new FiltroMovimientos();
            Verificar("Un filtro vacio devuelve todos los movimientos", servicioMovimientos.Listar(sinFiltros).Count == 2);

            Meta meta = new Meta();
            meta.Nombre = "Viaje";
            meta.MontoObjetivo = 1000;
            meta.MontoActual = 0;
            meta.FechaLimite = DateTime.Today.AddMonths(3);
            meta.Prioridad = PrioridadMeta.Alta;

            Resultado guardarMeta = servicioMetas.Guardar(meta);
            Verificar("Se crea una meta", guardarMeta.Exito);

            Resultado aporte = servicioMetas.Aportar(meta.Id, 600);
            Verificar("Se registra un aporte a la meta", aporte.Exito);
            Verificar("El total ahorrado refleja el aporte", servicioMetas.TotalAhorrado() == 600m);

            Resultado aporteExcedido = servicioMetas.Aportar(meta.Id, 500);
            Verificar("Un aporte mayor al faltante se ajusta al objetivo", aporteExcedido.Exito && servicioMetas.TotalAhorrado() == 1000m);

            MetaResumen resumenMeta = BuscarMeta(servicioMetas.Listar(), meta.Id);
            Verificar("La meta figura como cumplida", resumenMeta != null && resumenMeta.Estado == "Cumplida");

            Deuda deuda = new Deuda();
            deuda.Acreedor = "Banco Central";
            deuda.Capital = 500;
            deuda.Saldo = 500;
            deuda.TasaInteres = 35;
            deuda.PagoMinimo = 50;
            deuda.FechaVencimiento = DateTime.Today.AddDays(20);

            Resultado guardarDeuda = servicioDeudas.Guardar(deuda);
            Verificar("Se registra una deuda", guardarDeuda.Exito);

            Resultado abono = servicioDeudas.Abonar(deuda.Id, 200);
            Verificar("Se registra un abono", abono.Exito);
            Verificar("El abono reduce el saldo de la deuda", servicioDeudas.TotalSaldo() == 300m);

            Presupuesto presupuesto = new Presupuesto();
            presupuesto.Categoria = "Comida";
            presupuesto.Limite = 150;

            Resultado guardarPresupuesto = servicioPresupuestos.Guardar(presupuesto);
            Verificar("Se crea un presupuesto", guardarPresupuesto.Exito);

            Presupuesto presupuestoRepetido = new Presupuesto();
            presupuestoRepetido.Categoria = "comida";
            presupuestoRepetido.Limite = 150;
            servicioPresupuestos.Guardar(presupuestoRepetido);

            List<PresupuestoResumen> listaPresupuestos = servicioPresupuestos.Listar();
            Verificar("La categoria repetida actualiza el presupuesto", listaPresupuestos.Count == 1);

            PresupuestoResumen resumenPresupuesto = listaPresupuestos[0];
            Verificar("El presupuesto suma los gastos del mes", resumenPresupuesto.Gastado == 200m);
            Verificar("El presupuesto calcula el disponible", resumenPresupuesto.Disponible == -50m);
            Verificar("El presupuesto calcula el porcentaje usado", Math.Abs(resumenPresupuesto.Porcentaje - 133.33m) < 0.01m);

            int mesAnterior = DateTime.Today.Month == 1 ? 12 : DateTime.Today.Month - 1;
            int anioAnterior = DateTime.Today.Month == 1 ? DateTime.Today.Year - 1 : DateTime.Today.Year;

            Presupuesto presupuestoAnterior = new Presupuesto();
            presupuestoAnterior.Categoria = "Comida";
            presupuestoAnterior.Limite = 100;
            presupuestoAnterior.Anio = anioAnterior;
            presupuestoAnterior.Mes = mesAnterior;
            servicioPresupuestos.Guardar(presupuestoAnterior);

            List<PresupuestoResumen> historialAnterior = servicioPresupuestos.Listar(anioAnterior, mesAnterior);
            Verificar("El historial guarda presupuestos de otros meses", historialAnterior.Count == 1);
            Verificar("El presupuesto historico usa sus propios gastos", historialAnterior[0].Gastado == 0m);
            Verificar("El mes actual no mezcla presupuestos historicos", servicioPresupuestos.Listar().Count == 1);

            Presupuesto periodoInvalido = new Presupuesto();
            periodoInvalido.Categoria = "Viajes";
            periodoInvalido.Limite = 50;
            periodoInvalido.Mes = 13;
            Verificar("Se rechaza un mes invalido", !servicioPresupuestos.Guardar(periodoInvalido).Exito);

            ResumenDashboard tablero = servicioDashboard.Obtener();
            Verificar("El tablero suma los ingresos del mes", tablero.IngresosMes == 1000m);
            Verificar("El tablero suma los gastos del mes", tablero.GastosMes == 200m);
            Verificar("El tablero calcula el disponible", tablero.DisponibleMes == 800m);
            Verificar("El tablero suma el ahorro de metas", tablero.AhorroMetas == 1000m);
            Verificar("El tablero suma las deudas pendientes", tablero.SaldoDeudas == 300m);

            List<Alerta> alertas = servicioAlertas.Listar();
            Verificar("Se generan alertas con los datos del ejemplo", alertas.Count > 0);
            Verificar("Se alerta un presupuesto excedido", ContieneAlerta(alertas, "Presupuesto"));
            Verificar("Se alerta una deuda con interes alto", ContieneAlerta(alertas, "Deuda"));

            ResumenMensual resumenMensual = servicioAnalisis.ObtenerResumen(DateTime.Today.Year, DateTime.Today.Month);
            Verificar("El analisis suma ingresos del mes", resumenMensual.Ingresos == 1000m);
            Verificar("El analisis suma gastos del mes", resumenMensual.Gastos == 200m);
            Verificar("El analisis calcula el ahorro", resumenMensual.Ahorro == 800m);
            Verificar("El analisis calcula el porcentaje de ahorro", resumenMensual.PorcentajeAhorro == 80m);
            Verificar("El analisis devuelve seis meses", servicioAnalisis.ListarUltimosMeses(6).Count == 6);

            Resultado perfilGuardado = servicioPerfil.GuardarPerfil("Administrador Principal", "#FF8800", 11, true, false, true, true);
            Verificar("Se guarda el perfil y las preferencias", perfilGuardado.Exito && sesion.Usuario.TamanoFuente == 11);

            Resultado colorInvalido = servicioPerfil.GuardarPerfil("Administrador Principal", "naranja", 11, true, true, true, true);
            Verificar("Se rechaza un color de acento invalido", !colorInvalido.Exito);

            Resultado fuenteExcesiva = servicioPerfil.GuardarPerfil("Administrador Principal", "#FF8800", 15, true, true, true, true);
            Verificar("Se rechaza una fuente demasiado grande", !fuenteExcesiva.Exito);

            Resultado temaGuardado = servicioPerfil.GuardarTema(TemaVisual.Oscuro);
            Verificar("Se guarda el tema visual del perfil", temaGuardado.Exito && sesion.Usuario.Tema == TemaVisual.Oscuro);

            Resultado claveIncorrecta = servicioPerfil.CambiarContrasena("Mala1234", "Nueva1234", "Nueva1234");
            Verificar("Se rechaza el cambio con contrasena actual incorrecta", !claveIncorrecta.Exito);

            Resultado claveCambiada = servicioPerfil.CambiarContrasena("Clave1234", "Nueva1234", "Nueva1234");
            Verificar("Se cambia la contrasena", claveCambiada.Exito);

            ResultadoLogin nuevoIngreso = autenticacion.IniciarSesion("admin@finoptima.com", "Nueva1234");
            Verificar("Se inicia sesion con la contrasena nueva", nuevoIngreso.Exito);

            autenticacion.IniciarSesion("usuario@finoptima.com", "Clave1234");
            ServicioMovimientos movimientosUsuario = new ServicioMovimientos(movimientos, sobres, sesion);
            ServicioSobres sobresUsuario = new ServicioSobres(sobres, sesion);
            ServicioPresupuestos presupuestosUsuario = new ServicioPresupuestos(presupuestos, movimientos, sesion);

            Verificar("El usuario no ve movimientos de otra cuenta", movimientosUsuario.Listar().Count == 0);
            Verificar("El usuario no ve presupuestos de otra cuenta", presupuestosUsuario.Listar().Count == 0);
            Verificar("El usuario ve sus propios 7 sobres", sobresUsuario.Listar().Count == 7);

            List<Sobre> propios = sobresUsuario.Listar();

            while (propios.Count > 1)
            {
                sobresUsuario.Eliminar(propios[0].Id);
                propios = sobresUsuario.Listar();
            }

            Resultado eliminarUltimo = sobresUsuario.Eliminar(propios[0].Id);
            Verificar("No se puede eliminar el ultimo sobre", !eliminarUltimo.Exito);
        }

        private static void ProbarSimuladores()
        {
            Console.WriteLine();
            Console.WriteLine("-- Simuladores --");

            ServicioSimuladores simuladores = new ServicioSimuladores();

            Meta meta = new Meta();
            meta.Nombre = "Emergencias";
            meta.MontoObjetivo = 1000;
            meta.MontoActual = 0;
            meta.FechaLimite = DateTime.Today.AddMonths(6);

            ResultadoSimulacionAhorro simulacionAhorro = simuladores.SimularAhorro(meta, 250);
            Verificar("La simulacion de ahorro es posible", simulacionAhorro.Posible);
            Verificar("La simulacion de ahorro calcula 4 meses", simulacionAhorro.Meses == 4);
            Verificar("La simulacion usa la fecha de hoy", simulacionAhorro.FechaEstimada.Date == DateTime.Today.AddMonths(4));

            Meta metaCumplida = new Meta();
            metaCumplida.Nombre = "Lista";
            metaCumplida.MontoObjetivo = 100;
            metaCumplida.MontoActual = 100;

            ResultadoSimulacionAhorro yaCumplida = simuladores.SimularAhorro(metaCumplida, 50);
            Verificar("Una meta cumplida no requiere meses", yaCumplida.Posible && yaCumplida.Meses == 0);

            Deuda deuda = new Deuda();
            deuda.Acreedor = "Tienda";
            deuda.Saldo = 1200;
            deuda.TasaInteres = 0;
            deuda.FechaVencimiento = DateTime.Today.AddMonths(2);

            ResultadoSimulacionDeuda simulacionDeuda = simuladores.SimularPagoDeuda(deuda, 200);
            Verificar("La simulacion de deuda es posible", simulacionDeuda.Posible);
            Verificar("La simulacion de deuda calcula 6 meses", simulacionDeuda.Meses == 6);
            Verificar("La simulacion de deuda calcula el total pagado", simulacionDeuda.TotalPagado == 1200m);

            Deuda deudaCara = new Deuda();
            deudaCara.Acreedor = "Tarjeta";
            deudaCara.Saldo = 1000;
            deudaCara.TasaInteres = 60;
            deudaCara.FechaVencimiento = DateTime.Today.AddMonths(1);

            ResultadoSimulacionDeuda simulacionImposible = simuladores.SimularPagoDeuda(deudaCara, 10);
            Verificar("Un pago menor que los intereses no es viable", !simulacionImposible.Posible);
            Verificar("Se explica el problema de los intereses", simulacionImposible.Mensaje.Contains("intereses"));
        }

        private static void ProbarAdministracion()
        {
            Console.WriteLine();
            Console.WriteLine("-- Administracion, reportes y respaldos --");

            RepositorioUsuariosFalso usuarios = new RepositorioUsuariosFalso();
            RepositorioSobresFalso sobres = new RepositorioSobresFalso();
            RepositorioMovimientosFalso movimientos = new RepositorioMovimientosFalso();
            RepositorioMetasFalso metas = new RepositorioMetasFalso();
            RepositorioDeudasFalso deudas = new RepositorioDeudasFalso();
            RepositorioActividadFalso actividad = new RepositorioActividadFalso();
            RepositorioAuditoriaFalso auditoria = new RepositorioAuditoriaFalso(usuarios);
            ExportadorCuentasFalso exportador = new ExportadorCuentasFalso();
            GeneradorReportesFalso generador = new GeneradorReportesFalso();
            SesionActual sesion = new SesionActual();

            ServicioAutenticacion autenticacion = new ServicioAutenticacion(usuarios, sobres, actividad, sesion);
            autenticacion.Registrar("Administrador Principal", "admin@finoptima.com", "Clave1234", "Clave1234");
            autenticacion.Registrar("Usuario Secundario", "usuario@finoptima.com", "Clave1234", "Clave1234");

            // El rol de administrador se asigna en la base de datos; aqui se simula ese cambio.
            Usuario administrador = usuarios.BuscarPorCorreo("admin@finoptima.com");
            usuarios.ActualizarRolYEstado(administrador.Id, RolUsuario.Administrador, true);

            autenticacion.IniciarSesion("admin@finoptima.com", "Clave1234");

            ServicioAdministracion administracion = new ServicioAdministracion(usuarios, auditoria, exportador, sesion);
            Verificar("La cuenta activa es administradora", administracion.EsAdministrador());

            Usuario normal = usuarios.BuscarPorCorreo("usuario@finoptima.com");

            Resultado rolPropio = administracion.CambiarRol(sesion.UsuarioId, RolUsuario.Usuario);
            Verificar("Un administrador no puede cambiar su propio rol", !rolPropio.Exito);

            Resultado estadoPropio = administracion.CambiarEstado(sesion.UsuarioId, false);
            Verificar("Un administrador no puede desactivar su cuenta", !estadoPropio.Exito);

            Resultado promover = administracion.CambiarRol(normal.Id, RolUsuario.Administrador);
            Verificar("Se puede promover a otro usuario", promover.Exito);
            Verificar("El nuevo rol queda guardado", usuarios.BuscarPorId(normal.Id).Rol == RolUsuario.Administrador);
            Verificar("El cambio administrativo se audita", auditoria.Cantidad >= 1);

            ResumenAdministracion resumen = administracion.ObtenerResumen();
            Verificar("El resumen cuenta las cuentas", resumen.TotalCuentas == 2);
            Verificar("El resumen cuenta los administradores", resumen.Administradores == 2);
            Verificar("El resumen cuenta las cuentas activas", resumen.Activas == 2);

            Resultado exportacion = administracion.ExportarCuentas("cuentas.csv");
            Verificar("Se exportan las cuentas", exportacion.Exito && exportador.Cantidad == 2);

            ServicioReportes reportes = new ServicioReportes(generador, sobres, movimientos, metas, deudas, sesion);
            Resultado excel = reportes.ExportarExcel("reporte.xlsx");
            Verificar("Se genera el reporte de Excel", excel.Exito && generador.VecesExcel == 1);

            Resultado pdf = reportes.ExportarPdf("reporte.pdf");
            Verificar("Se genera el reporte PDF", pdf.Exito && generador.VecesPdf == 1);

            ServicioRespaldoFalso respaldoFalso = new ServicioRespaldoFalso();
            ServicioRespaldo respaldo = new ServicioRespaldo(respaldoFalso, actividad, sesion);
            ResumenRespaldo resumenRespaldo = respaldo.Exportar("copia.json");
            Verificar("Se exporta el respaldo", resumenRespaldo.Sobres == 7 && resumenRespaldo.Total == 11);
            Verificar("El respaldo usa la ruta indicada", respaldoFalso.RutaExportada == "copia.json");

            ResumenRespaldo resumenImportado = respaldo.Importar("copia.json");
            Verificar("Se importa el respaldo", resumenImportado.Movimientos == 2 && resumenImportado.Deudas == 1);
        }

        private static Sobre BuscarSobrePorNombre(List<Sobre> lista, string nombre)
        {
            foreach (Sobre sobre in lista)
            {
                if (sobre.Nombre == nombre)
                {
                    return sobre;
                }
            }

            return null;
        }

        private static decimal SaldoDeSobre(ServicioSobres servicioSobres, int sobreId)
        {
            foreach (Sobre sobre in servicioSobres.Listar())
            {
                if (sobre.Id == sobreId)
                {
                    return sobre.Saldo;
                }
            }

            return 0;
        }

        private static MetaResumen BuscarMeta(List<MetaResumen> lista, int metaId)
        {
            foreach (MetaResumen meta in lista)
            {
                if (meta.Id == metaId)
                {
                    return meta;
                }
            }

            return null;
        }

        private static bool ContieneAlerta(List<Alerta> alertas, string tipo)
        {
            foreach (Alerta alerta in alertas)
            {
                if (alerta.Tipo == tipo)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
