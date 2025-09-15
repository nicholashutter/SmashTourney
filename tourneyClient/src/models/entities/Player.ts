import { Character } from "./Character";

export interface Player
{
    displayName: string;
    currentScore: number;
    currentRound: number;
    currentCharacter: Character;
    currentGameId: string;
}