using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechfinCase.Application.Features.Auth.Login;
using TechfinCase.Application.Features.Auth.Register;

namespace TechfinCase.Api.Controllers.V1;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        if (!result.Success)
        {
            return BadRequest(new
            {
                status = "ERRO",
                detalheErro = result.Error
            });
        }

        return StatusCode(StatusCodes.Status201Created, new { status = "OK" });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        if (result is null)
        {
            return Unauthorized(new
            {
                status = "ERRO",
                detalheErro = "Credenciais inválidas."
            });
        }

        return Ok(result);
    }
}
