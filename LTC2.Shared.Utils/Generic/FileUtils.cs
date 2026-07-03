using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

namespace LTC2.Shared.Utils.Generic
{
    public class FileUtils
    {
        public static string GetEntryAssemblyFolder()
        {
            var assembly = Assembly.GetEntryAssembly();

            return GetAssemblyFolder(assembly);
        }

        public static string GetFolderInEntryAssembly(string folder)
        {
            var assembly = Assembly.GetEntryAssembly();
            var assemblyFolder = GetAssemblyFolder(assembly);

            return $"{GetPathWithEndingSlash(assemblyFolder)}{folder}";
        }

        public static string GetCallingAssemblyFolder()
        {
            var assembly = Assembly.GetCallingAssembly();

            return GetAssemblyFolder(assembly);
        }

        public static string GetAssemblyFolder(Assembly assembly)
        {
            var codeBase = assembly.Location;
            var assemblyFolder = System.IO.Path.GetDirectoryName(codeBase);

            return assemblyFolder;
        }

        public static string GetPathWithEndingSlash(string file)
        {
            if (!file.EndsWith("\\"))
            {
                file = file + "\\";
            }

            return file;
        }

        public static string GetPathWithEndingString(string file, string endingWith)
        {
            if (!file.EndsWith(endingWith))
            {
                file = file + endingWith;
            }

            return file;
        }

        public static string GetAppNameFromModule()
        {
            var appId = string.Empty;

            var processModule = Process.GetCurrentProcess().MainModule;
            
            if (processModule != null)
            {
                var exeName = Path.GetFileName(processModule.FileName);
                var parts = exeName.Split('.').Reverse();
                
                foreach (var part in parts)
                {
                    if (part.ToLower() != "exe" && part.ToLower() != "dll" && part.ToLower() != "so" && part.ToLower() != "dylib")
                    {
                        appId = $"{part}";
                        
                        break;
                    }
                }                  
            }

            return appId;
        }

        public static string GetConfigFileAddition()
        {
            var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

            if (isWindows || isLinux)
            {
                return string.Empty;
            }
            else
            {
                var appName = GetAppNameFromModule();
                
                if (!string.IsNullOrEmpty(appName))
                {
                    return $".{appName}";
                }
                else
                {
                    return  string.Empty;
                }
            }
        }
    }

}
