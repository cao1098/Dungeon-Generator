using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractDungeonGenerator : MonoBehaviour
{
    public void runGenerate() {
        generateDungeon();
    }

    protected abstract void generateDungeon();
}
