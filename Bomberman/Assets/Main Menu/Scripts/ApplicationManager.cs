using UnityEngine;
using System.Collections;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Collections.Generic;
using UnityEngine.UI;

/***
        Socket code credit to: https://msdn.microsoft.com/en-us/library/bew39x2a%28v=vs.110%29.aspx
***/



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

    public bool logged_in = false;
    bool connectedToServer = false;
    public bool log_in_error = false;
    public bool data_error = false;
    public bool connected = false;
    public int port = 7777;
    public string myip = "";
    public string address = "52.26.63.128";
    public string username = "";
    string _lastSentMsg;
    List<GameInfo> gamesList;
    List<Stats> playerStats; //For Later in the game, get stats for all players.

    System.Net.Sockets.TcpClient clientSocket;
    NetworkStream serverStream;

    bool initialized = false;

    public Text ipText;         //Displays ip address for game creation



    //Response from server
    private static String response = String.Empty;

    void Awake()

    {
        DontDestroyOnLoad(transform.gameObject);
    }

    //Send message to server and get server response
    public string GetServerResponse(string msg)
    {

        byte[] outStream = System.Text.Encoding.ASCII.GetBytes(msg + "$");
        serverStream.Write(outStream, 0, outStream.Length);
        serverStream.Flush();

        byte[] inStream = new byte[10025];
        serverStream.Read(inStream, 0, 256);
        string returndata = System.Text.Encoding.ASCII.GetString(inStream);

        return returndata;
    }



    void Start()
    {
        clientSocket = new System.Net.Sockets.TcpClient();
        clientSocket.Connect(address, port);
        serverStream = clientSocket.GetStream();
        DontDestroyOnLoad(transform.gameObject);
        myip = Network.player.ipAddress.ToString();
        ipText.text = myip;


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
        LSDisconnect();
		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit();
		#endif
	}

    void OnApplicationQuit()
    {
        LSDisconnect();
    }

    public void LSDisconnect()
    {
        if (logged_in)
        {
            Debug.Log(username);
            Debug.Log(GetServerResponse("8:" + username));
        }
        else
        {
            Debug.Log(GetServerResponse("Q:QUIT"));
        }
    }

}
