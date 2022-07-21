import * as React from 'react';
import { HierarchyString } from '~/app/types/appData';
import { Inline } from './Inline';

export type HierarchyStringProps = {
    value: HierarchyString
}

export const HierarchyStringContainer: React.FC<HierarchyStringProps> = props => {


    const getContainer = (hs: HierarchyString): React.ReactElement => {
        return (
            <>
                <div style={{ width: '100%', paddingLeft: (hs.level * 10) + 'px' }}>
                    {hs.value}
                </div>

                {hs.children.map((x) => getContainer(x))}
            </>
        )
    }

    return getContainer(props.value);
};
