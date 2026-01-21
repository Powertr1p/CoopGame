using Mirror;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerInteract : NetworkBehaviour
    {
        [SerializeField] private PlayerRaycaster _raycaster;
        [SerializeField] private Camera _camera;
        
        private LayerMask _interactMask;
        private Door _door;
        private bool _isInteracting;

        private void Start()
        {
            _interactMask = LayerMask.GetMask("Interact");
        }

        private void Update()
        {
            if (!isLocalPlayer) return;
            
            if (Input.GetMouseButtonDown(0))
            {
                if (_raycaster.GetRaycast(_interactMask, out Collider coll))
                {
                    if (coll.TryGetComponent(out Door door))
                    {
                        _isInteracting = true;
                        door.Bind(transform.position);
                        _door = door;
                    }
                }
            }
            
            if (Input.GetMouseButtonUp(0))
            {
                _door?.Unbind();
                _isInteracting = false;
            }

            if (_isInteracting && !ReferenceEquals(_door, null))
            {
                if (Vector3.Distance(transform.position, _door.transform.position) > 2f)
                {
                    _door.Unbind();
                    return;
                }
                
                float mouseX = Input.GetAxis("Mouse X");
                _door.Move(mouseX, transform.position);
            }
        }
    }
}