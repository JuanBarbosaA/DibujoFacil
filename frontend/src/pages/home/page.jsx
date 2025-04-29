import Navbar from "../../Components/Navbar";
export default function HomePage() {
    return(
        <div>
            <img src="menu.png" width="100%" margin="0" padding="0"/>
            <section className="bg-red-600 flex justify-between">
                <div className="">
                    Logo
                </div>
                <Navbar/>
                <button className="bg-blue-400 px-4 py-2 text-white">
                    iniciar sesión
                </button>
            </section>
        </div>
    )
};
