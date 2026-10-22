import { Navigate, Route, Routes } from 'react-router-dom';
import { Layout } from '../components/Layout';
import { LoginPage } from '../features/auth/LoginPage';
import { ProtectedRoute } from '../features/auth/ProtectedRoute';
import { useAuth } from '../features/auth/AuthContext';
import { EditarPedidoPage } from '../features/pedidos/EditarPedidoPage';
import { NuevoPedidoPage } from '../features/pedidos/NuevoPedidoPage';
import { PedidosListPage } from '../features/pedidos/PedidosListPage';
import { NotFoundPage } from './NotFoundPage';

function RedirigirInicio() {
  const { estaAutenticado, restaurando } = useAuth();

  if (restaurando) {
    return null;
  }

  return <Navigate to={estaAutenticado ? '/pedidos' : '/login'} replace />;
}

export function AppRoutes() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<Layout />}>
          <Route path="/pedidos" element={<PedidosListPage />} />
          <Route path="/pedidos/nuevo" element={<NuevoPedidoPage />} />
          <Route path="/pedidos/:id/editar" element={<EditarPedidoPage />} />
        </Route>
      </Route>

      <Route path="/" element={<RedirigirInicio />} />
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
