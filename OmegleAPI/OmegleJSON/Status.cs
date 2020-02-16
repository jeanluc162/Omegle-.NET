using System.Collections.Generic;

namespace OmegleAPI.OmegleJSON
{
    internal class Status
    {
        public int count { get; set; }
        public List<string> antinudeservers { get; set; }
        public double spyQueueTime { get; set; }
        public string rtmfp { get; set; }
        public double antinudepercent { get; set; }
        public double spyeeQueueTime { get; set; }
        public double timestamp { get; set; }
        public List<string> servers { get; set; }
    }
}
