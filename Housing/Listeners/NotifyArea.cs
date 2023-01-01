using UnityEngine;

namespace Housing.Listeners
{
    public class NotifyArea : MonoBehaviour
    {
        public string Message;
        
        public void OnTriggerEnter(Collider collider)
        {
            if (collider.GetComponent<EntityController>().Entity.isMe)
            {
                Notification.ShowText(Message);
            }
        }
    }
}