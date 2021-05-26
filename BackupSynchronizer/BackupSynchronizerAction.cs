using System;
using System.IO;

namespace BackupSynchronizer
{
    public class BackupSynchronizerAction
    {
        public void DoBackup(string sourceFolder, string destFolder, bool doNotDeleteDestFiles)
        {
            var sourceNode = new FolderNode(new DirectoryInfo(sourceFolder));
            var destNode = new FolderNode(new DirectoryInfo(destFolder));

            var synchronizer = new Synchronizer();
            var actions = synchronizer.Synchronize<FolderNode, FileNodeElement>(sourceNode, destNode);

            var fileSystemActions = new FileSystemActions();

            foreach (var action in actions)
            {
                switch (action.ActionType)
                {
                    case ActionType.DeleteDestNodeElement:
                        if (!doNotDeleteDestFiles)
                        {
                            OnFileDelete?.Invoke(this, action.DestinationNodeElement);
                            fileSystemActions.Remove(action.DestinationNodeElement);
                        }
                        break;
                    case ActionType.CopySourceNodeElementToDestination:
                        OnFileCopy?.Invoke(this, new FileCopyArgs(action.SourceNodeElement, action.DestinationNode));
                        fileSystemActions.Add(action.DestinationNode, action.SourceNodeElement);
                        break;

                    case ActionType.DeleteDestNode:
                        if (!doNotDeleteDestFiles)
                        {
                            OnFolderDelete?.Invoke(this, action.DestinationNode);
                            fileSystemActions.Remove(action.DestinationNode);
                        }
                        break;
                    case ActionType.CopySourceNodeToDestination:
                        OnFolderCopy?.Invoke(this, new FolderCopyArgs(action.SourceNode, action.DestinationNode));
                        fileSystemActions.Add(action.DestinationNode, action.SourceNode);
                        break;
                }
            }
        }

        public event EventHandler<FileNodeElement> OnFileDelete;
        public event EventHandler<FolderNode> OnFolderDelete;
        public event EventHandler<FileCopyArgs> OnFileCopy;
        public event EventHandler<FolderCopyArgs> OnFolderCopy;

        public class FileCopyArgs
        {
            public FileCopyArgs(FileNodeElement fileToCopy, FolderNode destinationFolder)
            {
                FileToCopy = fileToCopy;
                DestinationFolder = destinationFolder;
            }

            public readonly FileNodeElement FileToCopy;
            public readonly FolderNode DestinationFolder;
        }

        public class FolderCopyArgs
        {
            public FolderCopyArgs(FolderNode folderToCopy, FolderNode destinationFolder)
            {
                FolderToCopy = folderToCopy;
                DestinationFolder = destinationFolder;
            }

            public readonly FolderNode FolderToCopy;
            public readonly FolderNode DestinationFolder;
        }
    }
}
