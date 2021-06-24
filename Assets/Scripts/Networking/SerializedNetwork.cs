using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NetworkClases
{
    [System.Serializable]
    public class userData
    {
        public string nombre;
        public string apellido;
        public string keyUser;
        public string telefono;
        public string loginData;
        public int genero;
    }

    [System.Serializable]
    public class RoomData
    {
        public string name;
        public bool IsVisible = true;
        public int maxPlayer = 8;
        public int PlayerCount = 0;
        public Hashtable CustomRoomProperties;
        public List<PlayerRoom> players;

        public bool SyncScene;
        public string sceneName;
    }

    public class PlayerRoom
    {
        public bool isLocal;
        public string NickName;
        public string UserId;
        public string Team;
    }

    [System.Serializable]
    public class KillHistory
    {
        public string playerKiller;
        public string victim;
        public string date;
        public string roomID;
    }
}