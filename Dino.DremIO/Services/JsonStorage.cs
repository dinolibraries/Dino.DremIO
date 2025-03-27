using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Dino.DremIO.Services
{
    public class JsonStorage
    {
        private readonly string _folderPath;

        public JsonStorage(string folderPath)
        {
            _folderPath = folderPath;
            if (!Directory.Exists(_folderPath))
            {
                Directory.CreateDirectory(_folderPath);
            }
        }

        public async Task AddAsync<TModel>(string key, TModel data, TimeSpan expiration) where TModel : class 
        {
            string filePath = GetFilePath(key);
            var entry = new CacheEntry<TModel>
            {
                Data = data,
                ExpirationTime = DateTime.UtcNow.Add(expiration)
            };

            string json = JsonConvert.SerializeObject(entry);
            await File.WriteAllTextAsync(filePath, json);
        }

        public async Task<TModel?> GetAsync<TModel>(string key) where TModel : class 
        {
            string filePath = GetFilePath(key);

            if (!File.Exists(filePath)) return default;

            string json = await File.ReadAllTextAsync(filePath);
            var entry = JsonConvert.DeserializeObject<CacheEntry<TModel>>(json);

            if (entry == null || entry.ExpirationTime < DateTime.UtcNow)
            {
                File.Delete(filePath);
                return default;
            }

            return entry.Data;
        }

        private string GetFilePath(string key) => Path.Combine(_folderPath, $"{key}.json");

        private class CacheEntry<T>
        {
            public T Data { get; set; } = default!;
            public DateTime ExpirationTime { get; set; }
        }
    }
    }
