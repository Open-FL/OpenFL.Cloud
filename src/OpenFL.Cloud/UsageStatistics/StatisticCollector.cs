using System;
using System.IO;
using System.Reflection;

namespace OpenFL.Cloud.UsageStatistics
{
    public static class StatisticCollector
    {

        private static string StatRoot =>
            Path.Combine(
                         Path.GetDirectoryName(new Uri(Assembly.GetEntryAssembly().CodeBase).AbsolutePath),
                         "statistics"
                        );

        private static string SuccessPath => Path.Combine(StatRoot, "success");

        private static string ErrorPath => Path.Combine(StatRoot, "error");

        private static string GetUniqueName(string extension)
        {
            return DateTime.Now.ToFileTimeUtc() + extension;
        }

        private static void EnsureStructure()
        {
            Directory.CreateDirectory(SuccessPath);
            Directory.CreateDirectory(ErrorPath);
        }

        public static void OnProgramBuilt(string source, out string outFilePath)
        {
            EnsureStructure();
            string name = GetUniqueName("");
            Directory.CreateDirectory(Path.Combine(SuccessPath, name));
            outFilePath = Path.Combine(SuccessPath, name, "out.png");
            File.WriteAllText(Path.Combine(SuccessPath, name, "in.fl"), source);
        }

        public static void OnProgramFailed(string source, Exception ex)
        {
            EnsureStructure();
            string name = GetUniqueName("");
            Directory.CreateDirectory(Path.Combine(ErrorPath, name));
            File.WriteAllText(Path.Combine(ErrorPath, name, "in.fl"), source);
            File.WriteAllText(Path.Combine(ErrorPath, name, "error.log"), FormatException(ex));
        }

        private static string FormatException(Exception ex)
        {
            string ret = "";
            Exception current = ex;
            do
            {
                ret = current + "\n_________________________________________" + ret;
                current = current.InnerException;
            } while (current != null);

            return ret;
        }

    }
}