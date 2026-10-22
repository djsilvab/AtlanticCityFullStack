using Xunit;

namespace Pedidos.IntegrationTests.Infraestructura;

/// <summary>
/// Colección que comparte una única instancia de la API entre las pruebas.
/// </summary>
[CollectionDefinition(Nombre)]
public sealed class ApiCollection : ICollectionFixture<PedidosApiFactory>
{
    public const string Nombre = "API";
}
