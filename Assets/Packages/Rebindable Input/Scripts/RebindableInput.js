import System;
import System.Collections.Generic;

private static var rebindableManager : RebindableData;

function Awake () {

	rebindableManager = RebindableData.GetRebindableManager();
}

static function GetKey (inputName : String) {

	var keyDatabase = rebindableManager.GetCurrentKeys();
	
	for (var i = 0; i < keyDatabase.Count; i++) {
	
		if (keyDatabase[i].inputName == inputName) {
		
			return Input.GetKey (keyDatabase[i].input);
		}
	}
	
	throw RebindableNotFoundException ("The rebindable key '" + inputName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
}

static function GetKeyDown (inputName : String) {

	var keyDatabase = rebindableManager.GetCurrentKeys();

	for (var i = 0; i < keyDatabase.Count; i++) {
	
		if (keyDatabase[i].inputName == inputName) {
		
			return Input.GetKeyDown (keyDatabase[i].input);
		}
	}
	
	throw RebindableNotFoundException ("The rebindable key '" + inputName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
}

static function GetKeyUp (inputName : String) {

	var keyDatabase = rebindableManager.GetCurrentKeys();

	for (var i = 0; i < keyDatabase.Count; i++) {
	
		if (keyDatabase[i].inputName == inputName) {
		
			return Input.GetKeyUp (keyDatabase[i].input);
		}
	}
	
	throw RebindableNotFoundException ("The rebindable key '" + inputName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
}

static function GetAxis (axisName : String) {

	var axisDatabase = rebindableManager.GetCurrentAxes();
	
	for (var i = 0; i < axisDatabase.Count; i++) {
	
		if (axisDatabase[i].axisName == axisName) {
		
			var posPressed = Input.GetKey(axisDatabase[i].axisPos);
			var negPressed = Input.GetKey(axisDatabase[i].axisNeg);
		
			return 0 + (posPressed ? 1 : 0) - (negPressed ? 1 : 0);
		}
	}
	
	throw RebindableNotFoundException ("The rebindable axis '" + axisName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
}

static function GetKeyFromBinding (inputName : String) {

	var keyDatabase = rebindableManager.GetCurrentKeys();

	for (var i = 0; i < keyDatabase.Count; i++) {
	
		if (keyDatabase[i].inputName == inputName) {
		
			return keyDatabase[i].input;
		}
	}
	
	throw RebindableNotFoundException ("The rebindable key '" + inputName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
}

static function GetPositiveFromAxis (axisName : String) {

	var axisDatabase = rebindableManager.GetCurrentAxes();
	
	for (var i = 0; i < axisDatabase.Count; i++) {
	
		if (axisDatabase[i].axisName == axisName) {
		
			return axisDatabase[i].axisPos;
		}
	}
	
	throw RebindableNotFoundException ("The rebindable axis '" + axisName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
}

static function GetNegativeFromAxis (axisName : String) {

	var axisDatabase = rebindableManager.GetCurrentAxes();
	
	for (var i = 0; i < axisDatabase.Count; i++) {
	
		if (axisDatabase[i].axisName == axisName) {
		
			return axisDatabase[i].axisNeg;
		}
	}
	
	throw RebindableNotFoundException ("The rebindable axis '" + axisName + "' was not found.\nBe sure you have created it and haven't misspelled it.");
}