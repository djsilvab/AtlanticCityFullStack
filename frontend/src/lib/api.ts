import axios from 'axios';
import { borrarSesion, leerToken } from './sesion';

const BASE_URL = import.meta.env.VITE_API_URL ?? 'http://localhost:5080';

/**
 * Cliente HTTP de la API. Adjunta el token Bearer y, ante un 401, cierra la sesión
 * y redirige al login.
 */
export const api = axios.create({
  baseURL: BASE_URL,
  headers: { 'Content-Type': 'application/json' },
});

api.interceptors.request.use((configuracion) => {
  const token = leerToken();

  if (token) {
    configuracion.headers.Authorization = `Bearer ${token}`;
  }

  return configuracion;
});

api.interceptors.response.use(
  (respuesta) => respuesta,
  (error: unknown) => {
    if (axios.isAxiosError(error) && error.response?.status === 401) {
      borrarSesion();

      if (window.location.pathname !== '/login') {
        window.location.assign('/login');
      }
    }

    return Promise.reject(error);
  },
);
