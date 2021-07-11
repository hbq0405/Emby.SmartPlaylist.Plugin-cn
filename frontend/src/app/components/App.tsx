import { Modal } from '~/common/components/Modal/Modal';
import { PlaylistEditor } from '~/app/components/PlaylistEditor';
import * as React from 'react';
import { Button } from '~/common/components/Button';
import { createPlaylistContextValue, PlaylistContext } from '~/app/state/playlist/playlist.context';
import { appReducer, initAppState } from '~/app/state/app.reducer';
import { AppContext, createAppContextValue } from '~/app/state/app.context';
import { loadAppData } from '~/app/app.data';
import { AppData } from '~/app/types/appData';
import { PlaylistList } from '~/app/components/PlaylistList';
import { AddButton } from '~/common/components/AddButton';

export type AppProps = {
    appId: string;
};

export const App: React.FC<AppProps> = props => {
    const [appState, appDispatcher] = React.useReducer(appReducer, {
        ...initAppState,
    });

    const appContext = createAppContextValue(appState, appDispatcher);

    React.useEffect(() => {
        loadAppData(props.appId).then((appData: AppData) => {
            appContext.loadAppData(appData);
        });
    }, [props.appId]);

    const {
        addNewPlaylist,
        discardPlaylist,
        savePlaylist,
        getEditedPlaylist,
        isNewPlaylist,
    } = appContext;

    const editedPlaylist = getEditedPlaylist();

    return (
        <>
            <AppContext.Provider value={appContext}>
                <div className="flex align-items-center justify-content-center focuscontainer-x itemsViewSettingsContainer padded-top padded-bottom padded-left padded-left-page padded-right">
                    <AddButton 
                        onClick={()=>addNewPlaylist()}
                        label="Add Smart Playlist" />
                </div>

                <div className="verticalSection verticalSection-extrabottompadding">
                    <PlaylistList />
                </div>
                
                {editedPlaylist && (
                    <Modal
                        confirmLabel="Save"
                        onClose={() => discardPlaylist()}
                        title={isNewPlaylist(editedPlaylist.id) ? 'Add Playlist' : 'Edit Playlist'}
                        onConfirm={() => savePlaylist()}
                    >
                        <PlaylistContext.Provider
                            value={createPlaylistContextValue(
                                editedPlaylist,
                                appDispatcher,
                                appContext,
                            )}
                        >
                            <PlaylistEditor />
                        </PlaylistContext.Provider>
                    </Modal>
                )}
            </AppContext.Provider>
        </>
    );
};
