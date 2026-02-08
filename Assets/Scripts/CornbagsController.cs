using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class CornbagsController : MonoBehaviour
    {
        public event Action<Cornbag> Thrown;

        [Serializable]
        private class CornbagWithInitializationDetails 
        {
            public GameObject CornbagPrefab;
            public Vector3 InitialPosition;
            public Quaternion InitialRotation;
        }

        [SerializeField] private List<CornbagWithInitializationDetails> _cornbagInitializationDetails;
        private IList<Cornbag> _thrownCornbags = new List<Cornbag>();

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

            Debug.Log($"Subscribed to {gameObject.name}");
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
        }

        private void OnThrown(Cornbag cornbag)
        {
            ReSpawn(cornbag);
            Thrown?.Invoke(cornbag);

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
        }

        private void ReSpawn(Cornbag cornbag)
        {
            var cornbagInitialization = _cornbagInitializationDetails[cornbag.Index - 1];
            var newCornbag = Instantiate(cornbagInitialization.CornbagPrefab, transform);
            newCornbag.transform.localPosition = cornbagInitialization.InitialPosition;
            newCornbag.transform.localRotation= cornbagInitialization.InitialRotation;
            Subscribe(newCornbag);
        }
    }
}
