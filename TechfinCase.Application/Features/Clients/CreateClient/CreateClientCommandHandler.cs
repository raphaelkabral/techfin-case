using MediatR;
using TechfinCase.Application.Abstractions;
using TechfinCase.Application.Common;
using TechfinCase.Domain.Entities;

namespace TechfinCase.Application.Features.Clients.CreateClient;

public sealed class CreateClientCommandHandler(
    IClientRepository clientRepository,
    ICacheService cacheService)
    : IRequestHandler<CreateClientCommand, CreateClientResponse>
{
    public async Task<CreateClientResponse> Handle(CreateClientCommand request, CancellationToken cancellationToken)
    {
        if (request.ValorLimite < 0)
        {
            return Error("Não é permitido cadastrar cliente com limite negativo.");
        }

        if (string.IsNullOrWhiteSpace(request.Nome) || string.IsNullOrWhiteSpace(request.Cpf))
        {
            return Error("Nome e CPF são obrigatórios.");
        }

        var cpf = NormalizeCpf(request.Cpf);

        if (string.IsNullOrWhiteSpace(cpf))
        {
            return Error("CPF inválido.");
        }

        var existingClient = await clientRepository.GetByCpfAsync(cpf, cancellationToken);

        if (existingClient is not null)
        {
            return Error("Cliente já cadastrado.");
        }

        var client = new Client
        {
            Name = request.Nome.Trim(),
            Cpf = cpf,
            CreditLimit = request.ValorLimite
        };

        await clientRepository.AddAsync(client, cancellationToken);
        cacheService.Remove(CacheKeys.AllClients);

        return new CreateClientResponse(client.Id.ToString(), "OK");
    }

    private static string NormalizeCpf(string cpf) => new(cpf.Where(char.IsDigit).ToArray());

    private static CreateClientResponse Error(string detail) => new(null, "ERRO", detail);
}
