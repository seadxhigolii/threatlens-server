using Confluent.Kafka;
using Microsoft.ML;
using PacketDotNet;
using SharpPcap;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using threatlens_server.Models;

namespace threatlens_server.Services
{
    public class PacketCaptureService
    {
        private ICaptureDevice _device;
        private IConfiguration _configuration;
        private readonly MlModelService _mlModelService;
        private bool _isCapturing;

        public PacketCaptureService(IConfiguration configuration, MlModelService mlModelService)
        {
            _configuration = configuration;
            _mlModelService = mlModelService;
        }

        public async Task StartCaptureAsync(CancellationToken cancellationToken)
        {
            if (_isCapturing)
            {
                return;
            }

            var devices = CaptureDeviceList.Instance;
            if (devices.Count < 1)
            {
                Debug.WriteLine("No capture devices found.");
                return;
            }

            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            //\Device\NPF_{3002FBDD-BD00-4CFB-956C-4CF632CC3CA9}
            var targetInterface = networkInterfaces.FirstOrDefault(ni =>
            {
                var ipProperties = ni.GetIPProperties();
                return ipProperties.UnicastAddresses.Any(ua =>
                    ua.Address.AddressFamily == AddressFamily.InterNetwork && // IPv4
                    ua.Address.ToString() == "192.168.100.80" ||
                    ua.Address.AddressFamily == AddressFamily.InterNetworkV6 && // IPv6
                    ua.Address.ToString() == "fe80::d0fe:ff94:1a8c:150d%8");
            });

            if (targetInterface == null)
            {
                Debug.WriteLine("No network interface found with the specified IP address.");
                return;
            }

            string deviceName = $"\\Device\\NPF_{targetInterface.Id}";

            _device = devices.FirstOrDefault(device => device.Name.Contains(deviceName));
            if (_device == null)
            {
                Debug.WriteLine("No capture device found for the specified network interface.");
                return;
            }

            Debug.WriteLine($"Listening on device: {_device.Description}");

            ConfigureDevice();
            _device.Open();
            _device.StartCapture();
            _isCapturing = true;

            await Task.Run(() => WaitUntilStopped(cancellationToken), cancellationToken);
        }
        public void StopCapture()
        {
            if (!_isCapturing)
            {
                return;
            }

            _device.StopCapture();
            _device.Close();
            _isCapturing = false;
        }

        private void ConfigureDevice()
        {
            _device.OnPacketArrival += OnPacketArrival;

            var config = new DeviceConfiguration
            {
                Mode = DeviceModes.Promiscuous,
                ReadTimeout = 1000,
                Snaplen = 65536
            };

            _device.Open(config);
        }

        private void WaitUntilStopped(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested && _isCapturing)
            {
                Thread.Sleep(500);
            }
        }

        private void OnPacketArrival(object sender, PacketCapture e)
        {
            var rawPacket = Packet.ParsePacket(e.GetPacket().LinkLayerType, e.GetPacket().Data);
            var tcpPacket = rawPacket.Extract<TcpPacket>();
            var ipPacket = rawPacket.Extract<IPPacket>();

            if (tcpPacket != null && ipPacket != null)
            {
                var inputData = new ModelInput
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

                

                var prediction = _mlModelService.Predict(inputData);

                if (prediction.Prediction)
                {
                    Debug.WriteLine($"Anomaly detected! SrcIp: {inputData.SrcIp}, Score: {prediction.Score}");
                }
                else
                {
                    Debug.WriteLine($"Normal traffic. SrcIp: {inputData.SrcIp}, Score: {prediction.Score}");
                }
            }

        }
    }
}
