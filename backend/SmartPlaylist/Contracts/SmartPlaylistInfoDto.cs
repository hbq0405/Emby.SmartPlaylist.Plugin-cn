
using System;
using System.Reflection;

namespace SmartPlaylist.Contracts
{
    [Serializable]
    public class SmartPlaylistInfoDto : SmartPlaylistDto
    {
        public static SmartPlaylistInfoDto FromSmartPlaylist(SmartPlaylistDto smartPlaylist)
        {
            SmartPlaylistInfoDto infoDto = new SmartPlaylistInfoDto();
            foreach (PropertyInfo srcProp in smartPlaylist.GetType().GetProperties())
            {
                PropertyInfo desProp = infoDto.GetType().GetProperty(srcProp.Name);
                if (desProp != null && desProp.CanWrite)
                    desProp.SetValue(infoDto, srcProp.GetValue(smartPlaylist));
            }
            return infoDto;
        }
    }
}