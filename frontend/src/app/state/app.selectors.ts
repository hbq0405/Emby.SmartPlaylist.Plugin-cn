import { Playlist, PlaylistInfo } from '~/app/types/playlist';
import {
    RuleCriteriaDefinition,
    RuleCriteriaOperator,
    ListValueRange,
    ListValue,
    ListMapValue
} from '~/app/types/rule';
import { AppPlaylistState, AppState } from '~/app/state/app.reducer';
import { AppData, AppPlaylist, Source } from '~/app/types/appData';
import { TreeViewData } from '~/common/components/TreeView/types/tree';
import { ConfirmationProps } from '~/emby/components/Confirmation';

export type AppSelectors = {
    getPlaylists(): Playlist[];
    getEditedPlaylist(): Playlist;
    isNewPlaylist(id: string): boolean;
    getRuleCriteriaOperators(criteriaName: string): RuleCriteriaOperator[];
    getRulesCriteriaDefinitions(): RuleCriteriaDefinition[];
    getRulesCriteriaDefinition(criteriaName: string): RuleCriteriaDefinition;
    getAppData(): AppData;
    getLimitOrdersBy(): string[];
    getViewPlaylist(): PlaylistInfo;
    getConfirmation(): ConfirmationProps;
    getSourcesFor(type: string): Source[];
};

export const createAppSelectors = (state: AppState): AppSelectors => {
    return {
        getPlaylists: (): Playlist[] => {
            return state.playlists.names.map(name => state.playlists.byId[name]);
        },
        getViewPlaylist: (): PlaylistInfo => {
            return state.viewPlaylist;
        },
        getEditedPlaylist: (): Playlist => {
            return state.editedPlaylist;
        },
        isNewPlaylist: (id: string): boolean => {
            return !Object.keys(state.playlists.byId).includes(id);
        },
        getRuleCriteriaOperators: (criteriaName: string): RuleCriteriaOperator[] => {
            const ruleCritDef = state.rulesCriteriaDefinitions.find(x => x.name === criteriaName);
            if (ruleCritDef && ruleCritDef.type.operators) {
                return ruleCritDef.type.operators;
            } else {
                return [];
            }
        },
        getRulesCriteriaDefinitions: (): RuleCriteriaDefinition[] => {
            return state.rulesCriteriaDefinitions;
        },
        getRulesCriteriaDefinition: (criteriaName: string): RuleCriteriaDefinition => {
            return state.rulesCriteriaDefinitions.find(x => x.name === criteriaName);
        },
        getAppData: (): AppData => {
            return getAppData(state);
        },
        getLimitOrdersBy: (): string[] => {
            return state.limitOrdersBy;
        },
        getConfirmation: (): ConfirmationProps => {
            return state.confirmation;
        },
        getSourcesFor: (type: string): Source[] => {
            console.log('Getting:' + type);
            return state.sources.filter(s => s.type === type);
        }
    };
};

export const getAppData = (state: AppState): AppData => {
    return {
        ...state,
        playlists: convertToAppPlaylists(state.playlists),
        sources: state.sources
    };
};

export const getAppPlaylist = (state: AppState): AppPlaylist => {
    return getAppPlaylistForPlaylist(state.editedPlaylist);
};

export const getAppPlaylistForPlaylist = (playlist: Playlist): AppPlaylist => {
    return {
        ...playlist,
        rulesTree: getOrderedNodeIds(playlist.rulesTree.rootIds, playlist.rulesTree).map(
            id => playlist.rulesTree.byId[id],
        ),
    };
}

const convertToAppPlaylists = (playlistState: AppPlaylistState): AppPlaylist[] => {
    return playlistState.names
        .map(x => playlistState.byId[x])
        .map(x => ({
            ...x,
            rulesTree: getOrderedNodeIds(x.rulesTree.rootIds, x.rulesTree).map(
                id => x.rulesTree.byId[id],
            ),
        }));
};

const getOrderedNodeIds = (ids: string[], treeViewData: TreeViewData): string[] => {
    const arr: string[] = [];

    ids.forEach(id => {
        arr.push(id);
        const node = treeViewData.byId[id];
        if (node.children.length > 0) {
            getOrderedNodeIds(node.children, treeViewData).map(i => arr.push(i));
        }
    });

    return arr;
};
