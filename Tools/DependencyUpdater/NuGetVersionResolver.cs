using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace DependencyUpdater;

/// <summary>
/// Resolves available package versions from the sources configured in NuGet.config
/// (respecting the repo's own feed list). Versions are cached per package id.
/// </summary>
public sealed class NuGetVersionResolver : IDisposable
{
    private readonly List<SourceRepository> _repositories;
    private readonly SourceCacheContext _cache = new();
    private readonly ILogger _logger = NullLogger.Instance;
    private readonly Dictionary<string, IReadOnlyList<NuGetVersion>> _versionCache = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<string> SourceNames { get; }

    public NuGetVersionResolver(string settingsRoot)
    {
        var settings = Settings.LoadDefaultSettings(settingsRoot);
        var sourceProvider = new PackageSourceProvider(settings);
        var sources = sourceProvider.LoadPackageSources().Where(s => s.IsEnabled).ToList();

        var providers = Repository.Provider.GetCoreV3();
        _repositories = sources.Select(s => new SourceRepository(s, providers)).ToList();
        SourceNames = sources.Select(s => s.Name).ToList();
    }

    /// <summary>All listed versions of a package, aggregated across every enabled source.</summary>
    public async Task<IReadOnlyList<NuGetVersion>> GetVersionsAsync(string packageId, CancellationToken ct)
    {
        if (_versionCache.TryGetValue(packageId, out var cached))
            return cached;

        var all = new SortedSet<NuGetVersion>();
        foreach (var repo in _repositories)
        {
            try
            {
                var metadata = await repo.GetResourceAsync<MetadataResource>(ct);
                if (metadata is null)
                    continue;

                var versions = await metadata.GetVersions(
                    packageId, includePrerelease: true, includeUnlisted: false, _cache, _logger, ct);
                foreach (var v in versions)
                    all.Add(v);
            }
            catch (Exception)
            {
                // A source that is unreachable or lacks the package is skipped;
                // other sources may still satisfy the reference.
            }
        }

        var list = all.ToList();
        _versionCache[packageId] = list;
        return list;
    }

    public void Dispose() => _cache.Dispose();
}
