using System.Reflection;

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

    }

}
