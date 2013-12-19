using UnityEngine;
using System.Collections.Generic;
using System;

public abstract class PooledGameObject<P> : MonoBehaviour
{
	[HideInInspector]
	public Transform trans;
	[HideInInspector]
	public GameObject gameObj;

	public virtual void Awake()
	{
		trans = transform;
		gameObj = gameObject;
	}

	public abstract void Activate(P param);
}

public class GameObjectManager : MonoBehaviour 
{
	[Serializable]
	public class GameObjectPool<T, P> : Queue<T> where T : PooledGameObject<P> , new()
	{
		public GameObject blankPrefab;
		public GameObject container;
		[NonSerialized]
		public List<T> All = new List<T>();
		public int Preallocation = 5;
		public int UponEmptySpawn = 1;
		private bool started = false;

		public void Start ()
		{
			if(!started)
			{
				for(int i = 0; i < Preallocation; i++)
				{
					Enqueue(CreateNew());
				}
				started = true;
			}
		}

		private T CreateNew()
		{
			GameObject go = (GameObject)Instantiate (blankPrefab);
			T newT = go.GetComponent<T>();
			newT.gameObj = go;
			go.SetActive (false);
			go.transform.parent = container.transform;
			All.Add (newT);
			return newT;
		}

		private T CustomDequeue()
		{
			if(Count == 0)
			{
				for(int i = 0; i < UponEmptySpawn; i++)
				{
					Enqueue(CreateNew());
				}
			}
			return Dequeue();
		}

		public T Get(P param)
		{
			T newT = CustomDequeue();
			newT.Activate(param);
			return newT;
		}

		public T Spawn(Vector3 pos, P param)
		{
			return Spawn (pos, Quaternion.identity, param);
		}

		public T Spawn(Vector3 pos, Quaternion rotation, P param)
		{
			T newT = Get(param);
			newT.trans.position = pos;
			newT.trans.rotation = rotation;
			newT.gameObj.SetActive (true);
			return newT;
		}

		public void Return(T t)
		{
			t.gameObj.SetActive (false);
			Enqueue(t);
		}
	}

	[Serializable]
	public class BulletPool : GameObjectPool<Bullet, BulletTag> { }
	[Serializable]
	public class PickupPool : GameObjectPool<Pickup, PickupType> { }
	[Serializable]
	public class PlayerShotPool : GameObjectPool<PlayerShot, bool> { }
	[Serializable]
	public class ScorePopupPool : GameObjectPool<ScorePopup, ScorePopup.Params> { }

	public BulletPool bullets;
	public PickupPool pickups;
	public PlayerShotPool playerShots;
	public ScorePopupPool scorePopups;
	
	public static BulletPool Bullets
	{
		get { return manager.bullets; }
	}
	
	public static PickupPool Pickups
	{
		get { return manager.pickups; }
	}

	public static PlayerShotPool PlayerShots
	{
		get { return manager.playerShots; }
	}

	public static ScorePopupPool ScorePopups
	{
		get { return manager.scorePopups; }
	}

	public static GameObjectManager manager;

	void Awake()
	{
		bullets.Start ();
		pickups.Start ();
		playerShots.Start ();
		scorePopups.Start ();
		manager = this;
	}
}
