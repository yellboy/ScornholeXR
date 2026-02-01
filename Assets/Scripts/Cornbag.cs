using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class Cornbag : MonoBehaviour
    {
        [SerializeField] private int _index;

        public event Action<Cornbag> HitBoard;
        public event Action<Cornbag> HitFloor;
        public event Action<Cornbag> HitHole;

        public int Index => _index;

        private void OnCollisionEnter(Collision other)
        {
            Debug.Log($"Collided with {other.gameObject.name}");
            switch (other.gameObject.tag)
            {
                case "Board":
                    HitBoard?.Invoke(this);
                    break;
                case "Floor":
                    HitFloor?.Invoke(this);
                    break;
                case "Hole":
                    HitHole?.Invoke(this);
                    break;
                default:
                    Debug.Log($"Collided with something else: {other.gameObject.tag}");
                    break;

            }
        }
    }
}
