using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBLogAnalyzer
{
    public class CollectionWriter
    {
        private string directory;

        public CollectionWriter()
        {
            CurrentDirectory = Environment.CurrentDirectory;
        }

        public CollectionWriter(string directory)
        {
            CurrentDirectory = directory;
        }

        public string CurrentDirectory
        {
            get { return directory; }
            set
            {
                foreach (char c in Path.GetInvalidPathChars())
                    value = value.Replace(c.ToString(), String.Empty);
                value = value.Replace("?", String.Empty);

                directory = value;
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }
        }

        public void WriteFile<T>(string fileName, IEnumerable<T> collection, Func<T, string> writer)
        {
            using (FileStream fs = new FileStream(Path.Combine(CurrentDirectory, fileName), FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
            {
                foreach (var item in collection)
                    sw.WriteLine(writer(item));
            }
        }

        public void WriteFileWithPosition<T>(string fileName, IEnumerable<T> collection, Func<T, string> writer)
        {
            using (FileStream fs = new FileStream(Path.Combine(CurrentDirectory, fileName), FileMode.Create, FileAccess.Write, FileShare.None))
            using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
            {
                int position = 0;
                foreach (var item in collection)
                {
                    position++;
                    sw.WriteLine(writer(item).Replace("%POSITION%", position.ToString()));
                }
            }
        }
    }
}
