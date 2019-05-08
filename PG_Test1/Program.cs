using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace PG_Test1
{
    class Program
    {
        static void Main(string[] args)
        {
            string directoryPath = @"C:\Users\aagoyal\Documents\Project\P&G\P&G data\P&G data_7May";
            string filesType = "*.png";
            OCR ocr = new OCR();
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            var fileList = directoryInfo.GetFiles(filesType);
            foreach(var file in fileList)
            {
                string filePath = directoryPath + @"\" + file.ToString().Replace(".png", ".txt");
                var JSON = ocr.getJSON(directoryPath + @"\" + file);
                Console.WriteLine(JSON.ToString());
                // Console.WriteLine(ocr.getProperty(JSON));
                Console.WriteLine("\n");
                // Check if file already exists. If yes, delete it. 
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                
                // Create a new file     
                using (FileStream fs = File.Create(filePath))
                {
                    // Add some text to file    
                    Byte[] content = new UTF8Encoding(true).GetBytes(ocr.getProperty(JSON));
                    fs.Write(content, 0, content.Length);
                }
            }
            
            Console.ReadKey();
        }
    }
}
