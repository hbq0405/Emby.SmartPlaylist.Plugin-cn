import * as React from 'react';
import { Override, useOverrides } from '~/common/hooks/useOverrides';
import { TreeNode, TreeNodeProps } from '~/common/components/TreeView/TreeNode';
import { TreeNodeData, TreeViewData } from '~/common/components/TreeView/types/tree';
import { Button } from '../Button';
import { Icon } from '../Icon';
import { TreeViewMultiAdd } from './TreeViewMultiAdd';
import { PlaylistContext } from '~/app/state/playlist/playlist.context';

type TreeViewProps = {
    data: TreeViewData;
    overrides?: {
        Node: Override<TreeNodeProps>;
    };
    onExpandedChange?(nodeData: TreeNodeData, isExpanded: boolean): void;
    renderNodeContent(data: TreeNodeData): React.ReactNode;
};

export const TreeView: React.FC<TreeViewProps> = props => {
    const playlistContext = React.useContext(PlaylistContext);
    let [showMultiAdd, setShowMultiAdd] = React.useState(false);

    const [Node, nodeProps] = useOverrides(props.overrides && props.overrides.Node, TreeNode);

    const { data } = props;
    const getRootNodes = (): TreeNodeData[] => {
        return data.rootIds.map(x => data.byId[x]);
    };

    const getChildNodes = (nodeData: TreeNodeData): TreeNodeData[] => {
        return nodeData.children.map(x => data.byId[x]);
    };

    const setExpanded = (nodeData: TreeNodeData, isExpanded: boolean): void => {
        props.onExpandedChange(nodeData, isExpanded);
    };

    return (
        <>
            <div className='multi-add'>
                <Button onClick={_ => setShowMultiAdd(true)}>
                    <Icon type="library_add" />
                </Button>
            </div>
            <div>
                {showMultiAdd && (
                    <TreeViewMultiAdd
                        onClose={() => setShowMultiAdd(false)}
                        onConfirm={(rules) => {
                            var node = playlistContext.getLastTreeNode();
                            if (node)
                                for (let i = rules.length - 1; i >= 0; i--) {
                                    playlistContext.addRuleEntity(node, rules[i]);
                                }
                            setShowMultiAdd(false);
                        }}
                    />
                )}
            </div>
            {getRootNodes().map(nodeData => (
                <Node
                    key={nodeData.id}
                    data={nodeData}
                    getChildNodes={getChildNodes}
                    setExpanded={setExpanded}
                    renderContent={props.renderNodeContent}
                    {...nodeProps}
                />
            ))}
        </>
    );
};
