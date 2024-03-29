import random
import json
def getopts(argv):
  opts = {}
  while argv:
      if argv[0][0] == '-':
         opts[argv[0]] = argv[1]
      argv = argv[1:]
  return opts

if __name__ == '__main__':
   from sys import argv
   myargs = getopts(argv)
   from time import sleep
   #sleep(100)
   results =  {"i": myargs['--i'], "j": myargs['--j'], "k": myargs['--k'], "images": myargs['--images'], "accuracy": random.random()}
   print(json.dumps(results, indent = 4))