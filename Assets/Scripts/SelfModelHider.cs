using Mirror;
using UnityEngine;

namespace DefaultNamespace
{
    public class SelfModelHider : NetworkBehaviour
    {
        [SerializeField] private GameObject _selfModel;
        
        public override void OnStartLocalPlayer()
        {
            SetLayerRecursively(_selfModel, LayerMask.NameToLayer("Self"));
        }
        
        void SetLayerRecursively(GameObject obj, int newLayer)
        {
            obj.layer = newLayer;
            foreach (Transform child in obj.transform)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}