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
        public CollectionItemUpdater(ILibraryManager libraryManager)
        {
            _libraryManager = libraryManager;
        }

        public async Task<(long internalId, string message)> UpdateAsync(UserFolder folder, BaseItem[] newItems)
        {
            (long internalId, string message) ret = (0, string.Empty);
            if (folder is LibraryUserFolder<Folder> libraryUserCollection)
            {
                var currentItems = libraryUserCollection.GetItems();
                int removed = await RemoveItems(libraryUserCollection, currentItems, folder.SmartPlaylist.IsShuffleUpdateType || folder.SmartPlaylist.Limit.HasLimit ? new BaseItem[] { } : newItems);
                int added = await AddItemsToCollection(libraryUserCollection, folder.SmartPlaylist.IsShuffleUpdateType || folder.SmartPlaylist.Limit.HasLimit ? new BaseItem[] { } : currentItems, newItems).ConfigureAwait(false);
                libraryUserCollection.DynamicUpdate();
                ret = (libraryUserCollection.InternalId, $"Completed - (Removed: {removed} Added: {added} items to the existing collection)");

            }
            else if (newItems.Any())
            {
                BoxSet result = await CreateCollection(new CollectionCreationOptions()
                {
                    ItemIdList = newItems.Select(x => x.InternalId).ToArray<long>(),
                    Name = folder.SmartPlaylist.Name
                }).ConfigureAwait(false);

                ret = (result.InternalId, $"Completed - (Added {newItems.Count()} to new collection)");

            }
            else
                ret = (-1, "Completed - (Collection not created, no items found to add)");

            return ret;
        }

        public Task<int> RemoveItems(UserFolder folder, BaseItem[] currentItems, BaseItem[] newItems)
        {
            List<BaseItem> toRemove = new List<BaseItem>(currentItems.Except(newItems, (c, n) => c.InternalId == n.InternalId));

            if (toRemove.Any() && folder is LibraryUserFolder<Folder> collectionFolder)
            {
                toRemove.ForEach<BaseItem>((BaseItem item) =>
                   {
                       item.RemoveCollection(collectionFolder.InternalId);
                   });

                UpdateItems(collectionFolder.Item, toRemove);
            }

            return Task.FromResult<int>(toRemove.Count);
        }

        private async Task<int> AddItemsToCollection(LibraryUserFolder<Folder> collection, BaseItem[] currentItems, BaseItem[] newItems)
        {
            List<BaseItem> toAdd = new List<BaseItem>(newItems.Except(currentItems, (c, n) => c.InternalId == n.InternalId));
            toAdd.ForEach<BaseItem>((BaseItem item) =>
                {
                    item.AddCollectionInfo(new LinkedItemInfo()
                    {
                        Id = collection.InternalId,
                        Name = collection.SmartPlaylist.Name
                    });
                });

            await Task.Run(() =>
            {
                UpdateItems(collection.Item, toAdd);

            }).ConfigureAwait(false);

            return toAdd.Count;
        }

        private void UpdateItems(Folder collection, List<BaseItem> items)
        {
            _libraryManager.UpdateItems(
                            items,
                            collection,
                            ItemUpdateType.None,
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
                        itemById.UpdateToRepository(ItemUpdateType.None);
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

        public Task<int> ClearPlaylist(UserFolder folder)
        {
            if (folder is LibraryUserFolder<Folder> collectionFolder)
            {
                BaseItem[] items = collectionFolder.GetItems();
                items.ForEach<BaseItem>((BaseItem item) =>
                   {
                       item.RemoveCollection(collectionFolder.InternalId);
                   });

                UpdateItems(collectionFolder.Item, new List<BaseItem>(items));
                return Task.FromResult<int>(items.Length);
            }
            return Task.FromResult<int>(0);
        }
    }
}