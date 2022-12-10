using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Oculus.Platform;
using TMPro;
public class LoginManager : MonoBehaviourPunCallbacks
{
    public GameObject _spawnPoint;
    [SerializeField] TextMeshProUGUI m_screenText;
    [SerializeField] ulong m_userId;

    //Singleton implementation
    private static LoginManager m_instance;
    public static LoginManager Instance
    {
        get
        {
            return m_instance;
        }
    }
    private void Awake()
    {
        if (m_instance == null)
        {
            m_instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    void Start()
    {
        StartCoroutine(SetUserIdFromLoggedInUser());
        StartCoroutine(ConnectToPhotonRoomOnceUserIdIsFound());
        StartCoroutine(InstantiateNetworkedAvatarOnceInRoom());
    }

    IEnumerator SetUserIdFromLoggedInUser()
    {
        
        if (!Oculus.Platform.Core.IsInitialized())
        {
            Oculus.Platform.Core.Initialize();
            //OvrPlatformInit.InitializeOvrPlatform();
        }
        
        while (Oculus.Platform.Core.IsInitialized() != true)
        {
            if (!Oculus.Platform.Core.IsInitialized())
            {
                Debug.LogError("OVR Platform failed to initialise");
                m_screenText.text = "OVR Platform failed to initialise";
                yield break;
            }
            yield return null;
        }

        Users.GetLoggedInUser().OnComplete(message =>
        {
            if (message.IsError)
            {
                Debug.LogError("Getting Logged in user error " + message.GetError());
            }
            else
            {
                //Debug.LogWarning(message.Data.InviteToken);
                m_userId = message.Data.ID;
            }
        });
        yield return null;
    }

    IEnumerator ConnectToPhotonRoomOnceUserIdIsFound()
    {
        while (m_userId == 0)
        {
            Debug.Log("Waiting for User id to be set before connecting to room");
            yield return null;
        }
        ConnectToPhotonRoom();
    }

    void ConnectToPhotonRoom()
    {
        PhotonNetwork.ConnectUsingSettings();
        m_screenText.text = "Connecting to Server";
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        m_screenText.text = "Connecting to Lobby";
    }

    public override void OnJoinedLobby()
    {
        m_screenText.text = "Creating Room";
        PhotonNetwork.JoinOrCreateRoom("room", null, null);
    }

    public override void OnJoinedRoom()
    {
        string roomName = PhotonNetwork.CurrentRoom.Name;
        m_screenText.text = "Joined room with name " + roomName;
    }

    IEnumerator InstantiateNetworkedAvatarOnceInRoom()
    {
        while (PhotonNetwork.InRoom == false)
        {
            Debug.Log("Waiting to be in room before intantiating avatar");
            yield return null;
        }
        InstantiateNetworkedAvatar();
    }

    void InstantiateNetworkedAvatar()
    {
        Int64 userId = Convert.ToInt64(m_userId);
        object[] objects = new object[1] { userId };
        GameObject myAvatar = PhotonNetwork.Instantiate("NetworkPlayer", _spawnPoint.transform.position, Quaternion.identity, 0, objects);
    }
}
