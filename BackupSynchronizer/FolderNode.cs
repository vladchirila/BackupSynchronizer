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
            return FolderPath.Exists ? FolderPath.EnumerateFiles().Select(file => new FileNodeElement(file)) : Enumerable.Empty<FileNodeElement>();
        }

        public IEnumerable<FolderNode> GetSubnodes()
        {
            return FolderPath.Exists ? FolderPath.EnumerateDirectories().Select(folder => new FolderNode(folder)) : Enumerable.Empty<FolderNode>();
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
