import * as React from 'react';
import { Playlist } from '~/app/types/playlist';
import { createPlaylist } from '~/app/types/playlist.factory';
import { AppAction, AppState } from '~/app/state/app.reducer';
import { AppData } from '~/app/types/appData';
import { saveAppPlaylist, deletePlaylist, viewPlaylist } from '~/app/app.data';
import { getAppPlaylist } from '~/app/state/app.selectors';

export type AppActions = {
    addNewPlaylist(): void;
    editPlaylist(plalist: Playlist): void;
    savePlaylist(): void;
    deletePlaylist(plalist: Playlist): void;
    discardPlaylist(): void;
    loadAppData(appData: AppData): void;
    viewPlaylist(plalist: Playlist): void;
};

export const createAppActions = (
    dispatcher: React.Dispatch<AppAction>,
    state: AppState,
): AppActions => {
    return {
        addNewPlaylist: () => {
            dispatcher({
                type: 'app:addNewPlaylist',
                playlist: createPlaylist(),
            });
        },
        editPlaylist: (playlist: Playlist) => {
            dispatcher({
                type: 'app:editPlaylist',
                playlist: playlist,
            });
        },
        savePlaylist: async () => {
            await saveAppPlaylist(getAppPlaylist(state));
            dispatcher({
                type: 'app:savePlaylist',
            });
        },
        deletePlaylist: async (plalist: Playlist) => {
            await deletePlaylist(plalist.id);
            dispatcher({
                type: 'app:removePlaylist',
                playlist: plalist,
            });
        },
        viewPlaylist: async (plalist: Playlist) => {
            dispatcher({
                type: 'app:loadPlaylistInfo',
                playlistInfo: await viewPlaylist(plalist.id)
            });
        },
        discardPlaylist: () => {
            dispatcher({
                type: 'app:discardPlaylist',
            });
        },
        loadAppData: (appData: AppData) => {
            dispatcher({
                type: 'app:loadSettings',
                settings: appData,
            });
        },
    };
};
