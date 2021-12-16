import * as React from 'react';
import { EmbyProps, parseEmbyProps } from '~/emby/components/embyProps';
import { Inline } from './Inline';

export type InfoItem = {
    label: string,
    text: string
}

export type InfoRowProps = {
    InfoItems: InfoItem[]
} & React.AllHTMLAttributes<HTMLDivElement> & BaseProps;


export const InfoRow: React.FC<InfoRowProps> = props => {
    const embyProps = parseEmbyProps(props);
    console.log(props)
    return (
        <Inline>
            {props.InfoItems.map((i, key) =>
                <div className='info-row' key={key}>
                    <div className='info-row-label'>{i.label}</div>
                    <div className='info-row-text'>{i.text}</div>
                </div>
            )};
        </Inline>
    )


    return <div style={{ width: '100%', display: 'inline-flex' }} {...embyProps}>{props.children}</div>;
};