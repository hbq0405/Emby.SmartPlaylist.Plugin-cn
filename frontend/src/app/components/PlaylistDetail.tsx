import * as React from 'react';
import { PlaylistInfo } from '~/app/types/playlist';
import './PlaylistDetail.css'
import { PlaylistContext } from '~/app/state/playlist/playlist.context';
import { InfoRow } from '~/common/components/InfoRow';

export type PlaylistDetailProps = {
    playlist: PlaylistInfo
}

export const PlaylistDetail: React.FC<PlaylistDetailProps> = props => {
    const playlist = props.playlist
    return (
        <>
            <InfoRow InfoItems={[
                { label: 'Id: ', text: playlist.id },
                { label: 'Name: ', text: playlist.name },
            ]} />
            <InfoRow InfoItems={[
                { label: 'Type: ', text: playlist.smartType },
                { label: 'EpiMode: ', text: playlist.smartType == 'Collection' ? playlist.collectionMode : 'N/A' }
            ]} />

        </>
    );
};