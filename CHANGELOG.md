# ChangeLog
## Version 2.0.0.2
- ### Feature
  - Added a new feature that can roll up episode items to either their season or series, this is only available for smart collections. (Emby restriction)
    - EpiModes:
      - Item: Collection will contain episodes
      - Season: Collection will roll up episode to season
      - Series: Collection will roll up episode to series
- ### Bug fix
  - Fixed a bug then when generating a new playlist or collection that an error was raised with 'Invalid Parameter Id'