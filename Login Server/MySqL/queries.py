import mysql.connector
from mysql.connector import connection

gameconnection = connection.MySQLConnection(user='root', password='password', host='127.0.0.1', database='raginghamsters')
cursor = gameconnection.cursor()



#Add a user to the database
def create_account(username: str, password: str) -> str:

   userInfo = ( username, password)
   accQuery = ("INSERT INTO users " "(username, password)" "VALUES (%s, %s)")
   try:
      cursor.execute(accQuery, userInfo)
      cursor.commit()
   except mysql.connector.Error as e:
      return "EXISTS"
   else:
      return "SUCCESS"

assert create_account("Test", "123") == "EXISTS"

#Log user into Server
def log_in(username: str, password: str) -> str:
   userInfo = (username, password, "OFFLINE")
   accQuery = ("SELECT users, password FROM users " "WHERE username = %s AND password = %s AND status=%s")
   cursor.execute(accQuery, userInfo)
   if not cursor.rowcount:
      return "FAIL"
   else:
      userInfo = ("ONLINE", username, password)
      online = ("UPDATE users SET status=%s WHERE username=%s AND password=%s")
      try:
         cursor.execute(online, userInfo)
         cursor.comit()
      except:
         return "FAIL"
      else:
         return "SUCCESS"

assert log_in("Test", "123") == "SUCCESS"

#Log user out from Server
def log_out(username: str):
   userInfo("OFFLINE",username)
   try:
      cursor.execute("UPDATE users SET status=%s WHERE username=%s", userInfo)
      cursor.comit()
   except:
      pass
   else:
      return "SUCCESS"
   
assert log_out("Test") == "SUCCESS"

def update_stats(user: str, k: int, d: int, w: int, g: int) -> str:
   userInfo = (str(k), str(d), str(w), str(g), user)
   statUpdate = ("UPDATE users set kills = kills + %s, deaths = deaths + %s, wins = wins + %s, games= games + %s, WHERE username = %s")
   try:
      cursor.execute(statUpdate, userInfo)
   except:
      return "FAIL"
   else:
      return "SUCCESS"

#Get Player Stats
def get_stats(user: str) -> str:
   results = ""
   try:
      cursor.execute("SELECT kills, deaths, wins, games FROM users WHERE username = %s", user)
   except:
      return "FAIL"
   else:
      rows = cursor.fetchall()
      for row in rows:
         results = user + ":" + row['kills'] + ":" + row['deaths'] + ":" + row['wins'] + ":" + row['games']
         return results

#Get Currently available games.
def get_games()->str:
   
   pass

#Host a New Game
def host_game(user: str, ip_addy: str, portno: str)->str:
   userInfo=(user, ip_addy, portno, user)
   hostgame = "INSERT INTO games " "(host, ipaddr, port, player1)" "VALUES %s, %s, %s, %s"
   try:
      cursor.execute(hostgame, userInfo)
      cursor.commit()
   except:
      return "FAIL"
   else:
      return "SUCCESS"

#Join Game Belonging to host user
def join_game(user: str, hostuser: str, hostip:str, hostport: str)->str:
   userInfo=(user, hostuser, hostip, hostport)
   gameinfo = (hostuser, hostip, hostport)
   players = ["player2", "player3", "player4"]

   selectGame = "SELECT * FROM games WHERE host = %s AND ipaddr= %s AND port = %s AND status=INACTIVE"

   cursor.execute(selectGame, gameInfo)
   if cursor.rowcount == 0:
      return "DNE"
   else:
      rows = cursor.fetchall()
      if rows['players'] == 3 or row['status'] == 'ACTIVE':
         return 'FULL'
      else:
         for player in players:
            if rows[player] == "EMPTY":
               joinGame = "UPDATE games SET " + player + " = %s WHERE host=%s AND ipaddr=%s AND port = %s"
               try:
                  cursor.execute(joinGame, userInfo)
                  cursor.comit()
               except:
                  return "FAIL"
               else:
                  return "SUCCESS"      

#Change games status from Inactive to Active to indicate game is started
def start_game(ip: str)->str:
   updateGame = ("ACTIVE", ip)
   startGame = "UPDATE games SET status=%s WHERE ipaddr = %s"
   try:
      cursor.execute(startGame, updateGame)
      cursor.comit()
   except:
      print("Error (Start Game): Failed to Update Player Status")
      return "FAIL"
   else:
      return "SUCCESS"

#delete game from database. Game has ended
def destroy_game(ip: str)->str:
   players = ["player1","player2","player3", "playe4"]
   getPlayers = "SELECT player1, player2, player3, player4 FROM games WHERE ipaddr = %s"
   cursor.execute(getPlayers, ip)
   results = cursor.fetchall()
   for player in players:
      try:
         userinfo = ("ONLINE", row[player])
         endGame = "UPDATE users SET status=%s WHERE user=%s"
         cursor.execute(endGame, userinfo)
         cursor.comit()
      except:
         print("Error (Destroy_Game): Failed to Update Player Status")
         continue
   deleteGame = "DELETE FROM games WHERE ipaddr = %s"
   try:
      cursor.execute(deleteGame, ip)
      cursor.comit()
   except:
      return "FAIL"
      print("Error (Destroy_Game): Failed to Delete Game From Database")
   else:
      return "SUCCESS"


#Send game stats to database. Game is active.
def send_game_stats(ip: str, gamestats: dict)->str:
   i = len(gamestats)
   for key,

def can_invite(user: str)-> bool:
   pass
   
   

   
