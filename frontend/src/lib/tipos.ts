export type EstadoPedido = 'Registrado' | 'EnProceso' | 'Completado' | 'Cancelado';

export type Rol = 'User' | 'Admin';

export interface Pedido {
  id: number;
  numeroPedido: string;
  cliente: string;
  fecha: string;
  total: number;
  estado: EstadoPedido;
}

export interface PaginaPedidos {
  items: Pedido[];
  pagina: number;
  pageSize: number;
  total: number;
  totalPaginas: number;
}

export interface LoginResponse {
  token: string;
  expiresIn: number;
}

export interface UsuarioSesion {
  email: string;
  rol: Rol;
  expiraEn: number;
}

export interface PayloadJwt {
  sub?: string;
  email?: string;
  role?: Rol;
  exp?: number;
  iat?: number;
  jti?: string;
}

/** Errores de la API en formato ProblemDetails (RFC 7807). */
export interface ProblemaDetalle {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}

export const ESTADOS: EstadoPedido[] = ['Registrado', 'EnProceso', 'Completado', 'Cancelado'];
