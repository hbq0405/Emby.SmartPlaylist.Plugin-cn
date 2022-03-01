import { AppData, AppPlaylist, AppPlaylists } from '~/app/types/appData';
import camelcaseKeys = require('camelcase-keys');
import { parseDate } from '~/common/helpers/date';
import { convertObjectPropValues } from '~/common/helpers/object';
import { PlaylistInfo, PlaylistViewData } from '~/app/types/playlist';

type ApiClient = {
    getPluginConfiguration<TConfig>(pluginId: string): Promise<TConfig>;
    updatePluginConfiguration<TConfig>(pluginId: string, config: TConfig): Promise<any>;
    ajax<T = any>(request: any): Promise<T>;
};

export const version = "2.2.0.2";

declare global {
    // tslint:disable-next-line:interface-name
    interface Window {
        Dashboard: any;
        ApiClient: ApiClient;
    }
}

export const loadAppData = async (appId: string): Promise<AppData> => {
    let appData = await window.ApiClient.ajax<AppData>(
        {
            url: `/smartplaylist/appData?v=${version}`,
            type: 'GET',
            headers: {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            },
            contentType: 'application/json',
            dataType: 'json',
        }
    );

    appData = camelcaseKeys(appData, {
        deep: true,
    }) as AppData;

    convertObjectPropValues(appData, o => parseDate(o));

    return new Promise<AppData>(res => {
        res({
            appId: appId,
            ...appData,
        });
    });
};

export const saveAppPlaylist = async (playlist: AppPlaylist): Promise<AppPlaylists> => {
    return window.ApiClient.ajax(
        {
            url: `/smartplaylist?v=${version}`,
            type: 'POST',
            headers: {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            },
            data: JSON.stringify(playlist),
        }
    );
};



export const deletePlaylist = async (playlistId: string, keep: boolean): Promise<any> => {
    return window.ApiClient.ajax(
        {
            url: `/smartplaylist/${playlistId}/${keep}?v=${version}`,
            type: 'DELETE',
        }
    );
};

export const viewPlaylist = async (playlistId: string, execute: boolean): Promise<PlaylistInfo> => {
    let playlistInfo = await window.ApiClient.ajax<PlaylistInfo>(
        {
            url: `/smartplaylist/info/${playlistId}?v=${version}`,
            type: execute ? 'POST' : 'GET',
            headers: {
                Accept: 'application/json',
                'Content-Type': 'application/json',
            },
            contentType: 'application/json',
            dataType: 'json',
        }
    );

    playlistInfo = camelcaseKeys(playlistInfo, {
        deep: true,
    }) as PlaylistInfo;

    convertObjectPropValues(playlistInfo, o => parseDate(o));

    return new Promise<PlaylistInfo>(res => {
        res(playlistInfo);
    });
};
