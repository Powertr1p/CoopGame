using Mirror;
using UnityEngine;

namespace DefaultNamespace
{
    public class PickupItem : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnPickedUpByChanged))]
        private NetworkIdentity _pickedUpBy;
        
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        
        private void Start()
        {
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
        }
        
        private void OnPickedUpByChanged(NetworkIdentity oldPlayer, NetworkIdentity newPlayer)
        {
            if (newPlayer != null)
            {
                Transform hand = newPlayer.GetComponentInChildren<PlayerPickup>().HandPoint;
                if (hand != null)
                {
                    transform.SetParent(hand);
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                }
            }
            else
            {
                transform.SetParent(null);
                transform.position = _initialPosition;
                transform.rotation = _initialRotation;
            }
        }
        
        [Server]
        public void PickUp(NetworkIdentity player)
        {
            if (_pickedUpBy == null)
            {
                _pickedUpBy = player;
            }
        }

        [Server]
        public void Drop()
        {
            if (_pickedUpBy != null)
            {
                _pickedUpBy = null;
            }
        }
    }
}