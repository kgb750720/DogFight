using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DamageBase : MonoBehaviour 
{

	public GameObject Effect;
	public float LifeTimeEffect = 3;
	[HideInInspector]
    public GameObject Owner;
    public int Damage = 20;
	//public string[] TargetTag = new string[1]{"Enemy"};
	public HashSet<string> TargetTag = new HashSet<string>();


	protected HitInstance ei;        //子弹效果预制体池对象
	protected virtual void Awake()
	{
		TargetTag.Add("Enemy");
		TargetTag.Add("Interfere");
		ei = Effect.GetComponent<HitInstance>();
	}

	protected void InitTargetTag(string parentTag)
	{
		GameManager manager = GameObject.FindObjectOfType<GameManager>();
		if (manager.TargetTags.ContainsKey(parentTag))
		{
			string[] tags = manager.TargetTags[parentTag];
			TargetTag.Clear();
			foreach (var item in tags)
			{
				TargetTag.Add(item);
			}
		}
	}

}

public class WeaponBase : MonoBehaviour 
{
	[HideInInspector]
	public GameObject Owner;
	//[HideInInspector]
	public GameObject Target;
	public HashSet<string> TargetTag = new HashSet<string>();
	public bool RigidbodyProjectile;
	public Vector3 TorqueSpeedAxis;	
	public GameObject TorqueObject; //扭矩物体

    protected virtual void Awake()
    {
		TargetTag.Add("Enemy");
		TargetTag.Add("Interfere");
    }

	public void ResetTargetTag(string parentTag)
	{
		GameManager manager = GameManager.Manager;
		if (manager.TargetTags.ContainsKey(parentTag))
		{
			string[] tags = manager.TargetTags[parentTag];
			TargetTag.Clear();
			foreach (var item in tags)
			{
				TargetTag.Add(item);
			}
		}
	}
}

