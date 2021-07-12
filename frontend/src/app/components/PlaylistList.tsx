import * as React from 'react';
import { AppContext } from '~/app/state/app.context';
import './PlaylistList.css'
import { ListItem } from '~/common/components/ListItem';

type PlaylistListProps = {};

export const PlaylistList: React.FC<PlaylistListProps> = () => {
    const appContext = React.useContext(AppContext);

    const { getPlaylists, editPlaylist, deletePlaylist } = appContext;

    return (
        <>
            {getPlaylists().map(playlist => (

                <ListItem
                    onDeleteClick={()=>deletePlaylist(playlist)}
                    onEditClick={()=>editPlaylist(playlist)}
                    label={playlist.name}
                    type={playlist.smartType}
                />
                /*<Inline key={playlist.id} 
                    class="plist-row"
                >
                    
                    <Label>{playlist.name}</Label>
                    <FloatRight>
                        <Button onClick={() => editPlaylist(playlist)}>Edit</Button>
                        <Button onClick={() => deletePlaylist(playlist)}>Delete</Button>
                    </FloatRight>
                </Inline>*/
            ))}
        </>
    );
};
