using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechfinCase.Application.Features.Clients.CreateClient;
using TechfinCase.Application.Features.Clients.GetClients;

namespace TechfinCase.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/clientes")]
public sealed class ClientsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateClientCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        if (result.Status == "OK")
        {
            return StatusCode(StatusCodes.Status201Created, new
            {
                idCliente = result.IdCliente,
                status = "OK"
            });
        }

        return BadRequest(new
        {
            status = "ERRO",
            detalheErro = result.DetalheErro
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var clients = await sender.Send(new GetClientsQuery(), cancellationToken);

        return Ok(clients);
    }
}
