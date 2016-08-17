using System;
using System.Linq;
using UnityEngine;

namespace Rogue
{
	public enum CharacterType {
		Player,
		Enemy
	}

	public abstract class Character {
		public Game Game { get; protected set; }
		public int Id;
		public string Name;
		public CharacterType Type;
		public int Hp;
		public int MaxHp;
		public int Attack = 10;
		public int Defense;
		public Point Position;
		public Direction Dir = Direction.South;
		public bool IsDead { get { return Hp <= 0; } }

		public Character(Game game){
			Game = game;
		}

		public virtual void DoAttack(){}
		public virtual void DoMove(){}
		public virtual void DoTurnEnd(){}
			
		public virtual void AttackTo(Character target){
			int damage = Attack+UnityEngine.Random.Range(0,5);
			if (target != null) {
				Dir = (target.Position - Position).ToDir ();
				Game.Send ("message", string.Format ("{0}の攻撃！\n{1}に{2}のダメージを与えた", Name, target.Name, damage));
				Game.Send ("attack", this, Dir, damage, Math.Max (target.Hp - damage, 0));
				Game.Send ("damaged", target, damage, Math.Max (target.Hp - damage, 0));
				target.AddDamage (damage);
			} else {
				Game.Send("attack", this, Dir, damage);
			}
		}

		public bool AddDamage(int damage){
			Hp -= damage;
			bool dead = false;
			if( Hp <= 0 ){
				Hp = 0;
				dead = true;
				if (Type == CharacterType.Enemy) {
					Game.Map.RemoveCharacter (this);
					Game.Send ("message", string.Format ("{0}を倒した", Name));
					Game.Send ("dead", this);
				} else {
					Game.Send ("message", string.Format ("{0}は力尽きた・・・", Name));
					Game.Send ("gameover", this);
					throw new GameOverException ();
				}
			}
			return dead;
		}

		public bool IsAttackableTo(Character target){
			return target != null && Game.Map.StepFlyable() (Position, target.Position) <= 1;
		}

		public override string ToString(){
			return Name;
		}
	}

	public class Player : Character {
		public int Turn = 100;
		public Player(Game game):base(game){
			Type = CharacterType.Player;
		}
		public override void DoAttack(){
		}
		public override void DoTurnEnd(){
			if (Turn > 0) {
				Turn -= 1;
				Hp += 1;
				if (Hp > MaxHp) {
					Hp = MaxHp;
				}
			} else {
				Hp -= 1;
				if (Hp < 1)
					Hp = 1;
			}
			Game.Send ("turn_down", Turn, Hp);
		}
	}

	public class Enemy : Character {
		public Enemy(Game game):base(game){
			Type = CharacterType.Enemy;
		}

		public override void DoAttack(){
			foreach( var p in Game.PathFinder.FindAround(Position, 1, false, Game.Map.FloorIsFlyable) ){
				var ch = Game.Map [p].Character;
				if (ch != null && ch.Type == CharacterType.Player) {
					if (IsAttackableTo (ch)) {
						AttackTo (ch);
					}
				}
			}
		}

		public override void DoMove(){
			Game.Map [Game.Player.Position].Character = null; // 一時的に退いてもらう
			var path = Game.PathFinder.FindPath(Position, Game.Player.Position, 10, Game.Map.StepWalkableNow());
			Game.Map [Game.Player.Position].Character = Game.Player;
			//if( path != null ) Game.log (string.Join (">", path));
			//Debug.Log(""+ this + " "+ path.Count());
			if( path != null && path.Count() > 0){
				if (Game.Map.FloorIsWalkableNow (path [0])) {
					var oldPos = Position;
					Game.Map.MoveCharacter (this, path [0]);
					Game.Send("walk", this, oldPos, path[0]);			
				}
			}else{
			}
		}

	}

}

