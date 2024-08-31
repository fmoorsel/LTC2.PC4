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

function leftpad(val: number, resultLength = 2, leftpadChar = '0'): string {
    return (String(leftpadChar).repeat(resultLength)
          + String(val)).slice(String(val).length);
  }