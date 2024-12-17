namespace threatlens_server.Models
{
    public class InputData
    {
        public string SrcIp { get; set; }
        public int Sport { get; set; }
        public string DstIp { get; set; }
        public int Dsport { get; set; }
        public string Proto { get; set; }
        public int Sbytes { get; set; }
        public int Dbytes { get; set; }
        public int Sttl { get; set; }
        public bool Label { get; set; }
    }
}
