import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { PedidoForm } from './PedidoForm';
import type { FormularioPedido } from './PedidoForm';
import { crearPedido, extraerProblema } from './pedidosApi';
import { useToast } from '../../components/Toast';
import type { ProblemaDetalle } from '../../lib/tipos';

export function NuevoPedidoPage() {
  const navegar = useNavigate();
  const { mostrar } = useToast();

  const [guardando, setGuardando] = useState(false);
  const [errorGeneral, setErrorGeneral] = useState<string | null>(null);
  const [erroresServidor, setErroresServidor] = useState<Record<string, string[]>>({});

  const onEnviar = async (valores: FormularioPedido) => {
    setGuardando(true);
    setErrorGeneral(null);
    setErroresServidor({});

    try {
      const pedido = await crearPedido({
        numeroPedido: valores.numeroPedido,
        cliente: valores.cliente,
        fecha: new Date(valores.fecha).toISOString(),
        total: valores.total,
        estado: valores.estado,
      });

      mostrar(`Pedido ${pedido.numeroPedido} creado.`, 'exito');
      navegar('/pedidos');
    } catch (fallo) {
      const problema: ProblemaDetalle | null = extraerProblema(fallo);

      if (problema?.errors) {
        setErroresServidor(problema.errors);
      }

      setErrorGeneral(
        problema?.status === 409
          ? (problema.detail ?? 'Ya existe un pedido con ese número.')
          : (problema?.detail ?? 'No se ha podido crear el pedido.'),
      );
    } finally {
      setGuardando(false);
    }
  };

  return (
    <section className="mx-auto max-w-2xl space-y-5">
      <div>
        <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white">Nuevo pedido</h1>
        <p className="text-sm text-slate-500 dark:text-slate-400">
          Completa los datos del pedido. El total debe ser mayor que cero.
        </p>
      </div>

      <PedidoForm
        onEnviar={onEnviar}
        guardando={guardando}
        errorGeneral={errorGeneral}
        erroresServidor={erroresServidor}
        textoEnviar="Crear pedido"
        onCancelar={() => navegar('/pedidos')}
      />
    </section>
  );
}
