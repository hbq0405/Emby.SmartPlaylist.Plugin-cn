import * as React from 'react';
import { SmartTypes } from '~/app/app.const';
import { SmartType } from '~/app/types/playlist';

export type ListItemProps = {
    onEditClick(): void;
    onDeleteClick(): void;
    onViewClick(): void;
    label: string;
    type: SmartType;
} & React.AllHTMLAttributes<HTMLDivElement> &
    BaseProps;

export const ListItem: React.FC<ListItemProps> = props => {
    var icon = props.type == SmartTypes[1] ? 'video_library' : 'playlist_play';
    var txt = props.type == SmartTypes[1] ? 'Collection' : 'Playlist';
    return (
        <a className="listItem listItem-border emby-button plist-row" data-ripple="false">
            <div className="listItemImageContainer">
                <i className="listItemIcon md-icon listItemIcon-transparent">{icon}</i>
            </div>
            <div className="listItemBody">
                <div className="listItemBodyText" onClick={() => props.onEditClick()}>{props.label}</div>
            </div>
            <div className='popper'>
                <span className="tooltiptext">{txt}</span>
                <button type="button" is="paper-icon-button-light" className="paper-icon-button-light icon-button-conditionalfocuscolor" onClick={() => props.onViewClick()}><i className="md-icon">info</i></button>
                <button type="button" is="paper-icon-button-light" className="paper-icon-button-light icon-button-conditionalfocuscolor" onClick={() => props.onEditClick()}><i className="md-icon">edit</i></button>
                <button type="button" is="paper-icon-button-light" className="paper-icon-button-light icon-button-conditionalfocuscolor" onClick={() => props.onDeleteClick()}><i className="md-icon">delete</i></button>
            </div>
        </a>
    )

}