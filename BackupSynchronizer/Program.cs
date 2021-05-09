using System;
using System.IO;

namespace BackupSynchronizer
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO: extras din args sursa si destinatia mai bine
            var source = args[0];
            var dest = args[1];
            var doNotDeleteDestFiles = false;
            if (args.Length > 2)
                if (args[3] == "-keepDestinationFilesMissingFromSource" || args[3] == "-incremental")
                    doNotDeleteDestFiles = true;

            if (!Directory.Exists(source))
                throw new Exception("Source directory not found.");

            if (!Directory.Exists(dest))
                throw new Exception("Destination directory not found.");

            var sourceNode = new FolderNode(new DirectoryInfo(source));
            var destNode = new FolderNode(new DirectoryInfo(dest));

            var synchronizer = new Synchronizer();
            var actions = synchronizer.Synchronize(sourceNode, destNode);

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
