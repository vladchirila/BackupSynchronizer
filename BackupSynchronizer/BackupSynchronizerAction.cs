using System.IO;

namespace BackupSynchronizer
{
    public class BackupSynchronizerAction
    {
        public static void DoBackup(string sourceFolder, string destFolder, bool doNotDeleteDestFiles)
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
                            fileSystemActions.Remove(action.DestinationNodeElement);
                        break;
                    case ActionType.CopySourceNodeElementToDestination:
                        fileSystemActions.Add(action.DestinationNode, action.SourceNodeElement);
                        break;

                    case ActionType.DeleteDestNode:
                        if (!doNotDeleteDestFiles)
                            fileSystemActions.Remove(action.DestinationNode);
                        break;
                    case ActionType.CopySourceNodeToDestination:
                        fileSystemActions.Add(action.DestinationNode, action.SourceNode);
                        break;
                }
            }
        }
    }
}
