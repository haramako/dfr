using UnityEngine;
using System.Collections;
using Rogue;

public class CharacterRenderer : MonoBehaviour {

	public string AtlasName;
	public Direction Dir;

	static Vector2 BasePos = new Vector2 (0, 0.85f);
	static int[] RotateTable = new int[]{0,1,2,3,4,3,2,1};
	static bool[] FlipTable = new bool[]{false,false,false,false,false,true,true,true};

	SpriteRenderer sprite;

	// Use this for initialization
	void Start () {
		var spriteObj = new GameObject ();
		sprite = spriteObj.AddComponent<SpriteRenderer> ();
		spriteObj.AddComponent<SimpleBillboard> ();
		spriteObj.transform.SetParent (this.transform, false);
		spriteObj.transform.localPosition = BasePos;
		if (AtlasName != null) redraw ();
	}

	void Update(){
		var anim = Mathf.FloorToInt (Time.time * 4) % 2 + 1;
		transform.localRotation *= Quaternion.Euler (0, 50 * Time.deltaTime, 0);
		var angles = transform.localRotation.eulerAngles;
		var rot = Mathf.FloorToInt (angles.y * 8f / 360f + 0.5f) % 8;
		var imgNum = RotateTable [rot];
		var spr = ResourceCache.LoadSync<Sprite> (AtlasName + "$move_" + imgNum + "_" + anim);
		sprite.sprite = spr;
		sprite.flipX = FlipTable [rot];
	}

	void redraw(){
		var layer = LayerMask.NameToLayer("MapObject");
		sprite.gameObject.layer = layer;
	}
}
