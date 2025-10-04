import { Character } from "./Character";

export interface Player
{
    Id: string;
    displayName: string;
    currentScore: number;
    currentRound: number;
    currentCharacter: Character;
    currentGameId: string;
}