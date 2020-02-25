using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace RLTKTutorial.Part1_5A
{
    public class MonsterCounter : MonoBehaviour
    {
        [SerializeField]
        Text _counterText = null;


        // Update is called once per frame
        void Update()
        {
            _counterText.text = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<ChangeMonsterCountSystem>().MonsterCount.ToString();
        }
    }

}