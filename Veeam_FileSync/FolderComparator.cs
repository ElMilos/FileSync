using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Veeam_FileSync
{
    internal class FolderComparator
    {

         string sourceDirPath = "";
         string replicaDirPath = "";

        public FolderComparator(string source = "", string replica = "")
        {
            this.sourceDirPath = source;
            this.replicaDirPath = replica;
        }

         public void CheckFodlers()
        {

            if (!Directory.Exists(sourceDirPath)) return;
            if (!Directory.Exists(replicaDirPath)) return;


            string[] filesSource = Directory.GetFiles(sourceDirPath, "*", SearchOption.AllDirectories);
            string[] fileSourceRelative = filesSource.Select(s => s.Replace($"{sourceDirPath}\\", "")).ToArray();

            string[] dirsSource = Directory.GetDirectories(sourceDirPath, "*", SearchOption.AllDirectories);

            foreach (var dir in dirsSource)
            {
                string rel = Path.GetRelativePath(sourceDirPath, dir);
                string destDir = Path.Combine(replicaDirPath, rel);
                Directory.CreateDirectory(destDir);
            }


            var sourceMap = filesSource.Zip(fileSourceRelative, (full, relative) => new { full, relative })
                                 .ToDictionary(x => x.full, x => x.relative);

            string[] filesReplica = Directory.GetFiles(replicaDirPath, "*", SearchOption.AllDirectories);
            string[] fileReplicaRelative = filesSource.Select(s => s.Replace($"{replicaDirPath}\\", "")).ToArray();

            var replicaMap = filesReplica.Zip(fileReplicaRelative, (full, relative) => new { full, relative })
                                 .ToDictionary(x => x.full, x => x.relative);





            foreach (var pathSource in sourceMap)
            {
                Console.WriteLine(pathSource);
                var infoSource = new FileInfo(pathSource.Key);

                Console.WriteLine(infoSource.Length);
                Console.WriteLine(infoSource.LastWriteTimeUtc);
                Console.WriteLine(infoSource.CreationTimeUtc);
                Console.WriteLine("\r\r");

                foreach (var pathReplica in replicaMap)
                {
                    var infoReplica = new FileInfo(pathReplica.Key);
                    if (pathReplica.Value.Equals(pathSource.Value))
                    {
                        if (infoSource.LastWriteTimeUtc != infoReplica.LastWriteTimeUtc)
                        {
                            CopyFile(pathSource.Key, replicaDirPath, pathReplica.Value);
                        }
                        replicaMap.Remove(pathReplica.Key);
                    }
                    continue;
                }
                CopyFile(pathSource.Key, replicaDirPath, pathSource.Value);
            }

        }


         private void CopyFile(string source, string replica, string name)
        {
            //int i = name.LastIndexOf('\\');
            //string rel = name.Remove(i, name.Length - i);

            File.Copy(source, $"{replica}\\{name}", true);
        }


        private void LogOperation()
        {
            //TO DO
        }

    }
}
