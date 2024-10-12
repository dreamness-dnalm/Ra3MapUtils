using System;
using System.IO;
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

            var pathSplits = mapPath.Split('\\');
            var mapName = pathSplits[pathSplits.Length - 1];
            return (retPath, mapName);
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
            
            Directory.Delete(dir, true);
        }
        
        public static void Move(string sourceDir, string destinationDir)
        {
            Copy(sourceDir, destinationDir);
            Del(sourceDir);
        }
    }
}