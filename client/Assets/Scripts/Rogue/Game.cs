using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rogue
{
	public class GameOverException : Exception {}

	public enum GameState {
		TurnStart,
		Player,
		EnemyAttack,
		EnemyMove,
		TurnEnd,
		GameOver,
	}

	public class Game
	{
		public class Message{
			public string Type;
			public object[] Param;
		}

		public Map Map { get; private set; }
		public PathFinder PathFinder { get; private set; }
		public GameState State { get; private set; }
		public int TurnNum{ get; private set; }
		public Player Player;
		public List<Message> Messages = new List<Message>();

		public void log(object obj){
		//	Console.WriteLine (obj);
		}
		
		public Game ()
		{
			Map = new Map (20, 20);
			PathFinder = new PathFinder (Map.Width, Map.Height);
			State = GameState.TurnStart;
			TurnNum = 0;
		}


		public void Send(string command, params object[] param){
			log(command + ": " + String.Join(", ", param.Select(x=>x.ToString()).ToArray()) );
			Messages.Add (new Message (){ Type = command, Param = param });
		}

		public void Process(){
			try {
				switch (State) {
				case GameState.TurnStart:
					DoTurnStart ();
					break;
				case GameState.Player:
					DoPlayer ();
					break;
				case GameState.EnemyAttack:
					DoEnemyAttack ();
					break;
				case GameState.EnemyMove:
					DoEnemyMove ();
					break;
				case GameState.TurnEnd:
					DoTurnEnd ();
					break;
				case GameState.GameOver:
					break;
				}
			} catch (GameOverException ex) {
				State = GameState.GameOver;
			}
		}

		public void DoTurnStart(){
			TurnNum++;
			State = GameState.Player;
		}

		public void DoPlayer(){
			State = GameState.EnemyAttack;
		}

		public void DoEnemyAttack(){
			State = GameState.EnemyMove;
			foreach (var ch in Map.CharacterList) {
				if (ch.Type == CharacterType.Player) continue;
				ch.DoAttack ();
			}
		}

		public void DoEnemyMove(){
			State = GameState.TurnEnd;
			foreach (var ch in Map.CharacterList) {
				if (ch.Type == CharacterType.Player) continue;
				ch.DoMove ();
			}
		}

		public void DoTurnEnd(){
			State = GameState.TurnStart;
			if (!(CheckAttack (Player.Dir))) {
				foreach( var p in PathFinder.FindAround(Player.Position, 1, false, Map.FloorIsFlyable) ){
					var ch = Map [p].Character;
					if (ch != null && ch.Type == CharacterType.Enemy) {
						Player.Dir = (ch.Position - Player.Position).ToDir ();
						Send("changedir", Player, Player.Dir);
						break;
					}
				}
			}
			Player.DoTurnEnd ();
		}

		public string Display(){
			var sb = new StringBuilder ();
			for (int y = 0; y < Map.Width; y++) {
				for (int x = 0; x < Map.Height; x++) {
					var ch = Map[x,y].Character;
					if( ch != null ){
						sb.AppendFormat ("{0} ", ch.Name[0]);
					}else{
						if (Map [x, y].Kind.Id == 0) {
							sb.AppendFormat (". ", Map [x, y].Kind.Id);
						} else {
							sb.AppendFormat ("{0} ", Map [x, y].Kind.Id);
						}
					}
				}
				sb.AppendLine ();
			}
			foreach( var ch in Map.CharacterList ){
				sb.AppendFormat( "{0:d2}:{1} {2} HP={3} ATK={4} DEF={5}\n", ch.Id, ch.Name, ch.Position, ch.Hp, ch.Attack, ch.Defense);
			}
			return sb.ToString ();
		}

		public void CmdMovePlayer(Point to){
			var oldPos = Player.Position;
			Map.MoveCharacter (Player, to);
			Player.Dir = (to - oldPos).ToDir ();
			Send("walk", Player, oldPos, Player.Position);			
			if (Map [to].Kind.Id == 19) {
				// ゴール
				Send("goal");
			}
		}

		public void CmdAttack(Direction dir){
			Player.Dir = dir;
			var ch = Map [Player.Position + dir].Character;
			if (Player.IsAttackableTo (ch)) {
				Player.AttackTo (ch);
			} else {
				Send ("suburi", Player);
			}
		}

		public bool CheckAttack(Direction dir){
			Player.Dir = dir;
			var ch = Map [Player.Position + dir].Character;
			return (Player.IsAttackableTo (ch));
		}
	}
}

