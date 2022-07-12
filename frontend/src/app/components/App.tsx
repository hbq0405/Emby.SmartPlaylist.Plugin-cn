
import { PlaylistEditor } from '~/app/components/PlaylistEditor';
import { SortJobEditor } from '~/app/components/SortJobEditor';
import * as React from 'react';
import { createPlaylistContextValue, PlaylistContext } from '~/app/state/playlist/playlist.context';
import { appReducer, initAppState } from '~/app/state/app.reducer';
import { AppContext, createAppContextValue } from '~/app/state/app.context';
import { loadAppData } from '~/app/app.data';
import { AppData } from '~/app/types/appData';
import { PlaylistList } from '~/app/components/PlaylistList';
import { AddButton } from '~/common/components/AddButton';
import { PlaylistDetail } from '~/app/components/PlaylistDetail';
import './App.css';
import { Confirmation } from '~/emby/components/Confirmation';
import { Modal } from '~/emby/components/Modal';
import BeatLoader from "react-spinners/BeatLoader"
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
        saveSortJob,
        getEditedPlaylist,
        isNewPlaylist,
        getViewPlaylist,
        getConfirmation,
        isLoaded,
        getSortJobPlaylist,
    } = appContext;

    const editedPlaylist = getEditedPlaylist();
    const viewPlaylistInfo = getViewPlaylist();
    const confirmation = getConfirmation();
    const loaded = isLoaded()
    const sortJobPlaylist = getSortJobPlaylist();

    return (
        <>
            <AppContext.Provider value={appContext}>
                <div className="flex align-items-center justify-content-center focuscontainer-x itemsViewSettingsContainer padded-top padded-bottom padded-left padded-left-page padded-right">
                    <AddButton
                        onClick={() => addNewPlaylist()}
                        label="Add Smart Playlist" />
                </div>

                {!loaded && (
                    <div className='app-container'>
                        <BeatLoader loading={true} color='green' />
                    </div>
                )}

                <div className="verticalSection verticalSection-extrabottompadding app-container">
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

                {viewPlaylistInfo && (
                    <Modal
                        confirmLabel='Close'
                        title={`Playlist detail for ${viewPlaylistInfo.name}`}
                        onClose={() => discardPlaylist()}
                        onConfirm={() => discardPlaylist()}
                    >
                        <PlaylistDetail
                            playlist={viewPlaylistInfo}
                        />
                    </Modal>
                )}

                {sortJobPlaylist && (
                    <Modal
                        confirmLabel="Save"
                        onClose={() => discardPlaylist()}
                        title={`Edit Sort Job for ${sortJobPlaylist.name}`}
                        onConfirm={() => saveSortJob()}
                    >
                        <PlaylistContext.Provider
                            value={createPlaylistContextValue(
                                sortJobPlaylist,
                                appDispatcher,
                                appContext,
                            )}
                        >
                            <SortJobEditor />
                        </PlaylistContext.Provider>
                    </Modal>
                )}

                {confirmation && (
                    <Confirmation
                        {...confirmation}
                        onNo={(data) => discardPlaylist()}
                        onYes={(data) => {
                            confirmation.onYes(data);
                        }}
                    >
                    </Confirmation>
                )}
            </AppContext.Provider>
        </>
    );
};
