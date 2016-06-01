import mysql.connector
from mysql.connector import connection

gameconnection = connection.MySQLConnection(user='root', password='password', host='127.0.0.1', database='raginghamsters')




#Add a user to the database
def create_account(username: str, password: str) -> str:
   cursor = gameconnection.cursor()
   userInfo = ( username, password)
   accQuery = ("INSERT INTO users " "(username, password)" "VALUES (%s, %s)")
   try:
      cursor.execute(accQuery, userInfo)
      gameconnection.commit()
   except mysql.connector.Error as e:
      print("Error (create_account): Account Exists")
      cursor.close()
      return "EXISTS"
   else:
      cursor.close()
      return "SUCCESS"



#Log user into Server
def log_in(username: str, password: str) -> str:
   cursor = gameconnection.cursor()
   userInfo = (username, password, "OFFLINE")
   accQuery = ("SELECT username, password FROM users " "WHERE username = %s AND password = %s AND status=%s")
   cursor.execute(accQuery, userInfo)
   response = cursor.fetchall()
   if len(response) == 0:
      cursor.close()
      return "FAIL"
   else:
      userInfo = ("ONLINE", username, password)
      online = ("UPDATE users SET status=%s WHERE username=%s AND password=%s")
      try:
         cursor.execute(online, userInfo)
         gameconnection.commit()
      except:
         print("Error (Log_In): Incorrect Username or Password")
         cursor.close()
         return "FAIL"
      else:
         cursor.close()
         return "SUCCESS"


#Log user out from Server
def log_out(username: str):
   to_return = _changePlayerStatus("OFFLINE", username)
   if to_return == 0:
      return "FAIL"
   else:
      return "SUCCESS"
  
   

def update_stats(user: str, k: str, d: str, w: str, g: str) -> str:
   cursor = gameconnection.cursor()
   userInfo = (k, d, w, g, user)
   statUpdate = ("UPDATE users SET kills = kills + %s, deaths = deaths + %s, wins = wins + %s, games= games + %s WHERE username = %s")
   try:
      cursor.execute(statUpdate, userInfo)
      gameconnection.commit()
   except:
      print("Error (Update Stats): Failed to Update Player Stats")
      cursor.close()
      return "FAIL"
   else:
      cursor.close()
      return "SUCCESS"

#Get Player Stats
def get_stats(user: str) -> str:
   cursor = gameconnection.cursor()
   results = ""
   try:
      cursor.execute("SELECT kills, deaths, wins, games FROM users WHERE username = %s", (user,))
   except:
      print("Error (Get Stats): Failed to Retrieve Stats from Database")
      cursor.close()
      return "FAIL"
   else:
      if cursor.rowcount == 0:
         print("Error (GET_STATS): User Doesnt Exist")
         cursor.close()
         return "FAIL"
      rows = cursor.fetchall()
      for row in rows:
         results = user + ":{}:{}:{}:{}".format(row[0], row[1],row[2],row[3])
         cursor.close()
         return results

#Get Currently available games.
def get_games()->str:
   cursor = gameconnection.cursor()
   results = ""
   try:
      cursor.execute("SELECT host, ipaddr, port FROM games")
      rows = cursor.fetchall()
      if len(rows) <= 0:
         return "NONE"
      for row in rows:
         results += "{}:{}:{} ".format(row[0], row[1],row[2])
   except:
      return "NONE"
   cursor.close()
   return results

#Host a New Game
def host_game(user: str, ip_addy: str, portno: str)->str:
   cursor = gameconnection.cursor()
   userInfo=(user, ip_addy, portno, user)
   hostgame = "INSERT INTO games " "(host, ipaddr, port, player1)" "VALUES (%s, %s, %s, %s)"
   _changePlayerStatus("GAME", user)
   try:
      cursor.execute(hostgame, userInfo)
      gameconnection.commit()
   except:
      print("Error (host_Game): Failed to add game to Database")
      cursor.close()
      return "FAIL"
   else:
      _changePlayerStatus("GAME", (user,))
      cursor.close()
      return "SUCCESS"

##Join Game Belonging to host user
def join_game(user: str, hostip:str, hostport: str)->str:
   print("Join Game Called")
   cursor = gameconnection.cursor()
   userInfo=(user, hostip, hostport)
   gameInfo = (hostip, hostport, "INACTIVE")
   players = ["player2", "player3", "player4"]
   selectGame = "SELECT * FROM games WHERE ipaddr= %s AND port = %s AND status=%s"
   print("Trying To Execute")
   try:
      cursor.execute(selectGame, gameInfo)
   except:
      print("error Raised.")
   rows = cursor.fetchall()
   if len(rows) == 0:
      print("Fail")
      cursor.close()
      return 'DNE'
   else:
      print(rows)
      for row in rows:
         if row[-1] == 3 or row[-2] == 'ACTIVE':
            cursor.close()
            return 'FULL'
         else:
            i = 4
            for player in players:
               if row[i] == "EMPTY":
                  userJoin = (user, hostip, hostport)
                  joinGame = "UPDATE games SET " + player + " = %s, players = players+1 WHERE ipaddr=%s AND port = %s"
##                  cursor.execute(joinGame, userJoin)
##                  gameconnection.commit()
                  try:
                     cursor.execute(joinGame, userJoin)
                     gameconnection.commit()
                  except:
                     print("Error (Join Game): Failed to Update Game")
                     cursor.close()
                     return "FAIL"
                  else:
                     if _changePlayerStatus("GAME", user) == 0:
                        cursor.close()
                        return "FAIL"
                     cursor.close()
                  return "SUCCESS"
               i = i + 1

#Change PLayer Status:
def _changePlayerStatus(stat: str, user: str)->int:
   cursor = gameconnection.cursor()
   userInfo = (stat, user)
   changeStatus = "UPDATE users SET status=%s WHERE username=%s"
   try:
      cursor.execute(changeStatus, userInfo)
      gameconnection.commit()
   except:
      print("Error (_changePlayerStatus): Failed to Update Player")
      cursor.close()
      return 0
   else:
      cursor.close()
      return 1

#Change games status from Inactive to Active to indicate game is started
def start_game(ip: str)->str:
   cursor = gameconnection.cursor()
   updateGame = ("ACTIVE", ip)
   startGame = "UPDATE games SET status=%s WHERE ipaddr = %s"
   try:
      cursor.execute(startGame, updateGame)
      gameconnection.commit()
   except:
      print("Error (Start Game): Failed to Update Player Status")
      cursor.close()
      return "FAIL"
   else:
      cursor.close()
      return "SUCCESS"

#delete game from database. Game has ended
def destroy_game(ip: str)->str:
   cursor = gameconnection.cursor()
   players = [0, 1, 2, 3]
   getPlayers = "SELECT player1, player2, player3, player4 FROM games WHERE ipaddr = %s"
   cursor.execute(getPlayers, (ip,))
   results = cursor.fetchall()
   for player in players:
      if _changePlayerStatus("ONLINE", results[0][player]) == 0:
         continue
   deleteGame = "DELETE FROM games WHERE ipaddr = %s"
   deleteStats = "DELETE FROM gamestats WHERE gameip = %s"
   try:
      cursor.execute(deleteGame, (ip,))
      gameconnection.commit()
   except:
      print("Error (Destroy_Game): Failed to Delete Game From Database")
      cursor.close()
      return "FAIL"
   else:
      try:
         cursor.execute(deleteStats, (ip,))
         gameconnection.commit()
      except:
         print("Error (Destroy_Game): Failed to Delete Game Stats from Database")
         cursor.close()
         return "FAIL"
      else:
         return "SUCCESS"


#Send game stats to database. Game is active.
def send_game_stats(ip: str, p1: str, p2: str, p3: str, p4: str)->str:
   #gamestats[playernumber] = (score, status)
   cursor = gameconnection.cursor()

   pnfo = (p1, p2, p3, p4)
   pnfo = (ip,) + pnfo + pnfo;
   to_update = ("INSERT INTO gamestats (gameip, p1score, p2score, p3score, p4score)"
                "VALUES (%s, %s, %s, %s, %s) ON DUPLICATE KEY UPDATE p1score = %s, "
                "p2score = %s, p3score = %s, p4score = %s")

   try:
      cursor.execute(to_update, pnfo)
      gameconnection.commit()
   except:
      print("Error (send_game_stats): Failed to update game stats.")
      cursor.close()
   else:
      cursor.close()
      return "SUCCESS"
      
      

def can_invite(user: str)-> bool:
   cursor = gameconnection.cursor()
   queryUser = "SELECT status FROM users WHERE username = %s"
   cursor.execute(queryUser, (user,))
   status = cursor.fetchall()
   if status[0][0] == "ONLINE":
      cursor.close()
      return True
   else:
      cursor.close()
      return False

def get_users()->str:
   cursor = gameconnection.cursor()
   cursor.execute("SELECT username FROM users WHERE status='ONLINE'")
   rows = cursor.fetchall()
   results = ""
   for row in rows:
      results = "{} ".format(row[0], row[1],row[2])
   cursor.close()
   if results == "":
      results = "NONE"
   return results
   
def leave_game(user:str)->str:
   _changePlayerStatus("ONLINE", user)
   return "SUCCESS"
   
