import { useEffect, useState } from 'react';
import { NavLink, Outlet, useNavigate } from 'react-router-dom';
import { useAuth } from '../features/auth/AuthContext';

export function Layout() {
  const { usuario, esAdmin, logout } = useAuth();
  const navegar = useNavigate();
  const [oscuro, setOscuro] = useState(() => localStorage.getItem('pedidos.tema') === 'oscuro');

  useEffect(() => {
    document.documentElement.classList.toggle('dark', oscuro);
    localStorage.setItem('pedidos.tema', oscuro ? 'oscuro' : 'claro');
  }, [oscuro]);

  const cerrarSesion = () => {
    logout();
    navegar('/login', { replace: true });
  };

  const enlace = ({ isActive }: { isActive: boolean }) =>
    `rounded-lg px-3 py-2 text-sm font-medium transition ${
      isActive
        ? 'bg-marca-600 text-white'
        : 'text-slate-600 hover:bg-slate-200/70 dark:text-slate-300 dark:hover:bg-slate-800'
    }`;

  return (
    <div className="min-h-screen">
      <header className="border-b border-slate-200 bg-white dark:border-slate-800 dark:bg-slate-900">
        <div className="mx-auto flex max-w-6xl flex-wrap items-center gap-3 px-4 py-3">
          <span className="mr-2 text-lg font-bold tracking-tight text-slate-900 dark:text-white">Pedidos</span>

          <nav aria-label="Principal" className="flex items-center gap-1">
            <NavLink to="/pedidos" className={enlace}>
              Pedidos
            </NavLink>
            <NavLink to="/pedidos/nuevo" className={enlace}>
              Nuevo pedido
            </NavLink>
          </nav>

          <div className="ml-auto flex items-center gap-3">
            <button
              type="button"
              className="boton-secundario px-3 py-1.5"
              onClick={() => setOscuro((valor) => !valor)}
              aria-pressed={oscuro}
              title={oscuro ? 'Cambiar a tema claro' : 'Cambiar a tema oscuro'}
            >
              {oscuro ? '☀' : '☾'}
              <span className="sr-only">{oscuro ? 'Tema claro' : 'Tema oscuro'}</span>
            </button>

            {usuario && (
              <div className="hidden text-right text-xs leading-tight text-slate-600 sm:block dark:text-slate-300">
                <p className="font-semibold text-slate-900 dark:text-white">{usuario.email}</p>
                <p className="uppercase tracking-wide text-slate-400">{usuario.rol}</p>
              </div>
            )}

            <button type="button" className="boton-secundario" onClick={cerrarSesion}>
              Cerrar sesión
            </button>
          </div>
        </div>
      </header>

      <main className="mx-auto max-w-6xl px-4 py-6">
        <Outlet />
      </main>

      <footer className="mx-auto max-w-6xl px-4 pb-8 text-xs text-slate-400">
        {esAdmin ? 'Sesión con permisos de administrador' : 'Sesión con permisos de usuario'}
      </footer>
    </div>
  );
}
