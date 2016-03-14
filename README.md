# ActionScript Client API #

## Command structure ##

Command is a json object with two required field

	{
		"command" : "command name",
		"args" : "arguments value"
	}

## Command list ##

- **echo**

Request

Echo command will return text passed as args value to the caller

	{
		"command" : "echo",
		"args" : "string literal which will be returned in stdout"
	}

Response

	string literal which will be returned in stdout

- **login**

Login command will show login window of running control panel instance if user has not been logged in.

 

Request

	{
		"command" : "login",
		"args" : null
	}

Response

	True 
	
if user was logged in

	False

if user was not logged in

- **getPrivateKey**

Request

Get Private Key command will return private part of the key pair of the ticket found by its unique identifier (email, phone number, etc. )

	{
		"command" : "getPrivateKey",
		"args" : "someuser@mail.com"
	}

Response

if such ticket was found on local machine

	-----BEGIN EC PRIVATE KEY-----
	MIHbAgEBBEEAkOVHiQqRFMM8U8KyKEzHZTHLGDJJLWeYrNw+bYhn/9+BeYGGNHzW
	rVm4aucxr5zPINVrw8He/g7lbQ1p67eomqALBgkrJAMDAggBAQ2hgYUDgYIABEbe
	aI9NQ4omrbQCdp1P3VYXaG2aTNFUFUpN+TkNC6CH7LIXQjj8qhsaz9149qeDbJnn
	6vSN8zwlfl1LqtpODaYYLRjGXKoKK6T/6pZ8FEq27JxJ120Z9qma1Xo/0JQUNaLA
	cO9y7lhHlM+XH+27sk26WBARvZuZroEVoiWjL2rn
	-----END EC PRIVATE KEY-----

otherwise it will return nothing

- **getPublicKey**

Request

Get Public Key command will return public part of the key pair of the ticket found by its unique identifier (email, phone number, etc. )

	{
		"command" : "getPrivateKey",
		"args" : "someuser@mail.com"
	}

Response

if such ticket was found on local machine

	-----BEGIN EC PUBLIC KEY-----
	MIHbAgEBBEEAkOVHiQqRFMM8U8KyKEzHZTHLGDJJLWeYrNw+bYhn/9+BeYGGNHzW
	rVm4aucxr5zPINVrw8He/g7lbQ1p67eomqALBgkrJAMDAggBAQ2hgYUDgYIABEbe
	aI9NQ4omrbQCdp1P3VYXaG2aTNFUFUpN+TkNC6CH7LIXQjj8qhsaz9149qeDbJnn
	6vSN8zwlfl1LqtpODaYYLRjGXKoKK6T/6pZ8FEq27JxJ120Z9qma1Xo/0JQUNaLA
	cO9y7lhHlM+XH+27sk26WBARvZuZroEVoiWjL2rn
	-----END EC PUBLIC KEY-----

otherwise it will return nothing
