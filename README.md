# GameDB
GameDB is a tool to store Steam detailed application list to a local database.

## How it works?

1. Browse /Game/GetList to store the entire list of application ids into the local text file for later use.
2. Browse /Game/StoreList to store application ids and names into the local database that are extracted from the local text file.
3. Browse /Game/MineRange/0 to start mining game details 10 by 10.

### Note
MineRange will refresh the page every 15 seconds by increasing the id by 10 each time. This was done due to Steam API limited usage policy of 200 queries per 5 minutes.
