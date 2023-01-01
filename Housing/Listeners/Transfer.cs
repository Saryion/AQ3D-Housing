using UnityEngine;

namespace Housing.Listeners
{
    public class Transfer : MonoBehaviour
    {
        public int Destination;
        
        public void OnTriggerEnter(Collider collider)
        {
            if (collider.GetComponent<EntityController>().Entity.isMe)
            {
                Game.Instance.SendAreaJoinRequest(Destination, true);
            }
        }
    }
}