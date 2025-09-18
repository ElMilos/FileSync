namespace Veeam_FileSync
{
    internal class FolderComparator
    {

       private string _sourceDirPath = "";
       private string _replicaDirPath = "";
       private string _logFilePath = "";
       List<string> operationList = new List<string>();

        public FolderComparator(string source = "E:\\FolderTestowy\\source", string replica = "E:\\FolderTestowy\\replica", string log = "log.txt")
        {
            this._sourceDirPath = source;
            this._replicaDirPath = replica;
            this._logFilePath = log;;
        }

        public void CheckFodlers()
        {

            if (!Directory.Exists(_sourceDirPath)) return;
            if (!Directory.Exists(_replicaDirPath)) return;


            var sourceMap = GetFilesMap(_sourceDirPath);
            var replicaMap = GetFilesMap(_replicaDirPath);

            var dirsSourceRelative = GetDirsList(_sourceDirPath);
            var dirsReplicaRelative = GetDirsList(_replicaDirPath);

            SyncDir(dirsSourceRelative, dirsReplicaRelative);
            SyncFiles(sourceMap, replicaMap);
            DeleteExcessDirs(dirsSourceRelative, dirsReplicaRelative);
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
                                 .ToDictionary(x => x.relative, x => x.full);
            return map;
        }


        private void SyncDir(List<string> sourceDir, List<string> replicaDir)
        {
            var addition = sourceDir.Except(replicaDir).OrderByDescending(p => p.Length)
                        .ToList();

            foreach (var folder in addition)
            {
                string destDir = Path.Combine(_replicaDirPath, folder);
                Directory.CreateDirectory(destDir);

                operationList.Add($"folder added: {folder}");
            }
        }

        private void DeleteExcessDirs(List<string> sourceDir, List<string> replicaDir)
        {
            var excess = replicaDir.Except(sourceDir).OrderByDescending(p => p.Length)
                        .ToList();

            foreach (var folder in excess)
            {
                string path = Path.Combine(_replicaDirPath, folder);

                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
        }

        private void SyncFiles(Dictionary<string, string> sourceMap, Dictionary<string, string> replicaMap)
        { 

            foreach (var pathSource in sourceMap)
            {

                if (!replicaMap.ContainsKey(pathSource.Key))
                {
                    CopyFile(pathSource.Value, _replicaDirPath, pathSource.Key);
                    operationList.Add($"File added: {_replicaDirPath}{pathSource.Key}");
                    continue;
                }
                else
                {
                    string hashOrginal = GetFileHash(pathSource.Value);
                    string hashCopy = GetFileHash(replicaMap[pathSource.Key]);

                    if (hashOrginal == hashCopy) continue;

                    CopyFile(pathSource.Value, _replicaDirPath, pathSource.Key);
                    operationList.Add($"File updated: {_replicaDirPath}{pathSource.Key}");
                }
            }

            DeleteExcessFiles(sourceMap, replicaMap);
        }

        private string GetFileHash(string path)
        {
            return PowerShellComm.RunScript(
            $"Get-FileHash -Path  \"{path}\"  | Select-Object -ExpandProperty Hash").Trim();
            
        }

         private void CopyFile(string source, string replica, string name)
        {
            File.Copy(source, $"{replica}\\{name}", true);
        }

        private void DeleteExcessFiles(Dictionary<string, string> sourceMap, Dictionary<string, string> replicaMap)
        {
            var excess = replicaMap.Keys.Except(sourceMap.Keys);

            foreach (var relPath in excess)
            {
                string path = replicaMap[relPath];
                if (File.Exists(path))
                {
                    File.Delete(path);
                    operationList.Add($"File deleted: {path}");
                }
            }
        }

        private void LogToFile()
        {
            foreach (string log in operationList)
            {
                File.AppendAllText(_logFilePath, $"{log}\n");
            }
            operationList.Clear();
        }
    }
}
