import type { EstadoPedido } from '../lib/tipos';

const ESTILOS: Record<EstadoPedido, string> = {
  Registrado: 'bg-sky-100 text-sky-800 ring-sky-600/20 dark:bg-sky-950 dark:text-sky-200',
  EnProceso: 'bg-amber-100 text-amber-800 ring-amber-600/20 dark:bg-amber-950 dark:text-amber-200',
  Completado: 'bg-emerald-100 text-emerald-800 ring-emerald-600/20 dark:bg-emerald-950 dark:text-emerald-200',
  Cancelado: 'bg-slate-200 text-slate-700 ring-slate-500/20 dark:bg-slate-800 dark:text-slate-300',
};

export function BadgeEstado({ estado }: { estado: EstadoPedido }) {
  return (
    <span
      className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ring-1 ring-inset ${ESTILOS[estado]}`}
    >
      {estado}
    </span>
  );
}
