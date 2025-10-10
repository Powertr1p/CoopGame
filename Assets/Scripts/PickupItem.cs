using Mirror;
using UnityEngine;

namespace DefaultNamespace
{
    public class PickupItem : NetworkBehaviour
    {
        [SyncVar(hook = nameof(OnPickedUpByChanged))]
        private NetworkIdentity _pickedUpBy;

        private Rigidbody _rigidbody;
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        
        private void Start()
        {
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;

            _rigidbody = GetComponent<Rigidbody>();
        }
        
        private void OnPickedUpByChanged(NetworkIdentity oldPlayer, NetworkIdentity newPlayer)
        {
            gameObject.SetActive(false);
            
            if (newPlayer != null)
            {
                Transform hand = newPlayer.GetComponentInChildren<PlayerPickup>().HandPoint;
                if (hand != null)
                {
                    if (_rigidbody != null)
                    {
                        _rigidbody.isKinematic = true;
                    }
                    
                    transform.SetParent(hand);
                    transform.localPosition = Vector3.zero;
                    transform.localRotation = Quaternion.identity;
                }
                
                gameObject.SetActive(true);
                Debug.Log("Picked up...");
            }
            else
            {
                transform.SetParent(null);
                gameObject.SetActive(true);

                if (_rigidbody != null)
                {
                    _rigidbody.isKinematic = false;
                    _rigidbody.AddForce(transform.forward.normalized * 5f, ForceMode.Impulse);
                }

                // transform.position = _initialPosition;
                // transform.rotation = _initialRotation;
                
                Debug.Log("Dropped...");
            }
            
            // gameObject.SetActive(true);
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
            _pickedUpBy = null;
        }
    }
}