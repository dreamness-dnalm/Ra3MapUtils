using System.Linq;

namespace UtilLib.models
{
    public class MapInfoJsonModel
    {
        public double mapWidth { get; set; } = -1;
        public double mapHeight { get; set; } = -1;
        public double border { get; set; } = -1;
        public int playerCnt { get; set; } = -1;
        private MapInfoJsonPosModel[] _pos = new MapInfoJsonPosModel[0];
        public MapInfoJsonPosModel[] pos
        {
            get => _pos;
            set
            {
                _pos = value;
                playerCnt = _pos
                    .Select(p => p.name)
                    .Count(n => n.StartsWith("Player_") && n.EndsWith("_Start"));
            }
        }
    }
}