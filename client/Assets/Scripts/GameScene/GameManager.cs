using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Rogue;
using System.Linq;
using System;

public class GameManager : MonoSingleton<GameManager> {

	public GameObject FieldPanel;
	//public MapManager[] MapManagers;
	public GameObject Objects;
	//public PlayerDispManager PlayerDisp;
	//public EnemyManager EnemyBase;

	//public HPGaugeController HpGauge;
	//public UILabel HpLabel;
	//public UILabel TurnLabel;
	//public UISprite TurnSprite;

	public Game Game;
	public Vector2 ScrollPos;

	public GameObject ObjGameOver;
	public GameObject ObjGameClear;

	// Use this for initialization
	void Start () {
		Game = new Game ();

		string mapdata = 
@"
22222222222222222222  	 
22222222222222222222 
22222222222222222222 
22222222211111222222 
22222222211311222222 
22221111111111222222 
22221222211111111222 
22221222211111221222 
22111122222222221222 
22111122222222221222 
22111111122222221222 
22111122122222111122 
22222222122222111122 
22222222122222111122 
22222111112222111122 
22222111111111111122 
22222111112222111122 
22222111112222222222 
22222222222222222222 
22222222222222222222
";
		var data = mapdata.ToArray().Where(c=>char.IsDigit(c)).Select(c=>int.Parse(""+c)).ToArray();

		data = MakeMap (data, 20, 20);

		for (int y = 0; y < 20; y++) {
			for (int x = 0; x < 20; x++) {
				Game.Map [x, y].Kind = CellKind.Find (data [y * 20 + x]);
			}
		}

		var pl = new Player (Game){ Name = "ドラン", MaxHp=120, Hp = 120, Attack=14 };
		Game.Map.AddCharacter (pl, new Point(7,17));
		Game.Player = pl;
		//CharManagers [pl.Id] = PlayerDisp;

		//var rand = new UnityEngine.Random ();
		for (int i = 0; i < 4; i++) {
			var en = new Enemy (Game){ Name = "パイロドラゴン", Hp=40 };
			while (true) {
				var x = UnityEngine.Random.Range (0, 19);
				var y = UnityEngine.Random.Range (0, 19);
				if (Game.Map.FloorIsWalkableNow (new Point (x, y))) {
					Game.Map.AddCharacter (en, new Point(x,y));
					break;
				}
			}
			en.Id = i + 1;
			//var eo = CreateEnemy (en.Id);
			//eo.MoveTo (en.Position, en.Position, DIRECTION.SOUTH);
			//MiniMapManager.Instance.positionEnemy (en.Id, en.Position.x, en.Position.y);
		}
		Game.CmdMovePlayer (Game.Player.Position);

		//HpGauge.SetMaxHP (120);
		//HpGauge.SetMaxHP (120);
		//HpLabel.text = "HP 120/120";
		//TurnLabel.text = ""+Game.Player.Turn;

		StartCoroutine (DoUpdate());
	}

	public int[] MakeMap(int[] d, int w, int h){
		int[] ct = new int[]{
			138, 77, 55, 60, 57, 54, 40, 46, 56, 61, 
			75, 64, 41, 47, 44, 0
		};
		var r = new int[d.Count ()];
		var wall = 2;
		for (var y = 0; y < h; y++) {
			for (var x = 0; x < w; x++) {
				if (x <= 0 || x >= w - 1 || y <= 0 || y >= h - 1) {
					//はじっこ
					r[y*w+x] = 0;
				} else {
					if (d [y * w + x] == 2) {
						var n = 0;
						if (d [(y - 1) * w + x] == wall)
							n += 1;
						if (d [y * w + x + 1] == wall)
							n += 2;
						if (d [(y + 1) * w + x] == wall)
							n += 4;
						if (d [y * w + x - 1] == wall)
							n += 8;
						if (n < 15) {
							r [y * w + x] = ct [n];
						} else {
							if (d [(y - 1) * w + x + 1] != wall) {
								r [y * w + x] = 62;
							} else if (d [(y + 1) * w + x + 1] != wall) {
								r [y * w + x] = 42;
							} else if (d [(y + 1) * w + x - 1] != wall) {
								r [y * w + x] = 43;
							} else if (d [(y - 1) * w + x - 1] != wall) {
								r [y * w + x] = 63;
							} else {
								r [y * w + x] = 0;
							}
						}
					}else if (d [y * w + x] == 3) {
						r [y * w + x] = 19;
					} else {
						r [y * w + x] = UnityEngine.Random.Range(1,3);
					}
				}
			}
		}
		return r;
	}

	/*
	Dictionary<int,CharacterDispManager> CharManagers = new Dictionary<int,CharacterDispManager>();

	EnemyManager CreateEnemy(int id ){
		var newEnemy = (GameObject)GameObject.Instantiate (EnemyBase.gameObject);
		var em = newEnemy.GetComponent<EnemyManager> ();
		CharManagers [id] = em;
		em.transform.SetParent (Objects.transform, false);
		newEnemy.gameObject.SetActive(true);
		return em;
	}
	*/
	
	// Update is called once per frame
	IEnumerator DoUpdate () {
		//MiniMapManager.Instance.Draw (Game.Map);
		//MiniMapManager.Instance.DrawPlayer (Game.Player.Position.x, Game.Player.Position.y);

		bool exit = false;
		string lastMessageType = null;
		while (!exit) {
			switch( Game.State ){
			case GameState.TurnEnd:
				Game.Process ();
				//MiniMapManager.Instance.Draw (Game.Map);
				break;
			case GameState.GameOver:
				exit = true;
				break;
			case GameState.Player:
				yield return null;
				if ( IsPressA ){
					IsPressA = false;
					Game.CmdAttack (Game.Player.Dir);
					Game.Process ();
				} else {
					Point apos = new Point (0, 0);
					int push_count = 0;


					if (Input.GetKey (KeyCode.UpArrow)) {
						apos += new Point (0, -1);
						push_count ++;
					}
					if (Input.GetKey (KeyCode.DownArrow)) {
						apos += new Point (0, 1);
						push_count ++;
					}
					if (Input.GetKey (KeyCode.LeftArrow)) {
						apos += new Point (-1, 0);
						push_count ++;
					}
					if (Input.GetKey (KeyCode.RightArrow)) {
						apos += new Point (1, 0);
						push_count ++;
					}
					if (Input.GetKey (KeyCode.R)) {
						if (push_count < 2) {
							apos = new Point (0, 0);
						}
					}
					if (!apos.IsOrigin) {
						var pos = Game.Player.Position + apos;
						if (Input.GetKey (KeyCode.Y)) {
							var dir = apos.ToDir ();
							//PlayerDisp.SetDirection (dir);
							Game.Player.Dir = dir;
						} else if (Game.Map.StepWalkableNow () (Game.Player.Position, pos) <= 1) {
							Game.CmdMovePlayer (pos);
							//MiniMapManager.Instance.DrawPlayer (pos.x, pos.y);
							Game.Process ();
						} else {
							var dir = apos.ToDir ();
							//PlayerDisp.SetDirection (dir);
							Game.Player.Dir = dir;
						}
					}
				}
				break;
			default:
				Game.Process ();
				break;
			}

			for( int i=0; i<Game.Messages.Count(); i++){
				var m = Game.Messages [i];
				float wait = ProcessMessage (m);
				if( lastMessageType == "walk" && m.Type != "walk" ){
					yield return new WaitForSeconds(0.2f - 1.0f/30f); // 連続のwalkが終わった瞬間だけ、waitをいれる
				}
				if (wait > 0) {
					yield return new WaitForSeconds (wait);
				}
				lastMessageType = m.Type;
			}
			Game.Messages.Clear ();
		}
	}

	public void Update(){
		//ScrollPos = PlayerDisp.transform.localPosition + new Vector3(0,-50);
		FieldPanel.transform.localPosition = new Vector3 (-ScrollPos.x, -ScrollPos.y, 0);
	}

	public float ProcessMessage(Game.Message m){
		/*
		//Debug.Log (m.Type + " " + String.Join (" ", m.Param.Select (x => x.ToString ()).ToArray ()));
		switch (m.Type) {
		case "walk":
			{
				var ch = (Character)m.Param [0];
				var fromPos = (Point)m.Param [1];
				var toPos = (Point)m.Param [2];
				var en = CharManagers [ch.Id];
				var dir = (toPos - fromPos).ToDir ();
				en.MoveTo (fromPos, toPos, dir);
				en.SetDirection (dir);
				if (ch.Type == CharacterType.Enemy) {
					MiniMapManager.Instance.positionEnemy (ch.Id, toPos.x, toPos.y);
				}
				return 0;
			}
		case "attack":
			{
				var ch = (Character)m.Param [0];
				var dir = (DIRECTION)m.Param [1];
				//var damage = (int)m.Param [2];
				var en = CharManagers [ch.Id];
				en.SetDirection (dir);
				en.SetAttack ();
				if (ch.Type == CharacterType.Player) {
					SoundManager.PlaySE("playerAttackPhysics");
				} else {
					SoundManager.PlaySE("enemyAttack");
				}
				return 0.3f;
			}
		case "suburi":
			{
				var ch = (Character)m.Param [0];
				var en = CharManagers [ch.Id];
				if (ch.Type == CharacterType.Player) {
					(en as PlayerDispManager).SetAttack (false, true);
					SoundManager.PlaySE("playerAttackMiss");
				} else {
					SoundManager.PlaySE("enemyAttack");
				}
				return 0.2f;
			}
		case "damaged":
			{
				var ch = (Character)m.Param [0];
				var damage = (int)m.Param [1];
				var hp = (int)m.Param [2];
				var en = CharManagers [ch.Id];
				en.Damaged (ch.Position, damage);
				if (en == PlayerDisp) {
					HpLabel.text = string.Format ("HP {0}/{1}", hp, 120);
					HpGauge.SetMaxHP (120);
					HpGauge.SetNowHP (hp);
					SoundManager.PlaySE ("playerDamage");
				} else {
					SoundManager.PlaySE ("monsterDamage");
				}
				return 0.5f;
			}
		case "goal":
			{
				GameClear ();
				return 0.3f;
			}
		case "message":
			{
				var mes = (string)m.Param [0];
				//Debug.Log (mes);
				messageWindow.GetComponent<MessageWindow> ().MessageText = mes;
				return 0;
			}
		case "dead":
			{
				var ch = (Character)m.Param [0];
				CharacterDispManager chm;
				if (CharManagers.TryGetValue (ch.Id, out chm)) {
					if (chm is EnemyManager) {
						SoundManager.PlaySE ("dead_mons_m");
						(chm as EnemyManager).SetDead ();
						MiniMapManager.Instance.positionEnemy (ch.Id, ch.Position.x, ch.Position.y, false);
					}
					CharManagers.Remove (ch.Id);
				}
				return 0.3f;
			}
		case "gameover":
			{
				GameOver ();
				return 0.3f;
			}
		case "turn_down":
			{
				var turn = (int)m.Param [0];
				var hp = (int)m.Param [1];
				TurnLabel.text = ""+turn;
				var tween = TurnSprite.gameObject.GetComponent<TweenRotation> ();
				tween.ResetToBeginning ();
				tween.PlayForward ();
				HpLabel.text = string.Format ("HP {0}/{1}", hp, 120);
				HpGauge.SetMaxHP (120);
				HpGauge.SetNowHP (hp);
				return 0;
			}
		case "changedir":
		{
			var ch = (Character)m.Param [0];
			var dir = (DIRECTION)m.Param [1];
			//var damage = (int)m.Param [2];
			var en = CharManagers [ch.Id];
			en.SetDirection (dir);
			return 0;
		}
		default:
			return 0f;
		}
		*/
		return 0;
	}

	public bool IsPressA = false;
	public void PressA(){
		IsPressA = true;
	}

	public Vector2 MapCenter(){
		return ScrollPos;
	}

	public int MapView(int layer, int x, int y){
		if (Game == null) {
			return 0;
		}else{
			if (layer == 0) {
				var k = Game.Map [x, y].Kind.Id;
				if( k == 0 || (k >= 40 && k < 80) ){
					return 5;
				}else{
					return -1;
				}
			} else if (layer == 1) {
				return Game.Map [x, y].Kind.Sprite;
			} else {
				return -1;
			}
		}
	}

	public void GameClear(){
		//Debug.Log("GameClear!!!");
		ObjGameClear.SetActive (true);
		//SoundManager.StopBGM(0);
		//SoundManager.PlaySE("jingle_getfang");
		//PlayerDisp.SetGet ("");
		Invoke ("LoadTitle", 6.5f);
	}

	public void GameOver(){
		//SoundManager.StopBGM(0);
		//SoundManager.PlaySE("playerDead");
		closeMessageWindow(true);
		ObjGameOver.SetActive(true);
		//PlayerDisp.SetDead();
	}

	public void LoadTitle() {
		//SoundManager.Stop();
		Application.LoadLevel ("00_Initialize");
	}

	public static Vector3 CellToLocal(Point p){
		return new Vector3(p.X * 100 + 50, -p.Y * 100 - 50);
	}

	/// <summary>
	/// メッセージ関連
	/// </summary>
	public GameObject messageWindow;
	protected bool is_close_message_window = false;
	protected float messageDispStartTime = 0.0f ;
	protected float messageDispDurationTime = 0.6f ;

	/// <summary>
	/// Opens the message window.
	/// </summary>
	void openMessageWindow() {
		messageWindow.SetActive(true);
		is_close_message_window = false;
		messageDispStartTime = Time.time;
	}
	
	/// <summary>
	/// Closes the message window.
	/// </summary>
	void closeMessageWindow(bool immediate = false) {
		if (immediate || (messageDispStartTime + messageDispDurationTime < Time.time)) {
			messageWindow.SetActive(false);
		} else {
			is_close_message_window = true;
		}
	}
	
	/// <summary>
	/// Checks the message window.
	/// </summary>
	void checkMessageWindow() {
		if (is_close_message_window) {
			if (messageDispStartTime + messageDispDurationTime < Time.time) {
				messageWindow.SetActive(false);
				is_close_message_window = false;
			}
		}
	}

}
