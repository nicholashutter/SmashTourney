import { Character } from "./Character";

export interface Player
{
    Id: string;
    id?: string;
    displayName: string;
    currentScore: number;
    currentRound: number;
    currentCharacter: Character;
    currentGameId: string;
    currentGameID?: string;
}