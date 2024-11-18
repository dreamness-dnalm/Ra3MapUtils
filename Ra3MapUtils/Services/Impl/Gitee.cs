using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using UtilLib.utils;
using Velopack.Util;

namespace Velopack.Sources;


    public class GiteeSource(string owner, string repoName, string? accessToken, bool prerelease, IFileDownloader? downloader = null)
        : GitBase<GiteeRelease>($"https://gitee.com/{owner}/{repoName}", accessToken, prerelease, downloader)
    {
        protected override async Task<GiteeRelease[]> GetReleases(bool includePrereleases)
        {
            
            GiteeSource giteeSource = this;
            // DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 3);
            // interpolatedStringHandler.AppendLiteral("repos");
            // interpolatedStringHandler.AppendFormatted(giteeSource.RepoUri.AbsolutePath);
            // interpolatedStringHandler.AppendLiteral("/releases?per_page=");
            // interpolatedStringHandler.AppendFormatted<int>(10);
            // interpolatedStringHandler.AppendLiteral("&page=");
            // interpolatedStringHandler.AppendFormatted<int>(1);
            // string stringAndClear = interpolatedStringHandler.ToStringAndClear();
            
            
            // Uri uri = new Uri(giteeSource.GetApiBaseUrl(giteeSource.RepoUri), $"/releases?per_page=10&page=1");
            var url = $"https://gitee.com/api/v5/repos/{owner}/{repoName}/releases?per_page=10&page=1";
            Logger.WriteLog(url);
            List<GiteeRelease> source = DeserializeGiteeReleaseList(await giteeSource.Downloader.DownloadString(url, giteeSource.Authorization, "application/json").ConfigureAwait(false));
            var ret = source != null
                ? source.OrderByDescending<GiteeRelease, DateTime?>((Func<GiteeRelease, DateTime?>)(d => d.PublishedAt))
                    .Where<GiteeRelease>((Func<GiteeRelease, bool>)(x => includePrereleases || !x.Prerelease))
                    .ToArray<GiteeRelease>()
                : Array.Empty<GiteeRelease>();
            // return new GiteeRelease[0];
            Logger.WriteLog($"release cnt: {ret.Length}");
            Logger.WriteLog(ret[0].ToString());
            return ret;
        }

        protected override string GetAssetUrlFromName(GiteeRelease release, string assetName)
        {
            if (release.Assets == null || release.Assets.Length == 0)
                throw new ArgumentException("No assets found in Gitee Release '" + release.Name + "'.");
            IEnumerable<GiteeReleaseAsset> source = ((IEnumerable<GiteeReleaseAsset>) release.Assets).Where<GiteeReleaseAsset>((Func<GiteeReleaseAsset, bool>) (a =>
            {
                string name = a.Name;
                return name != null && name.Equals(assetName, StringComparison.InvariantCultureIgnoreCase);
            }));
            if (!source.Any<GiteeReleaseAsset>())
            {
                DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(52, 2);
                interpolatedStringHandler.AppendLiteral("Could not find asset called '");
                interpolatedStringHandler.AppendFormatted(assetName);
                interpolatedStringHandler.AppendLiteral("' in GitHub Release '");
                interpolatedStringHandler.AppendFormatted(release.Name);
                interpolatedStringHandler.AppendLiteral("'.");
                throw new ArgumentException(interpolatedStringHandler.ToStringAndClear());
            }
            GiteeReleaseAsset githubReleaseAsset = source.First<GiteeReleaseAsset>();
            if (string.IsNullOrWhiteSpace(this.AccessToken) && githubReleaseAsset.BrowserDownloadUrl != null)
                return githubReleaseAsset.BrowserDownloadUrl;
            return githubReleaseAsset.BrowserDownloadUrl != null ? githubReleaseAsset.BrowserDownloadUrl : throw new ArgumentException("Could not find a valid asset url for the specified asset.");
        }
        
        // protected virtual Uri GetApiBaseUrl(Uri repoUrl)
        // {
        //     return !repoUrl.Host.EndsWith("gitee.com", StringComparison.OrdinalIgnoreCase) ? new Uri(string.Format("{0}{1}{2}/api/v5/", (object) repoUrl.Scheme, (object) Uri.SchemeDelimiter, (object) repoUrl.Host)) : new Uri("https://gitee.com/");
        // }
        
        // public JsonSerializerOptions Options
        // {
        //     get
        //     {
        //         var options = new JsonSerializerOptions()
        //         {
        //             TypeInfoResolver = (IJsonTypeInfoResolver) this
        //         };
        //         options.MakeReadOnly();
        //         return options;
        //     }
        // }
        //
        // public JsonTypeInfo<List<GiteeRelease>> ListGiteeRelease
        // {
        //     get
        //     {
        //         return  (JsonTypeInfo<List<GiteeRelease>>) this.Options.GetTypeInfo(typeof (List<GiteeRelease>));
        //     }
        // }

        public List<GiteeRelease>? DeserializeGiteeReleaseList(string json)
        {
            Logger.WriteLog(json);
            return JsonSerializer.Deserialize<List<GiteeRelease>>(json);
        }

        public static void test()
        {
            var url = "https://gitee.com/dreamness/update_test";
            
            DefaultInterpolatedStringHandler interpolatedStringHandler = new DefaultInterpolatedStringHandler(30, 3);
            interpolatedStringHandler.AppendLiteral("repos");
            interpolatedStringHandler.AppendFormatted(url);
            interpolatedStringHandler.AppendLiteral("/releases?per_page=");
            interpolatedStringHandler.AppendFormatted<int>(10);
            interpolatedStringHandler.AppendLiteral("&page=");
            interpolatedStringHandler.AppendFormatted<int>(1);
            string stringAndClear = interpolatedStringHandler.ToStringAndClear();
            Console.WriteLine(stringAndClear);

            var s = $"repos{url}/releases?per_page=10&page=1";
            Console.WriteLine(s);
            Console.WriteLine(stringAndClear == s);

            // var apiBase = !url.Host.EndsWith("gitee.com", StringComparison.OrdinalIgnoreCase) ? new Uri(string.Format("{0}{1}{2}/api/v5/", (object) repoUrl.Scheme, (object) Uri.SchemeDelimiter, (object) repoUrl.Host)) : new Uri("https://gitee.com/");
            //
            // Uri uri = new Uri(giteeSource.GetApiBaseUrl(giteeSource.RepoUri), stringAndClear);
        }
    }


public class GiteeReleaseAsset
{
    // [JsonPropertyName("url")]
    // public string? Url { get; set; }
        
    [JsonPropertyName("browser_download_url")]
    public string? BrowserDownloadUrl { get; set; }
        
    [JsonPropertyName("name")]
    public string? Name { get; set; }
        
    // [JsonPropertyName("content_type")]
    // public string? ContentType { get; set; }

    public override string ToString()
    {
        return "GithubReleaseAsset{" + Name + ", " + BrowserDownloadUrl + "}";
    }
}
    
public class GiteeRelease
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
        
    [JsonPropertyName("prerelease")]
    public bool Prerelease { get; set; }
        
    [JsonPropertyName("create_at")]
    public DateTime? PublishedAt { get; set; }
        
    [JsonPropertyName("assets")]
    public GiteeReleaseAsset[] Assets { get; set; } = new GiteeReleaseAsset[0];

    public override string ToString()
    {
        var ret = "GrieeRelease{" + Name + ", " + Prerelease + ", " + PublishedAt + ", " + Assets.Length + "}";
        ret += "[";
        foreach (var asset in Assets)
        {
            ret += ", " + asset.ToString();
        }
        ret += "]";

        return ret;
    }
}