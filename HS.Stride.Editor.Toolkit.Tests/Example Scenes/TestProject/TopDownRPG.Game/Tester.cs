using Stride.Engine;
using Stride.Core;

namespace TopDownRPG
{
    public class Tester : SyncScript
    {
        [DataMember]
        public int health { get; set; } = 100;

        public override void Start()
        {
            // Initialization of the script.
        }

        public override void Update()
        {
            // Do stuff every new frame
        }
    }
}
