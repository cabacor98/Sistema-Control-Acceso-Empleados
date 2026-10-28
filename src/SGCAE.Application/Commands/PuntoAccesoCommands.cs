using SGCAE.Application.Common.Dtos;
using SGCAE.Application.Common;
using SGCAE.Domain.Entities;
using SGCAE.Domain.Repositories;
using MediatR;

namespace SGCAE.Application.Commands;

public sealed record RegistrarPuntoAccesoCommand(
    string Nombre,
    string Descripcion) : ICommand<PuntoAccesoDto>;

public sealed record HabilitarPuntoAccesoCommand(Guid PuntoAccesoId) : ICommand<PuntoAccesoDto>;

public sealed record InhabilitarPuntoAccesoCommand(Guid PuntoAccesoId) : ICommand<PuntoAccesoDto>;
