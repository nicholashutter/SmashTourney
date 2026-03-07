
import { HubConnection, HubConnectionBuilder } from "@microsoft/signalr";
import { HUB_URL } from "@/ServerConstants";
import
{
    BracketSnapshotResponse,
    CreateGameWithModeRequest,
    CreateGameWithModeResponse,
    CurrentMatchResponse,
    GameStateResponse,
    SubmitMatchVoteRequest,
    SubmitMatchVoteResponse
} from "@/models/entities/Bracket";
import { Player } from "@/models/entities/Player";


// Manages the SignalR connection used for lobby and game realtime updates.
class PersistentConnection
{

    private connection: HubConnection | null = null;

    private onPlayersUpdated: ((players: Player[]) => void) | null = null;

    private onGameStarted: ((gameId: string) => void) | null = null;

    private onFlowStateUpdated: ((flowState: GameStateResponse | null) => void) | null = null;

    private onCurrentMatchUpdated: ((currentMatch: CurrentMatchResponse | null) => void) | null = null;

    private onBracketUpdated: ((snapshot: BracketSnapshotResponse | null) => void) | null = null;

    private onVoteSubmitted: ((voteResponse: SubmitMatchVoteResponse) => void) | null = null;

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

        this.connection.on("SuccessfullyJoined", () =>
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

        this.connection.on("FlowStateUpdated", (flowState: GameStateResponse | null) =>
        {
            if (this.onFlowStateUpdated)
            {
                this.onFlowStateUpdated(flowState);
            }
        });

        this.connection.on("CurrentMatchUpdated", (currentMatch: CurrentMatchResponse | null) =>
        {
            if (this.onCurrentMatchUpdated)
            {
                this.onCurrentMatchUpdated(currentMatch);
            }
        });

        this.connection.on("BracketUpdated", (snapshot: BracketSnapshotResponse | null) =>
        {
            if (this.onBracketUpdated)
            {
                this.onBracketUpdated(snapshot);
            }
        });

        this.connection.on("VoteSubmitted", (voteResponse: SubmitMatchVoteResponse) =>
        {
            if (this.onVoteSubmitted)
            {
                this.onVoteSubmitted(voteResponse);
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

    // Sets callback executed when flow-state updates are received.
    public setOnFlowStateUpdated(callback: (flowState: GameStateResponse | null) => void)
    {
        this.onFlowStateUpdated = callback;
    }

    // Sets callback executed when current-match updates are received.
    public setOnCurrentMatchUpdated(callback: (currentMatch: CurrentMatchResponse | null) => void)
    {
        this.onCurrentMatchUpdated = callback;
    }

    // Sets callback executed when bracket updates are received.
    public setOnBracketUpdated(callback: (snapshot: BracketSnapshotResponse | null) => void)
    {
        this.onBracketUpdated = callback;
    }

    // Sets callback executed when vote submission events are received.
    public setOnVoteSubmitted(callback: (voteResponse: SubmitMatchVoteResponse) => void)
    {
        this.onVoteSubmitted = callback;
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

    // Creates a game with explicit bracket mode and player count.
    public async createGameWithMode(request: CreateGameWithModeRequest): Promise<CreateGameWithModeResponse>
    {
        await this.ensureConnected();
        return await this.connection!.invoke("CreateGameWithMode", {
            bracketMode: request.bracketMode,
            totalPlayers: request.totalPlayers
        });
    }

    // Adds one player to a game and returns operation success.
    public async addPlayer(gameId: string, player: Player): Promise<boolean>
    {
        await this.ensureConnected(gameId);
        return await this.connection!.invoke("AddPlayer", gameId, player);
    }

    // Starts game progression and returns operation success.
    public async startGame(gameId: string): Promise<boolean>
    {
        await this.ensureConnected(gameId);
        return await this.connection!.invoke("StartGame", gameId);
    }

    // Requests players currently assigned to one game.
    public async getPlayersInGame(gameId: string): Promise<Player[]>
    {
        await this.ensureConnected(gameId);
        return await this.connection!.invoke("GetPlayersInGame", gameId);
    }

    // Requests current bracket snapshot for one game.
    public async getBracket(gameId: string): Promise<BracketSnapshotResponse | null>
    {
        await this.ensureConnected(gameId);
        return await this.connection!.invoke("GetBracket", gameId);
    }

    // Requests current active match for one game.
    public async getCurrentMatch(gameId: string): Promise<CurrentMatchResponse | null>
    {
        await this.ensureConnected(gameId);
        return await this.connection!.invoke("GetCurrentMatch", gameId);
    }

    // Requests high-level game flow state for one game.
    public async getFlowState(gameId: string): Promise<GameStateResponse | null>
    {
        await this.ensureConnected(gameId);
        return await this.connection!.invoke("GetFlowState", gameId);
    }

    // Submits one winner vote for the current active match.
    public async submitMatchVote(gameId: string, request: SubmitMatchVoteRequest): Promise<SubmitMatchVoteResponse>
    {
        await this.ensureConnected(gameId);
        return await this.connection!.invoke("SubmitMatchVote", gameId, request);
    }

    // Ensures connection is active and optionally joined to one game group.
    private async ensureConnected(gameId?: string)
    {
        if (gameId)
        {
            this.gameId = gameId;
        }

        if (!this.connection || this.connection.state === "Disconnected")
        {
            await this.createPlayerConnection(this.gameId);
            return;
        }

        if (this.gameId)
        {
            await this.connection.invoke("JoinGameGroup", this.gameId);
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