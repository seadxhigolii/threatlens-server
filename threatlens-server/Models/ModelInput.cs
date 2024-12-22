namespace threatlens_server.Models
{
    public class ModelInput
    {
        public string SrcIp { get; set; } // Source IP address
        public string DstIp { get; set; } // Destination IP address
        public int Sport { get; set; }    // Source port
        public int Dsport { get; set; }   // Destination port
        public string Proto { get; set; } // Protocol
        public int Sbytes { get; set; }   // Source bytes
        public int Dbytes { get; set; }   // Destination bytes
        public int Sttl { get; set; }     // Source TTL (Time To Live)
        public bool Label { get; set; }
    }
}
