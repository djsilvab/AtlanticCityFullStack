import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import type { ReactNode } from 'react';
import { api } from '../../lib/api';
import { borrarSesion, decodificarToken, guardarSesion, leerSesion, sesionVigente } from '../../lib/sesion';
import type { LoginResponse, Rol, UsuarioSesion } from '../../lib/tipos';

export interface AuthContextValue {
  usuario: UsuarioSesion | null;
  estaAutenticado: boolean;
  esAdmin: boolean;
  restaurando: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [usuario, setUsuario] = useState<UsuarioSesion | null>(null);
  const [restaurando, setRestaurando] = useState(true);

  // Restaura la sesión guardada al recargar la página (decisión D8).
  useEffect(() => {
    const guardada = leerSesion();

    if (sesionVigente(guardada)) {
      setUsuario(guardada);
    } else {
      borrarSesion();
    }

    setRestaurando(false);
  }, []);

  const login = useCallback(async (email: string, password: string) => {
    const respuesta = await api.post<LoginResponse>('/auth/login', { email, password });
    const { token, expiresIn } = respuesta.data;

    const payload = decodificarToken(token);
    const expiraEn = payload?.exp ?? Math.floor(Date.now() / 1000) + expiresIn;
    const sesion: UsuarioSesion = {
      email: payload?.email ?? email,
      rol: (payload?.role ?? 'User') as Rol,
      expiraEn,
    };

    guardarSesion(sesion, token);
    setUsuario(sesion);
  }, []);

  const logout = useCallback(() => {
    borrarSesion();
    setUsuario(null);
  }, []);

  const valor = useMemo<AuthContextValue>(
    () => ({
      usuario,
      estaAutenticado: usuario !== null,
      esAdmin: usuario?.rol === 'Admin',
      restaurando,
      login,
      logout,
    }),
    [usuario, restaurando, login, logout],
  );

  return <AuthContext.Provider value={valor}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const contexto = useContext(AuthContext);

  if (contexto === null) {
    throw new Error('useAuth debe usarse dentro de un AuthProvider.');
  }

  return contexto;
}
