using FinOptima.Entidades.Enums;

namespace FinOptima.Bot
{
    public static class ClasificadorSobres
    {
        private static readonly string[] Necesidades = { "mercado", "supermercado", "comida", "arriendo", "renta", "transporte", "bus", "pasaje", "gasolina", "combustible", "luz", "agua", "internet", "telefono", "servicios", "salud", "farmacia", "medico", "hogar", "aseo", "ropa" };
        private static readonly string[] Ahorro = { "ahorro", "ahorrar", "emergencia", "reserva" };
        private static readonly string[] Inversion = { "inversion", "invertir", "acciones", "cdt", "fondo", "cripto", "bitcoin" };
        private static readonly string[] Educacion = { "educacion", "curso", "universidad", "colegio", "libro", "libros", "estudio", "matricula", "capacitacion", "taller" };
        private static readonly string[] Ocio = { "ocio", "cine", "restaurante", "salida", "viaje", "cafe", "bar", "streaming", "netflix", "spotify", "concierto", "juego" };
        private static readonly string[] Generosidad = { "generosidad", "regalo", "donacion", "diezmo", "iglesia", "caridad" };
        private static readonly string[] Metas = { "meta", "metas", "objetivo" };

        public static TipoSobre Clasificar(string texto, out string categoria)
        {
            string palabra;

            if (Buscar(texto, Necesidades, out palabra))
            {
                categoria = palabra;
                return TipoSobre.Necesidades;
            }

            if (Buscar(texto, Ahorro, out palabra))
            {
                categoria = palabra;
                return TipoSobre.Ahorro;
            }

            if (Buscar(texto, Inversion, out palabra))
            {
                categoria = palabra;
                return TipoSobre.Inversion;
            }

            if (Buscar(texto, Educacion, out palabra))
            {
                categoria = palabra;
                return TipoSobre.Educacion;
            }

            if (Buscar(texto, Ocio, out palabra))
            {
                categoria = palabra;
                return TipoSobre.Ocio;
            }

            if (Buscar(texto, Generosidad, out palabra))
            {
                categoria = palabra;
                return TipoSobre.Generosidad;
            }

            if (Buscar(texto, Metas, out palabra))
            {
                categoria = palabra;
                return TipoSobre.Metas;
            }

            categoria = "";
            return TipoSobre.Necesidades;
        }

        private static bool Buscar(string texto, string[] palabras, out string encontrada)
        {
            foreach (string palabra in palabras)
            {
                if (InterpretadorMensaje.ContienePalabra(texto, palabra))
                {
                    encontrada = palabra;
                    return true;
                }
            }

            encontrada = "";
            return false;
        }
    }
}
