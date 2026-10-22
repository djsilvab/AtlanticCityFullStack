import { useCallback, useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';
import { useToast } from '../../components/Toast';
import { BadgeEstado } from '../../components/BadgeEstado';
import { Cargando } from '../../components/Cargando';
import { MensajeError } from '../../components/MensajeError';
import { DialogoConfirmacion } from '../../components/DialogoConfirmacion';
import { eliminarPedido, extraerProblema, listarPedidos } from './pedidosApi';
import type { EstadoPedido, Pedido, ProblemaDetalle } from '../../lib/tipos';
import { ESTADOS } from '../../lib/tipos';

const formatearMoneda = (valor: number) =>
  new Intl.NumberFormat('es-ES', { style: 'currency', currency: 'EUR' }).format(valor);

const formatearFecha = (fecha: string) => {
  const date = new Date(fecha);

  return Number.isNaN(date.getTime())
    ? fecha
    : new Intl.DateTimeFormat('es-ES', { dateStyle: 'medium' }).format(date);
};

export function PedidosListPage() {
  const { esAdmin } = useAuth();
  const { mostrar } = useToast();

  const [pedidos, setPedidos] = useState<Pedido[]>([]);
  const [cargando, setCargando] = useState(true);
  const [error, setError] = useState<ProblemaDetalle | null>(null);
  const [cliente, setCliente] = useState('');
  const [estado, setEstado] = useState<EstadoPedido | ''>('');
  const [aEliminar, setAEliminar] = useState<Pedido | null>(null);
  const [eliminando, setEliminando] = useState(false);

  const cargar = useCallback(async () => {
    setCargando(true);
    setError(null);

    try {
      const pagina = await listarPedidos({
        cliente: cliente.trim() || null,
        estado: estado || null,
        pageSize: 50,
      });

      setPedidos(pagina.items);
    } catch (fallo) {
      setError(extraerProblema(fallo));
    } finally {
      setCargando(false);
    }
  }, [cliente, estado]);

  useEffect(() => {
    void cargar();
  }, [cargar]);

  const confirmarEliminacion = async () => {
    if (!aEliminar) {
      return;
    }

    setEliminando(true);

    try {
      await eliminarPedido(aEliminar.id);
      mostrar(`Pedido ${aEliminar.numeroPedido} eliminado.`, 'exito');
      setAEliminar(null);
      await cargar();
    } catch (fallo) {
      const problema = extraerProblema(fallo);
      mostrar(problema?.detail ?? 'No se ha podido eliminar el pedido.', 'error');
    } finally {
      setEliminando(false);
    }
  };

  const hayFiltros = cliente.trim() !== '' || estado !== '';

  return (
    <section className="space-y-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900 dark:text-white">Pedidos</h1>
          <p className="text-sm text-slate-500 dark:text-slate-400">
            Listado de pedidos no eliminados, del más reciente al más antiguo.
          </p>
        </div>

        <Link to="/pedidos/nuevo" className="boton-primario">
          Nuevo pedido
        </Link>
      </div>

      <form
        className="tarjeta flex flex-wrap items-end gap-3 p-4"
        onSubmit={(evento) => {
          evento.preventDefault();
          void cargar();
        }}
        role="search"
        aria-label="Filtros de pedidos"
      >
        <div className="min-w-48 flex-1">
          <label htmlFor="filtro-cliente" className="etiqueta">
            Buscar por cliente
          </label>
          <input
            id="filtro-cliente"
            type="search"
            className="campo"
            placeholder="Nombre del cliente"
            value={cliente}
            onChange={(evento) => setCliente(evento.target.value)}
          />
        </div>

        <div className="min-w-40">
          <label htmlFor="filtro-estado" className="etiqueta">
            Estado
          </label>
          <select
            id="filtro-estado"
            className="campo"
            value={estado}
            onChange={(evento) => setEstado(evento.target.value as EstadoPedido | '')}
          >
            <option value="">Todos</option>
            {ESTADOS.map((valor) => (
              <option key={valor} value={valor}>
                {valor}
              </option>
            ))}
          </select>
        </div>

        <button type="submit" className="boton-secundario">
          Aplicar filtros
        </button>

        {hayFiltros && (
          <button
            type="button"
            className="boton-secundario"
            onClick={() => {
              setCliente('');
              setEstado('');
            }}
          >
            Limpiar
          </button>
        )}
      </form>

      {cargando && <Cargando etiqueta="Cargando pedidos..." />}

      {!cargando && error && (
        <MensajeError
          titulo="No se ha podido cargar el listado de pedidos."
          descripcion={error.detail ?? 'Comprueba que la API está disponible.'}
          onReintentar={() => void cargar()}
        />
      )}

      {!cargando && !error && pedidos.length === 0 && (
        <div className="tarjeta flex flex-col items-center gap-3 p-10 text-center">
          <p className="text-lg font-semibold text-slate-800 dark:text-slate-100">
            {hayFiltros ? 'Ningún pedido coincide con los filtros' : 'Todavía no hay pedidos'}
          </p>
          <p className="text-sm text-slate-500 dark:text-slate-400">
            {hayFiltros
              ? 'Prueba a cambiar el cliente o el estado filtrados.'
              : 'Crea el primer pedido para empezar a trabajar.'}
          </p>
          {!hayFiltros && (
            <Link to="/pedidos/nuevo" className="boton-primario">
              Nuevo pedido
            </Link>
          )}
        </div>
      )}

      {!cargando && !error && pedidos.length > 0 && (
        <div className="tarjeta overflow-hidden">
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-slate-200 text-sm dark:divide-slate-800">
              <caption className="sr-only">Listado de pedidos</caption>
              <thead className="bg-slate-50 text-left text-xs font-semibold uppercase tracking-wide text-slate-500 dark:bg-slate-800/60 dark:text-slate-400">
                <tr>
                  <th scope="col" className="px-4 py-3">Número</th>
                  <th scope="col" className="px-4 py-3">Cliente</th>
                  <th scope="col" className="px-4 py-3">Fecha</th>
                  <th scope="col" className="px-4 py-3 text-right">Total</th>
                  <th scope="col" className="px-4 py-3">Estado</th>
                  <th scope="col" className="px-4 py-3 text-right">Acciones</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100 dark:divide-slate-800">
                {pedidos.map((pedido) => (
                  <tr key={pedido.id} className="hover:bg-slate-50 dark:hover:bg-slate-800/40">
                    <td className="px-4 py-3 font-medium text-slate-900 dark:text-white">{pedido.numeroPedido}</td>
                    <td className="px-4 py-3 text-slate-600 dark:text-slate-300">{pedido.cliente}</td>
                    <td className="px-4 py-3 text-slate-600 dark:text-slate-300">{formatearFecha(pedido.fecha)}</td>
                    <td className="px-4 py-3 text-right font-medium text-slate-900 dark:text-white">
                      {formatearMoneda(pedido.total)}
                    </td>
                    <td className="px-4 py-3">
                      <BadgeEstado estado={pedido.estado} />
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex justify-end gap-2">
                        <Link
                          to={`/pedidos/${pedido.id}/editar`}
                          className="boton-secundario px-3 py-1.5"
                        >
                          Editar
                        </Link>
                        {esAdmin && (
                          <button
                            type="button"
                            className="boton-peligro px-3 py-1.5"
                            onClick={() => setAEliminar(pedido)}
                          >
                            Eliminar
                          </button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      <DialogoConfirmacion
        abierto={aEliminar !== null}
        titulo="Eliminar pedido"
        descripcion={`¿Seguro que quieres eliminar el pedido ${aEliminar?.numeroPedido ?? ''}? La eliminación es lógica: el registro se conserva.`}
        textoConfirmar="Eliminar"
        peligro
        procesando={eliminando}
        onCancelar={() => setAEliminar(null)}
        onConfirmar={() => void confirmarEliminacion()}
      />
    </section>
  );
}
