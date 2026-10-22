import type { PayloadJwt, UsuarioSesion } from './tipos';

const CLAVE_SESION = 'pedidos.sesion';
const CLAVE_TEMPORAL = 'pedidos.temporal';

/**
 * Decodifica el payload de un JWT sin verificarlo (la verificación la hace la API).
 */
export function decodificarToken(token: string): PayloadJwt | null {
  const partes = token.split('.');
  const payload = partes[1];

  if (partes.length !== 3 || payload === undefined) {
    return null;
  }

  try {
    const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
    const json = decodeURIComponent(
      atob(base64)
        .split('')
        .map((caracter) => `%${`00${caracter.charCodeAt(0).toString(16)}`.slice(-2)}`)
        .join(''),
    );

    return JSON.parse(json) as PayloadJwt;
  } catch {
    return null;
  }
}

/**
 * Guarda la sesión en `sessionStorage` (decisión D8).
 */
export function guardarSesion(usuario: UsuarioSesion, token: string): void {
  sessionStorage.setItem(CLAVE_SESION, JSON.stringify(usuario));
  sessionStorage.setItem(CLAVE_TEMPORAL, token);
}

export function leerSesion(): UsuarioSesion | null {
  const crudo = sessionStorage.getItem(CLAVE_SESION);

  if (!crudo) {
    return null;
  }

  try {
    return JSON.parse(crudo) as UsuarioSesion;
  } catch {
    return null;
  }
}

export function leerToken(): string | null {
  return sessionStorage.getItem(CLAVE_TEMPORAL);
}

export function borrarSesion(): void {
  sessionStorage.removeItem(CLAVE_SESION);
  sessionStorage.removeItem(CLAVE_TEMPORAL);
}

/**
 * Indica si la sesión guardada sigue vigente (el token no ha expirado).
 */
export function sesionVigente(usuario: UsuarioSesion | null): boolean {
  return usuario !== null && usuario.expiraEn * 1000 > Date.now();
}
