using System.Collections.Generic;

namespace smdc
{
    public class SmdScript
    {
        public string Device { get; set; }
        public string Version { get; set; }
        public List<LoadEntry> Entries { get; private set; }

        public SmdScript()
        {
            Device = "";
            Version = "";
            Entries = new List<LoadEntry>();
        }
    }
}
