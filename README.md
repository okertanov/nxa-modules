NXA Core Modules
================


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
Second param: unvote for public key. 

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
Create tranasction for voting. 
First param:  Vote from public key. 
Second param: Vote for public key. 

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
        "result": {
        "tx": {
            "hash": "0x163b8f1b34c03bbc66dbab56628ad0df6474d0e28be9e3b0413772ae12268b5e",
            "size": 109,
            "version": 0,
            "nonce": 1098350121,
            "sender": "NVANzkHFQ55TDiEFmLDWrZzC3suPC1BSYt",
            "sysfee": "3011400",
            "netfee": "1200520",
            "validuntilblock": 166047,
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
        "error": {
            "code": -500,
            "message": "No vote record!"
        }
    }
GetFundation
---
Get fundation. 

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
Returns true if exdended rpc is running. 
First param: Privte key to sign transaction with.
Second param: Transaction json object as string.

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
---
Relay signed transaction. Alternative to SendRawTransaction method.
Param: Transaction json object as string. 

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
