using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;


public class Server : MonoBehaviour {
    //GameObject text_area;
    List<int> connectionIDs = new List<int>();
    int _channelReliable = -1;
    int _channelUnreliable = -1;
    public int maxConnections = 4;
    int _serverID = -1;
    public string address;
    public bool initialized = false;
    public int port = 8888;


    //In Game Specific Server Parameters
    public bool gameStarted = false;
    List<int> playersAvailable = new List<int>(new int[] { 1, 2, 3 });
    List<int> playerID = new List<int>(new int[] { 0,-1,-1,-1 });
    float[] lastTimeRecorded = { -1f, -1f, -1f, -1f };
    _GameController gcScript;

    // Use this for initialization
    void Start () {

        address = Network.player.ipAddress;

        
        //CreateGame();       //SET THIS TO A BUTTON OR SOMETHING
        //if (gameStarted)
        //{
        //    gcScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<_GameController>();
        //    gcScript.SetPlayer(0);
        //}


    }

    void SetHost()
    {
        GameObject.Find("Network_Controller").GetComponent<Host>().setHost();
    }
    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }
    void OnMouseDown()
    {
        Debug.Log("Creating Game");
        CreateGame();
    }

    public void CreateGame()
    {
        //Sets up the server configuration for a game of Bomberman.
        GlobalConfig gconfig = new GlobalConfig();
        gconfig.ReactorModel = ReactorModel.FixRateReactor;
        gconfig.ThreadAwakeTimeout = 10;

        ConnectionConfig config = new ConnectionConfig();
        _channelReliable = config.AddChannel(QosType.Reliable);
        _channelUnreliable = config.AddChannel(QosType.Unreliable);
        HostTopology hostconfig = new HostTopology(config, maxConnections);

        NetworkTransport.Init(gconfig);
        _serverID = NetworkTransport.AddHost(hostconfig, port);
        if (_serverID < 0) { Debug.Log("Server socket creation failed!"); }
        initialized = true;
        Debug.Log("Server Initialized");
        //Joins its own game.
        //GameObject.Find(<NAME_OF_ATTACHED_OBJECT>).GetComponentOfType<Client>().JoinGame(address);

        gameStarted = true;
        if (gameStarted)
        {
            gcScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<_GameController>();
            gcScript.SetPlayer(0);
        }

    }
    void SendGameInformation()
    {
        //something to send game information to online server.
    }

    public void Send(string message = "Hello")
    {
        //Relays messages to all connected clients
        foreach (int connectionID in connectionIDs)
        {
            SendToClient(connectionID, message);
        }

    }

    void SendToClient(int clientID,string message)
    {
        byte error;
        byte[] buffer = new byte[1024];
        Stream stream = new MemoryStream(buffer);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(stream, message);

        NetworkTransport.Send(_serverID, clientID, _channelReliable, buffer, (int)stream.Position, out error);
        //if (error > 0) { Debug.Log("Error (" + ((NetworkError)error).ToString() + ") When Sending Message: " + message); }
    }

    //Switch statement to go through if the game has started
    void NetworkSwitchGame(NetworkEventType networkEvent, int recHostId, int connectionId, byte[] buffer)
    {
        switch (networkEvent)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                if (recHostId == _serverID)
                {
                    //Adds player to game
                    if(playersAvailable.Count > 0)
                    {
                        int pl = playersAvailable[0];
                        Send("Active:" + pl);
                        gcScript.ActivatePlayer(pl);
                        gcScript.PlayerJoined();
                        playersAvailable.Remove(pl);
                        playerID[pl] = connectionId;
                        connectionIDs.Add(connectionId);
                        SendToClient(connectionId, "Player:" + pl);
                        for (int i = 0; i < 4; i++)
                        {
                            if (playerID[i] != pl && playerID[i] != -1)
                                SendToClient(connectionId, "Active:" + playerID[i]);
                        }
                        Debug.Log("Server: Player " + pl + " connected!");
                    }
                    
                }

                break;

            case NetworkEventType.DataEvent:
                if (recHostId == _serverID)
                {
                    // deserialize data
                    Stream stream = new MemoryStream(buffer);
                    BinaryFormatter bf = new BinaryFormatter();
                    string msg = bf.Deserialize(stream).ToString();

                    InterpretMessage(msg, connectionId);

                    //Debug.Log("Server: Received Data from " + connectionId.ToString() + "! Message: " + msg);
                    Send(msg);
                }
                break;

            case NetworkEventType.DisconnectEvent:

                Debug.Log("Server: Received disconnect from " + connectionId.ToString());
                int p = playerID.IndexOf(connectionId);
                connectionIDs.Remove(connectionId);
                playersAvailable.Add(p);
                gcScript.DeactivatePlayer(p);
                playerID[p] = -1;
                Send("Deactivate:" + p);
                Debug.Log("Deactivating Player : " + p);
                break;
        }
    }

    void FixedUpdate()
    {
        if (gameStarted && initialized)
        {
            Send(CurrentGameState());
            SendDeathEvent();

        }
    }

    // Update is called once per frame
    void Update () {
        int recHostId;
        int connectionId;
        int channelId;
        int dataSize;
        byte[] buffer = new byte[1024];
        byte error;
        if (initialized)
        {
            NetworkEventType networkEvent = NetworkEventType.DataEvent;
        //Debug.Log("Initialized == " + initialized);

            do
            {
                networkEvent = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, buffer, 1024, out dataSize, out error);
                if (gameStarted)
                    NetworkSwitchGame(networkEvent, recHostId, connectionId,buffer);
                else
                {
                    switch (networkEvent)
                    {
                        case NetworkEventType.Nothing:
                            break;
                        case NetworkEventType.ConnectEvent:

                            if (recHostId == _serverID)
                            {
                                //adds connection to list of connections messages should be relayed to
                                connectionIDs.Add(connectionId);
                                Debug.Log("Server: Player " + connectionId.ToString() + " connected!");
                            }

                            break;

                        case NetworkEventType.DataEvent:
                            if (recHostId == _serverID)
                            {
                                // deserialize data
                                Stream stream = new MemoryStream(buffer);
                                BinaryFormatter bf = new BinaryFormatter();
                                string msg = bf.Deserialize(stream).ToString();

                                Debug.Log("Server: Received Data from " + connectionId.ToString() + "! Message: " + msg);
                                Send(msg);
                            }
                            break;

                        case NetworkEventType.DisconnectEvent:

                            Debug.Log("Server: Received disconnect from " + connectionId.ToString());
                            int player = playerID[connectionId];
                            playersAvailable.Add(player);
                            gcScript.DeactivatePlayer(player);
                            playerID[connectionId] = -1;
                            break;
                    }

                //if (connectionId != 0) {
                //Debug.Log("recHostID: " + recHostId.ToString() + "connectionID: " + connectionId.ToString() + "\n"); }
                //if (networkEvent.ToString() != "Nothing") { Debug.Log(networkEvent.ToString()+ " recHostID: " + recHostId.ToString() + "connectionID: " + connectionId.ToString() + "\n"); }
               
                }

            } while (networkEvent != NetworkEventType.Nothing);
        }
    }

    void InterpretMessage(string msg, int clientId)
    {
        
        //Updates player locations
        if(msg.Substring(0,4) == "Pos:"){
            float x, y, time;
            string[] temp = msg.Substring(4).Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            float.TryParse(temp[0],out x);
            float.TryParse(temp[1], out y);
            float.TryParse(temp[2], out time);

            int pID = playerID.IndexOf(clientId);

            if(lastTimeRecorded[pID] < time)
            {
                gcScript.UpdatePlayerPosition(pID, x, y);
                lastTimeRecorded[pID] = time;
            }

        } else
        {
            Debug.Log(msg);
        }

       
        //Drops bombs for players
        if(msg == "dropBomb"){
            //Debug.Log("Dropped Bomb: " + playerID[clientId]);
            gcScript.UpdateBombPlace(playerID[clientId]);
            SendBombEvent(playerID[clientId]);
        }

		//Death and Score update
		if (msg.Substring(0,5) == "Death") {
            int temp = -1;
            int.TryParse(msg.Substring(6, 1), out temp);
			gcScript.UpdateScores(playerID[clientId], temp);
		}

    }


    string CurrentGameState()
    {
        string state = "0:Pos:";

        //Get Positions of all players
        for(int i = 0; i < 4; i++)
        {
            float x = gcScript.GetPlayerPos(i).x;
            float y = gcScript.GetPlayerPos(i).y;
            state += i+":" + x + "," + y + ";";
        }

        if (state.Length > 1)
            state = state.Substring(0, state.Length - 2);

        return state;
    }

    public void SendBombEvent(int player)
    {
        Debug.Log("Sending Bomb");
        Send(player + ":" + "dropBomb");
    }
	public void SendDeathEvent()
	{
        string result = "Scores";
		foreach(GameObject pScore in gcScript.players){
            result += (":" + pScore.GetComponent<PlayerController>().score);
		}
		Send (result);

	}

    public void Disconnect()
    {
        byte error;
        initialized = false;
        gameStarted = false;
        //NetworkTransport.
        NetworkTransport.DisconnectNetworkHost(_serverID, out error);
        if (error != (byte)NetworkError.Ok)
        {
            Debug.Log(error.ToString());
        }
        
    }

}
