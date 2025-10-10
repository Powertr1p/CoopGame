using Mirror;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerPickup : NetworkBehaviour
    {
        [SerializeField] private Transform _handPoint;
        
        public Transform HandPoint => _handPoint;
        
        private GameObject _heldObject;
        LayerMask _pickupMask;

        private void Start()
        {
            _pickupMask = LayerMask.GetMask("Pickup");
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            
            Vector3 origin = Camera.main.transform.position;
            Vector3 direction = Camera.main.transform.forward;
            float maxDistance = 4f;
            
            Debug.DrawRay(origin, direction * maxDistance, Color.red);

            if (Input.GetKeyDown(KeyCode.E) && _handPoint != null)
            {
                if (_heldObject == null)
                {
                    if (Physics.Raycast(origin, direction, out var hit, maxDistance, _pickupMask))
                    {
                        if (hit.collider.TryGetComponent(out PickupItem pickupItem))
                        {
                            CmdPickupItem(pickupItem.netIdentity);
                            Debug.Log(pickupItem.name);
                        }
                    }
                }
                else
                {
                    if (_heldObject.TryGetComponent(out NetworkIdentity itemId))
                    {
                        CmdDropItem(itemId);
                    }
                }
            }
        }

        [Command]
        public void CmdPickupItem(NetworkIdentity itemId)
        {
            PickupItem item = itemId.GetComponent<PickupItem>();
            if (item == null) return;
            
            item.PickUp(netIdentity);
            _heldObject = item.gameObject;
        }

        [Command]
        public void CmdDropItem(NetworkIdentity itemId)
        {
            PickupItem item = itemId.GetComponent<PickupItem>();
            if (item == null) return;
            
            item.Drop();
            _heldObject = null;
        }
    }
}