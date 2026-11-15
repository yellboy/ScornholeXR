using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace Assets.Scripts
{
    public class CornbagsController : MonoBehaviour
    {
        private const float AuthorityRequestInterval = 0.5f;

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
        private readonly List<NetworkCornbag> _networkCornbags = new List<NetworkCornbag>();
        private readonly List<NetworkCornbag> _pendingDespawn = new List<NetworkCornbag>();
        private NetworkRunner _runner;
        private bool _networkingStarted;
        private float _nextAuthorityRequestTime;
        private bool _throwingEnabled;

        public bool ThrowingEnabled => _throwingEnabled;

        public void SetThrowingEnabled(bool throwingEnabled)
        {
            _throwingEnabled = throwingEnabled;

            foreach (Transform child in transform)
            {
                SetThrowingEnabled(child.gameObject);
            }

            foreach (var networkCornbag in _networkCornbags)
            {
                if (throwingEnabled)
                {
                    // The player whose turn it is simulates every cornbag so they collide consistently.
                    RequestStateAuthority(networkCornbag);
                }

                networkCornbag.RefreshAuthorityState();
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

            if (_networkingStarted && _runner.IsSharedModeMasterClient)
            {
                foreach (var networkCornbag in _networkCornbags)
                {
                    if (networkCornbag.IsThrown && !_pendingDespawn.Contains(networkCornbag))
                    {
                        _pendingDespawn.Add(networkCornbag);
                    }
                }

                _nextAuthorityRequestTime = 0f;
            }
        }

        public void StartNetworkedSession(NetworkRunner runner)
        {
            if (_networkingStarted)
            {
                return;
            }

            _runner = runner;
            _networkingStarted = true;

            // The scene's local cornbags are replaced by networked ones.
            foreach (Transform child in transform)
            {
                Unsubscribe(child.gameObject);
                Destroy(child.gameObject);
            }

            _thrownCornbags.Clear();

            if (_runner.IsSharedModeMasterClient)
            {
                for (var index = 1; index <= _cornbagInitializationDetails.Count; index++)
                {
                    SpawnNetworkCornbag(index);
                }
            }
        }

        public void RegisterNetworkCornbag(NetworkCornbag networkCornbag)
        {
            if (_networkCornbags.Contains(networkCornbag))
            {
                return;
            }

            _networkCornbags.Add(networkCornbag);
            networkCornbag.Cornbag.Thrown += OnThrown;

            if (_throwingEnabled)
            {
                RequestStateAuthority(networkCornbag);
            }
        }

        public void UnregisterNetworkCornbag(NetworkCornbag networkCornbag)
        {
            _networkCornbags.Remove(networkCornbag);
            _pendingDespawn.Remove(networkCornbag);

            if (networkCornbag.Cornbag != null)
            {
                networkCornbag.Cornbag.Thrown -= OnThrown;
            }
        }

        private void Update()
        {
            if (_pendingDespawn.Count == 0)
            {
                return;
            }

            var requestAuthority = Time.unscaledTime >= _nextAuthorityRequestTime;
            if (requestAuthority)
            {
                _nextAuthorityRequestTime = Time.unscaledTime + AuthorityRequestInterval;
            }

            // Only the state authority can despawn, so the master takes over the cornbags it removes.
            for (var i = _pendingDespawn.Count - 1; i >= 0; i--)
            {
                var networkCornbag = _pendingDespawn[i];
                if (networkCornbag == null || networkCornbag.Object == null || !networkCornbag.Object.IsValid)
                {
                    _pendingDespawn.RemoveAt(i);
                }
                else if (networkCornbag.Object.HasStateAuthority)
                {
                    _pendingDespawn.RemoveAt(i);
                    _runner.Despawn(networkCornbag.Object);
                }
                else if (requestAuthority)
                {
                    RequestStateAuthority(networkCornbag);
                }
            }
        }

        private static void RequestStateAuthority(NetworkCornbag networkCornbag)
        {
            var networkObject = networkCornbag.Object;
            if (networkObject != null && networkObject.IsValid && !networkObject.HasStateAuthority)
            {
                networkObject.RequestStateAuthority();
            }
        }

        private void SpawnNetworkCornbag(int cornbagIndex)
        {
            var initialization = _cornbagInitializationDetails[cornbagIndex - 1];
            var localRotation = initialization.InitialRotation;
            var isValidRotation = localRotation.x != 0f || localRotation.y != 0f ||
                localRotation.z != 0f || localRotation.w != 0f;

            _runner.Spawn(
                initialization.CornbagPrefab,
                transform.TransformPoint(initialization.InitialPosition),
                transform.rotation * (isValidRotation ? localRotation : Quaternion.identity),
                _runner.LocalPlayer);
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
        }

        private void OnThrown(Cornbag cornbag)
        {
            var networkCornbag = cornbag.GetComponent<NetworkCornbag>();
            if (networkCornbag != null)
            {
                OnNetworkCornbagThrown(cornbag, networkCornbag);
                return;
            }

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

        private void OnNetworkCornbagThrown(Cornbag cornbag, NetworkCornbag networkCornbag)
        {
            networkCornbag.MarkResolved(cornbag.Result.Value);
            SpawnNetworkCornbag(cornbag.Index);

            Thrown?.Invoke(cornbag);

            if (cornbag.Result == ThrowResult.HoleHit)
            {
                // Despawn if it is in the hole, so that it doesn't block other cornbags from going in.
                _runner.Despawn(networkCornbag.Object);
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
