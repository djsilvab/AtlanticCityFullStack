import { useEffect, useRef } from 'react';
import type { ReactNode } from 'react';

interface Props {
  abierto: boolean;
  titulo: string;
  descripcion?: string;
  children?: ReactNode;
  onCancelar: () => void;
  onConfirmar: () => void;
  textoConfirmar?: string;
  procesando?: boolean;
  peligro?: boolean;
}

/**
 * Diálogo de confirmación modal (usado para eliminar pedidos).
 */
export function DialogoConfirmacion({
  abierto,
  titulo,
  descripcion,
  children,
  onCancelar,
  onConfirmar,
  textoConfirmar = 'Confirmar',
  procesando = false,
  peligro = false,
}: Props) {
  const botonRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    if (abierto) {
      botonRef.current?.focus();
    }
  }, [abierto]);

  useEffect(() => {
    if (!abierto) {
      return;
    }

    const alPulsar = (evento: KeyboardEvent) => {
      if (evento.key === 'Escape' && !procesando) {
        onCancelar();
      }
    };

    document.addEventListener('keydown', alPulsar);

    return () => document.removeEventListener('keydown', alPulsar);
  }, [abierto, onCancelar, procesando]);

  if (!abierto) {
    return null;
  }

  return (
    <div className="fixed inset-0 z-40 flex items-center justify-center bg-slate-900/50 p-4" role="presentation">
      <div
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="dialogo-titulo"
        aria-describedby="dialogo-descripcion"
        className="tarjeta w-full max-w-md p-5"
      >
        <h2 id="dialogo-titulo" className="text-lg font-semibold text-slate-900 dark:text-white">
          {titulo}
        </h2>
        {descripcion && (
          <p id="dialogo-descripcion" className="mt-2 text-sm text-slate-600 dark:text-slate-300">
            {descripcion}
          </p>
        )}
        {children}

        <div className="mt-5 flex justify-end gap-2">
          <button type="button" className="boton-secundario" onClick={onCancelar} disabled={procesando}>
            Cancelar
          </button>
          <button
            ref={botonRef}
            type="button"
            className={peligro ? 'boton-peligro' : 'boton-primario'}
            onClick={onConfirmar}
            disabled={procesando}
          >
            {procesando ? 'Procesando...' : textoConfirmar}
          </button>
        </div>
      </div>
    </div>
  );
}
