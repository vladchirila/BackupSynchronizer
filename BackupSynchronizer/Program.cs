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

            BackupSynchronizerAction.DoBackup(source, dest, doNotDeleteDestFiles);
        }
    }
}
