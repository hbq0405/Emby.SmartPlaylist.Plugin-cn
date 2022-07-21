import * as React from 'react';
import { PlaylistContext } from '~/app/state/playlist/playlist.context';
import { Select } from '~/common/components/Select';
import { TreeView } from '~/common/components/TreeView/TreeView';
import { RuleTreeNodeContent } from '~/app/components/RuleTreeNodeContent';
import { Input } from '~/common/components/Input';
import { AppContext } from '~/app/state/app.context';
import { defaultGroupMatchType, RuleMatchTypes, SmartTypes, UpdateTypes, CollectionModes, SourceTypes } from '~/app/app.const';
import { Inline } from '~/common/components/Inline';
import { TreeNodeData } from '~/common/components/TreeView/types/tree';
import { RuleOrRuleGroup } from '~/app/types/rule';
import { AutoSize } from '~/common/components/AutoSize';
import { Toggle } from '~/common/components/Toggle';

type PlaylistEditorProps = {};

export const PlaylistEditor: React.FC<PlaylistEditorProps> = () => {
    const playlistContext = React.useContext(PlaylistContext);
    const appContext = React.useContext(AppContext);
    const { updateBasicData, changeExpand, isShuffleUpdateType } = playlistContext;
    const rulesTree = playlistContext.getRulesTree();
    const basicData = playlistContext.getBasicData();
    const ordersBy = appContext.getOrdersBy();
    var sourceItems = appContext.getSourcesFor(basicData.sourceType);

    return (
        <>
            <Inline>
                <Select
                    label="Type:"
                    values={SmartTypes.map(x => x)}
                    value={basicData.smartType}
                    onChange={newVal => {
                        updateBasicData({
                            smartType: newVal
                        })
                    }}
                    style={{ width: '120px' }}
                />
                {basicData.smartType === "Collection" && <Select
                    label='EpiMode:'
                    values={CollectionModes.map(x => x)}
                    value={basicData.collectionMode}
                    onChange={newVal =>
                        updateBasicData({
                            collectionMode: newVal
                        })
                    }
                    style={{ width: '100px' }}
                />}
                <Select
                    label='Source Type'
                    values={SourceTypes.map(x => x)}
                    value={basicData.sourceType}
                    onChange={newVal => {
                        sourceItems = appContext.getSourcesFor(newVal);
                        updateBasicData({
                            sourceType: newVal,
                            source: sourceItems[0]
                        });
                    }}
                    style={{ width: '150px' }}
                />
                {basicData.sourceType !== SourceTypes[0] && <Select
                    label='Source'
                    values={sourceItems.map(x => x.name)}
                    value={basicData.source.name}
                    onChange={newVal => updateBasicData({
                        source: sourceItems.filter(x => x.name === newVal)[0]
                    })}
                />}
                <Input
                    maxWidth={true}
                    value={basicData.name}
                    label="Name:"
                    onBlur={e =>
                        updateBasicData({
                            name: e.target.value,
                        })
                    }
                />
                <Select
                    label="Update type:"
                    values={UpdateTypes.map(x => x)}
                    value={basicData.updateType}
                    onChange={newVal =>
                        updateBasicData({
                            updateType: newVal,
                        })
                    }
                    style={{ width: '120px' }}
                />
            </Inline>

            <Inline>
                {(basicData.smartType === "Playlist" && basicData.updateType !== 'Live') && (
                    <>
                        <Toggle
                            id="Toggle-Sort"
                            label='Sort'
                            checked={basicData.newItemOrder.hasSort}
                            onToggled={c => {
                                updateBasicData({
                                    newItemOrder: {
                                        ...basicData.newItemOrder,
                                        hasSort: c,
                                        orderBy: ordersBy[0]
                                    }
                                })
                                if (c)
                                    updateBasicData({
                                        limit: {
                                            ...basicData.limit,
                                            hasLimit: false
                                        }
                                    })

                            }}
                        />
                        <Select
                            label='Sort newly added items by:'
                            disabled={!basicData.newItemOrder.hasSort}
                            maxWidth={true}
                            values={ordersBy}
                            value={basicData.newItemOrder.orderBy}
                            onChange={newVal =>
                                updateBasicData({
                                    newItemOrder: {
                                        ...basicData.newItemOrder,
                                        orderBy: newVal,
                                    },
                                })
                            }
                        />
                    </>
                )}
                <Toggle
                    id="Toggle-Limit"
                    label='Limited:'
                    checked={basicData.limit.hasLimit}
                    onToggled={c => {
                        updateBasicData({
                            limit: {
                                ...basicData.limit,
                                hasLimit: c,
                                orderBy: ordersBy[0]
                            }
                        })

                        if (c)
                            updateBasicData({
                                newItemOrder: {
                                    ...basicData.newItemOrder,
                                    hasSort: false
                                }
                            })

                    }}
                />
                <Input
                    disabled={!basicData.limit.hasLimit}
                    maxWidth={true}
                    value={basicData.limit.maxItems}
                    label="Max items:"
                    type="number"
                    onBlur={e =>
                        updateBasicData({
                            limit: {
                                ...basicData.limit,
                                maxItems: Number(e.target.value)
                            },
                        })
                    }
                />
                <Select
                    disabled={!basicData.limit.hasLimit || isShuffleUpdateType()}
                    maxWidth={true}
                    values={ordersBy}
                    label="Sort by:"
                    value={basicData.limit.orderBy}
                    onChange={newVal =>
                        updateBasicData({
                            limit: {
                                ...basicData.limit,
                                orderBy: newVal,
                            },
                        })
                    }
                />
            </Inline>
            <TreeView
                data={rulesTree}
                renderNodeContent={nodeData => <RuleTreeNodeContent node={nodeData} />}
                onExpandedChange={(node, isExpanded) => changeExpand(node, isExpanded)}
                overrides={{
                    Node: {
                        props: {
                            overrides: {
                                ContentContainer: {
                                    props: {
                                        renderGroupHeader: nodeData => (
                                            <GroupContentHeader node={nodeData} />
                                        ),
                                    },
                                },
                            },
                        },
                    },
                }}
            />
        </>
    );
};

const GroupContentHeader: React.FC<{ node: TreeNodeData<RuleOrRuleGroup> }> = props => {
    const playlistContext = React.useContext(PlaylistContext);
    const nodeData = props.node.data;
    return (
        <>
            {nodeData.kind === 'ruleGroup' && (
                <AutoSize>
                    <Select
                        title='All items need to match under an "All" group but only one needs to match in an "Any" group for the item to be matched successfully'
                        values={RuleMatchTypes.map(x => x)}
                        value={nodeData.matchMode || defaultGroupMatchType}
                        onChange={newVal => playlistContext.changeMatchMode(nodeData, newVal)}
                    />
                </AutoSize>
            )}
        </>
    );
};
