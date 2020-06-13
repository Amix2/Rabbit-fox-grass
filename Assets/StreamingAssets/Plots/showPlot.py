import sys
import time
import matplotlib.pyplot as plt


colors = ["red", "green", "blue"]

if __name__ == "__main__":
    plt.ylabel('Fitness')
    legendSet = False
    fileLen = 0
    while(plt.get_fignums()):
        print(fileLen)
        names = []
        values = {}
        title = ""
        F = open(sys.argv[2],"r") 
        currFileLen = 0
        for ind, line in enumerate(F.readlines()):
            currFileLen += 1
            line = line.rstrip()
            if(ind == 0):
                title = line
            elif(ind == 1):
                names = line.split(";")
            else:
                for valId, val in enumerate(line.split(";")):
                    values.setdefault(names[valId], []).append(float(val))
        F.close()

        if(fileLen < currFileLen):
            fileLen = currFileLen
            colorId = 0
            for key,val in values.items():
                plt.plot(val, color=colors[colorId], label=key)
                colorId += 1
            if not legendSet and colorId > 0:
                plt.legend(loc="upper left")
                legendSet = True
            plt.title(title)

        plt.pause(float(sys.argv[1]))
        #plt.show()

def on_exit(sig, func=None):
    plt.show()