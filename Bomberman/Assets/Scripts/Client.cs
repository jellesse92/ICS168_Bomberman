using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections.Generic;
//come back and add more documentation

public class Client : MonoBehaviour {

    int _channelReliable = -1;
    int _channelUnreliable = 01;
    int _hostID = -1;
    int _connID = -1;
    int port = 7777;
    bool connected = false;
    public int maxConnections = 4;
    string address;

	void Start () {
       // text_area = GameObject.Find("TextAA");
    }
	
    void JoinGame(string ip, int port = 8888)
    {
        address = ip;
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
        }
        else {
            
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

        if (connected)
        {
            NetworkTransport.Send(_hostID, _connID, _channelReliable, buffer, (int)stream.Position, out error);
            if (error > 0) { Debug.Log("Error Sending: " + ((NetworkError)error).ToString()); }
        } else { Debug.Log("Error, Message Not Sent. Not connected."); }
       
   
    }
    
    void Update () {
        int recHostId;
        int connectionId;
        int channelId;
        int dataSize;
        byte[] buffer = new byte[1024];
        byte error;
        if (connected)
        {
            NetworkEventType networkEvent = NetworkEventType.DataEvent;
            
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
                            Debug.Log("Client: Received Data from " + connectionId.ToString() + "!");
                            
                        }
                        break;

                    case NetworkEventType.DisconnectEvent:
                        // Client received disconnect event
                        if (connectionId == _connID)
                        {
                            Debug.Log("Client: Disconnected from server!");
                            // Flag to let client know it can no longer send data
                            connected = false;
                        }
                        break;
                }

            } while (networkEvent != NetworkEventType.Nothing);
        }
    }
}

