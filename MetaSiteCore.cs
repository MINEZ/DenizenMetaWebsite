using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DenizenMetaWebsite.MetaObjects;
using FreneticUtilities.FreneticDataSyntax;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticToolkit;
using SharpDenizenTools.MetaHandlers;
using SharpDenizenTools.MetaObjects;

namespace DenizenMetaWebsite
{
    public class MetaSiteCore
    {
        public static FDSSection Config;

        public static string ReloadWebhookToken;

        public static List<WebsiteMetaCommand> Commands;

        public static List<WebsiteMetaTag> Tags;

        public static List<WebsiteMetaObjectType> ObjectTypes;

        public static List<WebsiteMetaEvent> Events;

        public static List<WebsiteMetaAction> Actions;

        public static List<WebsiteMetaLanguage> Languages;

        public static List<WebsiteMetaMechanism> Mechanisms;

        public static List<WebsiteMetaObject> AllObjects;

        public static void Init()
        {
            Config = FDSUtility.ReadFile("config/config.fds");
            ReloadWebhookToken = Config.GetString("reload-webhook-token");
            if (Config.HasKey("alt-sources"))
            {
                MetaDocsLoader.SourcesToUse = [.. Config.GetStringList("alt-sources")];
            }
            if (Config.HasKey("header-line"))
            {
                Util.HeaderLine = Config.GetString("header-line");
            }
            MetaDocsLoader.AlternateZipSourcer = CachedSourcer;
            ReloadMeta();
        }

        /// <summary>Folder that downloaded source archives are cached in.</summary>
        public static string SourceCacheFolder = "./cache/sources/";

        /// <summary>How long a cached source archive stays usable without re-downloading.</summary>
        public static double SourceCacheHours = 12;

        /// <summary>When true, cached archives are ignored and everything is re-downloaded.</summary>
        public static bool BypassSourceCache = false;

        /// <summary>Maps a source URL to the file it is cached in.</summary>
        public static string CacheFileFor(string url)
        {
            char[] chars = url.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                char c = chars[i];
                if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '.' || c == '-' || c == '_'))
                {
                    chars[i] = '_';
                }
            }
            return SourceCacheFolder + new string(chars) + ".zip";
        }

        /// <summary>
        /// Downloads a source archive, keeping a copy on disk.
        /// A recent enough cached copy is used as-is, and an outdated copy is still preferred over failing outright.
        /// </summary>
        public static byte[] CachedSourcer(string url, HttpClient webClient)
        {
            Directory.CreateDirectory(SourceCacheFolder);
            string cacheFile = CacheFileFor(url);
            bool cached = File.Exists(cacheFile);
            if (cached && !BypassSourceCache && DateTime.UtcNow.Subtract(File.GetLastWriteTimeUtc(cacheFile)).TotalHours < SourceCacheHours)
            {
                Console.WriteLine($"Using cached copy of {url}");
                return File.ReadAllBytes(cacheFile);
            }
            try
            {
                byte[] data = webClient.GetByteArrayAsync(url).Result;
                if (data is not null && data.Length > 0)
                {
                    File.WriteAllBytes(cacheFile, data);
                    return data;
                }
                Console.Error.WriteLine($"Empty response for {url}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Download failed for {url}: {ex.GetType().Name}: {ex.Message}");
            }
            if (cached)
            {
                Console.Error.WriteLine($"Falling back to the cached copy of {url}");
                return File.ReadAllBytes(cacheFile);
            }
            throw new IOException($"Failed to download {url} and no cached copy is available.");
        }

        public static DateTimeOffset LastReload;

        public static LockObject ReloadTimeLock = new(), ReloadLock = new();

        public static void ReloadMeta(bool forceFresh = false)
        {
            lock (ReloadTimeLock)
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                if (now.Subtract(LastReload).TotalSeconds < 15)
                {
                    Console.WriteLine("Ignoring too-fast reload...");
                    return;
                }
                LastReload = now;
            }
            lock (ReloadLock)
            {
                Console.WriteLine("Reloading meta...");
                ExtraData.CachePath = "./wwwroot/data/minecraft.fds";
                ExtraData.ForceCache = true;
                MetaDocs docs;
                try
                {
                    BypassSourceCache = forceFresh;
                    docs = MetaDocsLoader.DownloadAll();
                }
                finally
                {
                    BypassSourceCache = false;
                }
                if (docs.SourceLoadFailed && AllObjects is not null)
                {
                    Console.Error.WriteLine("Meta reload aborted: a source failed to load, keeping the previously loaded meta.");
                    return;
                }
                Console.WriteLine("Meta loaded, HTMLizing...");
                List<WebsiteMetaCommand> _commands = [];
                List<WebsiteMetaTag> _tags = [];
                List<WebsiteMetaObjectType> _objectTypes = [];
                List<WebsiteMetaEvent> _events = [];
                List<WebsiteMetaAction> _actions = [];
                List<WebsiteMetaLanguage> _languages = [];
                List<WebsiteMetaMechanism> _mechanisms = [];
                List<WebsiteMetaObject> _allObjects = [];
                void procSet<T, T2>(ref List<T> webObjs, ICollection<T2> origObjs) where T: WebsiteMetaObject<T2>, new() where T2: MetaObject
                {
                    foreach (T2 obj in origObjs)
                    {
                        T webObj = new() { Object = obj };
                        webObjs.Add(webObj);
                    }
                    webObjs = webObjs.OrderBy(o => string.IsNullOrWhiteSpace(o.Object.Plugin) ? 0 : 1).ThenBy(o => o.Object.Warnings.Count).ThenBy(o => o.Object.Group).ThenBy(o => o.Object.CleanName).ToList();
                    _allObjects.AddRange(webObjs);
                }
                procSet(ref _commands, docs.Commands.Values);
                procSet(ref _tags, docs.Tags.Values);
                procSet(ref _objectTypes, docs.ObjectTypes.Values);
                procSet(ref _events, docs.Events.Values);
                procSet(ref _actions, docs.Actions.Values);
                procSet(ref _languages, docs.Languages.Values);
                procSet(ref _mechanisms, docs.Mechanisms.Values);
                foreach (WebsiteMetaObject obj in _allObjects)
                {
                    obj.Docs = docs;
                    obj.LoadHTML();
                }
                Commands = _commands;
                Tags = _tags;
                ObjectTypes = _objectTypes;
                Events = _events;
                Actions = _actions;
                Languages = _languages;
                Mechanisms = _mechanisms;
                AllObjects = _allObjects;
                MetaDocs.CurrentMeta = docs;
                Console.WriteLine("Meta loaded and ready!");
            }
        }
    }
}
