using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veeam_FileSync
{
    internal class FolderComparator
    {
        static public void CheckFodlers()
        {

            string sourceDirPath = "";
            string replicaDirPath = "";

            if (!Directory.Exists(sourceDirPath)) return;
            if (!Directory.Exists(replicaDirPath)) return;


            string[] filesSource = Directory.GetFiles(sourceDirPath, "*", SearchOption.AllDirectories);
            string[] fileSourceRelative = filesSource.Select(s => s.Replace($"{sourceDirPath}\\", "")).ToArray();

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


        static private void CopyFile(string source, string replica, string name)
        {
            File.Copy(source, $"{replica}\\{name}", true);
        }

    }
}
