import { test, expect, vi, beforeEach } from 'vitest';
import { Player } from '../src/models/entities/Player';
import { HubConnectionBuilder } from '@microsoft/signalr';
import SignalRService from '../src/services/PersistentConnection';
import type { HubConnectionBuilder as RealBuilderType, HubConnection } from '@microsoft/signalr';
import Marth from '../src/models/entities/Characters/Marth';


// Mock HubConnection and its methods
// Create a reusable mock connection object
const mockOn = vi.fn();
const mockStart = vi.fn().mockResolvedValue(undefined);
const mockInvoke = vi.fn().mockResolvedValue(undefined);

const mockConnection = {
  on: mockOn,
  start: mockStart,
  invoke: mockInvoke
};

// Mock HubConnectionBuilder chain
const mockBuild = vi.fn(() => mockConnection);
const mockWithAutomaticReconnect = vi.fn(() => ({ build: mockBuild }));
const mockWithUrl = vi.fn(() => ({ withAutomaticReconnect: mockWithAutomaticReconnect }));

vi.mock('@microsoft/signalr', async () => {
  const actual = await vi.importActual<typeof import('@microsoft/signalr')>('@microsoft/signalr');
  return {
    ...actual,
    HubConnectionBuilder: vi.fn(() => ({
      withUrl: mockWithUrl
    }))
  };
});

beforeEach(() => {
  vi.clearAllMocks();
});


/**
 *   Verifies that createPlayerConnection initializes a HubConnection with correct configuration.
 * - Mocks HubConnectionBuilder and ensures connection is built and started.
 * - Confirms event handlers for "Successfully Joined" and "PlayersUpdated" are registered.
 */
test("SignalRService creates and starts connection with correct handlers", async () => {
  const service = new SignalRService();
  service.createPlayerConnection();

  expect(mockWithUrl).toHaveBeenCalledWith(expect.any(String), { withCredentials: true });
  expect(mockWithAutomaticReconnect).toHaveBeenCalled();
  expect(mockBuild).toHaveBeenCalled();
  expect(mockStart).toHaveBeenCalled();

  expect(mockOn).toHaveBeenCalledWith("Successfully Joined", expect.any(Function));
  expect(mockOn).toHaveBeenCalledWith("PlayersUpdated", expect.any(Function));
});

/**
 *   Verifies that setGameId correctly stores the gameId internally.
 *   Ensures updateOthers uses the correct gameId when invoked after connection.
 */
test("SignalRService stores gameId and uses it in updateOthers", async () => {
  const service = new SignalRService();
  const gameId = "game-xyz123";

  service.setGameId(gameId);
  service.createPlayerConnection();

  await service.updateOthers(gameId);

  expect(mockInvoke).toHaveBeenCalledWith("UpdatePlayers", gameId);
});


/**
 *   Verifies that setOnPlayersUpdated registers a callback and invokes it when PlayersUpdated is received.
 *   Simulates receiving a PlayersUpdated event and checks callback execution.
 */
test("SignalRService invokes onPlayersUpdated callback when PlayersUpdated is received", async () => {
  const service = new SignalRService();
  const mockPlayers: Player[] = [
    {
      displayName: "Nick",
      currentScore: 10,
      currentRound: 2,
      currentGameId: "game-abc",
      currentCharacter: Marth
    }
  ];

  const callback = vi.fn();
  service.setOnPlayersUpdated(callback);
  service.createPlayerConnection();

  // Simulate the "PlayersUpdated" event manually
  const playersUpdatedHandler = mockOn.mock.calls.find(
    ([eventName]) => eventName === "PlayersUpdated"
  )?.[1];

  expect(playersUpdatedHandler).toBeDefined();
  playersUpdatedHandler?.(mockPlayers);

  expect(callback).toHaveBeenCalledWith(mockPlayers);
  });


/**
 *   Verifies that updateOthers invokes the UpdatePlayers method on the hub.
 *   Mocks connection.invoke and checks it is called with correct arguments.
 */
test("SignalRService calls UpdatePlayers with correct gameId", async () => {
  const service = new SignalRService();
  const gameId = "game-456";

  service.createPlayerConnection();
  await service.updateOthers(gameId);

  expect(mockInvoke).toHaveBeenCalledWith("UpdatePlayers", gameId);
});