# ChangeLog

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
