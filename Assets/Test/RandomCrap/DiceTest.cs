using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Sark.RNG.RandomExtensions;
using Random = Unity.Mathematics.Random;
using Unity.Collections;

public class DiceTest : MonoBehaviour
{
    List<int> ints = new List<int>(10);

    public int Iterations = 10000;
    public int NumDice = 3;
    public int Sides = 6;


    // Start is called before the first frame update
    void Start()
    {
        Random rand = new Random((uint)UnityEngine.Random.Range(1, int.MaxValue));
        var randRef = new NativeReference<Random>(Allocator.Temp);
        randRef.Value = rand;
        for(int i = 0; i < Iterations; ++i)
        {
            int val = randRef.RollDice(NumDice, Sides);
            while (val >= ints.Count)
                ints.Add(0);
            ints[val]++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnGUI()
    {
        for(int i = 0; i < ints.Count; ++i)
        {
            GUILayout.Label($"{i} : {ints[i]}");
        }
    }
}
