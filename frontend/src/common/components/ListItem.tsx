import * as React from 'react';
import { SmartTypes } from '~/app/app.const';
import { playlistReducer } from '~/app/state/playlist/playlist.reducer';
import { getIconsForPlayList, Playlist } from '~/app/types/playlist';
import { Toggle } from './Toggle';

export type ListItemProps = {
    onEditClick(): void;
    onDeleteClick(): void;
    onViewClick(): void;
    onExecuteClick(): void;
    onUpdateData(playlist: Partial<Playlist>): void;
    playList: Playlist;
} & React.AllHTMLAttributes<HTMLDivElement> &
    BaseProps;



export const ListItem: React.FC<ListItemProps> = props => {
    const mainStyle = "listItem listItem-border emby-button plist-row" + (props.playList.enabled ? "" : " plist-row-disabled");
    const sub = props.playList.sourceType + (props.playList.sourceType === "Media Items" ? "" : " (" + props.playList.source.name + ")");

    return (
        <a className={mainStyle} data-ripple="false">
            <div className="plist-icon-container">
                <span title={props.playList.smartType + ' ' + props.playList.updateType} className="plist-icon md-icon listItemIcon-transparent">{getIconsForPlayList(props.playList)}</span>
            </div>
            <div className="listItemBody">
                <div className="listItemBodyText" onClick={() => props.onEditClick()}>{props.playList.name}</div>
                <div className="listItemBodyTextSub" onClick={() => props.onEditClick()}>{sub}</div>
            </div>
            <div className='popper'>
                <span className={`tooltiptext`}>
                    <Toggle title='Enabled' id={'toggle-' + props.playList.id} checked={props.playList.enabled} onToggled={(checked: boolean) => {
                        props.onUpdateData({
                            enabled: checked
                        });
                    }
                    } />
                </span>
                <button type="button" title='Execute' is="paper-icon-button-light" className="paper-icon-button-light icon-button-conditionalfocuscolor" onClick={() => props.onExecuteClick()}><i className="md-icon sp-icon">play_arrow</i></button>
                <button type="button" title='Details' is="paper-icon-button-light" className="paper-icon-button-light icon-button-conditionalfocuscolor" onClick={() => props.onViewClick()}><i className="md-icon sp-icon">info</i></button>
                <button type="button" title='Edit' is="paper-icon-button-light" className="paper-icon-button-light icon-button-conditionalfocuscolor" onClick={() => props.onEditClick()}><i className="md-icon sp-icon">edit</i></button>
                <button type="button" title='Delete' is="paper-icon-button-light" className="paper-icon-button-light icon-button-conditionalfocuscolor" onClick={() => props.onDeleteClick()}><i className="md-icon sp-icon">delete</i></button>
            </div>
        </a >
    )

}