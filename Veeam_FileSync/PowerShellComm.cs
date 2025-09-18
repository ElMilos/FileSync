using System.Management.Automation;
using System.Text;

namespace Veeam_FileSync
{
    internal static class PowerShellComm
    {
        private static PowerShell ps = PowerShell.Create();

        public static string RunScript(string scriptText)
        {
            string errorMsg = "";


            ps.Commands.Clear();
            ps.AddScript(scriptText);

            ps.AddCommand("Out-String");

            PSDataCollection<PSObject> output = new ();

            ps.Streams.Error.DataAdded += (sender, e) =>
            {
                errorMsg = ((PSDataCollection<ErrorRecord>)sender)[e.Index].ToString();
            };

            IAsyncResult result = ps.BeginInvoke<PSObject, PSObject>(null, output);

            ps.EndInvoke(result);

            StringBuilder sb = new();

            foreach (PSObject obj in output)
            {
                sb.AppendLine(obj.ToString());
            }

            ps.Commands.Clear();

            return sb.ToString();
        }
    }
}
