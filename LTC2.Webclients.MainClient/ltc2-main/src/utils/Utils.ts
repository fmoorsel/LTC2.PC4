export function GetCookie(cname: string): string {
    const name = cname + "=";
    const decodedCookie = decodeURIComponent(document.cookie);
    const ca = decodedCookie.split(';');
    
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }

        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }

    return "";    
}

export function IsMobile(): boolean {
    const mobile = typeof orientation !== 'undefined' || navigator.userAgent.toLowerCase().includes('mobile');
    
    return mobile;
}

export function fromatDateAsYYYYDDMM(date: string): string {
    const nd = date === "" ? new Date() : new Date(date);

    return nd.getFullYear() + '-' + leftpad(nd.getMonth() + 1, 2) + '-' + leftpad(nd.getDate(), 2);
}

export function leftpad(val: number, resultLength = 2, leftpadChar = '0'): string {
    return (String(leftpadChar).repeat(resultLength)
          + String(val)).slice(String(val).length);
}

export function getCookie(cname: string): string {
    const name = cname + "=";
    const decodedCookie = decodeURIComponent(document.cookie);
    const ca = decodedCookie.split(';');
    
    for (let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }

    return "";
}

let isMultiSportMode: boolean | undefined = undefined;

export function runsInMultiSportMode(): boolean {
    if (isMultiSportMode != undefined) {
        return isMultiSportMode;
    } else {
        const multiCookie = getCookie('multi');

        isMultiSportMode = multiCookie === 'multi';

        return isMultiSportMode;
    }
}

let isRideWithGpsMode: boolean | undefined = undefined;

export function runsInRideWithGpsMode(): boolean {
    if (isRideWithGpsMode != undefined) {
        return isRideWithGpsMode;
    } else {
        const params = new URLSearchParams(window.location.search);
        const value = params.get('ridewithgps');

        isRideWithGpsMode = value !== null && value !== '' && !isNaN(Number(value));

        return isRideWithGpsMode;
    }
}