using System;
using UnityEngine;

namespace Assets.Scripts
{
    public class Cornbag : MonoBehaviour
    {
        [SerializeField] private int _index;
        private bool _resolved;

        private bool _hitBoard;
        private bool _hitFloor;
        private bool _hitHole;

        public event Action<Cornbag> Thrown;

        public int Index => _index;

        public ThrowResult? Result { get; private set; }

        private void FixedUpdate()
        {
            if (_resolved)
            {
                return;
            }

            if (!(_hitHole || _hitBoard || _hitFloor))
            {
                return;
            }

            if (_hitHole)
            {
                Result = ThrowResult.HoleHit;
            }
            else if (_hitBoard)
            {
                Result = ThrowResult.BoardHit;
            }
            else if (_hitFloor)
            {
                Result = ThrowResult.Missed;
            }

            Thrown?.Invoke(this);

            _resolved = true;
        }

        private void OnCollisionEnter(Collision other)
        {
            Debug.Log($"Collided with {other.gameObject.name}");
            switch (other.gameObject.tag)
            {
                case "Board":
                    _hitBoard = true;
                    break;
                case "Floor":
                    _hitFloor = true;
                    break;
                case "Hole":
                    _hitHole = true;
                    break;
                case "Cornbag":
                    var otherCornbag = other.gameObject.GetComponent<Cornbag>();
                    switch (otherCornbag.Result)
                    {
                        case ThrowResult.BoardHit:
                            _hitHole = true;
                            break;
                        case ThrowResult.Missed:
                            _hitFloor = true;
                            break;
                        case ThrowResult.HoleHit:
                            _hitHole = true;
                            break;
                    }

                    break;
                default:
                    Debug.Log($"Collided with something else: {other.gameObject.tag}");
                    break;

            }
        }
    }
}
