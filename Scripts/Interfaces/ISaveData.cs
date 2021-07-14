using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Generic.Data;

namespace Game.Interfaces
{
    public interface ISaveData
    {
        void SaveFile();
        void LoadDataFromFile();
        void DeleteFile();
        string GetPathFile();
    }
}
