using System.IO;

namespace BackupSynchronizer
{
    public class FileNodeElement : INodeElement
    {
        public FileInfo FilePath { get; }

        public FileNodeElement(FileInfo filePath)
        {
            FilePath = filePath;
        }

        public bool IsEqual(INodeElement other)
        {
            if (!(other is FileNodeElement otherFile))
                return false;
            return FilesAreEqual(FilePath, otherFile.FilePath);
        }

        static bool FilesAreEqual(FileInfo sourceFile, FileInfo destFile)
        {
            if (sourceFile.Name != destFile.Name)
                return false;

            if (sourceFile.Length != destFile.Length)
                return false;

            return true;
        }
    }
}
