import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';

export default function RegisterPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [name, setName] = useState('');
  const [error, setError] = useState('');
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();

    const response = await fetch('http://localhost:5054/api/Auth/register', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email, password, name }),
    });

    if (!response.ok) {
      const data = await response.json();
      setError(data.message || 'Registration failed');
    } else {
      const data = await response.json();
      localStorage.setItem('token', data.token); 
      navigate('/dashboard'); 
    }
  };

  return (
    <section className="min-h-screen flex items-center justify-center relative">
      {/* Fondo con imagen */}
      <div className="absolute inset-0 z-0">
        <img 
          src="Banner-hero-home.jpg" 
          alt="Background" 
          className="w-full h-full object-cover"
        />
        <div className="absolute inset-0 bg-black/40"></div>
      </div>
      
      {/* Formulario */}
      <div className="relative z-10 bg-white/90 p-8 rounded-lg shadow-xl max-w-md w-full mx-4">
        <div className='flex justify-center mb-2'>
          <img src="Dibujo-Facil-logo.png" width={120} alt="" />
        </div>
        
        <h2 className="text-xl font-bold text-blue-900 mb-6 text-center">
          Crear Cuenta
          <p className='text-gray-400 text-sm font-medium mt-1'>únete a nuestra comunidad</p>
        </h2>
  
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-gray-700 text-sm font-semibold mb-2">
              Nombre
            </label>
            <input
              type="text"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:border-blue-500"
              required
            />
          </div>
  
          <div>
            <label className="block text-gray-700 text-sm font-semibold mb-2">
              Email
            </label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:border-blue-500"
              required
            />
          </div>
  
          <div>
            <label className="block text-gray-700 text-sm font-semibold mb-2">
              Contraseña
            </label>
            <input
              type="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-3 py-2 border rounded-lg focus:outline-none focus:border-blue-500"
              required
            />
          </div>
  
          {error && <div className="text-red-500 text-sm text-center">{error}</div>}
  
          <button 
            type="submit"
            className="w-full bg-blue-600 text-white py-2 rounded-lg hover:bg-blue-700 transition-colors"
          >
            Registrarse
          </button>
  
          <p className="text-center text-sm text-gray-600 mt-4">
            ¿Ya tienes cuenta?{' '}
            <Link 
              to="/login" 
              className="text-blue-600 hover:underline"
            >
              Inicia sesión aquí
            </Link>
          </p>
        </form>
      </div>
    </section>
  );
  
}
