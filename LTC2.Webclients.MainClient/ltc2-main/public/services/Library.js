function Version(){
    return "v1.0.1"
}

console.log(Version());

var profile;

function getToken() {
    return getCookie("token");;
}

function getCookie(cname) {
    var name = cname + "=";
    var decodedCookie = decodeURIComponent(document.cookie);
    var ca = decodedCookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) == ' ') {
            c = c.substring(1);
        }
        if (c.indexOf(name) == 0) {
            return c.substring(name.length, c.length);
        }
    }
    return "";
}

function getProfile() {
    var token = getCookie("token");
    var request = new XMLHttpRequest();
    var profileUrl = "../api/Profile/profile";

    request.onreadystatechange = function () {
        if (request.readyState === 4) {
            if (request.status === 200) {
                profile = JSON.parse(this.response);

                document.getElementById("name").innerText = profile.name;
            } else if (request.status === 401) {
                window.location = "/Home?forceLogout=true";
            } else {
                window.location = "/Error";
            }
        }
    }

    request.open("GET", profileUrl);
    request.setRequestHeader('Authorization', 'Bearer ' + token);
    request.send();
}

function requestCalculation(refresh, bypassCache) {
    if (profile) {
        var token = getCookie("token");
        var request = new XMLHttpRequest();

        var updateKindIndication = refresh ? "true" : "false";
        var updateKindParameter = "refresh=" + updateKindIndication;
        var bypassCacheIndication = bypassCache ? "true" : "false";
        var bypassCacheParameter = "&bypassCache=" + bypassCacheIndication;
        var updateUrl = "../api/Update/update?" + updateKindParameter + bypassCacheParameter; 

        request.onreadystatechange = function () {
            if (request.readyState === 4) {
                if (request.status === 200) {
                    alert('Update posted!')
                } else if (request.status === 401) {
                    window.location = "/Home?forceLogout=true";
                } else {
                    window.location = "/Error";
                }
            }
        }

        request.open("POST", updateUrl);
        request.setRequestHeader('Authorization', 'Bearer ' + token);
        request.send();
    }
}
