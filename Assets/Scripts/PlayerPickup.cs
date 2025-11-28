using Mirror;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerPickup : NetworkBehaviour
    {
        [SerializeField] private Transform _handPoint;
        [SerializeField] private PlayerRaycaster _raycaster;
        
        public Transform HandPoint => _handPoint;
        
        [SyncVar(hook = nameof(OnHeldObjectChanged))] private NetworkIdentity _syncHeldObject;
        private GameObject _heldObject;
        private LayerMask _pickupMask;
        

        private void OnHeldObjectChanged(NetworkIdentity oldItem, NetworkIdentity newItem)
        {
            _heldObject = newItem?.gameObject;
        }
        
        private void Start()
        {
            _pickupMask = LayerMask.GetMask("Pickup");
            _heldObject = _syncHeldObject?.gameObject;
        }

        private void Update()
        {
            if (!isLocalPlayer) return;

            if (Input.GetKeyDown(KeyCode.E) && _handPoint != null)
            {
                if (_heldObject == null)
                {
                    if (_raycaster.GetRaycast(_pickupMask, out Collider coll))
                    {
                        if (coll.TryGetComponent(out PickupItem pickupItem))
                        {
                            if (pickupItem.PickedUpBy != null) 
                            {
                                Debug.Log("Drop your cube now! >:(");
                                pickupItem.Drop();
                                return;
                            }
                            
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
            _syncHeldObject = itemId;
        }

        [Command]
        public void CmdDropItem(NetworkIdentity itemId)
        {
            PickupItem item = itemId.GetComponent<PickupItem>();
            if (item == null) return;
            
            item.Drop();
            _syncHeldObject = null;
        }
    }
}