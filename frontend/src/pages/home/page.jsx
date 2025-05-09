import { Link } from "react-router-dom";
import Navbar from "../../Components/Navbar";

export default function HomePage() {
    return(
        <div>
            <section className=" flex justify-between items-center py-7 px-20">
                <div>
                    <img src="Dibujo-Facil-logo.png" className="w-36"/>
                </div>
                <Navbar/>
               
                <Link to={"/login"} className="bg-blue-400 px-7 py-3 rounded-full text-white font-bold hover:bg-blue-500 transition-colors duration-300 self-center">iniciar sesión</Link>
            </section>

            <section>
                <img src="Website-home-1.png" alt="" />
            </section>
            
            <section className="flex justify-center">
                <img src="WhatsApp Image 2025-04-27 at 7.42.01 PM.jpeg" width={1100} alt="" />
            </section>

            <section>
                <img src="Website-home-3.jpg" alt="" />
            </section>

            <section>
                <img src="Website-home-4.jpg" alt="" />
            </section>

            <section>
                <img src="Website-home-5.jpg" alt="" />
            </section>
            
            <section>
                <img src="Website-home-6.jpg" alt="" />
            </section>
            
            <section>
                <img src="Website-home-7.jpg" alt="" />
            </section>
            
            <section>
                <img src="Website-home-footer.jpg" alt="" />
            </section>
        </div>
    )
};