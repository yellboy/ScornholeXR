using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class CornbagsController : MonoBehaviour
    {
        public event Action<Cornbag> Thrown;
        public event Action<Cornbag, Vector3, Vector3> ThrowStarted;

        [Serializable]
        private class CornbagWithInitializationDetails 
        {
            public GameObject CornbagPrefab;
            public Vector3 InitialPosition;
            public Quaternion InitialRotation;
        }

        [SerializeField] private List<CornbagWithInitializationDetails> _cornbagInitializationDetails;
        private IList<Cornbag> _thrownCornbags = new List<Cornbag>();
        private bool _throwingEnabled;

        public void SetThrowingEnabled(bool throwingEnabled)
        {
            _throwingEnabled = throwingEnabled;

            foreach (Transform child in transform)
            {
                SetThrowingEnabled(child.gameObject);
            }
        }

        public void Reset()
        {
            foreach (var cornbag in _thrownCornbags)
            {
                if (cornbag != null)
                {
                    Destroy(cornbag.gameObject);
                }
            }

            _thrownCornbags.Clear();
        }

        private void OnEnable()
        {
            foreach (Transform child in transform)
            {
                Subscribe(child.gameObject);
            }
        }

        private void Subscribe(GameObject gameObject)
        {
            var cornbag = gameObject.GetComponent<Cornbag>();
            if (cornbag == null)
            {
                Debug.LogError($"GameObject {gameObject.name} missing Cornbag component!");
                return;
            }

            cornbag.Thrown += OnThrown;
            var throwEnabler = gameObject.GetComponent<CornBagThrowEnabler>();
            if (throwEnabler != null)
            {
                throwEnabler.Thrown += OnThrowStarted;
            }
            SetThrowingEnabled(gameObject);

            Debug.Log($"Subscribed to {gameObject.name}");
        }

        private void SetThrowingEnabled(GameObject gameObject)
        {
            var cornbag = gameObject.GetComponent<Cornbag>();
            var throwEnabler = gameObject.GetComponent<CornBagThrowEnabler>();
            if (cornbag != null && throwEnabler != null)
            {
                throwEnabler.SetCanThrow(_throwingEnabled && !cornbag.IsResolved);
            }
        }

        private void OnDisable()
        {
            foreach (Transform child in transform)
            {
                Unsubscribe(child.gameObject);
            }
        }

        private void Unsubscribe(GameObject child)
        {
            var cornbag = child.GetComponent<Cornbag>();
            if (cornbag == null)
            {
                Debug.LogError($"GameObject {child.name} missing Cornbag component!");
                return;
            }

            cornbag.Thrown -= OnThrown;
            var throwEnabler = child.GetComponent<CornBagThrowEnabler>();
            if (throwEnabler != null)
            {
                throwEnabler.Thrown -= OnThrowStarted;
            }
        }

        private void OnThrown(Cornbag cornbag)
        {
            ReSpawn(cornbag);

            if (cornbag.Result == ThrowResult.HoleHit)
            {
                // Destroy if it is in the hole, so that it doesn't block other cornbags from going in.
                Destroy(cornbag.gameObject);
            }
            else
            {
                // Add to the list of thrown cornbags, so that it can be destroyed when Reset is called.
                _thrownCornbags.Add(cornbag);
            }

            Thrown?.Invoke(cornbag);
        }

        private void OnThrowStarted(CornBagThrowEnabler throwEnabler, Vector3 velocity, Vector3 angularVelocity)
        {
            var cornbag = throwEnabler.GetComponent<Cornbag>();
            if (cornbag != null)
            {
                ThrowStarted?.Invoke(cornbag, velocity, angularVelocity);
            }
        }

        private void ReSpawn(Cornbag cornbag)
        {
            var cornbagInitialization = _cornbagInitializationDetails[cornbag.Index - 1];
            var newCornbag = Instantiate(cornbagInitialization.CornbagPrefab, transform);
            newCornbag.transform.localPosition = cornbagInitialization.InitialPosition;
            newCornbag.transform.localRotation= cornbagInitialization.InitialRotation;
            Subscribe(newCornbag);
        }

        public void SpawnRemoteThrow(
            int cornbagIndex,
            Vector3 position,
            Quaternion rotation,
            Vector3 velocity,
            Vector3 angularVelocity)
        {
            if (cornbagIndex < 1 || cornbagIndex > _cornbagInitializationDetails.Count)
            {
                Debug.LogError($"Cannot spawn remote cornbag with index {cornbagIndex}");
                return;
            }

            var initialization = _cornbagInitializationDetails[cornbagIndex - 1];
            var remoteBag = Instantiate(initialization.CornbagPrefab, position, rotation, transform);
            var cornbag = remoteBag.GetComponent<Cornbag>();
            var throwEnabler = remoteBag.GetComponent<CornBagThrowEnabler>();
            var rigidbody = remoteBag.GetComponent<Rigidbody>();

            if (cornbag != null)
            {
                cornbag.enabled = false;
                _thrownCornbags.Add(cornbag);
            }

            if (throwEnabler != null)
            {
                throwEnabler.SetCanThrow(false);
                throwEnabler.enabled = false;
            }

            if (rigidbody == null)
            {
                Debug.LogError($"Remote cornbag {cornbagIndex} is missing its Rigidbody");
                Destroy(remoteBag);
                return;
            }

            rigidbody.isKinematic = false;
            rigidbody.linearVelocity = velocity;
            rigidbody.angularVelocity = angularVelocity;
        }
    }
}
