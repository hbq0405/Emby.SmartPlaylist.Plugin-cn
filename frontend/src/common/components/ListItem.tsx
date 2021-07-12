import * as React from 'react';
import { SmartType } from '~/app/types/playlist';

export type ListItemProps = {
    onEditClick():void;
    onDeleteClick():void;
    label:string;
    type:SmartType;
} & React.AllHTMLAttributes<HTMLDivElement> &
    BaseProps;

export const ListItem: React.FC<ListItemProps> = props => {
    return(
            <a className="listItem listItem-border emby-button" data-ripple="false">
                <div className="listItemImageContainer"><i className="listItemIcon md-icon listItemIcon-transparent">dvr</i></div>
                <div className="listItemBody">
                    <div className="listItemBodyText">{props.label}</div>
                </div>
                <div style={{float:"right"}}>[{props.type}]</div>
                <button type="button" is="paper-icon-button-light" className="paper-icon-button-light icon-button-conditionalfocuscolor" onClick={()=>props.onEditClick()}><i className="md-icon">edit</i></button>
                <button type="button" is="paper-icon-button-light" className="paper-icon-button-light icon-button-conditionalfocuscolor" onClick={()=>props.onDeleteClick()}><i className="md-icon">delete</i></button>
            </a>
    )

}