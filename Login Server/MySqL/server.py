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

ClientConnection = namedtuple('ClientConnection', 'username, conn, invite')
#added invite to check for invitations


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
            clients[query[1]] = ClientConnection(query[1], conn, "NONE")
            #adds connection to connection list invite = 0 for no invite
            to_return = create_account(query[1], query[2])
            conn.sendall(to_return.encode())
            print("SENT: " + to_return)
         elif query[0] == '1': #Log In
            print("LOGGING IN AS..." + query[1])
            clients[query[1]] = ClientConnection(query[1], conn, "NONE")
            to_return = log_in(query[1], query[2])
            print("LOGIN: " + to_return)
            conn.send(to_return.encode())
         elif query[0] == '2': # Update Stats
            print("UPDATING GAME STATS")
            print(data)
            print(query[5])
            to_return = update_stats(query[1], query[2], query[3], query[4], query[5][:-1])
            conn.send(to_return.encode())
         elif query[0] == '3': # Get Stats
            print("GETTING USER STATS")
            if(query[1][-1] == "$"):
               query[1] = query[1][:-1]
            to_return = get_stats(query[1])
            conn[0].send(to_return.encode())
         elif query[0] == '4': # Host Game
            print("Host Game called: " + data)
            if(query[3][-1] == "$"):
               query[3] = query[3][:-1]
            to_return = host_game(query[1], query[2], query[3])
            conn.send(to_return.encode())
         elif query[0] == '5': # Get Games
            print(data)
            print("RETURNING INACTIVE GAMES")
            to_return = get_games()
            conn.send(to_return.encode())
         #code below here!!!!
         elif query[0] == '6': # Start Game
            if(query[1][-1] == "$"):
               query[1] = query[1][:-1]
            print("STARTING GAME")
            to_return = start_game(query[1])
            conn.send(to_return.encode())
         elif query[0] == '7': # Delete Game
            print("DELETING GAME")
            to_return = destroy_game(query[1][:-1])
            conn.send(to_return.encode())
         elif query[0] == '8': # Disconnect Player
            print("Player Disconnecting... " + query[1][:-1])
            to_return = log_out(query[1][:-1])
            print("Returning.... " + to_return)
            conn.send(to_return.encode())
            exit()
            #to_return = disconnect(conn)
         elif query[0] == '9': # Invite Player
            #"9:INVITED:HOST:IP:PORT"
            print("Inviting Player")
            if can_invite(query[1]):
               print("got HERE?")
               to_return = "I:" + query[2] + ":"+ query[3] + ":" + query[4][:-1]
               to_client = "SENT"
               clients[query[1]] = clients[query[1]]._replace(invite = to_return);
               conn.send(to_client.encode())
            else:
               to_client = "UNAVAILABLE"
               conn.send(to_client.encode())
         elif query[0] == 'A': #Send Active Game Stats to the Database
            print("Updating Game Stats...")
            conn.send(send_game_stats(query[1], query[3], query[4], query[5], query[6][:-1]).encode())
         elif query[0] == 'B': #Have user Leave Game Early
            if(query[1][-1] == "$"):
               query[1] = query[1][:-1]
            leave_game(query[1])
         elif query[0] == 'C': #C for CHECKIN :)
            if(query[1][-1] == "$"):
               query[1] = query[1][:-1]
            print(clients[query[1]].invite)
            conn.send(clients[query[1]].invite.encode())
         elif query[0] == 'J': #Join Game
            if(query[3][-1] == "$"):
               query[3] = query[3][:-1]
            to_return = join_game(query[1], query[2], query[3])
            print("Joining Game: " + to_return)
            conn.send(to_return.encode())
         elif query[0] == 'U': #Get all Users
            print("Getting users")
            to_return = get_users()
            print("Sending Users...." + to_return)
            conn.send(to_return.encode())
         elif query[0] == 'Q':
            if(query[1][-1] == "$"):
               query[1] = query[1][:-1]
            conn.send(query[1].encode())
            print("Calling disconnect...")
            to_return = disconnect(conn)
         #print(to_return)
   except Exception as e:
      print("Error Thrown." + str(e))
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






