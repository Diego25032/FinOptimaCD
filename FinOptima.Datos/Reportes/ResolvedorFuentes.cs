using System;
using System.Collections.Generic;
using System.IO;
using PdfSharp.Fonts;

namespace FinOptima.Datos.Reportes
{
    /// <summary>
    /// Busca una fuente sans-serif instalada en el equipo (Segoe UI o Arial en Windows,
    /// DejaVu o Liberation en Linux) para que el PDF se genere en cualquier sistema.
    /// </summary>
    internal class ResolvedorFuentes : IFontResolver
    {
        public const string Familia = "FinOptima Sans";

        private const string CaraNormal = "FinOptima#Normal";
        private const string CaraNegrita = "FinOptima#Negrita";

        private static readonly object Bloqueo = new object();
        private static bool registrado = false;

        private Dictionary<string, byte[]> cache = new Dictionary<string, byte[]>();

        public static void Registrar()
        {
            lock (Bloqueo)
            {
                if (registrado)
                {
                    return;
                }

                if (GlobalFontSettings.FontResolver == null)
                {
                    GlobalFontSettings.FontResolver = new ResolvedorFuentes();
                }

                registrado = true;
            }
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            if (isBold)
            {
                return new FontResolverInfo(CaraNegrita);
            }

            return new FontResolverInfo(CaraNormal);
        }

        public byte[] GetFont(string faceName)
        {
            lock (Bloqueo)
            {
                byte[] datos;

                if (cache.TryGetValue(faceName, out datos))
                {
                    return datos;
                }

                string ruta = faceName == CaraNegrita ? BuscarArchivo(true) : BuscarArchivo(false);

                if (ruta.Length == 0)
                {
                    throw new InvalidOperationException("No se encontro una fuente instalada para generar el PDF (Segoe UI, Arial, DejaVu Sans o Liberation Sans).");
                }

                datos = File.ReadAllBytes(ruta);
                cache[faceName] = datos;
                return datos;
            }
        }

        private static string BuscarArchivo(bool negrita)
        {
            List<string> candidatos = new List<string>();
            string carpetaWindows = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

            if (carpetaWindows == null || carpetaWindows.Length == 0)
            {
                string windir = Environment.GetEnvironmentVariable("WINDIR");

                if (windir != null && windir.Length > 0)
                {
                    carpetaWindows = Path.Combine(windir, "Fonts");
                }
            }

            if (carpetaWindows != null && carpetaWindows.Length > 0)
            {
                if (negrita)
                {
                    candidatos.Add(Path.Combine(carpetaWindows, "segoeuib.ttf"));
                    candidatos.Add(Path.Combine(carpetaWindows, "arialbd.ttf"));
                }
                else
                {
                    candidatos.Add(Path.Combine(carpetaWindows, "segoeui.ttf"));
                    candidatos.Add(Path.Combine(carpetaWindows, "arial.ttf"));
                }
            }

            if (negrita)
            {
                candidatos.Add("/System/Library/Fonts/Supplemental/Arial Bold.ttf");
                candidatos.Add("/usr/share/fonts/truetype/dejavu/DejaVuSans-Bold.ttf");
                candidatos.Add("/usr/share/fonts/TTF/DejaVuSans-Bold.ttf");
                candidatos.Add("/usr/share/fonts/dejavu/DejaVuSans-Bold.ttf");
                candidatos.Add("/usr/share/fonts/truetype/liberation/LiberationSans-Bold.ttf");
            }
            else
            {
                candidatos.Add("/System/Library/Fonts/Supplemental/Arial.ttf");
                candidatos.Add("/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf");
                candidatos.Add("/usr/share/fonts/TTF/DejaVuSans.ttf");
                candidatos.Add("/usr/share/fonts/dejavu/DejaVuSans.ttf");
                candidatos.Add("/usr/share/fonts/truetype/liberation/LiberationSans-Regular.ttf");
            }

            foreach (string candidato in candidatos)
            {
                if (File.Exists(candidato))
                {
                    return candidato;
                }
            }

            return "";
        }
    }
}
