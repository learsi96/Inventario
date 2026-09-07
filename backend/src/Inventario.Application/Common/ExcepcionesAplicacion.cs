namespace Inventario.Application.Common;

/// <summary>La entidad solicitada no existe (o no es visible para el tenant actual).</summary>
public sealed class NoEncontradoException(string mensaje) : Exception(mensaje);

/// <summary>La operación entra en conflicto con el estado actual (p. ej. clave duplicada).</summary>
public sealed class ConflictoException(string mensaje) : Exception(mensaje);

/// <summary>Los datos de entrada no son válidos.</summary>
public sealed class ValidacionException(string mensaje) : Exception(mensaje);

/// <summary>Credenciales inválidas o usuario inactivo.</summary>
public sealed class AutenticacionException(string mensaje) : Exception(mensaje);
