using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class CornbagsLifecycleController : MonoBehaviour
    {
        [Serializable]
        private class CornbagWithInitializationDetails 
        {
            public GameObject CornbagPrefab;
            public Vector3 InitialPosition;
            public Quaternion InitialRotation;
        }

        [SerializeField] private List<CornbagWithInitializationDetails> _cornbagInitializationDetails;

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

            cornbag.HitBoard += ReSpawn;
            cornbag.HitHole += ReSpawn;
            cornbag.HitFloor += ReSpawn;

            Debug.Log($"Subscribed to {gameObject.name}");
        }

        private void OnDisable()
        {
            foreach (GameObject child in transform)
            {
                Unsubscribe(child);
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

            cornbag.HitBoard -= ReSpawn;
            cornbag.HitHole -= ReSpawn;
            cornbag.HitFloor -= ReSpawn;
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
