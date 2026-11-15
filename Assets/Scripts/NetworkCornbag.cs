using Fusion;
using Fusion.Addons.Physics;
using UnityEngine;

namespace Assets.Scripts
{
    [RequireComponent(typeof(NetworkRigidbody3D))]
    public class NetworkCornbag : NetworkBehaviour, IStateAuthorityChanged
    {
        private const int Unresolved = 0;

        [Networked] public NetworkBool IsThrown { get; set; }
        [Networked] public int ResultCode { get; set; }

        private Cornbag _cornbag;
        private CornBagThrowEnabler _throwEnabler;
        private CornbagsController _controller;

        public Cornbag Cornbag => _cornbag;
        public bool IsResolved => ResultCode != Unresolved;

        private void Awake()
        {
            _cornbag = GetComponent<Cornbag>();
            _throwEnabler = GetComponent<CornBagThrowEnabler>();
        }

        public override void Spawned()
        {
            _controller = FindFirstObjectByType<CornbagsController>();
            _throwEnabler.Thrown += OnThrowStarted;
            _controller.RegisterNetworkCornbag(this);
            RefreshAuthorityState();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            if (_throwEnabler != null)
            {
                _throwEnabler.Thrown -= OnThrowStarted;
            }

            if (_controller != null)
            {
                _controller.UnregisterNetworkCornbag(this);
            }
        }

        public override void Render()
        {
            // Proxies learn the result from the thrower so they can score collisions against this bag later.
            if (IsResolved && !_cornbag.IsResolved)
            {
                _cornbag.ApplyNetworkResult((ThrowResult)(ResultCode - 1));
            }
        }

        public void StateAuthorityChanged()
        {
            RefreshAuthorityState();
        }

        public void MarkResolved(ThrowResult result)
        {
            if (Object.HasStateAuthority)
            {
                ResultCode = (int)result + 1;
            }

            RefreshAuthorityState();
        }

        public void RefreshAuthorityState()
        {
            if (Object == null || !Object.IsValid)
            {
                return;
            }

            var hasAuthority = Object.HasStateAuthority;

            // Only the player simulating the bag evaluates its collisions and may grab it.
            _cornbag.enabled = hasAuthority && !IsResolved;
            _throwEnabler.SetCanThrow(
                hasAuthority && !IsResolved && _controller != null && _controller.ThrowingEnabled);
        }

        private void OnThrowStarted(CornBagThrowEnabler throwEnabler, Vector3 velocity, Vector3 angularVelocity)
        {
            if (Object.HasStateAuthority)
            {
                IsThrown = true;
            }
        }
    }
}
