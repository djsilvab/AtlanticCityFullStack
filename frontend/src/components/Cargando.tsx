export function Cargando({ etiqueta = 'Cargando...' }: { etiqueta?: string }) {
  return (
    <div className="flex flex-col items-center justify-center gap-3 py-16" role="status" aria-live="polite">
      <span className="size-8 animate-spin rounded-full border-2 border-slate-300 border-t-marca-600" aria-hidden="true" />
      <span className="text-sm text-slate-500 dark:text-slate-400">{etiqueta}</span>
    </div>
  );
}
