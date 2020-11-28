using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.ServiceBus;
using APICotacoes.Models;

namespace APICotacoes.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CotacoesController : ControllerBase
    {
        private readonly ILogger<CotacoesController> _logger;

        public CotacoesController(ILogger<CotacoesController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(Resultado), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]        
        public async Task<Resultado> Post(
            [FromServices] IConfiguration configuration,
            CotacaoMoeda cotacao)
        {
            var conteudoCotacao = JsonSerializer.Serialize(cotacao);
            _logger.LogInformation($"Dados: {conteudoCotacao}");

            var body = Encoding.UTF8.GetBytes(conteudoCotacao);

            string queue = configuration["Queue-AzureServiceBus"];
            var client = new QueueClient(
                configuration.GetConnectionString("AzureServiceBus"),
                queue);
            await client.SendAsync(new Message(body));
            _logger.LogInformation(
                $"Azure Service Bus - Envio para a queue {queue} concluído");

            return new Resultado()
            {
                Mensagem = "Informações de cotação de moeda enviadas com sucesso!"
            };
        }
    }
}