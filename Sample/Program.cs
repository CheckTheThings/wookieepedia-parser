using System.Text.Json;
using CheckTheThings.StarWars.Wookieepedia;

var url = "https://starwars.fandom.com/wiki/Timeline_of_canon_media";
var httpClient = new HttpClient();
var response = await httpClient.GetAsync(url);

var mediaItems = await TimelineParser.Parse(await response.Content.ReadAsStreamAsync());
var sortedMediaItems = mediaItems
    .Where(m => m.ReleaseDate is not null)
    .OrderBy(m => m.ReleaseDate)
    .ThenBy(m => m.Name)
    .ToList();

var fileName = "media.json";
var json = JsonSerializer.Serialize(sortedMediaItems, new JsonSerializerOptions { WriteIndented = true });
await File.WriteAllTextAsync(fileName, json);
