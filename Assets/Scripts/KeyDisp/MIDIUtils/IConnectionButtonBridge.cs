using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IConnectionButtonBridge
{
    Color UnselectedButtonColor {get; }
    Color SelectedButtonColor {get; }
    void SetLayoutDirty();
}
