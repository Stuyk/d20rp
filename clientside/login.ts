var loginCamera = null;
var player = API.getLocalPlayer();
// Kill the HUD, Setup the Login Camera, Push the Login Screen.
API.onResourceStart.connect(function () {
    API.setHudVisible(false);
    createLoginCamera();
    resource.browser.createBrowser("clientside/login.html");
});
// If the resource happens to stop, fix the hud.
API.onResourceStop.connect(function () {
    API.setHudVisible(true);
});
// This just spins the camera around real slow like.
API.onUpdate.connect(function () {
    if (loginCamera !== null) {
        API.setCameraRotation(loginCamera, API.getCameraRotation(loginCamera).Add(new Vector3(0, 0, 0.02)));
        API.disableAllControlsThisFrame();
    }
});
// Handle Server Events
API.onServerEventTrigger.connect(function (eventName, args) {
    switch (eventName) {
        case "finishLoginCamera":
            finishLoginCamera();
            return;
        case "loginFailed":
            resource.browser.loginFailed();
            return;
        case "registerFailed":
            resource.browser.registerFailed();
            return;
        case "registerSuccess":
            resource.browser.registerSuccess();
            return;
    }
});
// Create the login camera.
function createLoginCamera() {
    player = API.getLocalPlayer();
    loginCamera = API.createCamera(API.getEntityPosition(player).Add(new Vector3(0, 0, 100)), new Vector3());
    API.setActiveCamera(loginCamera);
}
// Kill the login camera, push the ARPG mode out.
function finishLoginCamera() {
    loginCamera = null;
    API.setActiveCamera(null);
    API.setHudVisible(true);
    resource.browser.killBrowser();
    resource.arpg.setupARPG();
}
// Trigger the server event for login.
function pushLogin(username, password) {
    API.triggerServerEvent("tryLogin", username, password);
}
// Trigger the server event for registration.
function pushRegister(username, password) {
    API.triggerServerEvent("tryRegister", username, password);
}