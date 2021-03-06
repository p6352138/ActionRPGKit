﻿using UnityEngine;
using System.Collections;

[RequireComponent (typeof (StatusC))]
[RequireComponent (typeof (StatusWindowC))]
[RequireComponent (typeof (HealthBarC))]
[RequireComponent (typeof (PlayerAnimationC))]
[RequireComponent (typeof (PlayerInputControllerC))]
[RequireComponent (typeof (CharacterMotorC))]
[RequireComponent (typeof (InventoryC))]
[RequireComponent (typeof (QuestStatC))]
[RequireComponent (typeof (SkillWindowC))]


public class AttackTriggerC : MonoBehaviour {
	
	//private bool  masterNetwork = false;
	public GameObject mainModel;
	public Transform attackPoint;
	public Transform attackPrefab;
	public enum whileAtk{
		MeleeFwd = 0,
		Immobile = 1,
		WalkFree = 2
	}
	public whileAtk whileAttack = whileAtk.MeleeFwd;
	public Transform upperBody;
	//private bool  mixingAnim = false;
	
	public Transform[] skillPrefab = new Transform[3];
	
	private bool  atkDelay = false;
	public bool  freeze = false;
	
	public Texture2D[] skillIcon = new Texture2D[3];
	public int skillIconSize = 80;
	
	public float attackSpeed = 0.15f;
	private float nextFire = 0.0f;
	public float atkDelay1 = 0.1f;
	public float skillDelay = 0.3f;
	
	public AnimationClip[] attackCombo = new AnimationClip[3];
	public float attackAnimationSpeed = 1.0f;
	public AnimationClip[] skillAnimation = new AnimationClip[3];
	public float skillAnimationSpeed = 1.0f;
	public int[] manaCost = new int[3];
	private AnimationClip hurt;
	
	
	private bool  meleefwd = false;
	private bool  isCasting = false;
	
	private int c = 0;
	private int conCombo = 0;
	
	public Transform Maincam;
	public GameObject MaincamPrefab;
	public GameObject attackPointPrefab;
	
	private int str = 0;
	private int matk = 0;
	
	public Texture2D aimIcon;
	public int aimIconSize = 40;
	
	private bool  flinch = false;
	private int skillEquip  = 0;
	private Vector3 knock = Vector3.zero;
	
	void  Awake (){
		if(!mainModel){
			mainModel = this.gameObject;
		}
		//Assign This mainModel to Status Script
		GetComponent<StatusC>().mainModel = mainModel;
		gameObject.tag = "Player";

		GameObject[] cam = GameObject.FindGameObjectsWithTag("MainCamera"); 
		foreach(GameObject cam2 in cam) { 
			if(cam2){
				Destroy(cam2.gameObject);
			}
		}
		GameObject newCam = GameObject.FindWithTag ("MainCamera");
		newCam = Instantiate(MaincamPrefab, transform.position , transform.rotation) as GameObject;
		Maincam = newCam.transform;
		Maincam.GetComponent<ARPGcameraC>().target = this.transform;

		str = GetComponent<StatusC>().addAtk;
		matk = GetComponent<StatusC>().addMatk;
		//Set All Attack Animation'sLayer to 15
		int animationSize = attackCombo.Length;
		int a = 0;
		if(animationSize > 0){
			while(a < animationSize && attackCombo[a]){
				mainModel.animation[attackCombo[a].name].layer = 15;
				a++;
			}
		}
		
		animationSize = skillAnimation.Length;
		a = 0;
		//Set All Skill Animation'sLayer to 16
		if(animationSize > 0){
			while(a < animationSize && skillAnimation[a]){
				mainModel.animation[skillAnimation[a].name].layer = 16;
				mainModel.animation[skillAnimation[a].name].speed = skillAnimationSpeed;
				a++;
			}
		}
		
		//--------------------------------
		//Spawn new Attack Point if you didn't assign it.
		if(!attackPoint){
			if(!attackPointPrefab){
				print("Please assign Attack Point");
				freeze = true;
				return;
			}
			GameObject newAtkPoint = Instantiate(attackPointPrefab, transform.position , transform.rotation) as GameObject;
			newAtkPoint.transform.parent = this.transform;
			attackPoint = newAtkPoint.transform;	
		}
		hurt = GetComponent<PlayerAnimationC>().hurt;
		GameObject minimap = GameObject.FindWithTag("Minimap");
			if(minimap){
				GameObject mapcam = minimap.GetComponent<MinimapOnOffC>().minimapCam;
				mapcam.GetComponent<MinimapCameraC>().target = this.transform;
			}
	}
	
	
	void  Update (){
		StatusC stat = GetComponent<StatusC>();
		if(freeze || atkDelay || Time.timeScale == 0.0f || stat.freeze){
			return;
		}
		CharacterController controller = GetComponent<CharacterController>();
		if (flinch){
			controller.Move(knock * 6* Time.deltaTime);
			return;
		}
		
		if (meleefwd){
			Vector3 lui = transform.TransformDirection(Vector3.forward);
			controller.Move(lui * 5 * Time.deltaTime);
		}
		attackPoint.transform.rotation = Maincam.GetComponent<ARPGcameraC>().aim;
		//----------------------------
		//Normal Trigger
		if (Input.GetButton("Fire1") && Time.time > nextFire && !isCasting) {
			if(Time.time > (nextFire + 0.5f)){
				c = 0;
			}
			//Attack Combo
			if(attackCombo.Length >= 1){
				conCombo++;
				//AttackCombo();
				StartCoroutine(AttackCombo());

			}
		}
		//Magic
		if (Input.GetButtonDown("Fire2") && Time.time > nextFire && !isCasting && skillPrefab[skillEquip] && !stat.silence) {
			//MagicSkill(skillEquip);
			StartCoroutine(MagicSkill(skillEquip));
		}
		if(Input.GetKeyDown("1") && !isCasting && skillPrefab[0]){
			skillEquip = 0;
		}
		if(Input.GetKeyDown("2") && !isCasting && skillPrefab[1]){
			skillEquip = 1;
		}
		if(Input.GetKeyDown("3") && !isCasting && skillPrefab[2]){
			skillEquip = 2;
		}
		
	}
	//Hit Bear :P
	void  OnGUI (){
		GUI.DrawTexture ( new Rect(Screen.width/2 - 16,Screen.height/2 - 90,aimIconSize,aimIconSize), aimIcon);
		
		if(skillPrefab[skillEquip] && skillIcon[skillEquip]){
			GUI.DrawTexture ( new Rect(Screen.width -skillIconSize - 28,Screen.height - skillIconSize - 20,skillIconSize,skillIconSize), skillIcon[skillEquip]);
		}
		if(skillPrefab[0] && skillIcon[0]){
			GUI.DrawTexture ( new Rect(Screen.width -skillIconSize -50,Screen.height - skillIconSize -50,skillIconSize /2,skillIconSize /2), skillIcon[0]);
		}
		if(skillPrefab[1] && skillIcon[1]){
			GUI.DrawTexture ( new Rect(Screen.width -skillIconSize -10,Screen.height - skillIconSize -60,skillIconSize /2,skillIconSize /2), skillIcon[1]);
		}
		if(skillPrefab[2] && skillIcon[2]){
			GUI.DrawTexture ( new Rect(Screen.width -skillIconSize +30 ,Screen.height - skillIconSize -50,skillIconSize /2,skillIconSize /2), skillIcon[2]);
		}
	}


	IEnumerator  AttackCombo (){
		if (attackCombo [c]) {
			str = GetComponent<StatusC>().addAtk;
			matk = GetComponent<StatusC>().addMatk;
			Transform bulletShootout;
			isCasting = true;
			// If Melee Dash
			if(whileAttack == whileAtk.MeleeFwd){
				GetComponent<CharacterMotorC>().canControl = false;
				//MeleeDash();
				StartCoroutine(MeleeDash());
			}
			// If Immobile
			if(whileAttack == whileAtk.Immobile){
				GetComponent<CharacterMotorC>().canControl = false;
			}
			
			while(conCombo > 0){
				if(c >= 1){
					mainModel.animation.PlayQueued(attackCombo[c].name, QueueMode.PlayNow).speed = attackAnimationSpeed;
				}else{
					mainModel.animation.PlayQueued(attackCombo[c].name, QueueMode.PlayNow).speed = attackAnimationSpeed;
				}
				
				float wait = mainModel.animation[attackCombo[c].name].length;
				
				yield return new WaitForSeconds(atkDelay1);
				c++;
				
				nextFire = Time.time + attackSpeed;
				bulletShootout = Instantiate(attackPrefab, attackPoint.transform.position , attackPoint.transform.rotation) as Transform;
				bulletShootout.GetComponent<BulletStatusC>().Setting(str , matk , "Player" , this.gameObject);
				conCombo -= 1;
				
				if(c >= attackCombo.Length){
					c = 0;
					atkDelay = true;
					yield return new WaitForSeconds(wait);
					atkDelay = false;
				}else{
					yield return new WaitForSeconds(attackSpeed);
				}
				
			}
			
			isCasting = false;
			GetComponent<CharacterMotorC>().canControl = true;
		} else {
			print ("Please assign attack animation in Attack Combo");
		}

	}

	
	IEnumerator  MeleeDash (){
		meleefwd = true;
		yield return new WaitForSeconds(0.2f);
		meleefwd = false;
		
	}
	
	//---------------------
	//-------
	IEnumerator  MagicSkill ( int skillID  ){
		if(skillAnimation[skillID]){
			str = GetComponent<StatusC>().addAtk;
			matk = GetComponent<StatusC>().addMatk;
			
			if(GetComponent<StatusC>().mana > manaCost[skillID] && !GetComponent<StatusC>().silence){
				isCasting = true;
				GetComponent<CharacterMotorC>().canControl = false;
				mainModel.animation.Play(skillAnimation[skillID].name);
				
				nextFire = Time.time + skillDelay;
				Maincam.GetComponent<ARPGcameraC>().lockOn = true;
				//Transform bulletShootout;
				
				float wait = mainModel.animation[skillAnimation[skillID].name].length -0.3f;
				yield return new WaitForSeconds(wait);
				Maincam.GetComponent<ARPGcameraC>().lockOn = false;
				Transform bulletShootout = Instantiate(skillPrefab[skillID], attackPoint.transform.position , attackPoint.transform.rotation) as Transform;
				bulletShootout.GetComponent<BulletStatusC>().Setting(str , matk , "Player" , this.gameObject);
				yield return new WaitForSeconds(skillDelay);
				isCasting = false;
				GetComponent<CharacterMotorC>().canControl = true;
				GetComponent<StatusC>().mana -= manaCost[skillID];
			}


		}else{
			print("Please assign skill animation in Skill Animation");
		}

	}
	
	public void  Flinch ( Vector3 dir  ){
		knock = dir;
		GetComponent<CharacterMotorC>().canControl = false;
		//KnockBack();
		StartCoroutine(KnockBack());
		mainModel.animation.PlayQueued(hurt.name, QueueMode.PlayNow);
		GetComponent<CharacterMotorC>().canControl = true;
	}
	
	IEnumerator  KnockBack (){
		flinch = true;
		yield return new WaitForSeconds(0.2f);
		flinch = false;
	}

	public void WhileAttackSet(int watk){
		if (watk == 2) {
			whileAttack = whileAtk.WalkFree;
		} else if (watk == 1) {
			whileAttack = whileAtk.Immobile;
		} else {
			whileAttack = whileAtk.MeleeFwd;
		}
	}
	
			
}
