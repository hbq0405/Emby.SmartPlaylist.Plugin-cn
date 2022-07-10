import * as React from 'react';
import { AppContext } from '~/app/state/app.context';
import { SelectProps } from '~/common/components/Select';
import { Guid } from '~/common/helpers/guid';

type UserListInputProps = {
    userId: string;
    onChange(userId: string): void;
} & React.SelectHTMLAttributes<HTMLSelectElement> &
    BaseProps;

export const UserListInput: React.FC<UserListInputProps> = props => {
    const appContext = React.useContext(AppContext);

    return (
        <div className='selectContainer padding-lr inline'>
            <select is="emby-select" {...props} onChange={e => props.onChange(e.target.value)} value={props.userId}>
                <option value={''}>[Current User]</option>
                {appContext.getUsers().map(item => (
                    <option key={item.id} value={item.id}>
                        {item.name}
                    </option>
                ))}
            </select>
        </div>
    )
}