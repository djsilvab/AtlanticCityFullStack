import { render, screen } from '@testing-library/react';
import { MemoryRouter, Route, Routes } from 'react-router-dom';
import { describe, expect, it } from 'vitest';
import { AuthProvider } from '../features/auth/AuthContext';
import { ProtectedRoute } from '../features/auth/ProtectedRoute';

function renderRutas() {
  return render(
    <MemoryRouter initialEntries={['/pedidos']}>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<p>Pantalla de login</p>} />
          <Route element={<ProtectedRoute />}>
            <Route path="/pedidos" element={<p>Listado de pedidos</p>} />
          </Route>
        </Routes>
      </AuthProvider>
    </MemoryRouter>,
  );
}

describe('ProtectedRoute', () => {
  it('redirige al login cuando no hay sesión', async () => {
    renderRutas();

    expect(await screen.findByText('Pantalla de login')).toBeInTheDocument();
    expect(screen.queryByText('Listado de pedidos')).not.toBeInTheDocument();
  });

  it('deja pasar al contenido privado cuando la sesión es válida', async () => {
    sessionStorage.setItem(
      'pedidos.sesion',
      JSON.stringify({
        email: 'user@email.com',
        rol: 'User',
        expiraEn: Math.floor(Date.now() / 1000) + 3600,
      }),
    );

    renderRutas();

    expect(await screen.findByText('Listado de pedidos')).toBeInTheDocument();
  });

  it('redirige al login cuando la sesión está caducada', async () => {
    sessionStorage.setItem(
      'pedidos.sesion',
      JSON.stringify({
        email: 'user@email.com',
        rol: 'User',
        expiraEn: Math.floor(Date.now() / 1000) - 60,
      }),
    );

    renderRutas();

    expect(await screen.findByText('Pantalla de login')).toBeInTheDocument();
  });
});
