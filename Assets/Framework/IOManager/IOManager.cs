using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CoffeeFramework
{
    ///  <summary>
    /// File IO bagin with Application.persistentDataPath
    /// </summary>
    public class IOManager
    {
        /// <summary>
        /// Read From File, path should not begin with "/"
        /// </summary>
        /// <param name="path">example:"setting/main.ini"</param>
        /// <returns>String of text</returns>
        public static string ReadFromFile(string path)
        {
            path = Path.Combine(Application.persistentDataPath, path);
            try
            {
                if (File.Exists(path))
                {
                    string content = File.ReadAllText(path);
                    return content;
                }
                else
                {
                    Debug.LogWarning("File not found: " + path);
                    return null;
                }
            }
            catch (IOException e)
            {
                Debug.LogError("An error occurred while reading the file: " + e.Message);
                return null;
            }
        }

        /// <summary>
        /// Write To File, path should not begin with "/"
        /// </summary>
        /// <param name="path">example:"setting/main.ini"</param>
        /// <param name="content">string content</param>
        public static void WriteToFile(string path, string content)
        {
            path = Path.Combine(Application.persistentDataPath, path);
            string directory = Path.GetDirectoryName(path);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            try
            {
                File.WriteAllText(path, content);
                Debug.Log($"File {path} written successfully");
            }
            catch (IOException e)
            {
                Debug.LogError("An error occurred while writing to the file: " + e.Message);
            }
        }
    }
}