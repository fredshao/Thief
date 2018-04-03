import os
import platform
import paramiko
from scp import SCPClient
import time

host = '192.168.3.24'
port = 22
username = 'pi'
password = 'raspberry'

print('Connecting ',host,':',port, 'with username ',username,'and password ',password)
ssh = paramiko.SSHClient()
ssh.set_missing_host_key_policy(paramiko.AutoAddPolicy())
ssh.connect(host,port,username,password)

scp = SCPClient(ssh.get_transport())

os.chdir('../../Bundles/Windows/')
for root,dirs,files in os.walk('./'):
	for file in files:
		print('Uploading: ',file)
		scp.put(file, remote_path = '/var/www/html/Thief/Bundles/Windows')


os.chdir('../../Bundles/iOS/')
for root,dirs,files in os.walk('./'):
	for file in files:
		print('Uploading: ',file)
		scp.put(file, remote_path = '/var/www/html/Thief/Bundles/iOS')

print('Upload Successful!')
scp.close()

time.sleep(2)