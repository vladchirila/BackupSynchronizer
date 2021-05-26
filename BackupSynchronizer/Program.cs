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

            var backupSynchronizerAction = new BackupSynchronizerAction();
            backupSynchronizerAction.OnFileCopy += (sender, args) => Console.WriteLine($"Copying file {args.FileToCopy.FilePath} to {args.DestinationFolder.FolderPath}...");
            backupSynchronizerAction.OnFolderCopy += (sender, args) => Console.WriteLine($"Copying folder {args.FolderToCopy.FolderPath} to {args.DestinationFolder.FolderPath}...");
            backupSynchronizerAction.OnFileDelete += (sender, args) => Console.WriteLine($"Deleting file {args.FilePath}...");
            backupSynchronizerAction.OnFolderDelete += (sender, args) => Console.WriteLine($"Deleting folder {args.FolderPath}...");
            backupSynchronizerAction.DoBackup(source, dest, doNotDeleteDestFiles);
        }
    }
}
