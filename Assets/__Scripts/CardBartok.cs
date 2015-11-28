using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum CBState{
	drawpile,
	toHand,
	hand,
	toTarget,
	target,
	discard,
	to,
	idle
}

public class CardBartok : Card {
	static public float MOVE_DURATION = 0.5f;
	static public string MOVE_EASING = Easing.InOut;
	static public float CARD_HEIGHT = 3.5f;
	static public float CARD_WIDTH = 2f;

	public CBState state = CBState.drawpile;

	public List<Vector3> bezierPts;
	public List<Quaternion> bezierRots;
	public float timeStart, timeDuration;

	public int eventualSortOrder;
	public string eventualSortLayer;

	public GameObject reportFinishTo = null;
	public Player callbackPlayer = null;

	public void MoveTo(Vector3 ePos, Quaternion eRot){
		bezierPts = new List<Vector3> ();
		bezierPts.Add (transform.localPosition);
		bezierPts.Add (ePos);
		bezierRots = new List<Quaternion> ();
		bezierRots.Add (transform.rotation);
		bezierRots.Add (eRot);

		if (timeStart == 0) {
			timeStart = Time.time;
		}

		timeDuration = MOVE_DURATION;

		state = CBState.to;
	}

	public void MoveTo(Vector3 ePos){
		MoveTo (ePos, Quaternion.identity);
	}

	void Awake(){
		callbackPlayer = null;
	}

	void Update(){
		switch (state) {
		case CBState.toHand:
		case CBState.toTarget:
		case CBState.to:
			float u = (Time.time - timeStart)/timeDuration;

			float uC = Easing.Ease(u,MOVE_EASING);

			if(u<0){
				transform.localPosition = bezierPts[0];
				transform.rotation = bezierRots[0];
			}else if (u>=1){
				uC = 1;

				if(state == CBState.toHand) state=CBState.hand;
				if(state == CBState.toTarget) state=CBState.toTarget;
				if(state == CBState.to) state=CBState.idle;

				transform.localPosition = bezierPts[bezierPts.Count -1];
				transform.rotation = bezierRots[bezierPts.Count -1];

				timeStart = 0;

				if(reportFinishTo != null){
					reportFinishTo.SendMessage("CBCallback",this);

					reportFinishTo = null;
				}else if(callbackPlayer != null){
					callbackPlayer.CBCallback(this);
					callbackPlayer = null;
				}else{
				}
			}else{
				Vector3 pos = Utils.Bezier(uC,bezierPts);
				transform.localPosition = pos;
				Quaternion rotQ = Utils.Bezier(uC, bezierRots);
				transform.rotation = rotQ;

				if(u>0.5 && spriteRenderers[0].sortingOrder != eventualSortOrder){
					SetSortOrder(eventualSortOrder);
				}
				if(u>0.75 && spriteRenderers[0].sortingLayerName != eventualSortLayer){
					SetSortingLayerName(eventualSortLayer);
				}
			}
			break;

		}
	}

	override public void OnMouseUpAsButton(){
		Bartok.S.CardClicked (this);
		base.OnMouseUpAsButton();
	}
}
