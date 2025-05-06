import { Link } from "react-router-dom";
import '../index.css';  

export default function Navbar(){
    return(
        <nav className="flex text-gray-400">
            <Link to={"/"}>Home</Link>
            <Link to={"/dashboard"}>Dashboard</Link>
            <Link to={"/login"}>Login</Link>
            <Link to={"/register"}>Register</Link>
            <Link to={"/profile"}>Profile</Link>
            <Link to={"/createTutorial"}>Create tutorial</Link>
            <Link to={"/admin"}>Admin</Link>
        </nav>
    )
}