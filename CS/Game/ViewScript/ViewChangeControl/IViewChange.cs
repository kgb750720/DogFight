using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IViewChange
{
    abstract void Update();
    abstract void Started();
    abstract void Canceled();
    abstract void Close();
    abstract void Open();

}
