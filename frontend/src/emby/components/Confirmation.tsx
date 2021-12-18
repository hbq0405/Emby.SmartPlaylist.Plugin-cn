import * as React from 'react';
import { Button } from '~/emby/components/Button';
import './Modal.css';
import './Confirmation.css'

export type ConfirmationProps = {
    title?: string;
    question: string;
    data?: any
    onYes(data: any): void;
    onNo(data: any): void;
};

export const Confirmation: React.FC<ConfirmationProps> = props => {
    return (
        <>
            <div className="dialogBackdrop dialogBackdropOpened" />
            <div className="dialogContainer">
                <div
                    className="focuscontainer dialog formDialog opened"
                    data-lockscroll="true"
                    data-history="true"
                    data-autofocus="true"
                    data-removeonclose="true"
                >
                    <div className="formDialogHeader">
                        <h3 className="formDialogHeaderTitle">{props.title}</h3>
                    </div>
                    <div className="formDialogContent scrollY">
                        <div className="confirm-text"><span>{props.question}</span></div>
                        <div className="confirm-footer">
                            <Button
                                type="submit"
                                onClick={_ => props.onYes(props.data)}
                                class="formDialogFooterItem"
                            >
                                Yes
                            </Button>
                            <Button
                                type="submit"
                                onClick={_ => props.onNo(props)}
                                class="formDialogFooterItem"
                            >No
                            </Button>
                        </div>
                    </div>
                </div>
            </div>
        </>
    );
};