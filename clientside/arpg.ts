var arpg = false;
var resX = API.getScreenResolutionMantainRatio().Width;
var resY = API.getScreenResolutionMantainRatio().Height;
var arpgCamera = null;
var arpgTaskPosition = null;
var arpgTaskMarker = null;
var arpgRunModifier = false;
var arpgEntity = null;
var parking = false;

function setupARPG() {
    arpg = true;
    setupCamera();
    showCursor();
}

API.onKeyDown.connect(function(player, e) {
	if (!arpg) {
		return;
	}
	
	if (e.Shift) {
		switch (arpgRunModifier) {
			case true:
				arpgRunModifier = false;
				break;
            case false:
                arpgRunModifier = true;
                break;
		}
	}
});

API.onUpdate.connect(function() {
	// If Director Mode is not enabled. Do not go any further.
	if (arpg == false) {
		return;
	}
	// Line from player to Cursor.
	drawLineFromPositionToPlayer();
	drawLineFromCursorToPlayer();

	// Disable Controls Draw Some Text
	API.disableAllControlsThisFrame();
	API.drawText("~r~A-RPG MODE", 27, resY - 285, 0.5, 50, 211, 82, 255, 4, 0, true, true, 0);
	API.drawText("~g~Run: ~w~" + arpgRunModifier, 27, resY - 250, 0.5, 50, 211, 82, 255, 4, 0, true, true, 0);
	
	// If Left-Click is pressed, move the player.
	if (API.isDisabledControlJustPressed(24)) {
		var cursorPos = API.getCursorPositionMantainRatio();
		var cursorScreenToWorld = API.screenToWorldMantainRatio(cursorPos);
		var ray = API.createRaycast(API.getCameraPosition(API.getActiveCamera()), cursorScreenToWorld, -1, null);
		if (ray.didHitEntity) {
			var entity = ray.hitEntity;
			var entityType = API.getEntityType(entity);
			switch(entityType) {
				case 1:
					API.callNative("TASK_ENTER_VEHICLE", API.getLocalPlayer(), entity, -1, -1, 1.0, 1, 0);
					return;
			}
		}
		
		if (API.isPlayerInAnyVehicle(API.getLocalPlayer())) {
			if (arpgRunModifier) {
				arpgPark();
				return;
			}
			arpgDrive(30);
			return;
		}
		
		if (arpgRunModifier) {
			arpgMove(4.0);
			return;
		}
		
		arpgMove(1.0);
		return;
	}
	
	// Face Coordinate after Run / Walk
	if (arpgTaskPosition != null) {
		if (API.getEntityPosition(API.getLocalPlayer()).DistanceTo(arpgTaskPosition) <= 2) {
			if (API.isPlayerInAnyVehicle(API.getLocalPlayer()) === false) {
				API.callNative("TASK_TURN_PED_TO_FACE_COORD", API.getLocalPlayer(), arpgTaskPosition.X, arpgTaskPosition.Y, arpgTaskPosition.Z, -1);
			}
			
			if (parking) {
				API.callNative("TASK_TURN_PED_TO_FACE_COORD", API.getLocalPlayer(), arpgTaskPosition.X, arpgTaskPosition.Y, arpgTaskPosition.Z, -1);
				parking = false;
			}
			
			arpgTaskPosition = null;
			if (arpgTaskMarker != null) {
				API.deleteEntity(arpgTaskMarker);
				arpgTaskMarker = null;
			}
			return;
		}
	}
});

// Camera for ARPG mode.
function setupCamera() {
	var player = API.getLocalPlayer();
	arpgCamera = API.createCamera(new Vector3(), new Vector3());
	API.attachCameraToEntity(arpgCamera, player, new Vector3(0, -2, 15));
	API.pointCameraAtEntity(arpgCamera, player, new Vector3());
	API.setCameraFov(arpgCamera, 80);
    API.setActiveCamera(arpgCamera);
}

// Cursors
function showCursor() {
	API.showCursor(true);
}

function disableCursor() {
	API.showCursor(false);
}

// Utility
function cursorToWorld() {
	var cursorPos = API.getCursorPosition();
	arpgTaskPosition = API.screenToWorld(cursorPos);
}

function cursorToWorldReturn() {
	var cursorPos = API.getCursorPosition();
	var worldPos = API.screenToWorld(cursorPos);
	return worldPos;
}

function createMovementMarker() {
	if (arpgTaskMarker != null) {
		API.deleteEntity(arpgTaskMarker);
		arpgTaskMarker = null;
	}
	
	arpgTaskMarker = API.createMarker(27, new Vector3(arpgTaskPosition.X, arpgTaskPosition.Y, arpgTaskPosition.Z + 0.2), new Vector3(), new Vector3(), new Vector3(0.5, 0.5, 1.0), 66, 197, 244, 200);
}

function drawLineFromPositionToPlayer()
{
	if (arpgTaskPosition === null) {
		return;
	}
	
	var playerPos = API.getEntityPosition(API.getLocalPlayer());
	API.drawLine(arpgTaskPosition, playerPos, 200, 66, 197, 244);
	
	var pos = playerPos.DistanceTo(arpgTaskPosition);
	var lerp = Vector3.Lerp(playerPos, arpgTaskPosition, 0.5);
	var pointer = Point.Round(API.worldToScreen(lerp));
	API.drawText("Distance: " + Math.round(pos), pointer.X, pointer.Y, 0.5, 66, 197, 244, 200, 4, 0, false, true, 0);
}

function drawLineFromCursorToPlayer()
{
	if (arpgTaskPosition !== null) {
		return;
	}
	
	var worldPos = cursorToWorldReturn();
	var playerPos = API.getEntityPosition(API.getLocalPlayer());
	API.drawLine(worldPos, playerPos, 255, 0, 255, 0);
	
	var pos = playerPos.DistanceTo(worldPos);
	var lerp = Vector3.Lerp(playerPos, worldPos, 0.5);
	var pointer = Point.Round(API.worldToScreen(lerp));
	API.drawText("Distance: " + Math.round(pos), pointer.X, pointer.Y, 0.5, 0, 255, 0, 200, 4, 0, false, true, 0);
}

// Movement and Stuff
// General Movement
function arpgMove(speed) {
	cursorToWorld();
	//API.callNative("TASK_GO_STRAIGHT_TO_COORD", API.getLocalPlayer(), arpgTaskPosition.X, arpgTaskPosition.Y, arpgTaskPosition.Z, API.f(speed), -1, 0, 0);
	API.callNative("TASK_GO_TO_COORD_ANY_MEANS", API.getLocalPlayer(), arpgTaskPosition.X, arpgTaskPosition.Y, arpgTaskPosition.Z, API.f(speed), 0, 0, 786603, 0);
	createMovementMarker();
}

// General Driving
function arpgDrive(speed) {
	var cursorPos = API.getCursorPosition();
	arpgTaskPosition = API.screenToWorld(cursorPos);
	API.callNative("TASK_VEHICLE_DRIVE_TO_COORD", API.getLocalPlayer(), API.getPlayerVehicle(API.getLocalPlayer()), arpgTaskPosition.X, arpgTaskPosition.Y, arpgTaskPosition.Z, API.f(speed), API.f(1), null, 786603, API.f(1), true);
	createMovementMarker();
}

// If Run is true, park.
function arpgPark() {
	var cursorPos = API.getCursorPosition();
	arpgTaskPosition = API.screenToWorld(cursorPos);
	API.callNative("TASK_VEHICLE_PARK", API.getLocalPlayer(), API.getPlayerVehicle(API.getLocalPlayer()), arpgTaskPosition.X, arpgTaskPosition.Y, arpgTaskPosition.Z, API.f(0), 1, API.f(20), true);
	parking = true;
	if (arpgTaskMarker != null) {
		API.deleteEntity(arpgTaskMarker);
		arpgTaskMarker = null;
	}
	arpgTaskMarker = API.createMarker(1, arpgTaskPosition, new Vector3(), new Vector3(), new Vector3(0.1, 0.1, 1.0), 66, 197, 244, 200);
}