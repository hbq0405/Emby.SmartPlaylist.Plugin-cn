import { toast } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

toast.configure();

export type errorProps = {
    msg: string,
    content: any
    timeout?: number
}

export function showError(errorProps): void {
    var m = `${errorProps.msg}: ${(errorProps.content instanceof Error ? errorProps.content.message : errorProps.content)}`;
    toast.error(m, {
        containerId: "modalToast",
        autoClose: errorProps.timeout ? errorProps.timeout : false,
        position: 'top-center',
        bodyStyle: {
            zIndex: 1000
        }
    });
}

export function showInfo(msg: string) {
    toast(msg);
}

export function tryParseInt(i: any) {
    if (isNaN(i))
        throw Error(i + ' is not a valid number.');

    return parseInt(i);
}