import axios from 'axios';
import { api } from '../../lib/api';
import type { PaginaPedidos, Pedido, ProblemaDetalle } from '../../lib/tipos';

export interface PedidoPayload {
  numeroPedido: string;
  cliente: string;
  fecha: string;
  total: number;
  estado: Pedido['estado'];
}

export interface ConsultaPedidos {
  pagina?: number;
  pageSize?: number;
  estado?: Pedido['estado'] | null;
  cliente?: string | null;
}

/** Extrae el ProblemDetails de una respuesta de error de la API. */
export function extraerProblema(error: unknown): ProblemaDetalle | null {
  if (axios.isAxiosError<ProblemaDetalle>(error)) {
    return error.response?.data ?? null;
  }

  return null;
}

export async function listarPedidos(consulta: ConsultaPedidos = {}): Promise<PaginaPedidos> {
  const parametros: Record<string, string | number> = {
    pagina: consulta.pagina ?? 1,
    pageSize: consulta.pageSize ?? 20,
  };

  if (consulta.estado) {
    parametros.estado = consulta.estado;
  }

  if (consulta.cliente) {
    parametros.cliente = consulta.cliente;
  }

  const respuesta = await api.get<PaginaPedidos>('/api/pedidos/paginado', { params: parametros });

  return respuesta.data;
}

export async function obtenerPedido(id: number): Promise<Pedido> {
  const respuesta = await api.get<Pedido>(`/api/pedidos/${id}`);

  return respuesta.data;
}

export async function crearPedido(payload: PedidoPayload): Promise<Pedido> {
  const respuesta = await api.post<Pedido>('/api/pedidos', payload);

  return respuesta.data;
}

export async function actualizarPedido(id: number, payload: PedidoPayload): Promise<Pedido> {
  const respuesta = await api.put<Pedido>(`/api/pedidos/${id}`, payload);

  return respuesta.data;
}

export async function eliminarPedido(id: number): Promise<void> {
  await api.delete(`/api/pedidos/${id}`);
}
