import socket
from _thread import *
from server import *
from queries import *
from collections import namedtuple


#Socket Connection for game
host = '172.31.39.29'
port = 7777
backlog = 1000
size = 1024

print("Creating Socket")
soc = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
soc.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
soc.bind((host,port))
soc.listen(backlog)

print("Socket Initialized, Searching for connections")
clients = {}

to_return = "EMPTY"

ClientConnection = namedtuple('ClientConnection', 'username, conn')

def disconnect(conn):
   print("Remote Client Disconnected")
   try:
      nc = ""
      for k, v in clients:
         print(c)
         if v.conn == conn:
            log_out(k)
            nc = k
      if nc != "":
         del clients[nc]
      return "X"
   except:
      conn.close()
      return "X"
   
def initiateConnection(conn: tuple):
   print("Client Connected!")
   to_return = "EMPTY"
   try:
      #conn.send("Connection Verified")
      while True:
         data = conn.recv(size)
         data = str(data).strip("b\'").strip("\\r\\n\'")
         query = data.split(':')

         if query[0] == '0': # Add Account
            clients[query[1]] = ClientConnection(query[1], conn) #adds connection to connection list
            to_return = create_account(query[1], query[2])
            conn.sendall(to_return.encode())
            print("SENT: " + to_return)
         elif query[0] == '1': #Log In
            clients[query[1]] = ClientConnection(query[1], conn)
            to_return = log_in(query[1], query[2])
            conn.send(to_return.encode())
         elif query[0] == '2': # Update Stats
            to_return = update_stats(query[1], query[2], query[3], query[4], query[5])
            conn.send(to_return.encode())
         elif query[0] == '3': # Get Stats
            to_return = get_stats(query[1])
            conn[0].send(to_return.encode())
         elif query[0] == '4': # Host Game
            to_return = host_game(query[1], query[2], query[3])
            conn.send(to_return.encode())
         elif query[0] == '5': # Get Games
            to_return = get_games()
            conn.send(to_return.encode())
         #code below here!!!!
         elif query[0] == '6': # Start Game
            to_return = start_game(query[1])
            conn.send(to_return.encode())
         elif query[0] == '7': # Delete Game
            to_return = destroy_game(query[1])
            conn.send(to_return.encode())
         elif query[0] == '8': # Disconnect Player
            print("Player Disconnecting... " + query[1][:-1])
            to_return = log_out(query[1][:-1])
            print("Returning.... " + to_return)
            conn.send(to_return.encode())
            exit()
            #to_return = disconnect(conn)
            print("closing connection")
         elif query[0] == '9': # Invite Player
            if can_invite(query[1]):
                to_return = "I:" + query[1] + ":"+ query[3] + ":" + query[4]
                to_client = "SENT"
                clients[query[2]].conn[0].send(to_return.encode())
                conn.send(to_client.encode())
         elif query[0] == 'A': #Send Active Game Stats to the Database
            gamestats = {}
            for i in len(query):
                if i >= 2:
                   data = query.split("_")
                   gamestats[i] = (data[1], data[2])
            conn.send(send_game_stats(query[1], gamestats))
         elif query[0] == 'B': #Have user Leave Game Early
            leave_game(query[1])
         elif query[0] == 'Q':
            print("received quit")
            conn.send(query[1].encode())
            print("Calling disconnect...")
            to_return = disconnect(conn)
            
         print(to_return)
   except:
      to_return = disconnect(conn)
   
while True:
   try:
      print("trying this.")
      client, addr = soc.accept()
      #initiateConnection(client)
      start_new_thread(initiateConnection, (client,))
   except:
      soc.close()
      client.close()
      break






