using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
int keyLength = 4;

List<string> KeyList = [];
//GenerateKeys("", alphabet, keyLength)

static void GenerateKeys(string prefix, string alphabet, int keyLength, List<string> keys)
{
    if (prefix.Length == keyLength)
    {
        keys.Add(prefix);
        return;
    }

    foreach (char c in alphabet)
    {
        GenerateKeys(prefix + c, alphabet, keyLength, keys);
    }
}

var client = new HttpClient();

async Task<bool> TestKey(HttpClient client, string key, int count = 0, int maxCount = 3)
{
    try
    {
        string url = $"https://fiapnet.azurewebsites.net/fiap";

        using StringContent jsonContent = new(
            JsonSerializer.Serialize(new KeyAndGroup
            {
                Key = key,
                Group = "Room 3"
            }),
            Encoding.UTF8,
            "application/json");
        
        HttpResponseMessage response = await client.PostAsync(url, jsonContent);

        if (response.IsSuccessStatusCode)
        {
            string result = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Result: {result}");
            if ("Tente novamente grupo: Room 3" == result)
                return false;
            
            
            return true;
        }
        else
        {
            Console.WriteLine($"Erro tentando novamente...");
            await TestKey(client, key, count + 1);
            
            if (count > maxCount)
                return false;
        }
    }
    catch (Exception ex)
    {
        await TestKey(client, key, count + 1);
        if (count > maxCount)
            return false;
    }

    return false;
}


GenerateKeys("", alphabet, keyLength, KeyList);

List<Task> tasks = new List<Task>();
int dop = 1000;

for (int i = 0; i < KeyList.Count; i++)
{
    tasks.Add(TestKey(client, KeyList[i]));

    if ((i + 1) % dop == 0)
    {
        Console.WriteLine($"trying {i + 1}");
        await Task.WhenAll(tasks);
    }
}

await Task.WhenAll(tasks);
Console.WriteLine("fim");

class KeyAndGroup
{
    [JsonPropertyName("Key")]
    public string Key { get; set; }
    [JsonPropertyName("Grupo")]
    public string Group { get; set; }
}