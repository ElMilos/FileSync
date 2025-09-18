using Veeam_FileSync;

List<String> agrsList = new List<string>(args);
int interval = 1000;

if (args.Length < 2 || args.Length > 4)
{
    Console.WriteLine("Usage: Veeam_FileSync <source_directory> <replica_directory> [log_file_path] [interval_ms]");
    return;
}

if ( args.Length == 4 && (!int.TryParse(args[3], out interval) || interval <= 0))
{
    Console.WriteLine("Error: priovided time is not correct");
    return;
}


using var cts = new CancellationTokenSource();

Console.WriteLine("\nTo stop program press ctrl + c ");


Console.CancelKeyPress += (s, e) =>
{
    if (!cts.IsCancellationRequested)
    {
        cts.Cancel();
        e.Cancel = true;
    }
};

try
{
    using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(interval));

    var fc = new FolderComparator(agrsList[0], agrsList[1], agrsList[2]);

    while (await timer.WaitForNextTickAsync(cts.Token))
    {
        Console.WriteLine("Checking floders");
        fc.CheckFodlers();
    }
}
catch (OperationCanceledException)
{
    Console.WriteLine("\nProgram stopped");
}
catch
{
    Console.Error.WriteLine("Error: incorrect data");
}