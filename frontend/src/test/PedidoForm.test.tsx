import { render, screen, waitFor } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { describe, expect, it, vi } from 'vitest';
import { PedidoForm } from '../features/pedidos/PedidoForm';
import type { FormularioPedido } from '../features/pedidos/PedidoForm';

function renderFormulario(onEnviar = vi.fn().mockResolvedValue(undefined)) {
  render(
    <PedidoForm
      onEnviar={onEnviar}
      guardando={false}
      errorGeneral={null}
      textoEnviar="Crear pedido"
      onCancelar={vi.fn()}
    />,
  );

  return onEnviar;
}

describe('PedidoForm', () => {
  it('muestra los errores de los campos obligatorios al enviar vacío', async () => {
    const usuario = userEvent.setup();
    const onEnviar = renderFormulario();

    await usuario.click(screen.getByRole('button', { name: 'Crear pedido' }));

    expect(await screen.findByText('El número de pedido es obligatorio.')).toBeInTheDocument();
    expect(screen.getByText('El cliente es obligatorio.')).toBeInTheDocument();
    expect(screen.getByText('El total debe ser mayor que cero.')).toBeInTheDocument();
    expect(onEnviar).not.toHaveBeenCalled();
  });

  it('rechaza un total igual a cero', async () => {
    const usuario = userEvent.setup();
    const onEnviar = renderFormulario();

    await usuario.type(screen.getByLabelText('Número de pedido'), 'PED-001');
    await usuario.type(screen.getByLabelText('Cliente'), 'Juan Perez');
    await usuario.clear(screen.getByLabelText('Total (€)'));
    await usuario.type(screen.getByLabelText('Total (€)'), '0');
    await usuario.click(screen.getByRole('button', { name: 'Crear pedido' }));

    expect(await screen.findByText('El total debe ser mayor que cero.')).toBeInTheDocument();
    expect(onEnviar).not.toHaveBeenCalled();
  });

  it('envía los datos cuando la validación es correcta', async () => {
    const usuario = userEvent.setup();
    const onEnviar = renderFormulario();

    await usuario.type(screen.getByLabelText('Número de pedido'), 'PED-001');
    await usuario.type(screen.getByLabelText('Cliente'), 'Juan Perez');
    await usuario.clear(screen.getByLabelText('Total (€)'));
    await usuario.type(screen.getByLabelText('Total (€)'), '250.75');
    await usuario.click(screen.getByRole('button', { name: 'Crear pedido' }));

    await waitFor(() => expect(onEnviar).toHaveBeenCalledTimes(1));

    const enviado = onEnviar.mock.calls[0]?.[0] as FormularioPedido;
    expect(enviado.numeroPedido).toBe('PED-001');
    expect(enviado.cliente).toBe('Juan Perez');
    expect(enviado.total).toBe(250.75);
    expect(enviado.estado).toBe('Registrado');
  });

  it('muestra el error general devuelto por la API', () => {
    render(
      <PedidoForm
        onEnviar={vi.fn().mockResolvedValue(undefined)}
        guardando={false}
        errorGeneral="Ya existe un pedido con ese número."
        textoEnviar="Crear pedido"
        onCancelar={vi.fn()}
      />,
    );

    expect(screen.getByRole('alert')).toHaveTextContent('Ya existe un pedido con ese número.');
  });
});
