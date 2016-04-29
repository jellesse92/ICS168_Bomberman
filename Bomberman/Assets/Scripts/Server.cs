using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;


public class Server : MonoBehaviour {
    //GameObject text_area;
    List<int> connectionIDs = new List<int>();
    int _channelReliable = -1;
    int _channelUnreliable = -1;
    public int maxConnections = 4;
    int _serverID = -1;
    public string address;
    bool initialized = false;

	// Use this for initialization
	void Start () {

        address = Network.player.ipAddress;
        //text_area = GameObject.Find("TextAA");
    }

    void OnMouseDown()
    {
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
        _serverID = NetworkTransport.AddHost(hostconfig, 8888);
        if (_serverID < 0) { Debug.Log("Server socket creation failed!"); }
        initialized = true;
        Debug.Log("Server Initialized");
        //Joins its own game.
        //GameObject.Find(<NAME_OF_ATTACHED_OBJECT>).GetComponentOfType<Client>().JoinGame(address);
    }
    void SendGameInformation()
    {
        //something to send game information to online server.
    }

    void Send(string message = "Hello")
    {
        //Relays messages to all connected clients
        byte error;
        byte[] buffer = new byte[1024];
        Stream stream = new MemoryStream(buffer);
        BinaryFormatter bf = new BinaryFormatter();
        bf.Serialize(stream, message);

        foreach (int connectionID in connectionIDs)
        {
            //Debug.Log("ConnectionID: " + connectionID);
            NetworkTransport.Send(_serverID, connectionID, _channelReliable, buffer, (int)stream.Position, out error);
            if (error > 0) { Debug.Log("Error (" + ((NetworkError)error).ToString() + ") When Sending Message: " + message); }
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

        NetworkEventType networkEvent = NetworkEventType.DataEvent;
        if (initialized)
        {
            do
            {
                networkEvent = NetworkTransport.Receive(out recHostId, out connectionId, out channelId, buffer, 1024, out dataSize, out error);
                //if (connectionId != 0) {
                    //Debug.Log("recHostID: " + recHostId.ToString() + "connectionID: " + connectionId.ToString() + "\n"); }
                //if (networkEvent.ToString() != "Nothing") { Debug.Log(networkEvent.ToString()+ " recHostID: " + recHostId.ToString() + "connectionID: " + connectionId.ToString() + "\n"); }
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

            } while (networkEvent != NetworkEventType.Nothing);
        }
    }
}
