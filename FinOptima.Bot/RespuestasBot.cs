using System;
using System.Collections.Generic;
using System.Text;
using FinOptima.Entidades.Entidades;
using FinOptima.Entidades.Enums;
using FinOptima.Logica.Modelos;

namespace FinOptima.Bot
{
    public static class RespuestasBot
    {
        private const int MaximoAlertas = 10;

        public const string BotonGasto = "➖ Registrar gasto";
        public const string BotonIngreso = "➕ Registrar ingreso";
        public const string BotonResumen = "📊 Resumen";
        public const string BotonSobres = "✉️ Sobres";
        public const string BotonAlertas = "🔔 Alertas";
        public const string BotonAyuda = "❓ Ayuda";

        public static string Ayuda()
        {
            StringBuilder texto = new StringBuilder();
            texto.AppendLine("👋 Hola, soy tu asistente de FinOptima.");
            texto.AppendLine();
            texto.AppendLine("Usa los botones de abajo para todo:");
            texto.AppendLine("➖ Registrar gasto · ➕ Registrar ingreso");
            texto.AppendLine("📊 Resumen del mes · ✉️ Saldo de tus sobres · 🔔 Alertas");
            texto.AppendLine();
            texto.AppendLine("¿Prefieres escribir? También te entiendo:");
            texto.AppendLine("• Gasté 80000 en el cine");
            texto.AppendLine("• Pagué 45 mil de mercado");
            texto.AppendLine("• Me llegó el sueldo de 3.500.000");
            texto.AppendLine();
            texto.AppendLine("Antes de guardar siempre te pido confirmación.");
            return texto.ToString();
        }

        public static string AyudaCorta()
        {
            StringBuilder texto = new StringBuilder();
            texto.AppendLine("🤔 No entendí tu mensaje.");
            texto.AppendLine();
            texto.AppendLine("Toca un botón para empezar o escribe algo como:");
            texto.AppendLine("• Gasté 80000 en mercado");
            texto.AppendLine("• Me llegó el sueldo de 3.000.000");
            return texto.ToString();
        }

        public static string PreguntaTipoIngreso()
        {
            return "➕ Nuevo ingreso\n\n¿Qué tipo de ingreso recibiste?";
        }

        public static string PreguntaSobre(OperacionPendiente operacion)
        {
            if (operacion.Tipo == TipoMovimiento.Ingreso)
            {
                return "✉️ ¿A qué sobre va este ingreso?";
            }

            return "➖ Nuevo gasto\n\n¿De qué sobre sale el dinero?";
        }

        public static string PreguntaMonto(OperacionPendiente operacion)
        {
            StringBuilder texto = new StringBuilder();

            if (operacion.Tipo == TipoMovimiento.Ingreso)
            {
                texto.AppendLine(operacion.EsSueldo ? "💼 Sueldo (se reparte entre tus sobres)" : "➕ Nuevo ingreso");
            }
            else
            {
                texto.AppendLine("➖ Gasto desde " + NombreSobreElegido(operacion));
            }

            texto.AppendLine();
            texto.AppendLine("¿Cuánto fue? Toca un valor o escribe el monto (ej: 45000 o 45 mil).");
            return texto.ToString();
        }

        public static string MontoNoEntendido()
        {
            return "No pude leer el monto. Escribe solo el número, por ejemplo 45000 o 45 mil.";
        }

        public static string PreguntaDescripcion(OperacionPendiente operacion)
        {
            StringBuilder texto = new StringBuilder();
            texto.AppendLine((operacion.Tipo == TipoMovimiento.Ingreso ? "➕ Ingreso de " : "➖ Gasto de ") + Moneda(operacion.Monto));
            texto.AppendLine();
            texto.AppendLine("✏️ Escribe una descripción corta (ej: \"almuerzo\", \"venta de bicicleta\") o toca Sin descripción.");
            return texto.ToString();
        }

        public static string Propuesta(OperacionPendiente operacion)
        {
            StringBuilder texto = new StringBuilder();
            bool esIngreso = operacion.Tipo == TipoMovimiento.Ingreso;

            texto.AppendLine(esIngreso ? "🟢 Revisa tu INGRESO" : "🔴 Revisa tu GASTO");
            texto.AppendLine();
            texto.AppendLine("Monto: " + Moneda(Total(operacion)));
            texto.AppendLine("Categoría: " + operacion.Categoria);
            texto.AppendLine("Descripción: " + operacion.Descripcion);

            if (operacion.Asignaciones.Count == 1)
            {
                AsignacionSobre unica = operacion.Asignaciones[0];
                texto.AppendLine("Sobre: " + unica.SobreNombre);
            }
            else if (operacion.Asignaciones.Count > 1)
            {
                texto.AppendLine();
                texto.AppendLine("Reparto por sobres:");
                foreach (AsignacionSobre asignacion in operacion.Asignaciones)
                {
                    texto.AppendLine("• " + asignacion.SobreNombre + ": " + Moneda(asignacion.Monto)
                        + " (" + asignacion.Porcentaje.ToString("0.#") + "%)");
                }
            }
            else
            {
                texto.AppendLine("Sobre: sin sobre (no afecta saldos)");
            }

            texto.AppendLine();
            texto.AppendLine("¿Confirmas el registro?");
            return texto.ToString();
        }

        public static string Confirmada(List<AsignacionSobre> asignaciones)
        {
            StringBuilder texto = new StringBuilder();
            decimal total = 0;

            foreach (AsignacionSobre asignacion in asignaciones)
            {
                total = total + asignacion.Monto;
            }

            texto.AppendLine("✅ ¡Listo! Registro guardado.");
            texto.AppendLine();
            texto.AppendLine(asignaciones.Count == 1
                ? "Guardé 1 movimiento por " + Moneda(total) + " en " + asignaciones[0].SobreNombre + "."
                : "Guardé " + asignaciones.Count + " movimientos por " + Moneda(total) + ".");

            if (asignaciones.Count > 1)
            {
                texto.AppendLine();
                foreach (AsignacionSobre asignacion in asignaciones)
                {
                    texto.AppendLine("• " + asignacion.SobreNombre + ": " + Moneda(asignacion.Monto));
                }
            }

            return texto.ToString();
        }

        public static string Cancelada()
        {
            return "✖ Registro cancelado. No se guardó ningún movimiento.";
        }

        public static string Expirada()
        {
            return "⌛ Esta operación expiró. Toca un botón para empezar de nuevo.";
        }

        public static string SinSobres()
        {
            return "No tienes sobres activos. Actívalos desde la aplicación de escritorio.";
        }

        private static string NombreSobreElegido(OperacionPendiente operacion)
        {
            if (operacion.Asignaciones.Count == 1)
            {
                return operacion.Asignaciones[0].SobreNombre;
            }

            return "tu sobre";
        }

        public static string Resumen(ResumenDashboard resumen)
        {
            StringBuilder texto = new StringBuilder();
            texto.AppendLine("📊 Resumen de " + resumen.NombreMes + " " + resumen.Anio);
            texto.AppendLine();
            texto.AppendLine("Ingresos del mes: " + Moneda(resumen.IngresosMes));
            texto.AppendLine("Gastos del mes: " + Moneda(resumen.GastosMes));
            texto.AppendLine("Disponible: " + Moneda(resumen.DisponibleMes));
            texto.AppendLine();
            texto.AppendLine("Saldo en sobres: " + Moneda(resumen.SaldoSobres));
            texto.AppendLine("Ahorro en metas: " + Moneda(resumen.AhorroMetas));
            texto.AppendLine("Deudas pendientes: " + Moneda(resumen.SaldoDeudas));
            texto.AppendLine();
            texto.AppendLine("Alertas activas: " + resumen.Alertas.Count);
            return texto.ToString();
        }

        public static string ListaSobres(List<Sobre> sobres)
        {
            StringBuilder texto = new StringBuilder();
            decimal total = 0;
            int activos = 0;

            foreach (Sobre sobre in sobres)
            {
                if (sobre.Activo)
                {
                    activos = activos + 1;
                }
            }

            if (activos == 0)
            {
                return "No tienes sobres activos.";
            }

            texto.AppendLine("✉️ Saldos de tus sobres");
            texto.AppendLine();

            foreach (Sobre sobre in sobres)
            {
                if (!sobre.Activo)
                {
                    continue;
                }

                total = total + sobre.Saldo;
                texto.AppendLine("• " + sobre.Nombre + ": " + Moneda(sobre.Saldo)
                    + " (objetivo " + sobre.PorcentajeObjetivo.ToString("0.#") + "%)");
            }

            texto.AppendLine();
            texto.AppendLine("Total: " + Moneda(total));
            return texto.ToString();
        }

        public static string ListaAlertas(List<Alerta> alertas)
        {
            if (alertas.Count == 0)
            {
                return "🔔 No hay alertas activas. Todo en orden.";
            }

            StringBuilder texto = new StringBuilder();
            int mostradas = 0;

            texto.AppendLine("🔔 Alertas activas (" + alertas.Count + ")");
            texto.AppendLine();

            foreach (Alerta alerta in alertas)
            {
                if (mostradas >= MaximoAlertas)
                {
                    break;
                }

                mostradas = mostradas + 1;
                texto.AppendLine(mostradas + ". [" + alerta.Nivel + "] " + alerta.Titulo);
                texto.AppendLine(alerta.Mensaje);
                texto.AppendLine();
            }

            if (alertas.Count > mostradas)
            {
                texto.AppendLine("Y " + (alertas.Count - mostradas) + " alerta(s) más en la aplicación.");
            }

            return texto.ToString();
        }

        public static string Moneda(decimal monto)
        {
            return monto.ToString("C0");
        }

        private static decimal Total(OperacionPendiente operacion)
        {
            decimal total = 0;

            foreach (AsignacionSobre asignacion in operacion.Asignaciones)
            {
                total = total + asignacion.Monto;
            }

            return total;
        }
    }
}
