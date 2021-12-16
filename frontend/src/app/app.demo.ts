import { RuleCriteriaDefinition } from '~/app/types/rule';
import { AppData, AppPlaylist } from '~/app/types/appData';
import { createTreeNodeData } from '~/common/components/TreeView/types/tree.factory';
import { Guid } from '~/common/helpers/guid';
import { createRule, createRuleGroup } from '~/app/types/rule.factory';
import {
    dateCriteriaDefinitionType,
    defaultPlaylistLimit,
    defaultRuleCriteriaDefinition,
    stringCriteriaDefinitionType,
    defaultUpdateType,
    defaultSmartType,
    defaultCollectionMode
} from '~/app/app.const';
import { PlaylistInfo } from './types/playlist';

export const demoRulesCritDefinitions: RuleCriteriaDefinition[] = [
    defaultRuleCriteriaDefinition,
    {
        name: 'Artist',
        values: [],
        type: stringCriteriaDefinitionType,
    },
    {
        name: 'DateLastSaved',
        type: dateCriteriaDefinitionType,
        values: [],
    },
];

export const demoAppPlaylists: AppPlaylist[] = [
    {
        id: Guid.newGuid(),
        name: 'Playlist1',
        limit: defaultPlaylistLimit,
        updateType: defaultUpdateType,
        smartType: defaultSmartType,
        collectionMode: defaultCollectionMode,
        priorNames: [],
        rulesTree: [
            createTreeNodeData({
                isRoot: true,
                data: createRuleGroup(),
                children: ['child1'],
            }),
            createTreeNodeData({
                id: 'child1',
                data: createRule(),
                level: 1,
            }),
        ],
    },
];

export const demoAppPlaylistView: PlaylistInfo = {
    id: Guid.newGuid(),
    name: 'Playlist1',
    limit: defaultPlaylistLimit,
    updateType: defaultUpdateType,
    smartType: defaultSmartType,
    collectionMode: defaultCollectionMode,
    priorNames: [],
    rulesTree: {
        byId: {},
        rootIds: []
    }
}

export const demoLimitOrdersBy = ['Album', 'Artist'];

export const demoAppData: AppData = {
    appId: Guid.newGuid(),
    playlists: demoAppPlaylists,
    rulesCriteriaDefinitions: demoRulesCritDefinitions,
    limitOrdersBy: demoLimitOrdersBy,
};
