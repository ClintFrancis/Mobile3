using System;
namespace Camera2Basic
{
    public enum CameraState
    {
        Preview,
        WaitingLock,
        WaitingPrecapture,
        WaitingNonPrecapture,
        PictureTaken
    }
}
