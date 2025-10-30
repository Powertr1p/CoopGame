using Mirror;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerPickup : NetworkBehaviour
    {
        private static readonly int IsHoldingItem = Animator.StringToHash("isHolding");
        
        [SerializeField] private Transform _handPoint;
        [SerializeField] private Animator animator;     // todo: move into separate character animator controller
        
        public Transform HandPoint => _handPoint;
        
        [SyncVar(hook = nameof(OnHeldObjectChanged))] private NetworkIdentity _syncHeldObject;
        private GameObject _heldObject;
        
        LayerMask _pickupMask;

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
                            if (pickupItem.PickedUpBy != null) 
                            {
                                Debug.Log("Drop your cube now! >:(");
                                pickupItem.Drop();
                                return;
                            }
                            
                            CmdPickupItem(pickupItem.netIdentity);
                            animator.SetBool(IsHoldingItem, true);
                            
                            Debug.Log(pickupItem.name);
                        }
                    }
                }
                else
                {
                    if (_heldObject.TryGetComponent(out NetworkIdentity itemId))
                    {
                        CmdDropItem(itemId);
                        animator.SetBool(IsHoldingItem, false);
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