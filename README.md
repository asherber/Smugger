![Icon](https://raw.githubusercontent.com/asherber/Smugger/develop/Icon/Smugger-64.png)

# Smugger

A C# library for interacting with v2 of the [SmugMug API](https://api.smugmug.com/api/v2/doc). This is based on [SmugMug.NET](https://github.com/justmarks/SmugMug.NET.v2).

## Usage

```csharp
var client = new SmugMugClient("myApiKey");
var user = await client.GetUserAsync("cmac");
var albums = await client.GetAlbumsAsync(user);
```

For operations not supported by the library, you can also interact directly with the API.

```csharp
// This operation is actually supported, but...
JObject json = await client.GetJsonAsync("/user/cmac!profile");
```

