
NXABlockListener
================

Config
------

Example:

    {
      "PluginConfiguration": {
        "TurnedOn": true,
        "AutoStart": false,
        "Network": 199,
        "StartBlock": 0,
        "RMQ": {
          "VirtualHost": "/",
          "RMQHost": [ "localhost:5672" ],
          "Username": "guest",
          "Password": "guest",
          "ConfirmSelect": true,
          "Exchanges": [
            {
              "type": "block",
              "name": "org.nxa.blockchain.listener.block",
              "exchange": true
            },
            {
              "type": "transaction",
              "name": "org.nxa.blockchain.listener.transaction",
              "exchange": true
            },
            {
              "type": "transfer",
              "name": "org.nxa.blockchain.listener.transfer",
              "exchange": true
            },
            {
              "type": "scdeployment",
              "name": "org.nxa.blockchain.listener.scdeployment",
              "exchange": true
            }
          ]
        },
        "DB": {
          "Path": "/nxa-node-data/RMQData_Test_{0}"
        }
      }
    }

PluginConfiguration  
TurnedOn:       If not true plugin will not turn on. This is set to false if no plugin configuration provided.  
AutoStart:      To start block sync automatically on node start.  
Network:        NXA network. If it does not match node network plugin will not start. (Same as with "Active")  
StartBlock:     Last sent block to RabbitMQ. If 0 starts to send with block index 1. Set to -1 if there is need to send genesis block.  
  
RMQ  
VirtualHost:    RabbitMQ virtual host.
RMQHost:        Possible RabbitMQ hosts. If one doesn't work will try to connect to next one. (fallback options)
Username:       RabbitMQ username
Password:       RabbitMQ password.
ConfirmSelect:  Flag to check if publish to RabbitMQ successful before continue.

Exchanges
type:           Witch visitor this is. Possible values block, transaction, transfer, scdeployment
name:           RMQ exchange or queue name where to announce data.
exchange:       Use RMQ exchange or rmq queue.

DB   
Path:           persistence location (leveldb)         

Console
-------

start blocklistener:    starts blocklistener            (If there is no active task with type=blocklistener creates one and starts it)  
stop blocklistener:     stops blocklistener   
show blocklistener:     show blocklistener console output   

RPC
---

Main way to interact with NXABlocklistener plugin

IsWorking
---
Returns true if NXABlocklistener plugin is turned on. Additionally returns if blocklistner is active. 

Request body:

    {
        "jsonrpc": "2.0",
        "method": "isworking",
        "params": [],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "message": "NXABlockListener active: False",
            "success": true
        }
    }
StartBlockListener
---
Returns true if block listener started or allready running.  
(If there is no active task with type=blocklistener creates one and starts it)

Request body:

    {
        "jsonrpc": "2.0",
        "method": "startblocklistener",
        "params": [],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "success": true,
            "message": "Block listener started"
        }
    }
StopBlockListener
---
Returns true if block listener stopped.

Request body:

    {
        "jsonrpc": "2.0",
        "method": "stopblocklistener",
        "params": [],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "success": true,
            "message": "Block listener stopped"
        }
    }
GetTaskList
---
Returns list of blocklistener tasks.  
Possible parameters taskstate and tasktype. If no parameter provided returns all tasks.  
taskstate: None, Active, Finished, Canceled, Error  
tasktype: BlockListener, Search  

Request body:

    {
        "jsonrpc": "2.0",
        "method": "gettasklist",
        "params": [{"taskstate":["Active","Canceled"],"tasktype":["BlockListener",1]}],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": "1",
        "result": {
            "success": true,
            "message": [
                {
                    "Id": "d6bc3345-c021-4519-b939-cba6e9a71a83",
                    "TaskState": "Finished",
                    "TaskType": "Search",
                    "ActiveBlock": 200000,
                    "TaskParameters": {
                        "SearchJSON": {
                            "transaction": {},
                            "transfer": {},
                            "scdeployment": {}
                        },
                        "FromBlock": 0,
                        "ToBlock": 200000
                    }
                },
                {
                    "Id": "bd3e9e89-f04c-4520-9977-18c88b16771b",
                    "TaskState": "Active",
                    "TaskType": "BlockListener",
                    "ActiveBlock": 17204,
                    "TaskParameters": {
                        "SearchJSON": {},
                        "FromBlock": 0,
                        "ToBlock": 0
                    }
                }
            ]
        }
    }
GetTask
---
Retrieve specific task by guid.

Request body:

    {
        "jsonrpc": "2.0",
        "method": "gettask",
        "params": ["39c41f43-b9d4-4299-9bfb-8a9c974ee293"],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "success": true,
            "message": {
                "Id": "39c41f43-b9d4-4299-9bfb-8a9c974ee293",
                "TaskState": "Active",
                "TaskType": "Search",
                "ActiveBlock": 320378,
                "TaskParameters": {
                    "SearchJSON": {
                        "scdeployment": {}
                    },
                    "FromBlock": 0,
                    "ToBlock": 500000000
                }
            }
        }
    }
StartTask
---
Start task by guid. Returns started task.

Request body:

    {
        "jsonrpc": "2.0",
        "method": "starttask",
        "params": ["39c41f43-b9d4-4299-9bfb-8a9c974ee293"],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "success": true,
            "message": {
                "Id": "39c41f43-b9d4-4299-9bfb-8a9c974ee293",
                "TaskState": "Active",
                "TaskType": "Search",
                "ActiveBlock": 321044,
                "TaskParameters": {
                    "SearchJSON": {
                        "scdeployment": {}
                    },
                    "FromBlock": 0,
                    "ToBlock": 500000000
                }
            }
        }
    }
StopTask
---
Cancel task by guid. Returns stopped task.

Request body:

    {
        "jsonrpc": "2.0",
        "method": "stoptask",
        "params": ["39c41f43-b9d4-4299-9bfb-8a9c974ee293"],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "success": true,
            "message": {
                "Id": "39c41f43-b9d4-4299-9bfb-8a9c974ee293",
                "TaskState": "Canceled",
                "TaskType": "Search",
                "ActiveBlock": 321049,
                "TaskParameters": {
                    "SearchJSON": {
                        "scdeployment": {}
                    },
                    "FromBlock": 0,
                    "ToBlock": 500000000
                }
            }
        }
    }
CreateTask
---
Create new task. Returns created task.  
Parameters:
TaskType: Search or Blocklistener. 
* Blocklistener - is default running task and should not be created without reason. Blocklistener task doesnt require extra parameters and will ignore TaskParameters field. You can specify ActiveBlock.  
* Search - Search task has end conditions and you can specify what exactly you need to find. Return Id guid will be RMQ queue name where results will be announced.  

ActiveBlock: Block on witch task is right now.  
TaskParameters: Specify Search task search conditions and end conditions.  
* FromBlock - block where to start search task
* ToBlock - block when serach task will end. (If larger than current chain index will run till chain reaches configured end block)
* SearchJSON - specify what to announce to RMQ.

Example1:  
"SearchJSON":{"block":{"hash": "0xf34593eb02437a099cf7f703e23dcf6f585534caa8d1769b6df250abb2f6d718"},"transaction":{},"transfer":{},"scdeployment":{}}  
This expects to announce blocks with specified hash (Witch will be one block) and all transactions, all transfers and all scdeployments.

Example2:  
"SearchJSON":{ "transfer":{},"scdeployment":{"name": "Team11Token"}}  
This expects to announce all transfers and scdeployments where name property is "Team11Token".

!!! For search task return Id will be RMQ queue name where results will be announced !!!

Request body:

    {
        "jsonrpc": "2.0",
        "method": "createtask",
        "params": [{"TaskType":"Search","ActiveBlock":45000,"TaskParameters":{"FromBlock":0,"ToBlock":200000,"SearchJSON":{"block":{"hash": "0xf34593eb02437a099cf7f703e23dcf6f585534caa8d1769b6df250abb2f6d718"},"transaction":{},"transfer":{},"scdeployment":{}}}}],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "success": true,
            "message": {
                "Id": "0dd10a68-5e23-40dc-9be1-67541e16b98d",
                "TaskState": "Active",
                "TaskType": "Search",
                "ActiveBlock": 45000,
                "TaskParameters": {
                    "SearchJSON": {
                        "block": {
                            "hash": "0xf34593eb02437a099cf7f703e23dcf6f585534caa8d1769b6df250abb2f6d718"
                        },
                        "Transaction": {},
                        "Transfer": {},
                        "SCDeployment": {}
                    },
                    "FromBlock": 0,
                    "ToBlock": 200000
                }
            }
        }
    }