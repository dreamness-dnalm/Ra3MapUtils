using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using MapCoreLib.Core.Util;

namespace UtilLib.mapFileHelper
{
    public class MapFileHelper
    {
        public static (string, string) TranslateMapPath(string mapPath)
        {
            var retPath = mapPath;
            retPath = retPath.Replace("/", "\\");
            if (!retPath.Contains("\\"))
            {
                retPath = Path.Combine(PathUtil.RA3MapFolder, mapPath);
            }

            var pathSplits = retPath.Split('\\');
            var mapName = pathSplits[pathSplits.Length - 1];
            return (retPath, mapName);
        }
        
        public static bool IsMap(string mapPath)
        {
            string mapName;
            (mapPath, mapName) = TranslateMapPath(mapPath);
            if (!Directory.Exists(mapPath))
            {
                return false;
            }

            var coreFilePath = Path.Combine(mapPath, mapName + ".map");
            return File.Exists(coreFilePath);
        }
        
        
        public static void Copy(string sourceDir, string destinationDir)
        {
            string sourceMapName, destinationMapName;
            (sourceDir, sourceMapName) = TranslateMapPath(sourceDir);
            (destinationDir, destinationMapName) = TranslateMapPath(destinationDir);
            
            if(!Directory.Exists(sourceDir))
            {
                throw new Exception("Source directory does not exist: " + sourceDir);
            }

            if (!IsMap(sourceDir))
            {
                throw new Exception("Source directory is not a map: " + sourceDir);
            }
            
            // 检查目标文件夹是否存在，如果不存在则创建
            if (Directory.Exists(destinationDir))
            {
                throw new Exception("Destination directory already exists: " + destinationDir);
            }
            
            Directory.CreateDirectory(destinationDir);

            // 复制和重命名文件
            foreach (string filePath in Directory.GetFiles(sourceDir))
            {
                // 获取文件名
                string fileName = Path.GetFileName(filePath);

                // 如果文件名以旧前缀开头，替换为新前缀
                if (fileName.StartsWith(sourceMapName))
                {
                    fileName = destinationMapName + fileName.Substring(sourceMapName.Length);
                }

                string destinationFilePath = Path.Combine(destinationDir, fileName);
                Console.WriteLine("Copying " + filePath + " to " + destinationFilePath);
                // 复制文件并覆盖
                File.Copy(filePath, destinationFilePath, true);
            }
        }

        public static void Del(String dir)
        {
            string mapName;
            (dir, mapName) = TranslateMapPath(dir);
            
            if (!Directory.Exists(dir))
            {
                throw new Exception("Directory does not exist: " + dir);
            }
            
            if (!IsMap(dir))
            {
                throw new Exception("Source directory is not a map: " + dir);
            }
            Console.WriteLine("Deleting " + dir);
            Directory.Delete(dir, true);
        }
        
        public static void Move(string sourceDir, string destinationDir)
        {
            Copy(sourceDir, destinationDir);
            Del(sourceDir);
        }
        
        public static List<String> Ls(string dir)
        {
            if(dir == null)
            {
                dir = PathUtil.RA3MapFolder;
            }
            
            Console.WriteLine("Maps in " + dir + ":");
            List<String> mapNames = new List<string>();
            
            foreach (string dirPath in Directory.GetDirectories(dir))
            {
                if (IsMap(dirPath))
                {
                    var mapName = Path.GetFileName(dirPath);
                    mapNames.Add(mapName);
                    // Console.WriteLine(mapName);
                }
            }

            return mapNames;
        }

        public static string Compress(string dir, string targetDir,string method)
        {
            string mapName;
            (dir, mapName) = TranslateMapPath(dir);
            
            if (!Directory.Exists(dir))
            {
                throw new Exception("Directory does not exist: " + dir);
            }
            
            if (!IsMap(dir))
            {
                throw new Exception("Source directory is not a map: " + dir);
            }
            
            if(!Directory.Exists(targetDir))
            {
                throw new Exception("Target directory does not exist: " + targetDir);
            }

            if (method == "zip")
            {
                var zipFilePath = Path.Combine(targetDir, mapName + ".zip");
                ZipFile.CreateFromDirectory(dir, zipFilePath, CompressionLevel.Optimal, true);
                return zipFilePath;
            }
            else
            {
                throw new Exception("Unsupported compression method: " + method);
            }

            return null;

        }

        public static List<String> LsMapFiles(string dir)
        {
            string mapName;
            (dir, mapName) = TranslateMapPath(dir);
            
            if (!Directory.Exists(dir))
            {
                throw new Exception("Directory does not exist: " + dir);
            }
            
            
            List<String> fileNames = new List<string>();
            
            foreach (string file in Directory.GetFiles(dir))
            {
                    var fileName = Path.GetFileName(file);
                    fileNames.Add(fileName);
            }

            return fileNames;
        }
        
        
    }
}