import socket
import _thread
from server import *
#import interpret


#Socket Connection for game
host = '172.31.39.29'
port = 8888
backlog = 1000
size = 1024

soc = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
soc.bind((host,port))
soc.listen(backlog)

clients = {}

ClientConnection = namedtuple('ClientConnection', 'username, conn, addr')

def initiateConnection(conn):
   data = conn[0].recv(size)
   data = str(data).strip("b\'").strip("\\r\\n\'")
   while True:
      query = data.split(':')

      if query[0] == '0': # Add Account
         clients[query[1]] = ClientConnection(query[1], conn[0], conn[1]) #adds connection to connection list
         to_return = create_account(query[1], query[2])
         conn[0].send(to_return.encode())
      elif query[0] == '1': #Log In
         clients[query[1]] = ClientConnection(query[1], conn[0], conn[1])
         to_return = log_in(query[1], query[2])
         conn[0].send(to_return.encode())
      elif query[0] == '2': # Update Stats
         to_return = update_stats(query[1], query[2], query[3], query[4], query[5])
         conn[0].send(to_return.encode())
      elif query[0] == '3': # Get Stats
         to_return = get_stats(query[1])
         conn[0].send(to_return.encode())
      elif query[0] == '4': # Host Game
         to_return = host_game(query[1], query[2], query[3])
         conn[0].send(to_return.encode())
      elif query[0] == '5': # Get Games
         to_return = get_games()
         conn[0].send(to_return.encode())
      #code below here!!!!
      elif query[0] == '6': # Start Game
         to_return = start_game(query[1])
         conn[0].send(to_return.encode())
      elif query[0] == '7': # Delete Game
         to_return = destroy_game(query[1])
         conn[0].send(to_return.encode())
      elif query[0] == '8': # Disconnect Player
         to_return = log_out(query[1])
         conn[0].send(to_return.encode())
      elif query[0] == '9': # Invite Player
          #Only receive inviterusername:inviteeusername:ip:port call can invite on the username, and if it responds
          #true, find the username in clients who sent it.connection and send it
          if(can_invite(query[1]):
             to_return = "I:" + query[1] + ":"+ query[3] + ":" + query[4]
             clients[query[2]].conn.send(to_Return.encode())
             
                  #Im pretty much exactly positive I did this one wrong but?
         pass
      elif query[0] == 'A': #Send Active Game Stats to the Database
         #format: #0::1::2::
         to_return = send_game_stats
   
while True:
   client = sock.accept()
   initiateConnection(client)

client.close()
sock.close()
   


