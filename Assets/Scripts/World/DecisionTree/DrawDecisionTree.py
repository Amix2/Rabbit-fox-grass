import tkinter as tk
from tkinter import filedialog
import json
from treelib import Node, Tree
import graphviz


file_path = filedialog.askopenfilename()

print(file_path)
f = open(file_path)
data = json.load(f)
f.close()

bestList = data["best"]
for best in bestList:
    tree = Tree()
    for item in best:
        if("parent" in item):
            tree.create_node(item["value"],  item["id"], parent=item["parent"])
        else:
            tree.create_node(item["value"],  item["id"])
    tree.show()
