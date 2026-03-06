
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { HUB_URL } from "@/ServerConstants";
import { Player } from "@/models/entities/Player";


// Manages the SignalR connection used for lobby and game realtime updates.
class PersistentConnection
{

    private connection: HubConnection | null = null;

    private onPlayersUpdated: ((players: Player[]) => void) | null = null;

    private onGameStarted: ((gameId: string) => void) | null = null;

    private gameId: string = "";

    // Creates a player connection and subscribes to hub events.
    public async createPlayerConnection(gameId?: string)
    {
        if (gameId)
        {
            this.gameId = gameId;
        }

        if (this.connection && this.connection.state !== "Disconnected")
        {
            return;
        }

        this.connection = new HubConnectionBuilder()
            .withUrl(HUB_URL,
                {
                    withCredentials: true,
                }
            )
            .withAutomaticReconnect()
            .build();

        this.registerConnectionHandlers();

        this.connection.onreconnected(async () =>
        {
            if (!this.gameId)
            {
                return;
            }

            try
            {
                if (this.connection)
                {
                    await this.connection.invoke("JoinGameGroup", this.gameId);
                }
                await this.updateOthers(this.gameId);
            }
            catch (error)
            {
                console.error(error);
            }
        });
        try
        {
            await this.connection.start();
            if (this.gameId)
            {
                await this.connection.invoke("JoinGameGroup", this.gameId);
            }
        }
        catch (error)
        {
            console.log(error);
        }
    }

    // Registers handlers for incoming hub events.
    private registerConnectionHandlers()
    {
        if (!this.connection)
        {
            return;
        }

        this.connection.on("Successfully Joined", () =>
        {
            if (this.gameId)
            {
                this.updateOthers(this.gameId);
            }
        });

        this.connection.on("PlayersUpdated", (players: Player[]) =>
        {
            if (this.onPlayersUpdated)
            {
                this.onPlayersUpdated(players);
            }
        });

        this.connection.on("GameStarted", (startedGameId: string) =>
        {
            if (this.onGameStarted)
            {
                this.onGameStarted(startedGameId);
            }
        });
    }

    // Sets callback executed when player updates are received.
    public setOnPlayersUpdated(callback: (players: Player[]) => void)
    {
        this.onPlayersUpdated = callback;
    }

    // Sets callback executed when a game-started event is received.
    public setOnGameStarted(callback: (gameId: string) => void)
    {
        this.onGameStarted = callback;
    }

    // Stores the game identifier used for hub operations.
    public setGameId(gameId: string)
    {
        this.gameId = gameId;
    }

    // Notifies other clients that player data should refresh.
    public async updateOthers(gameId: string)
    {
        if (!gameId)
        {
            return;
        }

        try
        {
            if (!this.connection)
            {
                return;
            }

            await this.connection.invoke("UpdatePlayers", gameId);
        }
        catch (err)
        {
            console.error(err);
        }
    }

    // Notifies other clients that the game has started.
    public async notifyGameStarted(gameId: string)
    {
        if (!gameId)
        {
            return;
        }

        try
        {
            if (!this.connection)
            {
                return;
            }

            await this.connection.invoke("NotifyGameStarted", gameId);
        }
        catch (err)
        {
            console.error(err);
        }
    }

    // Closes the SignalR connection and clears local connection state.
    public async disconnect()
    {
        try
        {
            if (this.connection)
            {
                await this.connection.stop();
            }
        }
        catch (error)
        {
            console.error(error);
        }
        finally
        {
            this.connection = null;
        }
    }

}
export { PersistentConnection };