using System;
using System.Collections.Generic;
using System.Dynamic;
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
        string logFilePath = "";
        List<string> operationList = new List<string>();

        public FolderComparator(string source = "", string replica = "", string log = "log.txt")
        {
            this.sourceDirPath = source;
            this.replicaDirPath = replica;
            this.logFilePath = log;
        }

        public void CheckFodlers()
        {

            if (!Directory.Exists(sourceDirPath)) return;
            if (!Directory.Exists(replicaDirPath)) return;


            var sourceMap = GetFilesMap(sourceDirPath);
            var replicaMap = GetFilesMap(replicaDirPath);

            var dirsSourceRelative = GetDirsList(sourceDirPath);
            var dirsReplicaRelative = GetDirsList(replicaDirPath);

            SyncDir(dirsSourceRelative, dirsReplicaRelative);
            SyncFiles(sourceMap, replicaMap);

            LogToFile();
        }

        private List<string> GetDirsList(string folderPath)
        {
            List<string> dirsSource = Directory.
                GetDirectories(folderPath, "*", SearchOption.AllDirectories)
                .Select(d => Path.GetRelativePath(folderPath, d))
                .ToList();

            return dirsSource;
        }

        private Dictionary<string, string> GetFilesMap(string folderPath)
        {
            string[] filesSource = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            string[] fileSourceRelative = filesSource.Select(
                s => s.Replace($"{folderPath}\\", "")).ToArray();

            var map = filesSource.Zip(fileSourceRelative, (full, relative) => new { full, relative })
                                 .ToDictionary(x => x.full, x => x.relative);
            return map;
        }


        private void SyncDir(List<string> sourceDir, List<string> replicaDir)
        {
            var addition = sourceDir.Except(replicaDir).ToList();

            foreach (var folder in addition)
            {
                string destDir = Path.Combine(replicaDirPath, folder);
                Directory.CreateDirectory(destDir);

                operationList.Add($"folder added: {folder}");
            }
        }

        private void SyncFiles(Dictionary<string, string> sourceMap, Dictionary<string, string> replicaMap)
        { 

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
                            operationList.Add($"File updated: {pathReplica.Key}");
                        }
                        replicaMap.Remove(pathReplica.Key);
                    }
                    continue;
                }
                CopyFile(pathSource.Key, replicaDirPath, pathSource.Value);
                operationList.Add($"File added: {replicaDirPath}{pathSource.Value}");
            }

            DeleteExcessFiles(replicaMap);
        }

         private void CopyFile(string source, string replica, string name)
        {
            File.Copy(source, $"{replica}\\{name}", true);
        }


        private void LogToFile()
        {
           foreach (string log in operationList)
            {
                File.AppendAllText(logFilePath, $"{log}\n");
            }
        }

        private void DeleteExcessFiles(Dictionary<string,string> excessMap)
        {
            foreach (var file in excessMap)
            {
                File.Delete(file.Key);

                operationList.Add($"File deleted: {file.Key}");
            }
        }

        private void DeleteExcessDirs(List<string> sourceDir, List<string> replicaDir)
        {
            var excess = replicaDir.Except(sourceDir).ToList();

            foreach (var folder in excess)
            {
                File.Delete(folder);

                operationList.Add($"folder deleted: {folder}");
            }
        }
    }
}
