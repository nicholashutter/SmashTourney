import { Player } from "@/models/entities/Player";

export const resolvePlayerId = (player: Player): string =>
{
    return player.Id ?? player.id ?? "";
};

export const normalizePlayer = (player: Player): Player =>
{
    return {
        ...player,
        Id: resolvePlayerId(player),
        currentGameId: player.currentGameId ?? player.currentGameID ?? ""
    };
};

export const normalizePlayers = (players: Player[]): Player[] =>
{
    return players.map(normalizePlayer);
};
