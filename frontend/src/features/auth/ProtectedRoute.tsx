import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { useAuth } from './AuthContext';
import { Cargando } from '../../components/Cargando';

/**
 * Bloquea las rutas privadas: si no hay sesión válida redirige a `/login`.
 */
export function ProtectedRoute() {
  const { estaAutenticado, restaurando } = useAuth();
  const ubicacion = useLocation();

  if (restaurando) {
    return <Cargando etiqueta="Restaurando sesión..." />;
  }

  if (!estaAutenticado) {
    return <Navigate to="/login" replace state={{ desde: ubicacion.pathname }} />;
  }

  return <Outlet />;
}
