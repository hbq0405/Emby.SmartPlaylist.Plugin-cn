import * as React from 'react';
import { EmbyProps } from "~/emby/components/embyProps";

export type InfoLabelProps = {
    key?: number,
    label: string,
    text: string
} & React.AllHTMLAttributes<HTMLDivElement> & BaseProps & EmbyProps;

export const InfoLabel: React.FC<InfoLabelProps> = props => {
    return (
        <div {...props} className='info-row' key={props.key}>
            <div className='info-row-label'>{props.label}</div>
            <div className='info-row-text'>{props.text}</div>
        </div>
    );
};