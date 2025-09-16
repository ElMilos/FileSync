using Veeam_FileSync;
using System.CommandLine;

Option<string> sourceFolder = new("-s")
{
    Description = "Source folder path"
};

Option<string> replicaFolder = new("-r")
{
    Description = "Replica folder path"
};

Option<int> interval = new("-i")
{
    Description = "Time between each copy"
};


var fc = new FolderComparator();
fc.CheckFodlers();
