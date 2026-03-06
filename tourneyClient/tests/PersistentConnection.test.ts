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
        currentScore: 10,
        currentRound: 2,
        currentGameId: "game-abc",
        currentCharacter: Marth
    }
];

beforeEach(() =>
{
    vi.clearAllMocks();
});

// Verifies SignalR connection uses credentials when building the Hub URL.
test("createPlayerConnection configures Hub URL with credentials", async () =>
{
    const connectionService = createPersistentConnection();

    await connectionService.createPlayerConnection();

    expect(signalrWithUrlSpy).toHaveBeenCalledWith(expect.any(String), { withCredentials: true });
});

// Verifies SignalR connection enables automatic reconnect strategy.
test("createPlayerConnection enables automatic reconnect", async () =>
{
    const connectionService = createPersistentConnection();

    await connectionService.createPlayerConnection();

    expect(signalrEnableReconnectSpy).toHaveBeenCalled();
});

// Verifies SignalR connection builder creates a concrete connection instance.
test("createPlayerConnection builds a connection instance", async () =>
{
    const connectionService = createPersistentConnection();

    await connectionService.createPlayerConnection();

    expect(signalrBuildConnectionSpy).toHaveBeenCalled();
});

// Verifies SignalR connection starts after creation.
test("createPlayerConnection starts the connection", async () =>
{
    const connectionService = createPersistentConnection();

    await connectionService.createPlayerConnection();

    expect(signalrConnectionStartSpy).toHaveBeenCalled();
});

// Verifies PlayersUpdated handler is registered.
test("createPlayerConnection registers PlayersUpdated event", async () =>
{
    const connectionService = createPersistentConnection();

    await connectionService.createPlayerConnection();

    expect(signalrOnEventSpy).toHaveBeenCalledWith("PlayersUpdated", expect.any(Function));
});

// Verifies Successfully Joined handler is registered.
test("createPlayerConnection registers Successfully Joined event", async () =>
{
    const connectionService = createPersistentConnection();

    await connectionService.createPlayerConnection();

    expect(signalrOnEventSpy).toHaveBeenCalledWith("Successfully Joined", expect.any(Function));
});

// Verifies GameStarted handler is registered.
test("createPlayerConnection registers GameStarted event", async () =>
{
    const connectionService = createPersistentConnection();

    await connectionService.createPlayerConnection();

    expect(signalrOnEventSpy).toHaveBeenCalledWith("GameStarted", expect.any(Function));
});

// Verifies updateOthers sends UpdatePlayers with provided game identifier.
test("updateOthers invokes UpdatePlayers with gameId", async () =>
{
    const connectionService = createPersistentConnection();
    const gameId = "game-xyz123";

    await connectionService.createPlayerConnection();
    await connectionService.updateOthers(gameId);

    expect(signalrConnectionInvokeSpy).toHaveBeenCalledWith("UpdatePlayers", gameId);
});

// Verifies createPlayerConnection joins SignalR game group when gameId is passed.
test("createPlayerConnection joins game group", async () =>
{
    const connectionService = createPersistentConnection();
    const gameId = "group-123";

    await connectionService.createPlayerConnection(gameId);

    expect(signalrConnectionInvokeSpy).toHaveBeenCalledWith("JoinGameGroup", gameId);
});

// Verifies setOnPlayersUpdated callback runs when PlayersUpdated event is raised.
test("PlayersUpdated event triggers callback", async () =>
{
    const connectionService = createPersistentConnection();
    const onPlayersUpdatedSpy = vi.fn();

    connectionService.setOnPlayersUpdated(onPlayersUpdatedSpy);
    await connectionService.createPlayerConnection();

    const playersUpdatedHandler = findRegisteredEventHandler("PlayersUpdated");
    playersUpdatedHandler?.(samplePlayers);

    expect(onPlayersUpdatedSpy).toHaveBeenCalledWith(samplePlayers);
});

// Verifies PlayersUpdated handler exists after connection initialization.
test("PlayersUpdated event handler is defined", async () =>
{
    const connectionService = createPersistentConnection();

    await connectionService.createPlayerConnection();

    expect(findRegisteredEventHandler("PlayersUpdated")).toBeDefined();
});

// Verifies setOnGameStarted callback runs when GameStarted event is raised.
test("GameStarted event triggers callback", async () =>
{
    const connectionService = createPersistentConnection();
    const gameId = "game-start-123";
    const onGameStartedSpy = vi.fn();

    connectionService.setOnGameStarted(onGameStartedSpy);
    await connectionService.createPlayerConnection(gameId);

    const gameStartedHandler = findRegisteredEventHandler("GameStarted");
    gameStartedHandler?.(gameId);

    expect(onGameStartedSpy).toHaveBeenCalledWith(gameId);
});

// Verifies GameStarted handler exists after connection initialization.
test("GameStarted event handler is defined", async () =>
{
    const connectionService = createPersistentConnection();

    await connectionService.createPlayerConnection("game-start-123");

    expect(findRegisteredEventHandler("GameStarted")).toBeDefined();
});

// Verifies notifyGameStarted invokes NotifyGameStarted with provided gameId.
test("notifyGameStarted invokes hub method", async () =>
{
    const connectionService = createPersistentConnection();
    const gameId = "notify-123";

    await connectionService.createPlayerConnection(gameId);
    await connectionService.notifyGameStarted(gameId);

    expect(signalrConnectionInvokeSpy).toHaveBeenCalledWith("NotifyGameStarted", gameId);
});

// Verifies disconnect stops SignalR connection.
test("disconnect stops active connection", async () =>
{
    const connectionService = createPersistentConnection();

    await connectionService.createPlayerConnection();
    await connectionService.disconnect();

    expect(signalrConnectionStopSpy).toHaveBeenCalled();
});
