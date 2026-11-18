using System;
using Oculus.Interaction;
using UnityEngine;

namespace Assets.Scripts
{
    public class CornBagThrowEnabler : MonoBehaviour
    {
        public event Action<CornBagThrowEnabler, Vector3, Vector3> Thrown;

        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Grabbable _grabbable;

        private void OnEnable()
        {
            _grabbable.VelocityThrow.WhenThrown += OnThrown;
        }

        private void OnThrown(Vector3 velocity, Vector3 torque)
        {
            _rigidbody.isKinematic = false;
            Thrown?.Invoke(this, velocity, torque);
        }

        public void SetCanThrow(bool canThrow)
        {
            _grabbable.enabled = canThrow;
        }

        private void OnDisable()
        {
            _grabbable.VelocityThrow.WhenThrown -= OnThrown;
        }
    }
}
