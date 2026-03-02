
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { HUB_URL } from "@/ServerConstants";
import { Player } from "@/models/entities/Player";


class SignalRService
{

    private connection: HubConnection | null = null;

    private onPlayersUpdated: ((players: Player[]) => void) | null = null;

    private onGameStarted: ((gameId: string) => void) | null = null;

    private gameId: string = "";

    constructor()
    {

    }
    /**
   * Establishes a SignalR connection and sets up event listeners.
   * "Successfully Joined" triggers updateOthers(gameId)
   * "PlayersUpdated" invokes the registered callback with updated player data
   */
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
    /**
   * Registers a callback to handle incoming player updates from the hub.
   */
    public setOnPlayersUpdated(callback: (players: Player[]) => void)
    {
        this.onPlayersUpdated = callback;
    }

    public setOnGameStarted(callback: (gameId: string) => void)
    {
        this.onGameStarted = callback;
    }
    /**
   * Stores the current game session ID used for hub communication.
   */
    public setGameId(gameId: string)
    {
        this.gameId = gameId;
    }
    /**
   * Invokes the "UpdatePlayers" method on the hub to notify other clients.
   */
    public async updateOthers(gameId: string)
    {
        if (!gameId)
        {
            return;
        }

        try
        {
            await this.connection?.invoke("UpdatePlayers", gameId);
        }
        catch (err)
        {
            console.error(err);
        }
    }

    public async notifyGameStarted(gameId: string)
    {
        if (!gameId)
        {
            return;
        }

        try
        {
            await this.connection?.invoke("NotifyGameStarted", gameId);
        }
        catch (err)
        {
            console.error(err);
        }
    }

    public async disconnect()
    {
        try
        {
            await this.connection?.stop();
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
export default SignalRService;