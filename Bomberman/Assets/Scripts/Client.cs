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
    int port = 8888;
    bool connected = false;
    bool connectedToServer = false;
    public int maxConnections = 4;
    public string address = "192.168.1.74";

    //In-Game Related Parameters
    _GameController gcScript;
    bool gameStarted = true;

    void Start () {
        // text_area = GameObject.Find("TextAA");

        //Change when this happens depending on what scene client is instantiated
        gcScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<_GameController>();
    }
	
    public void Join()
    {
        JoinGame(address);
        connectedToServer = true;
    }

    void JoinGame(string ip, int port_num = 8888)
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

            _connID = NetworkTransport.Connect(_hostID, address, port_num, 0, out error);


            if (error != (byte)NetworkError.Ok)
            {
                NetworkError nerror = (NetworkError)error;
                Debug.Log("Error " + nerror.ToString());
            }
            connected = true;
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

        NetworkEventType networkEvent = NetworkEventType.DataEvent;
        if (connected) {
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
                            connectedToServer = true;
                            gameStarted = true;
                        }

                        break;

                    case NetworkEventType.DataEvent:
                        if (recHostId != connectionId)
                        {
                            Stream stream = new MemoryStream(buffer);
                            BinaryFormatter bf = new BinaryFormatter();
                            string msg = bf.Deserialize(stream).ToString();

                            Debug.Log("Client: Received Data from " + connectionId.ToString() + "! Message: " + msg);
                            
                        }
                        break;

                    case NetworkEventType.DisconnectEvent:
                        // Client received disconnect event
                        if (connectionId == _connID)
                        {
                            Debug.Log("Client: Disconnected from server!");
                            // Flag to let client know it can no longer send data
                            gameStarted = false;
                        }
                        break;
                }

            } while (networkEvent != NetworkEventType.Nothing);
        }
    }

    void FixedUpdate()
    {
        if (gameStarted && connectedToServer)
        {
            //Send(GetGameData());
        }
    }

    //Gathers game state data to send to server
    string GetGameData()
    {
        string data = "";
        data += "Pos:" + gcScript.GetPlayerPos(gcScript.ControlledPlayer()).x + "," + gcScript.GetPlayerPos(gcScript.ControlledPlayer()).y;
        return data;
    }

    //Sends message for bomb drops
    public void SendBombDrop()
    {
        Send("dropBomb");
    }

    void InterpretMessage(string msg)
    {
        int player = -1;
        if (msg.Length == 0)
            return;

        //Set what player the client controls
        if (msg.Substring(0,msg.Length-2) == "Player:")
        {
            int.TryParse(msg.Substring(msg.Length - 1), out player);
            gcScript.SetPlayer(player);
            return;
        }


        int.TryParse(msg.Substring(0, 1), out player);

        //Ignore the server trying to tell you what you've already done
        if (player == gcScript.ControlledPlayer())
            return;

        //Someone dropped a bomb
        if (msg.Substring(2) == "dropBomb")
            gcScript.UpdateBombPlace(player);

        //Server sending everyone's information
        if(msg.Substring(0,6) == "0:Pos:")
        {
            string[] temp = msg.Substring(6).Split(new[] { ';', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string posData in temp)
            {
                int.TryParse(posData.Substring(0, 1), out player);

                float x, y;
                string[] pos = posData.Substring(2).Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                float.TryParse(pos[0], out x);
                float.TryParse(pos[1], out y);
                if(player != gcScript.ControlledPlayer())
                    gcScript.UpdatePlayerPosition(player, x, y);
            }
            

        }
    }
}

