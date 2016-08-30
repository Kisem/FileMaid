using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace FileMaid
{
    static class FileManager
    {
        #region Declarations
        // Get the downloads folder info
        static string pathUser = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        static string pathDownload = Path.Combine(pathUser, "Downloads");
        static DirectoryInfo dirInfo = new DirectoryInfo(pathDownload);

        static Timer timer = new Timer();
        public static List<FileType> FileTypes = new List<FileType>();
        #endregion

        public static void Initialize()
        {
            // Set timer
            timer.Interval = 1000;
            timer.Elapsed += TimerTick;
            timer.Enabled = true;
            
            // Read config file line by line
            foreach (string line in File.ReadLines("filetypes.conf"))
            {
                // Define filetype properties
                var data = line.Split(':');
                var name = "• " + data[0] + " •";
                var formats = data[1].Split('|');
                
                // Add filetype to list
                FileTypes.Add(new FileType(name, formats.ToList()));
            }
        }

        public static List<FileInfo> GetFiles()
        {
            // Get all files from the specified folder and return them
            List<FileInfo> allFiles = dirInfo.GetFiles("*.*").ToList();
            List<FileInfo> allowedFiles = new List<FileInfo>();

            // Iterate through defined filetypes
            foreach (FileType fileType in FileTypes)
            {
                // Check if the current filetype's folder exists, and if not, then create it
                if (!Directory.Exists(pathDownload + "\\" + fileType.Name))
                {
                    dirInfo.CreateSubdirectory(fileType.Name);
                }

                // Iterate through defined extensions of each filetype to filter the files
                foreach (string format in fileType.Extensions)
                {
                    foreach (var file in allFiles)
                    {
                        if (format == file.Extension.Replace(".", ""))
                        {
                            allowedFiles.Add(file);
                        }
                    }
                    foreach (var file in allowedFiles)
                    {
                        allFiles.Remove(file);
                    }
                }
            }
            return allowedFiles;
        }
        public static void MoveFilesToSubfolders(List<FileInfo> files)
        {
            // Check if the folder has any files to move
            if (files.Count == 0) return;

            int filesMoved = 0;
            foreach (FileInfo file in files)
            {
                // Why do i do this???
                if (file.Length == 0) continue;
                
                // Iterate through defined filetypes
                foreach (FileType fileType in FileTypes)
                {
                    // Iterate through defined extensions of each filetype
                    foreach (string format in fileType.Extensions)
                    {
                        if (format != file.Extension.Replace(".", ""))
                        {
                            continue;
                        }

                        // Black magic stuff bellow
                        // Needs a lot of work to make it flawless
                        string subfolderPath = pathDownload + "\\" + fileType.Name + "\\";
                        string fileName = file.Name.Substring(0, file.Name.Length - file.Extension.Length);
                        string movePath = subfolderPath + file.Name;
                        if (File.Exists(movePath))
                        {
                            int i = 1;
                            while (File.Exists(subfolderPath + fileName + " (" + i + ")" + file.Extension))
                            {
                                i++;
                            }
                            movePath = subfolderPath + fileName + " (" + i + ")" + file.Extension;
                        }

                        try
                        {
                            Console.WriteLine("Move file to " + movePath);
                            file.MoveTo(movePath);
                            filesMoved++;
                        }
                        catch (IOException e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
            }

            if (filesMoved == 0) return;

            Console.WriteLine("========================");
            Console.WriteLine("Done!   "+ filesMoved +" file"+ (filesMoved == 1 ? "" : "s") +" moved");
            Console.WriteLine("========================");

        }

        private static void TimerTick(object source, ElapsedEventArgs e)
        {
            var files = GetFiles();
            MoveFilesToSubfolders(files);
        }
    }
}
