using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class BaseVolumeComponent : VolumeComponent,IPostProcessComponent
{
    public bool IsTileCompatible()
    {
        return false;
    }

    public bool IsActive()
    {
        return true;
    }
}