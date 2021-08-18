using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace RotoscopeWindows
{
    public partial class AppConfig
    {
        [JsonProperty("mainImage")]
        public string MainImage { get; set; }

        [JsonProperty("touchPoints")]
        public TouchPoint[] TouchPoints { get; set; }

        [JsonProperty("totalCM")]
        public int TotalCM { get; set; }
        
        [JsonProperty("port")]
        public string Port { get; set; } 
        
        [JsonProperty("errorCM")]
        public int ErrorCM { get; set; }

        [JsonProperty("tSpeed")]
        public int TransitionSpeed { get; set; }
    }

    public partial class TouchPoint
    {
        [JsonProperty("position")]
        public Position Position { get; set; }

        [JsonProperty("fileH")]
        public string FileH { get; set; }
        
        [JsonProperty("fileE")]
        public string FileE { get; set; }
    }

    public partial class Position
    {
        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }
    }
}
