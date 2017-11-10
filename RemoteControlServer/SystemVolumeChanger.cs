using CoreAudioApi;
using System;

namespace RemoteControlServer
{
    public static class SystemVolumeChanger
    {
        public static void SetVolume(int value)
        {
            if (value < 0)
                value = 0;

            if (value > 100)
                value = 100;

            try
            {
                var devEnum = new MMDeviceEnumerator();
                var device = devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);

                device.AudioEndpointVolume.MasterVolumeLevelScalar = value / 100.0f;
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public static int GetVolume()
        {
            var result = 100;
            try
            {
                var devEnum = new MMDeviceEnumerator();
                var device = devEnum.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia);
                result = (int) (device.AudioEndpointVolume.MasterVolumeLevelScalar * 100);
            }
            catch (Exception)
            {
                // ignored
            }

            return result;
        }
    }
}