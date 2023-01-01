namespace Housing.Types
{
    public class TransferPad
    {
        /*
         *  Transfer pads are used to move the player from one
         *  location to the next one. I had to make my own because AQ3D's
         *  native one only works server-side.
         */
        public int MapID;
        public int Destination;
        public float[] TPPosition;
        public float[] TPRotation;
        public float[] TPScale;
        public float[] NAPosition;
        public float[] NARotation;
        public float[] NAScale;
        public bool Loaded;
    }
}