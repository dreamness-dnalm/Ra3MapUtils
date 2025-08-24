using Ra3MapParser.Core;

namespace Ra3MapUtils.Services.Impl.MapTransform;

public abstract class MapTransformCommand
{
    protected Ra3Map OriginMap { get; private set; }
    
    protected Ra3Map DestinationMap { get; private set; }

    public MapTransformCommand(Ra3Map originMap)
    {
        // TOOD clone
        this.OriginMap = originMap;
    }

    protected abstract void transform();

}