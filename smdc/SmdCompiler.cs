using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace smdc
{
    public class SmdCompiler
    {
        private static void CopyToStream(Stream fs, string file, int size = 0)
        {
            FileStream fs2 = new FileStream(file, FileMode.Open);
            byte[] buffer = new byte[4096];
            long total = size == 0 ? fs2.Length : size;
            long offset = 0;

            size = size == 0 ? 4096 : Math.Min(size, 4096);

            while (offset < total)
            {
                int read = fs2.Read(buffer, 0, size);

                offset += read;
                fs.Write(buffer, 0, read);

                if (read == 0)
                    break;
            }

            fs2.Close();
        }

        private static byte[] CalculateHash(string fileLocation)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(fileLocation))
                {
                    byte[] b = md5.ComputeHash(stream);
                    stream.Close();
                    return b;
                }
            }
        }
        private static byte[] CalculateHash(Stream stream)
        {
            using (var md5 = MD5.Create())
                return md5.ComputeHash(stream);
        }

        private static uint GetFileSize(string p)
        {
            using (FileStream fs = new FileStream(p, FileMode.Open))
                return (uint)fs.Length;
        }

        public string Input { get; set; }

        public string Output { get; set; }

        public void Compile()
        {
            SmdScript script;
            Stopwatch clock = new Stopwatch();
            
            clock.Start();

            if (Output == null)
                Output = Path.Combine(Path.GetDirectoryName(Input), Path.GetFileNameWithoutExtension(Input) + ".smd");

            Console.WriteLine("Parsing script...");

            using (StreamReader reader = new StreamReader(Input))
            {
                SmdScanner scanner = new SmdScanner(reader);
                SmdParser parser = new SmdParser(scanner);
                script = parser.Parse();
            }

            if (script.Device.Length > 16)
                throw new Exception("Device name too big");

            if (script.Version.Length > 8)
                throw new Exception("Device name too big");

            uint offset = 0x200 + (uint)script.Entries.Count * 0x40;

            Console.WriteLine("Preparing files...");
            foreach (LoadEntry entry in script.Entries)
            {
                Console.WriteLine("Preparing {0}...", Path.GetFileName(entry.Source));

                entry.Source = MakePath(entry.Source);

                if (!File.Exists(entry.Source))
                    throw new Exception("File not found: " + entry.Source);

                if (entry.Name.Length > 16)
                    throw new Exception("Entry name too big: " + entry.Name);

                entry.FileOffset = offset;
                entry.FileSize = GetFileSize(entry.Source);
                entry.Checksum = CalculateHash(entry.Source);

                if (entry.Size == 0)
                    entry.Size = (entry.FileSize - 1) / 512 + 1;

                offset += entry.FileSize;
            }

            using (FileStream fs = new FileStream(Output, FileMode.Create))
            {
                BinaryWriter bw = new BinaryWriter(fs, Encoding.ASCII, true);

                bw.Write(Encoding.ASCII.GetBytes("WP8_EMMC_MainOS\0"));
                bw.Write(Encoding.ASCII.GetBytes(script.Device.PadRight(16, '\0')));
                bw.Write(0x5F738400);
                bw.Write(0x00000000);
                bw.Write(0x16161616);
                bw.Write(script.Entries.Count);
                bw.Write(0x00200C00);
                bw.Write(0x00000000);
                bw.Write(Encoding.ASCII.GetBytes(script.Version.PadRight(8, '\0')));
                bw.Write(0x00000000);
                bw.Write(0x2ECAD88D);
                bw.Write(0x00000000);
                bw.Write(0x00000000);
                bw.Write(new byte[Math.Max(0, 0x200 - fs.Position)]);

                for (int i = 0; i < script.Entries.Count; i++)
                {
                    LoadEntry entry = script.Entries[i];

                    bw.Write(Encoding.ASCII.GetBytes(entry.Name.PadRight(16, '\0')));
                    bw.Write(entry.Address);
                    bw.Write(entry.Size);
                    bw.Write(entry.FileOffset);
                    bw.Write(entry.FileSize);
                    bw.Write(0x1F1F1F1F);
                    bw.Write(i);
                    bw.Write(entry.FileSystem);
                    bw.Write(0x00000000);
                    bw.Write(entry.Checksum);
                }

                bw.Close();

                foreach (LoadEntry entry in script.Entries)
                {
                    Console.WriteLine("Building {0}...", Path.GetFileName(entry.Source));
                    CopyToStream(fs, entry.Source);
                }


                Console.WriteLine("Generating code...");
                fs.Flush();
                fs.Position = 0;
                byte[] headerHash = CalculateHash(fs);
                fs.Position = 0x50;
                fs.Write(headerHash, 0, 0x10);

                fs.Close();

                clock.Stop();
                Console.WriteLine("Done in {0} seconds.", clock.ElapsedMilliseconds / 1000);
            }
        }

        private string MakePath(string p)
        {
            if (Path.IsPathRooted(p))
                return p;

            return Path.Combine(Path.GetDirectoryName(Input), p);
        }
    }
}
