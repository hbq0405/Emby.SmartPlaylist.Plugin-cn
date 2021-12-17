import * as React from 'react';
import { EmbyProps } from '~/emby/components/embyProps';
import { Inline } from './Inline';

export type InfoItem = {
    label: string,
    text: string
}

export type InfoRowProps = {
    InfoItems: InfoItem[]
} & React.AllHTMLAttributes<HTMLDivElement> & BaseProps & EmbyProps;


export const InfoRow: React.FC<InfoRowProps> = props => {
    return (
        <Inline {...props}>
            {props.InfoItems.map((i, key) =>
                <div className='info-row' key={key}>
                    <div className='info-row-label'>{i.label}</div>
                    <div className='info-row-text'>{i.text}</div>
                </div>
            )};
        </Inline>
    )
};