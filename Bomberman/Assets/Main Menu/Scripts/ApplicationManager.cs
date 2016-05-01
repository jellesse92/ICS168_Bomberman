using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System;
using System.Collections.Generic;


public class GameInfo
{
    public string username;
    public string ipAddress;
    public int port;

    public GameInfo(string user, string ip, int p = 7777)
    {
        user = username;
        ipAddress = ip;
        port = p;
    }
}

public class Stats
{
    public string username;
    public int wins;
    public int loss;
    public int games;
    public int kills;
    public int deaths;

    public Stats(string stat)
    {
        string[] stat_split = stat.Split(':');
        username = stat_split[0];
        kills = Int32.Parse(stat_split[1]);
        deaths = Int32.Parse(stat_split[2]);
        wins = Int32.Parse(stat_split[3]);
        games = Int32.Parse(stat_split[4]);

    }
}
public class ApplicationManager : MonoBehaviour {

    int _channelReliable = -1;
    int _channelUnreliable = 01;
    int _hostID = -1;
    int _connID = -1;
    bool logged_in = true;
    bool connectedToServer = false;
    public bool log_in_error = false;
    public bool data_error = false;
    public bool connected = false;
    public int maxConnections = 1000;
    public int port = 8888;
    public string address = "128.195.67.168";
    string _lastSentMsg;
    List<GameInfo> gamesList;
    List<Stats> playerStats; //For Later in the game, get stats for all players.
    

    void Start()
    {
        DontDestroyOnLoad(transform.gameObject);
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
        }
    }

    public void Send(string message = "")
    {
        //sends information to server only if client is connected.
        byte error;
        byte[] buffer = new byte[1024];
        Stream stream = new MemoryStream(buffer);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(stream, message);

        if (connected)
        {
            NetworkTransport.Send(_hostID, _connID, _channelReliable, buffer, (int)stream.Position, out error);
            if (error > 0) { Debug.Log("Error Sending: " + ((NetworkError)error).ToString()); }
            else
            {
                _lastSentMsg = message;
            }
            
        }
        else { Debug.Log("Error, Message Not Sent. Not connected."); }

    }

    void Update()
    {
        int recHostId;
        int connectionId;
        int channelId;
        int dataSize;
        byte[] buffer = new byte[1024];
        byte error;

        NetworkEventType networkEvent = NetworkEventType.DataEvent;
        if (connected)
        {
            Send("0:house:cat");
            do
            {
                networkEvent = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, buffer, 1024, out dataSize, out error);
                //Debugging to check and compare connection ids
                //if (networkEvent.ToString() != "Nothing") { Debug.Log(networkEvent.ToString() + "   recHostID: " + recHostId.ToString() + "connectionID: " + connectionId.ToString() + "-Client \n"); }
                switch (networkEvent)
                {
                    case NetworkEventType.Nothing:
                        break;
                    case NetworkEventType.ConnectEvent:
                        if (connectionId == _connID)
                        {
                            Debug.Log("Client: Client connected to " + connectionId.ToString() + "!");
                            connected = true;
                        }

                        break;

                    case NetworkEventType.DataEvent:
                        if (recHostId != connectionId)
                        {
                            Stream stream = new MemoryStream(buffer);
                            BinaryFormatter bf = new BinaryFormatter();
                            string msg = bf.Deserialize(stream).ToString();
                            parseMessage(msg);
                            Debug.Log("Client: Received Data from " + connectionId.ToString() + "! Message: " + msg);

                        }
                        break;

                    case NetworkEventType.DisconnectEvent:
                        // Client received disconnect event
                        if (connectionId == _connID)
                        {
                            Debug.Log("Client: Disconnected from server!");
                            // Flag to let client know it can no longer send data
                        }
                        break;
                }

            } while (networkEvent != NetworkEventType.Nothing);
        }
    }

    void parseMessage(string msg)
    {
        //string[] splitMsg;
        if (msg[0] == 'G')
        {
            getGamesList(msg.Substring(2));
        } else if (msg[0] == 'S' && msg[1] == ':')
        {
            playerStats.Add(new Stats(msg.Substring(2)));
        }
        if(_lastSentMsg[0] == '0' || _lastSentMsg[0] == '1')
        {
            if (msg == "SUCCESS")
            {
                logged_in = true;
            } else {
                log_in_error = true;
            }
        }
        else if (_lastSentMsg[0] == '3')
        {
            if (msg == "SUCCESS")
            {
                data_error = false;
            }
            else {
                data_error = true;
            }
        }
    }

    void getGamesList(string games)
    {
        string[] splitmsg = games.Split();
        foreach(string msg in splitmsg)
        {
            string[] info = msg.Split(':');
            gamesList.Add(new GameInfo(info[0], info[1], Int32.Parse(info[2])));
        }
    }

   
    public void Quit () 
	{
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}
}
