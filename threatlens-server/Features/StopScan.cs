using MediatR;
using Microsoft.AspNetCore.Mvc;
using threatlens_server.Common;
using threatlens_server.Services;

namespace threatlens_server.Features
{
    public class StopScan : ApiControllerBase
    {
        [HttpPost("/api/stop-scan")]
        public async Task<ActionResult<bool>> Stop()
        {
            return await Mediator.Send(new StopScanCommand());
        }
    }

    public record StopScanCommand() : IRequest<bool>;

    internal sealed class StopScanCommandHandler : IRequestHandler<StopScanCommand, bool>
    {
        private readonly PacketCaptureService _packetCaptureService;

        public StopScanCommandHandler(PacketCaptureService packetCaptureService)
        {
            _packetCaptureService = packetCaptureService;
        }

        public Task<bool> Handle(StopScanCommand request, CancellationToken cancellationToken)
        {
            _packetCaptureService.StopCapture();
            return Task.FromResult(true);
        }
    }
}
