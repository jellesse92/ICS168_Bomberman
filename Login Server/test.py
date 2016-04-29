################################################################################
# Program:  Raging Hamsters Login Server
# Date:     2016 • 05 • 28
# Module:   test
################################################################################

from collections import namedtuple

import socket

################################################################################

Communication = namedtuple('communication',['socket', 'socket_in', 'socket_out'])

def test_connection() -> Communication:
    test_socket = socket.socket()
    test_socket.connect(('127.0.0.1', 8888))

    socket_in = test_socket.makefile('r')
    socket_out = test_socket.makefile('w')
    return Communication(test_socket, socket_in, socket_out)

def test_communication(channel: Communication, message: str) -> str:
    print('sending: ', message)
    write_line(channel, message)
    print('sent message')
    response = read_line(channel)
    print('recieved: ', response)
    return response
    

################################################################################

def read_line(connection: Communication) -> str:
    """
    Reads a line of text sent from the server and returns it without
    a newline on the end of it.
    """
    return connection.socket_in.readline()

def write_line(connection: Communication, line: str) -> None:
    """
    Writes a line of text to the server, including the appropriate
    newline sequence, and ensures that it is sent immediately.
    """
    connection.socket_out.write(line + '\r\n')
    connection.socket_out.flush()

################################################################################

##if __name__ == '__main__':
##    channel = test_connection()
##
##    print('Connected!')
##
##    # Create Account:
##    assert test_communication(channel, '0:Luke:123') == 'SUCCESS'
##    assert test_communication(channel, '0:Luke:123') == 'EXISTS'
##
##    print('Create Account Passed!')
##
##    # Log In:
##    assert test_communication(channel, '1:NotLuke:123') == 'FAIL'
##    assert test_communication(channel, '1:Luke:1234') == 'FAIL'
##    assert test_communication(channel, '1:Luke:123') == 'SUCCESS'
##
##    print('Log In Passed!')
##
##    # Update Stats:
##    assert test_communication(channel, '2:NotLuke:123:1:1:1:1') == 'INVALID'
##    assert test_communication(channel, '2:Luke:1234:1:1:1:1') == 'INVALID'
##    assert test_communication(channel, '2:Luke:123:1:1:1:1') == 'SUCCESS'
##
##    print('Update Stats Passed!')
##
##    # Get Stats:
##    assert test_communication(channel, '3:NotLuke:123') == 'INVALID'
##    assert test_communication(channel, '3:Luke:1234') == 'INVALID'
##    assert test_communication(channel, '4:Luke:123') == '1:1:1:1'
##
##    print('Get Stats Passed!')
##
##    print('Success!')
    




    
