using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Interfaces;
using Game.Generic.Data;
using Game.MonoBehaviours.Utiles;

namespace Game.MonoBehaviours.Settings
{
    public class GameSettings : MonoBehaviour, ISaveData, IResetteable
    {
        public static GameSettings S { get; private set; }

        private void Awake()
        {
            // Make instance a singleton
            if (S != null)
            {
                Destroy(gameObject);
            }
            else
            {
                S = this;
            }

            DontDestroyOnLoad(this);
        }

        [Header("Main")]
        [SerializeField] string gameTitle = "Survive"; // Can be set from inspector only

        [Header("Modified by User")]
        [Range(0.1f, 1f)]
        [SerializeField] float cameraMotionSensitivity = 0.5f;
        [Range(0.1f, 1f)]
        [SerializeField] float cameraRotationSensitivity = 0.5f;

        public void SetCameraMotionSensitivity(float value)
        {
            cameraMotionSensitivity = Mathf.Clamp(value, 0.1f, 1f);
            SaveFile();
        }
        public void SetCameraRotationSensitivity(float value)
        {
            cameraRotationSensitivity = Mathf.Clamp(value, 0.1f, 1f);
            SaveFile();
        }

        #region IResetteable

        public void Reset()
        {
            cameraMotionSensitivity = 0.5f;
            cameraRotationSensitivity = 0.5f;
        }

        #endregion

        #region ISaveData

        public void SaveFile()
        {
            GameSettings_SaveFile saveFile = new GameSettings_SaveFile();
            saveFile.CameraMotionSensitivity = cameraMotionSensitivity;
            saveFile.CameraRotationSensitivity = cameraRotationSensitivity;
            SaveGameManager.Save(saveFile, GetPathFile());
        }
        public void LoadDataFromFile()
        {
            GameSettings_SaveFile savedFile = SaveGameManager.Load<GameSettings_SaveFile>(GetPathFile());
            if (savedFile != null)
            {
                cameraMotionSensitivity = savedFile.CameraMotionSensitivity;
                cameraRotationSensitivity = savedFile.CameraRotationSensitivity;
            }
        }
        public void DeleteFile() => SaveGameManager.DeleteFile(GetPathFile());
        public string GetPathFile() => SaveGameManager.SettingsFilePath + "/GameSettings.asset";

        [System.Serializable]
        public class GameSettings_SaveFile : SaveGameManager.SaveFile
        {
            public float CameraMotionSensitivity;
            public float CameraRotationSensitivity;
        }

        #endregion

        #region Accessors

        public string GameTitle() => gameTitle;
        public float CameraMotionSensitivity() => cameraMotionSensitivity;
        public float CameraRotationSensitivity() => cameraRotationSensitivity;

        #endregion
    }
}

