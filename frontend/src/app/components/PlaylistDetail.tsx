import * as React from 'react';
import { PlaylistInfo } from '~/app/types/playlist';
import './PlaylistDetail.css'
import { InfoRow } from '~/common/components/InfoRow';
import { TagList } from '~/common/components/TagList';
import { Button } from '~/emby/components/Button';
import { Icon } from '~/emby/components/Icon';
import { viewPlaylistLog } from '~/emby/app.data';
import { toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

toast.configure();

export type PlaylistDetailProps = {
    playlist: PlaylistInfo
}

export const PlaylistDetail: React.FC<PlaylistDetailProps> = props => {
    const playlist = props.playlist;

    const handleButton = (btn: HTMLButtonElement, disable: boolean) => {
        try {
            btn.disabled = disable;
            btn.children[0].innerHTML = disable ? 'hourglass_empty' : 'checklist';
        } catch (e) { }
    }

    const loadLog = (ev) => {
        if (ev.target instanceof HTMLButtonElement)
            handleButton(ev.target, true);

        viewPlaylistLog(playlist.id).then((value: string) => {
            var w = window.open('', playlist.name + "_log");
            w.document.writeln("<html><head><title>" + playlist.name + " Log File</title></head>")
            w.document.writeln('<body><pre>');
            w.document.writeln(value);
            w.document.writeln('</pre></body></html>');

        }).catch((reason) => {
            var msg = reason instanceof Response ? "Log file for playlist does not exist yet" :
                reason instanceof Error ? reason.message : reason;

            toast.error(`Error loading playlist log: ${msg}`, {
                containerId: "modalToast",
                autoClose: false,
                position: 'top-center',
                bodyStyle: {
                    zIndex: 1000
                }
            });

        }).finally(() => {
            if (ev.target instanceof HTMLButtonElement)
                handleButton(ev.target, false);
        })
    }

    return (
        <>
            <InfoRow InfoItems={[
                { label: 'Id: ', text: playlist.id, visible: true },
                { label: 'Internal Item Id: ', text: playlist.internalId ? playlist.internalId.toString() : 'N/A', visible: true },
                { label: 'Name: ', text: playlist.name, visible: true },
                { label: 'Type: ', text: (playlist.smartType ? playlist.smartType : 'Playlist'), visible: true },

            ]} />
            <InfoRow InfoItems={[
                { label: 'EpiMode: ', text: playlist.smartType == 'Collection' ? (playlist.collectionMode ? playlist.collectionMode : 'Item') : 'N/A', visible: true },
                { label: 'Update: ', text: playlist.updateType, visible: true },
                { label: 'Next Update: ', text: playlist.lastShuffleUpdate ? playlist.lastShuffleUpdate.toLocaleString() : 'N/A', visible: true },
                { label: 'Last Edited: ', text: playlist.lastUpdated ? playlist.lastUpdated.toLocaleString() : 'N/A', visible: true },

            ]} />
            <InfoRow InfoItems={[
                { label: 'Last Populated: ', text: playlist.lastSync ? playlist.lastSync.toLocaleString() : 'N/A', visible: true },
                { label: 'Limit: ', text: playlist.limit.hasLimit ? `${playlist.limit.maxItems} (Sort:${playlist.limit.orderBy})` : 'None', visible: true },
                { label: 'Rules: ', text: playlist.ruleCount.toString(), visible: true },
                { label: 'Items: ', text: playlist.items.length.toString(), visible: true },

            ]} />
            <InfoRow InfoItems={[
                { label: 'Last Run Duration: ', text: playlist.lastDurationStr ? playlist.lastDurationStr : 'N/A', visible: true },
                { label: "Synced Count: ", text: playlist.syncCount ? playlist.syncCount.toString() : '0', visible: true },
                { label: 'New Item Sort: ', text: playlist.newItemOrder.hasSort ? playlist.newItemOrder.orderBy : 'N/A', visible: true },
                { label: 'Source: ', text: playlist.sourceType + (playlist.sourceType === "Media Items" ? "" : " (" + playlist.source.name + ")"), visible: true }

            ]} />
            <InfoRow InfoItems={[
                { label: 'Last Run Status: ', text: playlist.status ? playlist.status : 'N/A', visible: true },
            ]} />
            {
                playlist.sortJob.enabled && (
                    <>
                        <h4 className='pageTitle'><i className="md-icon sp-icon">sort_by_alpha</i> Sort Job</h4>
                        <InfoRow InfoItems={[
                            { label: 'Last Run Duration: ', text: playlist.sortJob.lastDurationStr ? playlist.sortJob.lastDurationStr : 'N/A', visible: true },
                            { label: 'Runs: ', text: playlist.sortJob.syncCount ? playlist.sortJob.syncCount.toString() : 'N/A', visible: true },
                            { label: 'Last Updated: ', text: playlist.sortJob.lastUpdated ? playlist.sortJob.lastUpdated.toLocaleString() : 'N/A', visible: true },
                            { label: 'Last Ran: ', text: playlist.sortJob.lastRan ? playlist.sortJob.lastRan.toLocaleString() : 'N/A', visible: true },
                            { label: 'Next Run: ', text: playlist.sortJob.nextUpdate ? playlist.sortJob.nextUpdate.toLocaleString() : 'N/A', visible: true }
                        ]} />
                        <InfoRow InfoItems={[
                            { label: 'Last Sort Status: ', text: playlist.sortJob.status ? playlist.sortJob.status : 'N/A', visible: true },
                        ]} />
                    </>
                )
            }

            <div className='info-row info-row-label'>Items:</div>
            <TagList Items={playlist.items} />

            <Button style={{ position: 'absolute', top: '15px', right: '10px' }} onClick={loadLog}>
                <Icon type='checklist' />
            </Button>
        </>
    );
};