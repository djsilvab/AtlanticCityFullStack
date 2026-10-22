namespace Pedidos.Infrastructure.Configuration;

/// <summary>
/// Lector del archivo <c>.env</c> de la raíz del repositorio.
/// Evita versionar secretos: la contraseña de SQL Server y la clave JWT viven en <c>.env</c>
/// (ignorado por git) o en variables de entorno / user-secrets.
/// </summary>
public static class EnvFile
{
    private const string FileName = ".env";

    /// <summary>
    /// Devuelve las variables definidas en el archivo <c>.env</c> de la raíz del repositorio.
    /// Si el archivo no existe devuelve un diccionario vacío.
    /// </summary>
    public static IReadOnlyDictionary<string, string> Read(string? rutaArchivo = null)
    {
        var ruta = rutaArchivo ?? BuscarArchivo();
        var valores = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (ruta is null || !File.Exists(ruta))
        {
            return valores;
        }

        foreach (var linea in File.ReadAllLines(ruta))
        {
            var trimmed = linea.Trim();

            if (trimmed.Length == 0 || trimmed.StartsWith('#'))
            {
                continue;
            }

            var separador = trimmed.IndexOf('=');

            if (separador <= 0)
            {
                continue;
            }

            var clave = trimmed[..separador].Trim();
            var valor = trimmed[(separador + 1)..].Trim();

            if (valor.Length >= 2 && valor[0] == '"' && valor[^1] == '"')
            {
                valor = valor[1..^1];
            }

            valores[clave] = valor;
        }

        return valores;
    }

    /// <summary>
    /// Publica las variables del archivo <c>.env</c> en las variables de entorno del proceso.
    /// Las variables ya definidas tienen prioridad: <c>.env</c> solo rellena huecos.
    /// </summary>
    public static void LoadIntoEnvironment(string? rutaArchivo = null)
    {
        foreach (var (clave, valor) in Read(rutaArchivo))
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(clave)))
            {
                Environment.SetEnvironmentVariable(clave, valor);
            }
        }
    }

    /// <summary>Localiza el archivo <c>.env</c> subiendo desde el directorio actual hasta la raíz del repositorio.</summary>
    public static string? BuscarArchivo()
    {
        var desdeDirectorioActual = BuscarDesde(Directory.GetCurrentDirectory());

        return desdeDirectorioActual ?? BuscarDesde(AppContext.BaseDirectory);
    }

    private static string? BuscarDesde(string directorioInicial)
    {
        var directorio = new DirectoryInfo(directorioInicial);

        while (directorio is not null)
        {
            var candidato = Path.Combine(directorio.FullName, FileName);

            if (File.Exists(candidato))
            {
                return candidato;
            }

            var solucion = Path.Combine(directorio.FullName, "Pedidos.sln");

            if (File.Exists(solucion))
            {
                return Path.Combine(directorio.FullName, FileName);
            }

            directorio = directorio.Parent;
        }

        return null;
    }
}
