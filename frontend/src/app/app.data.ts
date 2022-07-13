import { AppData, AppPlaylist } from '~/app/types/appData';
import { demoAppData, demoAppPlaylistView } from '~/app/app.demo';
import { PlaylistInfo, ServerResponse } from './types/playlist';

export const loadAppData = (appId: string): Promise<AppData> => {
    return new Promise<AppData>(res => {
        res({
            appId: appId,
            ...demoAppData,
        });
    });
};

export const saveAppPlaylist = (playlist: AppPlaylist, saveSortJob: boolean): Promise<ServerResponse | void> => {
    // tslint:disable-next-line:no-console
    return new Promise<ServerResponse | void>(res => {
        res();
    });
};

export const deletePlaylist = (playlistId: string, remove: boolean): Promise<void> => {
    return new Promise<void>(res => {
        res();
    });
};

export const viewPlaylist = (playlistId: string, execute: boolean): Promise<PlaylistInfo> => {
    return new Promise<PlaylistInfo>(res => {
        res({
            ...demoAppPlaylistView
        });
    });
};

export const viewPlaylistLog = (playlistId: string): Promise<string> => {
    return new Promise<string>(res => {
        res('');
    });
};
