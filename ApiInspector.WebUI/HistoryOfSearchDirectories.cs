﻿using System.IO;
using Newtonsoft.Json;

namespace ApiInspector.WebUI;

static class HistoryOfSearchDirectories
{
    static readonly string filePath = Path.Combine(CacheDirectory.CacheDirectoryPath, $"{nameof(HistoryOfSearchDirectories)}.json");

    static readonly List<string> value;

    static HistoryOfSearchDirectories()
    {
        if (ExistInStore(filePath))
        {
            value = JsonConvert.DeserializeObject<List<string>>(ReadFile(filePath));
        }
        else
        {
            value = new List<string>();
        }
    }

    public static IReadOnlyList<string> Value => value;

    public static void AddIfNotExists(string path)
    {
        if (value.Contains(path))
        {
            return;
        }

        value.Add(path);

        SaveFile(filePath, JsonConvert.SerializeObject(value, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented
        }));
    }
}