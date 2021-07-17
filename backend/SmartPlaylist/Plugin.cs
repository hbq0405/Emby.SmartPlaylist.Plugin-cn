using System;
using System.Collections.Generic;
using System.IO;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Playlists;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using SmartPlaylist.Configuration;
using SmartPlaylist.Handlers.CommandHandlers;
using SmartPlaylist.Infrastructure.MesssageBus;
using SmartPlaylist.Infrastructure.MesssageBus.Decorators;
using SmartPlaylist.Infrastructure.MesssageBus.Decorators.DebugDecorators;
using SmartPlaylist.PerfLoggerDecorators;
using SmartPlaylist.PerfLoggerDecorators.Services;
using SmartPlaylist.Services;
using SmartPlaylist.Services.SmartPlaylist;

namespace SmartPlaylist
{
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages, IHasThumbImage
    {
        private readonly ILogger _logger;
        private readonly ISessionManager _sessionManager;

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer,
            IPlaylistManager playlistManager,
            ILibraryManager libraryManager,
            ILogger logger, IUserManager userManager, IJsonSerializer jsonSerializer,
            IServerApplicationPaths serverApplicationPaths, ISessionManager sessionManager)
            : base(applicationPaths, xmlSerializer)
        {
            _logger = logger;
            _sessionManager = sessionManager;
            Instance = this;
            var smartPlaylistFileSystem =
                new EnsureBaseDirSmartPlaylistFileSystemDecorator(new SmartPlaylistFileSystem(serverApplicationPaths));
            var smartPlaylistStore = new SmartPlaylistStorePerfLoggerDecorator(new CacheableSmartPlaylistStore(
                new CleanupOldCriteriaDecorator(new SmartPlaylistStore(jsonSerializer, smartPlaylistFileSystem))));
            var userItemsProvider = new UserItemsProviderPerfLoggerDecorator(new UserItemsProvider(libraryManager));
            var smartPlaylistProvider =
                new SmartPlaylistProviderPerfLoggerDecorator(new SmartPlaylistProvider(smartPlaylistStore));
            var playlistItemsUpdater =
                new PlaylistItemsUpdaterPerfLoggerDecorator(new PlayListItemsUpdater(playlistManager));
            var collectionItemsUpdater =
                new PlaylistItemsUpdaterPerfLoggerDecorator(new CollectionItemUpdater(libraryManager));
            FolderRepository =
                new PlaylistRepositoryPerfLoggerDecorator(new FolderRepository(userManager, libraryManager, collectionItemsUpdater, playlistItemsUpdater));

            MessageBus = new MessageBus();

            SubscribeMessageHandlers(smartPlaylistProvider, userItemsProvider, FolderRepository,
                playlistItemsUpdater, smartPlaylistStore, collectionItemsUpdater, libraryManager);

            SmartPlaylistStore = smartPlaylistStore;
            SmartPlaylistValidator = new SmartPlaylistValidator();

            Logger.Instance = new Logger(logger);
        }

        public SmartPlaylistValidator SmartPlaylistValidator { get; }
        public IFolderRepository FolderRepository { get; }

        public override Guid Id => Guid.Parse("3C96F5BC-4182-4B86-B05D-F730F2611E45");

        public override string Name => "Smart Playlist";

        public override string Description => "Allow to define smart playlist and collection rules.";

        public static Plugin Instance { get; private set; }

        public MessageBus MessageBus { get; }
        public ISmartPlaylistStore SmartPlaylistStore { get; }

        public Stream GetThumbImage()
        {
            var type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".Images.plugin.png");
        }

        public ImageFormat ThumbImageFormat => ImageFormat.Png;

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "smartplaylist.html",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.smartplaylist.html",
                    EnableInMainMenu = true,
                    MenuIcon = "subscriptions"
                },
                new PluginPageInfo
                {
                    Name = "smartplaylist.css",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.smartplaylist.css"
                },
                new PluginPageInfo
                {
                    Name = "smartplaylist.js",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.smartplaylist.js"
                }
            };
        }

        private void SubscribeMessageHandlers(ISmartPlaylistProvider smartPlaylistProvider,
            IUserItemsProvider userItemsProvider, IFolderRepository folderRepository,
            IFolderItemsUpdater playlistItemsUpdater, ISmartPlaylistStore smartPlaylistStore,
            IFolderItemsUpdater collectionItemsUpdater, ILibraryManager libraryManager)
        {
            var updateSmartPlaylistCommandHandler =
                new UpdateSmartPlaylistCommandHandler(userItemsProvider, smartPlaylistProvider,
                    folderRepository, playlistItemsUpdater, smartPlaylistStore, collectionItemsUpdater,
                    libraryManager);

            var updateAllSmartPlaylistsWithItemsCommandHandler =
                new UpdateAllSmartPlaylistsCommandHandler(MessageBus, smartPlaylistProvider,
                    folderRepository, playlistItemsUpdater, collectionItemsUpdater);

            MessageBus.Subscribe(Decorate(updateSmartPlaylistCommandHandler));
            MessageBus.Subscribe(Decorate(updateAllSmartPlaylistsWithItemsCommandHandler));
        }

        private IMessageHandler<T> Decorate<T>(IMessageHandler<T> messageHandler) where T : IMessage
        {
            return new SuppressExceptionDecorator<T>(messageHandler, _logger);
        }

        private IMessageHandlerAsync<T> Decorate<T>(IMessageHandlerAsync<T> messageHandler) where T : IMessage
        {
#if DEBUG
            return new SuppressAsyncExceptionDecorator<T>(
                new DebugShowErrorMessageDecorator<T>(
                    new DebugShowDurationMessageDecorator<T>(new PerLoggerMessageDecorator<T>(messageHandler),
                        _sessionManager), _sessionManager),
                _logger);
#else
            return new SuppressAsyncExceptionDecorator<T>(new PerLoggerMessageDecorator<T>(messageHandler), _logger);

#endif
        }
    }
}