# ChangeLog
## Version 2.1.0.3
- ### Feature
- Added "Country" Filter Criteria. Filter item on the "ProductionLocation" metadata.
- Added "Genre (List)" Filter Criteria. Displays a drop down list of Genres already in the system, if the Genre does not exist the fall back it to use the already existing "Genre" string filter.
- Added checkbox on delete confirmation to retain the generated playlist/collection
- Sorted playlists/collections alphabetically
- Added "Tag (Tree Crawl)" Criteria. Filters the item on tag or its parents tags (and so on, all the way to to root).
- Added "Studio (Tree Crawl)" Criteria. Filters the item on it's studio value or its parents studio value (and so on, all the way to to root).
- Updating sourcing to navigate to the lowest item in the path.
- Added new Update types: Daily, Weekly, Monthly, The main difference between this and the shuffle update types is that the shuffle will remove all items in the playlist/collection prior to adding new items, and then shuffle the playlist randomly, where as the scheduled update types will only add any new items to the playlist/collection and NOT shuffle the playlist/collection. Scheduled update types are way better when it comes to performance as there are only small updates to the the backend after the initial populate.
- Added updateType Icons
- Added tooltips to buttons and icons
## Version 2.1.0.2
- ### Performance
  - Optimized addition and removal of items to a playlist or collection.
- ### Bugs
  - Fixed a bug where images could be removed under certain circumstances.
  - Fixed bug where it sometimes would not remove the related playlist when deleting or converting to collection.
  - Fixed when limiting results to 'X' number of items, was not being applied correctly. 
## Version 2.1.0.1
- ### Feature
  - Added Sync Count to playlist statistics
  - Added more detailed status messages to playlist statistics
  - Added the ability to turn playlist syncing on or off
  - Added sort for newly added items
  - Added new source type for playlists
    - Media Items: Use all media items as a base to apply filters on.
    - Playlist: Use an existing playlist as a base to apply filters on.
    - Collection: Use an existing Collection as a base to apply filters on.
  - Added the ability to execute a playlist from the editor.(Pop up is not the actual execution time, you need to check the info to see how the playlist updated)
  - Renamed criteria "Path" to "Path/Filename".  <span style="color:red">**BREAKING CHANGE, please re-select path/filename, save any playlist that uses the criteria**</span>
  - Fixed thumbnail image rendering in plugin list.
## Version 2.1.0.0
- ### Feature
  - Added a new feature that can roll up episode items to either their season or series, this is only available for smart collections. (Emby restriction)
    - EpiModes:
      - Item: Collection will contain episodes
      - Season: Collection will roll up episode to season
      - Series: Collection will roll up episode to series
- Added a new feature that shows smart play list statistics. (Info Icon on smartlist)
- New Layout
- Small UI changes
- Added the following criteria:
  - Studio
  - Added "Music and Home Video" to Media Type. <span style="color:red">**BREAKING CHANGE, please re-select the media type criteria save any playlist that uses the criteria**</span>
- Added confirmation on playlist delete
- ### Bug fix
  - Fixed a bug then when generating a new playlist or collection that an error was raised with 'Invalid Parameter Id'
