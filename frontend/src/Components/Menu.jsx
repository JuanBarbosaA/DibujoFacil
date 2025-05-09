import { Link } from "react-router-dom";
import '../index.css';  

export default function Menu(){
    return(
        <nav className="flex text-gray-500 font-semibold gap-4">
            <Link to={"/"} className="flex items-end p-4 hover:text-green-300">Home</Link>
            <Link to={"/tutorials"} className="flex items-end p-4 hover:text-green-300">Tutoriales</Link>
            <Link to={"/tutorials"} className="flex items-end p-4 hover:text-green-300">Tips</Link>
            <Link to={"/tutorials"} className="flex items-end p-4 hover:text-green-300">Actividades</Link>
            <Link to={"/admin"} className="flex items-end p-4 hover:text-green-300">Administrador</Link>
        </nav>
    )
}