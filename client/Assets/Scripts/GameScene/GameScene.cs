using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;
using RSG;
using Rogue;

public class GameScene : MonoBehaviour {

	public Terrain Terrain;
	public MeshRenderer TerrainGridMesh;
	public GameObject CameraTarget;
	public Camera MainCamera;
	public CharacterRenderer CharacterBase;

    public Game Game { get; private set; }

	void Start(){

        Game = new Game();
        Game.StartThread();

		var atlas = new int[]{ 1, 2, 3, 168, 174, 210, 211 };

		for (int i = 0; i < 30; i++) {
			var cObj = Instantiate (CharacterBase);
			var x = Random.Range (0, 20);
			var z = Random.Range (0, 20);
			var pos = new Vector3 (x + 0.5f, 0, z + 0.5f);
			pos.y = GetTerrainHeight (pos);
			cObj.transform.position = pos;
			cObj.AtlasName = string.Format ("Enemy{0:0000}", atlas [Random.Range (0, atlas.Length - 1)]);
		}
	}

	void Update(){
		Application.targetFrameRate = 60;
        Rogue.Message mes;
        if( Game.SendQueue.TryDequeue(out mes))
        {
            Debug.Log(mes);
            Game.RecvQueue.Enqueue(new Message("hoge", 1, 2));
        }
	}

	void MoveCameraTo(Vector3 pos){
		CameraTarget.transform.localPosition = pos;
	}

	public void OnBeginDrag(PointerEventData ev){
		//Debug.Log("Begin Drag");
	}

	public void OnEndDrag(PointerEventData ev){
		//Debug.Log("End Drag");
	}

	public void OnDrag(PointerEventData ev){
		var delta = new Vector3 (ev.delta.x / Screen.width, 0, ev.delta.y / Screen.height);
		var rot = Quaternion.Euler (0, 45, 0);
		delta = rot * delta;
		MoveCameraTo (CameraTarget.transform.localPosition - delta * 10f);
	}

	public void OnPointerClick (PointerEventData ev){
		Vector2 hit;
		if (LaycastByScreenPos (ev.position, out hit)) {
			Debug.Log (hit);
		}
	}
		
	// 高さを取得する
	public float GetTerrainHeight(Vector3 pos){
		var data = Terrain.terrainData;
		return data.GetInterpolatedHeight (pos.x / data.size.x, pos.z / data.size.z);
	}

	// スクリーン座標から、マス目の位置を取得する
	public bool LaycastByScreenPos(Vector2 screenPos, out Vector2 hitPos){
		RaycastHit hit;
		if (!Physics.Raycast (MainCamera.ScreenPointToRay (screenPos), out hit, 1000, 1 << LayerMask.NameToLayer("MapGrid"))) {
			hitPos = new Vector2 ();
			return false;
		} else {
			hitPos = new Vector2 (hit.point.x, hit.point.z);
			return true;
		}
	}

}

