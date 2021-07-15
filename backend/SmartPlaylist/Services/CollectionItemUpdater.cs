using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Collections;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using SmartPlaylist.Domain;
using SmartPlaylist.Extensions;

namespace SmartPlaylist.Services
{
    public class CollectionItemUpdater : IFolderItemsUpdater
    {
        private readonly ILibraryManager _libraryManager;
        private readonly IFolderRepository _folderRepository;
        public CollectionItemUpdater(ILibraryManager libraryManager, IFolderRepository folderRepository)
        {
            _libraryManager = libraryManager;
            _folderRepository = folderRepository;
        }

        public async Task UpdateAsync(UserFolder folder, BaseItem[] newItems)
        {
            if (folder is LibraryUserFolder<Folder> libraryUserCollection)
            {
                RemoveItemsFromCollection(libraryUserCollection);
                await AddItemsToCollection(libraryUserCollection, new List<BaseItem>(newItems)).ConfigureAwait(false);
            }
            else
            {
                await CreateCollection(new CollectionCreationOptions()
                {
                    ItemIdList = newItems.Select(x => x.InternalId).ToArray<long>(),
                    Name = folder.SmartPlaylist.Name
                }).ConfigureAwait(false);
            }

            foreach (string priorName in folder.SmartPlaylist.ToDto().PriorNames)
            {
                if (!string.Equals(folder.SmartPlaylist.Name, priorName, StringComparison.OrdinalIgnoreCase))
                {
                    Folder priorFolder = _folderRepository.FindCollectionFolder(folder.SmartPlaylist, priorName);
                    if (priorFolder != null)
                        RemoveItemsFromCollection(priorFolder, new List<BaseItem>(priorFolder.GetChildren(folder.User)));
                }
            }
        }

        private void RemoveItemsFromCollection(LibraryUserFolder<Folder> collection)
        {
            RemoveItemsFromCollection(collection.Item, new List<BaseItem>(collection.GetItems()));
        }

        private void RemoveItemsFromCollection(Folder collection, List<BaseItem> items)
        {
            items.ForEach<BaseItem>((BaseItem item) =>
               {
                   item.RemoveCollection(collection.InternalId);
               });

            UpdateItems(collection, items);
        }

        private async Task AddItemsToCollection(LibraryUserFolder<Folder> collection, List<BaseItem> items)
        {
            items.ForEach<BaseItem>((BaseItem item) =>
                {
                    item.AddCollectionInfo(new LinkedItemInfo()
                    {
                        Id = collection.InternalId,
                        Name = collection.SmartPlaylist.Name
                    });
                });

            await Task.Run(() =>
            {
                UpdateItems(collection.Item, items);
            }).ConfigureAwait(false);
        }

        private void UpdateItems(Folder collection, List<BaseItem> items)
        {
            _libraryManager.UpdateItems(
                            items,
                            collection,
                            ItemUpdateType.MetadataEdit,
                            new CancellationToken(false));

        }
        private List<Folder> FindFolders() => this._libraryManager.GetUserRootFolder().GetChildren(new InternalItemsQuery()
        {
            IsFolder = new bool?(true)
        }).OfType<CollectionFolder>().ToList<CollectionFolder>()
            .Where<CollectionFolder>(
                (Func<CollectionFolder, bool>)
                (
                    i => MemoryExtensions.Equals(i.CollectionType.AsSpan(), CollectionType.BoxSets.Span, StringComparison.OrdinalIgnoreCase))
                ).OfType<Folder>().ToList<Folder>();


        public async Task<BoxSet> CreateCollection(CollectionCreationOptions options)
        {
            Folder folder = await EnsureLibraryFolder(true).ConfigureAwait(false);
            BoxSet newEntry = null;

            foreach (long itemId in options.ItemIdList)
            {
                BaseItem itemById = _libraryManager.GetItemById(itemId);
                if (itemById != null)
                {
                    if (newEntry == null)
                        newEntry = AddCollection(itemById, options.Name);
                    else if (itemById.AddCollection(newEntry))
                        itemById.UpdateToRepository(ItemUpdateType.MetadataEdit);
                }
            }
            return newEntry;
        }

        internal async Task<Folder> EnsureLibraryFolder(bool createIfNeeded)
        {
            List<Folder> folders = this.FindFolders();
            if (folders.Count > 0)
                return folders[0];
            if (!createIfNeeded)
                return (Folder)null;
            LibraryOptions options = new LibraryOptions()
            {
                EnableRealtimeMonitor = false,
                SaveLocalMetadata = true,
                ContentType = CollectionType.BoxSets.ToString()
            };

            await this._libraryManager.AddVirtualFolder("Collections", options, true).ConfigureAwait(false);
            return this.FindFolders().FirstOrDefault<Folder>();
        }


        private BoxSet AddCollection(BaseItem item, string collectionName)
        {
            LinkedItemInfo linkedItemInfo = item.AddCollection(collectionName);
            if (!linkedItemInfo.Id.Equals(0L))
                return (BoxSet)this._libraryManager.GetItemById(linkedItemInfo.Id);
            item.UpdateToRepository(ItemUpdateType.MetadataEdit);
            return (BoxSet)this._libraryManager.GetItemById(linkedItemInfo.Id);
        }


    }
}