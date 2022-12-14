using UnityEngine;

namespace RLTY.Customisation
{
    public interface IVideoPlayer
    {
        /// <summary>
        /// Before using IVideoPlayer, make sure to set this value to a MonoBehaviour that implements the IVideoPlayer interface
        /// </summary>
        static MonoBehaviour Implementation;

        float Volume { get; set; }
        int GetVideoWidth();
        int GetVideoHeight();

        
    }

    public interface ISessionsConfig
    {
        static MonoBehaviour Implementation;


    }
}