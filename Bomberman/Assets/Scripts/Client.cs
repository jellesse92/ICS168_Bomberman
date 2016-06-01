using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Collections.Generic;
//come back and add more documentation

public class Client : MonoBehaviour {

    int _channelReliable = -1;
    int _channelUnreliable = 01;
    int _hostID = -1;
    int _connID = -1;
    public int port = 8888;
    bool isHost = false;
    bool connected = false;
    bool connectedToServer = false;
    public int maxConnections = 4;
    public string address = "192.168.43.115";

    //In-Game Related Parameters
    _GameController gcScript;
    GameEndUIController guiScript;
    ApplicationManager appScript;
    bool gameStarted = true;
    bool inLobby = true;

    int setPlayer = -1;
    List<int> ActivatePlayers = new List<int>();

    //Syncing
    float timeToReport;

    void Start () {
        appScript = GameObject.Find("ApplicationManager").GetComponent<ApplicationManager>();
    }

    void Awake()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    public void Join()
    {
        JoinGame(address);
        connectedToServer = true;
    }

    public void JoinGame(string ip = "", int port_num = 0)
    {
        //Allows players to join a game at the given port and ip address.
        if(ip != "") { address = ip; }
        //may be neccessary later
        //isHost = GameObject.Find("Network_Controller").GetComponent<Server>().initialized;
        Debug.Log("Connected == " + connected);
        if (port_num != 0) { port = port_num; }
        if (!connected)
        {
            // global config
            GlobalConfig gconfig = new GlobalConfig();
            gconfig.ReactorModel = ReactorModel.FixRateReactor;
            gconfig.ThreadAwakeTimeout = 10;


            ConnectionConfig config = new ConnectionConfig();
            _channelReliable = config.AddChannel(QosType.Reliable);
            _channelUnreliable = config.AddChannel(QosType.Unreliable);

            HostTopology hostconfig = new HostTopology(config, maxConnections);


            NetworkTransport.Init(gconfig);
            _hostID = NetworkTransport.AddHost(hostconfig);


            byte error;

            _connID = NetworkTransport.Connect(_hostID, address, port, 0, out error);


            if (error != (byte)NetworkError.Ok)
            {
                NetworkError nerror = (NetworkError)error;
                Debug.Log("Error " + nerror.ToString());
            }
            connected = true;
            gcScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<_GameController>();
            guiScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameEndUIController>();

        }
        else {
            //stops users from connecting multiple times -- allows us to check if its sending messages at all.
            Send(); 
        }
    }

    void Send(string message = "Hello")
    {
        //sends information to server only if client is connected.
        byte error;
        byte[] buffer = new byte[1024];
        Stream stream = new MemoryStream(buffer);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(stream, message);

        if (connectedToServer)
        {
            NetworkTransport.Send(_hostID, _connID, _channelReliable, buffer, (int)stream.Position, out error);
            if (error > 0) { Debug.Log("Error Sending: " + ((NetworkError)error).ToString()); }
        }
        else { Debug.Log("Error, Message Not Sent. Not connected."); }
       
   
    }
    
    void Update () {
        int recHostId;
        int connectionId;
        int channelId;
        int dataSize;
        byte[] buffer = new byte[1024];
        byte error;

        NetworkEventType networkEvent = NetworkEventType.DataEvent;
        if (connected) {
            do
            {
                networkEvent = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, buffer, 1024, out dataSize, out error);
                switch (networkEvent)
                {
                    case NetworkEventType.Nothing:
                        break;
                    case NetworkEventType.ConnectEvent:
                        if (connectionId == _connID)
                        {
                            Debug.Log("Client: Client connected to " + connectionId.ToString() + "!");
                            connectedToServer = true;
                            connected = true;
                            gameStarted = true;
                            Send("User:" + appScript.username);
                        }

                        break;

                    case NetworkEventType.DataEvent:
                        if (recHostId != connectionId)
                        {
                            Stream stream = new MemoryStream(buffer);
                            BinaryFormatter bf = new BinaryFormatter();
                            string msg = bf.Deserialize(stream).ToString();

                            //Debug.Log("Client: Received Data from " + connectionId.ToString() + "! Message: " + msg);
                            InterpretMessage(msg);

                        }
                        break;

                    case NetworkEventType.DisconnectEvent:
                        // Client received disconnect event
                        if (connectionId == _connID)
                        {
                            Debug.Log("Client: Disconnected from server!");
                            connected = false;
                            // Flag to let client know it can no longer send data
                            gameStarted = false;
                            guiScript.ActivateDisconnectScreen();

                        }
                        break;
                }

            } while (networkEvent != NetworkEventType.Nothing);
        }
    }

    void FixedUpdate()
    {
        if(!inLobby)
        {
            timeToReport = Time.time;
            Send(GetGameData());
        }
    }

    //Gathers game state data to send to server
    string GetGameData()
    {
        string data = "";
        data += "Pos:" + gcScript.GetPlayerPos(gcScript.ControlledPlayer()).x + "," + gcScript.GetPlayerPos(gcScript.ControlledPlayer()).y + "," + timeToReport;
        return data;
    }

    //Sends message for bomb drops
    public void SendBombDrop()
    {
        Send("dropBomb");
    }

	public void SendDeath(int killer)
	{
		Send ("Death:" + killer);
	}

    void StartGame()
    {
        inLobby = false;
        Time.timeScale = 1.0f;
        GameObject.FindGameObjectWithTag("SceneManager").transform.GetChild(0).gameObject.SetActive(false);
        GameObject.FindGameObjectWithTag("SceneManager").transform.GetChild(1).gameObject.SetActive(true);
        gcScript.SetPlayer(setPlayer);
        foreach(int p in ActivatePlayers)
            gcScript.ActivatePlayer(p);
    }
    
    void InterpretMessage(string msg)
    {
    
        int player = -1;
        if (msg.Length == 0)
            return;

        if(msg == "START")
        {
            StartGame();
        }

        if(msg == "GAME_END")
        {
            gcScript.ActivateGameOver();
        }

        if(msg.Length > 5 && msg.Substring(0,5) == "Time:")
        {
            float t;
            float.TryParse(msg.Substring(5), out t);
            gcScript.SetTimeRemaining(t);
        }

        if(msg.Length > 5 && msg.Substring(0,5) == "Pick:")
        {
            string assignments = msg.Substring(5);
            gcScript.SetPickUps(assignments);
        }

        if (msg.Substring(0, msg.Length - 1) == "Player:")
        {
            int.TryParse(msg.Substring(msg.Length - 1), out setPlayer);
            return;
        }

        if(msg.Substring(0, msg.Length - 1) == "Active:")
        {
            int newPlayer = 0;
            int.TryParse(msg.Substring(msg.Length - 1), out newPlayer);
            ActivatePlayers.Add(newPlayer);
            if (inLobby)
                GameObject.FindGameObjectWithTag("GameController").GetComponent<LobbyScript>().ActivatePlayer(newPlayer);

        }
        if (msg.Substring(0, msg.Length - 1) == "Deactivate:")
        {
            int newPlayer = 0;
            int.TryParse(msg.Substring(msg.Length - 1), out newPlayer);
            gcScript.DeactivatePlayer(newPlayer);
        }
        int.TryParse(msg.Substring(0, 1), out player);

        //Ignore the server trying to tell you what you've already done
        if (player == gcScript.ControlledPlayer())
            return;

        //Someone dropped a bomb
        if (msg.Substring(2) == "dropBomb")
        {
            Debug.Log(player);
            gcScript.UpdateBombPlace(player);
        }


		//Someone died
		if (msg.Substring (0,7) == "Scores:") {
			string[] temp = msg.Substring(7).Split(new[] { ':', ' ' }, StringSplitOptions.RemoveEmptyEntries);
			for(int i = 0; i < gcScript.players.Length; i++){
				int.TryParse (temp[i], out gcScript.players[i].GetComponent<PlayerController>().score);
                gcScript.players[i].GetComponent<PlayerController>().UpdateScoreDisplay();

            }
		}

        //Server sending everyone's information
        if(msg.Substring(0,6) == "0:Pos:")
        {
            string[] temp = msg.Substring(6).Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string posData in temp)
            {
                int.TryParse(posData.Substring(0, 1), out player);

                float x, y;
                string[] pos = posData.Substring(2).Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (pos.Length < 2)
                    return;
                float.TryParse(pos[0], out x);
                float.TryParse(pos[1], out y);
                if(player != gcScript.ControlledPlayer())
                    gcScript.UpdatePlayerPosition(player, x, y);
            }
            

        }
    }

    public void Disconnect()
    {
        byte error;
        NetworkTransport.Disconnect(_hostID, _connID, out error);
        if (error != (byte)NetworkError.Ok)
        {
            Debug.Log(error.ToString());
        }
    }

    public void SetServer(string serverIp, int serverPort = 8888)
    {
        address = serverIp;
        port = serverPort;
        Debug.Log("CONNECT: " + address);
        Debug.Log("ADDRESS LENGTH: " + address.Length);
        Debug.Log("PORT: " + port);
        
    }
}

