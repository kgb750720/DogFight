﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResManager : MonoBehaviour
{
    public static GameObject LoadPrefab(string path)
    {
        return Resources.Load<GameObject>(path);
    }
}
