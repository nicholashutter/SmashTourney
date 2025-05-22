'use client'
import User from "@/app/resources/user";
import Link from "next/link";
import "./page.css";
import { useState, ReactNode } from "react";
import Button from "@mui/material/Button"; 



const SignUp = (props: { children?: ReactNode }) => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [playerName, setPlayerName] = useState("");
  const [register, setRegister] = useState(false);
  
  const clientUser:User = 
  {
    playerName: playerName,
    email: email,
    passwordHash: password
  }

  function handleSubmit (e:React.FormEvent)
  {
   window.alert("Submission Success"); 
  }

  return (
    <>
      <meta charSet="UTF-8" />
      <meta name="viewport" content="width=device-width, initial-scale=1.0" />
      <title>User Registration</title>
      <div className="container">
        <h2 className="title">Sign Up</h2>
        <form action="#" method="post" onSubmit={(e)=> handleSubmit(e)}>
          <div className="form-group">
            <label htmlFor="username">Username:</label>
            <input type="text" id="username" name="username" value={playerName} onChange={(e)=> setPlayerName(e.target.value)} /> 
          </div>
          <div className="form-group">
            <label htmlFor="email">Email:</label>
            <input type="email" id="email" name="email" value={email} onChange={(e) => setEmail(e.target.value)}/>
          </div>
          <div className="form-group">
            <label htmlFor="password">Password:</label>
            <input type="password" id="password" name="password" value={password} onChange={(e) => setPassword(e.target.value)}/>
          </div>
          <div className="form-group">
            <input type="button" onClick={handleSubmit} id ="submitButton" defaultValue="Sign Up" />
          </div>
          <div className="form-group"> 
            <span className="switch-text">
              Already have an account? <Link  href="/">Sign In Here</Link>
            </span>
          </div>
        </form>
      </div>
    </>
  );
};

export default SignUp;
