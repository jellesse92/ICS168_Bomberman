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
    public int port = 7777;


    //In Game Specific Server Parameters
    public bool gameStarted = false;
    List<int> playersAvailable = new List<int>(new int[] { 1, 2, 3 });
    List<int> playerID = new List<int>(new int[] { 0,-1,-1,-1 });
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
            Debug.Log("ConnectionID: " + connectionID);
            //Debug.Log(message);
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
        if (error > 0) { Debug.Log("Error (" + ((NetworkError)error).ToString() + ") When Sending Message: " + message); }
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
                        int player = playersAvailable[0];
                        gcScript.ActivatePlayer(player);
                        playersAvailable.Remove(player);
                        playerID[player] = connectionId;
                        connectionIDs.Add(connectionId);
                        SendToClient(connectionId, "Player:" + player);
                        Debug.Log("Server: Player " + connectionId.ToString() + " connected!");
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

                    Debug.Log("Server: Received Data from " + connectionId.ToString() + "! Message: " + msg);
                    Send(msg);
                }
                break;

            case NetworkEventType.DisconnectEvent:

                if (recHostId == connectionId)
                {
                    Debug.Log("Server: Received disconnect from " + connectionId.ToString());
                    int player = playerID[connectionId];
                    playersAvailable.Add(player);
                    gcScript.DeactivatePlayer(player);
                    playerID[connectionId] = -1;
                }
                break;
        }
    }

    void FixedUpdate()
    {
        if (gameStarted)
            Send(CurrentGameState());
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
                Debug.Log(networkEvent.ToString());
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

                            if (recHostId == connectionId)
                            {
                                Debug.Log("Server: Received disconnect from " + connectionId.ToString());
                            }
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
            float x, y;
            string[] temp = msg.Substring(4).Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            float.TryParse(temp[0],out x);
            float.TryParse(temp[1], out y);
            gcScript.UpdatePlayerPosition(playerID[clientId], x, y);
        }

        //Drops bombs for players
        if(msg == "dropBomb"){
            //Debug.Log("Dropped Bomb: " + playerID[clientId]);
            gcScript.UpdateBombPlace(playerID[clientId]);
            SendBombEvent(playerID[clientId]);
        }

		//Death and Score update
		if (msg.Substring(0,4) == "Death") {
			gcScript.UpdateScores(playerID[clientId], int(msg.Substring (5,6)));

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
	public void SendDeathEvent(int player)
	{
		int[] scoreList = new int[];
		Debug.Log ("Sending Death");
		foreach(GameObject pScore in gcScript.players){
			pScore.GetComponent<PlayerController>().score 
		}
		Send ("Scores:");

	}
}
