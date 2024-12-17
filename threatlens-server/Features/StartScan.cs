using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using threatlens_server.Common;
using SharpPcap;
using PacketDotNet;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ML;
using System.Linq;

namespace threatlens_server.Features
{
    public class StartScan : ApiControllerBase
    {
        [HttpPost("/api/start-scan")]
        public async Task<ActionResult<int>> Create(StartScanCommand command)
        {
            return await Mediator.Send(command);
        }
    }
    public record StartScanCommand() : IRequest<int>;

    internal sealed class StartScanCommandHandler : IRequestHandler<StartScanCommand, int>
    {
        private readonly MlModelLoader _modelLoader = new();

        public async Task<int> Handle(StartScanCommand request, CancellationToken cancellationToken)
        {
            var devices = CaptureDeviceList.Instance;
            if (devices.Count < 1)
            {
                Console.WriteLine("No devices were found on this machine.");
                return -1;
            }

            var device = devices[0];
            Console.WriteLine($"Listening on device: {device.Description}");

            device.OnPacketArrival += new PacketArrivalEventHandler(this.OnPacketArrival);

            var config = new DeviceConfiguration
            {
                Mode = DeviceModes.Promiscuous,
                ReadTimeout = 1000,            
                Snaplen = 65536            
            };

            device.Open(config);

            device.StartCapture();

            await Task.Run(() => Console.ReadLine(), cancellationToken);

            device.StopCapture();
            device.Close();

            return 1;
        }

        private void OnPacketArrival(object sender, PacketCapture e)
        {
            var rawPacket = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
            var tcpPacket = rawPacket.Extract<TcpPacket>();
            var ipPacket = rawPacket.Extract<IPPacket>();

            if (tcpPacket != null && ipPacket != null)
            {
                var inputData = new Models.InputData
                {
                    SrcIp = ipPacket.SourceAddress.ToString(),
                    DstIp = ipPacket.DestinationAddress.ToString(),
                    Sport = tcpPacket.SourcePort,
                    Dsport = tcpPacket.DestinationPort,
                    Proto = ipPacket.Protocol.ToString(),
                    Sbytes = tcpPacket.PayloadData.Length,
                    Dbytes = tcpPacket.TotalPacketLength,
                    Sttl = ipPacket.TimeToLive
                };

                bool isAnomaly = _modelLoader.Predict(inputData);
                Console.WriteLine(isAnomaly
                    ? $"[ALERT] Anomaly Detected: {inputData.SrcIp} -> {inputData.DstIp}"
                    : $"Normal Packet: {inputData.SrcIp} -> {inputData.DstIp}");
            }
        }

    }
}
