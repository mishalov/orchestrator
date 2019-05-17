using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orchestrator
{
    public static class FileWorker
    {
        public static void Copy(string From, string To)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(From, "*",
                SearchOption.AllDirectories))
                Directory.CreateDirectory(dirPath.Replace(From, To));

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(From, "*.*",
                SearchOption.AllDirectories))
                File.Copy(newPath, newPath.Replace(From, To), true);
        }

        public static string makeDirectoryName(string Id, string UserId)
        {
            return $"/home/orchestrator/app/{UserId}/{Id}";
        }
    }
}
