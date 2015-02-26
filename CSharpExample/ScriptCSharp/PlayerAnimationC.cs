using UnityEngine;
using System.Collections;

public class PlayerAnimationC : MonoBehaviour {
	
	public float runMaxAnimationSpeed = 1.0f;
	public float backMaxAnimationSpeed = 1.0f;
	public float sprintAnimationSpeed = 1.5f;
	
	private GameObject player;
	private GameObject mainModel;
	
	//string idle = "idle";
	public AnimationClip idle;
	public AnimationClip run;
	public AnimationClip right;
	public AnimationClip left;
	public AnimationClip back;
	public AnimationClip jump;
	public AnimationClip hurt;
	
	void  Start (){
		if(!player){
			player = this.gameObject;
		}
		mainModel = GetComponent<AttackTriggerC>().mainModel;
		if(!mainModel){
			mainModel = this.gameObject;
		}
		
		mainModel.animation[run.name].speed = runMaxAnimationSpeed;
		mainModel.animation[right.name].speed = runMaxAnimationSpeed;
		mainModel.animation[left.name].speed = runMaxAnimationSpeed;
		mainModel.animation[back.name].speed = backMaxAnimationSpeed;
		
		mainModel.animation[jump.name].wrapMode  = WrapMode.ClampForever;
		
		if(hurt){
			mainModel.animation[hurt.name].layer = 5;
		}
		
	}
	
	void  Update (){
		CharacterController controller = player.GetComponent<CharacterController>();
		if ((controller.collisionFlags & CollisionFlags.Below) != 0){
			if (Input.GetAxis("Horizontal") > 0.1f)
				mainModel.animation.CrossFade(right.name);
			else if (Input.GetAxis("Horizontal") < -0.1f)
				mainModel.animation.CrossFade(left.name);
			else if (Input.GetAxis("Vertical") > 0.1f)
				mainModel.animation.CrossFade(run.name);
			else if (Input.GetAxis("Vertical") < -0.1f)
				mainModel.animation.CrossFade(back.name);
			else
				mainModel.animation.CrossFade(idle.name);
		}else{
			mainModel.animation.CrossFade(jump.name);
		}
	}
	
	public void  AnimationSpeedSet (){
		mainModel = GetComponent<AttackTriggerC>().mainModel;
		if(!mainModel){
			mainModel = this.gameObject;
		}
		mainModel.animation[run.name].speed = runMaxAnimationSpeed;
		mainModel.animation[right.name].speed = runMaxAnimationSpeed;
		mainModel.animation[left.name].speed = runMaxAnimationSpeed;
		mainModel.animation[back.name].speed = backMaxAnimationSpeed;
	}
}