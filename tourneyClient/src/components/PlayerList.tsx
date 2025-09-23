import { Card,CardContent } from "@/components/ui/card";
import { Player } from "@/models/entities/Player";

type AppProps =
  {
    players: Player[];
  }

const PlayerList = ({ players }: AppProps) =>
{
  return (
    <div className="flex flex-col gap-1">
      {players.map((player) => (
        <Card className="max-w-sm overflow-hidden p-1 m-1" key={player.displayName}>
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