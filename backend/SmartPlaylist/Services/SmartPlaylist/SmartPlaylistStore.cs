using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaBrowser.Model.Serialization;
using SmartPlaylist.Contracts;

namespace SmartPlaylist.Services.SmartPlaylist
{
    public class SmartPlaylistStore : ISmartPlaylistStore
    {
        private readonly ISmartPlaylistFileSystem _fileSystem;
        private readonly IJsonSerializer _jsonSerializer;

        public SmartPlaylistStore(IJsonSerializer jsonSerializer, ISmartPlaylistFileSystem fileSystem)
        {
            _jsonSerializer = jsonSerializer;
            _fileSystem = fileSystem;
        }


        public async Task<SmartPlaylistDto> GetSmartPlaylistAsync(Guid smartPlaylistId)
        {
            var fileName = _fileSystem.GetSmartPlaylistFilePath(smartPlaylistId);

            return await LoadPlaylistAsync(fileName).ConfigureAwait(false);
        }

        public async Task<SmartPlaylistDto[]> LoadPlaylistsAsync(Guid userId)
        {
            var deserializeTasks = _fileSystem.GetSmartPlaylistFilePaths(userId).Select(LoadPlaylistAsync).ToArray();

            await Task.WhenAll(deserializeTasks).ConfigureAwait(false);

            return deserializeTasks.Where(x => x.Result != null).Select(x => x.Result).OrderBy(x => x.Name).ToArray();
        }

        public async Task<SmartPlaylistDto[]> GetAllSmartPlaylistsAsync()
        {
            var deserializeTasks = _fileSystem.GetAllSmartPlaylistFilePaths().Select(LoadPlaylistAsync).ToArray();

            await Task.WhenAll(deserializeTasks).ConfigureAwait(false);

            return deserializeTasks.Where(x => x.Result != null).Select(x => x.Result).ToArray();
        }

        public void Save(SmartPlaylistDto smartPlaylist)
        {
            var filePath = _fileSystem.GetSmartPlaylistPath(smartPlaylist.UserId, smartPlaylist.Id);
            _jsonSerializer.SerializeToFile(smartPlaylist, filePath);
        }

        public void Delete(Guid userId, string smartPlaylistId)
        {
            var filePath = _fileSystem.GetSmartPlaylistPath(userId, smartPlaylistId);
            if (File.Exists(filePath))
                File.Delete(filePath);
            var logPath = _fileSystem.GetSmartPlaylistLog(userId, smartPlaylistId);
            if (File.Exists(logPath))
                File.Delete(logPath);
        }

        private async Task<SmartPlaylistDto> LoadPlaylistAsync(string filePath)
        {
            try
            {
                using (var reader = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None, 4096,
                    FileOptions.Asynchronous))
                {
                    var res = await _jsonSerializer.DeserializeFromStreamAsync<SmartPlaylistDto>(reader)
                        .ConfigureAwait(false);
                    reader.Close();
                    if (res == null)
                        throw new ArgumentNullException();
                    return res;
                }
            }
            catch (Exception)
            {
                MoveToFailed(filePath);
                return null;
            }
        }

        private void MoveToFailed(string filePath)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            var failed = Path.Combine(fileInfo.DirectoryName,
                Path.GetFileNameWithoutExtension(filePath) + ".failed");
            if (File.Exists(failed))
                File.Delete(failed);
            File.Move(filePath, failed);
        }

        public bool Exists(Guid userId, string smartPlaylistId)
        {
            return _fileSystem.PlaylistFileExists(userId, smartPlaylistId);
        }

        public async Task WriteToLogAsync(Domain.SmartPlaylist smartPlaylist)
        {
            var filePath = _fileSystem.GetSmartPlaylistLog(smartPlaylist.UserId, smartPlaylist.Id.ToString());
            if (File.Exists(filePath))
                File.Delete(filePath);

            using (TextWriter tw = File.CreateText(filePath))
            {
                for (int i = 0; i < smartPlaylist.LogEntries.Count; i++)
                    await tw.WriteLineAsync(smartPlaylist.LogEntries[i]);
                tw.Close();
            }
        }

        public Stream GetLogFileStream(Guid userId, string smartPlaylistId)
        {
            return File.Open(GetLogFilePath(userId, smartPlaylistId), FileMode.Open);
        }

        public string GetLogFilePath(Guid userId, string smartPlaylistId)
        {
            var filePath = _fileSystem.GetSmartPlaylistLog(userId, smartPlaylistId);
            if (!File.Exists(filePath))
                throw new FileLoadException($"Log file {filePath} does not exist");
            return filePath;
        }
    }
}