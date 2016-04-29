################################################################################
# Program:  Raging Hamsters Login Server
# Date:     2016 • 05 • 28
# Module:   main
################################################################################

import socket
import binascii

import queries
import dataManage

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
        client = soc.accept()
        dataManage = client[0].recv(size)
        
        if dataManage:
            dataManage = str(dataManage).strip("b\'").strip("\\r\\n\'")
            print("Data: ", dataManage)
            query = dataManage.split(':')
            print('Query: ' + str(query))
            
            if query[0] == '0':
                call = queries.create_account(database, query[1], query[2])
                print(call[0])
                client[0].send(call[0].encode())
                database = call[1]
                dataManage.save_database(database) # Somehow it believes dataManage to be a str and not a module...?
                
            elif query[0] == '1':
                client[0].send(queries.validate(database, query[1], query[2]))
                
            elif query[0] == '2':
                call = queries.update_stats(database, query[1], query[2], query[3],
                                            query[4], query[5], query[6])
                client.send(call[0].encode())
                database = call[1]
                dataManage.save_database(database)
                
            elif query[0] == '3':
                client[0].send(queries.get_stats(database, query[1], query[2]).encode())
                
        client[0].close()

if __name__ == '__main__':
    users = dataManage.load_database()
    LogIn_Server(users)


    
