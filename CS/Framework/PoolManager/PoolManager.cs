using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PoolManager
{
	//内存池集合<场景,<预制体路径,内存池>>
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
	/// 获取对象池，若没有就用prepareCreateProcessCallback创建一个
	/// </summary>
	/// <param name="scene">对象池的场景索引</param>
	/// <param name="poolInstance">对象池的具体对象</param>
	/// <param name="prepareCreateProcessCallback">备用在没用对应对象池时用来创建对象池的对象创建回调</param>
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
