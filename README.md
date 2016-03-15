#Virgil Sync

Virgil Sync encrypts user data using Virgil Crypto elliptic curve library, Virgil Public and Private key infrastructure APIs and Dropbox Rest API

Virgil Sync gets the list of files from application folder on Dropbox and synchronizes them with the files from mapped folder on the user’s machine. Files from the user’s selected folder are encrypted with the user’s public key and file names are hashed with SHA256. 

The general scheme:
![Virgil Sync Process](https://github.com/VirgilSecurity/virgil-sync/blob/master/Setup/virgil-sync-scheme.png "Virgil Sync Process")
