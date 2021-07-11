import * as React from 'react';
import { parseEmbyProps } from '~/emby/components/embyProps';
export type ListItemProps = {
    onEditClick():void;
    onDeleteClick():void;
    label:string;
} & React.AllHTMLAttributes<HTMLDivElement> &
    BaseProps;

export const ListItem: React.FC<ListItemProps> = props => {
    return(
            <a className="listItem listItem-border emby-button" is="emby-linkbutton" data-ripple="false" onClick={()=>props.onEditClick()}>
                <div className="listItemImageContainer"><i className="listItemIcon md-icon listItemIcon-transparent">notifications_active</i></div>
                <div className="listItemBody">
                    <div className="listItemBodyText">{props.label}</div>
                </div>
                <button type="button" is="paper-icon-button-light" className="paper-icon-button-light icon-button-conditionalfocuscolor" onClick={()=>props.onEditClick()}><i className="md-icon">edit</i></button>
            </a>
    )

}