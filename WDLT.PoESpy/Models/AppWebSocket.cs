using Stylet;

namespace WDLT.PoESpy.Models
{
    public class AppWebSocket : PropertyChangedBase
    {
        public bool IsOpen { get; set; }
        public string Name { get; }

        public AppWebSocket(string name)
        {
            Name = name;
        }
    }
}