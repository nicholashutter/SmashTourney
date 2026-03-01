import { Card, CardContent } from "@/components/ui/card";
import { Player } from "@/models/entities/Player";

type AppProps =
  {
    players: Player[];
  }

const PlayerList = ({ players }: AppProps) =>
{
  if (players.length === 0)
  {
    return (
      <Card className="max-w-sm overflow-hidden p-2 m-1">
        <CardContent>
          <p className="text-sm">No players yet. Share the session code and wait for joins.</p>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="flex flex-col gap-1">
      {players.map((player, index) => (
        <Card className="max-w-sm overflow-hidden p-1 m-1" key={player.Id ?? `${player.displayName}-${index}`}>
          <CardContent>
            <p className="text-sm truncate font-bold ">{player.displayName}</p>
            <p className="text-sm truncate">{player.currentCharacter.characterName}</p>
          </CardContent>
        </Card>
      ))}
    </div>
  );
};

export default PlayerList