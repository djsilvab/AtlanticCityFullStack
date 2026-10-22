import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ESTADOS } from '../../lib/tipos';
import type { EstadoPedido } from '../../lib/tipos';

export const esquemaPedido = z.object({
  numeroPedido: z
    .string()
    .min(1, 'El número de pedido es obligatorio.')
    .max(50, 'El número de pedido no puede superar 50 caracteres.'),
  cliente: z
    .string()
    .min(1, 'El cliente es obligatorio.')
    .max(150, 'El cliente no puede superar 150 caracteres.'),
  fecha: z.string().min(1, 'La fecha del pedido es obligatoria.'),
  total: z
    .number({ message: 'El total debe ser un número.' })
    .positive('El total debe ser mayor que cero.')
    .refine((valor) => Number.isInteger(valor * 100), 'El total admite como máximo 2 decimales.'),
  estado: z.enum(ESTADOS as [EstadoPedido, ...EstadoPedido[]], {
    message: 'El estado del pedido no es válido.',
  }),
});

export type FormularioPedido = z.infer<typeof esquemaPedido>;

export const valoresIniciales: FormularioPedido = {
  numeroPedido: '',
  cliente: '',
  fecha: new Date().toISOString().slice(0, 10),
  total: 0,
  estado: 'Registrado',
};

interface Props {
  valoresInicialesFormulario?: FormularioPedido;
  onEnviar: (valores: FormularioPedido) => Promise<void>;
  guardando: boolean;
  errorGeneral: string | null;
  erroresServidor?: Record<string, string[]>;
  textoEnviar: string;
  onCancelar: () => void;
}

export function PedidoForm({
  valoresInicialesFormulario = valoresIniciales,
  onEnviar,
  guardando,
  errorGeneral,
  erroresServidor = {},
  textoEnviar,
  onCancelar,
}: Props) {
  const {
    register,
    handleSubmit,
    reset,
    setError,
    formState: { errors },
  } = useForm<FormularioPedido>({
    resolver: zodResolver(esquemaPedido),
    defaultValues: valoresInicialesFormulario,
  });

  useEffect(() => {
    reset(valoresInicialesFormulario);
  }, [valoresInicialesFormulario, reset]);

  // Los errores de validación del servidor (400) se muestran junto al campo.
  useEffect(() => {
    Object.entries(erroresServidor).forEach(([campo, mensajes]) => {
      if (campo in valoresIniciales) {
        setError(campo as keyof FormularioPedido, { message: mensajes[0] });
      }
    });
  }, [erroresServidor, setError]);

  const errorDe = (campo: keyof FormularioPedido) => errors[campo]?.message;

  return (
    <form onSubmit={handleSubmit((valores) => onEnviar(valores))} noValidate className="tarjeta space-y-5 p-6">
      <div className="grid gap-5 sm:grid-cols-2">
        <div>
          <label htmlFor="numeroPedido" className="etiqueta">
            Número de pedido
          </label>
          <input
            id="numeroPedido"
            className="campo"
            maxLength={50}
            aria-invalid={Boolean(errorDe('numeroPedido'))}
            aria-describedby={errorDe('numeroPedido') ? 'numeroPedido-error' : undefined}
            {...register('numeroPedido')}
          />
          {errorDe('numeroPedido') && (
            <p id="numeroPedido-error" role="alert" className="mt-1 text-sm text-rose-600 dark:text-rose-400">
              {errorDe('numeroPedido')}
            </p>
          )}
        </div>

        <div>
          <label htmlFor="cliente" className="etiqueta">
            Cliente
          </label>
          <input
            id="cliente"
            className="campo"
            maxLength={150}
            aria-invalid={Boolean(errorDe('cliente'))}
            aria-describedby={errorDe('cliente') ? 'cliente-error' : undefined}
            {...register('cliente')}
          />
          {errorDe('cliente') && (
            <p id="cliente-error" role="alert" className="mt-1 text-sm text-rose-600 dark:text-rose-400">
              {errorDe('cliente')}
            </p>
          )}
        </div>

        <div>
          <label htmlFor="fecha" className="etiqueta">
            Fecha
          </label>
          <input
            id="fecha"
            type="date"
            className="campo"
            aria-invalid={Boolean(errorDe('fecha'))}
            aria-describedby={errorDe('fecha') ? 'fecha-error' : undefined}
            {...register('fecha')}
          />
          {errorDe('fecha') && (
            <p id="fecha-error" role="alert" className="mt-1 text-sm text-rose-600 dark:text-rose-400">
              {errorDe('fecha')}
            </p>
          )}
        </div>

        <div>
          <label htmlFor="total" className="etiqueta">
            Total (€)
          </label>
          <input
            id="total"
            type="number"
            step="0.01"
            min="0"
            className="campo"
            aria-invalid={Boolean(errorDe('total'))}
            aria-describedby={errorDe('total') ? 'total-error' : undefined}
            {...register('total', { valueAsNumber: true })}
          />
          {errorDe('total') && (
            <p id="total-error" role="alert" className="mt-1 text-sm text-rose-600 dark:text-rose-400">
              {errorDe('total')}
            </p>
          )}
        </div>

        <div className="sm:col-span-2">
          <label htmlFor="estado" className="etiqueta">
            Estado
          </label>
          <select id="estado" className="campo" {...register('estado')}>
            {ESTADOS.map((valor) => (
              <option key={valor} value={valor}>
                {valor}
              </option>
            ))}
          </select>
          {errorDe('estado') && (
            <p role="alert" className="mt-1 text-sm text-rose-600 dark:text-rose-400">
              {errorDe('estado')}
            </p>
          )}
        </div>
      </div>

      {errorGeneral && (
        <div
          role="alert"
          className="rounded-lg border border-rose-200 bg-rose-50 px-3 py-2 text-sm text-rose-700 dark:border-rose-900 dark:bg-rose-950 dark:text-rose-300"
        >
          {errorGeneral}
        </div>
      )}

      <div className="flex flex-wrap justify-end gap-2">
        <button type="button" className="boton-secundario" onClick={onCancelar} disabled={guardando}>
          Cancelar
        </button>
        <button type="submit" className="boton-primario" disabled={guardando}>
          {guardando ? 'Guardando...' : textoEnviar}
        </button>
      </div>
    </form>
  );
}
