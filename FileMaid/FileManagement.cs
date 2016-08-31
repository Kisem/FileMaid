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
        
        // Program startup (before the form load)
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
        
        // Function that scans the defined folder and returns a list with the whitelisted files
        public static List<FileInfo> GetFiles()
        {
            // Get all files from the specified folder
            List<FileInfo> allFiles = dirInfo.GetFiles("*.*").ToList();

            // Create an empty list that will contain the allowed files
            List<FileInfo> allowedFiles = new List<FileInfo>();

            // Iterate through defined filetypes
            foreach (FileType fileType in FileTypes)
            {
                // Check if the current filetype's folder exists, and if not, then create it
                dirInfo.CreateSubdirectory(fileType.Name);

                // Iterate through defined extensions of each filetype to filter the files
                foreach (string format in fileType.Extensions)
                {
                    foreach (var file in allFiles)
                    {
                        // Check if the current files extension is on the list
                        if (format == file.Extension.Replace(".", ""))
                        {
                            // Add the file to the allowedFiles list if it's type defined
                            allowedFiles.Add(file);
                        }
                    }

                    // Remove allowedFiles from allFiles
                    foreach (var file in allowedFiles)
                    {
                        allFiles.Remove(file);
                    }
                }
            }
            return allowedFiles;
        }

        // Function to move each file to its subfolder
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
                        
                        #region BLACKMAGIC ALERT
                        // Black magic stuff bellow
                        // Needs a lot of work to make it flawless

                        // (At this point it won't make any big mistakes, but it can not recognize the format of dupe files.
                        // For example: file (1).zip will going to be file (1) (1).zip) if there is already a file with the same name

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
                        // Black magic region is over, you can continue the review from this line
                        #endregion

                        // Try to move the file (if it is used by another process or any error occurs, then print the exception to the console)
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
            

            // Print a nice message for the user
            Console.WriteLine("========================");
            Console.WriteLine("Done!   "+ filesMoved +" file"+ (filesMoved == 1 ? "" : "s") +" moved");
            Console.WriteLine("========================");
        }


        // Function that is called every time, when timer ticks
        private static void TimerTick(object source, ElapsedEventArgs e)
        {
            var files = GetFiles();
            MoveFilesToSubfolders(files);
        }
    }
}
