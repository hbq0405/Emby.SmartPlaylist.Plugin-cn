import * as React from 'react';
import { AppContext } from '~/app/state/app.context';
import './PlaylistList.css'
import { ListItem } from '~/common/components/ListItem';
import { Playlist } from '../types/playlist';


type PlaylistListProps = {};

export const PlaylistList: React.FC<PlaylistListProps> = () => {
    const appContext = React.useContext(AppContext);

    const { getPlaylists, editPlaylist, updatePlaylist, confirmDeletePlaylist, viewPlaylist } = appContext;

    return (
        <>
            {getPlaylists().map(playlist => (

                <ListItem
                    onDeleteClick={() => confirmDeletePlaylist(playlist)}
                    onEditClick={() => editPlaylist(playlist)}
                    onViewClick={() => viewPlaylist(playlist)}
                    onUpdateData={(pl: Partial<Playlist>) => {
                        updatePlaylist({
                            ...playlist,
                            ...pl
                        });

                    }}
                    playList={playlist}
                />
            ))}
        </>
    );
};
