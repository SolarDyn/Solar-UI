// JsonConfig.cs
// Solar Dynamics 2026

#region

using System;
using System.IO;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#endregion

namespace Solar.UI;

public sealed class JsonConfig
{
    public string FilePath { get; }
    public JObject Data { get; }

    public JsonConfig(
        Assembly pluginAssembly,
        string relativePath,
        JObject defaults)
    {
        FilePath = Path.Combine(
            Path.GetDirectoryName(pluginAssembly.Location)!,
            relativePath);

        if (File.Exists(FilePath))
        {
            try
            {
                Data = JObject.Parse(File.ReadAllText(FilePath));
            }
            catch (Exception e)
            {
                Plugin.LoggerCache!.LogError($"Failed to parse JSON config file at {FilePath}: {e}");
            }
        }

        Data ??= new JObject();

        if (AddDefaults(Data, defaults) || !File.Exists(FilePath))
            Save();
    }

    private static bool AddDefaults(JObject target, JObject defaults)
    {
        bool changed = false;

        foreach (JProperty property in defaults.Properties())
        {
            JProperty? existing = target.Property(property.Name) ?? null;

            if (existing == null)
            {
                target.Add(property.Name, property.Value.DeepClone());
                changed = true;
            }
            else if (existing.Value is JObject targetObject &&
                     property.Value is JObject defaultObject)
            {
                changed |= AddDefaults(targetObject, defaultObject);
            }
        }

        return changed;
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        File.WriteAllText(FilePath, Data.ToString(Formatting.Indented));
    }

    public void UpdateDefaults(JObject defaults)
    {
        if (AddDefaults(Data, defaults) || !File.Exists(FilePath))
            Save();
    }
}