"use client"
import Link from "next/link";
import "./page.css";
import {ReactNode} from "react";

interface GenericBracketProps {
  children: ReactNode;
}

export const GenericBracket = ({children}:GenericBracketProps) => 
{
  return (
    <>
    <div className="container">
        {children}
      <div className="title"></div>
    </div>
    </>
  ); 
}

export default GenericBracket;