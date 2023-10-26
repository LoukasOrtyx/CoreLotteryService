using System.Diagnostics;

namespace CoreLotteryService.Helpers
{
    /// <summary>
    /// An utility static class to allow easy execution of commands and scripts.
    /// </summary>
    public static class CmdHelper
    {
        /// <summary>
        /// Execute a command line on the terminal and returns its output.
        /// </summary>
        /// <param name="cmd">A <see cref="string"/> representing the command</param>
        /// <param name="args"> A <see cref="string"/> representing the arguments of the given command</param>
        /// <returns>
        /// The <see cref="void"/>
        /// </returns> 
        public static string ExecuteCmd(string cmd, string args)
        {
            string result = "";
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = cmd;
            start.Arguments = args;
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using(var process = Process.Start(start))
            {
                if (process is not null) 
                {
                    using(StreamReader reader = process.StandardOutput)
                    {
                        result = reader.ReadToEnd();
                    }
                }
            }
            return result;
        }
    }
}