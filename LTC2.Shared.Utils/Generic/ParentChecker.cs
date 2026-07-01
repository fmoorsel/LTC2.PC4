using System;
using System.Diagnostics;

namespace LTC2.Shared.Utils.Generic
{
    public class ParentChecker
    {
        
        public static bool IsParentProcessRunning()
        {
            var pPid = GetParentProcessIdFromCommandLine();

            if (pPid > 0)
            {
                try
                {
                    var p = Process.GetProcessById(pPid);
                    
                    return !p.HasExited;
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }

            return true;
        }

        public static int GetParentProcessIdFromCommandLine()
        {
            var arguments = Environment.GetCommandLineArgs();

            foreach (var parameter in arguments)
            {
                var ppidParToken = "ppid:";
                if (parameter.ToLower().StartsWith(ppidParToken))
                {
                    var ppidAsString = parameter.Substring(ppidParToken.Length);
                    
                    if (int.TryParse(ppidAsString, out var ppid))
                    {
                        return ppid;
                    }
                }
            }
            
            return -1;
        }
    }
}

