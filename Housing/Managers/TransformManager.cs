using UnityEngine;

namespace Housing.Managers
{
    public class TransformManager
    {
        public static GameObject MergeGoTransform(GameObject transform1, Transform transform2)
        {
            transform1.transform.position = transform2.position;
            transform1.transform.eulerAngles = transform2.eulerAngles;
            transform1.transform.localScale = transform2.localScale;

            return transform1;
        }
    }
}