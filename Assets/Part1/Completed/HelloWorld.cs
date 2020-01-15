
using RLTK.MonoBehaviours;
using UnityEngine;

namespace RLTKTutorial.Part1
{
    public class HelloWorld : MonoBehaviour
    {
        [SerializeField]
        SimpleConsoleProxy _console = null;

        private void Start()
        {
            _console.Print(5, 5, "Hello, world!");
        }
    }
}