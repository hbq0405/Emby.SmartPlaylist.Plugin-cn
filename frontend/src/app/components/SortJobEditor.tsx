import * as React from 'react';
import { InfoRow } from "~/common/components/InfoRow";
import { Inline } from "~/common/components/Inline";
import { Select } from "~/common/components/Select";
import { Toggle } from "~/common/components/Toggle";
import { UpdateTypes } from "../app.const";
import { AppContext } from "../state/app.context";
import { PlaylistContext } from "../state/playlist/playlist.context";

type SortJobEditorProps = {};

export const SortJobEditor: React.FC<SortJobEditorProps> = () => {
    const playlistContext = React.useContext(PlaylistContext);
    const appContext = React.useContext(AppContext);
    const { updateBasicData } = playlistContext;
    const basicData = playlistContext.getBasicData();
    const OrdersBy = appContext.getOrdersBy();

    return (
        <>
            <Inline>
                <Toggle
                    id="Sort-Enabled"
                    label='Enabled:'
                    checked={basicData.sortJob.enabled}
                    onToggled={c => {
                        updateBasicData({
                            sortJob: {
                                ...basicData.sortJob,
                                enabled: c
                            }
                        })

                    }} />
                <Select
                    label='Sort by:'
                    disabled={!basicData.sortJob.enabled}
                    maxWidth={true}
                    values={OrdersBy}
                    value={basicData.sortJob.orderBy}
                    onChange={newVal =>
                        updateBasicData({
                            sortJob: {
                                ...basicData.sortJob,
                                orderBy: newVal
                            },
                        })
                    }
                />
                <Select
                    label="Sort Trigger:"
                    disabled={!basicData.sortJob.enabled}
                    values={UpdateTypes.filter(x => x == 'Daily' || x == 'Weekly' || x == 'Monthly').map(x => x)}
                    value={basicData.sortJob.updateType}
                    onChange={newVal =>
                        updateBasicData({
                            sortJob: {
                                ...basicData.sortJob,
                                updateType: newVal
                            }
                        })
                    }
                    style={{ width: '120px' }}
                />
            </Inline>
            {basicData.sortJob.enabled && (
                <>
                    <InfoRow InfoItems={[
                        { label: 'Last Run Duration: ', text: basicData.sortJob.lastDurationStr ? basicData.sortJob.lastDurationStr : 'N/A', visible: true },
                        { label: 'Runs: ', text: basicData.sortJob.syncCount ? basicData.sortJob.syncCount.toString() : 'N/A', visible: true },
                        { label: 'Last Updated: ', text: basicData.sortJob.lastUpdated ? basicData.sortJob.lastUpdated.toLocaleString() : 'N/A', visible: true }
                    ]} />
                    <InfoRow InfoItems={[
                        { label: 'Last Ran: ', text: basicData.sortJob.lastRan ? basicData.sortJob.lastRan.toLocaleString() : 'N/A', visible: true },
                        { label: 'Next Run: ', text: basicData.sortJob.nextUpdate ? basicData.sortJob.nextUpdate.toLocaleString() : 'N/A', visible: true }

                    ]} />
                    <InfoRow InfoItems={[
                        { label: 'Status: ', text: basicData.sortJob.status ? basicData.sortJob.status : 'N/A', visible: true }
                    ]} />
                </>
            )}
        </>
    )
}