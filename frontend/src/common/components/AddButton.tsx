

import * as React from 'react';

export type AddButtonProps = {
    onClick():void;
    label:string;
} & React.AllHTMLAttributes<HTMLDivElement> &
    BaseProps;

export const AddButton: React.FC<AddButtonProps> = props => {
    return(
        <button is="emby-button" type="button" className="raised raised-mini btnAdd submit emby-button" onClick={()=>props.onClick()} style={{marginLeft:'.5em'}} title={props.label} aria-label={props.label}>
            <i className="md-icon button-icon button-icon-left"></i><span>{props.label}</span>
        </button>
    )
}


