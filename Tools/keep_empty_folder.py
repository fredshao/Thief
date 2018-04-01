import os
import os.path
rootdir = "../Assets/"

for parent, dirnames, filenames in os.walk(rootdir):
    for dirname in dirnames:
        path = os.path.join(parent,dirname + '/.keep')
        print(path)
        fp = open(path,'w')
        fp.close()
        
