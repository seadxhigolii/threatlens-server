using MediatR;
using Microsoft.AspNetCore.Mvc;
using threatlens_server.Common;
using threatlens_server.Services;

namespace threatlens_server.Features
{
    public class StartScan : ApiControllerBase
    {
        [HttpPost("/api/start-scan")]
        public async Task<ActionResult<bool>> Start()
        {
            return await Mediator.Send(new StartScanCommand());
        }
    }

    public record StartScanCommand() : IRequest<bool>;

    internal sealed class StartScanCommandHandler : IRequestHandler<StartScanCommand, bool>
    {
        private readonly PacketCaptureService _packetCaptureService;

        public StartScanCommandHandler(PacketCaptureService packetCaptureService)
        {
            _packetCaptureService = packetCaptureService;
        }

        public async Task<bool> Handle(StartScanCommand request, CancellationToken cancellationToken)
        {
            await _packetCaptureService.StartCaptureAsync(cancellationToken);
            return true;
        }
    }
}
