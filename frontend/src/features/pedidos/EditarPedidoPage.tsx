import { useEffect, useMemo, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { PedidoForm } from './PedidoForm';
import type { FormularioPedido } from './PedidoForm';
import { valoresIniciales } from './PedidoForm';
import { actualizarPedido, extraerProblema, obtenerPedido } from './pedidosApi';
import { Cargando } from '../../components/Cargando';
import { MensajeError } from '../../components/MensajeError';
import { useToast } from '../../components/Toast';
import type { ProblemaDetalle } from '../../lib/tipos';

export function EditarPedidoPage() {
  const { id } = useParams<{ id: string }>();
  const navegar = useNavigate();
  const { mostrar } = useToast();

  const [pedidoId, setPedidoId] = useState<number | null>(null);
  const [valores, setValores] = useState<FormularioPedido>(valoresIniciales);
  const [cargando, setCargando] = useState(true);
  const [guardando, setGuardando] = useState(false);
  const [error, setError] = useState<ProblemaDetalle | null>(null);
  const [errorGeneral, setErrorGeneral] = useState<string | null>(null);
  const [erroresServidor, setErroresServidor] = useState<Record<string, string[]>>({});

  useEffect(() => {
    const identificador = Number(id);

    if (!Number.isFinite(identificador) || identificador <= 0) {
      setError({ detail: 'El identificador del pedido no es válido.' });
      setCargando(false);
      return;
    }

    const cargar = async () => {
      setCargando(true);
      setError(null);

      try {
        const pedido = await obtenerPedido(identificador);

        setPedidoId(pedido.id);
        setValores({
          numeroPedido: pedido.numeroPedido,
          cliente: pedido.cliente,
          fecha: pedido.fecha.slice(0, 10),
          total: pedido.total,
          estado: pedido.estado,
        });
      } catch (fallo) {
        setError(extraerProblema(fallo));
      } finally {
        setCargando(false);
      }
    };

    void cargar();
  }, [id]);

  const valoresFormulario = useMemo(() => valores, [valores]);

  const onEnviar = async (datos: FormularioPedido) => {
    if (pedidoId === null) {
      return;
    }

    setGuardando(true);
    setErrorGeneral(null);
    setErroresServidor({});

    try {
      const pedido = await actualizarPedido(pedidoId, {
        numeroPedido: datos.numeroPedido,
        cliente: datos.cliente,
        fecha: new Date(datos.fecha).toISOString(),
        total: datos.total,
        estado: datos.estado,
      });

      mostrar(`Pedido ${pedido.numeroPedido} actualizado.`, 'exito');
      navegar('/pedidos');
    } catch (fallo) {
      const problema: ProblemaDetalle | null = extraerProblema(fallo);

      if (problema?.errors) {
        setErroresServidor(problema.errors);
      }

      setErrorGeneral(
        problema?.status === 409
          ? (problema.detail ?? 'Ya existe un pedido con ese número.')
          : (problema?.detail ?? 'No se ha podido actualizar el pedido.'),
      );
    } finally {
      setGuardando(false);
    }
  };

  if (cargando) {
    return <Cargando etiqueta="Cargando pedido..." />;
  }

  if (error) {
    return (
      <section className="mx-auto max-w-2xl space-y-4">
        <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white">Editar pedido</h1>
        <MensajeError
          titulo="No se ha podido cargar el pedido."
          descripcion={error.detail ?? 'El pedido no existe o ha sido eliminado.'}
          onReintentar={() => navegar('/pedidos')}
        />
      </section>
    );
  }

  return (
    <section className="mx-auto max-w-2xl space-y-5">
      <div>
        <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white">Editar pedido</h1>
        <p className="text-sm text-slate-500 dark:text-slate-400">
          Modifica los datos y guarda los cambios.
        </p>
      </div>

      <PedidoForm
        valoresInicialesFormulario={valoresFormulario}
        onEnviar={onEnviar}
        guardando={guardando}
        errorGeneral={errorGeneral}
        erroresServidor={erroresServidor}
        textoEnviar="Guardar cambios"
        onCancelar={() => navegar('/pedidos')}
      />
    </section>
  );
}
