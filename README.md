NOTE: Bot contains hardcoded values like discord channel ids, and user ids, beware that you will need to change these or remove them entierly from the code.

This is a simple discord music bot written with the DSharpPlus library, the bot plays music from any source supported by lavalink (youtube, soundcloud, vimeo, twitch). 

Simply place a config.json file in the same directory as the executable and populate it with these required data structures:

```JSON
{
  "token": "yourtokenhere",
  "prefix": "!",
  "Hostname": "127.0.0.1",
  "Port": "2333",
  "Password": "yourpasswordhere"
}
```
#### Commands
                
- Join 
- Leave
- Play (Also joins the voice channel of the user requesting the command if not already in one)
- Pause
- Resume
- Stop
- Jump (Skips ahead, formatted in hours:minutes:seconds, eg: !jump 00:45:00.
- Volume (Sets the volume globally, a value between 0 - 100 required)
- Help (shows all useable bot commands)
     
