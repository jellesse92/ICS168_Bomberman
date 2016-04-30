################################################################################
# Program:  Raging Hamsters Login Server
# Date:     2016 • 05 • 28
# Module:   queries
################################################################################

import userClass

################################################################################

# Queries:

def create_account(database: dict, username: str, password: str) -> (str, dict):
    """
    Creates an account with given username and password if possible.
    Returns a tuple with a string representing the success of the function
    (EXISTS/SUCCESS) and a dict representing the changed dictionary.
    """
    if username in database.keys():
        return ('EXISTS', database)
    else:
        database[username] = userClass.User(username, password)
        return ('SUCCESS', database)

def validate(database: dict, username: str, password: str) -> str:
    """
    Checks database for the username and password combination.
    Returns a string representing the result of the search (FAIL/SUCCESS)
    """
    if username in database:
        if database[username].password == password:
            return 'SUCCESS'
    return 'FAIL'

def update_stats(database: dict, username: str, password: str, kills: int,
                 deaths: int, wins: int, games: int) -> (str, dict):
    """
    Updates the given user's statistics by incrementing by the numbers given.
    Returns a tuple with a string representing the success of the function
    (INVALID/SUCCESS) and a dict representing the changed dictionary.
    """
    if username in database:
        if database[username].password == password:
            database[username].update(kills, deaths, wins, games)
            return ('SUCCESS', database)
    return ('INVALID', database)

def get_stats(database: dict, username: str, password: str) -> str:
    """
    Checks database for the username and password combination.
    Returns a string representing the stats of the user, INVALID if
    the username/password incorrect.
    """
    if username in database:
        if database[username].password == password:
            stats = database[username].stats
            return (str(stats[0]) + ':' + str(stats[0]) + ':' + \
                    str(stats[0]) + ':' + str(stats[0]) + ':')
    return 'INVALID'




    
