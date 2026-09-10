using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechfinCase.Application.Features.Transactions.SimulateTransaction;

namespace TechfinCase.Api.Controllers.V1;

[ApiController]
[Authorize]
[Route("api/v1/transacoes")]
public sealed class TransactionsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Simulate([FromBody] SimulateTransactionCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        if (result.Status == "APROVADO")
        {
            return Ok(new
            {
                status = "APROVADO",
                idTransacao = result.IdTransacao
            });
        }

        return Ok(new { status = "NEGADO" });
    }
}
