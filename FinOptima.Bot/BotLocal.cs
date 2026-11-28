using System;
using System.Collections.Generic;
using System.Threading;
using FinOptima.Logica.Composicion;

namespace FinOptima.Bot
{
    public sealed class BotLocal : IDisposable
    {
        private readonly FabricaServicios servicios;
        private readonly ClienteTelegram cliente;
        private readonly ManejadorBot manejador;
        private readonly CancellationTokenSource cancelacion;
        private readonly object sincronizacion;
        private Thread hilo;
        private Exception error;
        private bool detenido;

        public BotLocal(FabricaServicios fabrica, string token, long chatAutorizado)
        {
            servicios = fabrica;
            cliente = new ClienteTelegram(token);
            manejador = new ManejadorBot(cliente, servicios, chatAutorizado);
            cancelacion = new CancellationTokenSource();
            sincronizacion = new object();
        }

        public bool EstaActivo
        {
            get
            {
                lock (sincronizacion)
                {
                    return !detenido && hilo != null && hilo.IsAlive;
                }
            }
        }

        public Exception Error
        {
            get
            {
                lock (sincronizacion)
                {
                    return error;
                }
            }
        }

        public void Iniciar()
        {
            lock (sincronizacion)
            {
                if (detenido)
                {
                    throw new InvalidOperationException("El bot local ya fue detenido.");
                }

                if (hilo != null && hilo.IsAlive)
                {
                    return;
                }

                hilo = new Thread(Ejecutar);
                hilo.IsBackground = true;
                hilo.Start();
            }
        }

        private void Ejecutar()
        {
            try
            {
                cliente.ObtenerActualizaciones(0, 0, cancelacion.Token);

                try
                {
                    cliente.ConfigurarComandos();
                }
                catch (Exception)
                {
                }

                int ultimoNumero = 0;

                while (!cancelacion.IsCancellationRequested)
                {
                    try
                    {
                        List<ActualizacionTelegram> actualizaciones = cliente.ObtenerActualizaciones(ultimoNumero, 25, cancelacion.Token);

                        foreach (ActualizacionTelegram actualizacion in actualizaciones)
                        {
                            if (cancelacion.IsCancellationRequested)
                            {
                                break;
                            }

                            manejador.Procesar(actualizacion);
                            ultimoNumero = actualizacion.Numero + 1;
                        }
                    }
                    catch (OperationCanceledException) when (cancelacion.IsCancellationRequested)
                    {
                        break;
                    }
                    catch (Exception excepcion)
                    {
                        GuardarError(excepcion);

                        if (cancelacion.Token.WaitHandle.WaitOne(4000))
                        {
                            break;
                        }
                    }
                }
            }
            catch (OperationCanceledException) when (cancelacion.IsCancellationRequested)
            {
            }
            catch (Exception excepcion)
            {
                GuardarError(excepcion);
            }
        }

        private void GuardarError(Exception excepcion)
        {
            lock (sincronizacion)
            {
                error = excepcion;
            }
        }

        public void Detener()
        {
            Thread hiloActual;

            lock (sincronizacion)
            {
                if (detenido)
                {
                    return;
                }

                detenido = true;
                hiloActual = hilo;
                cancelacion.Cancel();
            }

            if (hiloActual != null && hiloActual != Thread.CurrentThread)
            {
                hiloActual.Join(2000);
            }

            cliente.Dispose();
        }

        public void Dispose()
        {
            Detener();
        }
    }
}
