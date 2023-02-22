using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PoolManager
{
	//�ڴ�ؼ���<����,<Ԥ����·��,�ڴ��>>
	private static Dictionary<Scene, Dictionary<string, Pool>> SceneObjetsPool = new Dictionary<Scene, Dictionary<string, Pool>>();
	public static Pool GetPool(Scene scene, string prefabPath)
	{
		if (!SceneObjetsPool.ContainsKey(scene))
			SceneObjetsPool[scene] = new Dictionary<string, Pool>();
		if (!SceneObjetsPool[scene].ContainsKey(prefabPath))
			SceneObjetsPool[scene][prefabPath] = null;
		return SceneObjetsPool[scene][prefabPath];
	}

	public static Pool GetPool(Scene scene, PoolInstanceBase poolInstance)
	{
		return GetPool(scene, poolInstance.PrefabPath);
	}

	/// <summary>
	/// ��ȡ����أ���û�о���prepareCreateProcessCallback����һ��
	/// </summary>
	/// <param name="scene">����صĳ�������</param>
	/// <param name="poolInstance">����صľ������</param>
	/// <param name="prepareCreateProcessCallback">������û�ö�Ӧ�����ʱ������������صĶ��󴴽��ص�</param>
	/// <returns></returns>
	public static Pool GetPool(Scene scene, PoolInstanceBase poolInstance, Func<PoolInstanceBase> prepareCreateProcessCallback)
	{
		Pool pool = GetPool(scene, poolInstance);
		return pool != null ? pool : CreatePool(scene, poolInstance, prepareCreateProcessCallback);
	}

	public static Pool CreatePool(Scene scene, string prefabPath, Func<PoolInstanceBase> CreateProcessCallback)
	{
		if (!SceneObjetsPool.ContainsKey(scene))
			SceneObjetsPool[scene] = new Dictionary<string, Pool>();
		SceneObjetsPool[scene][prefabPath] = new Pool(CreateProcessCallback);
		void sceneUnloadCallback(Scene unloadSecen)
		{
			if (SceneObjetsPool != null && SceneObjetsPool.ContainsKey(unloadSecen))
				SceneObjetsPool.Remove(unloadSecen);
			SceneManager.sceneUnloaded -= sceneUnloadCallback;
		};
		SceneManager.sceneUnloaded += sceneUnloadCallback;
		return SceneObjetsPool[scene][prefabPath];
	}

	public static Pool CreatePool(Scene scene, PoolInstanceBase poolInstance, Func<PoolInstanceBase> CreateProcessCallback)
	{
		return CreatePool(scene, poolInstance.PrefabPath, CreateProcessCallback);
	}
}
