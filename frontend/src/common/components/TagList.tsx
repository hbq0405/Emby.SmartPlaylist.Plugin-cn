import * as React from 'react';
import { EmbyProps } from '~/emby/components/embyProps';

export type TagListProps = {
    Items: string[]
} & React.AllHTMLAttributes<HTMLDivElement> & BaseProps & EmbyProps;


export const TagList: React.FC<TagListProps> = props => {
    return (
        <div className='tag-list'>
            {props.Items.map(x =>
                <div className='emby-button'>{x}</div>
            )}
        </div>
    )
};