import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from './AuthContext';
import { extraerProblema } from '../pedidos/pedidosApi';
import type { ProblemaDetalle } from '../../lib/tipos';

const esquema = z.object({
  email: z.string().min(1, 'El email es obligatorio.').email('El email no tiene un formato válido.'),
  password: z.string().min(1, 'La contraseña es obligatoria.').min(6, 'La contraseña debe tener al menos 6 caracteres.'),
});

type FormularioLogin = z.infer<typeof esquema>;

export function LoginPage() {
  const { login, estaAutenticado, restaurando } = useAuth();
  const navegar = useNavigate();
  const ubicacion = useLocation();

  const [errorGeneral, setErrorGeneral] = useState<string | null>(null);
  const [erroresServidor, setErroresServidor] = useState<Record<string, string[]>>({});

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors, isSubmitting },
  } = useForm<FormularioLogin>({
    resolver: zodResolver(esquema),
    defaultValues: { email: '', password: '' },
  });

  if (!restaurando && estaAutenticado) {
    return <Navigate to="/pedidos" replace />;
  }

  const onSubmit = async (datos: FormularioLogin) => {
    setErrorGeneral(null);
    setErroresServidor({});

    try {
      await login(datos.email, datos.password);

      const destino = (ubicacion.state as { desde?: string } | null)?.desde ?? '/pedidos';
      navegar(destino, { replace: true });
    } catch (error) {
      const problema: ProblemaDetalle | null = extraerProblema(error);

      if (problema?.errors) {
        setErroresServidor(problema.errors);
        Object.entries(problema.errors).forEach(([campo, mensajes]) => {
          setError(campo as keyof FormularioLogin, { message: mensajes[0] });
        });
      }

      setErrorGeneral(
        problema?.status === 401
          ? 'Las credenciales introducidas no son válidas.'
          : (problema?.detail ?? 'No se ha podido iniciar sesión. Inténtalo de nuevo.'),
      );
    }
  };

  return (
    <main className="flex min-h-screen items-center justify-center bg-slate-100 px-4 py-10 dark:bg-slate-950">
      <div className="w-full max-w-md">
        <div className="mb-6 text-center">
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white">Pedidos</h1>
          <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">Acceso a la gestión de pedidos</p>
        </div>

        <form onSubmit={handleSubmit(onSubmit)} noValidate className="tarjeta space-y-5 p-6">
          <div>
            <label htmlFor="email" className="etiqueta">
              Email
            </label>
            <input
              id="email"
              type="email"
              autoComplete="username"
              className="campo"
              aria-invalid={Boolean(errors.email)}
              aria-describedby={errors.email ? 'email-error' : undefined}
              {...register('email')}
            />
            {errors.email && (
              <p id="email-error" className="mt-1 text-sm text-rose-600 dark:text-rose-400" role="alert">
                {errors.email.message}
              </p>
            )}
          </div>

          <div>
            <label htmlFor="password" className="etiqueta">
              Contraseña
            </label>
            <input
              id="password"
              type="password"
              autoComplete="current-password"
              className="campo"
              aria-invalid={Boolean(errors.password)}
              aria-describedby={errors.password ? 'password-error' : undefined}
              {...register('password')}
            />
            {errors.password && (
              <p id="password-error" className="mt-1 text-sm text-rose-600 dark:text-rose-400" role="alert">
                {errors.password.message}
              </p>
            )}
          </div>

          {errorGeneral && (
            <div
              role="alert"
              className="rounded-lg border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700 dark:border-rose-900 dark:bg-rose-950 dark:text-rose-300"
            >
              {errorGeneral}
            </div>
          )}

          {Object.keys(erroresServidor).length > 0 && (
            <p className="text-xs text-slate-500 dark:text-slate-400">
              Revisa los campos marcados antes de continuar.
            </p>
          )}

          <button type="submit" className="boton-primario w-full" disabled={isSubmitting}>
            {isSubmitting ? 'Entrando...' : 'Iniciar sesión'}
          </button>

          <div className="rounded-lg bg-slate-50 p-3 text-xs text-slate-500 dark:bg-slate-800/50 dark:text-slate-400">
            <p className="font-semibold text-slate-600 dark:text-slate-300">Credenciales de prueba</p>
            <p>user@email.com / 123456 (User)</p>
            <p>admin@email.com / Admin123! (Admin)</p>
          </div>
        </form>
      </div>
    </main>
  );
}
