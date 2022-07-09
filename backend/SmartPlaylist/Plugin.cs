using System;
using System.Collections.Generic;
using System.IO;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Collections;
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
        public readonly ILogger Logger;
        private readonly ISessionManager _sessionManager;
        public SmartPlaylistValidator SmartPlaylistValidator { get; }
        public IFolderRepository FolderRepository { get; }
        public override Guid Id => Guid.Parse("3C96F5BC-4182-4B86-B05D-F730F2611E45");
        public override string Name => "Smart Playlist";
        public override string Description => "Allow to define smart playlist and collection rules.";
        public static Plugin Instance { get; private set; }
        public MessageBus MessageBus { get; }
        public UpdateSmartPlaylistCommandHandler SmartPlaylistCommandHandler { get; }
        public ISmartPlaylistStore SmartPlaylistStore { get; }
        public ILibraryManager LibraryManager { get; }
        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer,
            IPlaylistManager playlistManager,
            ILibraryManager libraryManager,
            ILogger logger, IUserManager userManager, IJsonSerializer jsonSerializer,
            IServerApplicationPaths serverApplicationPaths, ISessionManager sessionManager, ICollectionManager collectionManager)
            : base(applicationPaths, xmlSerializer)
        {
            Logger = logger;
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
                new PlaylistItemsUpdaterPerfLoggerDecorator(new CollectionItemUpdater(libraryManager, collectionManager));
            FolderRepository =
                new PlaylistRepositoryPerfLoggerDecorator(new FolderRepository(userManager, libraryManager, collectionItemsUpdater, playlistItemsUpdater));

            MessageBus = new MessageBus();
            SmartPlaylistCommandHandler =
                new UpdateSmartPlaylistCommandHandler(userItemsProvider, smartPlaylistProvider,
                    FolderRepository, playlistItemsUpdater, smartPlaylistStore, collectionItemsUpdater,
                    libraryManager);

            LibraryManager = libraryManager;
            SubscribeMessageHandlers(smartPlaylistProvider, userItemsProvider, FolderRepository,
                playlistItemsUpdater, smartPlaylistStore, collectionItemsUpdater, libraryManager);

            SmartPlaylistStore = smartPlaylistStore;
            SmartPlaylistValidator = new SmartPlaylistValidator();

            SmartPlaylist.Logger.Instance = new Logger(logger);
        }

        public Stream GetThumbImage()
        {
            Type type = GetType();
            return type.Assembly.GetManifestResourceStream(type.Namespace + ".Configuration.thumb.png");
        }

        public ImageFormat ThumbImageFormat => ImageFormat.Png;

        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "smartplaylist.2.3.0.3.html",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.smartplaylist.2.3.0.3.html",
                    EnableInMainMenu = true,
                    MenuIcon = "subscriptions"
                },
                new PluginPageInfo
                {
                    Name = "smartplaylist.2.3.0.3.css",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.smartplaylist.2.3.0.3.css"
                },
                new PluginPageInfo
                {
                    Name = "smartplaylist.2.3.0.3.js",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.smartplaylist.2.3.0.3.js"
                }

            };
        }

        private void SubscribeMessageHandlers(ISmartPlaylistProvider smartPlaylistProvider,
            IUserItemsProvider userItemsProvider, IFolderRepository folderRepository,
            IFolderItemsUpdater playlistItemsUpdater, ISmartPlaylistStore smartPlaylistStore,
            IFolderItemsUpdater collectionItemsUpdater, ILibraryManager libraryManager)
        {
            var updateAllSmartPlaylistsWithItemsCommandHandler =
                new UpdateAllSmartPlaylistsCommandHandler(MessageBus, smartPlaylistProvider,
                    folderRepository, playlistItemsUpdater, collectionItemsUpdater, smartPlaylistStore);

            var sortAllSmartPlaylistsCommandHandler =
            new SortAllSmartPlaylistsCommandHandler(smartPlaylistProvider, folderRepository, userItemsProvider,
                    collectionItemsUpdater, playlistItemsUpdater, smartPlaylistStore);

            MessageBus.Subscribe(Decorate(SmartPlaylistCommandHandler));
            MessageBus.Subscribe(Decorate(updateAllSmartPlaylistsWithItemsCommandHandler));
            MessageBus.Subscribe(Decorate(sortAllSmartPlaylistsCommandHandler));

        }

        private IMessageHandler<T> Decorate<T>(IMessageHandler<T> messageHandler) where T : IMessage
        {
            return new SuppressExceptionDecorator<T>(messageHandler, Logger);
        }

        private IMessageHandlerAsync<T> Decorate<T>(IMessageHandlerAsync<T> messageHandler) where T : IMessage
        {
#if DEBUG
            return new SuppressAsyncExceptionDecorator<T>(
                new DebugShowErrorMessageDecorator<T>(
                    new DebugShowDurationMessageDecorator<T>(new PerLoggerMessageDecorator<T>(messageHandler),
                        _sessionManager), _sessionManager),
                Logger);
#else
            return new SuppressAsyncExceptionDecorator<T>(new PerLoggerMessageDecorator<T>(messageHandler), Logger);

#endif
        }
    }
}