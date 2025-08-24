import { Character } from "./Character";

export interface Player
{
    id: string;
    userId: string;
    displayName: string;
    currentScore: number;
    currentRound: number;
    currentCharacter: Character;
    currentGameID: string;
}