using System.IO;

namespace Edulink.Models
{
    public class SharingFile
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }

        public SharingFile(string filePath)
        {
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
            FileSize = new FileInfo(filePath).Length;
        }
    }
}
