import * as React from 'react';
import { PlaylistInfo } from '~/app/types/playlist';
import './PlaylistDetail.css'
import { PlaylistContext } from '~/app/state/playlist/playlist.context';
import { InfoRow } from '~/common/components/InfoRow';
import { TagList } from '~/common/components/TagList';

export type PlaylistDetailProps = {
    playlist: PlaylistInfo
}

export const PlaylistDetail: React.FC<PlaylistDetailProps> = props => {
    const playlist = props.playlist;

    return (
        <>
            <InfoRow InfoItems={[
                { label: 'Id: ', text: playlist.id },//`${playlist.id} (Internal Item Id: ${playlist.internalId}`
                { label: 'Internal Item Id: ', text: playlist.internalId ? playlist.internalId.toString() : 'N/A' },
                { label: 'Name: ', text: playlist.name },
            ]} />
            <InfoRow InfoItems={[
                { label: 'Type: ', text: playlist.smartType },
                { label: 'EpiMode: ', text: playlist.smartType == 'Collection' ? (playlist.collectionMode ? playlist.collectionMode : 'Item') : 'N/A' },
                { label: 'Update: ', text: playlist.updateType },
            ]} />
            <InfoRow InfoItems={[
                { label: 'Last Shuffle: ', text: playlist.lastShuffleUpdate ? playlist.lastShuffleUpdate.toLocaleString() : 'N/A' },
                { label: 'Last Edited: ', text: playlist.lastUpdated ? playlist.lastUpdated.toLocaleString() : 'N/A' },
                { label: 'Last Populated: ', text: playlist.lastSync ? playlist.lastSync.toLocaleString() : 'N/A' }
            ]} />
            <InfoRow InfoItems={[
                { label: 'Limit: ', text: playlist.limit.hasLimit ? `${playlist.limit.maxItems} (Sort:${playlist.limit.orderBy})` : 'None' },
                { label: 'Rules: ', text: playlist.ruleCount.toString() },
                { label: 'Items: ', text: playlist.items.length.toString() },
            ]} />
            <InfoRow InfoItems={[
                { label: 'Last Run Status: ', text: playlist.status ? playlist.status : 'N/A' },
                { label: 'Last Run Duration: ', text: playlist.lastDurationStr ? playlist.lastDurationStr : 'N/A' }
            ]} />
            <div className='info-row info-row-label'>Items:</div>
            <TagList Items={playlist.items} />
        </>
    );
};