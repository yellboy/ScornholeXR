using Oculus.Interaction;
using UnityEngine;

namespace Assets.Scripts
{
    public class CornBagThrowEnabler : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Grabbable _grabbable;

        private void OnEnable()
        {
            _grabbable.VelocityThrow.WhenThrown += OnThrown;
        }

        private void OnThrown(Vector3 velocity, Vector3 torque)
        {
            _rigidbody.isKinematic = false;
        }

        private void OnDisable()
        {
            _grabbable.VelocityThrow.WhenThrown -= OnThrown;
        }
    }
}
