import * as React from 'react';
import './Toggle.css';

export type ToggleProps = {
    id: string,
    checked: boolean,
    label?: string,
    switchStyle?: React.CSSProperties,
    containerStyle?: React.CSSProperties,
    title?: string,
    onToggled(change: boolean): void,

} & BaseProps;

export const Toggle: React.FC<ToggleProps> = props => {
    function handleKeyPress(e) {
        if (e.keyCode !== 32) return;

        e.preventDefault();
        props.onToggled(!props.checked)
    }

    const main = <div className="toggle-container" style={props.containerStyle}>
        <div className="toggle-switch" title={props.title}>
            <input
                type="checkbox"
                className="toggle-checkbox"
                id={props.id}
                name={props.id}
                checked={props.checked}
                onKeyDown={(e) => { handleKeyPress(e) }}
                onChange={e => {
                    props.onToggled(e.target.checked);
                }}
            />
            <label className="toggle-label" htmlFor={props.id}>
                <span className="toggle-inner" />
                <span className="toggle-toggle-switch" style={props.switchStyle} />
            </label>
        </div>
    </div>

    const labeled = <div className='inputContainer padding-lr'>
        <label className='inputLabel inputLabelUnfocused' htmlFor={props.id}>{props.label}</label>
        {main}
    </div>

    return props.label ? labeled : main;
}