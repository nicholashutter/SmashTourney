import { Character } from "./Character";

export interface Player
{
    Id: string;
    id?: string;
    displayName: string;
    currentCharacter: Character;
    currentGameId: string;
    currentGameID?: string;
}