
using UnityEngine;
using System.Collections.Generic;

public class CharacterUpdater : MonoBehaviour
{
    List<Character> characters = new List<Character>();

    void FixedUpdate()
    {
        GetComponentsInChildren<Character>(characters);
        for (int i = 0; i<characters.Count; i++)
            characters[i].DoUpdate();
    }
}

