using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Netcode;

public class ServerSingleton : MonoBehaviour
{
    private static ServerSingleton instance;

    public ServerGameManager GameManager { get; private set; }

    public static ServerSingleton Instance {
        get
        {
            if (instance != null) { return instance; }

            instance = FindObjectOfType<ServerSingleton>();

            if(instance == null)
            {
                Debug.LogError("No ServerSingleton in the scene!");
                return null;
            }

            return instance;
        }
    }
    
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public async Task CreateServer()
    {
        await UnityServices.InitializeAsync();

        GameManager = new ServerGameManager(
            ApplicationData.IP(),
            ApplicationData.Port(),
            ApplicationData.QPort(),
            NetworkManager.Singleton
        );

    }

    private void OnDestroy()
    {
        GameManager?.Dispose();
    }
}
