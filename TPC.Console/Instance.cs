using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TPC.Console;

public class Instance
{
    public Instance(string iPAddress, int port, DateTime lastUpdate)
    {
        IPAddress = iPAddress;
        Port = port;
        LastUpdate = lastUpdate;
    }

    [JsonPropertyName("address")]
    public string IPAddress { get; set; }
    [JsonPropertyName("port")]
    public int Port { get; set; }
    [JsonPropertyName("last_update")]
    public DateTime LastUpdate { get; set; }
}

public static class InstanceEngine
{
    public static string path = Environment.CurrentDirectory + "/SavedInstances/";

    public static async Task SaveAsync(this Instance instance)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var files = Directory.GetFiles(path, "*.json");
        bool instanceUpdated = false;

        foreach (var file in files)
        {
            string fileContent = await File.ReadAllTextAsync(file);
            var obj = JsonSerializer.Deserialize<Instance>(fileContent);

            if (obj != null && instance.Compare(obj))
            {
                obj.LastUpdate = DateTime.Now;

                var updatedJson = JsonSerializer.Serialize(obj);
                await File.WriteAllTextAsync(file, updatedJson);
                instanceUpdated = true;
                break; 
            }
        }

        if (!instanceUpdated)
        {
            var json = JsonSerializer.Serialize(instance);
            string fileName = $"instance_{ConvertDateTimeToString(instance.LastUpdate)}";
            await File.WriteAllTextAsync(path + fileName + ".json", json);
        }
    }

    public static List<Instance> GetInstances()
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);

            return new List<Instance>();
        }

        var files = Directory.GetFiles(path, "*.json");
        var instances = new List<Instance>();

        foreach (var file in files)
        {
            string fileContent = File.ReadAllText(file);
            var instance = JsonSerializer.Deserialize<Instance>(fileContent);

            if (instance != null)
            {
                instances.Add(instance);
            }
        }

        instances.Sort((x, y) => y.LastUpdate.CompareTo(x.LastUpdate));

        return instances;
    }

    private static string ConvertDateTimeToString(DateTime time)
    {
        string input = time.ToString();

        string invalidChars = new string(Path.GetInvalidFileNameChars());
        string replaceChar = "_";

        foreach (char invalidChar in invalidChars)
        {
            input = input.Replace(invalidChar.ToString(), replaceChar);
        }

        return input;
    } 

    private static bool Compare(this Instance instance, Instance comparedInstance)
    {
        return instance.IPAddress == comparedInstance.IPAddress && instance.Port == comparedInstance.Port;
    }
}
