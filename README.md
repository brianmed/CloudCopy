# CloudCopy

Self Hosted Copy and Paste ClipBoard

# Install

After running the server, then goto the hosted address in a browser so that the app pin code can be set.

After setting, then login from the command-line like so:

```bash
CLOUDCOPY_JWT=$(curl --silent http://cloudcopy.bmedley.org/v1/Account/Login/MyPassword | jq -r .Jwt -)
```

Once logged in, next copy something:

```bash
curl -H "Authorization: Bearer $CLOUDCOPY_JWT" 'http://cloudcopy.bmedley.org/v1/Copied/Create/http://news.ycombinator.com'
```

Finally, paste:

```bash
curl -H "Authorization: Bearer $CLOUDCOPY_JWT" 'http://cloudcopy.bmedley.org/v1/Copied/Latest' | jq -r .Copy.Body -
```
