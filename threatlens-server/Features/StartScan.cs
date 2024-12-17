using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace threatlens_server.Features
{
    public class StartScan
    {
        [HttpPost("/api/start-scan")]
        public async Task<ActionResult<int>> Create(StartScanCommand command)
        {
            return await Mediator.Send(command);
        }
    }
    public record StartScanCommand() : IRequest<int>;
}
