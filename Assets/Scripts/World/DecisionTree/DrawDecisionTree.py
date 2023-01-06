import tkinter as tk
from tkinter import filedialog
import json
from treelib import Node, Tree
import graphviz


file_path = filedialog.askopenfilename()

  
f = open(file_path)
data = json.load(f)
f.close()

best = data["best"]
tree = Tree()
for item in best:
    if("parent" in item):
        tree.create_node(item["value"],  item["id"], parent=item["parent"])
    else:
        tree.create_node(item["value"],  item["id"])

tree.show()