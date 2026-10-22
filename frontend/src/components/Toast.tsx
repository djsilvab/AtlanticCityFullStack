import { createContext, useCallback, useContext, useMemo, useRef, useState } from 'react';
import type { ReactNode } from 'react';

export type TipoAviso = 'exito' | 'error' | 'info';

interface Aviso {
  id: number;
  mensaje: string;
  tipo: TipoAviso;
}

interface ToastContextValue {
  mostrar: (mensaje: string, tipo?: TipoAviso) => void;
}

const ToastContext = createContext<ToastContextValue | null>(null);

const ESTILOS: Record<TipoAviso, string> = {
  exito: 'border-emerald-300 bg-emerald-50 text-emerald-900 dark:border-emerald-800 dark:bg-emerald-950 dark:text-emerald-100',
  error: 'border-rose-300 bg-rose-50 text-rose-900 dark:border-rose-800 dark:bg-rose-950 dark:text-rose-100',
  info: 'border-sky-300 bg-sky-50 text-sky-900 dark:border-sky-800 dark:bg-sky-950 dark:text-sky-100',
};

export function ToastProvider({ children }: { children: ReactNode }) {
  const [avisos, setAvisos] = useState<Aviso[]>([]);
  const siguienteId = useRef(1);

  const mostrar = useCallback((mensaje: string, tipo: TipoAviso = 'info') => {
    const id = siguienteId.current;
    siguienteId.current += 1;

    setAvisos((actuales) => [...actuales, { id, mensaje, tipo }]);

    window.setTimeout(() => {
      setAvisos((actuales) => actuales.filter((aviso) => aviso.id !== id));
    }, 5000);
  }, []);

  const valor = useMemo(() => ({ mostrar }), [mostrar]);

  return (
    <ToastContext.Provider value={valor}>
      {children}
      <div
        aria-live="polite"
        aria-atomic="false"
        className="pointer-events-none fixed inset-x-0 bottom-4 z-50 flex flex-col items-center gap-2 px-4"
      >
        {avisos.map((aviso) => (
          <div
            key={aviso.id}
            role="status"
            className={`pointer-events-auto w-full max-w-sm rounded-lg border px-4 py-3 text-sm font-medium shadow-lg ${ESTILOS[aviso.tipo]}`}
          >
            {aviso.mensaje}
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
}

export function useToast(): ToastContextValue {
  const contexto = useContext(ToastContext);

  if (contexto === null) {
    throw new Error('useToast debe usarse dentro de un ToastProvider.');
  }

  return contexto;
}
