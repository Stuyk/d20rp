var cef = null;
// Browser Class
class Browser {
    path: string;
    open: boolean;
    browser: any;

    constructor(resourcePath) {
        this.path = resourcePath;
        this.open = false;
    }

    show() {
        if (this.open == false) {
            this.open = true;
            var resolution = API.getScreenResolution();
            this.browser = API.createCefBrowser(resolution.Width, resolution.Height, true);
            API.waitUntilCefBrowserInit(this.browser);
            API.setCefBrowserPosition(this.browser, 0, 0);
            API.loadPageCefBrowser(this.browser, this.path);
            API.showCursor(true);
            API.setCanOpenChat(false);
        }
    }

    destroy() {
        this.open = false;
        API.destroyCefBrowser(this.browser);
        API.showCursor(false);
        API.setCanOpenChat(true);
    }

    eval(string) {
        this.browser.eval(string);
    }
}
// Destroy the browser if the resource stops.
API.onResourceStop.connect(function() {
    killBrowser();
});
// Create the browser.
function createBrowser(path) {
    if (cef !== null) {
        return;
    }

    cef = new Browser(path);
    cef.show();
}
// Destroy the browser.
function killBrowser() {
    if (cef === null) {
        return;
    }

    cef.destroy();
    cef = null;
}
// Login Function
function tryToLogin(username, password) {
    resource.login.pushLogin(username, password);
}
// Register Function
function tryToRegister(username, password) {
    resource.login.pushRegister(username, password);
}
// Login Function Failed
function loginFailed() {
    cef.browser.call("loginFailed");
}
// Register Function Failed
function registerFailed() {
    cef.browser.call("registerFailed");
}
// Register Function Success
function registerSuccess() {
    cef.browser.call("registerSuccess");
}