using DotsRogue;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuInputTest : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (InputUtility.ReadMenuInput(out int input))
            Debug.Log($"Menu input {input}");
    }
}
