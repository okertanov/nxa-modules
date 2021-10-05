NXA Core Modules
================


NXAExtendedRpc
================

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
            "candidates": []
        }
    }

