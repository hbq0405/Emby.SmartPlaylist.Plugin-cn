import * as React from 'react';
import { Playlist, ServerResponse } from '~/app/types/playlist';
import { createPlaylist } from '~/app/types/playlist.factory';
import { AppAction, AppState } from '~/app/state/app.reducer';
import { AppData, AppPlaylist } from '~/app/types/appData';
import { saveAppPlaylist, deletePlaylist, viewPlaylist } from '~/app/app.data';
import { getAppEditPlaylist, getAppPlaylistForPlaylist, getAppSortJobPlaylist } from '~/app/state/app.selectors';
import { toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';
import { render } from 'react-dom';
import { ConfirmDeletePlaylist, DeleteData } from '~/emby/components/Confirmation';
import { response } from 'express';

toast.configure();

export type AppActions = {
    addNewPlaylist(): void;
    editPlaylist(plalist: Playlist): void;
    editSortJob(plalist: Playlist): void;
    updatePlaylist(plalist: Playlist): void;
    savePlaylist(): void;
    saveSortJob(): void;
    deletePlaylist(plalist: Playlist, keep: boolean): void;
    discardPlaylist(): void;
    loadAppData(appData: AppData): void;
    viewPlaylist(plalist: Playlist): void;
    executePlaylist(plalist: Playlist): void;
    confirmDeletePlaylist(plalist: Playlist): void;
};

export function handleSaveResponse(response: ServerResponse, dis) {
    if (response.success) {
        dis({
            type: 'app:savePlaylist',
            playlist: response.playlist
        });
    }
    else {
        toast.error(`Error saving playlist: ${response.error}`, {
            containerId: "modalToast",
            autoClose: false,
            position: 'top-center',
            bodyStyle: {
                zIndex: 1000
            }
        });
    }
}

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
        editSortJob: (playlist: Playlist) => {
            dispatcher({
                type: 'app:editSortJob',
                playlist: playlist,
            });
        },
        updatePlaylist: async (playlist: Playlist) => {
            await saveAppPlaylist(getAppPlaylistForPlaylist(playlist), false);
            dispatcher({
                type: 'app:updatePlaylist',
                playlist: playlist
            });
        },
        savePlaylist: async () => {
            handleSaveResponse(await saveAppPlaylist(getAppEditPlaylist(state), false) as ServerResponse, dispatcher);

        },
        saveSortJob: async () => {
            handleSaveResponse(await saveAppPlaylist(getAppSortJobPlaylist(state), true) as ServerResponse, dispatcher);
        },

        deletePlaylist: async (plalist: Playlist, keep: boolean) => {
            await deletePlaylist(plalist.id, keep);
            dispatcher({
                type: 'app:removePlaylist',
                playlist: plalist,
                keep: keep
            });
        },
        viewPlaylist: async (plalist: Playlist) => {
            dispatcher({
                type: 'app:loadPlaylistInfo',
                playlistInfo: await viewPlaylist(plalist.id, false)
            });
        },
        executePlaylist: async (plalist: Playlist) => {
            toast('Executing playlist: ' + plalist.name);
            viewPlaylist(plalist.id, true)
        },
        discardPlaylist: () => {
            dispatcher({
                type: 'app:discardPlaylist',
            });
        },
        confirmDeletePlaylist: (plalist: Playlist) => {
            dispatcher({
                type: 'app:confirmDeletePlaylist',
                confirmationProps: {
                    title: 'Confirm playlist removal',
                    data: { playlist: plalist, keep: false } as DeleteData,
                    question: `Are you sure you want to delete the playlist: "${plalist.name}" ?`,
                    control: ConfirmDeletePlaylist,
                    onNo: (data => { }),
                    onYes: (data => {
                        deletePlaylist(data.playlist.id, data.keep).finally(() => {
                            dispatcher({
                                type: 'app:removePlaylist',
                                playlist: data.playlist,
                                keep: data.keep
                            })
                        })
                    })
                }
            })
        },
        loadAppData: (appData: AppData) => {
            dispatcher({
                type: 'app:loadSettings',
                settings: appData,
            });
        },
    };
};
