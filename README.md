# CloudCopy

Self Hosted Copy and Paste ClipBoard

# Install

After running the server, then goto the hosted address in a browser so that the app pin code can be set.

## Login

```bash
CLOUDCOPY_JWT=$(curl --silent http://cloudcopy.host.org/v1/Account/Login/MyPassword | jq -r .Jwt -)
```

## Copy

```bash
curl -H "Authorization: Bearer $CLOUDCOPY_JWT" 'http://cloudcopy.host.org/v1/Copied/Create/http://news.ycombinator.com'
```

## Paste

```bash
curl -H "Authorization: Bearer $CLOUDCOPY_JWT" 'http://cloudcopy.host.org/v1/Copied/Latest' | jq -r .Copy.Body -
```
