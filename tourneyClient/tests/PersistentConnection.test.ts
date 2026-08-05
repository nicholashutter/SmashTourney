import { beforeEach, expect, test, vi } from "vitest";
import { Player } from "../src/models/entities/Player";
import { Marth } from "../src/models/entities/Characters/Marth";
import { PersistentConnection } from "../src/services/PersistentConnection";

const signalrOnEventSpy = vi.fn();
const signalrReconnectedHandlerSpy = vi.fn();
const signalrConnectionStartSpy = vi.fn().mockResolvedValue(undefined);
const signalrConnectionInvokeSpy = vi.fn().mockResolvedValue(undefined);
const signalrConnectionStopSpy = vi.fn().mockResolvedValue(undefined);

const simulatedSignalrConnection = {
    on: signalrOnEventSpy,
    onreconnected: signalrReconnectedHandlerSpy,
    start: signalrConnectionStartSpy,
    invoke: signalrConnectionInvokeSpy,
    stop: signalrConnectionStopSpy,
    state: "Disconnected"
};

const signalrBuildConnectionSpy = vi.fn(() => simulatedSignalrConnection);
const signalrEnableReconnectSpy = vi.fn(() => ({ build: signalrBuildConnectionSpy }));
const signalrWithUrlSpy = vi.fn(() => ({ withAutomaticReconnect: signalrEnableReconnectSpy }));

vi.mock("@microsoft/signalr", async () =>
{
    const actualSignalrModule = await vi.importActual<typeof import("@microsoft/signalr")>("@microsoft/signalr");

    return {
        ...actualSignalrModule,
        HubConnectionBuilder: vi.fn(() => ({
            withUrl: signalrWithUrlSpy
        }))
    };
});

const createPersistentConnection = () =>
{
    return new PersistentConnection();
};

const findRegisteredEventHandler = (eventName: string) =>
{
    return signalrOnEventSpy.mock.calls.find(([registeredEventName]) => registeredEventName === eventName)?.[1];
};

const samplePlayers: Player[] = [
    {
        Id: "player-1",
        displayName: "Nick",
        currentGameId: "game-abc",
        currentCharacter: Marth
    }
];

beforeEach(() =>
{
    vi.clearAllMocks();
});

// Verifies createPlayerConnection wires SignalR options (credentials, reconnect, build, start).
test("createPlayerConnection configures credentials, reconnect, build, and start", async () =>
{
    const connectionService = createPersistentConnection();

    await connectionService.createPlayerConnection();

    expect(signalrWithUrlSpy).toHaveBeenCalledWith(expect.any(String), { withCredentials: true });
    expect(signalrEnableReconnectSpy).toHaveBeenCalled();
    expect(signalrBuildConnectionSpy).toHaveBeenCalled();
    expect(signalrConnectionStartSpy).toHaveBeenCalled();
});

// Verifies createPlayerConnection registers all three event handlers and joins the game group when a gameId is provided.
test("createPlayerConnection registers handlers and joins the game group when a gameId is provided", async () =>
{
    const connectionService = createPersistentConnection();
    const gameId = "group-123";

    await connectionService.createPlayerConnection(gameId);

    expect(signalrOnEventSpy).toHaveBeenCalledWith("PlayersUpdated", expect.any(Function));
    expect(signalrOnEventSpy).toHaveBeenCalledWith("Successfully Joined", expect.any(Function));
    expect(signalrOnEventSpy).toHaveBeenCalledWith("GameStarted", expect.any(Function));
    expect(signalrConnectionInvokeSpy).toHaveBeenCalledWith("JoinGameGroup", gameId);
});

// Verifies that PlayersUpdated and GameStarted events raise the registered callbacks with the payload.
test("PlayersUpdated and GameStarted events raise registered callbacks", async () =>
{
    const connectionService = createPersistentConnection();
    const gameId = "game-start-123";
    const onPlayersUpdatedSpy = vi.fn();
    const onGameStartedSpy = vi.fn();

    connectionService.setOnPlayersUpdated(onPlayersUpdatedSpy);
    connectionService.setOnGameStarted(onGameStartedSpy);
    await connectionService.createPlayerConnection(gameId);

    findRegisteredEventHandler("PlayersUpdated")?.(samplePlayers);
    findRegisteredEventHandler("GameStarted")?.(gameId);

    expect(onPlayersUpdatedSpy).toHaveBeenCalledWith(samplePlayers);
    expect(onGameStartedSpy).toHaveBeenCalledWith(gameId);
});

// Verifies that updateOthers and notifyGameStarted invoke the correct hub methods.
test("updateOthers and notifyGameStarted invoke the correct hub methods", async () =>
{
    const connectionService = createPersistentConnection();
    const gameId = "game-xyz123";

    await connectionService.createPlayerConnection(gameId);
    await connectionService.updateOthers(gameId);
    await connectionService.notifyGameStarted(gameId);

    expect(signalrConnectionInvokeSpy).toHaveBeenCalledWith("UpdatePlayers", gameId);
    expect(signalrConnectionInvokeSpy).toHaveBeenCalledWith("NotifyGameStarted", gameId);
});

// Verifies disconnect stops the active SignalR connection.
test("disconnect stops active connection", async () =>
{
    const connectionService = createPersistentConnection();

    await connectionService.createPlayerConnection();
    await connectionService.disconnect();

    expect(signalrConnectionStopSpy).toHaveBeenCalled();
});
