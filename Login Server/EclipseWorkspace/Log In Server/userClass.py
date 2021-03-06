################################################################################
# Program:  Raging Hamsters Login Server
# Date:     2016 • 05 • 28
# Module:   userClass
################################################################################

################################################################################

# Class:

class User:

    def __init__(self, username: str, password: str, stats = [0,0,0,0]):
        self.username = username
        self.password = password

        self.stats = stats

    def update(self, kills: int, deaths: int, wins: int, games: int):
        for i in range(len(self.stats)):
            self.stats[i] = int(self.stats[i])  
        self.stats[0] += kills
        self.stats[1] += deaths
        self.stats[2] += wins
        self.stats[3] += games
        
