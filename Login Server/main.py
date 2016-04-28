################################################################################
# Program:  Raging Hamsters Login Server
# Date:     2016 • 05 • 28
# Module:   main
################################################################################

import socket

import queries
import data

################################################################################

# Main Server

def LogIn_Server(database: dict):
    # Socket Binding & Listening:
    host = ''
    port = 8888
    backlog = 5
    size = 1024

    soc = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    soc.bind((host,port))
    soc.listen(backlog)

    # Request Handling:
    while True:
        client, address = soc.accept()
        data = client.recv(size)
        if data:
            query = data.split(':')
            if query[0] == '0':
                call = create_account(database, query[1], query[2])
                client.send(call[0])
                database = call[1]
                data.save_database(database)
            elif query[0] == '1':
                client.send(validate(database, query[1], query[2]))
            elif query[0] == '2':
                call = update_stats(database, query[1], query[2], query[3],
                                    query[4], query[5], query[6])
                client.send(call[0])
                database = call[1]
                data.save_database(database)
            elif query[0] == '3':
                client.send(get_stats(database, query[1], query[2]))
        client.close()

if __name__ == '__main__':
    users = data.load_database()
    LogIn_Server(users)


    
