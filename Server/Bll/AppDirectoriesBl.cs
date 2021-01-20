using System.IO;
using System.Collections.Generic;

namespace CloudCopy.Server.Bll
{
    public class AppDirectoriesBl
    {
        public static bool IsProvisioned()
        {
            return Directory.Exists(Program.AppCfg.BaseDirectory)
                && Directory.Exists(Program.AppCfg.AppDbDirectory)
                && Directory.Exists(Program.AppCfg.LogDirectory)
                && Directory.Exists(Program.AppCfg.FilesDirectory);
        }

        public static void Provision()
        {
            List<string> directories = new List<string>();

            directories.Add(Program.AppCfg.BaseDirectory);
            directories.Add(Program.AppCfg.AppDbDirectory);
            directories.Add(Program.AppCfg.LogDirectory);
            directories.Add(Program.AppCfg.FilesDirectory);

            foreach (string directory in directories)
            {
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
            }
        }
    }
}