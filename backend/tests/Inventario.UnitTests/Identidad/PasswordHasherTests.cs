using Inventario.Infrastructure.Identidad;
using Shouldly;

namespace Inventario.UnitTests.Identidad;

public sealed class PasswordHasherTests
{
    private readonly PasswordHasherAdapter _hasher = new();

    [Fact]
    public void Verificar_acepta_la_contrasena_correcta()
    {
        var hash = _hasher.Hash("Demo1234!");

        _hasher.Verificar(hash, "Demo1234!").ShouldBeTrue();
    }

    [Fact]
    public void Verificar_rechaza_una_contrasena_incorrecta()
    {
        var hash = _hasher.Hash("Demo1234!");

        _hasher.Verificar(hash, "otra").ShouldBeFalse();
    }

    [Fact]
    public void Cada_hash_es_distinto_por_la_sal()
    {
        _hasher.Hash("Demo1234!").ShouldNotBe(_hasher.Hash("Demo1234!"));
    }
}
