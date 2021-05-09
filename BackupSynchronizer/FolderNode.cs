using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BackupSynchronizer
{
    public class FolderNode : INode<FolderNode, FileNodeElement>
    {
        public DirectoryInfo FolderPath { get; }

        public FolderNode(DirectoryInfo folderPath)
        {
            FolderPath = folderPath;
        }

        public IEnumerable<FileNodeElement> GetElements()
        {
            return FolderPath.EnumerateFiles().Select(file => new FileNodeElement(file));
        }

        public IEnumerable<FolderNode> GetSubnodes()
        {
            return FolderPath.EnumerateDirectories().Select(folder => new FolderNode(folder));
        }

        public bool IsEqual(FolderNode other)
        {
            return FoldersAreEqual(FolderPath, other.FolderPath);
        }

        static bool FoldersAreEqual(DirectoryInfo sourceFolder, DirectoryInfo destFolder)
        {
            return sourceFolder.Name == destFolder.Name;
        }
    }
}
