using Fintech.Records;
using Microsoft.AspNetCore.Mvc;

namespace Fintech.Controllers;

[ApiController]
[Route("api/spi")]
public class PixController : ControllerBase
{
    private static readonly Random _random = new();

    [HttpPost("payment")]
    public async Task<IActionResult> ProcessPayment([FromBody] ExternalPixRequest request)
    {
        // 1. Simula Latência de Rede (Chaos: Latency)
        // 10% das requisições vão demorar 10 segundos (Timeout)
        if (_random.Next(1, 100) <= 10)
        {
            await Task.Delay(10000); 
            return StatusCode(504, "Gateway Timeout simulado");
        }

        // Delay normal de processamento (100ms a 500ms)
        await Task.Delay(_random.Next(100, 500));

        // 2. Simula Falha Intermitente (Chaos: Error)
        // 15% das requisições vão falhar com erro 500
        if (_random.Next(1, 100) <= 15)
        {
            return StatusCode(500, "Erro interno do Banco Central");
        }

        // 3. Simula Rejeição de Negócio (Ex: Chave Pix não existe)
        // 5% das requisições retornam 400 Bad Request
        if (_random.Next(1, 100) <= 5)
        {
            return BadRequest(new { Code = "PIX_KEY_NOT_FOUND" });
        }

        // Sucesso! Devolve o EndToEndId (ID oficial da transação)
        return Ok(new { EndToEndId = Guid.NewGuid().ToString("N"), Status = "LIQUIDATED" });
    }
}