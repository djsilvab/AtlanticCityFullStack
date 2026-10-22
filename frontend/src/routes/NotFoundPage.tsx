import { Link } from 'react-router-dom';

export function NotFoundPage() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center gap-4 px-4 text-center">
      <p className="text-6xl font-black text-marca-600">404</p>
      <h1 className="text-xl font-semibold text-slate-900 dark:text-white">Página no encontrada</h1>
      <p className="max-w-md text-sm text-slate-500 dark:text-slate-400">
        La dirección que has abierto no corresponde a ninguna pantalla de la aplicación.
      </p>
      <Link to="/pedidos" className="boton-primario">
        Ir a los pedidos
      </Link>
    </main>
  );
}
