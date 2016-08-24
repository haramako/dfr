using UnityEngine;
using System.Collections;
using Rogue;

public class CharacterRenderer : MonoBehaviour {

	public string AtlasName;
	public Direction Dir;

	static Vector2 BasePos = new Vector2 (0, 0.85f);
	static int[] RotateTable = new int[]{0,4,3,2,1,0,1,2,3};
    static bool[] FlipTable = new bool[] { false, false, true, true, true, false, false, false, false };

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
        /*
		var anim = Mathf.FloorToInt (Time.time * 4) % 2 + 1;
		transform.localRotation *= Quaternion.Euler (0, 50 * Time.deltaTime, 0);
		var angles = transform.localRotation.eulerAngles;
		var rot = Mathf.FloorToInt (angles.y * 8f / 360f + 0.5f) % 8;
        */
        var anim = Mathf.FloorToInt(Time.time * 4) % 2 + 1;
        var imgNum = RotateTable [(int)Dir.Rotate(5)];
		var spr = ResourceCache.LoadSync<Sprite> (AtlasName + "$move_" + imgNum + "_" + anim);
		sprite.sprite = spr;
		sprite.flipX = FlipTable [(int)Dir.Rotate(5)];
	}

	void redraw(){
		var layer = LayerMask.NameToLayer("MapObject");
		sprite.gameObject.layer = layer;
	}
}
