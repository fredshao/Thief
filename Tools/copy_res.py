import os
import os.path
import shutil

source_res_dir = "C:/Workspace/FatBearGames/Client/Fun/Assets/Res/"
target_res_dir = "E:/Res/"

for parent,dirnames, filenames in os.walk(source_res_dir):
	for filename in filenames:
		file_path = parent + "/" + filename
		target_dir = (parent + "/").replace(source_res_dir,target_res_dir)
		target_path = file_path.replace(source_res_dir,target_res_dir)
		if not os.path.exists(target_dir):
			os.mkdir(target_dir)
		print(file_path + "->" + target_path)
		shutil.copy(file_path,target_path)
'''
rootdir = "../Assets/"

for parent, dirnames, filenames in os.walk(rootdir):
    for dirname in dirnames:
        path = os.path.join(parent,dirname + '/.keep')
        print(path)
        fp = open(path,'w')
        fp.close()
        
'''