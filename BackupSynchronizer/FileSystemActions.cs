using System.IO;

namespace BackupSynchronizer
{
    public class FileSystemActions : INodeActions<FolderNode, FolderNode, FileNodeElement>
    {
        public void Add(FolderNode node, FolderNode subnode)
        {
            var folderName = subnode.FolderPath.Name;
            var folderPath = node.FolderPath.FullName;
            var newFolderPath = Path.Combine(folderPath, folderName);

            var destination = new DirectoryInfo(newFolderPath);

            CopyDir(subnode.FolderPath, destination);
        }

        private static void CopyDir(DirectoryInfo source, DirectoryInfo target)
        {
            Directory.CreateDirectory(target.FullName);

            foreach (var fi in source.GetFiles())
            {
                fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
            }

            foreach (var diSourceSubDir in source.GetDirectories())
            {
                var nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);
                CopyDir(diSourceSubDir, nextTargetSubDir);
            }
        }

        public void Add(FolderNode node, FileNodeElement element)
        {
            var fileName = element.FilePath.Name;
            var folderPath = node.FolderPath.FullName;
            var newFilePath = Path.Combine(folderPath, fileName);

            element.FilePath.CopyTo(newFilePath, true);
        }

        public void Remove(FolderNode node)
        {
            node.FolderPath.Attributes &= ~FileAttributes.ReadOnly;
            node.FolderPath.Delete(true);
        }

        public void Remove(FileNodeElement nodeElement)
        {
            nodeElement.FilePath.Attributes &= ~FileAttributes.ReadOnly;
            nodeElement.FilePath.Delete();
        }
    }
}
