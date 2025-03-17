import Link from 'next/link';

export default function HomePage() {
    return (
        <div className='bg-blue-500'>
            <h1 className='text-3xl'>Bienvenido a DibujoFácil</h1>
            <p>La plataforma para aprender a dibujar paso a paso.</p>
            <div>
                <Link href="/register">
                    <button>Registrarse</button>
                </Link>
                <Link href="/login">
                    <button>Iniciar Sesión</button>
                </Link>
            </div>
        </div>
    );
}