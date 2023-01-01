using System.Collections.Generic;

namespace Housing.Types
{
    public class Model
    {
        public string Name;
        public float[] Position;
        public float[] Rotation;
        public float[] Scale;
        public List<ModelObject> Objects;
    }
}