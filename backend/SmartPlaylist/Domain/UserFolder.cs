using MediaBrowser.Controller.Entities;

namespace SmartPlaylist.Domain
{
    public class UserFolder
    {
        public UserFolder(User user, string folderName, SmartType smartType)
        {
            User = user;
            Name = folderName;
            SmartType = smartType;
        }

        public string Name { get; }
        public User User { get; }
        public SmartType SmartType { get; }


        public virtual BaseItem[] GetItems()
        {
            return new BaseItem[0];
        }
    }
}