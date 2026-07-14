window.StudentRegistration = window.StudentRegistration || {};
window.StudentRegistration.antiforgery = {
    getRequestToken: function () {
        const prefix = "XSRF-TOKEN=";
        const cookie = document.cookie
            .split(";")
            .map(value => value.trim())
            .find(value => value.startsWith(prefix));

        return cookie ? decodeURIComponent(cookie.substring(prefix.length)) : "";
    }
};
