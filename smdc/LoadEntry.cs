namespace smdc
{
    public class LoadEntry
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public uint Address { get; set; }
        public uint Size { get; set; }
        public uint FileSystem { get; set; }

        public uint FileOffset { get; set; }
        public uint FileSize { get; set; }
        public byte[] Checksum { get; set; }

        public LoadEntry()
        {
            Name = "";
            Source = "";
        }
    }
}
