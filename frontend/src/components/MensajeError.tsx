interface Props {
  titulo: string;
  descripcion?: string;
  onReintentar?: () => void;
}

/**
 * Mensaje de error con acción opcional de reintento.
 */
export function MensajeError({ titulo, descripcion, onReintentar }: Props) {
  return (
    <div
      role="alert"
      className="flex flex-col items-start gap-3 rounded-xl border border-rose-200 bg-rose-50 p-5 text-sm text-rose-800 dark:border-rose-900 dark:bg-rose-950/60 dark:text-rose-200"
    >
      <div>
        <p className="font-semibold">{titulo}</p>
        {descripcion && <p className="mt-1 text-rose-700/90 dark:text-rose-300/90">{descripcion}</p>}
      </div>
      {onReintentar && (
        <button type="button" className="boton-secundario" onClick={onReintentar}>
          Reintentar
        </button>
      )}
    </div>
  );
}
