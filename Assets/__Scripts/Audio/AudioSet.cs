using UnityEngine;
using SilentKnight.Utility;

namespace SilentKnight.Audio
{
    /// <summary>
    /// Container and wrapper for audio clip reference.
    /// </summary>
    public struct AudioSet
    {
        // Location of Audio SFX files in the resources folder.
        const string ROOT_DIR = "Audio/SFX/";

        // The sub-folder + file name (without index).
        string m_file;

        // The number of files that are known to be available for this particular SFX type.
        public int Count { get; private set; }

        // The type of SFX represented by this container.
        public Enums.SFX_TYPE Type { get; private set; }

        /// <summary>
        /// Returns a random AudioClip from the available range.
        /// </summary>
        public AudioClip Random
        {
            get
            {
                if (Count == 1) return Resources.Load(ROOT_DIR + m_file + 1) as AudioClip;
                var ran = UnityEngine.Random.Range(1, Count + 1);
                AudioClip c = Resources.Load(ROOT_DIR + m_file + ran) as AudioClip;
                return c;
            }
        }

        /// <summary>
        /// Note: Only include folder structure past "Resources/Audio/SFX/".
        /// </summary>
        public AudioSet(Enums.SFX_TYPE clipType, int numberOfClips, string filePath)
        {
            Type = clipType;
            Count = numberOfClips;
            m_file = filePath;
        }
    }
}