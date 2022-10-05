from PIL import Image
import os, glob

for x in os.listdir():
    if x != "replace.py":
        os.rename(x, f"T{int(x.split('.')[0]) - 93}.png")

for x in os.listdir():
    if x != "replace.py":
        os.rename(x, f"{x[1:]}")

input("done")