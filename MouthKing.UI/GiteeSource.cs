using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Velopack.Sources;

namespace MouthKing.UI;

public class GiteeSource : GitBase<GiteeRelease>
{
    public GiteeSource(string repoUrl, string accessToken, bool upcomingRelease, IFileDownloader? downloader = null) : base(repoUrl, accessToken, upcomingRelease, downloader)
    {
    }

    protected override string GetAssetUrlFromName(GiteeRelease release, string assetName)
    {
        if (release.assets == null || release.assets.Count == 0)
        {
            throw new ArgumentException($"No assets found in Gitee Release '{release.name}'.");
        }

        Asset? packageFile =
            release.assets.FirstOrDefault(a => a.name?.Equals(assetName, StringComparison.InvariantCultureIgnoreCase) == true);
        if (packageFile == null)
        {
            throw new ArgumentException($"Could not find asset called '{assetName}' in Gitee Release '{release.name}'.");
        }

        if (String.IsNullOrWhiteSpace(AccessToken) && packageFile.browser_download_url != null)
        {
            return packageFile.browser_download_url;
        }
        else if (packageFile.browser_download_url != null)
        {
            return packageFile.browser_download_url;
        }
        else
        {
            throw new Exception($"Could not find a valid URL for asset '{assetName}' in Gitee Release '{release.name}'.");
        }
    }

    protected override async Task<GiteeRelease[]> GetReleases(bool includePrereleases)
    {
        var releasesPath = $"repos{RepoUri.AbsolutePath}/releases";
        var baseUri = new Uri("https://gitee.com/api/v5/");
        var getReleasesUri = new Uri(baseUri, releasesPath);
        var response = await Downloader.DownloadString(getReleasesUri.ToString(), Authorization).ConfigureAwait(false);
        var releases = CompiledJson.DeserializeGiteeReleaseList(response);
        if (releases == null) return new GiteeRelease[0];
        return releases.OrderByDescending(d => d.created_at).Where(x => includePrereleases || !x.prerelease).ToArray();
    }
}

public class GiteeRelease
{
    public int id { get; set; }
    public string tag_name { get; set; }
    public string target_commitish { get; set; }
    public bool prerelease { get; set; }
    public string name { get; set; }
    public string body { get; set; }
    public DateTime created_at { get; set; }
    public List<Asset> assets { get; set; }
}

public class Asset
{
    public string browser_download_url { get; set; }
    public string name { get; set; }
}

[JsonSerializable(typeof(List<GiteeRelease>))]
[JsonSourceGenerationOptions(UseStringEnumConverter = true)]
internal partial class CompiledJsonSourceGenerationContext : JsonSerializerContext
{
}

internal static class CompiledJson
{
    private static readonly JsonSerializerOptions Options = new()
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = {
                new SemanticVersionConverter(),
            },
    };

    private static readonly CompiledJsonSourceGenerationContext Context = new(Options);


    public static List<GiteeRelease>? DeserializeGiteeReleaseList(string json)
    {
        return JsonSerializer.Deserialize(json, Context.ListGiteeRelease);
    }

}

internal class SemanticVersionConverter : JsonConverter<SemanticVersion>
{
    public override SemanticVersion? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var str = reader.GetString();
        if (str == null) return null;
        return SemanticVersion.Parse(str);
    }

    public override void Write(Utf8JsonWriter writer, SemanticVersion value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToFullString());
    }
}