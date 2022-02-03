import * as React from 'react';
import './Toggle.css';

export type ToggleProps = {
    id: string,
    checked: boolean,
    onChange(change: boolean): void,

} & BaseProps;

export const Toggle: React.FC<ToggleProps> = props => {
    function handleKeyPress(e) {
        if (e.keyCode !== 32) return;

        e.preventDefault();
        props.onChange(!props.checked)
    }

    return (
        <div className="toggle-container">
            <div className="toggle-switch">
                <input
                    type="checkbox"
                    className="toggle-checkbox"
                    id={props.id}
                    name={props.id}
                    checked={props.checked}
                    onKeyDown={(e) => { handleKeyPress(e) }}
                    onChange={e => {
                        console.log('change');
                        props.onChange(e.target.checked);
                    }}
                />
                <label className="toggle-label" htmlFor={props.id}>
                    <span className="toggle-inner" />
                    <span className="toggle-toggle-switch" />
                </label>
            </div>
        </div>
    );
}