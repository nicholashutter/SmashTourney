
import {ReactNode} from "react";

type ParentProps = {
  children: ReactNode;
}

export const GenericBracket = ({children}:ParentProps) => 
{
  return (
    <div className="flex flex-col items-center justify-center h-dvh w-dvw"> {/* center all content and take up entire viewport */}
    <div className="flex flex-col content-center text-center bg-black/25 rounded shadow-md text-white m-2 text-4xl max-w-9/10 ">\
    <div className='shrink flex flex-col text-2xl p-4 m-4 '>
      {children}
    </div>
    </div>
    </div>

  ); 
}

export default GenericBracket;