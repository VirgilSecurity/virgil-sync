namespace Virgil.DropBox.Client.Encryption
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Newtonsoft.Json;

    public class LogRepository
    {
        private const string StorageFileName = @"D:\ocr-export\storage.txt";

        public void AddOrReplace(LogEntry entry)
        {
            var logEntries = this.Load() ?? new List<LogEntry>();

            var existingEntry = logEntries.FirstOrDefault(it => it.Path.ToLowerInvariant() == entry.Path.ToLowerInvariant());
            if (existingEntry != null)
            {
                existingEntry.Hashes = entry.Hashes;
            }
            else
            {
                logEntries.Add(entry);
            }

            this.Save(logEntries);
        }

        public List<LogEntry> GetAll()
        {
            return this.Load();
        }

        public LogEntry GetByLocalPath(string localPath)
        {
            return this.Load()?.FirstOrDefault(it => string.Equals(it.Path, localPath, StringComparison.InvariantCultureIgnoreCase));
        }

        private List<LogEntry> Load()
        {
            using (var stream = File.Open(StorageFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                var streamReader = new StreamReader(stream);
                var text = streamReader.ReadToEnd();
                return JsonConvert.DeserializeObject<List<LogEntry>>(text);
            }
        }

        private void Save(List<LogEntry> list)
        {
            using (var stream = File.Open(StorageFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using (var streamWriter = new StreamWriter(stream))
                {
                    streamWriter.Write(JsonConvert.SerializeObject(list));
                }
            }
        }
    }
}