using System;
using System.Collections.Generic;
using System.IO;
using ReferenceDeletion.Interfaces;
using ReferenceDeletion.Models;
using UnityEngine;
using ILogger = ReferenceDeletion.Interfaces.ILogger;

namespace ReferenceDeletion.Persistence
{
    /// <summary>
    /// Persists <see cref="CacheData"/> as a compact custom binary format under
    /// <c>Library/ReferenceDeletion/</c>. Never writes under <c>Assets/</c>, so the
    /// cache is never version controlled or exposed to other users.
    /// </summary>
    public sealed class CacheStorage : ICacheStorage
    {
        private const string CacheFileName = "reference-cache.bin";
        private readonly ILogger _logger;
        private readonly string _cacheFilePath;

        public CacheStorage(ILogger logger)
        {
            _logger = logger;
            string libraryDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Library", "ReferenceDeletion");
            _cacheFilePath = Path.Combine(libraryDir, CacheFileName);
        }

        public bool TryLoad(out CacheData data)
        {
            data = null;

            if (!File.Exists(_cacheFilePath))
            {
                return false;
            }

            try
            {
                using (FileStream stream = File.Open(_cacheFilePath, FileMode.Open, FileAccess.Read))
                using (BinaryReader reader = new BinaryReader(stream))
                {
                    int version = reader.ReadInt32();
                    if (version != CacheData.CurrentVersion)
                    {
                        _logger.LogWarning($"Cache version mismatch (found {version}, expected {CacheData.CurrentVersion}). Discarding cache.");
                        return false;
                    }

                    data = ReadCacheData(reader, version);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Reference cache appears corrupt and will be rebuilt. Reason: {ex.Message}");
                TryDeleteFile();
                data = null;
                return false;
            }
        }

        public void Save(CacheData data)
        {
            try
            {
                string dir = Path.GetDirectoryName(_cacheFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                string tempPath = _cacheFilePath + ".tmp";
                using (FileStream stream = File.Open(tempPath, FileMode.Create, FileAccess.Write))
                using (BinaryWriter writer = new BinaryWriter(stream))
                {
                    WriteCacheData(writer, data);
                }

                // Atomic-ish swap so a crash mid-write never leaves a corrupt primary file.
                if (File.Exists(_cacheFilePath))
                {
                    File.Delete(_cacheFilePath);
                }
                File.Move(tempPath, _cacheFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to save reference cache: {ex.Message}");
            }
        }

        public void Delete()
        {
            TryDeleteFile();
        }

        private void TryDeleteFile()
        {
            try
            {
                if (File.Exists(_cacheFilePath))
                {
                    File.Delete(_cacheFilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Failed to delete reference cache file: {ex.Message}");
            }
        }

        private static void WriteCacheData(BinaryWriter writer, CacheData data)
        {
            writer.Write(CacheData.CurrentVersion);

            writer.Write(data.Assets.Count);
            foreach (AssetMetadata meta in data.Assets)
            {
                writer.Write(meta.Guid ?? string.Empty);
                writer.Write(meta.Path ?? string.Empty);
                writer.Write(meta.Name ?? string.Empty);
                writer.Write(meta.AssetTypeName ?? string.Empty);
                writer.Write(meta.FileTimestamp);
                writer.Write(meta.MetaTimestamp);
            }

            writer.Write(data.ForwardIndex.Count);
            foreach (CacheData.ForwardEntry entry in data.ForwardIndex)
            {
                writer.Write(entry.Guid ?? string.Empty);
                writer.Write(entry.References.Count);
                foreach (string reference in entry.References)
                {
                    writer.Write(reference);
                }
            }
        }

        private static CacheData ReadCacheData(BinaryReader reader, int version)
        {
            CacheData data = new CacheData { Version = version };

            int assetCount = reader.ReadInt32();
            data.Assets = new List<AssetMetadata>(assetCount);
            for (int i = 0; i < assetCount; i++)
            {
                AssetMetadata meta = new AssetMetadata
                {
                    Guid = reader.ReadString(),
                    Path = reader.ReadString(),
                    Name = reader.ReadString(),
                    AssetTypeName = reader.ReadString(),
                    FileTimestamp = reader.ReadInt64(),
                    MetaTimestamp = reader.ReadInt64()
                };
                data.Assets.Add(meta);
            }

            int forwardCount = reader.ReadInt32();
            data.ForwardIndex = new List<CacheData.ForwardEntry>(forwardCount);
            for (int i = 0; i < forwardCount; i++)
            {
                string guid = reader.ReadString();
                int refCount = reader.ReadInt32();
                List<string> references = new List<string>(refCount);
                for (int r = 0; r < refCount; r++)
                {
                    references.Add(reader.ReadString());
                }
                data.ForwardIndex.Add(new CacheData.ForwardEntry(guid, references));
            }

            return data;
        }
    }
}
