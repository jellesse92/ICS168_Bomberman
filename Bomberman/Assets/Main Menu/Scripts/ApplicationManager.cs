using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;

/***
        Socket code credit to: https://msdn.microsoft.com/en-us/library/bew39x2a%28v=vs.110%29.aspx
***/

public class StateObject
{
    // Client socket.
    public Socket workSocket = null;
    // Size of receive buffer.
    public const int BufferSize = 256;
    // Receive buffer.
    public byte[] buffer = new byte[BufferSize];
    // Received data string.
    public StringBuilder sb = new StringBuilder();
}


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

    bool logged_in = true;
    bool connectedToServer = false;
    public bool log_in_error = false;
    public bool data_error = false;
    public bool connected = false;
    public int port = 8888;
    public string address = "128.195.67.168";
    string _lastSentMsg;
    List<GameInfo> gamesList;
    List<Stats> playerStats; //For Later in the game, get stats for all players.


    private static ManualResetEvent connectDone = new ManualResetEvent(false);
    private static ManualResetEvent sendDone = new ManualResetEvent(false);
    private static ManualResetEvent receiveDone = new ManualResetEvent(false);

    //Response from server
    private static String response = String.Empty;

    void ConnectClient()
    {
        try
        {
            IPAddress ipAddress = IPAddress.Parse(address);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);

            Socket clientSocket = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.Tcp);

            clientSocket.BeginConnect(remoteEP,new AsyncCallback(ConnectCallback), clientSocket);
            connectDone.WaitOne();

            //Sending to server
            Send(clientSocket, "This is a test<EOF>");
            sendDone.WaitOne();

            //Recieving from server
            Receive(clientSocket);
            receiveDone.WaitOne();

            Debug.Log("Response received: " + response);

            // Release the socket.
            clientSocket.Shutdown(SocketShutdown.Both);
            clientSocket.Close();

        }
        catch(Exception e)
        {
            Debug.Log("Error: " + e);
        }
           
    }


    private static void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket clientSocket = (Socket)ar.AsyncState;
            clientSocket.EndConnect(ar);
            connectDone.Set();
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e);
        }
    }

    //For sending data to server
    private static void Send(Socket client, String data)
    {
        // Convert the string data to byte data using ASCII encoding.
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.
        client.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), client);
    }

    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            Socket client = (Socket)ar.AsyncState;
            int bytesSent = client.EndSend(ar);
            sendDone.Set();
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e);
        }
    }

    //For recieving data from server
    private static void Receive(Socket client)
    {
        try
        {
            StateObject state = new StateObject();
            state.workSocket = client;
            client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e);
        }
    }

    private static void ReceiveCallback(IAsyncResult ar)
    {
        try
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket client = state.workSocket;

            int bytesRead = client.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            else {
                if (state.sb.Length > 1){
                    response = state.sb.ToString();
                }
                receiveDone.Set();
            }
        }
        catch (Exception e)
        {
            Debug.Log("Error: " + e);
        }
    }

    void Start()
    {
        DontDestroyOnLoad(transform.gameObject);
        ConnectClient();

    }



    void Update()
    {

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
