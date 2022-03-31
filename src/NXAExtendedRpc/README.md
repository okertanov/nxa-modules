NXAExtendedRpc
================

HealthCheck
---
Returns true if exdended rpc is running.

Request body:

    {
        "jsonrpc": "2.0",
        "method": "healthcheck",
        "params": [],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "success": true
        }
    }
GetBalance
---
Get token balance
First param: Address for witch to get token balance.  
Second param: Token. Pass token hash. Alternative pass strings "DVITA" or "GAS".  

Request body:

    {
        "jsonrpc": "2.0",
        "method": "getbalance",
        "params": ["NVANzkHFQ55TDiEFmLDWrZzC3suPC1BSYt", "DVITA"],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": "1",
        "result": {
            "address": "NVANzkHFQ55TDiEFmLDWrZzC3suPC1BSYt",
            "token": "GAS",
            "balance": "0"
        }
    }

NewAddress
---
Create and return new address

Request body:

    {
        "jsonrpc": "2.0",
        "method": "newaddress",
        "params": [],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "address": "NRQYKxySbY8sWnj51fgzEtXetxU3zXYHj4",
            "pubkey": "027aa1c5bdc7f2a8a848f206f29b02792917b7f30ca5d9330adcf3353154249e47",
            "privkey": "KwuH9BpkFDdPuyQLzwpYc3o1zqGeehew7xuVeQt2Lyn5XA5aUVNF",
            "scripthash": "0x75aeee47472e9c09c1e08512edae8ad1e6f03f3c"
        }
    }
GetCandidates
---
Return list of candidates  

Request body:

    {
        "jsonrpc": "2.0",
        "method": "getcandidates",
        "params": [],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "candidates": [
                {
                    "DVITA": "50",
                    "PubKey": "035997eaa3682cab4a2f701a9085ab891ad97e852b2ba30bdb5713fe62856664d7"
                }
            ]
        }
    }
RegisterCandidate
---
Create sign and relay transaction for registering new candidate for given private key.  
Pass private key.  

Request body:

    {
        "jsonrpc": "2.0",
        "method": "registercandidate",
        "params": ["L3SRadqAWNhee9jMwa4a77eAR8vSvZ6xPHVTB4q2podYLwryRWN7"],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": "0xf953b00cecae6f2aff7a052ebff084ea32c6b056bd9c222aabd858129a000c64"
    }
CreateRegisterCandidateTx
---
Create tranasction for registering new candidate.  
Pass public key.  

Request body:

    {
        "jsonrpc": "2.0",
        "method": "createregistercandidatetx",
        "params": ["035997eaa3682cab4a2f701a9085ab891ad97e852b2ba30bdb5713fe62856664d7"],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "tx": {
                "hash": "0xf5ed807e71f174872446e20cd562607767ddcf6ccffc964af635cdfdd7dbe7a5",
                "size": 134,
                "version": 0,
                "nonce": 2018658546,
                "sender": "NVANzkHFQ55TDiEFmLDWrZzC3suPC1BSYt",
                "sysfee": "100001045290",
                "netfee": "1225520",
                "validuntilblock": 165971,
                "signers": [
                    {
                        "account": "0x40a33185d9c4d19c3097eea4b72d4cd1cdd47265",
                        "scopes": "CalledByEntry"
                    }
                ],
                "attributes": [],
                "script": "DCEDWZfqo2gsq0ovcBqQhauJGtl+hSsrowvbVxP+YoVmZNcRwB8MEXJlZ2lzdGVyQ2FuZGlkYXRlDBRfngMbwo5HEd8xgpE6lR45JRBOs0FifVtS",
                "witnesses": []
            }   
        }
    }
UnregisterCandidate
---
Create sign and relay transaction for unregistering new candidate.  
Pass private key.  

Request body:

    {
        "jsonrpc": "2.0",
        "method": "unregistercandidate",
        "params": ["L3SRadqAWNhee9jMwa4a77eAR8vSvZ6xPHVTB4q2podYLwryRWN7"],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": "0xf953b00cecae6f2aff7a052ebff084ea32c6b056bd9c222aabd858129a000c64"
    }
CreateUnregisterCandidateTx
---
Create tranasction for unregistering candidate.  
Pass public key.  

Request body:

    {
        "jsonrpc": "2.0",
        "method": "createunregistercandidatetx",
        "params": ["035997eaa3682cab4a2f701a9085ab891ad97e852b2ba30bdb5713fe62856664d7"],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "tx": {
                "hash": "0x212873ff8492117601a5810aa0543d69d233a23b4d25421776cf1d9dc9500864",
                "size": 136,
                "version": 0,
                "nonce": 926116074,
                "sender": "NVANzkHFQ55TDiEFmLDWrZzC3suPC1BSYt",
                "sysfee": "3011370",
                "netfee": "1227520",
                "validuntilblock": 165979,
                "signers": [
                    {
                        "account": "0x40a33185d9c4d19c3097eea4b72d4cd1cdd47265",
                        "scopes": "CalledByEntry"
                    }
                ],
                "attributes": [],
                "script": "DCEDWZfqo2gsq0ovcBqQhauJGtl+hSsrowvbVxP+YoVmZNcRwB8ME3VucmVnaXN0ZXJDYW5kaWRhdGUMFF+eAxvCjkcR3zGCkTqVHjklEE6zQWJ9W1I=",
                "witnesses": []
            }
        }
    }
Vote
---
Create sign and relay transaction for voting.  
First param:  Vote from private key.  
Second param: Vote for public key.  

Request body:

    {
        "jsonrpc": "2.0",
        "method": "vote",
        "params": ["L3SRadqAWNhee9jMwa4a77eAR8vSvZ6xPHVTB4q2podYLwryRWN7", "0368e5c7dccb2ee8f8e1e7fd07aceae8a4b81b131b108c168551089f97ebd45dd8"],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": "0xf953b00cecae6f2aff7a052ebff084ea32c6b056bd9c222aabd858129a000c64"
    }
CreateVoteTx
---
Create tranasction for voting.  
First param:  Vote from public key.  
Second param: Vote for public key.  

Request body:

    {
        "jsonrpc": "2.0",
        "method": "createvotetx",
        "params": ["035997eaa3682cab4a2f701a9085ab891ad97e852b2ba30bdb5713fe62856664d7","0368e5c7dccb2ee8f8e1e7fd07aceae8a4b81b131b108c168551089f97ebd45dd8"],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "tx": {
                "hash": "0xf7d0576486e368166bb68cf79c9b15a080d946e2398cd09deafff4b49be7a38a",
                "size": 143,
                "version": 0,
                "nonce": 1226081487,
                "sender": "NVANzkHFQ55TDiEFmLDWrZzC3suPC1BSYt",
                "sysfee": "3011610",
                "netfee": "1234520",
                "validuntilblock": 166036,
                "signers": [
                    {
                        "account": "0x40a33185d9c4d19c3097eea4b72d4cd1cdd47265",
                        "scopes": "CalledByEntry"
                    }
                ],
                "attributes": [],
                "script": "DCEDaOXH3Msu6Pjh5/0HrOropLgbExsQjBaFUQifl+vUXdgMFGVy1M3RTC23pO6XMJzRxNmFMaNAEsAfDAR2b3RlDBRfngMbwo5HEd8xgpE6lR45JRBOs0FifVtS",
                "witnesses": []
            }
        }
    }
Unvote
---
Create sign and relay transaction for unvoting. 
First param:  Unvote from private key. 

Request body:

    {
        "jsonrpc": "2.0",
        "method": "unvote",
        "params": ["L3SRadqAWNhee9jMwa4a77eAR8vSvZ6xPHVTB4q2podYLwryRWN7"],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": "0xf953b00cecae6f2aff7a052ebff084ea32c6b056bd9c222aabd858129a000c64"
    }

CreateUnvoteTx
---
Create tranasction for unvoting.  
First param:  Unvote from public key.  

Request body:

    {
        "jsonrpc": "2.0",
        "method": "createunvotetx",
        "params": ["035997eaa3682cab4a2f701a9085ab891ad97e852b2ba30bdb5713fe62856664d7"],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": 
        {
            "tx": {
                "hash": "0xaa513b8667c71530dbaf2f1865b14e9008a19e04984d5824120f5096e0fdb640",
                "size": 109,
                "version": 0,
                "nonce": 213359459,
                "sender": "NVANzkHFQ55TDiEFmLDWrZzC3suPC1BSYt",
                "sysfee": "3011400",
                "netfee": "1200520",
                "validuntilblock": 172181,
                "signers": [
                    {
                        "account": "0x40a33185d9c4d19c3097eea4b72d4cd1cdd47265",
                        "scopes": "CalledByEntry"
                    }
                ],
                "attributes": [],
                "script": "CwwUZXLUzdFMLbek7pcwnNHE2YUxo0ASwB8MBHZvdGUMFF+eAxvCjkcR3zGCkTqVHjklEE6zQWJ9W1I=",
                "witnesses": []
            }
        }
    }
GetAccountState
---
Get state of account. 

Request body:

    {
        "jsonrpc": "2.0",
        "method": "getaccountstate",
        "params": ["NVANzkHFQ55TDiEFmLDWrZzC3suPC1BSYt"],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "voted": "NVANzkHFQ55TDiEFmLDWrZzC3suPC1BSYt",
            "amount": "50",
            "block": "166264"
        }
    }
GetFundation
---
Get fundation returns settings validators. (Publick keys)

Request body:

    {
        "jsonrpc": "2.0",
        "method": "getfundation",
        "params": [],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "fundation": [
                "0264b5ff308d12d9dcbcb929784278a666f988d8a9388e61653cf5c22cb9d05864",
                "0384be83f3b9a5f28650e255b808a248e86465c62e64a12d9a592350b224e02f6f",
                "03e0e908d30c8585c39de45056f2702778c660a561aa4aa60d18be15441e55f5c0",
                "026a7d8beae7163109aabb87e75af0ae1946a80a093a7e6fd4b36086b58527e87d"
            ]
        }
    }

SignTx
---
Signs tranasction then returns signed transaction and context. 

First param: Privte key to sign transaction with. 
Second param: Transaction json object as string or as base64 string. 

Request body:

    {
        "jsonrpc": "2.0",
        "method": "signtx",
        "params": ["L3SRadqAWNhee9jMwa4a77eAR8vSvZ6xPHVTB4q2podYLwryRWN7", "{\u0022hash\u0022:\u00220xcd30640cd12603e589d2132c32aa7675e57a72cd25c1d89ed85310291f7cfd43\u0022,\u0022size\u0022:109,\u0022version\u0022:0,\u0022nonce\u0022:1246334298,\u0022sender\u0022:\u0022NVANzkHFQ55TDiEFmLDWrZzC3suPC1BSYt\u0022,\u0022sysfee\u0022:\u00223011400\u0022,\u0022netfee\u0022:\u00221200520\u0022,\u0022validuntilblock\u0022:139892,\u0022signers\u0022:[{\u0022account\u0022:\u00220x40a33185d9c4d19c3097eea4b72d4cd1cdd47265\u0022,\u0022scopes\u0022:\u0022CalledByEntry\u0022}],\u0022attributes\u0022:[],\u0022script\u0022:\u0022CwwUZXLUzdFMLbek7pcwnNHE2YUxo0ASwB8MBHZvdGUMFF\\u002BeAxvCjkcR3zGCkTqVHjklEE6zQWJ9W1I=\u0022,\u0022witnesses\u0022:[]}"],
    "id": 1
}
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "tx": {
                "hash": "0xcd30640cd12603e589d2132c32aa7675e57a72cd25c1d89ed85310291f7cfd43",
                "size": 217,
                "version": 0,
                "nonce": 1246334298,
                "sender": "NVANzkHFQ55TDiEFmLDWrZzC3suPC1BSYt",
                "sysfee": "3011400",
                "netfee": "1200520",
                "validuntilblock": 139892,
                "signers": [
                    {
                        "account": "0x40a33185d9c4d19c3097eea4b72d4cd1cdd47265",
                        "scopes": "CalledByEntry"
                    }
                ],
                "attributes": [],
                "script": "CwwUZXLUzdFMLbek7pcwnNHE2YUxo0ASwB8MBHZvdGUMFF+eAxvCjkcR3zGCkTqVHjklEE6zQWJ9W1I=",
                "witnesses": [
                    {
                        "invocation": "DEDz9t5lHchdyeqO3din9vjhs4AkuklEXdjTrDh4QxoEXV2IxqlMmojOPmch2SaHnku68ibAytw9bUZVW9Vqddgc",
                        "verification": "DCEDWZfqo2gsq0ovcBqQhauJGtl+hSsrowvbVxP+YoVmZNdBVuezJw=="
                    }
                ]
            },
            "context": {
                "type": "Neo.Network.P2P.Payloads.Transaction",
                "data": "AFqNSUpI8y0AAAAAAIhREgAAAAAAdCICAAFlctTN0Uwtt6TulzCc0cTZhTGjQAEAOwsMFGVy1M3RTC23pO6XMJzRxNmFMaNAEsAfDAR2b3RlDBRfngMbwo5HEd8xgpE6lR45JRBOs0FifVtS",
                "items": {
                    "0x40a33185d9c4d19c3097eea4b72d4cd1cdd47265": {
                        "script": "DCEDWZfqo2gsq0ovcBqQhauJGtl+hSsrowvbVxP+YoVmZNdBVuezJw==",
                        "parameters": [
                            {
                                "type": "Signature",
                                "value": "8/beZR3IXcnqjt3Yp/b44bOAJLpJRF3Y06w4eEMaBF1diMapTJqIzj5nIdkmh55LuvImwMrcPW1GVVvVanXYHA=="
                            }
                        ],
                        "signatures": {
                            "035997eaa3682cab4a2f701a9085ab891ad97e852b2ba30bdb5713fe62856664d7": "8/beZR3IXcnqjt3Yp/b44bOAJLpJRF3Y06w4eEMaBF1diMapTJqIzj5nIdkmh55LuvImwMrcPW1GVVvVanXYHA=="
                        }
                    }
                },
                "network": 199
            }
        }
    }
RelayTx
-------
Relay signed transaction. Alternative to SendRawTransaction method. 

Param: Transaction json object as string or as base64 string. 

Request body:

    {
        "jsonrpc": "2.0",
        "method": "relaytx",
        "params": ["{\u0022hash\u0022:\u00220xcd30640cd12603e589d2132c32aa7675e57a72cd25c1d89ed85310291f7cfd43\u0022,\u0022size\u0022:217,\u0022version\u0022:0,\u0022nonce\u0022:1246334298,\u0022sender\u0022:\u0022NVANzkHFQ55TDiEFmLDWrZzC3suPC1BSYt\u0022,\u0022sysfee\u0022:\u00223011400\u0022,\u0022netfee\u0022:\u00221200520\u0022,\u0022validuntilblock\u0022:139892,\u0022signers\u0022:[{\u0022account\u0022:\u00220x40a33185d9c4d19c3097eea4b72d4cd1cdd47265\u0022,\u0022scopes\u0022:\u0022CalledByEntry\u0022}],\u0022attributes\u0022:[],\u0022script\u0022:\u0022CwwUZXLUzdFMLbek7pcwnNHE2YUxo0ASwB8MBHZvdGUMFF\\u002BeAxvCjkcR3zGCkTqVHjklEE6zQWJ9W1I=\u0022,\u0022witnesses\u0022:[{\u0022invocation\u0022:\u0022DEDTgzlPHSy\\u002BXbqjQIjGIaN8LsMMuWY0vFbrqrsYkpQYDPsXzcmWn1AnRamd/II0QJwiCRaRCI2SJ/cpAIKRsCdW\u0022,\u0022verification\u0022:\u0022DCEDWZfqo2gsq0ovcBqQhauJGtl\\u002BhSsrowvbVxP\\u002BYoVmZNdBVuezJw==\u0022}]}"],
    "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": "0xcd30640cd12603e589d2132c32aa7675e57a72cd25c1d89ed85310291f7cfd43"
    }

DeployContract
---
Deploy smart contract.   
First param:  Private key to sign deploy contract transaction.  
Second param: nef_image as base64 string  
Third param: manifest as base64 string or json string.   

Request body:

    {
        "jsonrpc": "2.0",
        "method": "createdeploycontract",
        "params": ["035997eaa3682cab4a2f701a9085ab891ad97e852b2ba30bdb5713fe62856664d7","TkVGM05l...","{\"name\":\"Team..."],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "scriptHash":"0x40a33185d9c4d19c3097eea4b72d4cd1cdd47265",
            "address":"NVANzkHFQ55TDiEFmLDWrZzC3suPC1BSYt",
            "sent":true
        } 
    }

CreateDeployContract
---
Create tranasction for deplying smart contract.   
First param:  Public key to create transaction singers.  
Second param: nef_image as base64 string  
Third param: manifest as base64 string or json string.   

Request body:

    {
        "jsonrpc": "2.0",
        "method": "createdeploycontract",
        "params": ["035997eaa3682cab4a2f701a9085ab891ad97e852b2ba30bdb5713fe62856664d7","TkVGM05l...","{\"name\":\"Team..."],
        "id": 1
    }
Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "tx": {
                "hash": "0x24e132642d32d87758aadfa1b76a005bdb3cf425c0ce16535fee121bc2a5c547",
                "size": 3532,
                "version": 0,
                "nonce": 1204185569,
                "sender": "NVANzkHFQ55TDiEFmLDWrZzC3suPC1BSYt",
                "sysfee": "1005351030",
                "netfee": "4623520",
                "validuntilblock": 326134,
                "signers": [
                    {
                        "account": "0x40a33185d9c4d19c3097eea4b72d4cd1cdd47265",
                        "scopes": "CalledByEntry"
                    }
                ],
                "attributes": [],
                "script": "DewHeyJuYW1lIj...",
                "witnesses": []
            },
            "base64txjson": "eyJoYXNoIjoiMHgy..."
        }
    }

Canonical Name Resolution
=========================

Resolve
-------

Resolves canonical name to according address.
Param: canonical name as string 

Request body:

    {
        "jsonrpc": "2.0",
        "id": 2,
        "method": "resolve",
        "params": ["@name123"]
    }

Response body:

    {
        "jsonrpc": "2.0",
        "id": 2,
        "result": {
            "cname": "@name123",
            "address": "NTcRXWqsRbR3XqtZWWVunxV7CGph27dMAY"
        }
    }

curl example:

    curl rpc.testnet.dvita.com:20332 -s -X POST -d '{"jsonrpc": "2.0","method": "resolve","params":["@name123"],"id": 1}' | jq .result

Response:

    {
      "cname": "@name123",
      "address": "NTcRXWqsRbR3XqtZWWVunxV7CGph27dMAY"
    }

Register
--------

Registers new canonical name to given address
First param: canonical name as string,
Second param: Address to add the canonical name to
Third param: signer address

Request body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "method": "register",
        "params": ["@name123","NTcRXWqsRbR3XqtZWWVunxV7CGph27dMAY","L26KYxNcUjcWUAic8UoX9GKuVAZRmuJvbaCjQbULRN8mLCX6tft5"]
    }

Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "cname": "@name123",
            "address": "0xfa4b61e682edc9a8a7393e6879ea4902ce816f54",
            "txHash": "0x2e64f7f4b36bbf6764db1f404172ae3523193c9eb088b4cb9de18cb299ee7a07"
        }
    }

curl example:

    curl rpc.testnet.dvita.com:20332 -s -X POST -d '{"jsonrpc": "2.0","method": "register","params":["@name123","NTcRXWqsRbR3XqtZWWVunxV7CGph27dMAY","L26KYxNcUjcWUAic8UoX9GKuVAZRmuJvbaCjQbULRN8mLCX6tft5"],"id": 1}' | jq .result

Response:

    {
      "cname": "@name123",
      "address": "0xfa4b61e682edc9a8a7393e6879ea4902ce816f54",
      "txHash": "0x632cfbfd97355e2ada44fd072912efc7c06c8662d00bca857c8f4d8f67e8b675"
    }

Unregister
----------
Unregisters canonical name
First param: canonical name as string,
Second param: signer address

Request body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "method": "unregister",
        "params": ["@name123","L26KYxNcUjcWUAic8UoX9GKuVAZRmuJvbaCjQbULRN8mLCX6tft5"]
    }

Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "cname": "@name123",
            "txHash": "0xfd49cf514402998d432a6b84398bb7d6b92542cf55979bd8d6c38c109ccae8c6"
        }
    }

curl example:

    curl rpc.testnet.dvita.com:20332 -s -X POST -d '{"jsonrpc": "2.0","method": "unregister","params":["@name123","L26KYxNcUjcWUAic8UoX9GKuVAZRmuJvbaCjQbULRN8mLCX6tft5"],"id": 1}' | jq .result

Response:

    {
      "cname": "@name123",
      "txHash": "0xb4f63dce905f57eb7d841ecbee41e1898e4e97e18dd990fb61b8b093663b810a"
    }

CreateRegisterTx
----------------
Create transaction for registering a canonical name to an address
First param: canonical name
Second param: address to add the canonical name to
Third param: public key

Request body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "method": "createregistertx",
        "params": ["@name1234","NZJsKhsKzi9ipzjC57zU53EVMC97zqPDKG","02ed27360f4d91c123785c1114c20a8b95db43229a933411538968292189124ff4"]
    }

Response body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "tx": {
                "hash": "0xbfa4c0a7001659c176a54c6dbda7d8aa86bd64ca8a915e6af758e720dd5c1278",
                "size": 123,
                "version": 0,
                "nonce": 599093283,
                "sender": "NZJsKhsKzi9ipzjC57zU53EVMC97zqPDKG",
                "sysfee": "6754950",
                "netfee": "1214520",
                "validuntilblock": 440632,
                "signers": [
                    {
                        "account": "0x4ded38d56567184bd9872d1e0e464985e44eee92",
                        "scopes": "CalledByEntry"
                    }
                ],
                "attributes": [],
                "script": "DBSS7k7khUlGDh4th9lLGGdl1TjtTQwJQG5hbWUxMjM0EsAfDAhyZWdpc3RlcgwUa0b4D+QqSye3q/hAo5/dafABeClBYn1bUg==",
                "witnesses": []
            },
            "base64txjson": "eyJoYXNoIjoiMHhiZmE0YzBhNzAwMTY1OWMxNzZhNTRjNmRiZGE3ZDhhYTg2YmQ2NGNhOGE5MTVlNmFmNzU4ZTcyMGRkNWMxMjc4Iiwic2l6ZSI6MTIzLCJ2ZXJzaW9uIjowLCJub25jZSI6NTk5MDkzMjgzLCJzZW5kZXIiOiJOWkpzS2hzS3ppOWlwempDNTd6VTUzRVZNQzk3enFQREtHIiwic3lzZmVlIjoiNjc1NDk1MCIsIm5ldGZlZSI6IjEyMTQ1MjAiLCJ2YWxpZHVudGlsYmxvY2siOjQ0MDYzMiwic2lnbmVycyI6W3siYWNjb3VudCI6IjB4NGRlZDM4ZDU2NTY3MTg0YmQ5ODcyZDFlMGU0NjQ5ODVlNDRlZWU5MiIsInNjb3BlcyI6IkNhbGxlZEJ5RW50cnkifV0sImF0dHJpYnV0ZXMiOltdLCJzY3JpcHQiOiJEQlNTN2s3a2hVbEdEaDR0aDlsTEdHZGwxVGp0VFF3SlFHNWhiV1V4TWpNMEVzQWZEQWh5WldkcGMzUmxjZ3dVYTBiNERcdTAwMkJRcVN5ZTNxL2hBbzUvZGFmQUJlQ2xCWW4xYlVnPT0iLCJ3aXRuZXNzZXMiOltdfQ=="
        }
    }

curl example:

    curl localhost:20332 -s -X POST -d '{"jsonrpc": "2.0","method": "createregistertx","params":["@name1234", "NZJsKhsKzi9ipzjC57zU53EVMC97zqPDKG", "02ed27360f4d91c123785c1114c20a8b95db43229a933411538968292189124ff4"],"id": 1}' | jq .result

Response:

    {
      "tx": {
        "hash": "0xb407eee687d5446035fd1a05980def18f6b7d8b024b3fe6bdc04e1ea3eba38c6",
        "size": 123,
        "version": 0,
        "nonce": 882755443,
        "sender": "NZJsKhsKzi9ipzjC57zU53EVMC97zqPDKG",
        "sysfee": "6754950",
        "netfee": "1214520",
        "validuntilblock": 440670,
        "signers": [
          {
            "account": "0x4ded38d56567184bd9872d1e0e464985e44eee92",
            "scopes": "CalledByEntry"
          }
        ],
        "attributes": [],
        "script": "DBSS7k7khUlGDh4th9lLGGdl1TjtTQwJQG5hbWUxMjM0EsAfDAhyZWdpc3RlcgwUa0b4D+QqSye3q/hAo5/dafABeClBYn1bUg==",
        "witnesses": []
      },
      "base64txjson": "eyJoYXNoIjoiMHhiNDA3ZWVlNjg3ZDU0NDYwMzVmZDFhMDU5ODBkZWYxOGY2YjdkOGIwMjRiM2ZlNmJkYzA0ZTFlYTNlYmEzOGM2Iiwic2l6ZSI6MTIzLCJ2ZXJzaW9uIjowLCJub25jZSI6ODgyNzU1NDQzLCJzZW5kZXIiOiJOWkpzS2hzS3ppOWlwempDNTd6VTUzRVZNQzk3enFQREtHIiwic3lzZmVlIjoiNjc1NDk1MCIsIm5ldGZlZSI6IjEyMTQ1MjAiLCJ2YWxpZHVudGlsYmxvY2siOjQ0MDY3MCwic2lnbmVycyI6W3siYWNjb3VudCI6IjB4NGRlZDM4ZDU2NTY3MTg0YmQ5ODcyZDFlMGU0NjQ5ODVlNDRlZWU5MiIsInNjb3BlcyI6IkNhbGxlZEJ5RW50cnkifV0sImF0dHJpYnV0ZXMiOltdLCJzY3JpcHQiOiJEQlNTN2s3a2hVbEdEaDR0aDlsTEdHZGwxVGp0VFF3SlFHNWhiV1V4TWpNMEVzQWZEQWh5WldkcGMzUmxjZ3dVYTBiNERcdTAwMkJRcVN5ZTNxL2hBbzUvZGFmQUJlQ2xCWW4xYlVnPT0iLCJ3aXRuZXNzZXMiOltdfQ=="
    }


CreateUnregisterTx
----------------
Create transaction for unregistering a canonical name from an address
First param: canonical name
Second param: public key

Request body:

    {
        "jsonrpc": "2.0",
        "id": 1,
        "method": "createunregistertx",
        "params": ["@name123","02ed27360f4d91c123785c1114c20a8b95db43229a933411538968292189124ff4"]
    }
    
Response body

    {
        "jsonrpc": "2.0",
        "id": 1,
        "result": {
            "tx": {
                "hash": "0x3659da374eb3591c702ab4d3374f66ff40a9bfd6be91aacd4d5bf294234ae3b0",
                "size": 102,
                "version": 0,
                "nonce": 712992494,
                "sender": "NZJsKhsKzi9ipzjC57zU53EVMC97zqPDKG",
                "sysfee": "3754440",
                "netfee": "1193520",
                "validuntilblock": 440641,
                "signers": [
                    {
                        "account": "0x4ded38d56567184bd9872d1e0e464985e44eee92",
                        "scopes": "CalledByEntry"
                    }
                ],
                "attributes": [],
                "script": "DAhAbmFtZTEyMxHAHwwKdW5yZWdpc3RlcgwUa0b4D+QqSye3q/hAo5/dafABeClBYn1bUg==",
                "witnesses": []
            },
            "base64txjson": "eyJoYXNoIjoiMHgzNjU5ZGEzNzRlYjM1OTFjNzAyYWI0ZDMzNzRmNjZmZjQwYTliZmQ2YmU5MWFhY2Q0ZDViZjI5NDIzNGFlM2IwIiwic2l6ZSI6MTAyLCJ2ZXJzaW9uIjowLCJub25jZSI6NzEyOTkyNDk0LCJzZW5kZXIiOiJOWkpzS2hzS3ppOWlwempDNTd6VTUzRVZNQzk3enFQREtHIiwic3lzZmVlIjoiMzc1NDQ0MCIsIm5ldGZlZSI6IjExOTM1MjAiLCJ2YWxpZHVudGlsYmxvY2siOjQ0MDY0MSwic2lnbmVycyI6W3siYWNjb3VudCI6IjB4NGRlZDM4ZDU2NTY3MTg0YmQ5ODcyZDFlMGU0NjQ5ODVlNDRlZWU5MiIsInNjb3BlcyI6IkNhbGxlZEJ5RW50cnkifV0sImF0dHJpYnV0ZXMiOltdLCJzY3JpcHQiOiJEQWhBYm1GdFpURXlNeEhBSHd3S2RXNXlaV2RwYzNSbGNnd1VhMGI0RFx1MDAyQlFxU3llM3EvaEFvNS9kYWZBQmVDbEJZbjFiVWc9PSIsIndpdG5lc3NlcyI6W119"
        }
    }

curl example:

    curl localhost:20332 -s -X POST -d '{"jsonrpc": "2.0","method": "createunregistertx","params":["@name123", "02ed27360f4d91c123785c1114c20a8b95db43229a933411538968292189124ff4"],"id": 1}' | jq .result

Response:

    {
      "tx": {
        "hash": "0x14303b42ea1974a7e80f2602dc3b7f58a202c0e8b7d849f6bb4923ed4ae7c64e",
        "size": 102,
        "version": 0,
        "nonce": 572588883,
        "sender": "NZJsKhsKzi9ipzjC57zU53EVMC97zqPDKG",
        "sysfee": "3754440",
        "netfee": "1193520",
        "validuntilblock": 440677,
        "signers": [
          {
            "account": "0x4ded38d56567184bd9872d1e0e464985e44eee92",
            "scopes": "CalledByEntry"
          }
        ],
        "attributes": [],
        "script": "DAhAbmFtZTEyMxHAHwwKdW5yZWdpc3RlcgwUa0b4D+QqSye3q/hAo5/dafABeClBYn1bUg==",
        "witnesses": []
      },
      "base64txjson": "eyJoYXNoIjoiMHgxNDMwM2I0MmVhMTk3NGE3ZTgwZjI2MDJkYzNiN2Y1OGEyMDJjMGU4YjdkODQ5ZjZiYjQ5MjNlZDRhZTdjNjRlIiwic2l6ZSI6MTAyLCJ2ZXJzaW9uIjowLCJub25jZSI6NTcyNTg4ODgzLCJzZW5kZXIiOiJOWkpzS2hzS3ppOWlwempDNTd6VTUzRVZNQzk3enFQREtHIiwic3lzZmVlIjoiMzc1NDQ0MCIsIm5ldGZlZSI6IjExOTM1MjAiLCJ2YWxpZHVudGlsYmxvY2siOjQ0MDY3Nywic2lnbmVycyI6W3siYWNjb3VudCI6IjB4NGRlZDM4ZDU2NTY3MTg0YmQ5ODcyZDFlMGU0NjQ5ODVlNDRlZWU5MiIsInNjb3BlcyI6IkNhbGxlZEJ5RW50cnkifV0sImF0dHJpYnV0ZXMiOltdLCJzY3JpcHQiOiJEQWhBYm1GdFpURXlNeEhBSHd3S2RXNXlaV2RwYzNSbGNnd1VhMGI0RFx1MDAyQlFxU3llM3EvaEFvNS9kYWZBQmVDbEJZbjFiVWc9PSIsIndpdG5lc3NlcyI6W119"
    }

