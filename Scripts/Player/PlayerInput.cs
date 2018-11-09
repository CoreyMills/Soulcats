using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class PlayerInput : MonoBehaviour {

	public static PlayerInput GET;

	public GameObject players;

	private List<Player> playersList = new List<Player>();
	private PlayerXInput xInput = new PlayerXInput();
	private Game game;

	public void Awake(){

		GET = this;

		// init cinput for keyboard only
		cInput.Init ();
		cInput.Clear ();
		cInput.allowDuplicates = true;
		cInput.deadzone = 0f;
		cInput.gravity = float.MaxValue;
		cInput.sensitivity = float.MaxValue;
		cInput.SetKey ("p0lsu", Keys.W);
		cInput.SetKey ("p0lsd", Keys.S);
		cInput.SetKey ("p0lsl", Keys.A);
		cInput.SetKey ("p0lsr", Keys.D);
		cInput.SetKey ("p0rsu", Keys.UpArrow);
		cInput.SetKey ("p0rsd", Keys.DownArrow);
		cInput.SetKey ("p0rsl", Keys.LeftArrow);
		cInput.SetKey ("p0rsr", Keys.RightArrow);
		cInput.SetKey ("p0crl", Keys.Q);
		cInput.SetKey ("p0crr", Keys.E);
		cInput.SetKey ("p0spr", Keys.LeftShift);
		cInput.SetKey ("p0w", Keys.Space, Keys.Mouse0);
		cInput.SetAxis ("p0lsv", "p0lsd", "p0lsu");
		cInput.SetAxis ("p0lsh", "p0lsl", "p0lsr");
		cInput.SetAxis ("p0rsv", "p0rsd", "p0rsu");
		cInput.SetAxis ("p0rsh", "p0rsl", "p0rsr");
		cInput.SetKey ("pause", Keys.Escape);
		cInput.SetKey ("pauseQuit", Keys.Enter);

		// sort players by player ids
		playersList = new List<Player>(this.players.GetComponentsInChildren<Player>());
		playersList.Sort((p1,p2)=>p1.playerId.CompareTo(p2.playerId));
	}

	public void Start(){

		// get game
		game = GameObject.FindObjectOfType<Game> ();
	}

	public void Update(){

		// handle input
		if (!game.paused) {
			for (int i = 0; i < playersList.Count; i++) {
				
				float lsv = (i == 0 ? cInput.GetAxis ("p0lsv") : 0f) + xInput.GetState (i).ThumbSticks.Left.Y;
				float lsh = (i == 0 ? cInput.GetAxis ("p0lsh") : 0f) + xInput.GetState (i).ThumbSticks.Left.X;
				float rsv = (i == 0 ? cInput.GetAxis ("p0rsv") : 0f) + xInput.GetState (i).ThumbSticks.Right.Y;
				float rsh = (i == 0 ? cInput.GetAxis ("p0rsh") : 0f) + xInput.GetState (i).ThumbSticks.Right.X;
				bool crl = (i == 0 ? cInput.GetKeyDown ("p0crl") : false) || xInput.KeyDown (SCKey.ROTATE_LEFT, i);
				bool crr = (i == 0 ? cInput.GetKeyDown ("p0crr") : false) || xInput.KeyDown (SCKey.ROTATE_RIGHT, i);
				bool sprint = (i == 0 ? cInput.GetKey ("p0spr") : false) || xInput.KeyDown (SCKey.SPRINT, i);
				bool weapon = (i == 0 ? cInput.GetKey ("p0w") : false) || xInput.KeyDown (SCKey.ATTACK, i);

				Player player = playersList [i];
				player.HandleInput (lsv, lsh, rsv, rsh, crl, crr, sprint, weapon);
			}
		}

		// toggle pause
		if (cInput.GetKeyDown ("pause") || xInput.KeyDownAll(SCKey.MENU_QUIT) || (game.paused && xInput.KeyDownAll(SCKey.SELECT_BACK))) {
			game.Pause ();
		}

		// quit game
		if (game.paused && (cInput.GetKeyDown ("pauseQuit") || xInput.KeyDownAll (SCKey.MENU_START))) {
			Time.timeScale = 1f;
			SceneManager.LoadScene ("Menu");
		}

		xInput.UpdatePreviousStates ();
	}
}
