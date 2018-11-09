using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using XInputDotNetPure;

public enum SCKey{
	MENU_START,
	MENU_CREDITS,
	MENU_QUIT,

	SELECT_LEFT,
	SELECT_RIGHT,
	SELECT_ACCEPT,
	SELECT_BACK,

	MOVE_FORWARD,
	MOVE_BACKWARD,
	MOVE_LEFT,
	MOVE_RIGHT,

	AIM_FORWARD,
	AIM_BACKWARD,
	AIM_LEFT,
	AIM_RIGHT,

	ROTATE_LEFT,
	ROTATE_RIGHT,

	SPRINT,
	ATTACK
}

public class PlayerXInput {

	public const int NUM_PLAYERS = 4;
	public const float THUMBSTICK_THRESHOLD = 0.33f;
	public const float TRIGGER_THRESHOLD = 0.33f;

	private Dictionary<int, GamePadState> prev = new Dictionary<int, GamePadState> ();

	public PlayerXInput(){
		UpdatePreviousStates ();
	}

	public void UpdatePreviousStates(){

		// update previous gamepad states
		for (int i = 0; i < NUM_PLAYERS; i++) {
			prev [i] = GamePad.GetState ((PlayerIndex)i);
		}
	}

	public bool GamePadsConnected(){

		// check if any gamepads are connected
		for (int i = 0; i < NUM_PLAYERS; i++) {
			if (GamePad.GetState ((PlayerIndex)i).IsConnected) {
				return true;
			}
		}

		return false;
	}

	public bool PlayerConnected(int playerId){
		return GamePad.GetState((PlayerIndex) playerId).IsConnected;
	}

	public bool KeyDownAll(SCKey key){

		// detect a keydown for all players
		for (int i = 0; i < NUM_PLAYERS; i++) {
			if (KeyDown (key, i)) {
				return true;
			}
		}

		return false;
	}

	public GamePadState GetState(int playerId){
		return GamePad.GetState ((PlayerIndex)playerId);
	}

	public bool KeyDown(SCKey key, int playerId){

		bool isKeyDown = false;

		// player id not connected
		if (!PlayerConnected (playerId)) {
			return isKeyDown;
		}

		// retrieve gamepad for player id
		GamePadState state = GamePad.GetState((PlayerIndex) playerId);

		// menu
		if (key == SCKey.MENU_START && ButtonPressed(state.Buttons.A, prev[playerId].Buttons.A)) { isKeyDown = true; }
		else if (key == SCKey.MENU_CREDITS && ButtonPressed(state.Buttons.Y, prev[playerId].Buttons.Y)) { isKeyDown = true; }
		else if (key == SCKey.MENU_QUIT && ButtonPressed(state.Buttons.Start, prev[playerId].Buttons.Start)) { isKeyDown = true; }

		// character select
		else if (key == SCKey.SELECT_LEFT && ButtonPressed(state.DPad.Left, prev[playerId].DPad.Left)) { isKeyDown = true; }
		else if (key == SCKey.SELECT_LEFT && StickPressed(state.ThumbSticks.Left.X,-1f)) { isKeyDown = true; }
		else if (key == SCKey.SELECT_RIGHT && ButtonPressed(state.DPad.Right, prev[playerId].DPad.Right)) { isKeyDown = true; }
		else if (key == SCKey.SELECT_RIGHT && StickPressed(state.ThumbSticks.Left.X,1f)) { isKeyDown = true; }
		else if (key == SCKey.SELECT_ACCEPT && ButtonPressed(state.Buttons.A, prev[playerId].Buttons.A)) { isKeyDown = true; }
		else if (key == SCKey.SELECT_BACK && ButtonPressed(state.Buttons.B, prev[playerId].Buttons.B)) { isKeyDown = true; }

		// movement
		else if (key == SCKey.MOVE_FORWARD && StickPressed(state.ThumbSticks.Left.Y,1f)) { isKeyDown = true; }
		else if (key == SCKey.MOVE_BACKWARD && StickPressed(state.ThumbSticks.Left.Y,-1f)) { isKeyDown = true; }
		else if (key == SCKey.MOVE_LEFT && StickPressed(state.ThumbSticks.Left.X,-1f)) { isKeyDown = true; }
		else if (key == SCKey.MOVE_RIGHT && StickPressed(state.ThumbSticks.Left.X,1f)) { isKeyDown = true; }

		// aim
		else if (key == SCKey.AIM_FORWARD && StickPressed(state.ThumbSticks.Right.Y,1f)) { isKeyDown = true; }
		else if (key == SCKey.AIM_BACKWARD && StickPressed(state.ThumbSticks.Right.Y,-1f)) { isKeyDown = true; }
		else if (key == SCKey.AIM_LEFT && StickPressed(state.ThumbSticks.Right.X,-1f)) { isKeyDown = true; }
		else if (key == SCKey.AIM_RIGHT && StickPressed(state.ThumbSticks.Right.X,1f)) { isKeyDown = true; }

		// rotate
		else if (key == SCKey.ROTATE_LEFT && ButtonPressed(state.Buttons.LeftShoulder, prev[playerId].Buttons.LeftShoulder)) { isKeyDown = true; }
		else if (key == SCKey.ROTATE_RIGHT && ButtonPressed(state.Buttons.RightShoulder, prev[playerId].Buttons.RightShoulder)) { isKeyDown = true; }

		// actions
		else if (key == SCKey.SPRINT && TriggerPressed(state.Triggers.Left)) { isKeyDown = true; }
		else if (key == SCKey.ATTACK && TriggerPressed(state.Triggers.Right)) { isKeyDown = true; }

		return isKeyDown;
	}

	public IEnumerator Rumble(int playerId, float amount, float duration){		

		// rumble on
		GamePad.SetVibration ((PlayerIndex)playerId, amount, amount);

		yield return new WaitForSeconds (duration);

		// rumble off
		GamePad.SetVibration ((PlayerIndex)playerId, 0f, 0f);
	}

	private bool ButtonPressed(ButtonState state, ButtonState previousState){
		return state == ButtonState.Pressed && previousState == ButtonState.Released;
	}

	private bool StickPressed(float stick, float direction){
		return Mathf.Abs (stick) > THUMBSTICK_THRESHOLD && Mathf.Sign (stick) == Mathf.Sign (direction);
	}

	private bool TriggerPressed(float trigger){
		return Mathf.Abs (trigger) > TRIGGER_THRESHOLD;
	}
}
