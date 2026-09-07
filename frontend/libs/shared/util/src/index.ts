// Utilidades puras compartidas (formato de moneda y fecha es-MX, helpers de
// colecciones, etc.). Sin dependencias de framework. Se poblará según haga falta.

/** Formatea un importe en pesos mexicanos. */
export function formatoMoneda(valor: number): string {
  return new Intl.NumberFormat('es-MX', {
    style: 'currency',
    currency: 'MXN',
  }).format(valor);
}
